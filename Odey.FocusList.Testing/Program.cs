﻿using Odey.FocusList.Clients;
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
            ImportLegacy();  


            FocusListService.FocusListService client = new FocusListService.FocusListService();
          //  FocusListClient client = new FocusListClient();
          //  client.Add(7244, new DateTime(2013,9,20),3.48M, 89, false);
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
