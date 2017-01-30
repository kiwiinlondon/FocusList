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

        [OperationContract]
        void Add(int instrumentMarketId, DateTime inDate, decimal inPrice, int analystId, bool isLong, bool skipCodeRed = false);

        [OperationContract]
        void Remove(int instrumentMarketId, int analystId, decimal outPrice, DateTime outDate);

        [OperationContract]
        void ProcessAnalystIdea(int[] issuerId, int analystId, DateTime date);

        [OperationContract]
        IEnumerable<OF.AnalystIdea> GetAllIdeas();

        [OperationContract]
        int CreateIdea(OF.AnalystIdea idea);

        [OperationContract]
        void DeleteIdea(int id);

        [OperationContract]
        void SetAnalyst(int ideaId, int? userId);

        [OperationContract]
        void SetInternalOriginator(int ideaId, int? userId);

        [OperationContract]
        void SetInternalOriginator2(int ideaId, int? userId);

        [OperationContract]
        void SetExternalOriginator(int ideaId, int? externalPersonId);

        [OperationContract]
        void SetOriginatingDate(int ideaId, DateTime originatingDate);

    }
}
