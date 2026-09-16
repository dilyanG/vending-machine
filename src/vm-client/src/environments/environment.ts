/**
 * Production environment. `apiBaseUrl` is empty so every request is relative
 * (e.g. `/api/products`) and resolves against whatever origin actually served
 * this app — never hard-code a host here.
 */
export const environment = {
  production: true,
  apiBaseUrl: '',
};
