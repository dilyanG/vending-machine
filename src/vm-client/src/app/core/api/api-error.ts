/**
 * Every error code the backend can return (CLAUDE.md §3.3), plus two
 * client-only fallbacks the error interceptor produces when there is no
 * server error to read: `NETWORK_ERROR` (no response at all) and
 * `UNKNOWN_ERROR` (a response whose body doesn't match the §3.3 shape).
 */
export type ErrorCode =
  | 'INVALID_DENOMINATION'
  | 'INSUFFICIENT_FUNDS'
  | 'OUT_OF_STOCK'
  | 'CHANGE_UNAVAILABLE'
  | 'PRODUCT_NOT_FOUND'
  | 'INVALID_QUANTITY'
  | 'INVALID_PRICE'
  | 'INVALID_PRODUCT'
  | 'DUPLICATE_PRODUCT'
  | 'DUPLICATE_PRICE'
  | 'NETWORK_ERROR'
  | 'UNKNOWN_ERROR';

/**
 * The normalised shape every failed API call surfaces as, produced by
 * error.interceptor.ts. `message` is whatever the server (or the transport
 * layer) said and exists for logs/devtools only — CLAUDE.md §5.3 forbids
 * showing it to the user. Look up `ERROR_MESSAGES[code]` for that instead.
 */
export interface ApiError {
  readonly code: ErrorCode;
  readonly message: string;
  readonly details?: Readonly<Record<string, unknown>>;
}
