# CLAUDE.md

Project instructions for AI assistants (and humans) working in this repository.
Read this file **in full** before writing any code. If a request conflicts with
this file, say so and ask before proceeding.

---

## 1. What we are building

A **vending machine** web application, submitted as a technical assessment.

### Original requirements (verbatim intent, condensed)

- Inventory size — up to **15 products of the same type** (per product type).
- **Price of products** — must differ per product type.
- Use a **currency of choice**; the accepted coin denominations must be noted in
  the README, and the machine must accept **only** those denominations.
- The machine **must return change**.
- **Web design: responsive.**

Operations to implement:

1. **Products** — get the *initial* product list from an **external resource**
   (a mock API that we create ourselves).
2. **CRUD** on products, applied **only to application state** — the external
   resource is never written back to.
3. **Vending** — insert coins, buy product, reset process (return the inserted
   coins without a purchase).

Deliverables: a README describing how to build and run, plus a link to the repo.

### Our stack decisions (fixed — do not change without asking)

| Area | Choice |
| --- | --- |
| Backend | .NET 10 (LTS), C# 14, ASP.NET Core Minimal APIs |
| Frontend | Angular 22, standalone components, signals, TypeScript strict |
| Repo | Monorepo: `backend/` + `frontend/` |
| State | In-memory, thread-safe singleton store in the backend. **No database.** |
| Currency | **EUR** |
| Mock "external resource" | A dedicated mock-catalog endpoint inside the backend, reading a seed JSON file |
| Tests | xUnit (backend), Jasmine/Karma (frontend) |
| Styling | Plain SCSS with design tokens. No UI component framework. |

---

## 2. Domain rules — the single source of truth

These rules are non-negotiable. Both backend and frontend must agree on them,
and the backend is always the authority (the frontend may pre-validate for UX,
but never decides).

### 2.1 Money

- **All money is stored and transported as integer cents (`int`).** Never
  `double`, never `float`. `decimal` only at a presentation boundary if
  unavoidable.
- A price of €1.45 is `145`.
- Formatting to `€1.45` happens **only** in the Angular presentation layer.
- Field names carrying cents end in `Cents` / `cents` (e.g. `priceCents`).

### 2.2 Accepted coin denominations (EUR)

```
5, 10, 20, 50, 100, 200   (cents)  =  €0.05, €0.10, €0.20, €0.50, €1.00, €2.00
```

- **Rejected:** €0.01 and €0.02 coins, and all banknotes.
- An insert of any other value returns `400` with error code
  `INVALID_DENOMINATION`. It must never silently round or accept.
- This list lives in exactly one place per side:
  - backend: `CoinDenominations` static class in the domain project;
  - frontend: fetched from the backend (`GET /api/vending/denominations`) so the
    two can never drift. Do not hard-code the list in Angular.

### 2.3 Inventory

- `Quantity` is an `int` in `[0, 15]` inclusive. `15` is `MaxQuantityPerProduct`.
- Creating or updating a product with quantity outside that range →
  `INVALID_QUANTITY`.
- Buying a product with `Quantity == 0` → `OUT_OF_STOCK`.
- Product **names must be unique** (case-insensitive) — `DUPLICATE_PRODUCT`.
- **Prices must be distinct across product types** (an explicit requirement).
  Violation → `DUPLICATE_PRICE`.
- `priceCents` must be > 0 and a multiple of 5 (no price can be unmakeable from
  the accepted denominations).

### 2.4 Change

- The machine has a **coin bank** with a finite number of each denomination.
  Change is computed against the *actual* coins available, not an infinite float.
- Because the bank is finite, a greedy algorithm is **incorrect**. Use a
  **bounded coin-change** (dynamic programming) solution that minimises coin
  count and returns "not possible" when exact change cannot be made.
- If exact change cannot be made: **the purchase is refused**, nothing is
  dispensed, the inserted coins are returned in full, and the API returns
  `CHANGE_UNAVAILABLE`.
