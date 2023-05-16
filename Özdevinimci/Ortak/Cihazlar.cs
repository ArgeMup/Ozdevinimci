﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ArgeMup.HazirKod.Ekİşlemler;
using ArgeMup.HazirKod;
using System.Linq;
using static ArgeMup.HazirKod.Depo_Xml;

namespace Özdevinimci
{
    public partial class Cihaz
    {
        public enum Türü
        {
            Sonoff_BasicR3_DIYMode /*V3.6.0*/
        };
        public enum Durumu
        {
            Belirsiz,
            Kapalı,
            Açık
        };
        public enum KomutTürü
        {
            Aç,
            Kapat
        };
        public static readonly int EnBüyükİletişiZamanAşımı = 3500;

        public static string Çalıştır(string Cihaz, string Komut)
        {
            if (Cihazlar.Tümü_Çalışma == null || Cihazlar.Tümü_Çalışma.Length < 1) return "Henüz hiç cihaz bulunamadı";

            BirCihaz_ chz = Cihazlar.Tümü_Çalışma.FirstOrDefault(x => x.Adı == Cihaz);
            if (chz == null) return "Cihaz bulunamadı " + Cihaz;

            return chz.Çalıştır(Komut);
        }
    }

    public static class Cihazlar
    {
        public static BirCihaz_[] Tümü_Tarama = null, Tümü_Çalışma = null;

        public static void Listele_Adresleri()
        {
            List<BirCihaz_> liste = new List<BirCihaz_>();

            Task.Run(async () =>
            {
                IPAddress YerelAdres = AğAraçları.Yerel_ip;
                #if DEBUG
                    YerelAdres = new IPAddress(new byte[] { 192, 168, 137, 0 });
                #endif

                if (YerelAdres != null)
                {
                    byte[] YerelAdres_açıkHali = YerelAdres.GetAddressBytes();

                    Task<PingReply>[] Yankı_Cevapları = new Task<PingReply>[256];
                    for (int i = 0; i < Yankı_Cevapları.Length; i++)
                    {
                        YerelAdres_açıkHali[3] = (byte)i;
                        Yankı_Cevapları[i] = new Ping().SendPingAsync(new IPAddress(YerelAdres_açıkHali), 1500);
                    }
                    await Task.WhenAll(Yankı_Cevapları);

                    List<Task<Sorgula_Detaylar_>> Sorgu_Cevapları = new List<Task<Sorgula_Detaylar_>>();
                    for (int i = 0; i < Yankı_Cevapları.Length; i++)
                    {
                        if (Yankı_Cevapları[i].Result.Status != IPStatus.Success) continue;

                        //ilerde tüm cihaz tipleri için ayrı ayrı denenecek
                        Sorgu_Cevapları.Add(DetaylarınıOku(Yankı_Cevapları[i].Result.Address, Cihaz.Sonoff.ErişimNoktası, Cihaz.Sonoff.Bilgi.Sayfa, Cihaz.Sonoff.Bilgi.Sorgu));
                    }
                    await Task.WhenAll(Sorgu_Cevapları);

                    foreach (var biri in Sorgu_Cevapları)
                    {
                        if (biri.Result.Hata.DoluMu()) continue;

                        IDepo_Eleman chz_ayrlr = Ortak.Ayarlar["Cihazlar"].Elemanları.FirstOrDefault(x => x[0] == Cihaz.Türü.Sonoff_BasicR3_DIYMode.ToString() && x[1] == biri.Result.Cihaz.Detaylar.Tanımlayıcısı);
                        if (chz_ayrlr == null)
                        {
                            Günlük.Ekle("Tanımlı Cihazlar içinde mevcut değil " + biri.Result.Cihaz.Adresi.ToString() + " " + biri.Result.Cihaz.Detaylar.Tanımlayıcısı);
                            continue;
                        }

                        string cvp = biri.Result.Cihaz.İlkAçılışİşlemlerl();
                        if (cvp.DoluMu()) Günlük.Ekle(chz_ayrlr.Adı + " İlkAçılışİşlemlerl hatalı " + cvp);

                        if (chz_ayrlr.Elemanları.Length < 1)
                        {
                            Günlük.Ekle(chz_ayrlr.Adı + " cihazı için hiç komut tanımlanmadığından atlandı");
                            continue;
                        }

                        biri.Result.Cihaz.Türü = Cihaz.Türü.Sonoff_BasicR3_DIYMode;
                        biri.Result.Cihaz.Adı = chz_ayrlr.Adı;
                        biri.Result.Cihaz.Komutlar = new BirCihaz_BirKomut_[chz_ayrlr.Elemanları.Length];
                     
                        for (int i = 0; i < biri.Result.Cihaz.Komutlar.Length; i++)
                        {
                            IDepo_Eleman kmt_ayrlr = chz_ayrlr.Elemanları[i];
                            biri.Result.Cihaz.Komutlar[i] = new BirCihaz_BirKomut_()
                            {
                                Adı = kmt_ayrlr.Adı,
                                Türü = kmt_ayrlr[0] == "Aç" ? Cihaz.KomutTürü.Aç : Cihaz.KomutTürü.Kapat,
                                AçıkKalmaSüresi_Sn = kmt_ayrlr.Oku_TamSayı(null, 5, 1)
                            };
                        }

                        liste.Add(biri.Result.Cihaz);
                    }
                }
            }).ContinueWith((t) =>
            {
                Tümü_Tarama = liste.ToArray();
            });
        }

