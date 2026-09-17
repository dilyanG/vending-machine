# Vending Machine

A responsive vending machine web application: browse products, insert coins, buy,
get change — plus an admin view for managing the product list.

**Backend:** .NET 10 (ASP.NET Core Minimal APIs, C#) · **Frontend:** Angular 22

---

## Currency and accepted coins

The machine operates in **euro (EUR)** and accepts **only** these coins:

| Coin | Value |
| --- | --- |
| 5c | €0.05 |
| 10c | €0.10 |
| 20c | €0.20 |
| 50c | €0.50 |
| €1 | €1.00 |
| €2 | €2.00 |

€0.01 and €0.02 coins and all banknotes are **rejected**.

All amounts are handled internally as integer cents, so no rounding errors are
possible. Change is calculated against the coins the machine actually holds — if
exact change cannot be made, the purchase is refused and your coins are returned.

---

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 22.22.3+](https://nodejs.org/) (or 24.15+/26+) and npm 10+ —
  Angular CLI 22 refuses to run on older Node 22.x patches

Check with `dotnet --version` and `node --version`.

---

## Getting started

Clone, then run the backend and frontend in two terminals.

### Backend — http://localhost:5080

```bash
cd src/vm-server/VM.Server
dotnet restore
dotnet run --project VM.Server.API
```

Swagger UI: <http://localhost:5080/swagger>

### Frontend — http://localhost:4200

```bash
cd src/vm-client
npm install
npm start
```

Open <http://localhost:4200>.

---

## Tests

```bash
# backend
cd src/vm-server/VM.Server && dotnet test VM.Server.slnx

# frontend
cd src/vm-client && npm test     # watch mode
npm run test:ci                  # single headless run
```

---

## Project layout

```
.
├── src/
│   ├── vm-server/VM.Server/
│   │   ├── VM.Server.Domain/       # entities, coin rules, change algorithm
│   │   ├── VM.Server.Service/      # product & vending use cases + abstractions
│   │   ├── VM.Server.Repository/   # in-memory store + mock external catalog
│   │   ├── VM.Server.API/          # HTTP endpoints
│   │   └── tests/
│   └── vm-client/
│       └── src/app/{core,features,shared}/
├── .claude/
│   ├── CLAUDE.md             # engineering conventions and domain rules
│   └── task-progress.md      # live progress tracker
└── docs/
    └── IMPLEMENTATION_PLAN.md  # phased build plan
```

---

## How it works

**Products come from an external resource.** A mock external catalog lives in
the backend at
`src/vm-server/VM.Server/VM.Server.Repository/MockExternalApi/catalogue.seed.json`
and is exposed read-only at `GET /api/external/catalog`. It carries only each
product's name, price and image — no stock levels, since an external catalog
wouldn't know this machine's inventory. On first request the application loads
it into the vending machine, giving every product the same starting quantity
from configuration (see below).

**CRUD affects application state only.** Creating, updating or deleting a
product changes the in-memory store; the external catalog is never written to.
`POST /api/products/reload` discards in-memory changes and re-seeds from it.
Because state is in memory, it resets when the backend restarts.

**Buying.** Insert coins one at a time, pick a product, and the machine
dispenses it with change — using the fewest coins it can, drawn from its own
coin bank (which includes the coins you just inserted). *Return coins* gives
back exactly the coins you put in without buying anything.

### API summary

| Method | Route | |
| --- | --- | --- |
| `GET` | `/api/external/catalog` | Mock external product source (read-only) |
| `GET POST` | `/api/products` | List / create |
| `GET PUT DELETE` | `/api/products/{id}` | Read / update / delete |
| `POST` | `/api/products/reload` | Re-seed state from the external catalog |
| `GET` | `/api/vending/denominations` | Accepted coin values |
| `GET` | `/api/vending/session` | Coins inserted so far |
| `POST` | `/api/vending/coins` | Insert one coin |
| `POST` | `/api/vending/purchase` | Buy a product |
| `POST` | `/api/vending/reset` | Return inserted coins |

---

## Design notes

**Diagrams:** [`docs/diagrams/`](docs/diagrams/README.md) has the vending
state machine (insert coin / purchase / reset, every refusal path and its
error code) and the change calculation (where it sits in an atomic purchase,
the bounded coin-change algorithm, and why greedy doesn't work) — both
verified to render, not just eyeballed.

- **Integer cents everywhere.** Floating-point money is never used.
- **Bounded change-making.** The coin bank is finite, so change uses a
  dynamic-programming solve rather than a greedy pass — greedy can claim success
  on a combination the machine cannot actually pay out.
- **Atomic purchases.** A purchase either fully succeeds or leaves inventory,
  the coin bank and the session untouched.
- **Max 15 units per product type**, and every product type has a distinct
  price, per the requirements. Starting stock is uniform across products and
  comes from configuration, not the external catalog (see below).
- **Mobile-first responsive layout**, tested from 360px upwards; the product
  grid reflows from one to four columns and the coin panel docks to the bottom
  on small screens.
- **The server is the authority.** The frontend never computes change or decides
  whether a purchase is valid.

---

## Configuration

| Setting | Where | Default |
| --- | --- | --- |
| Backend port | `src/vm-server/VM.Server/VM.Server.API/Properties/launchSettings.json` | `5080` |
| Allowed CORS origin | `src/vm-server/VM.Server/VM.Server.API/appsettings.Development.json` → `Cors:AllowedOrigins` | `http://localhost:4200` |
| API base URL | `src/vm-client/src/environments/environment*.ts` | `''` (relative `/api`; the dev proxy forwards it to `:5080`) |
| Initial coin bank | `appsettings.json` → `VendingMachine:CoinBank` | see file |
| Initial quantity per product | `appsettings.json` → `VendingMachine:InitialQuantityPerSlot` | `10` |
| Seed products | `.../VM.Server.Repository/MockExternalApi/catalogue.seed.json` | 6 products, no quantity |
