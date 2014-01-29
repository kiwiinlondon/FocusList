using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Contracts
{
    [ServiceContract(Namespace = "Odey.FocusList.Contracts")]
    public interface IFocusList
    {
        [OperationContract]
        void ImportExisting();
    }
}
