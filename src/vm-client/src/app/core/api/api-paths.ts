/**
 * Every backend route (CLAUDE.md §3.1) in one place, so no service builds a
 * URL by string concatenation. Paths are relative to `environment.apiBaseUrl`.
 */
export const API_PATHS = {
  externalCatalog: '/api/external/catalog',
  products: '/api/products',
  product: (id: string) => `/api/products/${id}`,
  productsReload: '/api/products/reload',
  vendingDenominations: '/api/vending/denominations',
  vendingSession: '/api/vending/session',
  vendingCoins: '/api/vending/coins',
  vendingPurchase: '/api/vending/purchase',
  vendingReset: '/api/vending/reset',
} as const;
