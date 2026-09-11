// Lightweight client-side JWT decode. No signature verification — the backend
// already validated the token; this only reads the payload for display (name,
// role) and expiry checks.

export type UserRole = 'User' | 'MerchantOwner' | 'Admin' | 'SuperAdmin'

export interface DecodedUser {
  userId: string
  email: string
  firstName: string
  lastName: string
  role: UserRole | string
  /** Expiry as a UNIX epoch in seconds, when present. */
  exp?: number
}

// The Auth API signs tokens with mixed claim shapes: registered names (`sub`,
// `email`), a couple of custom claims (`firstName`, `lastName`) and .NET's
// schema-URI role claim. Check each known spelling.
const ROLE_CLAIM_URI =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAMEID_CLAIM_URI =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
const EMAIL_CLAIM_URI =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'

function base64UrlDecode(input: string): string {
  const padded = input.replace(/-/g, '+').replace(/_/g, '/')
  const withPadding = padded.padEnd(
    padded.length + ((4 - (padded.length % 4)) % 4),
    '=',
  )
  const binary = atob(withPadding)
  // Handle UTF-8 payloads (names with non-ASCII characters).
  const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0))
  return new TextDecoder().decode(bytes)
}

function pickString(
  claims: Record<string, unknown>,
  ...keys: string[]
): string {
  for (const key of keys) {
    const value = claims[key]
    if (typeof value === 'string' && value.length > 0) return value
    if (Array.isArray(value) && typeof value[0] === 'string') return value[0]
  }
  return ''
}

export function decodeToken(token: string): DecodedUser | null {
  const parts = token.split('.')
  if (parts.length < 2) return null

  let claims: Record<string, unknown>
  try {
    claims = JSON.parse(base64UrlDecode(parts[1])) as Record<string, unknown>
  } catch {
    return null
  }

  const userId = pickString(claims, 'sub', 'nameid', NAMEID_CLAIM_URI, 'userId')
  const email = pickString(claims, 'email', EMAIL_CLAIM_URI)
  const role = pickString(claims, 'role', ROLE_CLAIM_URI)

  return {
    userId,
    email,
    firstName: pickString(claims, 'firstName', 'given_name'),
    lastName: pickString(claims, 'lastName', 'family_name'),
    role,
    exp: typeof claims.exp === 'number' ? claims.exp : undefined,
  }
}

export function isTokenExpired(user: DecodedUser | null): boolean {
  if (!user?.exp) return false
  // 10s skew so a token about to expire isn't treated as still valid.
  return Date.now() >= user.exp * 1000 - 10_000
}
