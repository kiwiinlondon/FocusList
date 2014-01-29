using Odey.FocusList.Contracts;
using Odey.Framework.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Odey.FocusList.FocusListService
{
    public class FocusListService : OdeyServiceBase,  IFocusList
    {       
        public void ImportExisting()
        {
            Logger.Info("Hello World");
        }
    }
}
