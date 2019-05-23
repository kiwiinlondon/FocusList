using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Contracts
{
    [DataContract]
    public class FocusListDTO: IAnalystIdea
    {
        [DataMember]
        public int InstrumentMarketId { get; set; }
        [DataMember]
        public DateTime EffectiveFromDate { get; set; }
        [DataMember]
        public DateTime EffectiveToDate { get; set; }
        [DataMember]
        public decimal InPrice { get; set; }
        [DataMember]
        public int AnalystId { get; set; }
        [DataMember]
        public bool IsLong { get; set; }
        
    }
}
