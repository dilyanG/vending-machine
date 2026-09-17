import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CatalogueProduct } from '../models/product.model';
import { API_PATHS } from './api-paths';

/** Read-only access to the mock external catalogue, for the admin page to show what it holds. */
@Injectable({ providedIn: 'root' })
export class ExternalCatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  catalogue(): Observable<CatalogueProduct[]> {
    return this.http.get<CatalogueProduct[]>(`${this.baseUrl}${API_PATHS.externalCatalog}`);
  }
}
