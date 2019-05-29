
using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using log4net;
using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.MarketData.Clients;
using ServiceModelEx;
using OF = Odey.Framework.Keeley.Entities;


namespace Odey.FocusList.FocusListService
{
    public class FocusListService : OdeyServiceBase, IFocusList
    {
        private const int KeeleyServiceUserId = 54;

        private static readonly new ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Save(OF.FocusList focusList)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                OF.FocusList existingFocusList = context.FocusLists.FirstOrDefault(a => a.InstrumentMarketId == focusList.InstrumentMarketId);
                ApplyFocusList(focusList, context, existingFocusList);
                context.SaveChanges();
            }
        }
      
        private void ApplyFocusList(OF.FocusList newFocusList, OF.KeeleyModel context, OF.FocusList existingFocusList)
        {
            if (existingFocusList == null)
            {
                context.FocusLists.Add(newFocusList);
            }
            else
            {
                existingFocusList.AnalystId = newFocusList.AnalystId;
                existingFocusList.InDate = newFocusList.InDate;
                existingFocusList.InPrice = newFocusList.InPrice;
                existingFocusList.IsLong = newFocusList.IsLong;
                existingFocusList.CurrentPrice = newFocusList.CurrentPrice;
                existingFocusList.CurrentPriceId = newFocusList.CurrentPriceId;                
                existingFocusList.OutDate = newFocusList.OutDate;
                existingFocusList.OutPrice = newFocusList.OutPrice;
            }
        }

        public void SaveList(List<OF.FocusList> focusLists)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                Dictionary<Tuple<int,DateTime?>, OF.FocusList> existingFocusList = context.FocusLists.ToDictionary(a => new Tuple<int,DateTime?>( a.InstrumentMarketId,a.OutDate), a => a);
                foreach (OF.FocusList focusListEntry in focusLists)
                {
                    OF.FocusList existingFocusListEntry = null;
                    Tuple<int, DateTime?> key = new Tuple<int, DateTime?>(focusListEntry.InstrumentMarketId, focusListEntry.OutDate);
                    existingFocusList.TryGetValue(key, out existingFocusListEntry);
                    ApplyFocusList(focusListEntry,context,existingFocusListEntry);
                }                
                context.SaveChanges();
            }
        }

        public List<OF.FocusList> GetAll()
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                return context.FocusLists.ToList();
            }
        }

        public void UpdatePrice(OF.Price price)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                OF.FocusList focusList = context.FocusLists.Where(a => (a.CurrentPriceId == price.PriceId || a.RelativeCurrentPriceId == price.PriceId) && !a.OutDate.HasValue).FirstOrDefault();
                if (focusList != null)
                {
                    if (price.PriceId == focusList.CurrentPriceId)
                    {
                        focusList.CurrentPrice = price.Value;
                    }
                    else if (price.PriceId == focusList.RelativeCurrentPriceId)
                    {
                        focusList.RelativeCurrentPrice = price.Value;
                    }
                    context.SaveChanges();
                }
            }
        }

        public void Reprice(DateTime repriceDate)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                List<OF.FocusList> focusLists = context.FocusLists
                    .Where(a=>!a.OutDate.HasValue)
                    .ToList();
                PriceClient client = new PriceClient();
                foreach(OF.FocusList focusList in focusLists)
                {
                    PriceFocusList(client, focusList, repriceDate);                        
                }
                context.SaveChanges();
            }
        }

        private int GetRelativeIndexId(OF.KeeleyModel context,int issuerId)
        {
            OF.Industry subIndustry = context.IssuerIndustries.Include(a=>a.Industry).Where(a=>a.IssuerID == issuerId && a.IndustryClassificationID == (int)IndustryClassificationIds.GICS).Select(a=>a.Industry).First();
            if (subIndustry.IndustryID == (int)IndustryIds.GICSUnclassifiedSubIndustry)
            {
                throw new ApplicationException(String.Format("Issuer {0} does not have GICS industry so cannot establish relative index", issuerId));
            }
            OF.Industry industry = context.Industries.Where(a => a.IndustryID == subIndustry.ParentIndustryID).First();
            OF.Industry industryGroup = context.Industries.Where(a => a.IndustryID == industry.ParentIndustryID).First();
            if (!industryGroup.RelativeIndexInstrumentMarketId.HasValue)
            {
                throw new ApplicationException(String.Format("Industry Group {0} does not have index",industryGroup.Name));
            }
            return industryGroup.RelativeIndexInstrumentMarketId.Value;
        }


        public void Add(int instrumentMarketId, DateTime inDate, decimal inPrice, int analystId, bool isLong)
        {
            Logger.Info($"Adding to Focus List: instrumentMarketId {instrumentMarketId}, inDate {inDate}, inPrice {inPrice}, analystId {analystId}, isLong {isLong}");
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                context._applicationUserIdOverride = analystId;

                OF.FocusList existing = context.FocusLists.Where(a => a.InstrumentMarketId == instrumentMarketId && !a.OutDate.HasValue).FirstOrDefault();
                OF.InstrumentMarket instrumentMarket = context.InstrumentMarkets.Include(a=>a.Instrument).Where(a => a.InstrumentMarketID == instrumentMarketId).FirstOrDefault();
                if (existing != null)
                {
                    if (existing.IsLong == isLong && existing.AnalystId == analystId)
                    {
                        var emailClient = new EmailClient();
                        emailClient.Send("focuslistservice@odey.com", "Focus List Service", "programmers@odey.com", null, null, "Tried to add focus list entry but was already open. ", $"Tried to add to Focus List but was already openb. instrumentMarketId {instrumentMarketId}, inDate {inDate}, inPrice {inPrice}, analystId {analystId}, isLong {isLong}", null);
                        return;
                    }
                    else
                    {
                        OF.ApplicationUser analyst = context.ApplicationUsers.Where(a => a.UserID == existing.AnalystId).FirstOrDefault();
                        throw new ApplicationException(String.Format("{0} already has an open Focus List entry for {1}", analyst.Name,instrumentMarket.BloombergTicker));
                    }
                }
                OF.FocusList focusList = new OF.FocusList();
                context.FocusLists.Add(focusList);
                focusList.RelativeIndexInstrumentMarketId = GetRelativeIndexId(context, instrumentMarket.IssuerID);
                focusList.AnalystId = analystId;
                focusList.InDate = inDate;
                focusList.InPrice = inPrice;                
                focusList.IsLong = isLong;
                focusList.InstrumentMarketId = instrumentMarketId;
                focusList.IssuerId = instrumentMarket.IssuerID;
                
                PriceClient client = new PriceClient();
                PriceFocusList(client, focusList, DateTime.Today);
                focusList.EndOfYearPrice = inPrice;
                CheckPrice(focusList.InPrice, focusList, context, "In");
                focusList.RelativeInPrice = focusList.RelativeCurrentPrice;
                focusList.RelativeEndOfYearPrice = focusList.RelativeInPrice;
                context.SaveChanges();
                Logger.Info($"Add to Focus List instrumentMarketId {instrumentMarketId} now done");
            }
        }

        private void CheckPrice(decimal priceToCompare, OF.FocusList focusList, OF.KeeleyModel context,string priceType)
        {
            if (Math.Abs((priceToCompare - focusList.CurrentPrice) / priceToCompare) > .1m)
            {
                OF.InstrumentMarket instrumentMarket = context.InstrumentMarkets.Where(a => a.InstrumentMarketID == focusList.InstrumentMarketId).FirstOrDefault();
               // throw new ApplicationException(String.Format("{0} Price of {1} is more than 10% different than current price of {2} for {3}",priceType, Math.Round(focusList.InPrice, 2), Math.Round(focusList.CurrentPrice, 2), instrumentMarket.BloombergTicker));
            }
        }

        private void PriceFocusList(PriceClient client, OF.FocusList focusList, DateTime referenceDate)
        {
            OF.Price price = client.Get(focusList.InstrumentMarketId, (int)EntityRankingSchemeIds.Default, referenceDate);
            OF.Price relativePrice = client.Get(focusList.RelativeIndexInstrumentMarketId, (int)EntityRankingSchemeIds.Default, referenceDate);
           
            focusList.CurrentPrice = price.Value;
            focusList.CurrentPriceId = price.PriceId;
            focusList.CurrentPriceDate = price.ReferenceDate;
            focusList.RelativeCurrentPrice = relativePrice.Value;
            focusList.RelativeCurrentPriceId = relativePrice.PriceId;
            focusList.RelativeCurrentPriceDate = relativePrice.ReferenceDate;
        }

        public void Remove(int instrumentMarketId, int analystId, decimal outPrice, DateTime outDate)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                context._applicationUserIdOverride = analystId;

                OF.FocusList existing = context.FocusLists.Where(a => a.InstrumentMarketId == instrumentMarketId && !a.OutDate.HasValue && a.AnalystId == analystId).FirstOrDefault();

                OF.InstrumentMarket instrumentMarket = context.InstrumentMarkets.Where(a => a.InstrumentMarketID == instrumentMarketId).FirstOrDefault();
                if (existing == null)
                {                    
                    OF.ApplicationUser analyst = context.ApplicationUsers.Where(a => a.UserID == analystId).FirstOrDefault();
                    throw new ApplicationException(String.Format("Analyst {0} has no open Focus List entry for {1} to close", analyst.Name, instrumentMarket.BloombergTicker));
                }
                existing.OutPrice = outPrice;
                existing.OutDate = outDate;
                CheckPrice(existing.OutPrice.Value, existing, context, "Out");
                existing.RelativeOutPrice = existing.RelativeCurrentPrice;
                context.SaveChanges();
            }
        }

        public void ProcessAnalystIdea(int[] issuerIds, int analystId, DateTime date)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                context._applicationUserIdOverride = KeeleyServiceUserId;

                foreach (int issuerId in issuerIds)
                {
                    var idea = context.AnalystIdeas.SingleOrDefault(ai => ai.IssuerId == issuerId);

                    // Add Idea with analystId
                    if (idea == null)
                    {
                        Logger.InfoFormat($"New Analyst Idea  issuerId:{issuerId} - analystId:{analystId} - date:{date}");
                        var newIdea = new OF.AnalystIdea()
                        {
                            IssuerId = issuerId,
                            AnalystId = analystId,
                            ResearchNoteLastReceived =  date
                        };

                        context.AnalystIdeas.Add(newIdea);
                    }
                    else // Update idea
                    {
                        Logger.InfoFormat($"Update Idea: {idea.AnalystIdeaId} analistId:{analystId} - date:{idea.ResearchNoteLastReceived}");
                        // Only update newer dates
                        if (date >= idea.ResearchNoteLastReceived)
                        {
                            idea.ResearchNoteLastReceived = date;

                            if (idea.AnalystId != analystId)
                            {
                                Logger.InfoFormat($"Update with another userId: {analystId} - date:{date}");
                            }
                            else
                            {
                                Logger.InfoFormat($"Update date:{date}");
                            }
                        }
                    }

                }
                context.SaveChanges();
            }

        }


        #region Analyst Ideas

        public List<AnalystIdeaDTO> GetAllIdeas()
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var ideas = context.AnalystIdeas
                    .Include(i => i.Issuer.LegalEntity)
                    .Include("Issuer.Instruments.InstrumentMarkets")
                    .Include(i => i.Analyst)
                    .Include(i => i.InternalOriginator)
                    .Include(i => i.InternalOriginator2)
                    .Include(i => i.ExternalOriginator)
                    .Select(i => new AnalystIdeaDTO
                    {
                        AnalystIdeaId = i.AnalystIdeaId,
                        OriginatingDate = i.OriginatingDate,
                        ResearchNoteLastReceived = i.ResearchNoteLastReceived,
                        Issuer = i.Issuer.LegalEntity.Name,
                        BloombergTickers =  i.Issuer.Instruments.SelectMany(x => x.InstrumentMarkets.Select(im => im.BloombergTicker)),
                        IssuerId = i.IssuerId,
                        Analyst = (i.Analyst != null ? i.Analyst.Name : null),
                        AnalystId = i.AnalystId,
                        InternalOriginator = (i.InternalOriginator != null ? i.InternalOriginator.Name : null),
                        InternalOriginatorId = i.InternalOriginatorId,
                        InternalOriginator2 = (i.InternalOriginator2 != null ? i.InternalOriginator2.Name : null),
                        InternalOriginatorId2 = i.InternalOriginatorId2,
                        ExternalOriginator = (i.ExternalOriginator != null ? i.ExternalOriginator.Name : null),
                        ExternalOriginatorId = i.ExternalOriginatorId,
                        IsOriginatedLong = i.IsOriginatedLong,
                    })
                    .ToList();

                // Call .ToList() on nested list of entitites
                foreach (var idea in ideas) {
                    idea.BloombergTickers = idea.BloombergTickers.Where(t => t != null).Distinct().ToList();
                }

                return ideas;
            }
        }

        public int CreateIdea(AnalystIdeaDTO dto)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var newIdea = new OF.AnalystIdea
                {
                    IssuerId = dto.IssuerId,
                    AnalystId = dto.AnalystId,
                    InternalOriginatorId = dto.InternalOriginatorId,
                    InternalOriginatorId2 = dto.InternalOriginatorId2,
                    ExternalOriginatorId = dto.ExternalOriginatorId,
                    OriginatingDate = dto.OriginatingDate,
                    IsOriginatedLong = dto.IsOriginatedLong,
                };
                context.AnalystIdeas.Add(newIdea);
                context.SaveChanges();
                return newIdea.AnalystIdeaId;
            }
        }

        public void DeleteIdea(int id)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == id);
                context.AnalystIdeas.Remove(idea);
                context.SaveChanges();
            }
        }

        public void SetAnalyst(int ideaId, int? userId)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.AnalystId = userId;
                context.SaveChanges();
            }
        }

        public void SetInternalOriginator(int ideaId, int? userId)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.InternalOriginatorId = userId;
                context.SaveChanges();
            }
        }

        public void SetInternalOriginator2(int ideaId, int? userId)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.InternalOriginatorId2 = userId;
                context.SaveChanges();
            }
        }

        public void SetExternalOriginator(int ideaId, int? externalPersonId)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.ExternalOriginatorId = externalPersonId;
                context.SaveChanges();
            }
        }

        public void SetOriginatingDate(int ideaId, DateTime? originatingDate)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.OriginatingDate = originatingDate;
                context.SaveChanges();
            }
        }

        public void SetIsOriginatedLong(int ideaId, bool? isLong)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                var idea = context.AnalystIdeas.First(i => i.AnalystIdeaId == ideaId);
                idea.IsOriginatedLong = isLong;
                context.SaveChanges();
            }
        }

        public void AddIdea(AnalystIdea dto)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                AnalystIdeaManager analystIdeaManager = new AnalystIdeaManager(context);
                analystIdeaManager.Add(dto);
                context.SaveChanges();
            }
        }

        #endregion Analyst Ideas
    }
}
