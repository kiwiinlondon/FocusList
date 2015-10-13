
using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using log4net.Repository.Hierarchy;
using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.MarketData.Clients;
using ServiceModelEx;
using OF = Odey.Framework.Keeley.Entities;
using Odey.CodeRed.Clients;

namespace Odey.FocusList.FocusListService
{
    public class FocusListService : OdeyServiceBase, IFocusList
    {
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
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                OF.FocusList existing = context.FocusLists.Where(a => a.InstrumentMarketId == instrumentMarketId && !a.OutDate.HasValue).FirstOrDefault();
                OF.InstrumentMarket instrumentMarket = context.InstrumentMarkets.Include(a=>a.Instrument).Where(a => a.InstrumentMarketID == instrumentMarketId).FirstOrDefault();
                if (existing != null)
                {
                    if (existing.InDate== inDate && existing.InPrice == inPrice && existing.IsLong == isLong && existing.AnalystId == analystId)
                    {
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
                
                PriceClient client = new PriceClient();
                PriceFocusList(client, focusList, DateTime.Today);
                focusList.EndOfYearPrice = inPrice;
                CheckPrice(focusList.InPrice, focusList, context, "In");
                focusList.RelativeInPrice = focusList.RelativeCurrentPrice;
                focusList.RelativeEndOfYearPrice = focusList.RelativeInPrice;
                AddToCodeRed(instrumentMarket, isLong);
                context.SaveChanges();
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
                RemoveFromCodeRed(instrumentMarket);
                context.SaveChanges();
            }
        }

        public void ProcessAnalystIdea(int[] issuerIds, int analystId, DateTime date)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                foreach (int issuerId in issuerIds)
                {
                    var idea = context.AnalystIdeas.SingleOrDefault(ai => ai.IssuerId == issuerId);

                    // Add Idea with analystId
                    if (idea == null)
                    {
                        Logger.InfoFormat($"New Analyst Idea  issuerId:{issuerId} - analystId:{analystId}");
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
                        if (idea.AnalystId != analystId)
                        {
                            Logger.InfoFormat($"Update Idea: {idea.AnalystIdeaId} with another userId: {analystId}");
                        }
                        else
                        {
                            Logger.InfoFormat($"Update Idea: {idea.AnalystIdeaId}");
                        }

                        idea.ResearchNoteLastReceived = date;
                    }

                }
                context.SaveChanges();
            }

        }

        private void AddToCodeRed(OF.InstrumentMarket instrumentMarket,bool isLong)
        {
            CodeRedClient client = new CodeRedClient();
            client.AddToFocusList(instrumentMarket.BloombergTicker, isLong);
        }

        private void RemoveFromCodeRed(OF.InstrumentMarket instrumentMarket)
        {
            CodeRedClient client = new CodeRedClient();
            client.RemoveFromFocusList(instrumentMarket.BloombergTicker);
        }

        public void BringCodeRedInLine()
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                List<OF.FocusList> actualFocusList = context.FocusLists.Where(a => !a.OutDate.HasValue).ToList();
                CodeRedClient client = new CodeRedClient();
                List<string> codeRedFocusList = client.GetOpenFocusList();
                foreach (OF.FocusList focusList in actualFocusList)
                {
                    OF.InstrumentMarket instrumentMarket = context.InstrumentMarkets.Where(a => a.InstrumentMarketID == focusList.InstrumentMarketId).FirstOrDefault();
                    if (codeRedFocusList.Contains(instrumentMarket.BloombergTicker))
                    {
                        codeRedFocusList.Remove(instrumentMarket.BloombergTicker);
                    }
                    client.AddToFocusList(instrumentMarket.BloombergTicker, focusList.IsLong);
                }
                foreach (string ticker in codeRedFocusList)
                {
                    client.RemoveFromFocusList(ticker);
                }                
            }
        }
    }
}
