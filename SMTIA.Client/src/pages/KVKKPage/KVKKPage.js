import React from 'react';
import './KVKKPage.css';
import logo from '../../assets/logos/logo.png';

const KVKKPage = ({ onBack }) => {
  const handleBackClick = () => {
    onBack();
  };

  return (
    <div className="kvkk-page-container">
      <div className="kvkk-page-header">
        <div className="kvkk-logo-section">
          <img src={logo} alt="Logo" className="kvkk-logo" />
          <span className="kvkk-group-text">
            <span className="kvkk-by-text">by</span> <span className="kvkk-group-name">Group X</span>
          </span>
        </div>
        <h1 className="kvkk-page-title">KVKK</h1>
        <button className="kvkk-back-button" onClick={handleBackClick}>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M19 12H5"/>
            <path d="M12 19l-7-7 7-7"/>
          </svg>
          <span>Geri</span>
        </button>
      </div>

      <div className="kvkk-page-content">
        <div className="kvkk-content-wrapper">
          <h2 className="kvkk-main-title">KİŞİSEL VERİLERİN KORUNMASI HAKKINDA AYDINLATMA METNİ (KVKK)</h2>
          
          <div className="kvkk-intro">
            <p>Group X - İlaç Takip Uygulaması olarak, 6698 sayılı Kişisel Verilerin Korunması Kanunu ("KVKK") kapsamında kişisel verilerinizin güvenliğine ve gizliliğine azami özeni göstermekteyiz.</p>
            <p>Bu metin, hangi kişisel verilerin toplandığı, bunların hangi amaçlarla işlendiği, saklama süresi ile haklarınıza ilişkin bilgileri içermektedir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">1. Veri Sorumlusu</h3>
            <p>Bu uygulama kapsamında toplanan kişisel verilerinizden Group X veri sorumlusu olarak sorumludur.</p>
            <p><strong>İletişim:</strong> support@groupx.com</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">2. Toplanan Kişisel Veriler</h3>
            <p>Uygulamamız kapsamında aşağıdaki veriler toplanabilmektedir:</p>
            
            <div className="kvkk-subsection">
              <h4 className="kvkk-subsection-title">Kimlik ve iletişim verileri</h4>
              <ul>
                <li>Ad ve soyad</li>
                <li>E-posta adresi</li>
              </ul>
            </div>

            <div className="kvkk-subsection">
              <h4 className="kvkk-subsection-title">Hesap güvenliği verileri</h4>
              <ul>
                <li>Parola (şifrelenmiş/hashing ile saklanır)</li>
              </ul>
            </div>

            <div className="kvkk-subsection">
              <h4 className="kvkk-subsection-title">Oturum ve kullanım kayıtları</h4>
            </div>

            <div className="kvkk-subsection">
              <h4 className="kvkk-subsection-title">Sağlık ve fiziksel veriler (hassas nitelikli veriler)</h4>
              <ul>
                <li>Yaş / doğum tarihi</li>
                <li>Cinsiyet</li>
                <li>Boy, kilo</li>
                <li>Kullanılan ilaçlar, ilaç alerjileri ve ilaç tüketim bilgileri</li>
                <li>Sağlıkla ilişkili tercih ve bildirimler</li>
              </ul>
            </div>

            <div className="kvkk-subsection">
              <h4 className="kvkk-subsection-title">Teknik veriler</h4>
              <ul>
                <li>Uygulama kullanım verileri (oturum süresi, hatırlatma tercihleri, cihaz bilgileri vb.)</li>
              </ul>
            </div>

            <div className="kvkk-note">
              <p><strong>Not:</strong> Sağlıkla ilgili bilgiler özel nitelikli kişisel veriler kapsamında değerlendirildiğinden, bu verilerin işlenmesi için açık ve özgül rızanız alınmakta ve ek güvenlik önlemleri uygulanmaktadır.</p>
            </div>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">3. Kişisel Verilerin İşlenme Amaçları</h3>
            <p>Toplanan veriler aşağıdaki amaçlarla işlenmektedir:</p>
            <ul>
              <li>Kullanıcı hesabı oluşturma ve kimlik doğrulama, oturum yönetimi</li>
              <li>Kullanıcıya özel ilaç hatırlatmaları, uyarılar ve sağlıkla ilgili bildirimler sunma</li>
              <li>İlaç etkileşimleri ve kullanıcıya özel içerik önerileri için veri analizi (anonimleştirme/özetleme yapılabilir)</li>
              <li>Hesap güvenliğinin sağlanması, kötüye kullanımın tespiti ve önlenmesi</li>
              <li>Uygulama performansının izlenmesi ve kullanıcı deneyiminin iyileştirilmesi</li>
              <li>KVKK ve ilgili mevzuat gereği yerine getirilmesi gereken hukuki yükümlülüklerin sağlanması</li>
            </ul>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">4. İşleme Hukuki Sebebi ve Rıza</h3>
            <p>Sağlıkla ilgili hassas verilerin işlenmesi için açık rızanız alınmaktadır. Kayıt sürecinde ve ilgili formlarda bu rızaya uygun onay mekanizması yer almaktadır.</p>
            <p>Diğer verilerin işlenmesi ise hizmet sözleşmesinin kurulması ve yürütülmesi, hukuki yükümlülüklerin yerine getirilmesi ve kullanıcı onayı gibi hukuki sebeplere dayanmaktadır.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">5. Kişisel Verilerin Aktarımı</h3>
            <p>Toplanan veriler, açık rızanız olmadığı sürece üçüncü kişilere veya kuruluşlara aktarılmaz.</p>
            <p>Yalnızca hizmetin sağlanması için gerekli olması halinde (örn. bulut hizmet sağlayıcıları, e-posta servisleri) sözleşmeye dayalı olarak yetkilendirilmiş hizmet sağlayıcılarla paylaşım yapılabilir; bu durumlarda sağlayıcılar gerekli gizlilik ve güvenlik yükümlülüklerini üstlenir.</p>
            <p>Yasal zorunluluk halinde yetkili resmi kurumlarla veri paylaşımı yapılabilir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">6. Saklama Süresi</h3>
            <p>Hesabınız aktif olduğu sürece verileriniz saklanır.</p>
            <p>Hesabın kapatılması veya silinmesi talebi halinde verileriniz ilgili mevzuata uygun olarak ve geri dönülemez biçimde silinir; fakat yasal saklama yükümlülükleri varsa belirli kayıtlar mevzuata uygun süreyle muhafaza edilebilir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">7. Güvenlik Önlemleri</h3>
            <ul>
              <li>Parolalar güvenli hash algoritmaları ile saklanır; parola düz metin olarak tutulmaz.</li>
              <li>Veri iletimi HTTPS kullanılarak şifrelenir.</li>
              <li>Sunucu ve veritabanı erişimleri yetkilendirme ile sınırlandırılmıştır.</li>
              <li>Düzenli yedekleme, erişim kontrolü, güvenlik testleri ve güncellemeler uygulanmaktadır.</li>
              <li>Sağlık verileri gibi hassas veriler için ek erişim kontrolleri ve kayıt tutma mekanizmaları uygulanır.</li>
            </ul>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">8. Özel Durumlar: 18 Yaş Altı Kullanıcılar</h3>
            <p>18 yaşından küçük kişilerin (reşit olmayanların) kişisel verileri ilgili mevzuata göre ayrı hassasiyet gerektirir. Uygulamamız 18 yaş altı kullanıcıların kaydı için velî veya vasinin açık rızasını talep eder. Reşit olmayan kullanıcıların sağlık verileri yalnızca yasal çerçevede ve velî onayıyla işlenir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">9. KVKK Kapsamındaki Haklarınız</h3>
            <p>KVKK madde 11 uyarınca, veri sahibi olarak aşağıdaki haklara sahipsiniz:</p>
            <ul>
              <li>İşlenip işlenmediğini öğrenme,</li>
              <li>İşlenmişse buna ilişkin bilgi talep etme,</li>
              <li>İşlenme amacını ve amaca uygun kullanılıp kullanılmadığını öğrenme,</li>
              <li>Yurt içinde/yurt dışında aktarıldıysa kimlere aktarıldığını öğrenme,</li>
              <li>Eksik/yanlış işlenmişse düzeltilmesini isteme,</li>
              <li>KVKK'daki şartlara göre silinmesini veya yok edilmesini isteme,</li>
              <li>İşlemlerin üçüncü kişilere aktarılması halinde onların da bilgilendirilmesini isteme,</li>
              <li>Otomatik sistemlerle aleyhinize sonuç oluşturulması halinde itiraz etme,</li>
              <li>Zarara uğramanız hâlinde tazminat talep etme.</li>
            </ul>
            <p>Bu taleplerinizi <strong>support@groupx.com</strong> adresine yazılı olarak iletebilirsiniz; talepleriniz en kısa sürede mevzuata uygun olarak değerlendirilip cevaplandırılacaktır.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">10. İtiraz ve Başvuru Usulü</h3>
            <p>KVKK kapsamındaki talepleriniz için e-posta ile <strong>support@groupx.com</strong> adresine kimlik doğrulamanızı sağlayacak bilgilerle birlikte başvurunuz. Başvurularınız ücretsiz olarak değerlendirilecektir; ancak karmaşık talepler belirli bir süre alabilir ve bunun hakkında bilgilendirme yapılır.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">11. Yürürlük ve Güncelleme</h3>
            <p>Bu aydınlatma metni 21 Kasım 2025 tarihinde yürürlüğe girmiştir. Group X, kanuni düzenlemeler veya uygulama ihtiyaçları doğrultusunda metni güncelleme hakkını saklı tutar. Güncelleme yapıldığında kullanıcılar uygulama üzerinden bilgilendirilecektir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">12. Ek Bilgiler ve İletişim</h3>
            <p>Bu KVKK aydınlatma metni ile ilgili herhangi bir sorunuz veya talebiniz olması durumunda, aşağıdaki iletişim kanallarından bizimle iletişime geçebilirsiniz:</p>
            <ul>
              <li><strong>E-posta:</strong> support@groupx.com</li>
              <li><strong>Telefon:</strong> +90 (212) 555 0123</li>
              <li><strong>Adres:</strong> Group X Teknoloji A.Ş., İstanbul, Türkiye</li>
              <li><strong>Web Sitesi:</strong> www.groupx.com</li>
            </ul>
            <p>Başvurularınız en geç 30 gün içinde yanıtlanacaktır. Karmaşık durumlarda bu süre 60 güne kadar uzayabilir ve bu durumda sizlere bilgi verilecektir.</p>
          </div>

          <div className="kvkk-section">
            <h3 className="kvkk-section-title">13. Son Hükümler</h3>
            <p>Bu aydınlatma metni, 6698 sayılı Kişisel Verilerin Korunması Kanunu ve ilgili mevzuat hükümleri çerçevesinde hazırlanmıştır. Kanun ve mevzuatta yapılacak değişiklikler bu metni de etkileyebilir.</p>
            <p>Bu metinde yer alan hükümlerden herhangi birinin geçersiz sayılması durumunda, diğer hükümlerin geçerliliği etkilenmez.</p>
            <p>Bu aydınlatma metni Türkçe olarak hazırlanmış olup, farklı dillerdeki çevirilerinde uyuşmazlık olması durumunda Türkçe metin esas alınır.</p>
          </div>

        </div>
      </div>
    </div>
  );
};

export default KVKKPage;
