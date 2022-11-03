using Odey.Framework.Keeley.Entities.Enums;
using Odey.MarketData.Clients;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using OF = Odey.Framework.Keeley.Entities;
using MD = Odey.MarketData.Contracts;

namespace Odey.FocusList.FocusListService
{
    public class Pricer
    {

        public void Reprice(DateTime repriceDate)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                Reprice(context, repriceDate);
                context.SaveChanges();
            }
        }

        public void Reprice(OF.KeeleyModel context)
        {
            DateTime tradeDateRollDate = context.PortfolioRollDates.Where(a => a.PortfolioAggregationLevelId == (int)PortfolioAggregationLevelIds.TradeDate).FirstOrDefault().RollDate;
            Reprice(context, tradeDateRollDate);

        }

        public void Reprice(OF.KeeleyModel context, DateTime repriceDate)
        {

            List<OF.FocusList> focusLists = context.FocusLists
                .Where(a => !a.OutDate.HasValue)
                .ToList();
            PriceClient client = new PriceClient();
            foreach (OF.FocusList focusList in focusLists)
            {
                PriceFocusList(client, focusList, repriceDate);
            }

        }

        public void PriceFocusList(PriceClient client, OF.FocusList focusList, DateTime referenceDate)
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


        private Dictionary<int, Dictionary<DateTime,MD.Price>> GetAdjustedPricesByInstrumentMarketId(PriceClient priceClient, List<OF.FocusList> focusLists)
        {
            var instrumentMarketIds = focusLists.Select(a => a.InstrumentMarketId).Union(focusLists.Select(a => a.RelativeIndexInstrumentMarketId)).Distinct().ToArray();


            var minInDateByInstrumentMarketId = focusLists.GroupBy(a => a.InstrumentMarketId).ToDictionary(a => a.Key, a => a.Min(b => b.InDate));
            var adjustedPrices = priceClient.GetAdjustedPrices(instrumentMarketIds, focusLists.Min(a => a.InDate), DateTime.Today);
            Dictionary<int, Dictionary<DateTime, MD.Price>> pricesByInstrumentMarketId = new Dictionary<int, Dictionary<DateTime, MD.Price>>();

        

            foreach (var price in adjustedPrices)
            {
                if (!pricesByInstrumentMarketId.TryGetValue(price.InstrumentMarketId,out var pricesForInstruemntMarket))
                {
                    pricesForInstruemntMarket = new Dictionary<DateTime, MD.Price>();
                    pricesByInstrumentMarketId.Add(price.InstrumentMarketId, pricesForInstruemntMarket);
                }
                if (pricesForInstruemntMarket.ContainsKey(price.ReferenceDate))
                {
                    var minInDate = minInDateByInstrumentMarketId[price.InstrumentMarketId];
                    if (minInDate<=price.ReferenceDate)
                    {
                        throw new ApplicationException($"Duplicate price on referencedate for instrument {price.InstrumentMarketId} on {price.ReferenceDate}");
                    }
                }
                else
                {
                    pricesForInstruemntMarket.Add(price.ReferenceDate, price);
                }
            }
            return pricesByInstrumentMarketId;
        }



        public void ApplyAdjustedPrices()
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                List<OF.FocusList> focusLists = context.FocusLists.Include(a => a.FocusListPrices).Where(a => !a.OutDate.HasValue)
               // .Where(a => a.OutDate.HasValue && a.OutDate >= new DateTime(2019, 1, 1) && a.OutDate < new DateTime(2020, 1, 1))
               // .Where(a=>a.FocusListId == 686)// || a.FocusListId == 595 || a.FocusListId == 604)
                .ToList();


                ApplyAdjustedPrices(focusLists);

                context.SaveChanges();
            }
        }

        public void ApplyAdjustedPricesToClosedPositions(DateTime startDate, DateTime endDate)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                List<OF.FocusList> focusLists = context.FocusLists.Include(a => a.FocusListPrices)
                .Where(a => a.OutDate.HasValue && a.OutDate >= startDate && a.OutDate < endDate)
                .ToList();


                ApplyAdjustedPrices(focusLists);

                context.SaveChanges();
            }
        }

        public void ApplyAdjustedPrices(List<OF.FocusList> focusLists)
        {
            PriceClient priceClient = new PriceClient();
            var pricesByInstrumentMarketId = GetAdjustedPricesByInstrumentMarketId(priceClient, focusLists);

            foreach (var focusList in focusLists)
            {
                ApplyAdjustedPricesToFocusList(priceClient, focusList, pricesByInstrumentMarketId);
            }
        }


        private decimal GetAdjustedPrice(PriceClient priceClient, int instrumentMarketId,DateTime referenceDate,MD.Price price, decimal focusListPrice, int focusListId, MD.Price relativePrice,int relativeInstrumentMarketId, bool isOut, decimal inPrice)
        {
            if (relativePrice == null || relativePrice.ReferenceDate != referenceDate  )
            {
                if (!(referenceDate.DayOfWeek == DayOfWeek.Monday && relativePrice.ReferenceDate == referenceDate.AddDays(-3)))
                { 
                    throw new ApplicationException($"No relative price found on in date for instrument market {relativeInstrumentMarketId}. Focus List Id = {focusListId}");
                }
            }
            if (price==null)
            {
                DateTime previousDate = DateTime.Today.AddDays(-1);
                while (previousDate.DayOfWeek == DayOfWeek.Saturday || previousDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    previousDate = previousDate.AddDays(-1);
                }
                if ((referenceDate == DateTime.Today || referenceDate == previousDate) && !isOut)
                {
                    return inPrice;
                }
                 throw new ApplicationException($"Adjusted Price cant be null focuslist for instrument market {instrumentMarketId}. Focus List Id = {focusListId}");
            }
            if (isOut)
            {
                if (price.ReferenceDate< referenceDate.AddDays(-7))
                {
                    throw new ApplicationException($"Adjusted Price date must be close to out date of focuslist for instrument market {instrumentMarketId}. Focus List Id = {focusListId}");
                }
            }
            else
            {
                if (price.ReferenceDate != referenceDate)
                {
                    throw new ApplicationException($"Adjusted Price date must match in date of focuslist for instrument market {instrumentMarketId}. Focus List Id = {focusListId}");
                }
            }

            var closingPrice = priceClient.Get(instrumentMarketId, (int)EntityRankingSchemeIds.Default, referenceDate);
            if (closingPrice.ReferenceDate != closingPrice.RawPrice.ReferenceDate)
            {
                throw new ApplicationException($"No price found to adjust focusList on {referenceDate} for instrument market {instrumentMarketId}. Focus List Id = {focusListId}");
            }
            return Math.Round(GetPriceValue(price) / closingPrice.Value * focusListPrice, 6);
        }

        private decimal GetPriceValue(MD.Price price,decimal? defaultPrice = null)
        {
            return (price.AskValue + price.BidValue) / 2;
        }

        public void ApplyAdjustedPricesToFocusList(PriceClient priceClient , OF.FocusList focusList, Dictionary<int, Dictionary<DateTime, MD.Price>> pricesByInstrumentMarket)
        {
            if (focusList.InstrumentMarketId == 34315)
            {
                int i = 0;
            }
            if (pricesByInstrumentMarket.TryGetValue(focusList.InstrumentMarketId, out var prices))
            { 
                var relativePrices = pricesByInstrumentMarket[focusList.RelativeIndexInstrumentMarketId];
                var existingPrices = focusList.FocusListPrices.ToDictionary(a => a.ReferenceDate, a => a);

                DateTime currentDate = focusList.InDate;
                DateTime outDate = DateTime.Today;

                if (focusList.OutDate.HasValue)
                {
                    outDate = focusList.OutDate.Value;
                }


                MD.Price previousPrice = null;
                MD.Price previousRelativePrice = null;
                while (currentDate <= outDate)
                {
                    if (!existingPrices.TryGetValue(currentDate, out var existingPrice))
                    {
                        existingPrice = new OF.FocusListPrices() { FocusListId = focusList.FocusListId, ReferenceDate = currentDate };
                        focusList.FocusListPrices.Add(existingPrice);
                    }

                    if (!prices.TryGetValue(currentDate, out var price))
                    {
                        price = previousPrice;
                       
                    }


                    if (!relativePrices.TryGetValue(currentDate, out var relativePrice))
                    {
                        relativePrice = previousRelativePrice;
                        if (relativePrice == null && currentDate.DayOfWeek == DayOfWeek.Monday)
                        {
                            relativePrices.TryGetValue(currentDate.AddDays(-3), out relativePrice);
                        }
                    }


                    if (focusList.InDate == currentDate)
                    {
                        focusList.AdjustedInPrice = GetAdjustedPrice(priceClient, focusList.InstrumentMarketId, focusList.InDate, price, focusList.InPrice, focusList.FocusListId, relativePrice, focusList.RelativeIndexInstrumentMarketId, false, focusList.AdjustedInPrice);
                    }

                    if (focusList.OutDate.HasValue && currentDate == focusList.OutDate.Value)
                    {

                        focusList.AdjustedOutPrice = GetAdjustedPrice(priceClient, focusList.InstrumentMarketId, focusList.OutDate.Value, price, focusList.OutPrice.Value, focusList.FocusListId, relativePrice, focusList.RelativeIndexInstrumentMarketId, true, focusList.AdjustedInPrice);
                    }
                    previousPrice = price;
                    previousRelativePrice = relativePrice;
                    if (price == null)
                    {
                        price = prices[prices.Keys.Max()];
                    }
                    existingPrice.Price = GetPriceValue(price);
                    existingPrice.RelativePrice = GetPriceValue(relativePrice);

                    currentDate = currentDate.AddDays(1);
                    
                    
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        currentDate = currentDate.AddDays(2);
                    }
                    else if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        currentDate = currentDate.AddDays(1);
                    }
                }
                if (focusList.InstrumentMarketId == 28193 && focusList.AdjustedInPrice < 1)
                {
                    throw new ApplicationException("Codemasters");
                }
            }
        }

    }
}