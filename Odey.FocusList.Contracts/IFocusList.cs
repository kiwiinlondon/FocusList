using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OF = Odey.Framework.Keeley.Entities;

namespace Odey.FocusList.Contracts
{
    [ServiceContract(Namespace = "Odey.FocusList.Contracts")]
    public interface IFocusList
    {
        [OperationContract]
        void Save(OF.FocusList focusList);

        [OperationContract]
        void SaveList(List<OF.FocusList> focusList);

        [OperationContract]
        void UpdatePrice(OF.Price price);

        [OperationContract]
        void Reprice(DateTime repriceDate);
    }
}
