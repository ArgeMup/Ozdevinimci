using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Collections.Generic;
using System.Linq;
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

            _Menü_ertele_0.Tag = 0;
            _Menü_ertele_5.Tag = 5;
            _Menü_ertele_30.Tag = 30;
            _Menü_ertele_60.Tag = 60;
            _Menü_ertele_120.Tag = 120;

            Cihazlar.TümCihazlarıListele();
            for (int c = Cihazlar.Tümü.Length - 1; c >= 0 ; c--)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem();
                tsmi.Text = Cihazlar.Tümü[c].Adı;
                SağTuşMenü.Items.Insert(0, tsmi);

                for (int k = 0; k < Cihazlar.Tümü[c].Komutlar.Length; k++)
                {
                    ToolStripMenuItem tsmi_k = new ToolStripMenuItem();
                    tsmi_k.Text = Cihazlar.Tümü[c].Komutlar[k].Adı;
                    tsmi_k.Click += _Menü_cihaz_komut_Click;
                    tsmi.DropDownItems.Add(tsmi_k);
                }
            }

            Ortak.Görevler.Kur("Bilgisayarı Kapat", DateTime.Now.AddSeconds(2), null, Görev_BilgisayarıKapat);
            Ortak.Görevler.Kur("Cihazlar", DateTime.Now.AddSeconds(1), null, Görev_Cihazlar);
            HttpSunucu.Başlat();
        }
        private void _Menü_cihaz_komut_Click(object sender, EventArgs e)
        {
            string cihaz = (sender as ToolStripMenuItem).OwnerItem.Text;
            string komut = (sender as ToolStripMenuItem).Text;

            string cevap = Cihaz.Çalıştır(cihaz, komut);
            if (cevap.DoluMu()) MessageBox.Show(cevap, Text);
        }

        int Görev_BilgisayarıKapat(string TakmaAdı, object Hatırlatıcı)
        {
            switch (Ortak.BilgisayarıKapat.Sıradakiİşlem)
            {
                case 0:
                    IDepo_Eleman ayrl = Ortak.Ayarlar["Genel/Bilgisayarı Kapat"];
                    if (ayrl.Oku(null).BoşMu()) return -1;

                    Ortak.BilgisayarıKapat.Zamanı = DateTime.Now;
                    Ortak.BilgisayarıKapat.Zamanı = new DateTime(
                        Ortak.BilgisayarıKapat.Zamanı.Year,
                        Ortak.BilgisayarıKapat.Zamanı.Month,
                        Ortak.BilgisayarıKapat.Zamanı.Day,
                        ayrl.Oku_TamSayı(null, 23, 0),
                        ayrl.Oku_TamSayı(null, 59, 1), 0);
                    Ortak.BilgisayarıKapat.Sıradakiİşlem++;
                    return 1000;

                case 1:
                    double kalan_msn = (Ortak.BilgisayarıKapat.Zamanı - DateTime.Now).TotalMilliseconds;
                    if (kalan_msn > 0) return (int)(kalan_msn / 2);

                    //süre doldu, 5 dk sonrasına kur
                    Ortak.BilgisayarıKapat.Zamanı = DateTime.Now.AddMinutes(5);
                    Ortak.BilgisayarıKapat.Kapat(0);
                    Ortak.BilgisayarıKapat.Kapat(300);

                    DialogResult Dr = MessageBox.Show("Bilgisayarınız " + Ortak.BilgisayarıKapat.Zamanı.Yazıya() + " zamanı geldiğinde kapatılacak." + Environment.NewLine +
                        "Seçenekleri değerlendiriniz." + Environment.NewLine + Environment.NewLine +
                        "Evet : 1 saat ertele" + Environment.NewLine +
                        "Hayır : 5 dakika içinde kapansın" + Environment.NewLine +
                        "İptal : Bilgisayarı kapatmayı iptal et", "Bilgisayar Kapatılacak", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (Dr == DialogResult.Yes)
                    {
                        Ortak.BilgisayarıKapat.Zamanı = DateTime.Now.AddHours(1);
                        Ortak.BilgisayarıKapat.Kapat(0);
                        Ortak.BilgisayarıKapat.Kapat(60 * 60); //1 saat
                        return 60000;
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
                    Cihazlar.TaramayıBaşlat();
                    Ortak.Cihazlar.Sıradakiİşlem++;
                    break;
                    
                case 1:
                    if (Cihazlar.Tarama_Bitti)
                    {
                        BirCihaz_ başlatılamayan = Cihazlar.Tümü.FirstOrDefault(x => x.Adresi == null);
                        if (başlatılamayan == null)
                        {
                            //tüm cihazlar tespit edildi
                            return -1;
                        }

                        //kayıtlı cihazlar bulunamadı veya eksik, yeniden dene
                        Ortak.Cihazlar.Sıradakiİşlem = 0;
                        return 60000;
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
            Cihazlar.Durdur();
            HttpSunucu.Bitir();

            ArgeMup.HazirKod.ArkaPlan.Ortak.Çalışsın = false;
        }

        private void _Menü_ertele_x_Click(object sender, EventArgs e)
        {
            Ortak.Görevler.Sil("Bilgisayarı Kapat");

            int za_dk = (int)(sender as ToolStripMenuItem).Tag;
            Ortak.BilgisayarıKapat.Kapat(0);

            if (za_dk > 0) Ortak.BilgisayarıKapat.Kapat(za_dk * 60);
        }
    }
}