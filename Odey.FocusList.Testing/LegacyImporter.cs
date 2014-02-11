using OFE=Odey.Framework.Keeley.Entities;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.StaticServices.Clients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;
using Odey.Framework.Keeley.Entities;
using ServiceModelEx;
using Odey.MarketData.Clients;

namespace Odey.FocusList.Testing
{
    public static class LegacyImporter
    {
        private static int? GetUserId(int legacyUserId)
        {
            switch (legacyUserId)
            {
                case 1:  //Massey
                    return 46;
                case 21:  //Freddie Neave
                    return 92;
                case 3:  //Bruce
                    return 77;
                case 4:  //Roberto
                    return 85;
                case 5:  //Patrick
                    return 76;
                case 17:  //Alex M
                    return 83;
                case 7:  //Alaric
                    return 79;
                case 19:  //Julian W
                    return 78;
                case 18:  //Antonella
                    return null;
                case 10:  //Jamie
                    return 58;
                case 11:  //Ambrose
                    return null;
                case 12:  //Raj
                    return 27;
                case 13:  //James Spalton
                    return 81;
                case 20:  //Massimo
                    return 84;
                case 22:  //Joe Boorman
                    return null;
                case 23:  //Camilla Shirley
                    return 82;
                case 24:  //Jake Thomson
                    return 87;
                case 25:  //Aubrey
                    return 89;
                default:
                    throw new ApplicationException(String.Format("Unknown known legacy user id {0}", legacyUserId));

            }

        }

        private static string CleanTicker(string bloombergTicker)
        {
            return bloombergTicker.Replace(" UN ", " US ").Replace(" SM ", " SQ ").Replace(" GR ", " GY ");
        }

        private static int GetInstrumentMarketId(string bloombergTicker)
        {
            InstrumentMarketClient instrumentMarketClient = new InstrumentMarketClient();
            OFE.InstrumentMarket instrumentMarket = instrumentMarketClient.GetForIdentifierSettingUpIfNotPresent(IdentifierTypeIds.BBTicker, CleanTicker(bloombergTicker), false, (int)InstrumentClassIds.OrdinaryShare);
            if (instrumentMarket == null)
            {
                throw new ApplicationException(String.Format("Unable to resolve Instrument Market for ticker {0}",bloombergTicker));
            }
            return instrumentMarket.InstrumentMarketID;
        }


        public static List<OFE.FocusList> GetLegacyTransformed()
        {
            var connectionString = "Data Source = C:\\temp\\tradeinput.sdf;Persist Security Info=False";
            var adapter = new SqlCeDataAdapter("select * from positions", connectionString);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            List<OFE.FocusList> focusLists = new List<OFE.FocusList>();
            DataTable dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                int legacyAnalystId = int.Parse(row["analystid"].ToString());
                int? analystId = GetUserId(legacyAnalystId);
                if (analystId.HasValue)
                {
                    OFE.FocusList focusList = new OFE.FocusList();
                    focusLists.Add(focusList);

                    focusList.AnalystId = analystId.Value;
                    string bloombergTicker = row["ticker"].ToString();
                    focusList.InstrumentMarketId = GetInstrumentMarketId(bloombergTicker);
                    string inDateString = row["indate"].ToString();
                    focusList.InDate = DateTime.Parse(inDateString);
                    string inPriceString = row["inprice"].ToString();
                    focusList.InPrice = Decimal.Parse(inPriceString);
                    string outDateString = row["outdate"].ToString();
                    if (!string.IsNullOrWhiteSpace(outDateString))
                    {
                        focusList.OutDate = DateTime.Parse(outDateString);
                    }
                    string outPriceString = row["outprice"].ToString();
                    if (!string.IsNullOrWhiteSpace(outPriceString))
                    {
                        focusList.OutPrice = Decimal.Parse(outPriceString);
                    }
                    string direction = row["direction"].ToString();
                    if (direction == "0")
                    {
                        focusList.IsLong = true;
                    }
                    else
                    {
                        focusList.IsLong = false;
                    }
                    string sypoverride = row["sypoverride"].ToString();
                    if (!string.IsNullOrWhiteSpace(sypoverride))
                    {
                        focusList.StartOfYearPrice = Decimal.Parse(sypoverride);
                    }
                    DateTime referenceDateForPrice = DateTime.Today;
                    if (focusList.OutDate.HasValue)
                    {
                        referenceDateForPrice = focusList.OutDate.Value.Date;
                    }
                    PriceClient client = new PriceClient();
                    Price price = client.Get(focusList.InstrumentMarketId, (int)EntityRankingSchemeIds.Default, referenceDateForPrice);

                    focusList.CurrentPrice = price.Value;
                    focusList.CurrentPriceId = price.PriceId;
                }
            }
            return focusLists;
        }
    }
}