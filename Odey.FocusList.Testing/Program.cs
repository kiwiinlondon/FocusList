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
            flClient.Add(24011, DateTime.Parse("08-apr-2016"), 48.742m, 87, true, true);
        }

        public static void ImportLegacy()
        {
            List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
            FocusListClient client = new FocusListClient();
            client.SaveList(focusList);
        }
    }
}
