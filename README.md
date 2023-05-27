# Özdevinimci
Genel amaçlı, web api kullanan cihazlar için http sayfası üzerinden özdevinim ArgeMup@yandex.com

    Kök Klasör -> Özdevinimci.exe nin bulunduğu klasör
        C:\\Klasör\\Özdevinimci.exe -> C:\\Klasör

    Ayarlar Dosyası konumu -> Kök Klasör\Ayarlar.mup
        C:\\Klasör\\Ayarlar.mup

    Ayarlar Depo Dosyası İçeriği
        Genel
            Http Sunucu / <Erişim Noktası <= 0 ise kapalı>
            Bilgisayarı Kapat / <Saat> / Dakika (Uyarı mesajı çıkar ve +5dk sonra kapanır)
        Cihazlar
            <Cihaz Adı> / Sonoff_BasicR3_DIYMode / <Seri No - Tanımlayıcı> / <Kapatıldıktan sonra tekrar açılabilmesi için gereken süre - sn> 
                <Komut Adı> / Aç / <Rölenin açık kalma süresi - 1 ile 43600 sn arasında>
                <Komut Adı> / Kapat

    Örnek Ayarlar Depo Dosyası İçeriği
        Genel
        >Http Sunucu>9999
        >Bilgisayarı Kapat>22>30
        Cihazlar
        >Kapı>Sonoff_BasicR3_DIYMode>abcdef0123>1
        >>Aç>Aç>5
        >>Kapat>Kapat

    Tüm cihazları kontrol edebilmek için anasayfa
        <Bilgisayar ip>:<Erişim Noktası>
        http://192.168.2.1:9999

    Bir cihazın bir komutunu doğrudan çağırmak için
        <Bilgisayar ip>:<Erişim Noktası>/Komut/<Cihaz Adı>/<Komut Adı>
        http://192.168.2.1:9999/Komut/Kapı/Aç

    Bir cihazın bir komutunu doğrudan çağırıp anasayfayı açmak için
        <Bilgisayar ip>:<Erişim Noktası>/Komut/<Cihaz Adı>/<Komut Adı>/Anasayfa
        http://192.168.2.1:9999/Komut/Kapı/Aç/Anasayfa