# Implementation Plan

Build order for the vending machine. Each **phase** is one prompt / one review
unit: small enough to check in a few minutes, large enough to be a meaningful
commit. Do not start a phase before the previous one is `[x]` in
`task-progress.md`.

Read `CLAUDE.md` §2 (domain rules) and §3 (API contract) before every phase —
they are the specification this plan implements.

**Legend** — each phase lists: *Goal*, *Tasks* (with ids used in commits and in
`task-progress.md`), *Deliverables*, *Acceptance criteria*, and a *Prompt* you
can paste to start it.

Dependency order:

```
P0 ─ P1 ─ P2 ─ P3 ─ P4 ─┬─ P5 ─ P6 ─ P7 ─ P8 ─ P9
                        └─ (P5 can start once P4's contract is frozen)
```

---

## P0 — Repository scaffold

**Goal:** an empty but correctly shaped repo that builds on a clean machine.

**Tasks**

- `P0-1` Init git repo, `main` branch, `.gitignore` (.NET + Node + Angular + IDE).
- `P0-2` Add `.editorconfig` (4 spaces C#, 2 spaces TS/HTML/SCSS, LF, UTF-8, final newline).
- `P0-3` Create `backend/VendingMachine.sln` with the four `src` projects and three `tests` projects, wired with the dependency direction from `CLAUDE.md` §4.1.
- `P0-4` Add `backend/Directory.Build.props`: `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `LangVersion=latest`.
- `P0-5` Scaffold the Angular app into `src/vm-client/`: `ng new vm-client --directory=. --style=scss --ssr=false --routing`, strict mode on, `strictTemplates` on.
- `P0-6` Add ESLint + Prettier to `vm-client/`; `npm run lint`, `npm run format`.
- `P0-7` Copy `CLAUDE.md`, `README.md`, `IMPLEMENTATION_PLAN.md`, `task-progress.md` to the repo root.

**Deliverables:** repo tree, solution file, Angular app, config files.

**Acceptance criteria**

- `dotnet build` succeeds with zero warnings.
- `npm start` serves the default Angular page at :4200.
- `git status` is clean — no `bin/`, `obj/`, `node_modules/`, `.angular/` tracked.

**Prompt**

> Execute phase P0 of IMPLEMENTATION_PLAN.md. Create the repository scaffold only — no domain code yet. Show me the resulting tree and the output of `dotnet build`.

---

## P1 — Domain model and coin rules

**Goal:** the vocabulary of the system, with no I/O and no framework.

**Tasks**

- `P1-1` `Product` entity: `Id (Guid)`, `Name`, `PriceCents (int)`, `Quantity (int)`, `ImageUrl`. Guard: name non-empty, price > 0 and `% 5 == 0`, quantity in `[0, 15]`.
- `P1-2` `CoinDenominations`: the frozen set `{5, 10, 20, 50, 100, 200}`, `IsAccepted(int)`, `MaxQuantityPerProduct = 15`.
- `P1-3` `CoinBundle` value object: an immutable `denomination → count` map with `Add`, `Remove`, `TotalCents`, `Coins`. Rejects unaccepted denominations and negative counts.
- `P1-4` `ErrorCodes` constants and `DomainException(ErrorCode, message, details)`.
- `P1-5` Domain unit tests for every guard above, including the €0.01/€0.02 rejection and the quantity boundaries 0, 15, 16, -1.

**Deliverables:** `VendingMachine.Domain` + `VendingMachine.Domain.Tests`.

**Acceptance criteria**

- `VendingMachine.Domain.csproj` has **zero** package and project references.
- All domain tests pass; boundary values are covered explicitly.

**Prompt**

> Execute phase P1: the domain model. Follow CLAUDE.md §2.1–§2.3. Domain project must have no dependencies. Include the unit tests and show me `dotnet test` output.

---

## P2 — Change calculation

**Goal:** the algorithmic heart of the machine, done correctly.

**Tasks**

- `P2-1` `IChangeCalculator` with `ChangeResult Calculate(int amountCents, CoinBundle available)` returning either the coins to dispense or a "not possible" result. No exceptions for the impossible case.
- `P2-2` Implement with **bounded coin change** (DP over amount, respecting per-denomination counts), minimising coin count.
- `P2-3` Document in a code comment *why* greedy is wrong here, with the concrete counterexample.
- `P2-4` Tests: amount 0 → empty bundle; exact-coin cases; minimal-coin-count assertion; a case greedy gets wrong (e.g. need 40c from `{20c×1, 50c×5}` → impossible, greedy might take 50c); empty bank; bank smaller than the amount; a large amount for performance sanity.

**Deliverables:** change calculator + its tests.

**Acceptance criteria**

- The greedy counterexample test exists and passes.
- Calculating change for €5.00 from a bank of 200 coins completes in < 50ms.
- The calculator is pure: same inputs → same output, no mutation of the input bundle.

**Prompt**

> Execute phase P2: the change calculator. Bounded coin-change DP, minimal coin count, no greedy. Include the counterexample test proving greedy would fail. Show me `dotnet test` output.

---

## P3 — Service layer: products

**Goal:** product CRUD over in-memory state, seeded from the mock external resource.

**Tasks**

- `P3-1` `IProductStore` and `IExternalCatalogSource` abstractions in `Service/Abstractions`.
- `P3-2` `catalog.seed.json` — 6 products with **distinct prices**, sensible quantities ≤ 15, and `imageUrl` values pointing at frontend assets.
- `P3-3` `FileExternalCatalogSource` reading that file (read-only; never writes).
- `P3-4` `InMemoryProductStore`: thread-safe singleton, lazy one-time seed from the source, full CRUD, `Reload()`.
- `P3-5` `ProductService`: create / read / update / delete / list / reload, enforcing unique name (case-insensitive), **distinct price across product types**, quantity `[0,15]`, price > 0 and `% 5 == 0`.
- `P3-6` Tests: seed happens once; CRUD does not mutate the seed file; duplicate name → `DUPLICATE_PRODUCT`; duplicate price → `DUPLICATE_PRICE`; quantity 16 → `INVALID_QUANTITY`; reload discards in-memory edits.

**Deliverables:** `Service/Products`, `Repository/InMemory`, `Repository/MockExternalApi`.

**Acceptance criteria**

- After create/update/delete, `catalog.seed.json` is byte-identical (assert it in a test).
- Concurrent creates of the same name produce exactly one product.

**Prompt**

> Execute phase P3: product state and CRUD. Follow CLAUDE.md §2.3 and §2.6. Seed from the mock external catalog once, never write back to it. Include the file-immutability test.

---

## P4 — Service layer: vending

**Goal:** insert / purchase / reset, atomic and correct.

**Tasks**

- `P4-1` `ICoinBank` + `InMemoryCoinBank`, initial float configured from `appsettings.json` (`CoinBank` section).
- `P4-2` `VendingSession`: inserted `CoinBundle` + total.
- `P4-3` `VendingService.InsertCoin(denomination)` — rejects unaccepted values with `INVALID_DENOMINATION`.
- `P4-4` `VendingService.Purchase(productId)` — validates product exists, stock > 0, funds sufficient; moves inserted coins into the bank; computes change; on `CHANGE_UNAVAILABLE` **rolls everything back** and returns the coins; on success decrements quantity, clears the session, returns product + change coins.
- `P4-5` `VendingService.Reset()` — returns the exact denominations inserted, clears the session, leaves bank and inventory untouched.
- `P4-6` One lock covering session + bank + inventory mutations so a purchase is atomic.
- `P4-7` Tests: happy path with change; exact money (change 0); `INSUFFICIENT_FUNDS`; `OUT_OF_STOCK`; `CHANGE_UNAVAILABLE` leaves state untouched (assert bank, quantity and session all unchanged); reset returns identical denominations; inserted coins are usable as change.

**Deliverables:** `Service/Vending` + tests.

**Acceptance criteria**

- Post-condition asserted in tests: `paidCents == priceCents + changeCents`.
- A failed purchase is provably a no-op on all three pieces of state.

**Prompt**

> Execute phase P4: the vending use cases. Follow CLAUDE.md §2.4 and §2.5. Purchases must be atomic — include the test that proves a CHANGE_UNAVAILABLE purchase leaves bank, inventory and session unchanged.

---

## P5 — HTTP API

**Goal:** the contract in `CLAUDE.md` §3, live and documented. **Freeze the
contract here** — the frontend builds against it.

**Tasks**

- `P5-1` Minimal API endpoint groups: `ExternalEndpoints`, `ProductEndpoints`, `VendingEndpoints`.
- `P5-2` Request/response DTOs exactly as in §3.2; hand-written mapping.
- `P5-3` `ExceptionHandlingMiddleware` mapping `DomainException` → the §3.3 error shape with the §3 HTTP status mapping.
- `P5-4` DI wiring: store, bank and session as singletons; named CORS policy `frontend`; Swagger in Development.
- `P5-5` `launchSettings.json` fixing the port to 5080.
- `P5-6` Integration tests with `WebApplicationFactory`: insert → purchase → change over HTTP; each error code returns the right status and body; `/api/external/catalog` is read-only.

**Deliverables:** `VendingMachine.Api` + `VendingMachine.Api.Tests`.

**Acceptance criteria**

- Every route in §3.1 exists and is exercised by at least one integration test.
- Swagger lists all endpoints with correct response types.
- `curl` of the happy path works end to end against a running server.

**Prompt**

> Execute phase P5: the HTTP API. Implement exactly the routes, payloads and error shape in CLAUDE.md §3 — no additions. Include WebApplicationFactory integration tests and show me a curl of the happy path.

---

## P6 — Frontend foundation

**Goal:** the Angular app can talk to the API and format money, with nothing
rendered yet beyond a shell.

**Tasks**

- `P6-1` `environment.ts` / `environment.development.ts` with `apiBaseUrl`.
- `P6-2` Models mirroring the API DTOs (`Product`, `CoinDenomination`, `PurchaseResult`, `ResetResult`, `ApiError`).
- `P6-3` `ProductsApiService` and `VendingApiService` using `HttpClient` + `provideHttpClient(withFetch())`.
- `P6-4` `error.interceptor.ts` normalising failures into a typed `ApiError`; `ERROR_MESSAGES` map from error code → user-facing text.
- `P6-5` `centsToCurrency` pipe using `Intl.NumberFormat('de-DE', {style:'currency', currency:'EUR'})`.
- `P6-6` `styles/_tokens.scss` (colour, spacing, radius, type scale, breakpoints), `_mixins.scss` (`respond-to`), `_reset.scss`. Light + dark via `prefers-color-scheme`.
- `P6-7` App shell: header, router outlet, a `/` landing page with two cards, and lazily loaded `/vending` and `/products` (admin) feature route groups.
- `P6-8` Tests for the pipe and both API services (`HttpTestingController`).

**Deliverables:** `core/`, `styles/`, routing, shell.

**Acceptance criteria**

- `npm run lint` clean; `ng build` succeeds with no TS errors.
- Navigating between the two routes works; the shell is responsive at 360px.
- No hard-coded denomination list anywhere in the frontend.

**Prompt**

> Execute phase P6: the Angular foundation — API services, models, error mapping, currency pipe, design tokens, routing shell. No feature UI yet. Follow CLAUDE.md §5. Show me `ng build` and `ng test` output.

---

## P7 — Vending UI

**Goal:** the screen the reviewer will actually judge.

**Tasks**

- `P7-1` `vending.store` (signals): products, denominations, inserted coins, total, last purchase, last error, loading flags; `insert`, `purchase`, `reset`, `refresh` actions.
- `P7-2` `machine-display` — `aria-live` region showing inserted total, prompts and errors.
- `P7-3` `coin-slot` — one button per denomination (from the API), disabled while a request is in flight.
- `P7-4` `product-grid` + `product-card` — image, name, price, stock badge, *Buy* button; disabled and visibly marked when out of stock; affordability hinted (not enforced) client-side.
- `P7-5` `change-tray` — shows dispensed product and the coins returned, broken down by denomination.
- `P7-6` *Return coins* button wired to reset, showing the exact coins back.
- `P7-7` Responsive layout per `CLAUDE.md` §5.4 — grid 1/2/3/4 columns, coin panel sticky-bottom on mobile, side panel from `lg`.
- `P7-8` Loading and empty states; every error code rendered as its friendly message.
- `P7-9` Store tests: insert accumulates, reset clears, purchase success updates stock, purchase failure surfaces the message without changing inserted total.

**Deliverables:** `features/vending/*`.

**Acceptance criteria**

- Full manual run: insert coins → buy → correct change shown → stock decreases.
- `CHANGE_UNAVAILABLE` shows a clear message and the inserted total is still there.
- No horizontal scroll at 360 / 768 / 1024 / 1440px; keyboard-only operation works end to end.

**Prompt**

> Execute phase P7: the vending UI. Signal store plus presentational components, per CLAUDE.md §5. Mobile-first, verified at 360/768/1024/1440. Show me the store tests passing and screenshots or a description at each breakpoint.

---

## P8 — Products admin UI

**Goal:** CRUD over application state, visibly separate from vending.

**Tasks**

- `P8-1` `products.store` (signals) over `ProductsApiService`, incl. `reload`.
- `P8-2` `products-page` with a responsive table (cards below `md`): name, price, quantity, actions.
- `P8-3` `product-form-dialog` — reactive form: name required & unique, price > 0 and a multiple of 5 (entered in euro, submitted as cents), quantity `0–15`, image URL optional. Inline validation messages.
- `P8-4` Delete with a confirmation dialog.
- `P8-5` *Reload from external catalog* button, with a warning that in-memory changes are lost.
- `P8-6` Server-side rejections (`DUPLICATE_PRICE`, `DUPLICATE_PRODUCT`) mapped onto the offending form field, not just a toast.
- `P8-7` Form validation tests.

**Deliverables:** `features/products/*`.

**Acceptance criteria**

- Create, edit, delete and reload all work against a running backend.
- Entering quantity 16 or a duplicate price is blocked with a clear message.
- The table is usable at 360px.

**Prompt**

> Execute phase P8: the products admin UI with full CRUD and reload. Map server validation errors onto form fields. Follow CLAUDE.md §5. Show me the validation tests passing.

---

## P9 — Polish, verification and handover

**Goal:** what turns a working solution into a good submission.

**Tasks**

- `P9-1` Full manual test pass against the acceptance checklist below.
- `P9-2` Accessibility pass: keyboard-only run, focus order, `aria-label`s, contrast ≥ 4.5:1.
- `P9-3` Empty / loading / error states reviewed on every screen.
- `P9-4` README verified from a clean clone — every command in it actually run.
- `P9-5` Remove dead code, TODOs, debug logging; `dotnet build` and `npm run lint` warning-free.
- `P9-6` Screenshots or a short GIF in `docs/` and linked from the README.
- `P9-7` Optional: `Dockerfile` per side + `docker-compose.yml` for one-command startup.
- `P9-8` Optional: GitHub Actions workflow running `dotnet test` and `npm run test:ci` on push.
- `P9-9` Final pass over `task-progress.md`: all phases closed, decision log complete.

**Final acceptance checklist**

- [ ] Only the six EUR denominations are accepted; €0.01/€0.02 rejected with a clear message.
- [ ] Quantity per product type is capped at 15 and enforced on create and update.
- [ ] Every product type has a distinct price, enforced server-side.
- [ ] Initial products come from the mock external resource; CRUD never writes back to it.
- [ ] Change is correct, uses the fewest coins, and is drawn from a finite bank.
- [ ] Impossible change refuses the purchase and returns the coins intact.
- [ ] Reset returns exactly the coins inserted.
- [ ] Layout works at 360, 768, 1024 and 1440px with no horizontal scroll.
- [ ] `dotnet test` and `npm run test:ci` both green.
- [ ] README's build and run instructions work from a clean clone.

**Prompt**

> Execute phase P9: polish and verification. Work through the final acceptance checklist and report the real result of each item — do not tick anything you have not actually verified.

---

## Working with this plan

- One phase per prompt. Start each with: *"Execute phase Pn of
  IMPLEMENTATION_PLAN.md."*
- The assistant restates the goal, implements, runs build + tests, reports the
  real output, updates `task-progress.md`, then commits with `Refs: Pn-x`.
- If a phase turns out to need splitting, add `Pn-x.1` style sub-ids rather than
  renumbering anything.
- Scope changes go in the `task-progress.md` decision log before the code
  changes, not after.

### Rough sizing

| Phase | Size | Notes |
| --- | --- | --- |
| P0 | S | Mechanical |
| P1 | M | |
| P2 | M | Highest-value code in the repo |
| P3 | M | |
| P4 | L | Most business rules land here |
| P5 | M | Freeze the contract |
| P6 | M | |
| P7 | L | The screen that gets judged |
| P8 | M | |
| P9 | M | Do not skip |
