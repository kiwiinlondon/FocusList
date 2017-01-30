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


        public void Add(int instrumentMarketId, DateTime inDate, decimal inPrice, int analystId, bool isLong, bool skipCodeRed = false)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.Add(instrumentMarketId, inDate, inPrice, analystId, isLong, skipCodeRed);
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

        public void ProcessAnalystIdea(int[] issuerId, int analystId, DateTime date)
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

        public IEnumerable<OF.AnalystIdea> GetAllIdeas()
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                return proxy.GetAllIdeas();
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public int CreateIdea(OF.AnalystIdea idea)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                return proxy.CreateIdea(idea);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void DeleteIdea(int id)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.DeleteIdea(id);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void SetAnalyst(int ideaId, int? userId)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SetAnalyst(ideaId, userId);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void SetInternalOriginator(int ideaId, int? userId)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SetInternalOriginator(ideaId, userId);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void SetInternalOriginator2(int ideaId, int? userId)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SetInternalOriginator2(ideaId, userId);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void SetExternalOriginator(int ideaId, int? externalPersonId)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SetExternalOriginator(ideaId, externalPersonId);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void SetOriginatingDate(int ideaId, DateTime originatingDate)
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.SetOriginatingDate(ideaId, originatingDate);
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
