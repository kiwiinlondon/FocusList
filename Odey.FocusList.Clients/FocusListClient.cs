using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OF=Odey.Framework.Keeley.Entities;

namespace Odey.FocusList.Clients
{
    public class FocusListClient : OdeyClientBase<IFocusList>, IFocusList
    {


  
        public void Save(OF.FocusList focusList)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.Save(focusList);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }


        public void SaveList(List<OF.FocusList> focusList)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SaveList(focusList);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }


        public void UpdatePrice(OF.Price price)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.UpdatePrice(price);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }


        public void Reprice(DateTime repriceDate)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.Reprice(repriceDate);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }


        public void Add(int instrumentMarketId, DateTime inDate, decimal inPrice, int analystId, bool isLong)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.Add(instrumentMarketId, inDate, inPrice, analystId, isLong);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void Remove(int instrumentMarketId,int analystId, decimal outPrice, DateTime outDate)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.Remove(instrumentMarketId, analystId, outPrice, outDate);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void ProcessAnalystIdea(int issuerId, int analystId, DateTime date)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.ProcessAnalystIdea(issuerId, analystId, date);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }
    }
}
