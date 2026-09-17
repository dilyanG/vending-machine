# Task Progress

Live state of the project. **Read this first in every session.** Update it in
the same commit as the work it describes. Rules: `CLAUDE.md` §7.

**Status:** `[ ]` not started · `[~]` in progress · `[x]` done · `[!]` blocked · `[-]` dropped

---

## Next up

1. `P9-1` … `P9-9` — polish, verification, handover (now running against the
   post-service-layer-refactor code — see the new phase above P9)

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

- [-] `P1-1` `Product` entity's own guards (`Create`/`Restore` validating
      name/price) — **superseded by the service-layer refactor** (2026-09-18,
      see decision log): moved to `ProductValidationService`; `Product` is now
      plain data with no factories or validation of its own.
- [x] `P1-6` `Slot` entity (product + quantity, `Dispense`/`Restock`);
      `Quantity` removed from `Product`, which is now pure catalogue data —
      **superseded by the service-layer refactor** (2026-09-18): `Dispense`/
      `Restock`'s validation moved to `ProductValidationService`/`VendingService`;
      `Slot` is now plain data.
- [-] `P1-3` `CoinBundle` immutable value object — **superseded by `P1-8`**:
      the aggregate root makes rollback (and therefore persistent/immutable
      coin collections) unnecessary; replaced by mutable `CoinInventory`
- [x] `P1-7` `VendingMachine` aggregate root: owns slots, bank and session;
      compute-then-commit `Purchase` (no rollback code); `Reset`; slot CRUD
      for P3; id-uniqueness/price-distinctness enforced across slots;
      `IChangeCalculator`/`ChangeResult` declared (not implemented — P2) —
      **superseded by the service-layer refactor** (2026-09-18): all behaviour
      (`Purchase`/`InsertCoin`/`Reset`/slot CRUD/uniqueness enforcement) moved
      to `VendingService`/`ProductService`/`MachineStateService`/
      `ProductValidationService`; `VendingMachine` is now plain data (slots +
      bank + session), no longer an aggregate root in the DDD sense.
- [x] `P1-8` `CoinInventory` mutable entity, replacing `CoinBundle`, used for
      both the bank and the session's inserted coins — **superseded by the
      service-layer refactor** (2026-09-18): `Add`/`Remove`/`AddAll`/`From`
      and their validation moved to `VendingService`/`MachineStateService`;
      `CoinInventory` is now a plain `Counts` dictionary wrapper.

### P2 — Change calculation `[x]`

Executed after P3 (see P3's session log for why). `BoundedChangeCalculator`
(bounded coin-change DP, ascending denominations, deterministic
larger-denomination tie-break), `ChangeResult` `Made`/`NotPossible`
factories, the named 60c-from-{50:1,20:3} greedy-counterexample proof, 13
tests incl. a Stopwatch performance test. `P2-1`…`P2-5` all done — see
decision log for the algorithm/tie-break reasoning.

- [-] `P2-2` `BoundedChangeCalculator` implemented inside `VM.Server.Domain/Services`
      — **superseded by the service-layer refactor** (2026-09-18, see decision
      log): moved verbatim (same DP, same comment) to
      `VM.Server.Service.Vending.ChangeCalculationService`; `Domain/Services`
      no longer exists. The algorithm and its 13 tests are unchanged.

### P3 — Service layer: products `[x]`

`IVendingMachineStore`/`IExternalCatalogSource`, `FileExternalCatalogSource`,
`InMemoryVendingMachineStore`, `ProductService`, 15 tests (incl. byte-identical
seed file and a real concurrent-first-call race). `P3-1`…`P3-6` all done,
executed ahead of P2 (see decision/session log for the store/name-uniqueness
calls) — see `CLAUDE.md` §2.6/§4.1.

### P4 — Service layer: vending `[x]`

Much smaller than `IMPLEMENTATION_PLAN.md` describes — the P1 aggregate
refactor already absorbed most of it (`P4-1`/`P4-2`/`P4-6`/`P4-7` superseded,
see decision log). What remained: `VendingService`
(`InsertCoinAsync`/`PurchaseAsync`/`ResetAsync`, `P4-3`…`P4-5`), each one
aggregate call plus DTO mapping; `AddVendingMachineBackend` DI (`P4-8`); 6
mapping-focused tests (`P4-9`). `P4-3`…`P4-5`/`P4-8`/`P4-9` done.

### P5 — HTTP API `[x]`

**Contract frozen: 2026-09-16.** Every route, payload and error shape in
`CLAUDE.md` §3 is now live and tested; the frontend can build against it from
`P6-2`/`P6-3` onward. `ExternalEndpoints`/`ProductEndpoints`/
`VendingEndpoints` (Minimal API, `MapGroup`), `ExceptionHandlingMiddleware`
(§3.3 shape, full status table), DI/CORS/OpenAPI wiring, a CDN-loaded
Swagger UI at `/swagger` in Development, 26 `WebApplicationFactory`
integration tests (every route, every error code, isolation via a fresh
factory per test class), six placeholder product SVGs. `P5-1`…`P5-7` all
done — see decision log for the config-binding bug caught during manual
verification.

### P6 — Frontend foundation `[x]`

Environments, models (`product.model.ts`/`coin.model.ts`), three API services
(`Products`/`Vending`/`ExternalCatalogApiService`), error interceptor +
`ERROR_MESSAGES`, `centsToCurrency` pipe, SCSS token system, app shell +
lazy routes, five `shared/ui` primitives. `P6-1`…`P6-9` all done — most
pulled forward of `P1`…`P5` since they're contract-independent (see decision
log); `P6-2`/`P6-3`/`P6-8` done last once `P5` froze the contract, verified
against the real DTOs/live OpenAPI doc/curl responses, not `CLAUDE.md` §3
alone (see decision/session log for the `insertedCoins` field-name finding).

### P7 — Vending UI `[x]`

`vending.store` (busy-gated `insertCoin`/`purchase`/`reset`, response applied
in place, never refetched), `vm-machine-display` (the one `aria-live`
region), `vm-coin-slot` (denominations from the API, proportionally-sized
circles), `vm-product-grid`/`vm-product-card` (three stock states plus an
unaffordable-but-buyable one — Buy is `aria-disabled`, never natively
`disabled`, so out-of-stock is still reachable/explained to AT users),
`vm-change-tray` (per-denomination breakdown, focus-on-purchase), the
`ResizeObserver`-measured sticky mobile coin panel, and loading/empty/error
states. `P7-1`…`P7-9` all done — 22 tests (8 store + 4 `vm-product-card` +
others). See decision/session log for the `vm-button` extension, the
`denominationLabel` pipe, and the live-verified `CHANGE_UNAVAILABLE`/
sticky-panel-overlap findings.

### P8 — Products admin UI `[x]`

- [x] `P8-1` `products.store` (`core/state/products.store.ts`) — same
      private-signal/`.asReadonly()` shape as `vending.store`, kept
      deliberately separate (different concerns, same entity). Mutating
      methods return the underlying `Observable` (not auto-subscribed) so
      the page can react to *that specific* submission's outcome for field
      -level error mapping, while the store still applies the result via
      `tap` — list never refetched after create/update/delete. `reload()` is
      the one exception: `POST /api/products/reload` returns 204 (no body to
      apply), so it `switchMap`s into a follow-up `GET` — see decision log
- [x] `P8-2` `vm-product-table` — a real `<table>` (md+) and a card `<ul>`
      (below md) both always rendered, toggled by plain CSS `display`, not
      one table reflowed with ARIA-role overrides
- [x] `P8-3` `vm-product-form-dialog` — typed reactive form in `vm-modal`,
      euro-entry price converted with `Math.round(parseFloat(v) * 100)` (not
      truncated), validators mirroring the server (positive, multiple of 5,
      quantity 0–15) plus a client-only duplicate-price convenience check
