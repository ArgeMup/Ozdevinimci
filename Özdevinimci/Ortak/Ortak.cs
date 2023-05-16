using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ArgeMup.HazirKod;
using ArgeMup.HazirKod.ArkaPlan;
using ArgeMup.HazirKod.Ekİşlemler;

namespace Özdevinimci
{
    public static class Ortak
    {
        public static Hatırlatıcı_ Görevler = new Hatırlatıcı_();
        public static Depo_ Ayarlar = new Depo_(File.Exists(Kendi.Klasörü + "\\Ayarlar.mup") ? File.ReadAllText(Kendi.Klasörü + "\\Ayarlar.mup").Replace(Environment.NewLine, "\n") : null);

        public static class BilgisayarıKapat
        {
            public static int Sıradakiİşlem = 0;
            public static DateTime Zamanı;

            public static void Kapat(int Saniye)
            {
                #if DEBUG
                    MessageBox.Show("BilgisayarıKapat " + Saniye, "Bilgisayarı kapatma durum değişikliği");
                #else
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "shutdown";
                    process.StartInfo.Arguments = Saniye > 0 ? "-s -t " + Saniye : "-a";
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    process.Start();
                #endif
            }
        }
        public static class Cihazlar
        {
            public static int Sıradakiİşlem = 0;
        }
    }

    public static class AğAraçları
    {
        static IPAddress Yerel_ip_ = null;
        public static IPAddress Yerel_ip
        {
            get
            {
                //aaa.bbb.ccc.ddd
                if (Yerel_ip_ == null)
                {
                    System.Net.Sockets.Socket soket = null;
                    try
                    {
                        soket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
                        soket.Connect("8.8.8.8", 65530);
                        System.Net.IPEndPoint uçnokta = soket.LocalEndPoint as System.Net.IPEndPoint;
                        Yerel_ip_ = uçnokta.Address;
                    }
                    catch (Exception) { }
                    finally { soket?.Dispose(); }
                }

                return Yerel_ip_;
            }
        }

        public static string Htmlden_Yazıya(string Girdi)
        {
            if (Girdi.BoşMu(true)) return null;

            const string Eleme_Boşluk = @"(>|$)(\W|\n|\r)+<";           //Kodlama içindeki bir veya daha çok boşluk, satır sonu
            const string Eleme_Kodlaama = @"<[^>]*(>|$)";               //'<' ve '>' arasındaki tüm kodlama karakterleri
            const string Eleme_SatırSonu = @"<(br|BR)\s{0,1}\/{0,1}>";  //<br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var Regex_SatırSonu = new System.Text.RegularExpressions.Regex(Eleme_SatırSonu, System.Text.RegularExpressions.RegexOptions.Multiline);
            var Regex_Kodlama = new System.Text.RegularExpressions.Regex(Eleme_Kodlaama, System.Text.RegularExpressions.RegexOptions.Multiline);
            var Regex_Boşluk = new System.Text.RegularExpressions.Regex(Eleme_Boşluk, System.Text.RegularExpressions.RegexOptions.Multiline);

            Girdi = System.Net.WebUtility.HtmlDecode(Girdi);
            Girdi = Regex_Boşluk.Replace(Girdi, "><");
            Girdi = Regex_SatırSonu.Replace(Girdi, Environment.NewLine);
            Girdi = Regex_Kodlama.Replace(Girdi, string.Empty);
            return Girdi;
        }

        public static string WebAdresinden_Yazıya(string Girdi)
        {
            if (Girdi.BoşMu(true)) return null;

            while (Girdi.Contains("%"))
            {
                int k = Girdi.IndexOf("%");                                             //%C3%96deme
                byte _0 = Convert.ToByte(Girdi.Substring(k + 1, 2), 16);

                int s = 0; //110aaaaa 1110aaaa 11110aaa -> 1 lerin sayısı : karakteri oluşturmak için gereken bayt
                while ((_0 & 0x80) > 0) { s++; _0 <<= 1; }
                if (s > 4) return null;
                else if (s == 0) s = 1;

                s *= 3 /*%ab*/;
                string kesilecek = Girdi.Substring(k, s);                               //%C3%96
                string dönüştürülecek = kesilecek.Replace("%", "");                     //C396
                dönüştürülecek = dönüştürülecek.BaytDizisine_HexYazıdan().Yazıya();     //Ö
                Girdi = Girdi.Replace(kesilecek, dönüştürülecek);                       //Ödeme
            }

            return Girdi;
        }
    }
}
