using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.MarketData.Clients;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using OF = Odey.Framework.Keeley.Entities;

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
                existingFocusList.StartOfYearPrice = newFocusList.StartOfYearPrice;
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
                OF.FocusList focusList = context.FocusLists.Where(a => a.CurrentPriceId == price.PriceId && !a.OutDate.HasValue).FirstOrDefault();
                if (focusList != null)
                {
                    focusList.CurrentPrice = price.Value;
                    context.SaveChanges();
                }
            }
        }
    }
}
