# Task Progress

Live state of the project. **Read this first in every session.** Update it in
the same commit as the work it describes. Rules: `CLAUDE.md` §7.

**Status:** `[ ]` not started · `[~]` in progress · `[x]` done · `[!]` blocked · `[-]` dropped

---

## Next up

1. `P4-1` … `P4-7` — vending use cases
2. `P5-1` … `P5-6` — HTTP API
3. `P6-2`, `P6-3`, `P6-8` — the frontend pieces still blocked on the P5 contract

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

### P2 — Change calculation `[x]`

Executed after P3 (see P3's session log for why) - the interface and result
type P1-7 declared are now actually implemented.

- [x] `P2-1` `ChangeResult` — `Made`/`NotPossible` factories (renamed from
      P1-7's `Success`/`Failure` to match this task), `TotalCents` added;
      `IChangeCalculator`'s signature confirmed unchanged from P1-7
- [x] `P2-2` `BoundedChangeCalculator` — bounded coin-change DP exactly as
      specified (denominations ascending, min coins, deterministic
      larger-denomination tie-break — see decision log for how)
- [x] `P2-3` Class-level comment with the worked 60c-from-{50:1,20:3}
      counterexample
- [x] `P2-4` 13 tests: boundary table (amount 0, exact coin, impossible,
      empty bank, bank smaller than amount), the named greedy-counterexample
      test, minimal-coin-count, unaccepted-denomination filtering, purity,
      determinism, a Stopwatch-measured performance test (500c/200 coins:
      ~2ms, budget 50ms), and a range-of-amounts property test
- [x] `P2-5` Wired the real calculator into the `VendingMachine` purchase
      tests (P1-7), replacing the hand-written fake everywhere except the
      one `CHANGE_UNAVAILABLE` test, which still needs a fake that always
      reports impossible to exercise that path deterministically. Added
      `SpyChangeCalculator` (wraps the real calculator, records the last
      call's arguments) so the "offers bank + inserted coins to the
      calculator" test could use real computation too, not just a stub.

### P3 — Service layer: products `[x]`

`IVendingMachineStore`/`IExternalCatalogSource`, `FileExternalCatalogSource`,
`InMemoryVendingMachineStore`, `ProductService`, 15 tests (incl. byte-identical
seed file and a real concurrent-first-call race). `P3-1`…`P3-6` all done,
executed ahead of P2 (see decision/session log for the store/name-uniqueness
calls) — see `CLAUDE.md` §2.6/§4.1.

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

### P6 — Frontend foundation `[~]`

`P6-5`/`P6-6`/`P6-7` pulled forward ahead of `P1`…`P5` since they don't touch
the API contract (no models, no HTTP calls). `P6-1`/`P6-4` pulled forward the
same way — environments and the error interceptor are pure client-side
plumbing that doesn't need P5 either. `P6-9` (shared UI primitives) is a new
task, pulled forward from what P7/P8 will need, for the same contract-free
reason. Only `P6-2`/`P6-3`/`P6-8` remain, and all three are genuinely blocked
on the `P5` contract (they mirror/call the actual API DTOs).

- [x] `P6-1` environments + `apiBaseUrl`
- [ ] `P6-2` Models mirroring API DTOs
- [ ] `P6-3` `ProductsApiService`, `VendingApiService`
- [x] `P6-4` Error interceptor + `ERROR_MESSAGES`
- [x] `P6-5` `centsToCurrency` pipe
- [x] `P6-6` `_tokens.scss`, `_mixins.scss`, `_reset.scss`, dark mode
- [x] `P6-7` App shell + lazy routes `/` and `/products`
- [ ] `P6-8` Pipe + API service tests (pipe tests already exist from `P6-5`;
      the API-service half is still blocked on `P6-3`)
- [x] `P6-9` Shared UI primitives (`vm-button`, `vm-badge`, `vm-modal`,
      `vm-confirm-dialog`, `vm-empty-state`) — pulled forward from P7/P8's
      dependencies

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
- `2026-09-16` — **`CLAUDE.md` §3.3's error code list was missing
  `INVALID_PRODUCT`** (live in `ErrorCodes.cs` and used by `Product`/P3 since
  the P1 reopening, but never added to the doc) — caught while building the
  frontend `ErrorCode` union off §3.3 and cross-checking it against the
  actual backend source rather than trusting the doc alone; added it to both
  §3.3 and the frontend union. A stale contract doc is worse than a missing
  one, since it looks authoritative.
- `2026-09-16` — **`apiBaseUrl` is `''` in both `environment.ts` and
  `environment.development.ts`**, not just dev — this task said dev must be
  empty (the proxy handles it), but was silent on production; since nothing
  in the project defines a separate production deployment topology (frontend
  and API assumed same-origin), an empty string is the simplest default that
  works either way — rejected hard-coding `http://localhost:5080` anywhere
  (explicitly ruled out) and rejected inventing a production URL with
  nothing to point it at. Also fixed a stale README config row that had
  claimed the default was `http://localhost:5080`.
- `2026-09-16` — **`ApiError.message` keeps the server's/transport's raw
  message; `ERROR_MESSAGES[code]` is the only thing ever shown to a user** —
  keeps "data" (what actually happened, for logs/devtools) and "presentation"
  (what CLAUDE.md §5.3 allows on screen) as two separate concerns per the
  task's own split into two deliverables (an `ApiError` type and a separate
  `ERROR_MESSAGES` map) — rejected overwriting `message` with the friendly
  text in the interceptor, which would have thrown away the original server
  message with nowhere left to log it.
- `2026-09-16` — **`vm-modal` adds its own Tab-wrap focus trap** rather than
  relying solely on native `<dialog>`/`showModal()` — verified live (real
  keyboard input via a scripted Chrome session, not synthetic DOM events,
  since focus-navigation isn't scriptable that way) that Chrome's native
  modal containment stops focus from reaching background content but does
  **not** wrap it: tabbing off the last focusable element inside the dialog
  landed on `<body>` instead of cycling back to the first element. Added an
  explicit `(keydown.Tab)` handler that wraps at both ends — confirmed fixed
  with the same live check afterward. Escape-to-close, initial focus
  placement and outside-content containment are still left entirely to the
  browser, since those parts do work natively.
- `2026-09-17` — **`BoundedChangeCalculator` stays the straightforward
  O(denominations × amount × count) DP** — six denominations, change under a
  few hundred cents, counts in the low hundreds — the measured worst case in
  this repo's own performance test is ~2ms for 500c against 200 coins, four
  orders of magnitude under the 50ms budget. A logarithmic-time approach
  (binary/power-of-two splitting the per-denomination counts to shrink the
  inner loop) would earn its complexity at a much larger scale, but here it
  would only make the algorithm harder for a reviewer to verify by reading —
  and correctness-by-inspection is worth more than headroom nobody needs.
  Rejected switching algorithms pre-emptively for input sizes this domain
  will never see.
- `2026-09-17` — **Denominations are fed to the DP in ascending order**, with
  a tie-break that keeps the *largest* `k` among equally-good candidates at
  each layer — this is not an arbitrary choice: because ascending order
  means the largest denomination (200c) is decided *last*, its choice is
  made with full knowledge of the true optimal cost for every smaller
  remainder (already computed), so "prefer more of the current denomination
  on a tie" only ever fires at that point in a way that correctly favours
  larger denominations globally, not just locally. Verified by hand against
  a constructed tie (60c from `{20:3, 50:1, 5:2}` with no 10c/100c/200c
  available — two different 3-coin solutions exist, `{20:3}` and
  `{50:1,5:2}`; the algorithm returns the latter, which has strictly more of
  the larger 50c denomination). Rejected descending order, which would put
  this same tie-break on the *smallest* denomination's decision instead and
  produce the opposite (wrong) preference.

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
- `2026-09-16` — P6-1/P6-4/P6-9, pulled forward (contract-free, see the P6
  heading note): `environment.ts`/`environment.development.ts`
  (`apiBaseUrl: ''` both, wired via `angular.json`'s `development`
  `fileReplacements`); `core/api/api-error.ts` (`ErrorCode` — all ten §3.3
  codes, caught and fixed one missing from the doc, `INVALID_PRODUCT` — plus
  client-only `NETWORK_ERROR`/`UNKNOWN_ERROR`) and `error-messages.ts`
  (`ERROR_MESSAGES satisfies Record<ErrorCode, string>`, vending-machine
  voice, `getErrorMessage()` fallback); `error.interceptor.ts` distinguishing
  a well-formed §3.3 body, an unrecognised one, and `status === 0`, wired
  into `app.config.ts` via `provideHttpClient(withFetch(),
  withInterceptors(...))`. Five `shared/ui/` primitives (`vm-button`,
  `vm-badge`, `vm-modal`, `vm-confirm-dialog`, `vm-empty-state`), each
  standalone/OnPush/`input()`/`output()`, styled only from P6-6 tokens (grepped
  `shared/ui/` for hex/`rgb()`/`@media` — zero hits; the only literal `px`
  values left are 1-2px border/outline widths, which aren't spacing-scale
  values). `vm-modal` wraps a native `<dialog>` — added `--danger-contrast`,
  `--overlay-color`, `--touch-target-min`, `--measure-max-width` tokens it
  needed. Found (live, see decision log) that native `showModal()` doesn't
  wrap Tab at the dialog's boundaries, so added an explicit trap; re-verified
  live afterward that it does now, that Escape still closes both `vm-modal`
  and `vm-confirm-dialog` and returns focus to the trigger, and that all five
  primitives render correctly in light and dark (screenshots at both, plus
  the two dialogs open). Verification used a temporary showcase wired into
  `home-page`/`puppeteer-core` (`--no-save`), both fully reverted before
  committing — confirmed `git status`/`home-page`'s bundle size are back to
  their pre-showcase state. `lint`/`build`/`test:ci` green (45/45; two modal
  tests were flaky until rewritten to await the real `close` event instead of
  a `setTimeout` racing the same browser-internal queued task). Fixed
  `CLAUDE.md` §3.3 and a stale README config row along the way (see decision
  log). P6 now `[~]`; `P6-2`/`P6-3`/`P6-8` remain, blocked on P5.
- `2026-09-17` — P2 (`P2-1`…`P2-5`), Domain + Domain.Tests only:
  `BoundedChangeCalculator` (bounded coin-change DP, exact algorithm from the
  task spec - ascending denominations, deterministic largest-denomination
  tie-break, no denomination outside `CoinDenominations` ever surfaces even
  if the input dictionary has one). `ChangeResult`'s factories renamed
  `Success`/`Failure` → `Made`/`NotPossible` to match this task's P2-1 spec
  (the only caller was `FakeChangeCalculator`, updated); `IChangeCalculator`
  unchanged. Wired the real calculator into the P1-7 purchase tests
  (`Calculate_WhenGreedyWouldStrand_FindsTheCorrectCombination` is the named
  proof the counterexample works); kept one fake for the deterministic
  `CHANGE_UNAVAILABLE` path and added `SpyChangeCalculator` so the
  "offers bank + inserted coins" test could use real computation and still
  inspect what was passed. 13 new calculator tests incl. a Stopwatch-measured
  performance test (500c/200 coins in ~2ms, budget 50ms) and a
  range-of-amounts property test. 95 tests total across the solution, all
  green. See decision log for the algorithm-choice and tie-break-correctness
  reasoning. Closes P2.