        public class Sorgula_Detaylar_
        {
            public string Sayfa;
            public BirCihaz_ Cihaz;
            public string Hata;
        }
        public static async Task<Sorgula_Detaylar_> DetaylarınıOku(IPAddress Adres, int ErişimNoktası, string Sayfa, string Sorgu)
        {
            await Task.Delay(1); //eşzamanlı çalışma için

            Sorgula_Detaylar_ Detaylar = new Sorgula_Detaylar_() { Sayfa = Sayfa };
            Detaylar.Cihaz = new BirCihaz_() { Adresi = Adres };

            Detaylar.Hata = Detaylar.Cihaz.Bilgi(0);
            if (Detaylar.Hata.BoşMu())
            {
                Günlük.Ekle("Bulunan cihazın tanımlayıcısı " + Detaylar.Cihaz.Detaylar.Tanımlayıcısı + 
                    ", sürümü " + Detaylar.Cihaz.Detaylar.Sürümü + 
                    ", rssi " + Detaylar.Cihaz.Detaylar.Sinyal_Seviyesi + 
                    ", durumu " + Detaylar.Cihaz.Detaylar.Durumu +
                    ", AçıkKalmaSüresi_sn " + Detaylar.Cihaz.Detaylar.AçıkKalmaSüresi_sn +
                    ", KapalıOlarakAçılsın " + Detaylar.Cihaz.Detaylar.KapalıOlarakAçılsın +
                    ", KendiliğindenKapanır " + Detaylar.Cihaz.Detaylar.KendiliğindenKapanır);
            }
                
            return Detaylar;
        }
    }

    public class BirCihaz_
    {
        public string Adı;
        public Cihaz.Türü Türü;
        public IPAddress Adresi = null;

        public Cihaz.Sonoff.Bilgi Detaylar;
        public DateTime TekrarKurularakUzatılanKapanmaZamanı;

        public BirCihaz_BirKomut_[] Komutlar;

