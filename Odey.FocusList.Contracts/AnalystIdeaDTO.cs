using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Contracts
{
    [DataContract]
    public class AnalystIdea
    {
        [DataMember]
        public int IssuerId { get; set; }

        [DataMember]
        public List<OriginatorDTO> Originators { get; set; }

        [DataMember]
        public List<FocusListDTO> FocusLists { get; set; }

        [DataMember]
        public List<AnalystDTO> Analysts { get; set; }
    }
}
