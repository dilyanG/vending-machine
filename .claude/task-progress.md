# Task Progress

Live state of the project. **Read this first in every session.** Update it in
the same commit as the work it describes. Rules: `CLAUDE.md` §7.

**Status:** `[ ]` not started · `[~]` in progress · `[x]` done · `[!]` blocked · `[-]` dropped

---

## Next up

1. `P0-5`, `P0-6` — Angular app scaffold + ESLint/Prettier
2. `P1-1` … `P1-5` — domain model and coin rules
3. `P2-1` … `P2-4` — change calculator

## Open questions

- *(none yet — add anything needing the repo owner's decision here, not only in chat)*

---

## Phases

### P0 — Repository scaffold `[~]`

- [x] `P0-1` git init, `main`, `.gitignore` (.NET + Node + Angular + IDE)
- [x] `P0-2` `.editorconfig`
- [x] `P0-3` `VM.Server.slnx` + 4 src projects + 3 test projects, correct references
- [x] `P0-4` `Directory.Build.props` (net10.0, nullable, warnings-as-errors)
- [ ] `P0-5` Angular app scaffold (scss, routing, strict, strictTemplates)
- [ ] `P0-6` ESLint + Prettier + npm scripts
- [x] `P0-7` Root docs in place

### P1 — Domain model and coin rules `[ ]`

- [ ] `P1-1` `Product` entity with guards
- [ ] `P1-2` `CoinDenominations` {5,10,20,50,100,200}, `MaxQuantityPerProduct = 15`
- [ ] `P1-3` `CoinBundle` immutable value object
- [ ] `P1-4` `ErrorCodes` + `DomainException`
- [ ] `P1-5` Domain unit tests incl. boundaries 0/15/16/-1 and €0.01/€0.02 rejection

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

---

## Session log

One line per working session: date, phases touched, anything the next session
needs to know.

- `2026-09-15` — Planning. Wrote `CLAUDE.md`, `README.md`,
  `IMPLEMENTATION_PLAN.md` and this tracker. No code yet. Next: P0.
- `2026-09-15` — Executed P0-1..P0-4 (backend half of P0) on `iteration-01`.
  Added root `.gitignore`/`.editorconfig`, wired the four existing
  `VM.Server.*` projects into `VM.Server.slnx` with the correct dependency
  direction (none had any `ProjectReference` before this), added the three
  xUnit test projects, consolidated shared MSBuild properties into
  `Directory.Build.props`, and replaced the MVC-controller `Program.cs`
  scaffold with a minimal API (`/health` only, CORS policy `frontend`, Swagger
  in Development). `dotnet build`/`dotnet test` green, `/health` verified live
  on port 5080. `CLAUDE.md`/`IMPLEMENTATION_PLAN.md` updated for the
  `Service`/`Repository` naming. Next: P0-5..P0-7 (frontend scaffold + docs).
- `2026-09-15` — Rewrote the root `README.md` (currency/coins, getting
  started, tests, project layout, how seeding/CRUD/vending work, API
  summary, configuration), pointing every path at the real
  `src/vm-server/VM.Server/VM.Server.*` / `src/vm-client/` layout instead
  of an earlier `backend/`+`frontend/`+`VendingMachine.*` draft. Closes
  `P0-7`. Next: P0-5/P0-6 (Angular scaffold + lint/format).
