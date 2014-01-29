using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Clients
{
    public class FocusListClient : OdeyClientBase<IFocusList>, IFocusList
    {
        

        public void ImportExisting()
        {
            IFocusList proxy = factory.CreateChannel();
            try
            {
                proxy.ImportExisting();
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
