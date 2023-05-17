using ArgeMup.HazirKod;
using ArgeMup.HazirKod.DonanımHaberleşmesi;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.IO;

namespace Özdevinimci
{
    public static class HttpSunucu
    {
        static int ErişimNoktası = Ortak.Ayarlar.Oku_TamSayı("Genel/Http Sunucu");
        public static string Adresi = ErişimNoktası <= 0 ? null : "http://" + AğAraçları.Yerel_ip + ":" + ErişimNoktası;

        static TcpSunucu_ Sunucu = null;
        static IDonanımHaberlleşmesi Sunucu_DoHa = null;
        static byte[] Uygulama_ico;
        static string SayfaBaşlığı = "ArGeMuP " + Kendi.Adı + " " + Kendi.Sürüm;

        public static void Başlat()
        {
            if (ErişimNoktası <= 0) return;

            //Sayfadaki görsellerin oluşturulması
            using (MemoryStream ms = new MemoryStream())
            {
                Properties.Resources.mavi.Save(ms);
                Uygulama_ico = ms.ToArray();
            }

            Sunucu = new TcpSunucu_(ErişimNoktası, GeriBildirim_Islemi, SatırSatırGönderVeAl:false, SadeceYerel: false, Sessizlik_ZamanAşımı_msn:15000);
            Sunucu_DoHa = Sunucu;
        }
        public static void Durdur()
        {
            Sunucu?.Dispose();
            Sunucu = null;
            Sunucu_DoHa = null;
        }

        static void GeriBildirim_Islemi(string Kaynak, GeriBildirim_Türü_ Tür, object İçerik, object Hatırlatıcı)
        {
            if (Tür == GeriBildirim_Türü_.BilgiGeldi)
            {
                string[] istek = ((byte[])İçerik).Yazıya().Split('\n');
                string[] Sayfa_İçeriği = AğAraçları.WebAdresinden_Yazıya(istek[0].Trim(' ', '\r').Split(' ')[1]).Trim('/').Split('/');
                byte[] Gönderilecek_İçerik, Gönderilecek_Sayfa;

                if (Sayfa_İçeriği[0] == "DoEk")
                {
                    //Dosya ekleri
                    if (Sayfa_İçeriği.Length == 2 && Sayfa_İçeriği[1] == "Uygulama.ico")
                    {
                        Gönderilecek_İçerik = Uygulama_ico;
                        Gönderilecek_Sayfa = (
                                "HTTP/1.1 200 OK" + Environment.NewLine +
                                "Server: " + SayfaBaşlığı + Environment.NewLine +
                                "Content-Length: " + Gönderilecek_İçerik.Length + Environment.NewLine +
                                "Content-Type: image/x-icon" + Environment.NewLine +
                                "Connection: Closed" + Environment.NewLine + Environment.NewLine).BaytDizisine();
                    }
                    else goto Hata;
                }
                else
                {
                    if (Sayfa_İçeriği[0] == "Komut" && Sayfa_İçeriği.Length >= 3)
                    {
                        Sayfa_İçeriği[0] = Cihaz.Çalıştır(Sayfa_İçeriği[1], Sayfa_İçeriği[2]);
                        if (Sayfa_İçeriği[0].DoluMu()) Gönderilecek_İçerik = Sayfa_İçeriği[0].Replace(Environment.NewLine, "<br>").BaytDizisine(); //Hata
                        else
                        {
                            //başarılı
                            if (Sayfa_İçeriği.Length > 3) Gönderilecek_İçerik = _AnaSayfa_();
                            else Gönderilecek_İçerik = "Tamam".BaytDizisine();
                        }
                    }
                    else Gönderilecek_İçerik = _AnaSayfa_();
                    
                    Gönderilecek_Sayfa = (
                            "HTTP/1.1 200 OK" + Environment.NewLine +
                            "Server: " + SayfaBaşlığı + Environment.NewLine +
                            "Content-Length: " + Gönderilecek_İçerik.Length + Environment.NewLine +
                            "Content-Type: text/html;charset=utf-8" + Environment.NewLine +
                            "Connection: Closed" + Environment.NewLine + Environment.NewLine).BaytDizisine();
                }

                byte[] çıktı = new byte[Gönderilecek_Sayfa.Length + Gönderilecek_İçerik.Length];
                Array.Copy(Gönderilecek_Sayfa, 0, çıktı, 0, Gönderilecek_Sayfa.Length);
                Array.Copy(Gönderilecek_İçerik, 0, çıktı, Gönderilecek_Sayfa.Length, Gönderilecek_İçerik.Length);
                Sunucu_DoHa.Gönder(çıktı, Kaynak);
            }
            return;

            Hata:
            Sunucu.Durdur(Kaynak); 
        }

