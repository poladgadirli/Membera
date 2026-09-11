import { useContext } from 'react'
import { AuthContext, type AuthContextValue } from '@/context/auth'

/** Read the current session (user, role, tokens) and the login/logout actions. */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an <AuthProvider>.')
  }
  return context
}
