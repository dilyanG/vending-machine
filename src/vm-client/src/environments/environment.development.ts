/**
 * Development environment (swapped in for the `development` build
 * configuration via angular.json's fileReplacements). `apiBaseUrl` stays
 * empty so requests go to `/api` and the dev-server proxy (proxy.conf.json)
 * forwards them to the backend on :5080 — never hard-code that host here.
 */
export const environment = {
  production: false,
  apiBaseUrl: '',
};
