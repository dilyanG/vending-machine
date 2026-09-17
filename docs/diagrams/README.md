# Diagrams

Two diagrams, each documenting what the backend code actually does — not the
original design, not a generic vending machine.

- **[vending-state-machine.md](vending-state-machine.md)** — *what state is
  the machine in, and what can happen from there?* The customer-facing
  insert/purchase/reset flow, every refusal path with its real error code,
  and why a refused purchase keeps the customer's money instead of losing it.
- **[change-calculation.md](change-calculation.md)** — *how does the machine
  decide what coins to hand back, and why can't it just take the largest
  coin first?* Where the change calculation sits in an atomic purchase, the
  shape of the bounded coin-change algorithm, and the counterexample that
  rules out a greedy solution.

Both are Mermaid diagrams in fenced code blocks, verified to render with
`@mermaid-js/mermaid-cli` before being committed. If you change a vending
state transition or the change algorithm, update the matching diagram in the
same commit — see `CLAUDE.md`.
