# Release & Versioning — OKS Framework ve WaitMe

Bu doküman iki akış için sürümleme ve yayın yaklaşımını standardize eder.

---

## 1) OKS Framework — SemVer + Paketleme

### 1.1 SemVer
- **MAJOR**: breaking change
- **MINOR**: geriye uyumlu yeni özellik
- **PATCH**: geriye uyumlu hata düzeltme

### 1.2 Pre-release
- `1.2.0-alpha.1`, `1.2.0-beta.1`, `1.2.0-rc.1`

### 1.3 Changelog
Her release için:
- Added / Changed / Fixed / Breaking

### 1.4 Deprecation Policy (Öneri)
- Obsolete attribute + 1 minor sürüm süre
- Sonraki major’da kaldır

### 1.5 Paket Yapısı (Örnek)
- Oks.Abstractions
- Oks.Core
- Oks.Hosting
- Oks.Api
- Oks.Validation
- Oks.Caching
- Oks.Messaging
- Oks.Realtime
- Oks.Observability
- Oks.Security
- Oks.Connectors

---

## 2) WaitMe — Product Versioning

### 2.1 Uygulama Versiyonu
- Web: `YYYY.MM.Sprint` veya `MAJOR.MINOR.PATCH`
- Mobil: store gerekliliklerine göre

### 2.2 Release Train (Öneri)
- Her sprint sonunda “internal release”
- Her 2 sprintte “external beta”
- Pilot/Prod: release candidate süreci

---

## 3) Release Checklist (Ortak)
- Build/test yeşil
- Migration planı hazır (varsa)
- Rollback planı
- Observability kontrolleri (log/trace)
- Güvenlik kontrolleri (secrets, auth)
- Runbook güncel

---

## 4) Branching (Öneri)
- Trunk-based (önerilen) veya GitFlow (kurumsal zorunluluk varsa)
- Feature branch kısa ömürlü
- Tag ile release: `oks-v1.3.0`, `waitme-v0.8.0`

