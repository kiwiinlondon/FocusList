using Odey.CodeRed.Clients;
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
                        var flClient = new FocusListClient();
            //var flClient = new FocusListService.FocusListService();
            flClient.Add(433, DateTime.Parse("11-dec-2017"), 87.58m, 77, true, true);
        }

        public static void ImportLegacy()
        {
            List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
            FocusListClient client = new FocusListClient();
            client.SaveList(focusList);
        }
    }
}
