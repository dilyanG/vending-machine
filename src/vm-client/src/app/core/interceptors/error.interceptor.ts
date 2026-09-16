import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError, ErrorCode } from '../api/api-error';

interface ServerErrorBody {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

function isServerErrorBody(body: unknown): body is ServerErrorBody {
  return (
    typeof body === 'object' &&
    body !== null &&
    typeof (body as Record<string, unknown>)['code'] === 'string' &&
    typeof (body as Record<string, unknown>)['message'] === 'string'
  );
}

function toApiError(error: HttpErrorResponse): ApiError {
  // No response reached the client at all (offline, CORS failure, refused
  // connection, ...) — there is no body to read.
  if (error.status === 0) {
    return { code: 'NETWORK_ERROR', message: error.message };
  }

  // A real HTTP error whose body matches CLAUDE.md §3.3.
  if (isServerErrorBody(error.error)) {
    return {
      code: error.error.code as ErrorCode,
      message: error.error.message,
      details: error.error.details,
    };
  }

  // A real HTTP error, but the body isn't the §3.3 shape (proxy error page,
  // plain text, empty body, ...).
  return { code: 'UNKNOWN_ERROR', message: error.message };
}

/**
 * Normalises every failed HttpClient call into an ApiError so a
 * HttpErrorResponse never reaches a component. Register with
 * provideHttpClient(withInterceptors([errorInterceptor])).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      return throwError(() => toApiError(error));
    }),
  );