- The machine never over- or under-pays. `inserted == price + change` always.
- A successful purchase moves the inserted coins into the bank *before*
  computing change (a customer's own coins are usable as change).

### 2.5 Vending session

- One implicit session (single-user machine). It holds: inserted coins by
  denomination, and the running `insertedTotalCents`.
- `insert` → adds one coin of the given denomination.
- `purchase` → requires `insertedTotalCents >= priceCents`, else
  `INSUFFICIENT_FUNDS`. On success: decrement quantity, return change, clear
  session.
- `reset` → returns the exact coins inserted (same denominations, not
  equivalent value) and clears the session. Reset **never** touches the bank or
  inventory.
- Every vending operation is **atomic**. A failed purchase leaves inventory,
  bank and session exactly as they were.

### 2.6 Seeding from the "external resource"

- `src/vm-server/VM.Server/VM.Server.Repository/MockExternalApi/catalog.seed.json`
  is the mock external resource.
- It is exposed read-only at `GET /api/external/catalog` so a reviewer can see
  it is genuinely a separate source.
- The in-memory store loads from it **once**, lazily, on first access.
- CRUD mutates the in-memory store **only**. The seed file is never written.
- `POST /api/products/reload` re-seeds from the external resource (discarding
  in-memory changes) — useful for demos and tests.

---

## 3. API contract

Base URL `http://localhost:5080`. All responses JSON. All errors use the shape
in §3.3.

### 3.1 Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/external/catalog` | Mock external resource (read-only, never mutated) |
| `GET` | `/api/products` | List products from application state |
| `GET` | `/api/products/{id}` | Single product |
| `POST` | `/api/products` | Create |
| `PUT` | `/api/products/{id}` | Update |
| `DELETE` | `/api/products/{id}` | Delete |
| `POST` | `/api/products/reload` | Re-seed state from the external resource |
| `GET` | `/api/vending/denominations` | Accepted denominations (cents) |
| `GET` | `/api/vending/session` | Current inserted coins + total |
| `POST` | `/api/vending/coins` | Insert one coin `{ "denominationCents": 50 }` |
| `POST` | `/api/vending/purchase` | Buy `{ "productId": "..." }` |
| `POST` | `/api/vending/reset` | Return inserted coins, clear session |

### 3.2 Key payloads

```jsonc
// Product
{ "id": "guid", "name": "Espresso", "priceCents": 145, "quantity": 12,
  "imageUrl": "assets/products/espresso.svg" }

// POST /api/vending/purchase → 200
{ "product": { /* Product, quantity already decremented */ },
  "paidCents": 200,
  "priceCents": 145,
  "changeCents": 55,
  "changeCoins": [ { "denominationCents": 50, "count": 1 },
                   { "denominationCents": 5,  "count": 1 } ] }

// POST /api/vending/reset → 200
{ "returnedCoins": [ { "denominationCents": 100, "count": 2 } ],
  "returnedTotalCents": 200 }
```

### 3.3 Error shape

```jsonc
{ "code": "CHANGE_UNAVAILABLE",
  "message": "The machine cannot give exact change for this purchase.",
  "details": { "shortfallCents": 5 } }
```

Error codes: `INVALID_DENOMINATION`, `INSUFFICIENT_FUNDS`, `OUT_OF_STOCK`,
`CHANGE_UNAVAILABLE`, `PRODUCT_NOT_FOUND`, `INVALID_QUANTITY`, `INVALID_PRICE`,
`DUPLICATE_PRODUCT`, `DUPLICATE_PRICE`.

HTTP mapping: `400` validation/business rule, `404` not found, `409` conflict
(duplicates), `422` `CHANGE_UNAVAILABLE`, `500` unexpected.

---

## 4. Backend instructions (.NET 10 / C#)

### 4.1 Layout

```
src/vm-server/VM.Server/
  VM.Server.slnx
  Directory.Build.props
  VM.Server.Domain/                # entities, value objects, rules. No deps.
    Entities/            Product.cs
    ValueObjects/        Coin.cs, Money.cs, CoinBundle.cs
    CoinDenominations.cs
    Services/            IChangeCalculator.cs, BoundedChangeCalculator.cs
    Errors/              DomainError.cs, ErrorCodes.cs
  VM.Server.Service/               # use cases + abstractions. Depends on Domain only.
    Products/            ProductService.cs, dtos
    Vending/             VendingService.cs, dtos
    Abstractions/        IProductStore.cs, ICoinBank.cs, IExternalCatalogSource.cs
  VM.Server.Repository/            # implementations: in-memory store, mock external API
    InMemory/            InMemoryProductStore.cs, InMemoryCoinBank.cs
    MockExternalApi/     FileExternalCatalogSource.cs, catalog.seed.json
  VM.Server.API/                   # Minimal API endpoints, DI, CORS, Swagger
    Endpoints/           ProductEndpoints.cs, VendingEndpoints.cs, ExternalEndpoints.cs
    Middleware/          ExceptionHandlingMiddleware.cs
    Program.cs
  tests/
    VM.Server.Domain.Tests/
    VM.Server.Service.Tests/
    VM.Server.API.Tests/           # WebApplicationFactory integration tests
```

Dependency direction is strictly `API → Service → Domain`, with
`Repository` implementing `Service`'s abstractions. **Domain references
nothing.** Do not let ASP.NET types leak below `API`.

### 4.2 Conventions

- `Nullable` and `TreatWarningsAsErrors` enabled in `Directory.Build.props`;
  `ImplicitUsings` enabled; `LangVersion latest`.
- File-scoped namespaces, one type per file, `sealed` by default.
- Prefer `record` for DTOs and value objects, `class` for entities with identity.
- **Minimal APIs**, grouped with `MapGroup("/api/products")`, one extension
  method per endpoint group. No MVC controllers.
- Validation lives in the Service layer and throws `DomainException`
  carrying an `ErrorCode`; the exception middleware maps it to the §3.3 shape.
  Do not return raw `ProblemDetails`.
- Services are registered as **singletons** (the store and bank *are* the app
  state). Guard all mutations with a single `lock` or `SemaphoreSlim` — treat
  concurrent requests as real.
- CORS: named policy `frontend` allowing `http://localhost:4200`, configured in
  `appsettings.Development.json`, not hard-coded.
- Swagger/OpenAPI enabled in Development at `/swagger`.
- No AutoMapper, no MediatR, no Entity Framework. Hand-written mapping methods.
- Logging via the built-in `ILogger<T>`; log business refusals at `Information`,
  unexpected errors at `Error`.

### 4.3 Testing

- xUnit + `FluentAssertions`.
- Test names: `Method_Scenario_ExpectedResult`
  (e.g. `Purchase_WhenBankCannotMakeChange_RefusesAndReturnsCoins`).
- Mandatory coverage:
  - change calculator: exact change, minimal coin count, impossible change,
    empty bank, change of 0;
  - denomination validation incl. €0.01/€0.02 rejection;
  - quantity bounds 0 and 15;
  - purchase atomicity after a failed purchase;
  - reset returns the *same* denominations that were inserted;
  - duplicate name and duplicate price rejection;
  - integration: full insert → purchase → change happy path over HTTP.
- No test may depend on another test's state.

---

## 5. Frontend instructions (Angular)

### 5.1 Layout

```
frontend/
  src/app/
    core/
      api/          products-api.service.ts, vending-api.service.ts, api-error.ts
      models/       product.model.ts, coin.model.ts
      state/        products.store.ts, vending.store.ts     # signal stores
      interceptors/ error.interceptor.ts
      pipes/        cents-to-currency.pipe.ts
    features/
      vending/      vending-page, coin-slot, product-grid, product-card,
                    change-tray, machine-display
      products/     products-page (admin CRUD), product-form-dialog, product-table
    shared/ui/      button, modal, badge, empty-state, confirm-dialog
    styles/         _tokens.scss, _mixins.scss, _reset.scss
```

### 5.2 Conventions

- **Standalone components only.** No `NgModule`s.
- **Signals for all state.** `signal()` for writable, `computed()` for derived,
  `resource()`/`httpResource` or explicit loaders for async. No `BehaviorSubject`
  stores. RxJS only where it genuinely fits (debounced input, `switchMap`).
- `ChangeDetectionStrategy.OnPush` on every component.
- `input()` / `output()` signal APIs, not `@Input()` / `@Output()` decorators.
- Native control flow `@if` / `@for` / `@switch`. Never `*ngIf` / `*ngFor`.
- `inject()` over constructor injection.
- TypeScript `strict: true`; no `any`; template type-checking `strictTemplates`.
- Component files ≤ 200 lines. Split before exceeding.
- Smart/dumb split: feature *pages* talk to stores; everything under
  `shared/ui` and every `*-card` / `*-table` is presentational, driven by
  inputs and emitting outputs.

### 5.3 State and money on the client

- Stores hold cents. The `centsToCurrency` pipe is the **only** place that
  formats (`Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' })`).
- The frontend never computes change and never decides whether a purchase is
  allowed — it renders what the API returns. It may *disable* a buy button when
  `insertedTotal < price` for UX, but must still handle the server's rejection.
- Accepted denominations come from `GET /api/vending/denominations`.
- Every API error is surfaced by mapping `error.code` to a human message in one
  `ERROR_MESSAGES` map. Never print a raw server message.

### 5.4 Responsive design

- **Mobile-first.** Base styles target 360px; enhance upward.
- Breakpoints (in `_tokens.scss`, used via mixins only):
  `sm 480px`, `md 768px`, `lg 1024px`, `xl 1280px`.
- Product grid: 1 column < 480px, 2 up to 768px, 3 up to 1024px, 4 above.
- The coin slot / display docks to the bottom as a sticky bar on mobile and sits
  as a side panel from `lg` up.
- Touch targets ≥ 44×44px. No horizontal scroll at any width ≥ 320px.
- Use CSS custom properties from `_tokens.scss` for colour, spacing, radius and
  type scale. No magic numbers in component styles.
- Verify at 360 / 768 / 1024 / 1440px before marking a UI task done.

### 5.5 Accessibility

- Every interactive element is a real `button` / `a` / `input`.
- Coin buttons carry `aria-label="Insert 50 cent coin"`.
- The machine display is an `aria-live="polite"` region so inserted totals and
  errors are announced.
- Visible focus rings; never `outline: none` without a replacement.
- Colour contrast ≥ 4.5:1 for text.

### 5.6 Testing

- Jasmine/Karma via `ng test`.
- Mandatory: `centsToCurrency` pipe, both API services (with
  `HttpTestingController`), `vending.store` insert/reset/purchase transitions,
  and the product form's validation.
- Use `ComponentFixture` + harnesses; query by role or `data-testid`, never by
  CSS class.

---

## 6. Git and commit messages

### 6.1 Branching

- `main` is always green (builds, tests pass).
- Work on `feat/<short-slug>` branches; squash-merge into `main`.
- Never commit directly to `main` except for the initial scaffold and doc fixes.

### 6.2 Commit message format — Conventional Commits

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:** `feat`, `fix`, `refactor`, `test`, `docs`, `style`, `chore`, `build`, `ci`, `perf`.

**Scopes:** `be`, `fe`, `domain`, `api`, `vending`, `products`, `ui`, `repo`,
`docs`, `ci`. Use the narrowest scope that fits.

**Subject line**

- Imperative mood: "add", not "added" / "adds".
- Lower-case after the colon, no trailing period.
- ≤ 72 characters.
- Describe the *behaviour change*, not the files touched.
  ✅ `feat(vending): refuse purchase when exact change is impossible`
  ❌ `feat(vending): update VendingService.cs`

**Body** (required for anything non-trivial)

- Wrap at 72 columns, blank line after the subject.
- Explain **why**, and the trade-off taken — the diff already shows *what*.
- Mention any rule from §2 that the commit implements or relaxes.
- List follow-ups as `TODO:` lines if you knowingly left something out.

**Footer**

- `Refs: P3-2` — the task id from `task-progress.md` (**required**).
- `BREAKING CHANGE: <what and how to migrate>` when the API contract changes.
- **No AI/tool co-author or "generated with" trailers.** This is an assessment
  repo; keep the history clean and human-readable.

**Example**

```
feat(domain): compute change with bounded coin-change DP

The coin bank holds a finite number of each denomination, so greedy
selection can report success on a set it cannot actually pay out
(e.g. change of 40c from a bank of 20c x 1 and 50c x 5).

Replaces the greedy pass with a bounded dynamic-programming solve that
minimises coin count and returns a failure result when no exact
combination exists. Purchases that cannot be changed are now refused
and the customer's coins returned in full.

Refs: P2-3
```

### 6.3 Commit hygiene

- **One logical change per commit.** Backend and frontend changes go in separate
  commits unless a single API contract change forces both.
- Every commit must build and pass tests on its own.
- Never commit: `bin/`, `obj/`, `node_modules/`, `dist/`, `.angular/`,
  `*.user`, `.env`, editor folders. Keep `.gitignore` current.
- Never commit commented-out code or `Console.WriteLine` / `console.log` debug
  output.
- Update `task-progress.md` **in the same commit** as the work it describes.

---

## 7. `task-progress.md` protocol

`task-progress.md` at the repo root is the living state of the project. It is
how a fresh session picks up where the last one stopped.

**Read it at the start of every session, before touching code.**

Rules:

1. Its phases and task ids mirror `IMPLEMENTATION_PLAN.md` exactly. Ids never
   change or get reused.
2. Statuses: `[ ]` not started · `[~]` in progress · `[x]` done ·
   `[!]` blocked · `[-]` dropped (with a reason).
3. Mark a task `[x]` **only** when: the code is written, its tests pass, the
   whole solution builds, and the acceptance criteria in the plan are met.
   Partial work stays `[~]` with a note on what remains.
4. Update it in the **same commit** as the work. A commit that changes code
   without touching the tracker (or vice versa) is a mistake.
5. Append to the **Decision log** whenever you choose between real
   alternatives, deviate from this file, or discover a constraint. Format:
   `YYYY-MM-DD — decision — why — alternatives rejected`.
6. Keep **Next up** (3 items max) and **Open questions** current at the top.
   Anything needing the repo owner's input goes in Open questions, not in chat
   only.
7. Never delete history from the file. Strike through superseded entries.
8. Keep it under ~200 lines — summarise completed phases into one line each once
   they are closed.

---

## 8. Working agreements for the assistant

- **Follow `IMPLEMENTATION_PLAN.md` phase by phase.** Do not jump ahead or
  bundle phases together; each phase is sized to be reviewable.
- Before writing code for a phase, restate in one or two sentences what you are
  about to build and which acceptance criteria it satisfies.
- After each phase: run the build and the tests, report the actual output, then
  update `task-progress.md` and commit.
- **Do not invent requirements.** If the spec is silent (e.g. whether the
  machine should refill its bank), add it to Open questions and pick the
  simplest defensible default — then log it in the Decision log.
- Never weaken a rule in §2 to make a test pass. Fix the code or raise it.
- Prefer boring, readable code over clever code. A reviewer is going to read
  this in fifteen minutes and judge it.
- If you cannot run a command, say so plainly — never report a build or test run
  you did not actually perform.
