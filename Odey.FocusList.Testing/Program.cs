using Odey.FocusList.Clients;
using Odey.StaticServices.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OF = Odey.Framework.Keeley.Entities;

namespace Odey.FocusList.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            InstrumentClient client = new InstrumentClient();
            client.RepullFromExternalSourceUsingId(31037);
           // FocusListService.FocusListService client = new FocusListService.FocusListService();
          //  FocusListClient client = new FocusListClient();
          //  client.Reprice(DateTime.Today);

            ImportLegacy();            
        }

        public static void ImportLegacy()
        {
            List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
            FocusListClient client = new FocusListClient();
            client.SaveList(focusList);
        }
    }
}
