# WaitMe Docs

## Environment Profilleri
- **Development**: Lokal geliştirme, ayrıntılı log.
- **Test**: Entegrasyon testleri için azaltılmış log, mock bağımlılıklar.
- **Production**: Güvenli varsayılanlar ve minimum log seviyesi.

## Config Stratejisi
- Varsayılan ayarlar `appsettings.json` üzerinden gelir.
- Ortam bazlı override için `appsettings.{Environment}.json` kullanılır.
- Gizli değerler için ortam değişkenleri tercih edilir.
