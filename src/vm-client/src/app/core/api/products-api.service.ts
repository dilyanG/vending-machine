import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateProductRequest, Product, UpdateProductRequest } from '../models/product.model';
import { API_PATHS } from './api-paths';

/**
 * One HTTP call per method, correctly typed, nothing else — no caching, no
 * state, no retry logic, no error handling (the error interceptor already
 * normalises failures; state belongs in the P7/P8 stores).
 */
@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  list(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}${API_PATHS.products}`);
  }

  get(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}${API_PATHS.product(id)}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}${API_PATHS.products}`, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}${API_PATHS.product(id)}`, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}${API_PATHS.product(id)}`);
  }

  reload(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}${API_PATHS.productsReload}`, null);
  }
}
