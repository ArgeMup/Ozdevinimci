using System.Xml.XPath;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using ArgeMup.HazirKod.Ekİşlemler;
using ArgeMup.HazirKod;

namespace Özdevinimci
{
    public partial class Cihaz
    {
        public class Sonoff
        {
            public static readonly int ErişimNoktası = 8081;

            public class Bilgi
            {
                public static readonly string Sayfa = "/zeroconf/info";
                public static readonly string Sorgu = @"{""data"":{}}";

                public Cihaz.Durumu Durumu;
                public bool Ayıklandı = false, KapalıOlarakAçılsın, KendiliğindenKapanır;
                public int AçıkKalmaSüresi_sn, Sinyal_Seviyesi;
                public string Sürümü, Tanımlayıcısı;

                public Bilgi(string json)
                {
                    /*{
                        "seq": 2,
                        "error": 0,
                        "data": {
                            "switch": "off",
                            "startup": "off",
                            "pulse": "on",
                            "pulseWidth": 3600000,
                            "ssid": "_test_",
                            "otaUnlock": false,
                            "fwVersion": "3.6.0",
                            "deviceid": "1001231d03",
                            "bssid": "a:5b:d6:da:5b:97",
                            "signalStrength": -28
                        }
                    }*/

                    XElement Kök = XElement.Load(JsonReaderWriterFactory.CreateJsonReader(json.BaytDizisine(), new System.Xml.XmlDictionaryReaderQuotas()));
                    if (Kök.XPathSelectElement("//error").Value != "0") return;

                    Kök = Kök.XPathSelectElement("//data");
                    Durumu = Kök.XPathSelectElement("//switch").Value == "on" ? Durumu.Açık : Durumu.Kapalı;
                    KapalıOlarakAçılsın = Kök.XPathSelectElement("//startup").Value == "off";
                    KendiliğindenKapanır = Kök.XPathSelectElement("//pulse").Value == "on";
                    AçıkKalmaSüresi_sn = Kök.XPathSelectElement("//pulseWidth").Value.TamSayıya() / 1000;
                    Sürümü = Kök.XPathSelectElement("//fwVersion").Value;
                    Tanımlayıcısı = Kök.XPathSelectElement("//data/deviceid").Value;
                    Sinyal_Seviyesi = Kök.XPathSelectElement("//signalStrength").Value.TamSayıya();

                    Ayıklandı = true;
                }
            }
            public class AçKapat
            {
                public static readonly string Sayfa = "/zeroconf/switch";
                public static readonly string Sorgu_Aç = @"{""data"":{""switch"":""on""}}";
                public static readonly string Sorgu_Kapat = @"{""data"":{""switch"":""off""}}";

                public bool Ayıklandı = false;

                public AçKapat(string json)
                {
                    /*{
                        "seq": 2,
                        "error": 0
                    }*/

                    XElement Kök = XElement.Load(JsonReaderWriterFactory.CreateJsonReader(json.BaytDizisine(), new System.Xml.XmlDictionaryReaderQuotas()));
                    if (Kök.XPathSelectElement("//error").Value != "0") return;

                    Ayıklandı = true;
                }
            }
            public class KendiliğindenKapanma
            {
                public static readonly int AzamiZamanAşımı_sn = 3600;
                public static readonly string Sayfa = "/zeroconf/pulse";
                public static string Sorgu(int Gecikme_sn, out int KontrolEdilmişDeğer_sn)
                {
                    if (Gecikme_sn <= 0)
                    {
                        KontrolEdilmişDeğer_sn = 0;

                        return @"{""data"":{""pulse"":""off"",""pulseWidth"":500}}";
                    }
                    else
                    {
                        KontrolEdilmişDeğer_sn = Gecikme_sn > AzamiZamanAşımı_sn ? AzamiZamanAşımı_sn : Gecikme_sn;

                        return @"{""data"":{""pulse"":""on"",""pulseWidth"":" + KontrolEdilmişDeğer_sn + "000}}";
                    }
                }

                public bool Ayıklandı = false;

                public KendiliğindenKapanma(string json)
                {
                    /*{
                        "seq": 2,
                        "error": 0
                    }*/

                    XElement Kök = XElement.Load(JsonReaderWriterFactory.CreateJsonReader(json.BaytDizisine(), new System.Xml.XmlDictionaryReaderQuotas()));
                    if (Kök.XPathSelectElement("//error").Value != "0") return;

                    Ayıklandı = true;
                }
            }
            public class KapalıOlarakAçılma
            {
                public static readonly string Sayfa = "/zeroconf/startup";
                public static readonly string Sorgu = @"{""data"":{""startup"":""off""}}";
               
                public bool Ayıklandı = false;

                public KapalıOlarakAçılma(string json)
                {
                    /*{
                        "seq": 2,
                        "error": 0
                    }*/

                    XElement Kök = XElement.Load(JsonReaderWriterFactory.CreateJsonReader(json.BaytDizisine(), new System.Xml.XmlDictionaryReaderQuotas()));
                    if (Kök.XPathSelectElement("//error").Value != "0") return;

                    Ayıklandı = true;
                }
            }
        }
    }
}