- [x] `P8-4` Delete via `vm-confirm-dialog`, danger variant, names the
      product; focus-return to the row's Delete button comes free from
      `vm-modal`'s existing trigger-focus-restore (P6-9) — confirmed live,
      not assumed
- [x] `P8-5` Reload confirm dialog (states plainly that the external
      catalogue is never modified) plus a post-reload count message; an
      optional read-only external-catalogue disclosure panel via
      `ExternalCatalogApiService`, injected directly in the page (not
      through a store — see decision log)
- [x] `P8-6` `DUPLICATE_PRODUCT`→name, `DUPLICATE_PRICE`/`INVALID_PRICE`→price,
      `INVALID_QUANTITY`→quantity, each via `setErrors({..., server: msg})`
      + `markAsTouched()` so it's visible immediately; anything else falls
      back to a dialog-level message; the dialog never closes itself on a
      failed submission
- [x] `P8-7` 16 validator specs (euro-to-cents for 1.45/2.30/0.85/19.99/0.05,
      round-trip, 1.43 rejected/1.45 accepted) + 7 component specs (quantity
      16/0/15, `DUPLICATE_PRICE` on the price field with values preserved,
      the price round-trip through the real dialog) + 7 store specs
      (create/update/delete/reload without refetch, busy guard)

### Service-layer refactor (backend, run before P9) `[x]`

Not part of `IMPLEMENTATION_PLAN.md`'s original phase numbering — an explicit
repo-owner request to flatten `VM.Server.Domain` to pure data and move every
rule/calculation/state transition into `VM.Server.Service`, executed after P8
and before P9 (P9 verifies final state, so it needed to run against the
post-refactor code). Task ids below are new (`R-1`…`R-6`, not `P`-numbered,
to avoid colliding with or renumbering the real plan); the domain tasks it
supersedes (`P1-1`, `P1-6`, `P1-7`, `P1-8`, `P2-2`) are marked `[-]` in place
above, not deleted.

- [x] `R-1` Flattened `Product`/`Slot`/`CoinInventory`/`VendingMachine`/
      `PurchaseResult` to plain data carriers (public properties, `internal set`
      on mutable ones, no factories/guards/behaviour methods); deleted
      `Domain/Services/` (`BoundedChangeCalculator`/`ChangeResult`/
      `IChangeCalculator`) entirely. Added `Domain/AssemblyInfo.cs`
      (`InternalsVisibleTo` scoped to `VM.Server.Service`/`VM.Server.Repository`/
      `VM.Server.Service.Tests` only) so the API layer cannot bypass a service
      by assigning a property directly. `CoinDenominations`/`ErrorCodes`/
      `DomainException` kept as-is (already pure data).
- [x] `R-2` `ChangeCalculationService` (Service/Vending) — the DP algorithm
      moved verbatim, same greedy-counterexample comment, implementing a new
      `IChangeCalculator` now owned by Service.
- [x] `R-3` `ProductValidationService` (Service/Products) — name/price/quantity
      validation plus `EnsureUnique` (id/name-ci/price, previously the
      aggregate's `EnsureSlotInvariants`), the one place every product/slot
      rule lives.
- [x] `R-4` `MachineStateService` (Service/State, new folder — chosen over
      folding into the store so the store stays purely mechanical) —
      `LoadMachineAsync`: pulls the catalogue, validates/seeds the bank,
      assigns every slot the configured initial quantity, enforces uniqueness
      across the whole set. `InMemoryVendingMachineStore` now only owns *when*
      to load (lazy/`SemaphoreSlim`-guarded/reload), not *how*.
- [x] `R-5` `VendingService` rewritten to own `InsertCoin`/`Purchase`/`Reset`
      directly against the plain-data `VendingMachine`, preserving the
      compute-then-commit `Purchase` ordering exactly (find slot → check
      stock → check funds → compute change → mutate only once all four pass) —
      still no rollback code anywhere. `ProductService` rewritten to delegate
      every rule to `ProductValidationService` before mutating `machine.Slots`
      directly.
- [x] `R-6` Deleted `VM.Server.Domain.Tests` and `VM.Server.API.Tests`
      (removed from `VM.Server.slnx`); ported every behavioural assertion into
      `VM.Server.Service.Tests` against the new service APIs — see decision
      log for the two cases that couldn't be ported as-is and why, and for the
      API-test-coverage trade-off now on the record. 85 tests total, all
      green; solution-wide `dotnet build` clean (0 warnings — `TreatWarningsAsErrors`
      still holds). Full curl happy path re-run live and diffed against the P5
      baseline (denominations/products/insert/session/purchase/insert/reset/
      404/422) — byte-identical apart from fresh GUIDs, confirming no
      behaviour change.

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
- [x] `P9-10` `vm-button` gained a `warning` variant (filled light-yellow/dark
      text, muted amber in dark mode) and three new tokens
      (`--warning-surface`/`--warning-border`/`--warning-text`, both themes);
      wired to the products-admin Edit button (table row + mobile card, both
      layouts) so it no longer reads as inactive next to Delete. Text/fill
      contrast 8.54:1 light, 8.38:1 dark — both measured from the live
      page's own computed styles, not assumed from the token values on
      paper
- [x] `P9-11` `vm-modal` centering fixed at the primitive (both the
      add-product and reload-from-catalogue dialogs share the fix): the
      global reset's `* { margin: 0 }` was silently cancelling native
      `<dialog>`'s own UA-stylesheet `margin: auto` centering — reasserted
      on `.vm-modal`. `max-height` moved from a `vh`-based calc to `85dvh`.
      Found and fixed two knock-on bugs while verifying live, neither of
      which would have been caught by inspection alone: (1) `.vm-modal__panel`'s
      `max-height: 100%` silently resolved to nothing against the dialog's
      `auto` height (percentage heights need a *definite*-height ancestor),
      so the footer overflowed past the dialog's own bottom edge instead of
      the body scrolling internally — fixed by making `.vm-modal[open]` a
      flex container so the panel stretches via flex sizing instead; (2)
      that fix's first attempt (`display: flex` unscoped) made every
      *closed* dialog render inline in the page, since an unscoped author
      rule beats the UA stylesheet's `dialog:not([open]) { display: none }`
      regardless of specificity — rescoped to `.vm-modal[open]`. Skipped the
      "two vending-page buttons" item from the same request — investigated
      the page and found `vm-product-card` has exactly one button, no clean
      pair of "two buttons on a card" exists; repo owner said to skip it
      rather than guess
- [x] `P9-12` Two Mermaid diagrams added under `docs/diagrams/`:
      `vending-state-machine.md` (insert/purchase/reset, every refusal path
      with its real error code, why a refusal preserves the session) and
      `change-calculation.md` (the atomic purchase sequence, the bounded
      coin-change DP's shape, the greedy counterexample). `docs/diagrams/README.md`
      indexes both; linked from the main README's Design notes; `CLAUDE.md`
      §8 now requires updating the matching diagram in the same commit as any
      vending-transition or change-algorithm change. Both diagrams verified
      to actually render (`@mermaid-js/mermaid-cli`), not just eyeballed —
      caught and worked around a real Mermaid `stateDiagram-v2` bug in the
      process (silently collapses repeated self-loops on one state to the
      last one defined; see the diagram file's own note and the decision
      log).

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
- `2026-09-18` — **The P1 aggregate refactor shrank P4 to a thin mapping
  layer**, well below what `IMPLEMENTATION_PLAN.md` originally scoped for it
  — worth recording explicitly since a reviewer comparing the plan to the
  code will otherwise wonder what happened to four of its seven tasks.
  Absorbed: `P4-1` (`ICoinBank`/`InMemoryCoinBank`) — the `VendingMachine`
  aggregate owns `Bank` directly; `P4-2` (`VendingSession`) — the aggregate
  owns `InsertedCoins`/`InsertedTotalCents` directly; `P4-6` (a lock over
  session+bank+inventory) — done once, in `InMemoryVendingMachineStore`'s
  `SemaphoreSlim` (P3-4), which already guards every call whether P3's
  `ProductService` or P4's `VendingService` makes it; `P4-7` (atomicity +
  `paid == price + change` tests) — already proven directly on the
  aggregate in P1-7/P2-5, where the invariant actually lives. What
  remained for P4 itself: `VendingService` (`P4-3`/`P4-4`/`P4-5`), each
  method one aggregate call plus DTO mapping, no business logic; `P4-8` (DI)
  and `P4-9` (tests for the mapping only) were added since the original plan
  didn't anticipate a composition root existing yet at this point.
- `2026-09-18` — **`ServiceCollectionExtensions.AddVendingMachineBackend`
  lives in `VM.Server.Repository`, not `VM.Server.API`** — this task assumed
  P3 had already established a composition-root extension method to extend,
  but P3's scope explicitly excluded API/DI entirely, so nothing existed;
  this is the first one in the solution. Repository is the only project that
  can see both Service's interfaces and its own implementations of them
  (`API → Service → Domain`, `Repository` implements `Service`'s
  abstractions, per `CLAUDE.md` §4.1), so it can register the whole backend
  behind one call; P5's `Program.cs` will just invoke it — rejected putting
  the registrations in API directly, which would need to reference
  Repository's concrete types anyway and scatters wiring across two
  projects instead of one.
- `2026-09-16` — **Found and fixed a real P4 bug while doing P5's required
  manual curl verification**: `AddVendingMachineBackend` registered
  `InMemoryVendingMachineStore` but never called
  `services.Configure<VendingMachineOptions>(...)`, so `IOptions` silently
  fell back to an all-default instance — an **empty** coin bank in the
  actual running app (`InitialQuantityPerSlot` happened to still default
  correctly to 10, masking half the bug). Every unit test had bypassed this
  entirely by constructing `Options.Create(new VendingMachineOptions {...})`
  directly, so nothing caught it before a real HTTP purchase did. Fixed by
  binding in `API/Program.cs` instead of `Repository`: the
  `Configure<TOptions>(IConfiguration)` overload lives in
  `Microsoft.Extensions.Options.ConfigurationExtensions`, which a plain
  class library like `Repository` doesn't have, but which `API` gets for
  free via the ASP.NET Core shared framework (`Microsoft.NET.Sdk.Web`) —
  so no new package was needed. This is exactly why the phase's "start the
  API and run the full happy path with curl" step exists rather than
  trusting build-green/tests-green alone.
- `2026-09-16` — **API endpoints return the existing Service-layer DTOs
  directly** (`ProductDto`, `SessionDto`, `PurchaseResultDto`,
  `ReturnedCoinsDto`, `CoinCountDto`) rather than a parallel set of
  structurally-identical `API.Dtos.*` types — they already match §3.2's wire
  shapes exactly (camelCase via STJ defaults, quantity already assembled
  from the slot), so a second copy would be pure duplication mapped by a
  method that does nothing. New API-only DTOs were added only for shapes
  Service doesn't have a reason to own: `ExternalProductDto` (no quantity -
  a different shape from `ProductDto`, and mapping it is genuinely an API
  concern since `IExternalCatalogSource` returns raw `Product` entities) and
  the four request records (`Create`/`UpdateProductRequest`,
  `InsertCoinRequest`, `PurchaseRequest`), since Service's methods take
  primitives, not request objects.
