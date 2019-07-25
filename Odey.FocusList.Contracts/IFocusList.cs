using System;
using System.Collections.Generic;
using System.ServiceModel;
using OF = Odey.Framework.Keeley.Entities;

namespace Odey.FocusList.Contracts
{
    public class AnalystIdeaDTO
    {
        public int AnalystIdeaId { get; set; }

        public string Analyst { get; set; }

        public int? AnalystId { get; set; }

        public string Issuer { get; set; }

        public int IssuerId { get; set; }

        public IEnumerable<string> BloombergTickers { get; set; }

        public string InternalOriginator { get; set; }

        public int? InternalOriginatorId { get; set; }

        public string InternalOriginator2 { get; set; }

        public int? InternalOriginatorId2 { get; set; }

        public string ExternalOriginator { get; set; }

        public int? ExternalOriginatorId { get; set; }

        public DateTime? OriginatingDate { get; set; }

        public bool? IsOriginatedLong { get; set; }

        public DateTime? ResearchNoteLastReceived { get; set; }
    }

    [ServiceContract(Namespace = "Odey.FocusList.Contracts")]
    public interface IFocusList
    {
        [OperationContract]
        void Save(OF.FocusList focusList);

        [OperationContract]
        void SaveList(List<OF.FocusList> focusList);

        [OperationContract]
        List<OF.FocusList> GetAll();

        [OperationContract]
        void UpdatePrice(OF.Price price);

        [OperationContract]
        void Reprice(DateTime repriceDate);

        [OperationContract]
        void Add(int instrumentMarketId, DateTime inDate, decimal inPrice, int analystId, bool isLong);

        [OperationContract]
        void AddIdea(AnalystIdea dto);

        [OperationContract]
        void Remove(int instrumentMarketId, int analystId, decimal outPrice, DateTime outDate);

        [OperationContract]
        void ProcessAnalystIdea(int[] issuerId, int analystId, DateTime date);

        [OperationContract]
        List<AnalystIdeaDTO> GetAllIdeas();

        [OperationContract]
        int CreateIdea(AnalystIdeaDTO idea);

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
        void SetOriginatingDate(int ideaId, DateTime? originatingDate);

        [OperationContract]
        void SetIsOriginatedLong(int ideaId, bool? isLong);

        [OperationContract]
        void RunTasks(int[] taskIds);
    }
}
