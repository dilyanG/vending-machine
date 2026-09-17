import { Product } from './product.model';

/** Wire shape of `CoinCountDto` (VM.Server.Service.Vending). */
export interface CoinCount {
  readonly denominationCents: number;
  readonly count: number;
}

/**
 * Wire shape of `SessionDto` (VM.Server.Service.Vending) — confirmed live
 * against `GET /api/vending/session`. The field is `insertedCoins`, not
 * `coins`.
 */
export interface VendingSession {
  readonly insertedCoins: CoinCount[];
  readonly insertedTotalCents: number;
}

/** Wire shape of `PurchaseResultDto` (VM.Server.Service.Vending). */
export interface PurchaseResult {
  readonly product: Product;
  readonly paidCents: number;
  readonly priceCents: number;
  readonly changeCents: number;
  readonly changeCoins: CoinCount[];
}

/** Wire shape of `ReturnedCoinsDto` (VM.Server.Service.Vending). */
export interface ResetResult {
  readonly returnedCoins: CoinCount[];
  readonly returnedTotalCents: number;
}
