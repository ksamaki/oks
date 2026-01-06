# Definition of Ready (DoR) ve Definition of Done (DoD)

Bu doküman, OKS Framework ve WaitMe Product backlog’larında **kaliteyi standardize etmek** için kullanılır.

---

## 1) Definition of Ready (DoR)

Bir iş (story/task) sprint’e alınmadan önce:

### Ortak DoR
- İşin **amacı** (neden) ve **kapsamı** net
- Kabul kriterleri yazılı (en az 2 madde)
- Bağımlılıklar tanımlı (repo, servis, paket, dış sistem)
- Tahmini efor (S/M/L veya story point)
- Test yaklaşımı belli (unit/integration/e2e)

### OKS için ek DoR
- Public API tasarımı düşünülmüş (namespace, naming)
- Geriye uyumluluk etkisi not edilmiş (breaking change?)
- Örnek kullanım (sample snippet) tanımlı

### WaitMe için ek DoR
- Kullanıcı akışı (user journey) ile ilişkisi belirtilmiş
- UI/UX wireframe veya basit ekran taslağı (varsa)
- Veri modeli etkisi (migration gerekecek mi?) not edilmiş

---

## 2) Definition of Done (DoD)

### Ortak DoD (Minimum)
- Kod tamam, build başarılı
- Unit test eklendi/uygulandı (makul kapsam)
- Gerekli dokümantasyon güncellendi (`docs/` veya README)
- Log/telemetry temel düzeyde eklendi (en az hata ve kritik akış)
- Güvenlik: auth/authorization gereken yerde uygulanmış
- Review tamam (tek kişi repo ise self-review checklist uygulanmış)

### OKS Framework için ek DoD
- Public API doc + örnek kullanım eklendi
- Paket versiyonu ve changelog güncellendi
- En az 1 “sample” proje veya test ile doğrulandı
- Breaking change varsa migration/release note yazıldı

### WaitMe için ek DoD
- Feature flag / config ile yönetilebilir (gerekiyorsa)
- Integration test veya smoke test eklendi (minimum)
- UI tarafında temel hata/empty state davranışı var

---

## 3) Kabul Kriteri Örnekleri

### OKS Örnek
- “Validation error” 400 + problem details döner
- HTTP client wrapper hata gövdesini kaybetmeden parse eder

### WaitMe Örnek
- Kullanıcı login olur, token alır
- Mesaj gönderildiğinde alıcı anlık bildirim alır

---

## 4) Sprint Review Checklist (Öneri)
- Sprint goal’a ulaşıldı mı?
- Demo senaryosu çalışıyor mu?
- Öğrenimler ve aksiyonlar kaydedildi mi? (Retrospective)
