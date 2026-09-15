# Task Progress

Live state of the project. **Read this first in every session.** Update it in
the same commit as the work it describes. Rules: `CLAUDE.md` §7.

**Status:** `[ ]` not started · `[~]` in progress · `[x]` done · `[!]` blocked · `[-]` dropped

---

## Next up

1. `P2-1` … `P2-4` — change calculator
2. `P3-1` … `P3-6` — product state and CRUD
3. `P4-1` … `P4-7` — vending use cases

## Open questions

- *(none yet — add anything needing the repo owner's decision here, not only in chat)*

---

## Phases

### P0 — Repository scaffold `[x]`

Backend solution + Angular app scaffolded, both lint/build/test clean; root
docs in place. `P0-1`…`P0-7` all done — see decision/session log for the
CLI quirks worked around.

### P1 — Domain model and coin rules `[x]`

- [x] `P1-1` `Product` entity with guards
- [x] `P1-2` `CoinDenominations` {5,10,20,50,100,200}, `MaxQuantityPerProduct = 15`
- [x] `P1-3` `CoinBundle` immutable value object
- [x] `P1-4` `ErrorCodes` + `DomainException`
- [x] `P1-5` Domain unit tests incl. boundaries 0/15/16/-1 and €0.01/€0.02 rejection

### P2 — Change calculation `[ ]`

- [ ] `P2-1` `IChangeCalculator` + `ChangeResult`
- [ ] `P2-2` Bounded coin-change DP, minimal coin count
- [ ] `P2-3` Comment documenting why greedy is wrong, with counterexample
- [ ] `P2-4` Tests incl. the greedy counterexample and the empty-bank case

### P3 — Application layer: products `[ ]`

- [ ] `P3-1` `IProductStore`, `IExternalCatalogSource`
- [ ] `P3-2` `catalog.seed.json` — 6 products, distinct prices
- [ ] `P3-3` `FileExternalCatalogSource` (read-only)
- [ ] `P3-4` `InMemoryProductStore` — thread-safe, lazy seed, `Reload()`
- [ ] `P3-5` `ProductService` with all validation rules
- [ ] `P3-6` Tests incl. seed-file-immutability assertion

### P4 — Application layer: vending `[ ]`

- [ ] `P4-1` `ICoinBank` + `InMemoryCoinBank`, float from config
- [ ] `P4-2` `VendingSession`
- [ ] `P4-3` `InsertCoin` with denomination validation
- [ ] `P4-4` `Purchase` with rollback on `CHANGE_UNAVAILABLE`
- [ ] `P4-5` `Reset` returning identical denominations
- [ ] `P4-6` Single lock over session + bank + inventory
- [ ] `P4-7` Tests incl. atomicity proof and `paid == price + change`

### P5 — HTTP API `[ ]`

- [ ] `P5-1` Minimal API endpoint groups
- [ ] `P5-2` DTOs per `CLAUDE.md` §3.2
- [ ] `P5-3` Exception middleware → §3.3 error shape
- [ ] `P5-4` DI, CORS policy `frontend`, Swagger
- [ ] `P5-5` Port fixed to 5080
- [ ] `P5-6` `WebApplicationFactory` integration tests covering every route
- [ ] **Contract frozen** — note the date here once P5 is merged

### P6 — Frontend foundation `[ ]`

- [ ] `P6-1` environments + `apiBaseUrl`
- [ ] `P6-2` Models mirroring API DTOs
- [ ] `P6-3` `ProductsApiService`, `VendingApiService`
- [ ] `P6-4` Error interceptor + `ERROR_MESSAGES`
- [ ] `P6-5` `centsToCurrency` pipe
- [ ] `P6-6` `_tokens.scss`, `_mixins.scss`, `_reset.scss`, dark mode
- [ ] `P6-7` App shell + lazy routes `/` and `/products`
- [ ] `P6-8` Pipe + API service tests

### P7 — Vending UI `[ ]`

- [ ] `P7-1` `vending.store` signal store
- [ ] `P7-2` `machine-display` (`aria-live`)
- [ ] `P7-3` `coin-slot` from API denominations
- [ ] `P7-4` `product-grid` + `product-card`
- [ ] `P7-5` `change-tray`
- [ ] `P7-6` Return-coins / reset
- [ ] `P7-7` Responsive layout 1/2/3/4 columns, sticky coin panel on mobile
- [ ] `P7-8` Loading / empty / error states
- [ ] `P7-9` Store tests

### P8 — Products admin UI `[ ]`

- [ ] `P8-1` `products.store`
- [ ] `P8-2` Responsive table / card list
- [ ] `P8-3` `product-form-dialog` with full validation
- [ ] `P8-4` Delete confirmation
- [ ] `P8-5` Reload-from-external-catalog action
- [ ] `P8-6` Server errors mapped onto form fields
- [ ] `P8-7` Form validation tests

### P9 — Polish, verification, handover `[ ]`

- [ ] `P9-1` Manual pass over the final acceptance checklist
- [ ] `P9-2` Accessibility pass
- [ ] `P9-3` Empty / loading / error states reviewed
- [ ] `P9-4` README verified from a clean clone
- [ ] `P9-5` Dead code, TODOs, debug logging removed; builds warning-free
- [ ] `P9-6` Screenshots / GIF in `docs/`, linked from README
- [ ] `P9-7` *(optional)* Docker + compose
- [ ] `P9-8` *(optional)* GitHub Actions CI
- [ ] `P9-9` Final pass over this file

---

## Decision log

Format: `YYYY-MM-DD — decision — why — alternatives rejected`

- `2026-09-15` — **EUR, denominations 5/10/20/50/100/200 cents** — a realistic
  European coin set that makes change-making non-trivial — rejected USD (fewer
  interesting cases) and BGN (less familiar to reviewers).
- `2026-09-15` — **Mock external resource inside the backend**, a seed JSON file
  exposed read-only at `/api/external/catalog` — demonstrably a separate source
  without a third process to start — rejected a standalone `json-server`
  (extra moving part) and a static JSON in Angular (bypasses the backend).
- `2026-09-15` — **In-memory state, no database** — the brief says CRUD applies
  to application state only — rejected EF Core InMemory (ceremony with no
  benefit here).
- `2026-09-15` — **Money as integer cents throughout** — removes any
  floating-point rounding risk in a money-handling domain.
- `2026-09-15` — **Bounded coin-change DP instead of greedy** — the coin bank is
  finite, so greedy can claim a payout the machine cannot make.
- `2026-09-15` — **Refuse the purchase when exact change is impossible** and
  return the coins — a real machine does this, and it is the honest behaviour —
  rejected over-paying change or accepting the loss.
- `2026-09-15` — **Projects named `VM.Server.{Domain,Service,Repository,API}`**,
  not `VendingMachine.{Domain,Application,Infrastructure,Api}` as originally
  drafted in `CLAUDE.md` §4.1 — the solution had already been scaffolded under
  `src/vm-server/VM.Server/` with these names before P0 execution started; layer
  responsibilities and dependency direction (`API → Service → Domain`,
  `Repository` implements `Service`'s abstractions) are unchanged — rejected
  renaming the existing projects to match the original doc, which would have
  discarded real work to satisfy a plan written before the code existed.
  `CLAUDE.md` and `IMPLEMENTATION_PLAN.md` updated to match.
- `2026-09-15` — **Upgraded the machine's Node.js 22.11.0 → 22.23.2** (winget)
  — Angular CLI 22.1.8 hard-refuses below Node `22.22.3`/`24.15`/`26.0` (a
  blocking check, not a warning) — rejected pinning an older `@angular/cli`,
  which would break the fixed "Angular 22" decision in `CLAUDE.md` §1.
- `2026-09-15` — **`"strict"`/`"strictTemplates"` added to `tsconfig.json` by
  hand** — confirmed by diffing `--strict` vs `--strict=false` scaffolds that
  CLI 22.1.8's `--strict` flag never writes those two umbrella booleans (only
  the individual `noImplicit*` flags), even though it does everything else
  "strict" implies — a CLI templating gap, worked around locally.
- `2026-09-15` — **`--test-runner=karma` passed explicitly** — CLI 22.1.8
  defaults to Vitest; overridden to match `CLAUDE.md` §1's fixed
  "Jasmine/Karma" decision. (`--zoneless` was also tried but is a no-op on
  this CLI version — zoneless is already the unconditional default.)
- `2026-09-15` — **Name-uniqueness and price-distinctness are NOT enforced on
  `Product`** even though both are hard requirements — they are collection-level
  rules (comparing one product against every other product), and `Product` has
  no visibility of its siblings. A single entity cannot enforce them without a
  static registry or other cross-instance state, which would make every
  `Product.Create` call secretly stateful and untestable in isolation. They
  belong to `ProductService` in P3, which already holds the full collection —
  rejected a static/singleton registry inside `Domain` (violates "Domain
  references nothing" and makes unit tests order-dependent).
- `2026-09-15` — **`CoinBundle.Remove` throws `InvalidOperationException`, not
  `DomainException`, when asked to remove more coins than present** —
  `ErrorCodes` is fixed to the `CLAUDE.md` §3.3 list, and none of those ten
  codes generically fits "this bundle doesn't have enough of that coin"; the
  bundle also has no idea *why* it's being asked (inserted-coins session vs.
  the coin bank vs. a future context), so it can't safely pick one. Mirrors the
  BCL `Try*`/throwing convention (e.g. `Queue<T>.Dequeue`): callers where
  underflow is a real, expected outcome (the P2 change calculator) must use
  `TryRemove` and choose their own domain-specific error; hitting the throwing
  `Remove` path means the caller's own invariant was already broken — rejected
  reusing `CHANGE_UNAVAILABLE` (wrong layer — `CoinBundle` is also used for the
  session's inserted coins, which have nothing to do with change-making) and
  rejected adding a new error code (the task fixed `ErrorCodes` to the §3.3
  list plus `INVALID_PRODUCT` only).

---

## Session log

One line per working session: date, phases touched, anything the next session
needs to know.

- `2026-09-15` — Planning. Wrote `CLAUDE.md`, `README.md`,
  `IMPLEMENTATION_PLAN.md` and this tracker. No code yet.
- `2026-09-15` — P0-1..P0-4: root `.gitignore`/`.editorconfig`, `VM.Server.*`
  projects wired into `VM.Server.slnx` with correct dependency direction,
  three xUnit test projects added, `Directory.Build.props`, minimal-API
  `Program.cs` (`/health`, CORS policy `frontend`, Swagger). `dotnet
  build`/`test` green, `/health` verified live on 5080.
- `2026-09-15` — Rewrote root `README.md` to match the real
  `src/vm-server/`/`src/vm-client/` layout (was drafted against an earlier
  `backend/`+`frontend/` naming). Closes P0-7.
- `2026-09-15` — P0-5/P0-6: Node.js on the machine (22.11.0) was below
  Angular CLI 22's hard minimum (22.22.3), so `ng new` refused to run at all;
  upgraded Node to 22.23.2 via winget first (repo owner's go-ahead). Scaffolded
  `vm-client` in place (standalone, zoneless-by-default, karma), hand-fixed a
  CLI gap where `--strict` doesn't actually write `"strict"`/`"strictTemplates"`
  into `tsconfig.json`, added the dev proxy, wired `@angular-eslint` with
  explicit `prefer-signals`/`prefer-output-emitter-ref` rules (bans
  `@Input()`/`@Output()`/`@ViewChild()`), Prettier + `eslint-config-prettier`,
  and the `test:ci`/`lint:fix`/`format`/`format:check` scripts. Replaced the
  default boilerplate shell with a minimal `<h1>` + `<router-outlet>`; deleted
  the CLI's own `README.md`/`.editorconfig` inside `vm-client/` (the latter had
  `root = true` and would have shadowed the repo-root LF rule). `npm install`,
  `lint`, `build`, `test:ci` (2/2) and `format:check` all green; verified live
  on 4200 with the proxy reaching the real backend on 5080 (no `/api/*` route
  exists yet — that's P5). Updated `CLAUDE.md`/`IMPLEMENTATION_PLAN.md` off the
  stale `frontend/` name. Closes P0.
- `2026-09-15` — P1: `Product` entity (private setters, `Create`/`Restore`
  factories, `Rename`/`ChangePrice`/`SetQuantity`/`DecrementStock`, all
  re-validating), `CoinDenominations`, immutable value-equal `CoinBundle`
  (`Add`/`Remove`/`TryRemove`/`Combine`), `ErrorCodes` (+ new `INVALID_PRODUCT`)
  and `DomainException`. No collection-level rules (name/price uniqueness) on
  the entity — see decision log, that's P3's job. `VM.Server.Domain.csproj`
  still has zero package/project references. 47 new Domain tests (boundary
  tables via `[Theory]`), solution-wide `dotnet build`/`test` green (49 total).
  Closes P1.
