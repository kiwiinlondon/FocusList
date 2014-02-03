using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Services;
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
                if (existingFocusList == null)
                {
                    context.FocusLists.Add(focusList);
                }
                else
                {
                    existingFocusList.AnalystId = focusList.AnalystId;
                    existingFocusList.InDate = focusList.InDate;
                    existingFocusList.InPrice = focusList.InPrice;
                    existingFocusList.IsLong = focusList.IsLong;
                    existingFocusList.OutDate = focusList.OutDate;
                    existingFocusList.OutPrice = focusList.OutPrice;
                    existingFocusList.StartOfYearPrice = focusList.StartOfYearPrice;
                }
                context.SaveChanges();
            }
        }

        private void Save(OF.FocusList newFocusList, OF.KeeleyModel context, OF.FocusList existingFocusList)
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
                existingFocusList.OutDate = newFocusList.OutDate;
                existingFocusList.OutPrice = newFocusList.OutPrice;
                existingFocusList.StartOfYearPrice = newFocusList.StartOfYearPrice;
            }

        }

        public void SaveList(List<OF.FocusList> focusLists)
        {
            using (OF.KeeleyModel context = new OF.KeeleyModel(SecurityCallStackContext.Current))
            {
                Dictionary<int, OF.FocusList> existingFocusList = context.FocusLists.ToDictionary(a => a.InstrumentMarketId, a => a);
                foreach (OF.FocusList focusListEntry in focusLists)
                {
                    OF.FocusList existingFocusListEntry = null;
                    existingFocusList.TryGetValue(focusListEntry.InstrumentMarketId, out existingFocusListEntry);
                    Save(focusListEntry,context,existingFocusListEntry);
                }                
                context.SaveChanges();
            }
        }
    }
}
