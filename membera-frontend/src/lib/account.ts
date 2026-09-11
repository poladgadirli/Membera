// Account self-service calls (change password / email for the signed-in
// user). Thin wrappers over the shared api client (src/lib/apiClient.ts),
// which attaches the bearer token, unwraps BaseResponse<T> and throws
// ApiError. Both endpoints reply 204 No Content on success.

import { api } from '@/lib/apiClient'

export { ApiError } from '@/lib/apiClient'

export interface ChangePasswordPayload {
  currentPassword: string
  newPassword: string
}

export function changePassword(payload: ChangePasswordPayload): Promise<void> {
  return api.post<void>('/auth/change-password', payload)
}

export interface ChangeEmailPayload {
  newEmail: string
  currentPassword: string
}

export function changeEmail(payload: ChangeEmailPayload): Promise<void> {
  return api.post<void>('/auth/change-email', payload)
}
