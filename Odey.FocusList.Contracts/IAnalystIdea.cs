using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odey.FocusList.Contracts
{
    public interface IAnalystIdea
    {
        DateTime EffectiveFromDate { get; set; }

        DateTime EffectiveToDate { get; set; }

        bool IsLong { get; set; }
    }
}