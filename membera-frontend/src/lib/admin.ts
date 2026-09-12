// Admin user-management calls. Thin wrappers over the shared api client
// (src/lib/apiClient.ts). All routes require the Admin or SuperAdmin role; the
// promote/demote/delete-admin routes are SuperAdmin-only (enforced by the
// backend AdminController — the UI mirrors those rules but the server is
// authoritative).
//
// Backend routes (through the gateway, prefix /api):
//   GET    /admin/users?page&pageSize&search&role -> { users: [...], totalCount, page, pageSize }
//   DELETE /admin/users/{id}     -> 204   (delete a non-admin user)
//   POST   /admin/promote/{id}   -> 204   (SuperAdmin only — User -> Admin)
//   POST   /admin/demote/{id}    -> 204   (SuperAdmin only — Admin -> User)
//   DELETE /admin/{id}           -> 204   (SuperAdmin only — delete an Admin account)

import { api } from '@/lib/apiClient'

export { ApiError } from '@/lib/apiClient'

export interface AdminUser {
  id: string
  firstName: string
  lastName: string
  email: string
  role: string
  /** Soft-delete flag from the backend (User.IsDeleted). */
  isDeleted: boolean
  /** ISO timestamp. */
  createdAt: string
}

export interface PagedUsers {
  users: AdminUser[]
  totalCount: number
}

export interface GetAllUsersFilters {
  /** Case-insensitive substring match against first name, last name, or email. */
  search?: string
  /** Exact role match: User | MerchantOwner | Admin | SuperAdmin. */
  role?: string
}

export async function getAllUsers(
  page: number,
  pageSize: number,
  filters: GetAllUsersFilters = {},
): Promise<PagedUsers> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })
  if (filters.search?.trim()) params.set('search', filters.search.trim())
  if (filters.role) params.set('role', filters.role)

  // BaseResponse<GetAllUsersResult> -> client strips the envelope ->
  // { users: [...], totalCount, page, pageSize }.
  const data = await api.get<{ users?: AdminUser[]; totalCount?: number }>(
    `/admin/users?${params.toString()}`,
  )
  return { users: data.users ?? [], totalCount: data.totalCount ?? 0 }
}

/** Delete a non-admin user account. */
export function deleteUser(userId: string): Promise<void> {
  return api.delete<void>(`/admin/users/${userId}`)
}

/** Promote a User to Admin. SuperAdmin only. */
export function promoteToAdmin(userId: string): Promise<void> {
  return api.post<void>(`/admin/promote/${userId}`)
}

/** Demote an Admin back to User. SuperAdmin only. */
export function demoteAdmin(userId: string): Promise<void> {
  return api.post<void>(`/admin/demote/${userId}`)
}

/** Delete an Admin account. SuperAdmin only. */
export function deleteAdminAccount(adminId: string): Promise<void> {
  return api.delete<void>(`/admin/${adminId}`)
}
