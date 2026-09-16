# Task Progress

Live state of the project. **Read this first in every session.** Update it in
the same commit as the work it describes. Rules: `CLAUDE.md` §7.

**Status:** `[ ]` not started · `[~]` in progress · `[x]` done · `[!]` blocked · `[-]` dropped

---

## Next up

1. `P2-1` … `P2-4` — change calculator (still open; P4's `Purchase` needs a
   real `IChangeCalculator`, not just the interface P1-7 declared)
2. `P4-1` … `P4-7` — vending use cases
3. `P5-1` … `P5-6` — HTTP API

## Open questions

- *(none yet — add anything needing the repo owner's decision here, not only in chat)*

---

## Phases

### P0 — Repository scaffold `[x]`

Backend solution + Angular app scaffolded, both lint/build/test clean; root
docs in place. `P0-1`…`P0-7` all done — see decision/session log for the
CLI quirks worked around.

### P1 — Domain model and coin rules `[x]`

`Product`/`CoinDenominations`/`CoinBundle`/`ErrorCodes`/`DomainException` plus
47 boundary-table unit tests. `P1-1`…`P1-5` all done — see decision/session
log for the scope-boundary calls (name/price uniqueness live in P3, not here).

**Reopened** before P3 for an aggregate-root restructuring — see `P1-6`…`P1-8`
below and the decision log. `CoinBundle`/`P1-3` superseded.

- [x] `P1-6` `Slot` entity (product + quantity, `Dispense`/`Restock`);
      `Quantity` removed from `Product`, which is now pure catalogue data
- [-] `P1-3` `CoinBundle` immutable value object — **superseded by `P1-8`**:
      the aggregate root makes rollback (and therefore persistent/immutable
      coin collections) unnecessary; replaced by mutable `CoinInventory`
- [x] `P1-7` `VendingMachine` aggregate root: owns slots, bank and session;
      compute-then-commit `Purchase` (no rollback code); `Reset`; slot CRUD
      for P3; id-uniqueness/price-distinctness enforced across slots;
      `IChangeCalculator`/`ChangeResult` declared (not implemented — P2)
- [x] `P1-8` `CoinInventory` mutable entity, replacing `CoinBundle`, used for
      both the bank and the session's inserted coins

### P2 — Change calculation `[ ]`

- [ ] `P2-1` `IChangeCalculator` + `ChangeResult`
- [ ] `P2-2` Bounded coin-change DP, minimal coin count
- [ ] `P2-3` Comment documenting why greedy is wrong, with counterexample
- [ ] `P2-4` Tests incl. the greedy counterexample and the empty-bank case

### P3 — Service layer: products `[x]`

Executed on top of the P1-6..P1-8 aggregate, ahead of P2 (change calculator
not needed for product CRUD). `P3-1`…`P3-6` all done.

- [x] `P3-1` `IVendingMachineStore`, `IExternalCatalogSource` (not
      `IProductStore`/`ICoinBank` — the `VendingMachine` aggregate replaced
      both; `CLAUDE.md` §4.1 corrected)
- [x] `P3-2` `catalogue.seed.json` — 6 products, distinct prices 85-245c, no
      quantity field
- [x] `P3-3` `FileExternalCatalogSource` — read-only, `System.Text.Json`
      camelCase, fails loudly (not silently empty) on a missing/malformed file
- [x] `P3-4` `InMemoryVendingMachineStore` — `SemaphoreSlim`-guarded
      once-only lazy load, `VendingMachine:CoinBank` /
      `VendingMachine:InitialQuantityPerSlot` via `IOptions`, `ReloadAsync()`
- [x] `P3-5` `ProductService` — CRUD over the aggregate's slots, mapping only;
      every validation rule delegated to the domain. Added name-uniqueness
      (case-insensitive) to `VendingMachine` itself per this task's own
      instruction — the one collection invariant P1 hadn't covered
- [x] `P3-6` 15 tests: concurrent-first-call-reads-once, CRUD reflected in
      reads, byte-identical seed file after CRUD (SHA-256 before/after),
      duplicate name/price, quantity 16, price 3, reload discards edits,
      delete of an unknown id

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

`P6-5`/`P6-6`/`P6-7` pulled forward ahead of `P1`…`P5`: they don't touch the
API contract (no models, no HTTP calls), so there's no rework risk, and doing
them now removes three tasks from the critical path once `P5` unblocks the
rest of `P6`. `P6-1`…`P6-4` and `P6-8` are still blocked on the `P5` contract.

- [ ] `P6-1` environments + `apiBaseUrl`
- [ ] `P6-2` Models mirroring API DTOs
- [ ] `P6-3` `ProductsApiService`, `VendingApiService`
- [ ] `P6-4` Error interceptor + `ERROR_MESSAGES`
- [x] `P6-5` `centsToCurrency` pipe
- [x] `P6-6` `_tokens.scss`, `_mixins.scss`, `_reset.scss`, dark mode
- [x] `P6-7` App shell + lazy routes `/` and `/products`
- [ ] `P6-8` Pipe + API service tests (pipe tests already exist from `P6-5`;
      the API-service half is still blocked on `P6-3`)

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
- `2026-09-15` — **`centsToCurrency` uses the `de-DE` locale**, already named
  in `CLAUDE.md` §5.3, exported once as `CURRENCY_LOCALE` from the pipe's own
  file rather than duplicated at each call site — `de-DE` renders EUR as
  `1,45 €` (comma decimal, period thousands separator, symbol after the
  amount), which most European reviewers will read correctly at a glance —
  rejected `en-IE`/`en-US`-style formatting (`€1.45`, decimal/thousands swapped
  from what most of the EU expects) even though both are valid ways to render
  the same currency.
- `2026-09-15` — **`templateUrl` everywhere, and `vending`/`products` each own
  a `<feature>.routes.ts`** lazy-loaded via `loadChildren` — repo owner asked
  for "separate modules with their own routing"; confirmed this means the
  standalone equivalent (own routing, own lazy-loading boundary), not literal
  `NgModule`s, which §5.2 already rules out — rejected
  `@NgModule`/`RouterModule.forChild()` (reverses that fixed decision). Added
  to §5.1/§5.2.
- `2026-09-15` — **`/` is now a landing page (`features/home/`) with two
  navigation cards**; vending moved `/` → `/vending` — matches what was asked
  for ("two centralised card-like buttons ... redirected to the module") —
  rejected keeping vending at `/` with the landing page on top of it, which
  would make `/` do two jobs instead of one each.
- `2026-09-16` — **`VendingMachine` aggregate root replaces the
  store/bank/session-plus-service-lock design** — one object owns the slots,
  the coin bank and the current session together, so atomicity falls out of
  the design (single unit of consistency) instead of being arranged by a lock
  in the Service layer — rejected keeping three separately-lockable stores
  coordinated by `VendingService`, which is exactly the shape that made
  rollback necessary in the first place.
- `2026-09-16` — **`Purchase` validates everything, *then* mutates — no
  rollback code, no try/catch** — find the slot, check availability, check
  funds, and ask the change calculator, all before touching any state; only
  once the calculator confirms exact change is possible does the aggregate
  move inserted coins into the bank, remove the change coins, dispense, and
  clear the session, in that order. A failed purchase is a no-op because
  nothing happened yet, not because anything was undone — rejected the
  original P1 plan of mutating optimistically and rolling back on
  `CHANGE_UNAVAILABLE`, which needs persistent/immutable state (`CoinBundle`)
  specifically to make rollback cheap and safe.
- `2026-09-16` — **`CoinBundle` (immutable, `Combine`/`TryRemove`) replaced by
  `CoinInventory` (mutable, `Add`/`Remove`/`AddAll`)** — the whole reason
  `CoinBundle` was a persistent immutable structure was to make rollback safe;
  with the aggregate now validating before mutating, there is nothing to roll
  back, so a plain mutable entity is simpler and there is no reason to keep
  both types around. `CoinInventory.Remove` throws `DomainException`
  (`CHANGE_UNAVAILABLE`) rather than `CoinBundle.Remove`'s
  `InvalidOperationException`, since its only real call site now is removing
  confirmed-available change coins from the bank inside `Purchase` — a
  `DomainException` fits `CHANGE_UNAVAILABLE`'s own §3.3 code, and the
  ambiguity that justified a plain BCL exception in P1 (which of ten
  unrelated codes fits an unspecified caller) no longer applies now that
  there's exactly one caller with a specific meaning.
- `2026-09-16` — **`VendingMachine` now enforces slot product-id uniqueness
  and price distinctness itself** (`DUPLICATE_PRODUCT` / `DUPLICATE_PRICE`),
  superseding the P1 decision that deferred both to `ProductService` — with
  the aggregate owning the whole slot collection, these are no longer
  cross-entity rules a single entity can't see. **Name uniqueness
  (`DUPLICATE_PRODUCT` by name, case-insensitive) is deliberately NOT moved
  here** — the aggregate has no concept of name identity, only product ids
  and prices, and adding one only to satisfy this one rule would mean
  `VendingMachine` doing catalogue-shaped work it doesn't otherwise need;
  that check stays in `ProductService` (P3), which already has to look at
  every product's name for the same reason. Flagged explicitly in case this
  reading of "enforce both" (ids + prices, not names + prices) isn't what was
  intended.
- `2026-09-16` — **Name uniqueness moved into `VendingMachine` after all**,
  superseding the P1-reopening entry above that deliberately kept it in
  `ProductService` — explicit instruction for P3: "add it to the aggregate
  rather than here, so all collection invariants live in one place." The
  aggregate's `EnsureSlotInvariants` now checks id, name (case-insensitive)
  and price together in one pass; `ProductService` does no uniqueness
  checking of its own, only mapping and `PRODUCT_NOT_FOUND` translation for
  reads.
- `2026-09-16` — **`catalogue.seed.json` carries no `quantity` field** — every
  slot's starting stock instead comes from configuration
  (`VendingMachine:InitialQuantityPerSlot`), applied uniformly by
  `VendingMachine.Load`. An external product catalog is a source of *product*
  data (name, price, image) — it has no way to know this specific machine's
  physical stock levels, and baking a quantity into it would blur "external
  catalog" and "this machine's inventory" into one concept — rejected storing
  quantity in the seed file (the two are genuinely different data: one
  travels with the product, the other belongs to a specific machine
  instance and is reset by `Reload`/restart either way).

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
- `2026-09-15` — Pulled `P6-5`/`P6-6`/`P6-7` forward (see the P6 heading note)
  since they're contract-independent. `centsToCurrency`
  (`core/pipes/cents-to-currency.pipe.ts`): a module-scope `Intl.NumberFormat`,
  `de-DE` locale exported once, null/undefined → `''`, a dev-mode-only
  `console.warn` on non-integer input (still formats rather than throwing).
  Tests assert the real `Intl` output byte-for-byte, including the U+00A0
  non-breaking space before `€` (verified the exact codepoints with a Node
  probe first — `0,00 €` etc. — rather than guessing). SCSS token system
  in `src/styles/`: `_tokens.scss` (semantic custom properties — surface/text/
  accent/etc., `--space-1`…`--space-8`, a type scale, radius, shadow,
  transition-duration — redefined under `prefers-color-scheme: dark`, plus a
  `$breakpoints` Sass map), `_mixins.scss` (`respond-to()` over that map,
  min-width only), `_reset.scss` (box-sizing, margin reset, img/svg block,
  form elements inherit font, `prefers-reduced-motion`); root `styles.scss`
  forwards all three. Added `stylePreprocessorOptions.includePaths:
  ["src/styles"]` to `angular.json` so any component can `@use 'mixins'`
  without relative-path climbing. App shell: `App` (`app.ts`/`.html`/`.scss`)
  now renders a header (title + `routerLink`/`routerLinkActive` nav) and a
  `max-width`/`--space-4`-gutter container around `<router-outlet>`; two
  `loadComponent` lazy routes at `''` and `/products` point at new
  `features/vending/vending-page.ts` and `features/products/products-page.ts`
  placeholders (title-only, under 10 lines, replaced wholesale in `P7`/`P8`).
  `npm run lint`/`build`/`test:ci` all green (14/14 specs). Verified live with
  a scripted Chrome session (`puppeteer-core` against the system Chrome,
  installed with `--no-save` and removed again afterwards — never touched
  `package.json`): both routes navigate via the nav links, zero horizontal
  overflow (`scrollWidth === clientWidth`) at 320/360/768/1024/1440px, and
  `prefers-color-scheme: dark` flips every custom property with no unreadable
  text (screenshots inspected at 1024px in both themes, plus 320/360px
  light). Contrast, read back from the browser's own computed custom
  properties: light text/surface 17.35:1, text-muted/surface 7.39:1; dark
  text/surface 16.14:1, text-muted/surface 8.66:1 — all ≥4.5:1, focus-ring
  accent against both surface tones sits at 5.3–7.5:1 (≥3:1 non-text
  minimum). Does not close P6 — `P6-1`…`P6-4`/`P6-8` remain.
- `2026-09-15` — Amended `P6-7` per repo-owner follow-up (rationale in
  decision log): `vending-page`/`products-page` now use `templateUrl` + a
  sibling `.html`; each feature owns a `<feature>.routes.ts`, lazy-loaded via
  `loadChildren` (confirmed as its own chunk in the build output, not just
  file organisation). Added `features/home/` — a `/` landing page with two
  `routerLink` cards; vending moved `/` → `/vending`; the shell title is now
  a link back to `/`. Updated CLAUDE.md §5.1/§5.2 and the plan's P6-7 line to
  match. `lint`/`build`/`test:ci` green (16/16). Verified live again
  (`puppeteer-core`, `--no-save`, removed after): each card lands on the
  right URL/`<h1>`/active nav state, title returns to `/`, still zero
  horizontal overflow at 320–1440px. A screenshot made the card description
  look off-grey; computed colour checked out as exactly `--text-muted` —
  compression artefact, not a bug. P6 still not closed.
- `2026-09-16` — Reopened P1 for an aggregate-root restructuring
  (`P1-6`/`P1-7`/`P1-8`) ahead of P3, Domain/Domain.Tests only: `Slot` entity
  (quantity moved off `Product`, which is now pure catalogue data);
  `CoinBundle` deleted, replaced by mutable `CoinInventory`; new
  `VendingMachine` aggregate root owning slots + bank + session, with a
  compute-then-commit `Purchase` (validate fully, mutate once, no rollback
  code) and `IChangeCalculator`/`ChangeResult` declared for P2 to implement.
  Moved (not duplicated) the quantity-boundary/`DecrementStock`/`SetQuantity`
  tests from `ProductTests` to new `SlotTests`; wrote `VendingMachineTests`
  with a hand-written `FakeChangeCalculator` covering Load validation, the
  full purchase decision tree (happy path, exact money, insufficient funds,
  out of stock, product not found, change unavailable — each proving slot/
  bank/session are untouched on failure), reset, and the
  `paid == price + change` invariant. `VM.Server.Domain.csproj` still zero
  package/project references. 63 Domain tests, solution-wide `dotnet
  build`/`test` green (65 total). Flagged one reading call in the decision
  log (id+price enforced in the aggregate, name uniqueness deliberately left
  in P3) since the task's own wording was internally ambiguous on this point.
  Updated `CLAUDE.md` §2.3/§2.4/§2.5/§4.1. Does not start P3.
- `2026-09-16` — P3 (`P3-1`…`P3-6`), Service + Repository + their tests, done
  ahead of P2: `IVendingMachineStore`/`IExternalCatalogSource` abstractions;
  `catalogue.seed.json` (6 products, 85-245c, no quantity — see decision
  log); `FileExternalCatalogSource` (read-only, fails loudly on missing/bad
  JSON); `InMemoryVendingMachineStore` (single `SemaphoreSlim` guards lazy
  once-only load *and* every subsequent read/mutation — `AccessAsync`/
  `ExecuteAsync`/`ReloadAsync`), config-bound via `IOptions<VendingMachineOptions>`;
  `ProductService` (CRUD as pure mapping/orchestration over the aggregate,
  zero validation logic of its own). One authorized Domain change: moved
  name-uniqueness into `VendingMachine.EnsureSlotInvariants` per this task's
  explicit instruction (supersedes the P1-reopening decision that kept it in
  Service). 15 new Service.Tests incl. the SHA-256 byte-identical-seed-file
  proof and a real concurrent-first-call race test; 65 Domain tests
  unaffected (2 more added for the name-uniqueness change). Solution-wide
  `dotnet build`/`test` green (81 total: 65 + 15 + 1 API placeholder).
  Updated `CLAUDE.md` §2.6/§4.1 and README's Design notes/Configuration
  table. P2 still open — flagged in Next up since P4's `Purchase` needs a
  real `IChangeCalculator`, not just the interface.
