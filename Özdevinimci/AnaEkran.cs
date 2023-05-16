using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Özdevinimci
{
    public partial class AnaEkran : Form
    {
        public AnaEkran()
        {
            Günlük.Başlat(TümDosyaların_KapladığıAlan_bayt: 5 * 1024 * 2014/*5MB*/);

            InitializeComponent();

            Hide();
            Gösterge.Icon = Properties.Resources.mavi;
            Gösterge.Text = Kendi.Adı + " V" + Kendi.Sürümü_Dosya;

            Ortak.Görevler.Kur("Bilgisayarı Kapat", DateTime.Now, null, Görev_BilgisayarıKapat);
            Ortak.Görevler.Kur("Cihazlar", DateTime.Now, null, Görev_Cihazlar);
            HttpSunucu.Başlat();
        }

        int Görev_BilgisayarıKapat(string TakmaAdı, object Hatırlatıcı)
        {
            switch (Ortak.BilgisayarıKapat.Sıradakiİşlem)
            {
                case 0:
                    if (Ortak.Ayarlar.Oku("Bilgisayarı Kapat").BoşMu()) return -1;

                    Ortak.BilgisayarıKapat.Zamanı = DateTime.Now;
                    Ortak.BilgisayarıKapat.Zamanı = new DateTime(
                        Ortak.BilgisayarıKapat.Zamanı.Year,
                        Ortak.BilgisayarıKapat.Zamanı.Month,
                        Ortak.BilgisayarıKapat.Zamanı.Day,
                        Ortak.Ayarlar.Oku_TamSayı("Bilgisayarı Kapat", 23, 0),
                        Ortak.Ayarlar.Oku_TamSayı("Bilgisayarı Kapat", 59, 1), 0);
                    Ortak.BilgisayarıKapat.Sıradakiİşlem++;
                    return 1000;

                case 1:
                    double kalan_sn = (Ortak.BilgisayarıKapat.Zamanı - DateTime.Now).TotalSeconds;
                    if (kalan_sn > 0) return (int)(kalan_sn / 2);

                    //süre doldu, 5 dk sonrasına kur
                    Ortak.BilgisayarıKapat.Kapat(300);

                    DialogResult Dr = MessageBox.Show("Bilgisayarınız " + Ortak.BilgisayarıKapat.Zamanı.Yazıya() + " zamanı geldiğinde kapatılacak." + Environment.NewLine +
                        "Seçenekleri değerlendiriniz." + Environment.NewLine + Environment.NewLine +
                        "Evet : 1 saat ertele" + Environment.NewLine +
                        "Hayır : 5 dakika içinde kapansın" + Environment.NewLine +
                        "İptal : Bilgisayarı kapatmayı iptal et", "Bilgisayar Kapatılacak", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (Dr == DialogResult.Yes)
                    {
                        Ortak.BilgisayarıKapat.Kapat(60 * 60); //1 saat
                        return 60;
                    }
                    else if (Dr == DialogResult.Cancel)
                    {
                        Ortak.BilgisayarıKapat.Kapat(0);
                    }
                    return -1;

                default:
                    Günlük.Ekle("Hatalı işlem sırası " + TakmaAdı + " " + Ortak.BilgisayarıKapat.Sıradakiİşlem);
                    return -1;
            }
        }
        int Görev_Cihazlar(string TakmaAdı, object Hatırlatıcı)
        {
            switch (Ortak.Cihazlar.Sıradakiİşlem)
            {
                case 0:
                    Cihazlar.Tümü_Tarama = null;
                    Cihazlar.Listele_Adresleri();
                    Ortak.Cihazlar.Sıradakiİşlem++;
                    break;

                case 1:
                    if (Cihazlar.Tümü_Tarama != null)
                    {
                        if (Cihazlar.Tümü_Tarama.Length < 1)
                        {
                            //hiç cihaz bulunamadı
                            if (Ortak.Ayarlar["Cihazlar"].Elemanları.Length > 0)
                            {
                                //kayıtlı cihazlar bulunamadı, yeniden dene
                                Ortak.Cihazlar.Sıradakiİşlem = 0;
                                return 60000;
                            }
                            else
                            {
                                //hiç kayıtlı cihaz yok, mekanizmayı kapat
                                return -1;
                            }
                        }
                        else
                        {
                            List<BirCihaz_> bulunan_chz_ler = new List<BirCihaz_>();
                            foreach (BirCihaz_ chz in Cihazlar.Tümü_Tarama)
                            {
                                IDepo_Eleman chz_ayrlr = Ortak.Ayarlar["Cihazlar/" + chz.Adı];
                                if (chz_ayrlr != null)
                                {
                                    //bulunan cihazı ayarlar listesinden silerek tekrar bulunmasını engelle
                                    bulunan_chz_ler.Add(chz);
                                    chz_ayrlr.Sil(null);
                                }
                            }

                            if (bulunan_chz_ler.Count > 0)
                            {
                                if (Cihazlar.Tümü_Çalışma != null)
                                {
                                    bulunan_chz_ler.AddRange(Cihazlar.Tümü_Çalışma);
                                }
                                Cihazlar.Tümü_Çalışma = bulunan_chz_ler.ToArray();
                            }

                            Ortak.Ayarlar.YazıyaDönüştür(); //eklenen cihazların silinmesi için
                            if (Ortak.Ayarlar["Cihazlar"].Elemanları.Length < 1)
                            {
                                //tüm cihazlar faal, bu görev gereksiz
                                return -1;
                            }
                            else
                            {
                                //eksik kalan cihazlar için taramaya devam
                                Ortak.Cihazlar.Sıradakiİşlem = 0;
                                return 60000;
                            }
                        }
                    }
                    break;

                default:
                    Günlük.Ekle("Hatalı işlem sırası " + TakmaAdı + " " + Ortak.Cihazlar.Sıradakiİşlem);
                    return -1;
            }

            return 1000;
        }

        private void tarayıcıdaAçToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string adr = HttpSunucu.Adresi;
            if (adr.BoşMu())
            {
                MessageBox.Show("Sunucu özelliği kullanılmıyor", Text);
                return;
            }

            System.Diagnostics.Process.Start(adr);
        }
        private void çıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }      
        private void AnaEkran_FormClosed(object sender, FormClosedEventArgs e)
        {
            Günlük.Ekle(e.CloseReason.ToString());
            ArgeMup.HazirKod.ArkaPlan.Ortak.Çalışsın = false;
        }
    }
}
