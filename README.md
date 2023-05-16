# Özdevinimci
Genel amaçlı, web api kullanan cihazlar için http sayfası üzerinden özdevinim ArgeMup@yandex.com

    Kök Klasör -> Özdevinimci.exe nin bulunduğu klasör
        C:\\Klasör\\Özdevinimci.exe -> C:\\Klasör

    Ayarlar Dosyası konumu -> Kök Klasör\Ayarlar.mup
        C:\\Klasör\\Ayarlar.mup

    Ayarlar Depo Dosyası İçeriği
        Genel
            Http Sunucu / <Erişim Noktası <= 0 ise kapalı>>
            Bilgisayarı Kapat / <Saat> / Dakika (Uyarı mesajı çıkar ve +5dk sonra kapanır)
        Cihazlar
            <Cihaz Adı> / <Türü - Sonoff_BasicR3_DIYMode> / <Seri No - Tanımlayıcı>
                <Komut Adı> / Aç / <Rölenin açık kalma süresi - 1 ile 43600 sn arasında>
                <Komut Adı> / Kapat