        static byte[] _AnaSayfa_()
        {
            string CihazVeKomutlar = null, Zamanlama = null;
            if (Cihazlar.Tümü != null)
            {
                //const Zamanlama = [ ["Kapı", new Date("2023.05.18 02:01:00")], ["Pencere", new Date("2023.05.17 02:02:00")] ];
                Zamanlama = "const Zamanlama=[";

                foreach (BirCihaz_ chz in Cihazlar.Tümü)
                {
                    /*
                    <fieldset> <legend id="Kapı">Kapı  &#127823 &#127822</legend>
                        <button onclick="Sorgula('Kapı', 'Aç', 10)">Aç</button>
                     </fieldset>
                     */

                    CihazVeKomutlar += @"<fieldset><legend id=""" + chz.Adı + @""">" + chz.Adı + @"</legend>";
                    foreach (BirCihaz_BirKomut_ kmt in chz.Komutlar)
                    {
                        CihazVeKomutlar += @"<button onclick=""Sorgula('" + chz.Adı + "','" + kmt.Adı + @"'," + (kmt.Türü == Cihaz.KomutTürü.Aç ? kmt.AçıkKalmaSüresi_Sn : 0) + @")"">" + kmt.Adı + @"</button>";
                    }
                    CihazVeKomutlar += @"</fieldset>";

                    //["Kapı", new Date("2023.05.18 02:01:00")],
                    DateTime z = chz.TekrarKurularakUzatılanKapanmaZamanı;
                    Zamanlama += @"[""" + chz.Adı + @""", new Date(""" + z.Year + "." + z.Month + "." + z.Day + " " + z.Hour + ":" + z.Minute + ":" + z.Second + @""")],";
                }

                Zamanlama += "];";
            }

            string SayfaCevabı = Properties.Resources.Cihazlarınız;
            SayfaCevabı = SayfaCevabı.Replace("?=? Uygulama Adi ?=?", SayfaBaşlığı);
            SayfaCevabı = SayfaCevabı.Replace("<!-- ?=? Cihaz ve Komutlar ?=? -->", CihazVeKomutlar);
            SayfaCevabı = SayfaCevabı.Replace("<!-- ?=? Zamanlama ?=? -->", Zamanlama);

            return SayfaCevabı.BaytDizisine();
        }

        #region Seri Nolu İstek
        //GET /DoEk/Uygulama.ico HTTP/1.1
        //GET /Cihaz/Komut HTTP/1.1
        //Host: localhost
        //Connection: keep-alive
        //sec-ch-ua: "Google Chrome";v="111", "Not(A:Brand";v="8", "Chromium";v="111"
        //sec-ch-ua-mobile: ?0
        //sec-ch-ua-platform: "Windows"
        //DNT: 1
        //Upgrade-Insecure-Requests: 1
        //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36
        //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
        //Sec-Fetch-Site: none
        //Sec-Fetch-Mode: navigate
        //Sec-Fetch-User: ?1
        //Sec-Fetch-Dest: document
        //Accept-Encoding: gzip, deflate, br
        //Accept-Language: tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7
        //
        //

        //HTTP/1.1 200 OK
        //Server: Argemup Reklamı
        //Content-Length: 6                bayt olarak
        //Content-Type: text/html/plain image/x-icon/jpeg/png/bmp/gif application/pdf/octet-stream
        //Connection: Closed
        //
        //içerik
        #endregion
    }
}
