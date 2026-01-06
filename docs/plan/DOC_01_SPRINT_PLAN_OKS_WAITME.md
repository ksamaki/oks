# Sprint Planı — OKS Framework vs WaitMe Uygulaması (Literatür/Uygulama Uyumlu)

Bu paket; iki paralel akışı **net biçimde ayırır**:

- **OKS Framework (Platform/Enabling Work):** Kurumsal ölçekte tekrar kullanılabilir kütüphaneler, standardizasyon, cross-cutting concerns.
- **WaitMe Uygulaması (Product/Feature Work):** Son kullanıcı değerini üreten ürün geliştirme (web/mobil, domain servisler).

Dokümanlar; çevik literatüre uygun olarak **Epics → Capabilities → Sprint Goals → Stories/Tasks → DoD** ile yapılandırılmıştır.

> Varsayım: 2 haftalık sprint, toplam 8 sprint.  
> Not: Sprint 0 opsiyonel “inception/bootstrapping” sprintidir.

---

## 0. Terimler ve Model (Literatür Eşlemesi)

### 0.1. Ürün vs Platform ayrımı
- **Product Backlog (WaitMe):** kullanıcı değeri (user journey) üreten işlevler.
- **Platform / Enabling Backlog (OKS):** ürün ekiplerinin hızını artıran altyapı (framework) işleri.

Bu ayrım; **Team Topologies** (platform team & stream-aligned team) yaklaşımıyla uyumludur.

### 0.2. “Done” tanımı
Her iki akış için ortak “Definition of Done” (DoD) ve “Definition of Ready” (DoR) ayrı dokümanda verilmiştir.
- `DOC_02_DOD_DOR.md`

---

## 1. Epics (Üst Seviye)

### OKS Framework Epics
- **F1 — Oks.Core & Hosting**
- **F2 — API Standards & Error Contract**
- **F3 — Validation**
- **F4 — Caching**
- **F5 — Messaging**
- **F6 — Realtime**
- **F7 — Observability & Security Baseline**
- **F8 — Packaging & Release Engineering**

### WaitMe Product Epics
- **W1 — Identity & Profile**
- **W2 — Location**
- **W3 — Chat / Messaging**
- **W4 — Notifications**
- **W5 — Web UI**
- **W6 — Mobile**
- **W7 — Admin & Ops (Backoffice)**
- **W8 — Integration (Connectors/Gateway) + Pilot**

---

## 2. Sprint 0 (Opsiyonel) — Inception / Bootstrapping

### Sprint Goal
Çalışma anlaşmaları, repo yapısı, CI iskeleti, ADR süreci; OKS ve WaitMe için ortak zemin.

#### OKS Framework (Yapılacaklar)
- Repo/solution yapısı: `src/oks/*`, `tests/*`, `docs/*`, `samples/*`
- Paket isimlendirme: `Oks.*` (nuget metadata)
- Kod standartları + lint/format (EditorConfig)
- Sürümleme yaklaşımı (SemVer + prerelease)

#### WaitMe (Yapılacaklar)
- Uygulama repo/solution yapısı: `src/waitme/*`, `deploy/*`, `docs/*`
- Environment profilleri (dev/test/prod) + config stratejisi
- Minimum “hello” servis ve UI skeleton

#### Kabul Kriterleri
- CI: build + test pipeline çalışır
- ADR şablonu hazır ve ilk ADR yazılmış (ör. repo stratejisi)

---

## 3. Sprint 1 — Temel Platform + İlk Servis İskeleti

### Sprint Goal
OKS core “enabling” seti ve WaitMe’de ilk gerçek servis skeleton’ı çalışır.

#### OKS Framework
**Deliverables**
- `Oks.Abstractions`: Result modeli (Success/Fail), Error modeli
- `Oks.Core`: configuration wrapper, tenant/user context abstractions (opsiyonel)
- `Oks.Hosting`: standard host builder extensions

**Stories/Tasks**
- Standard logging adapter (Microsoft.Extensions.Logging)
- Health checks extension
- Sample: `Oks.Samples.HelloService`

**Kabul Kriterleri**
- `dotnet test` yeşil, sample servis local çalışır

#### WaitMe
**Deliverables**
- `WaitMe.Gateway` veya `WaitMe.Identity` skeleton (.NET)
- Web UI skeleton (Angular/React) — login page placeholder

**Stories/Tasks**
- Service discovery / base routing yaklaşımı (gateway yoksa basit reverse proxy planı)
- Local dev docker-compose (db + broker placeholder)

**Kabul Kriterleri**
- WaitMe servis health endpoint döner
- Web UI ayağa kalkar

---

## 4. Sprint 2 — API Standardı + Error Contract + Validation

### Sprint Goal
Tüm servislerde **tutarlı API sözleşmesi** ve validasyon davranışı.

#### OKS Framework
**Deliverables**
- `Oks.Http`: typed client + error handling (senin senaryo)
- `Oks.Api`: ProblemDetails (RFC7807) uyumlu hata sözleşmesi
- `Oks.Validation`: FluentValidation entegrasyonu (pipeline behavior / filter)

**Stories/Tasks**
- Validation errors → standart error response (exception fırlatmadan)
- Swagger conventions (operationId, response types)
- “Client library” tek `AddOksHttpClients()` ile kurulum

**Kabul Kriterleri**
- Validasyon hatası: 400 + problem details
- Client: validasyon hatalarını doğru parse eder

