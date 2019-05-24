using Odey.CodeRed.Clients;
using Odey.FocusList.Clients;
using Odey.FocusList.Contracts;
using Odey.FocusList.FocusListService;
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
            AnalystIdea dto = new AnalystIdea()
            {
                IssuerId = 6290,
                Analysts = new List<AnalystDTO>() { },
                FocusLists = new List<FocusListDTO> { new FocusListDTO() { EffectiveFromDate = new DateTime(2018, 1, 1), EffectiveToDate = new DateTime(9999, 12, 31),  AnalystId = 5, IsLong = true, InPrice = 99, InstrumentMarketId = 19183 } },
                Originators = new List<OriginatorDTO> { new OriginatorDTO() { EffectiveFromDate = new DateTime(1900, 1, 1), EffectiveToDate = new DateTime(9999, 12, 31), InternalOriginatorId = 1, IsLong = true } }
            };

            FocusListService.FocusListService focusListService = new FocusListService.FocusListService();
            focusListService.AddIdea(dto);


            var flClient = new FocusListClient();
            //var flClient = new FocusListService.FocusListService();
            //flClient.Add(433, DateTime.Parse("11-dec-2017"), 87.58m, 77, true, true);
        }

        //public static void ImportLegacy()
        //{
        //    List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
        //    FocusListClient client = new FocusListClient();
        //    client.SaveList(focusList);
        //}
    }
}
