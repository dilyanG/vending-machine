import { ErrorCode } from './api-error';

/**
 * The only place an error code becomes words a customer standing at the
 * machine (or an admin filling in a form) actually reads. `satisfies
 * Record<ErrorCode, string>` makes it a compile error to add a code to the
 * union without writing a message for it here.
 */
export const ERROR_MESSAGES = {
  INVALID_DENOMINATION:
    "Sorry, I don't accept that coin. Please use 5c, 10c, 20c, 50c, €1 or €2 coins only.",
  INSUFFICIENT_FUNDS: 'Please insert more money to buy this item.',
  OUT_OF_STOCK: 'Sorry, this item is sold out. Please choose another.',
  CHANGE_UNAVAILABLE: "Sorry, I can't make exact change for that. Your coins have been returned.",
  PRODUCT_NOT_FOUND: 'Sorry, this item is no longer available.',
  INVALID_QUANTITY: "That quantity isn't allowed. Please choose a value between 0 and 15.",
  INVALID_PRICE: "Sorry, that price isn't valid.",
  INVALID_PRODUCT: "Sorry, those product details aren't valid.",
  DUPLICATE_PRODUCT: 'A product with that name already exists. Please choose a different name.',
  DUPLICATE_PRICE: 'Another product already has that exact price. Please choose a different price.',
  NETWORK_ERROR: "Sorry, I can't connect right now. Please check your connection and try again.",
  UNKNOWN_ERROR: 'Sorry, something went wrong. Please try again.',
} satisfies Record<ErrorCode, string>;

/**
 * Safe lookup for a code that may not actually be one of the known
 * ErrorCode literals at runtime (e.g. the server adds a new one before the
 * frontend does) — never returns undefined, never leaks the raw code.
 */
export function getErrorMessage(code: string): string {
  return (ERROR_MESSAGES as Record<string, string>)[code] ?? ERROR_MESSAGES.UNKNOWN_ERROR;
}
