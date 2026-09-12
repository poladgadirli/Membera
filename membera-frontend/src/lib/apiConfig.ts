// Shared base URL for the backend API (reached through the Membera.Gateway
// reverse proxy). Kept in its own leaf module (no imports) so both apiClient.ts
// and api.ts's raw-fetch refreshAccessToken() can use it without a circular
// import between the two.

export const API_BASE_URL = (
  (import.meta.env.VITE_API_URL as string | undefined) ??
  'https://localhost:7174/api'
).replace(/\/$/, '')
