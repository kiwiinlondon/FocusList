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
          //  AnalystIdea dto = new AnalystIdea()
          //  {
          //      IssuerId = 6290,
          //      Analysts = new List<AnalystDTO>()
          //      { 
          //          new AnalystDTO { AnalystId = 1, EffectiveFromDate = new DateTime(1976,5,20), IsLong = false },
          //           // new AnalystDTO { AnalystId = 211, EffectiveFromDate = new DateTime(1976,5,20), IsLong = true }
          //      },
          //      FocusLists = new List<FocusListDTO> { new FocusListDTO() { EffectiveFromDate = new DateTime(2014, 2, 27), EffectiveToDate = null,  AnalystId = 81, IsLong = true, InPrice = 5, InstrumentMarketId = 19183 } },
          //      Originators = new List<OriginatorDTO> { new OriginatorDTO() { EffectiveFromDate = new DateTime(1900, 1, 1), EffectiveToDate = new DateTime(2018, 12, 31), InternalOriginatorId = 1, IsLong = true },
          //                                              new OriginatorDTO() { EffectiveFromDate = new DateTime(1900, 1, 1), EffectiveToDate = new DateTime(2018, 12, 31), InternalOriginatorId = 1, IsLong = false } }
          //  };

            FocusListClient flClient = new FocusListClient();
         //   FocusListService.FocusListService flClient = new FocusListService.FocusListService();
            //flClient.Remove(10184,81,0, DateTime.Today);
            //  focusListService.AddIdea(dto);


            // var flClient = new FocusListClient();
            flClient.ApplyAdjustedPrices();
           //var flClient = new FocusListService.FocusListService();
            
            
          //  flClient.ApplyAdjustedPricesToClosedPositions(new DateTime(2018, 1, 1),new DateTime(2019, 1, 1));
          //  flClient.ApplyAdjustedPricesToClosedPositions(new DateTime(2017, 1, 1), new DateTime(2018, 1, 1));
            flClient.ApplyAdjustedPricesToClosedPositions(new DateTime(2012, 1, 1), new DateTime(2013, 1, 1));

            ////flClient.Remove(24050, 181,193.02000000m, new DateTime(2020,5,8));
            
            flClient.Remove(26423, 98, 1.49m, new DateTime(2020, 5, 11));
        }
 	 	 	 	 	 	 	 	 	 	 	 	 

        //public static void ImportLegacy()
        //{
        //    List<OF.FocusList> focusList = LegacyImporter.GetLegacyTransformed();
        //    FocusListClient client = new FocusListClient();
        //    client.SaveList(focusList);
        //}
    }
}
