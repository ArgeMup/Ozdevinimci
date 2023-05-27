using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using ArgeMup.HazirKod.Ekİşlemler;
using ArgeMup.HazirKod;
using System.Linq;
using ArgeMup.HazirKod.ArkaPlan;

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
        public static readonly int EnBüyükİletişiZamanAşımı = 5000;

        public static string Çalıştır(string Cihaz, string Komut)
        {
            if (Cihazlar.Tümü == null || Cihazlar.Tümü.Length < 1) return "Henüz hiç cihaz bulunamadı";

            BirCihaz_ chz = Cihazlar.Tümü.FirstOrDefault(x => x.Adı == Cihaz);
            if (chz == null) return "Cihaz bulunamadı " + Cihaz;

            return chz.Çalıştır(Komut);
        }
    }

    public static class Cihazlar
    {
        public static BirCihaz_[] Tümü = null;
        public static void TümCihazlarıListele()
        {
            Tümü = new BirCihaz_[Ortak.Ayarlar["Cihazlar"].Elemanları.Length];
            for (int c = 0; c < Tümü.Length; c++)
            {
                IDepo_Eleman chz = Ortak.Ayarlar["Cihazlar"].Elemanları[c];
                Tümü[c] = new BirCihaz_()
                {
                    Adı = chz.Adı,
                    Türü = Cihaz.Türü.Sonoff_BasicR3_DIYMode,
                    KapatıldıktanSonraTekrarAçılabilmesiİçinGerekenSüre_sn = chz.Oku_TamSayı(null, 0, 2),
                    Komutlar = new BirCihaz_BirKomut_[chz.Elemanları.Length]
                };
             
                for (int k = 0; k < Tümü[c].Komutlar.Length; k++)
                {
                    IDepo_Eleman kmt = chz.Elemanları[k];
                    Tümü[c].Komutlar[k] = new BirCihaz_BirKomut_()
                    {
                        Adı = kmt.Adı,
                        Türü = kmt[0] == "Aç" ? Cihaz.KomutTürü.Aç : Cihaz.KomutTürü.Kapat,
                        AçıkKalmaSüresi_Sn = kmt.Oku_TamSayı(null, 15, 1)
                    };
                }
            }
        }
        public static void Durdur()
        {
            if (Tümü == null) return;

            foreach (BirCihaz_ chz in Tümü)
            {
                chz.Durdur();
            }
        }

        public static bool Tarama_Bitti;
        public static void TaramayıBaşlat()
        {
            Tarama_Bitti = false;
            Günlük.Ekle("Tarama başlatılıyor");

            Task.Run(async () =>
            {
                IPAddress YerelAdres = AğAraçları.Yerel_ip;
                #if DEBUG
                    YerelAdres = new IPAddress(new byte[] { 192, 168, 137, 0 });
                #endif

                if (YerelAdres != null)
                {
                    byte[] YerelAdres_açıkHali = YerelAdres.GetAddressBytes();

                    Task<Sorgula_Detaylar_>[] Sorgu_Cevapları = new Task<Sorgula_Detaylar_>[256];
                    for (int i = 0; i < Sorgu_Cevapları.Length; i++)
                    {
                        YerelAdres_açıkHali[3] = (byte)i;

                        //ilerde tüm cihaz tipleri için ayrı ayrı denenecek
                        Sorgu_Cevapları[i] = DetaylarınıOku(new IPAddress(YerelAdres_açıkHali), Cihaz.Sonoff.Bilgi.Sayfa, Cihaz.Sonoff.Bilgi.Sorgu);
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

                        BirCihaz_ Çalışanlar_içindeki_cihaz = Tümü.FirstOrDefault(x => x.Adı == chz_ayrlr.Adı);
                        if (Çalışanlar_içindeki_cihaz == null)
                        {
                            Günlük.Ekle(chz_ayrlr.Adı + " Beklenmeyen durum, ayarlar içindeki cihaz çalışanlar içine kaydedilmemiş");
                            continue;
                        }

                        if (Çalışanlar_içindeki_cihaz.Adresi != null) continue; //zaten alındı

                        string cvp = biri.Result.Cihaz.İlkAçılışİşlemlerl();
                        if (cvp.DoluMu())
                        {
                            Günlük.Ekle(chz_ayrlr.Adı + " İlkAçılışİşlemlerl hatalı " + cvp);
                            continue;
                        }

                        Çalışanlar_içindeki_cihaz.Detaylar = biri.Result.Cihaz.Detaylar;
                        Çalışanlar_içindeki_cihaz.Adresi = biri.Result.Cihaz.Adresi;
                        Günlük.Ekle(Çalışanlar_içindeki_cihaz.Adı + " cihazı faal");
                    }
                }
            }).ContinueWith((t) =>
            {
                Tarama_Bitti = true;
            });
        }

        public class Sorgula_Detaylar_
        {
            public string Sayfa;
            public BirCihaz_ Cihaz;
            public string Hata;
        }
        public static async Task<Sorgula_Detaylar_> DetaylarınıOku(IPAddress Adres, string Sayfa, string Sorgu)
        {
            await Task.Delay(1); //eşzamanlı çalışma için

            Sorgula_Detaylar_ Detaylar = new Sorgula_Detaylar_() 
            { 
                Sayfa = Sayfa,
                Cihaz = new BirCihaz_() { Adresi = Adres }
            };

            Detaylar.Hata = Detaylar.Cihaz.Bilgi(0);
            if (Detaylar.Hata.BoşMu())
            {
                Günlük.Ekle("Bulunan cihazın tanımlayıcısı " + Detaylar.Cihaz.Detaylar.Tanımlayıcısı + 
                    ", sürümü " + Detaylar.Cihaz.Detaylar.Sürümü + 
                    ", rssi " + Detaylar.Cihaz.Detaylar.Sinyal_Seviyesi + 
                    ", durumu " + Detaylar.Cihaz.Detaylar.Durumu +
                    ", adresi " + Adres.ToString() +
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
        
        public DateTime TekrarAçılabileceğiZaman;
        public int KapatıldıktanSonraTekrarAçılabilmesiİçinGerekenSüre_sn;

        public BirCihaz_BirKomut_[] Komutlar;

        int İstekNo;
        public string Çalıştır(string Komut)
        {
            int Kendi_İstekNo = ++İstekNo;
            
            BirCihaz_BirKomut_ kmt = Komutlar.FirstOrDefault(x => x.Adı == Komut);
            if (kmt == null) return "Komut bulunamadı " + Komut;

            string Cevap = Cevap = Bilgi(Kendi_İstekNo);
            if (Cevap.DoluMu())
            {
                Günlük.Ekle("Cevap alınamadığından silindi " + Adı + " " + Cevap);
                Adresi = null;

                Hatırlatıcı_.Durum_ drm = Ortak.Görevler.Bul("Cihazlar");
                if (drm == null || !drm.TetiklenmesiBekleniyor)
                {
                    Ortak.Cihazlar.Sıradakiİşlem = 0;
                    Ortak.Görevler.Sil("Cihazlar");
                    Ortak.Görevler.Kur("Cihazlar", DateTime.Now.AddSeconds(1), null, AnaEkran.Görev_Cihazlar);
                }
                return Cevap;
            }

            if (kmt.Türü == Cihaz.KomutTürü.Aç && Detaylar.Durumu == Cihaz.Durumu.Açık) return null;
            else if (kmt.Türü == Cihaz.KomutTürü.Kapat)
            {
                TekrarAçılabileceğiZaman = DateTime.MinValue;

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
            TimeSpan ts = TekrarAçılabileceğiZaman - DateTime.Now;
            if (ts.TotalMilliseconds > 0) return "Lütfen bekleyiniz " + Ortak.ZamanAşımıAnı_Yazıya(ts);

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
            TekrarAçılabileceğiZaman = TekrarKurularakUzatılanKapanmaZamanı.AddSeconds(KapatıldıktanSonraTekrarAçılabilmesiİçinGerekenSüre_sn);
            Ortak.Görevler.Kur("Cihaz " + Adı, DateTime.Now.AddMilliseconds(5), null, Görev_Açık, Kendi_İstekNo);

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

            Cevap = Bilgi(İstekNo);
            if (Cevap.DoluMu()) return Cevap;

            if (!Detaylar.KapalıOlarakAçılsın)
            {
                Cevap = Sorgula(Cihaz.Sonoff.KapalıOlarakAçılma.Sayfa, Cihaz.Sonoff.KapalıOlarakAçılma.Sorgu);
                if (Cevap.BoşMu()) return "Bağlantı kurulamadı - KapalıOlarakAçılma";

                Cihaz.Sonoff.KapalıOlarakAçılma Detaylar_ = new Cihaz.Sonoff.KapalıOlarakAçılma(Cevap);
                if (!Detaylar_.Ayıklandı) return "Sorgu cevabı ayıklanamadı - KapalıOlarakAçılma - " + Cevap;
            }
            
            return null;
        }
        public void Durdur()
        {
            AçKapat(İstekNo, false);
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
            if (Adresi == null) return null;
            
            Exception son_hata = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (TcpClient İstemci = new TcpClient())
                    {
                        İstemci.ReceiveTimeout = Cihaz.EnBüyükİletişiZamanAşımı;
                        İstemci.SendTimeout = Cihaz.EnBüyükİletişiZamanAşımı / 2;
                        if (!İstemci.ConnectAsync(Adresi, Cihaz.Sonoff.ErişimNoktası).Wait(Cihaz.EnBüyükİletişiZamanAşımı))
                        {
                            İstemci.Close();
                            continue;
                        }

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
                catch (Exception ex) { son_hata = ex; }

                Task.Delay(350).Wait();
            }

            son_hata?.Günlük();
            return null;
        }
        int Görev_Açık(string TakmaAdı, object Hatırlatıcı) //Kendi_İstekNo
        {
            if ((int)Hatırlatıcı != İstekNo)
            {
                Console.WriteLine("yeni komut bu görevi iptal etti");
                return -1; //yeni komut bu görevi iptal etti
            }

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
