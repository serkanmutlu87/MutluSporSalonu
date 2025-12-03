# Mutlu Spor Salonu – Yönetim ve Randevu Sistemi  
### Web Programlama Dersi – 2025-2026 Güz Dönemi

---

## Proje Hakkında
Bu proje, spor salonlarında ihtiyaç duyulan temel yönetim işlemlerini tek bir sistemde toplamak amacıyla geliştirilmiştir. Uygulama; spor salonu bilgileri, hizmetler, antrenörler, üyeler ve randevu işlemlerini kapsayan bir yapı sunar. Ayrıca kullanıcıların boy, kilo ve hedef bilgilerine göre öneri alabildiği yapay zekâ tabanlı bir modül de bulunmaktadır.

---

## Kullanılan Teknolojiler
- ASP.NET Core MVC  
- Entity Framework Core  
- SQL Server  
- LINQ  
- REST API  
- Bootstrap 5  
- HTML, CSS, JavaScript  
- Google Gemini API (öneri modülü için)

---

## Rol Sistemi
Uygulamada admin ve üye olmak üzere iki rol bulunmaktadır.

### Admin
- Spor salonlarını, hizmetleri, antrenörleri ve üyeleri yönetebilir  
- Randevu kayıtlarını görüntüleyebilir  
- API uçlarına erişebilir  

### Üye
- Kendi randevularını görüntüleyebilir  
- Yeni randevu oluşturabilir  
- Yapay zekâ öneri modülünü kullanabilir  

Varsayılan admin bilgileri:  
E-posta: ogrencinumarasi@sakarya.edu.tr  
Şifre: sau

---

## Projenin Temel Modülleri

### Spor Salonları
Salon adı, açıklama ve iletişim bilgileri tanımlanabilir. Antrenör ve hizmetlerle ilişkilendirilebilir.

### Hizmet Tanımları
Fitness, pilates, yoga gibi hizmet türleri süre ve ücret bilgisiyle sisteme eklenebilir.

### Antrenör Yönetimi
Uzmanlık alanları ve müsaitlik saatleri tanımlanabilir. Randevu sırasında uygunluk kontrolü yapılır.

### Üye Yönetimi
Üyeler kayıt olabilir, bilgilerini düzenleyebilir veya silebilir. ViewModel yapısı ile doğrulama uygulanır.

### Randevu Planlama
Üye, hizmet ve antrenör seçerek randevu oluşturabilir. Hizmet süresine göre otomatik bitiş saati hesaplanır ve çakışma kontrolü yapılır.

### Yapay Zekâ Modülü
Kullanıcıdan alınan bilgilere göre metin tabanlı öneri üretir. Gemini API kullanılır.

### REST API
- Antrenörleri listeleme  
- Tarihe göre uygun antrenörleri döndürme  
- Üye randevularını JSON olarak çekme  
API tarafında LINQ filtrelemeleri uygulanmaktadır.

---

## Ana Sayfa ve Kullanıcı Akışı
- Giriş yapmamış kullanıcılar: Tanıtım amaçlı landing sayfa  
- Admin: Yönetim paneli  
- Üye: Randevu ve öneri paneli  

---

## Proje Yapısı (Kısaca)