#### WaitMe
**Deliverables**
- Identity API endpoints (register/login) — MVP
- Kullanıcı profil DTO’ları + validasyon

**Stories/Tasks**
- Token issuance (JWT) / session modeli
- Basic auth middleware wiring (OKS üzerinden)

**Kabul Kriterleri**
- Register/login çalışır
- Yanlış input’ta standart problem details döner

---

## 5. Sprint 3 — Caching + Persistence Template

### Sprint Goal
Cache ve persistence kalıpları; üründe gerçek bir endpoint üzerinde kanıtlanır.

#### OKS Framework
**Deliverables**
- `Oks.Caching`: in-memory + Redis provider
- Cache key strategy (tenant/user scope)
- Attribute/interceptor: `Cacheable`, `CacheEvict`

**Stories/Tasks**
- Cache stampede önleme (opsiyonel)
- TTL policy + jitter
- Migration/release notları

**Kabul Kriterleri**
- Cache hit/miss ölçümü loglanır
- Integration test: cache invalidation

#### WaitMe
**Deliverables**
- Location servis (DB + cache) — MVP
- “Nearby users/places” endpoint (placeholder algoritma)

**Stories/Tasks**
- DB schema + migrations
- Location caching (OKS caching ile)

**Kabul Kriterleri**
- İlk çağrı DB, ikinci çağrı cache
- Cache invalidate senaryosu doğrulanır

---

## 6. Sprint 4 — Messaging (Queue) + Event Contract + Outbox

### Sprint Goal
Event-driven temel: publish/consume, retry/DLQ davranışı.

#### OKS Framework
**Deliverables**
- `Oks.Messaging`: abstraction + transport adapters
- RabbitMQ adapter (MVP)
- Kafka adapter (stub)
- Outbox pattern (MVP)

**Stories/Tasks**
- Event naming/versioning convention
- Idempotency strategy (consumer side)

**Kabul Kriterleri**
- Basit event publish → consume akışı demo
- Hata → retry/DLQ gözlemlenir

#### WaitMe
**Deliverables**
- Chat servisinde “MessageSent” event’i
- Notification servisinde event consumer (stub)

**Kabul Kriterleri**
- Chat mesajı at → event yayınla → consumer loglasın

---

## 7. Sprint 5 — Realtime (SignalR) + Notifications

### Sprint Goal
Gerçek zamanlı bildirim MVP.

#### OKS Framework
**Deliverables**
- `Oks.Realtime`: SignalR abstraction + auth integration
- Scale-out planı (Redis backplane) dokümantasyonu

**Kabul Kriterleri**
- Hub üzerinden auth’lu bağlantı ve mesaj push

#### WaitMe
**Deliverables**
- Notification Hub (WaitMe) + “notify user” endpoint
- Web UI: anlık bildirim dinleme

**Kabul Kriterleri**
- API’den notify → web client anında alır
- Unauthorized subscribe engellenir

---

## 8. Sprint 6 — WaitMe MVP (End-to-End User Journey)

### Sprint Goal
Kayıt → profil → konum → chat → bildirim uçtan uca.

#### OKS Framework
**Deliverables**
- Hardening: rate limiting hooks, security baseline docs
- Telemetry hooks (trace/log correlation ids)

#### WaitMe
**Deliverables**
- Servisler: Identity, Location, Chat, Notification (MVP)
- Web UI: login + map placeholder + chat
- Mobil: sadece login + list (opsiyonel)

**Kabul Kriterleri**
- Demo senaryosu: iki kullanıcı → konum → chat → bildirim

---

## 9. Sprint 7 — Observability + DevOps + Security Hardening

### Sprint Goal
Üretime giden yol: tracing/metrics/log, CI/CD, gateway policies.

#### OKS Framework
**Deliverables**
- `Oks.Observability`: OpenTelemetry setup helper
- `Oks.Security`: basic policies (CORS, headers, auth helpers)

**Kabul Kriterleri**
- Trace id servisler arası taşınır
- Merkezi logta korelasyon yapılır

#### WaitMe
**Deliverables**
- Gateway (APISIX/Kong/alternatif) baseline config
- Rate limit/throttling kuralları (dev ortam)
- Deploy runbook

**Kabul Kriterleri**
- Pipeline → dev deploy
- Gateway rate limit çalışır

---

## 10. Sprint 8 — Integrations + Pilot Release

### Sprint Goal
Connector yaklaşımı ve pilot yayın hazırlığı.

#### OKS Framework
**Deliverables**
- `Oks.Connectors`: connector interface + örnek REST/SOAP adapter
- Versioning + release notes otomasyonu

#### WaitMe
**Deliverables**
- En az 1 dış sistem entegrasyonu (dummy)
- Pilot kurulum dokümantasyonu + kullanıcı kılavuzu taslağı

**Kabul Kriterleri**
- Dış sistem → event → iç servis akışı çalışır
- Pilot ortam ayağa kalkar

---

## 11. Doküman Paketi İçeriği
Bu sprint planı paketi aşağıdaki ek dokümanlarla birlikte gelir:
- `DOC_01_SPRINT_PLAN_OKS_WAITME.md` (bu dosya)
- `DOC_02_DOD_DOR.md`
- `DOC_03_ADR_TEMPLATE.md`
- `DOC_04_RELEASE_VERSIONING.md`

