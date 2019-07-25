using Odey.Framework.Keeley.Entities.Enums;
using Odey.MarketData.Clients;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OF = Odey.Framework.Keeley.Entities;

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

    }
}