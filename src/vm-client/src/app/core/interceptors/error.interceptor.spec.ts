import {
  HttpClient,
  HttpErrorResponse,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiError } from '../api/api-error';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('normalises a well-formed §3.3 server error, passing the code/message/details through', () => {
    let captured: ApiError | undefined;

    httpClient.post('/api/vending/purchase', {}).subscribe({
      error: (err: ApiError) => (captured = err),
    });

    httpTesting.expectOne('/api/vending/purchase').flush(
      {
        code: 'CHANGE_UNAVAILABLE',
        message: 'The machine cannot give exact change for this purchase.',
        details: { shortfallCents: 5 },
      },
      { status: 422, statusText: 'Unprocessable Entity' },
    );

    expect(captured).toEqual({
      code: 'CHANGE_UNAVAILABLE',
      message: 'The machine cannot give exact change for this purchase.',
      details: { shortfallCents: 5 },
    });
  });

  it('falls back to UNKNOWN_ERROR for an HTTP error with an unrecognised body', () => {
    let captured: ApiError | undefined;

    httpClient.get('/api/products').subscribe({
      error: (err: ApiError) => (captured = err),
    });

    httpTesting
      .expectOne('/api/products')
      .flush('<html>502 Bad Gateway</html>', { status: 502, statusText: 'Bad Gateway' });

    expect(captured?.code).toBe('UNKNOWN_ERROR');
  });

  it('falls back to NETWORK_ERROR when no response reaches the client', () => {
    let captured: ApiError | undefined;

    httpClient.get('/api/products').subscribe({
      error: (err: ApiError) => (captured = err),
    });

    httpTesting
      .expectOne('/api/products')
      .error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown Error' });

    expect(captured?.code).toBe('NETWORK_ERROR');
  });

  it('never lets a raw HttpErrorResponse escape', () => {
    let captured: unknown;

    httpClient.get('/api/products').subscribe({
      error: (err: unknown) => (captured = err),
    });

    httpTesting
      .expectOne('/api/products')
      .flush(
        { code: 'PRODUCT_NOT_FOUND', message: 'not found' },
        { status: 404, statusText: 'Not Found' },
      );

    expect(captured).not.toBeInstanceOf(HttpErrorResponse);
    expect((captured as ApiError).code).toBe('PRODUCT_NOT_FOUND');
  });
});