- `2026-09-16` — **A CDN-loaded Swagger UI page at `/swagger` (Development
  only), not just the bare OpenAPI JSON from `MapOpenApi()`** — costs no new
  NuGet package (`swagger-ui-dist` loads from `cdn.jsdelivr.net` in a small
  static HTML page, gated behind `IsDevelopment()`), fulfills the README's
  pre-existing "Swagger UI" promise literally, and is genuinely useful for a
  reviewer who wants to click through the API rather than read raw JSON —
  rejected adding the Swashbuckle package (violates "no new packages") and
  rejected downgrading the README's claim to describe raw JSON only, when a
  real UI was achievable for free.
- `2026-09-17` — **Frontend models for P6-2 use `insertedCoins` for
  `VendingSession`**, not the `coins` name that task's own spec assumed —
  confirmed by reading `SessionDto` (`Service/Vending/SessionDto.cs`), the
  live OpenAPI schema, and a real `GET /api/vending/session` response, all
  three agreeing on `insertedCoins`. This is the one place the source of
  truth (actual wire format) disagreed with an assumption written into the
  task prompt itself, not with `CLAUDE.md` — the client models the wire
  shape exactly regardless of what a prompt guessed a field would be called.
  Added the previously-undocumented `GET /api/vending/session` payload to
  `CLAUDE.md` §3.2 while here, since it was the one endpoint response shape
  never actually written down.
- `2026-09-17` — **The live OpenAPI document is served at `/openapi/v1.json`,
  not `/swagger/v1/swagger.json`** (that path 404s; the CDN Swagger UI page
  built in P5 lives at `/swagger` and fetches the JSON from the correct URL
  itself) — a wrong guess in the task prompt, not a `CLAUDE.md` or code
  defect, so no doc change needed; noted here only so a future session
  doesn't repeat the wrong URL. Every schema in the real document
  (`SessionDto`, `ProductDto`, `PurchaseResultDto`, `ReturnedCoinsDto`,
  `CoinCountDto`, `ErrorResponseDto`, `ExternalProductDto`, both product
  request DTOs) matches the C# source exactly — confirmed no doc/code drift
  beyond the `insertedCoins` finding above.