        int İstekNo;
        public string Çalıştır(string Komut)
        {
            int Kendi_İstekNo = ++İstekNo;
            
            BirCihaz_BirKomut_ kmt = Komutlar.FirstOrDefault(x => x.Adı == Komut);
            if (kmt == null) return "Komut bulunamadı " + Komut;

            string Cevap = Cevap = Bilgi(Kendi_İstekNo);
            if (Cevap.DoluMu()) return Cevap;

            if (kmt.Türü == Cihaz.KomutTürü.Aç && Detaylar.Durumu == Cihaz.Durumu.Açık) return null;
            else if (kmt.Türü == Cihaz.KomutTürü.Kapat)
            {
                if (Detaylar.Durumu == Cihaz.Durumu.Kapalı) return null;
                else
                {
                    Cevap = AçKapat(Kendi_İstekNo, false);
                    if (Cevap.DoluMu()) return Cevap;

                    Detaylar.Durumu = Cihaz.Durumu.Kapalı;
                    return null;
                }
            }

            //Açmak için gerekli koşulları yerine getir
            string sorgu_kendiliğinden_kapanma = Cihaz.Sonoff.KendiliğindenKapanma.Sorgu(kmt.AçıkKalmaSüresi_Sn, out int KontrolEdilmişDeğer_sn);
            if (!Detaylar.KendiliğindenKapanır || Detaylar.AçıkKalmaSüresi_sn != KontrolEdilmişDeğer_sn)
            {
                //kendiliğinden kapanma durumunu kur
                Cevap = Sorgula(Cihaz.Sonoff.KendiliğindenKapanma.Sayfa, sorgu_kendiliğinden_kapanma);
                if (Kendi_İstekNo != İstekNo) return "İptal edildi";
                if (Cevap.BoşMu()) return "Bağlantı kurulamadı - KendiliğindenKapanma";

                Cihaz.Sonoff.KendiliğindenKapanma ken_kapanma = new Cihaz.Sonoff.KendiliğindenKapanma(Cevap);
                if (!ken_kapanma.Ayıklandı) return "Sorgu cevabı ayıklanamadı - KendiliğindenKapanma - " + Cevap;

                Detaylar.KendiliğindenKapanır = true;
                Detaylar.AçıkKalmaSüresi_sn = KontrolEdilmişDeğer_sn;
            }

            //Aç
            TekrarKurularakUzatılanKapanmaZamanı = DateTime.Now.AddSeconds(kmt.AçıkKalmaSüresi_Sn);
            Ortak.Görevler.Kur("Cihaz " + Adı, DateTime.Now, null, Görev_Açık, Kendi_İstekNo);

            return null;
        }
        public string Bilgi(int Kendi_İstekNo)
        {
            string Cevap = Sorgula(Cihaz.Sonoff.Bilgi.Sayfa, Cihaz.Sonoff.Bilgi.Sorgu);
            if (Kendi_İstekNo != İstekNo) return "İptal edildi";
            if (Cevap.BoşMu()) return "Bağlantı kurulamadı - Bilgi";

            Detaylar = new Cihaz.Sonoff.Bilgi(Cevap);
            if (!Detaylar.Ayıklandı) return "Sorgu cevabı ayıklanamadı - Bilgi - " + Cevap;

            return null;
        }
        public string İlkAçılışİşlemlerl()
        {
            string Cevap = AçKapat(İstekNo, false);
            if (Cevap.DoluMu()) return Cevap;

            Cevap = Sorgula(Cihaz.Sonoff.KapalıOlarakAçılma.Sayfa, Cihaz.Sonoff.KapalıOlarakAçılma.Sorgu);
            if (Cevap.BoşMu()) return "Bağlantı kurulamadı - KapalıOlarakAçılma";

            Cihaz.Sonoff.KapalıOlarakAçılma Detaylar = new Cihaz.Sonoff.KapalıOlarakAçılma(Cevap);
            if (!Detaylar.Ayıklandı) return "Sorgu cevabı ayıklanamadı - KapalıOlarakAçılma - " + Cevap;

            return null;
        }
        string AçKapat(int Kendi_İstekNo, bool Aç)
        {
            string Cevap = Sorgula(Cihaz.Sonoff.AçKapat.Sayfa, Aç ? Cihaz.Sonoff.AçKapat.Sorgu_Aç : Cihaz.Sonoff.AçKapat.Sorgu_Kapat);
            if (Kendi_İstekNo != İstekNo) return "İptal edildi";
            if (Cevap.BoşMu()) return "Bağlantı kurulamadı - AçKapat";

            Cihaz.Sonoff.AçKapat AçKapat = new Cihaz.Sonoff.AçKapat(Cevap);
            if (!AçKapat.Ayıklandı) return "Sorgu cevabı ayıklanamadı - AçKapat - " + Cevap;

            Detaylar.Durumu = Aç ? Cihaz.Durumu.Açık : Cihaz.Durumu.Kapalı;
            return null;
        }
        string Sorgula(string Sayfa, string Sorgu)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (TcpClient İstemci = new TcpClient())
                    {
                        İstemci.ReceiveTimeout = Cihaz.EnBüyükİletişiZamanAşımı;
                        İstemci.SendTimeout = Cihaz.EnBüyükİletişiZamanAşımı / 2;
                        İstemci.Connect(Adresi, Cihaz.Sonoff.ErişimNoktası);

                        /*
                        POST SAYFA HTTP/1.1
                        Host: ADRES:ERİŞİMNOKTASI
                        Content-Type: application/json
                        Content-Length: ADET
                        Cache-Control: no-cache
                        */
                        string http_sorgu = "POST " + Sayfa + " HTTP/1.1" + Environment.NewLine +
                            "Host: " + Adresi.ToString() + ":" + Cihaz.Sonoff.ErişimNoktası + Environment.NewLine +
                            "Content-Type: application/json" + Environment.NewLine +
                            "Content-Length: " + Sorgu.BaytDizisine().Length + Environment.NewLine +
                            "Cache-Control: no-cache" + Environment.NewLine +
                            Environment.NewLine +
                            Sorgu;
                        byte[] http_sorgu_dizi = http_sorgu.BaytDizisine();

                        using (NetworkStream Aracı = İstemci.GetStream())
                        {
                            Aracı.Write(http_sorgu_dizi, 0, http_sorgu_dizi.Length);
                            Aracı.Flush();

                            byte[] ilk_alınan = new byte[1];
                            int adet_okunan = Aracı.Read(ilk_alınan, 0, 1);
                            if (adet_okunan > 0)
                            {
                                int adet_bekleyen = İstemci.Available;

                                byte[] tampon_GelenBilgi = new byte[adet_bekleyen + 1];
                                tampon_GelenBilgi[0] = ilk_alınan[0];

                                adet_okunan = Aracı.Read(tampon_GelenBilgi, 1, adet_bekleyen);
                                if (adet_okunan != adet_bekleyen) Array.Resize(ref tampon_GelenBilgi, adet_okunan + 1);

                                string Cevap = tampon_GelenBilgi.Yazıya();
                                if (Cevap.DoluMu())
                                {
                                    List<string> l = new List<string>();
                                    l.AddRange(Cevap.Trim().Split('\n'));

                                    string[] cevap_dizi = l[0].Split(' ');
                                    if (cevap_dizi[1] == "200")
                                    {
                                        while (l[0] != "\r") l.RemoveAt(0);
                                        l.RemoveAt(0);

                                        Cevap = null;
                                        foreach (var s in l) Cevap += s.TrimEnd('\r') + Environment.NewLine;

                                        return Cevap;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { ex.Günlük(); }

                Task.Delay(150).Wait();
            }
            
            return null;
        }
        int Görev_Açık(string TakmaAdı, object Hatırlatıcı) //Kendi_İstekNo
        {
            if ((int)Hatırlatıcı != İstekNo) return -1; //yeni komut bu görevi iptal etti

            string Cevap;
            TimeSpan ts = TekrarKurularakUzatılanKapanmaZamanı - DateTime.Now;
            if (ts.TotalMilliseconds <= 0)
            {
                //tamamlandı
                Cevap = AçKapat((int)Hatırlatıcı, false);
                if (Cevap.BoşMu()) Detaylar.Durumu = Cihaz.Durumu.Kapalı;
                return -1;
            }
            
            //devam eden
            Cevap = AçKapat((int)Hatırlatıcı, true);
            if (Cevap.DoluMu())
            {
                Günlük.Ekle("açma işlemi başarısız " + (int)Hatırlatıcı + " " + Cevap);
                return -1;
            }
            Detaylar.Durumu = Cihaz.Durumu.Açık;

            if (ts.TotalMinutes >= 45)
            {
                //45 dk -> msn
                return 45 * 60 * 1000;
            }
            else
            {
                //gereken + 1/2 sn
                return (int)ts.TotalMilliseconds + 500;
            }
        }
    }
    
    public class BirCihaz_BirKomut_
    {
        public string Adı;
        public Cihaz.KomutTürü Türü;

        public int AçıkKalmaSüresi_Sn; //En fazla 3600000 msn - 3600 sn - 3600 den büyük ise 45 dk da 1 kez cihazı tekrar kur
    }
}
