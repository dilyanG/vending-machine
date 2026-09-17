/**
 * Wire shape of `ProductDto` (VM.Server.Service.Products) — application-state
 * products, quantity included. Field names/casing exactly as STJ's default
 * camelCase serialises the record.
 */
export interface Product {
  readonly id: string;
  readonly name: string;
  readonly priceCents: number;
  readonly quantity: number;
  readonly imageUrl: string | null;
}

/**
 * Wire shape of `ExternalProductDto` (VM.Server.API.Dtos) — the mock external
 * catalogue's shape. No `quantity`: an external catalogue has no concept of
 * this machine's stock levels (CLAUDE.md §2.6).
 */
export interface CatalogueProduct {
  readonly id: string;
  readonly name: string;
  readonly priceCents: number;
  readonly imageUrl: string | null;
}

/** Wire shape of `CreateProductRequest` (VM.Server.API.Dtos). */
export interface CreateProductRequest {
  readonly name: string;
  readonly priceCents: number;
  readonly quantity: number;
  readonly imageUrl: string | null;
}

/** Wire shape of `UpdateProductRequest` (VM.Server.API.Dtos). */
export interface UpdateProductRequest {
  readonly name: string;
  readonly priceCents: number;
  readonly quantity: number;
  readonly imageUrl: string | null;
}