- `2026-09-17` — **`vm-button` gained `ariaDisabled`/`ariaDescribedBy` inputs**
  rather than a local workaround in `vm-product-card` for the out-of-stock Buy
  button — P7-4 explicitly asked for `aria-disabled` (not native `disabled`)
  with an accessible reason, and P6-9's own usage note says to extend a
  primitive rather than route around it. Native `disabled` removes an element
  from the tab order entirely, which would hide the "why" from a keyboard/
  screen-reader user reaching for it; `ariaDisabled` keeps the button
  focusable and clickable and leaves it to the caller's click handler to
  ignore the click, while `ariaDescribedBy` points at a reason string —
  rejected only ever using native `disabled` (loses the explanation) and
  rejected duplicating a second button-like element in `product-card` instead
  of extending `vm-button` (violates the "extend, don't work around"
  instruction and would drift from the primitive's styling over time).
- `2026-09-17` — **`vm-product-card` derives "affordable" locally from an
  `insertedTotal` input plus its own `product().priceCents`**, rather than
  taking a precomputed boolean from the store's `canAfford()` — still
  presentational (driven by inputs, never injects `VendingStore`, per
  CLAUDE.md §5.2), and it lets the card also compute the exact shortfall
  amount for its hint text ("Insert €0.35 more") without a second input.
  `VendingStore.canAfford(id)` still exists (P7-1's own spec required it) and
  is store-tested directly; nothing currently reuses it beyond the store's own
  tests, which is fine — it is a genuine part of the store's public surface,
  not dead code.
- `2026-09-17` — **A `denominationLabel` pipe (`core/pipes/`), not
  `centsToCurrency`, labels individual coins** ("5c", "€1", "€2" — in the
  coin-slot circles and the change-tray breakdown) — CLAUDE.md §5.3 reserves
  `centsToCurrency`/`Intl.NumberFormat` for *formatted money amounts*
  (prices, totals); a coin's short glyph is a different kind of label
  entirely (matches the README's own denomination table style), and giving
  it a second small pipe keeps that distinction explicit rather than
  overloading the one pipe with a "short mode" flag. Totals (`changeCents`,
  `returnedTotalCents`, the machine display's `insertedTotal`) still go
  through `centsToCurrency` — only the per-coin breakdown lines use the new
  pipe.
- `2026-09-17` — **Three new layout tokens added to `_tokens.scss`**
  (`--sidebar-width: 20rem`, `--coin-size-max: 4.5rem`,
  `--product-image-size: 6rem`) rather than literal values in component
  SCSS — none of the existing spacing/radius/type-scale tokens fit these
  three structural sizes, and §5.4 bans magic numbers in component styles;
  extending the token set (precedent: P6-9 added `--touch-target-min` etc.
  the same way) keeps the "no literal values" rule intact without inventing
  a contrived `calc()` off an unrelated token. `--touch-target-min` (already
  44px, the WCAG minimum) doubles as the coin circles' *minimum* size, so the
  smallest coin (5c) is never smaller than an accessible touch target.
- `2026-09-17` — **`.visually-hidden` moved from a local declaration inside
  `home-page.scss` to a shared utility in the global `_reset.scss`** — P7's
  `vm-product-card` needed the same utility for its out-of-stock reason text,
  and duplicating a cross-cutting utility class per feature is exactly the
  kind of drift `_tokens.scss`/`_mixins.scss` already exist to prevent for
  colours and breakpoints; `_reset.scss` is already global and unencapsulated
  (Angular's emulated view encapsulation only scopes a component's *own*
  stylesheet, not the root `styles.scss` it forwards), so no component needs
  to `@use` anything new to get it.
- `2026-09-17` — **The mobile sticky coin panel's bottom-padding reservation
  is measured live via `ResizeObserver`, not a fixed guess** — the panel's
  height is genuinely dynamic (it grows with however many denominations the
  API returns, plus whether an error message or change-tray content is
  showing), so a hard-coded padding value would drift out of sync exactly
  when it mattered most; a signal-driven `effect()` on the page's own
  `viewChild` (same pattern as `vm-modal`'s focus trap) sets a
  `--vm-panel-height` custom property that the product grid's CSS reads,
  zeroed out above `lg` where the panel is a static side column instead —
  verified live by scrolling an actual page to its true document-bottom and
  confirming the last card's bottom edge cleared the panel's top edge, not
  by comparing raw unscrolled bounding-rect numbers (which looked like a
  false-positive "overlap" until re-checked at the real scroll position —
  see session log).
- `2026-09-17` — **`products.store`'s mutating methods return the underlying
  `Observable<T>` instead of being fire-and-forget like `vending.store`'s**
  — `vending.store` never needed a per-call result because nothing on the
  vending screen reacts differently per attempt; the product form dialog
  genuinely does (map *this* submission's `DUPLICATE_PRICE` onto the price
  field, keep the dialog open with the user's values intact). The store
  still applies the result to `products` via `tap` internally, so the
  "apply the response, don't refetch" rule holds either way — only the
  page additionally subscribes to route the per-call outcome into the
  dialog. Rejected keeping it void-returning like `vending.store` (the page
  would have no way to know which specific attempt failed) and rejected
  putting per-submission error state in the store itself (a `lastError`
  signal would need to be cleared at exactly the right moments and doesn't
  generalise to "this dialog's last attempt" cleanly the way a returned
  Observable does for free).
- `2026-09-17` — **`reload()` is the one `products.store` mutation that
  refetches instead of applying a response** — `POST /api/products/reload`
  returns `204 No Content` (CLAUDE.md's own P5 decision), so there is
  nothing to apply; `switchMap`s into a follow-up `GET /api/products`
  instead. Documented as a deliberate, narrow exception to the "apply the
  response, don't refetch" rule, not a drift from it.
- `2026-09-17` — **Extracted `stockBadgeVariant`/`stockLabel` out of
  `vm-product-card` into a shared `shared/ui/badge/stock-badge.ts`**, used
  by both `vm-product-card` (vending) and the new `vm-product-table`
  (products admin) — both needed the exact same
  out-of-stock/low-stock/in-stock thresholds and wording; keeping two copies
  risked exactly the kind of silent drift the rest of this project's shared
  tokens/mixins already guard against for colour and breakpoints. Behaviour
  -preserving: `vm-product-card`'s own tests still pass unchanged.
- `2026-09-17` — **`vm-product-table` renders a real `<table>` (md+) and a
  card `<ul>` (below md) simultaneously, toggled by plain CSS
  `display`**, rather than one `<table>` whose cells get reflowed into
  card-like blocks below `md` via CSS with explicit ARIA `role="table"`/
  `role="row"`/`role="cell"` overrides — the reflow technique is a known
  a11y minefield (some browsers/AT combinations lose native table semantics
  the moment `display` changes on table elements even with role overrides
  restoring them), whereas two plain, always-correct markup structures with
  one hidden via `display: none` is boring, easy to verify by reading, and
  never exposes broken semantics to AT since hidden content is excluded
  from the accessibility tree regardless of technique. Cost: duplicated
  markup for six fields; judged worth it for the accessibility certainty.
- `2026-09-17` — **The euro-to-cents conversion uses
  `Math.round(parseFloat(value) * 100)`**, exactly as this task's own
  warning specified — `parseFloat('1.45') * 100` is
  `144.99999999999997`, which truncates to `144` and silently prices
  everything a cent low. Verified directly: 16 unit tests including the
  five example values (1.45→145, 2.30→230, 0.85→85, 19.99→1999, 0.05→5)
  and a round-trip test through `centsToEuroString`, before any UI was
  built on top of it, per the task's explicit "do not discover this in
  review" instruction.
- `2026-09-17` — **Server round-trip field errors call `.markAsTouched()`
  in addition to `.setErrors()`** — Angular's `touched` state normally only
  flips on blur, and this dialog's inline error messages are gated on
  `invalid && touched`; without the explicit `markAsTouched()` call, a
  server-side `DUPLICATE_PRICE` arriving on a field the user hadn't yet
  blurred (e.g. they tabbed straight to Submit) would be silently
  invisible — set but never rendered. Caught by a component test, not by
  inspection.
- `2026-09-17` — **A missing `assets/products/placeholder.svg` asset,
  causing a live 404** — both `vm-product-card` (P7) and the new
  `vm-product-table` fall back to this path for a product with
  `imageUrl: null` (a real, expected case — the form's Image URL field is
  optional), but no such file was ever created; only the six seed products'
  named SVGs exist. Caught live during this phase's manual verification
  (a 404 in the browser console right after creating a product with no
  image) rather than by inspection, since neither `ng build` nor any unit
  test resolves `<img src>` paths against the actual `public/` directory.
  Added a simple neutral "No image" placeholder SVG matching the seed
  assets' `200x200` viewBox style.
- `2026-09-16` — **`POST /api/products/reload` and the `DELETE`/create
  actions return `204 No Content`/`201 Created` respectively**, choices
  `CLAUDE.md` §3 doesn't pin down explicitly (only the purchase/reset
  bodies are shown) — standard REST convention, and reload in particular
  has no natural response body to promise since its entire job is
  discarding state, not returning it (callers needing the refreshed list
  already have `GET /api/products` for that).
- `2026-09-18` — **Explicit repo-owner request: flatten `VM.Server.Domain` to
  pure data and move every rule/calculation/state transition into
  `VM.Server.Service`.** Honest accounting, gains and losses:
  - **Gains**: one place per concern (`ProductValidationService` for every
    product/slot rule, `ChangeCalculationService` for the DP, `VendingService`
    for the state transitions, `MachineStateService` for what "loading" means)
    instead of rules scattered across `Product`/`Slot`/`CoinInventory`/
    `VendingMachine`'s own methods; a reviewer now reads *all* business rules
    in one project instead of hunting through Domain entities too. Tests
    consolidate the same way — one test project instead of three.
  - **What it gives up**: **Domain entities can no longer enforce their own
    invariants.** Before this refactor, `Product.Create(name, -5)` or
    `Slot.Create(product, 99)` was *impossible to construct* — the invalid
    state literally could not exist as an object. After it, `new Product { Id
    = x, PriceCents = -5 }` compiles and runs fine; the entity itself asserts
    nothing, offers no self-protection, and would silently hold nonsense data
    if some future code path skipped `ProductValidationService`. `internal
    set` plus `InternalsVisibleTo` narrows *who* can mutate (only
    `VM.Server.Service`/`VM.Server.Repository`/their tests) but does not
    *validate* anything — it is a visibility fence, not a guard. This is a
    real trade of type-level safety for a simpler, more centralised call
    graph; it is the correct trade only because every current caller of these
    entities' setters is a Service-layer class that is disciplined about
    calling `ProductValidationService`/`ChangeCalculationService` first — a
    new caller added carelessly inside `VM.Server.Service` itself would not
    be caught by the compiler the way it used to be.
  - **What it also gives up**: deleting `VM.Server.API.Tests` (per explicit
    instruction, its behavioural assertions ported into `VM.Server.Service.Tests`)
    removes the only coverage of route spelling, JSON wire-shape (property
    names, casing), the `DomainException.Code` → HTTP status mapping in
    `ExceptionHandlingMiddleware`, and DI/composition-root wiring
    (`AddVendingMachineBackend` actually resolving at the ASP.NET level). None
    of that is business logic, so none of it belongs in
    `VM.Server.Service.Tests` by this task's own scope — but it is real
    coverage that existed and now doesn't. A route typo, a JSON casing
    regression, or a status-code mapping mistake would currently only be
    caught by the manual `curl` pass this refactor ran once, not by any
    automated test. Flagged here rather than silently accepted.
  - **Two test cases that could not be ported as literal equivalents** (both
    exercised guard methods that no longer exist as public throwing APIs):
    `CoinInventory.Remove_MoreThanAvailable_ThrowsDomainException` — the
    invariant it protected (the bank can never be asked to pay out more of a
    denomination than it holds) is now guaranteed by construction:
    `ChangeCalculationService`'s DP bounds every returned count by the
    availability it was given (proved by
    `Calculate_ForARangeOfAmounts_OnSuccessCoinsSumExactlyAndRespectAvailability`),
    and `VendingService.Purchase` only ever asks it to remove coins after
    merging the inserted coins into the bank first — so a bank underflow is
    now provably unreachable rather than defended against at the point of
    removal. `CoinInventory.ToSnapshot_ReturnsADefensiveCopy` — `CoinInventory`
    no longer offers a snapshot method; it's a plain `Dictionary<int,int>`
    property, and callers that need an immutable view (the atomicity tests)
    just copy it themselves (`new Dictionary<int,int>(machine.Bank.Counts)`).
  - **Alternatives rejected**: keeping denomination-acceptance *validation*
    (not the plain data-lookup `CoinDenominations.IsAccepted`) as a
    throwing guard inside `Domain` — rejected because it's a business rule
    (turns "not in the list" into `INVALID_DENOMINATION`), and the task's own
    instruction was that Domain has zero guards; it now lives in
    `VendingService.InsertCoinAsync` and (duplicated, ~6 lines) in
    `MachineStateService` for validating the configured coin bank at load
    time — accepted as a small, deliberate duplication rather than inventing
    a sixth service the task didn't ask for. Folding `MachineStateService`
    into `InMemoryVendingMachineStore` instead of a separate Service-layer
    class — rejected because it would leave "what loading means" (uniqueness
    enforcement, initial-quantity assignment) inside `Repository`, which the
    task says should be mechanical only.
- `2026-09-18` — **`vm-modal` fixed at the primitive for centering, not
  patched at each call site** — the add-product and reload-from-catalogue
  dialogs are both `vm-modal`; the actual bug (global reset's
  `* { margin: 0 }` cancelling native `<dialog>`'s own `margin: auto`
  centering) lives one level below either dialog, so the only correct fix
  is in the shared primitive — rejected adding `margin: auto` (or worse, a
  `position`/`transform` override) to each dialog's own host styles, which
  would have "fixed" both call sites while leaving the primitive itself
  broken for the next dialog someone adds.
- `2026-09-18` — **`.vm-modal__panel`'s height comes from flexbox stretch
  (`.vm-modal[open] { display: flex }` + `min-height: 0` on the panel), not
  a percentage height** — found live, not by inspection: `max-height: 100%`
  on the panel silently resolved to `none` because its ancestor (`.vm-modal`)
  has no *definite* `height`, only `max-height` (a CSS percentage-height
  rule, not an Angular bug) — the panel had no real height limit at all, and
  its last child (the footer) rendered past the dialog's own bottom edge
  instead of the body scrolling internally. Flexbox stretch sizing resolves
  correctly against an auto/max-height flex container in a way percentage
  heights do not — rejected giving the panel an explicit pixel/dvh height
  (would need to exactly duplicate the dialog's own `85dvh` and drift the
  moment one changed without the other).
- `2026-09-18` — **The flex fix is scoped to `.vm-modal[open]`, not bare
  `.vm-modal`** — the first attempt (`display: flex` on the plain class)
  made every *closed* dialog render inline in the page: author styles
  always win over User-Agent styles regardless of specificity, so an
  unscoped `display: flex` silently overrode the browser's own
  `dialog:not([open]) { display: none }` default. Caught live by a
  full-page screenshot showing the (closed) reload-confirmation dialog's
  text sitting in the normal page flow beneath the products table —
  rejected trusting the earlier isolated centering/sizing checks alone,
  which only ever looked at the *open* dialog and would never have
  surfaced this.
- `2026-09-18` — **Skipped the "two buttons on the vending-page cards"
  request** rather than guessing — investigated the actual page first, as
  instructed: `vm-product-card` has exactly one button (Buy, already
  dynamically primary/secondary by affordability); the only other
  persistent action button on the page is `vm-coin-slot`'s "Return coins",
  which isn't inside a card at all. Asked which pair was meant; repo owner
  said to skip it. No `vm-button` or template changes made for this part.
- `2026-09-18` — **`vending-state-machine.md`'s `purchase` transition routes
  through a `purchase_check` `<<choice>>` pseudostate instead of four direct
  `CoinsHeld → CoinsHeld` self-loops, and `insertCoin`'s two outcomes while
  `CoinsHeld` share one self-loop with a `|`-separated label instead of two
  arrows** — not a style preference. Verified with a minimal reproduction
  through `@mermaid-js/mermaid-cli` that `stateDiagram-v2` silently drops
  every self-loop on a state except the last one defined in source order (no
  error, no warning — the SVG just doesn't contain the earlier labels' text).
  Cross-state edges, including several sharing the same source and target,
  are unaffected. Rejected drawing the four purchase outcomes as direct
  self-loops (would have silently rendered as one, exactly the "embarrassing
  error box" the task warned about, except worse — no visible error at all)
  and rejected a semicolon as the separator inside the combined `insertCoin`
  label (also verified by minimal repro: `stateDiagram-v2` parses `;` as a
  statement separator mid-label, splitting it into a malformed fragment).
  Every diagram's rendered SVG was grepped for its expected text content, not
  just checked for a clean CLI exit code, precisely because this failure mode
  produces neither.
- `2026-09-18` — **`purchase` from `Idle` and `reset` from `Idle` are real,
  reachable no-op code paths, deliberately not drawn** in
  `vending-state-machine.md` — neither `VendingService.PurchaseAsync` nor
  `ResetAsync` guards against being called with an empty session; `purchase`
  from `Idle` yields `PRODUCT_NOT_FOUND`/`OUT_OF_STOCK`/`INSUFFICIENT_FUNDS`
  (never `CHANGE_UNAVAILABLE`, which requires funds already in excess of the
  price) and `reset` from `Idle` just returns nothing. Reported explicitly in
  the diagram file rather than either drawn (three more edges document a case
  where nothing was ever at stake) or silently omitted.

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
- `2026-09-18` — P4, much smaller than planned (see decision log for why):
  `VendingService` in `Service/Vending` — `GetDenominationsAsync`/
  `GetSessionAsync`/`InsertCoinAsync`/`PurchaseAsync`/`ResetAsync`, each one
  `IVendingMachineStore.AccessAsync` call into exactly one `VendingMachine`
  method plus hand-written DTO mapping (`CoinCountDto`/`SessionDto`/
  `PurchaseResultDto`/`ReturnedCoinsDto`, matching `CLAUDE.md` §3.2's shapes,
  coin arrays sorted denomination-descending). Added the solution's first
  composition-root DI extension
  (`VM.Server.Repository.ServiceCollectionExtensions.AddVendingMachineBackend`,
  singleton lifetimes throughout) registering everything P3 and P4 need,
  including `BoundedChangeCalculator` as `IChangeCalculator` — no new
  package, `Microsoft.Extensions.DependencyInjection.Abstractions` was
  already transitively available via `Microsoft.Extensions.Options`. 6 new
  `VendingServiceTests` (mapping consistency, real-calculator wiring against
  the same greedy-counterexample bank as `BoundedChangeCalculatorTests`,
  sort order, `DomainException` passthrough, denominations ascending, reset
  round-trip) — no business-rule tests duplicated, they're on the aggregate.
  `VM.Server.Service.csproj` still only references Domain. 101 tests total
  across the solution (79 Domain + 21 Service + 1 API placeholder), all
  green. Closes P4. Next: P5 (HTTP API, contract freeze).
- `2026-09-16` — P5 (`P5-1`…`P5-7`), the API contract frozen:
  `ExternalEndpoints`/`ProductEndpoints`/`VendingEndpoints` (Minimal API,
  `MapGroup`), `ExceptionHandlingMiddleware` (§3.3 shape, exact status
  table, Information/Error log split), DI + CORS + OpenAPI/Swagger wired
  into `Program.cs`. Caught and fixed a real bug during the required manual
  curl pass: the coin bank was silently empty in the live app because P4's
  DI never bound `VendingMachineOptions` from configuration (see decision
  log) - unit tests never would have caught this since they all construct
  options directly. 26 new `WebApplicationFactory` integration tests
  (isolation: one factory per test class instance, documented in
  `TestApp.cs`) cover every route, every error code by its `code` field,
  the full happy path, `CHANGE_UNAVAILABLE` leaving the session untouched,
  reset/reload round trips, and a 500 with no leaked detail (via a
  test-only throwing fake, not a production backdoor). Six placeholder
  product SVGs added under `src/vm-client/public/assets/products/`, every
  `catalogue.seed.json` `imageUrl` confirmed to resolve. Pasted the full
  manual verification: `dotnet build`/`test` (126 total, all green),
  denominations → insert 100/50/20 → session → purchase (paid 170, price
  85, change 85 = 50+20+10+5, quantity 10→9) → insert 200/10 → reset
  (returned 200+10) → session empty, plus a deliberate `CHANGE_UNAVAILABLE`
  (422, empty-bank override) and a deliberate `PRODUCT_NOT_FOUND` (404).
  README's routes and Swagger claim already matched exactly - no changes
  needed. Closes P5. Next: P6-2/P6-3 (frontend API services), now
  unblocked.
- `2026-09-17` — P6-2/P6-3/P6-8, closing P6: read the real DTOs
  (`ErrorCodes.cs`, `ProductDto`, `ExternalProductDto`, `CoinCountDto`,
  `SessionDto`, `PurchaseResultDto`, `ReturnedCoinsDto`, the four request
  records) rather than modelling off `CLAUDE.md` §3 alone, then started the
  backend and cross-checked against both the live OpenAPI document and real
  curl responses for every route (denominations, catalog, products, session,
  insert, purchase, reset, plus six error codes: `INVALID_DENOMINATION`,
  `PRODUCT_NOT_FOUND`, `DUPLICATE_PRODUCT`, `DUPLICATE_PRICE`,
  `INVALID_QUANTITY`, `INVALID_PRICE`). All three sources agreed with each
  other; the one real disagreement was with the task prompt's own assumed
  field name (see decision log — `insertedCoins`, not `coins`). Added
  `core/models/product.model.ts` (`Product`, `CatalogueProduct`,
  `CreateProductRequest`, `UpdateProductRequest`) and `core/models/coin.model.ts`
  (`CoinCount`, `VendingSession`, `PurchaseResult`, `ResetResult`) — plain
  interfaces, no classes, no mapping layer. `ErrorCode` (built in the earlier
  P6-1/P6-4 session) already covered all ten `ErrorCodes.cs` codes exactly;
  `ERROR_MESSAGES satisfies Record<ErrorCode, string>` still compiled with no
  changes needed. Added `core/api/api-paths.ts` (one `API_PATHS` constant,
  every route in one place) and three thin `providedIn: 'root'` services
  (`ProductsApiService`, `VendingApiService`, `ExternalCatalogApiService`,
  the last one new — P8's reload UI needs to show what the external catalog
  holds), each method one typed `HttpClient` call, no caching/state/retry.
  14 new tests via `HttpTestingController` (URL, verb, request body, typed
  response for every method), plus two tests proving the P6-4 interceptor and
  these services compose correctly end-to-end: a 409 `DUPLICATE_PRODUCT` and
  a 422 `CHANGE_UNAVAILABLE` both arrive at the caller as a normalised
  `ApiError` with the code intact. `lint`/`build`/`test:ci` all green
  (59/59). Reloaded the backend's in-memory state (`POST
  /api/products/reload`) after the manual verification to undo the test
  product/purchases before stopping. Fixed one real doc gap: `CLAUDE.md` §3.2
  never showed the `GET /api/vending/session` payload — added it. Closes P6.
  Next: P7 (vending UI).
- `2026-09-17` — Started P7 (vending UI), store first (`P7-1`/half of
  `P7-9`): `core/state/vending.store.ts`, a `providedIn: 'root'` signal
  store wrapping `ProductsApiService`/`VendingApiService`. Private writable
  signals, `.asReadonly()` public views. `busy` gates all three mutating
  actions so a double-click (or any concurrent call) is a real no-op — only
  one HTTP request ever in flight, proven with `httpTesting.match(...)`
  returning length 1 rather than trusting a single `expectOne`. `purchase`
  patches the returned product into `products` and zeroes `session` locally
  (the response has no session field to apply — see P6-2's `insertedCoins`
  finding — so this is bookkeeping on a known invariant from CLAUDE.md §2.5,
  not client-side business logic); `reset` does the same for
  `lastReturn`/session. `insertCoin` clears `lastPurchase`/`lastReturn`/
  `error` up front — cleared `lastReturn` too, not just the `lastPurchase`
  the task named, since a stale change-tray next to a freshly-growing total
  is exactly the bug the rule was warning about. 8 new
  `HttpTestingController` specs, all green. `lint` clean. `P7-9` stays `[~]`
  until the `vm-product-card` stock-state test is added alongside the
  components.
- `2026-09-17` — Finished P7 (vending UI): `vm-machine-display`,
  `vm-coin-slot`, `vm-product-card`, `vm-product-grid`, `vm-change-tray`,
  and the `vending-page` wiring them to the store (see decision log for the
  `vm-button` extension, the `denominationLabel` pipe, the new layout
  tokens, and the `ResizeObserver`-measured sticky panel). `lint`/`build`
  green; `test:ci` 81/81 (22 new: 8 store from the earlier session + 4
  `vm-product-card` + 3 `vm-change-tray` + 4 `vending-page` + 2 `vm-button`
  (new inputs) + 2 `denominationLabel` pipe).

  Manual verification, backend running, all done live (not assumed):
  - **Insert → buy → change → stock decrement**: inserted 2,10 € across
    1€/50c/20c/20c coins (keyboard-only: `Tab`+`Enter` on the focused Buy
    button, not a synthetic click), bought Chocolate Bar (210c, exact
    payment) — quantity 10→9, focus landed on the change tray
    (`document.activeElement` confirmed), tray read "Chocolate Bar
    dispensed / Change: 0,00 €".
  - **Can't afford**: inserted 20c, attempted Chocolate Bar (210c) —
    `INSUFFICIENT_FUNDS` shown as "Please insert more money to buy this
    item." (never the raw code), total stayed at 0,20 €.
  - **`CHANGE_UNAVAILABLE`**: deliberately drained the bank's entire 5c/10c
    supply first (20 real purchases via the API, each returning exactly one
    5c+10c as change, confirmed by the actual response bodies — not a
    stubbed/faked bank state), then in the browser inserted 1€ and bought
    Water (85c, needs 15c change, now unmakeable) — friendly message shown,
    total stayed at 1,00 €, Water's badge stayed "15 in stock" (unchanged).
    Restarted the backend afterward to reset the bank and every mutated
    quantity back to the seed.
  - **Return coins**: inserted 50c+20c+20c (90c), clicked Return — total
    back to 0,00 €, tray read "1 x 50c 2 x 20c", matching exactly what went
    in.
  - **No horizontal scroll**: `document.documentElement.scrollWidth ===
    clientWidth` confirmed at 320/360/768/1024/1440px.
  - **Sticky bar vs. last row**: first attempt at this check gave a
    false-positive "overlap" by comparing unscrolled bounding-rect numbers
    (the last card's *document position* is naturally far below an
    800px-tall viewport, which isn't the same thing as being visually
    covered). Re-checked correctly by scrolling an actual page to
    `scrollHeight` and comparing rects at that real position: last card
    bottom (−48.7px, already scrolled clear above the viewport) vs. panel
    top (359px) — no overlap. Screenshot saved.
  - **Keyboard-only**: coin insertion via `focus()` + `Enter` on a coin
    button moved the total from 0,00 € to 0,20 €; the purchase flow above
    was also driven the same way. Tab order sampled and consistent with DOM
    order (header → grid → panel); an apparent backward jump in the raw
    sample was the headless browser wrapping past the last focusable
    element back to the top, not a real trap.
  - **Dark mode**: read `--surface`/`--text`/`--danger`/`--success`/
    `--warning` back from the live page under `prefers-color-scheme: dark`
    — all match `_tokens.scss`'s dark block exactly (no new colour tokens
    were introduced this phase, so no new contrast pairs to compute).
    Screenshots at 1024px and 360px both dark, plus 360px light, all
    visually reviewed — coin circles, badges and the change tray all read
    correctly in both themes.

  Also found and fixed an unrelated environment issue: the long-running dev
  server (background task from an earlier session) had gotten stuck after a
  transient "stylesheet not found" error mid-edit and stopped rebuilding on
  further file changes, silently serving a stale bundle. Killed the orphaned
  process directly (`TaskStop` alone didn't reach it — `npm start` had
  detached a child process outside the tracked task) and restarted clean.
  Closes P7. Next: P8 (products admin UI).
- `2026-09-17` — P8 (`P8-1`…`P8-7`), products admin UI: `products.store`
  (`core/state/products.store.ts`, mutating methods return the underlying
  `Observable` — see decision log), `vm-product-table` (real `<table>` +
  card `<ul>`, CSS-toggled, not one reflowed table), `vm-product-form-dialog`
  (typed reactive form in `vm-modal`, `Math.round(parseFloat(v) * 100)`
  euro-to-cents conversion, validators mirroring the server plus a
  client-only duplicate-price convenience check), delete/reload confirm
  dialogs via `vm-confirm-dialog`, an optional read-only external-catalogue
  panel, and server-field-error mapping (P8-6). Extracted
  `stockBadgeVariant`/`stockLabel` out of `vm-product-card` into a shared
  `shared/ui/badge/stock-badge.ts` so `vm-product-table` doesn't duplicate
  the low-stock threshold. 30 new tests (16 validator + 7 component +
  7 store) — the validator tests cover the euro-to-cents trap directly
  (1.45/2.30/0.85/19.99/0.05 and the round trip) before any UI touched it,
  per the task's own "do not discover this in review" instruction.
  `lint`/`build` green; `test:ci` 123/123 (two re-runs, stable — a Modal
  test flaked once under full-suite load but passed 8/8 in isolation,
  consistent with the pre-existing flakiness noted in the P6-9 session, not
  a regression from this phase).

  Manual verification, backend running, all done live:
  - **Create/edit/delete/reload end to end**: created "Test Snack" at
    €3.00, confirmed via a raw `fetch('/api/products')` (not the UI) that
    `priceCents === 300` exactly — chose 3.00 over the task's own 1.45
    example because 1.45 collides with the seed catalogue's real Cola price
    and would have been legitimately blocked by the duplicate-price check;
    the 1.45 example itself is proven by the validator unit tests instead.
    Edited it back with no changes and confirmed the price round-tripped to
    exactly 300c, then a real edit (name/price/quantity) applied correctly,
    then deleted it, then reloaded and confirmed the catalogue returned to
    exactly the 6 seed products with a "now holds 6 products" message shown.
  - **Duplicate price caught on the field**: client-side, typing a price
    matching Water's real seed price (0.85) blocked submission with
    "Another product already uses this price" under the price field, no
    request fired. Server-side, forced a genuine `DUPLICATE_PRICE` by
    POSTing directly via `fetch` (bypassing the Angular form entirely) —
    409 with the code intact.
  - **Quantity 16**: blocked client-side (`aria-invalid`, dialog stays
    open, no request fired, boundary 15 confirmed valid immediately after);
    forced server-side via a direct `fetch` POST — 400 `INVALID_QUANTITY`.
  - **Table at 360px / cards below md**: confirmed live at 320/360/768/
    1024/1440px — the real `<table>` is hidden and the card list visible
    below md, and the reverse at md and up; zero horizontal scroll at every
    width. Screenshots saved.
  - **Modal a11y**: 15 successive Tabs from the Add-product dialog's
    initial focus never left the dialog (real focus trap, not assumed);
    Escape closed it; focus returned to the Add product trigger button
    afterward — confirmed via `document.activeElement`, not inference.
  - **Dark mode**: tokens read back from the live page matched
    `_tokens.scss`'s dark block exactly; screenshots of the table, the
    open form dialog, and the mobile card list all reviewed in dark mode.
  - **Delete focus-return** (P8-4's specific ask): confirmed live that
    Cancel on the delete confirm dialog returns focus to that row's own
    Delete button — this came free from `vm-modal`'s existing
    trigger-focus-restore mechanism (P6-9), nothing new needed.

  One real bug found and fixed live, not by inspection: `assets/products/
  placeholder.svg` (the fallback image for a product with no `imageUrl`)
  didn't exist, causing a 404 the moment a product without an image was
  rendered — added a simple placeholder SVG matching the seed assets' style
  (see decision log). Closes P8. Next: P9 (polish, verification, handover).
- `2026-09-18` — Service-layer refactor (backend only, run before P9 per
  explicit repo-owner request): flattened `Product`/`Slot`/`CoinInventory`/
  `VendingMachine`/`PurchaseResult` to plain data (`internal set` + a new
  `Domain/AssemblyInfo.cs` scoping `InternalsVisibleTo` to
  `VM.Server.Service`/`VM.Server.Repository`/`VM.Server.Service.Tests`);
  deleted `Domain/Services/` entirely. Moved its contents plus every rule
  that used to live on the flattened entities into `VM.Server.Service`:
  `ChangeCalculationService` (DP moved verbatim), `ProductValidationService`
  (name/price/quantity/uniqueness — the one place for every product rule),
  `MachineStateService` (new `Service/State/` folder — loading/reload logic,
  chosen over folding into the store so `InMemoryVendingMachineStore` stays
  mechanical), and a rewritten `VendingService`/`ProductService` that mutate
  the plain-data `VendingMachine` directly. Preserved `Purchase`'s
  compute-then-commit ordering exactly (find slot → stock → funds → change →
  mutate only once all four pass) — still no rollback code anywhere, proven
  by a re-verified atomicity test that snapshots slots/bank/session before
  and after every refusal reason (`OUT_OF_STOCK`/`INSUFFICIENT_FUNDS`/
  `CHANGE_UNAVAILABLE`/`PRODUCT_NOT_FOUND`). Deleted `VM.Server.Domain.Tests`
  and `VM.Server.API.Tests` (removed from `VM.Server.slnx`); ported every
  behavioural assertion into `VM.Server.Service.Tests` against the real
  service APIs, including the named greedy-counterexample proof, the
  200-coin performance test, all boundary tables, and the atomicity proof —
  two cases (`CoinInventory`'s own `Remove`/`ToSnapshot` guard tests) could
  not be ported as literal equivalents since the methods they exercised no
  longer exist as public APIs; see the decision log for why the invariants
  they protected are still provably held. 85 tests total, all green;
  solution-wide `dotnet build` clean, 0 warnings (`TreatWarningsAsErrors`
  still holds — no new packages, no MediatR/FluentValidation). Re-ran the
  full P5 manual curl verification live end to end (denominations → insert
  100/50/20 → session 170 → purchase Water 85c → paid 170/change 85 = 50+20+
  10+5/quantity 10→9 → insert 200/10 → reset → session empty → 404
  `PRODUCT_NOT_FOUND`, plus a second instance started with an empty coin
  bank via env-var config override to force a live 422 `CHANGE_UNAVAILABLE`
  with the session left untouched) and confirmed every response matches the
  P5 baseline exactly (same shapes, same codes, same status codes — only the
  product GUIDs differ, as expected). Updated `CLAUDE.md` §2.3/§2.4/§2.5/
  §2.6/§4.1/§4.2/§4.3 to describe the new layout and to mark the
  entity-enforced invariants as now service-enforced. Marked `P1-1`/`P1-6`/
  `P1-7`/`P1-8`/`P2-2` `[-]` superseded in place (history kept, not deleted).
  Being honest about the trade-off: Domain entities can no longer enforce
  their own invariants (a real loss of type-level safety, mitigated only by
  `internal set` narrowing *who* can mutate, not validating *what* gets
  written), and deleting `VM.Server.API.Tests` removes all coverage of route
  spelling, JSON wire-shape, the error-code-to-HTTP-status mapping, and DI
  wiring — see the decision log for the full accounting. Does not touch the
  frontend. Next: P9 (polish, verification, handover), now running against
  this post-refactor backend.
- `2026-09-18` — Four small frontend fixes requested ahead of the P9 pass
  (`P9-10`/`P9-11`); one (`vm-button` `warning` variant) done fully, one
  (`vm-modal` centering) done and expanded once live verification surfaced
  two real knock-on bugs, one skipped on the repo owner's instruction after
  I investigated and asked rather than guessed (see decision log for all
  three). `P9-10`: added `warning` to `ButtonVariant`, three tokens in both
  themes, wired to the products-admin Edit button (table row and mobile
  card). `P9-11`: `vm-modal` centering, `max-height` moved to `85dvh`, plus
  the panel-overflow and closed-dialog-visibility fixes found live (see
  decision log) — neither would have been caught by lint, build, or the
  existing test suite, since none of them exercise real geometry or the
  UA/author-style cascade.

  `lint`/`build` green; `test:ci` 124/124 (1 new: the `warning` variant
  class). Grepped both changed component stylesheets for hex codes,
  `rgb()`/`rgba()`, and `px` spacing values — zero hits beyond the
  pre-existing 1-2px border/outline widths (not spacing-scale values, same
  as every other primitive in `shared/ui`).

  Manual verification, backend running, all done live:
  - **Edit button contrast**: read the live computed `background-color`/
    `color` off the actual rendered button (not the token values on paper)
    in both themes — light 8.54:1, dark 8.38:1, both far past the 4.5:1
    requirement. Screenshots of both themes saved.
  - **Modal centering**: measured the actual dialog's left/right and top/
    bottom gutters against the viewport at 320/375/768/1440px — horizontally
    and vertically centered (gutters equal within rounding) at every width,
    and fully within the viewport bounds at every width. The
    reload-from-catalogue confirm dialog checked separately at 768px to
    confirm the primitive-level fix covers both dialogs, not just the one
    tested first.
  - **Many validation errors still fit**: triggered all three of the
    product form's validators at once (empty name, unparseable price,
    quantity 16) at 375×700 — before the panel-sizing fix this measurably
    pushed the footer 48px past the dialog's own bottom edge (caught via
    `getBoundingClientRect()`, not visually); after the fix the panel's
    height exactly matches the dialog's, the footer is fully visible, and
    the body is measurably scrollable (`scrollHeight` 456 vs `clientHeight`
    408). Screenshot saved showing all three inline errors with the footer
    still pinned and visible.
  - **Focus trap / Escape / restore**: an isolated check (fresh page load,
    nothing else open first) confirmed 15 successive Tabs never left the
    dialog, Escape closed it, and focus returned to the exact trigger
    button handle captured before opening — all still intact after the
    centering/sizing changes. (A combined run of every check in one script
    showed a false "focus not restored" because an earlier section in that
    *same script* had left a dialog open without closing it first — a
    test-harness ordering bug, not a product regression; resolved by
    re-running the check in isolation.)
  - **Closed dialogs stay hidden**: read every `dialog.vm-modal`'s
    `open`/computed-`display` at page load with nothing open — all three
    (product form, delete confirm, reload confirm) correctly `open: false`/
    `display: none`; a full-page screenshot confirmed no stray dialog
    content bleeds into the page layout.

  Backend state confirmed unchanged after verification (still the 6-product
  seed) — none of the scripted checks completed a real form submission.
  Two separate commits, scope `ui`: the button variant, and the modal fix
  (they're unrelated changes, per the task's own instruction). Left
  `angular.json`'s stray `"analytics": false` addition (an Angular-CLI-
  written line, not something I changed) out of both commits — unrelated to
  this task. A large, unrelated backend service-layer refactor was
  mid-flight in the working tree from a concurrent session throughout this
  turn; staged only the exact frontend files touched here, never a broad
  `git add`, so none of it is in either commit.
- `2026-09-18` — `P9-12`: read the current `VendingService`/
  `ChangeCalculationService` fresh (namespaces had moved to
  `VM.Server.Service.DTOs`/`.Implementations` since the last backend session
  touched this repo — confirmed the business logic itself is unchanged, only
  reorganised) before drawing anything, per the task's own instruction. Two
  Mermaid diagrams added under `docs/diagrams/` (`vending-state-machine.md`,
  `change-calculation.md`), an index README, and links from the main
  README/`CLAUDE.md` §8 — see the phase entry above and the two decision-log
  entries for what each diagram covers and the real Mermaid renderer bug
  (silent self-loop collapsing on `stateDiagram-v2`) found and designed
  around while verifying with `@mermaid-js/mermaid-cli`. Left
  `angular.json`'s stray CLI-written diff untouched, as before. Three
  commits, scope `docs`: one per diagram, one for the index/links/tracker
  update.
