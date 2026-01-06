# ADR — Repo Yapısı ve Sprint 0 Temel Kararları

**ADR No:** ADR-001  
**Başlık:** Repo yapısı, dokümantasyon düzeni ve sürümleme yaklaşımı  
**Tarih:** 2025-02-14  
**Durum:** Accepted  
**Karar Sahibi:** OKS Platform Team  

---

## 1) Bağlam
Sprint 0 kapsamında OKS Framework ve WaitMe için ortak bir depo yapısı, dokümantasyon düzeni ve sürümleme standardı belirlenmesi gerekiyor. Mevcut repo içinde dokümanlar farklı isimlendirmelerle dağınık ve sürümleme tekil projelerde tanımlı.

## 2) Karar
- Repo kökü altında **standart dizinler** kullanılacak: `src/`, `tests/`, `docs/`, `samples/`, `deploy/`.
- Dokümanlar `docs/` altında tutulacak, plan dokümanları `docs/plan/` ve mimari kararlar `docs/adr/` altında olacak.
- NuGet paket sürümlemesi **SemVer + prerelease** yaklaşımı ile merkezi olarak `Directory.Build.props` üzerinden yönetilecek.

## 3) Alternatifler
- Alternatif A: Her projede ayrı ayrı sürümleme tanımlamak (eksi: tutarsızlık, bakım yükü).
- Alternatif B: Dokümanları repo kökünde bırakmak (eksi: keşfedilebilirlik düşük).
- Alternatif C: CI/CD ile sürümleme çözmek (eksi: başlangıç karmaşıklığı).

## 4) Sonuçlar
- Pozitif: Tutarlı repo yapısı, dokümanlara hızlı erişim, merkezi sürümleme.
- Negatif/Risk: İlk taşıma sırasında link kırılmaları olabilir.
- Operasyonel: CI ve release süreçleri ileride standartlaşabilir.

## 5) Uygulama Notları
- Sprint 0 çalışması kapsamında dizinler oluşturulacak ve dokümanlar yeniden adlandırılacak.
- README doküman linkleri güncellenecek.
