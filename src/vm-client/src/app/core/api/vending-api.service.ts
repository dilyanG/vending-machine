import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PurchaseResult, ResetResult, VendingSession } from '../models/coin.model';
import { API_PATHS } from './api-paths';

/**
 * One HTTP call per method, correctly typed, nothing else. The frontend never
 * computes change or decides whether a purchase is allowed (CLAUDE.md §5.3) —
 * this service only relays the server's decision.
 */
@Injectable({ providedIn: 'root' })
export class VendingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  denominations(): Observable<number[]> {
    return this.http.get<number[]>(`${this.baseUrl}${API_PATHS.vendingDenominations}`);
  }

  session(): Observable<VendingSession> {
    return this.http.get<VendingSession>(`${this.baseUrl}${API_PATHS.vendingSession}`);
  }

  insertCoin(denominationCents: number): Observable<VendingSession> {
    return this.http.post<VendingSession>(`${this.baseUrl}${API_PATHS.vendingCoins}`, {
      denominationCents,
    });
  }

  purchase(productId: string): Observable<PurchaseResult> {
    return this.http.post<PurchaseResult>(`${this.baseUrl}${API_PATHS.vendingPurchase}`, {
      productId,
    });
  }

  reset(): Observable<ResetResult> {
    return this.http.post<ResetResult>(`${this.baseUrl}${API_PATHS.vendingReset}`, null);
  }
}
