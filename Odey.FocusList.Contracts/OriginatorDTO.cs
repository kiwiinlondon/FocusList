using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Contracts
{
    [DataContract]
    public class OriginatorDTO : IAnalystIdea
    {
        [DataMember]
        public DateTime EffectiveFromDate { get; set; }
        [DataMember]
        public DateTime? EffectiveToDate { get; set; }
        [DataMember]
        public bool IsLong { get; set; }
        [DataMember]
        public int ExternalOriginatorId { get; set; }
        [DataMember]
        public int InternalOriginatorId { get; set; }
        [DataMember]
        public int InternalOriginatorId2 { get; set; }
    }
}
