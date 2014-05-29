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

            
          //  FocusListService.FocusListService client = new FocusListService.FocusListService();
          
            FocusListClient client = new FocusListClient();
            client.Reprice(DateTime.Today);
            client.Add(20126, DateTime.Today, 4.5M, 81, true);
            client.Remove(7244, 89, 3.48M, DateTime.Today);
                      
        }

        public static void ImportLegacy()
        {
            List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
            FocusListClient client = new FocusListClient();
            client.SaveList(focusList);
        }
    }
}
