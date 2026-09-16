import { Route } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { RouteTransition } from './components/RouteTransition'
import AccountSettingsPage from './pages/AccountSettingsPage'
import BrowsePlansPage from './pages/BrowsePlansPage'
import DashboardPage from './pages/DashboardPage'
import ForgotPasswordPage from './pages/ForgotPasswordPage'
import LandingPage from './pages/LandingPage'
import LoginPage from './pages/LoginPage'
import NotAuthorizedPage from './pages/NotAuthorizedPage'
import NotFoundPage from './pages/NotFoundPage'
import PlaceholderPage from './pages/PlaceholderPage'
import RegisterPage from './pages/RegisterPage'
import SubscriptionCancelPage from './pages/SubscriptionCancelPage'
import SubscriptionSuccessPage from './pages/SubscriptionSuccessPage'
import VerifyEmailPage from './pages/VerifyEmailPage'

function App() {
  return (
    <RouteTransition>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/signup" element={<RegisterPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />

      {/* `/dashboard` renders the role-specific dashboard (User / MerchantOwner /
          Admin) — see DashboardPage. */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />

      {/* verify-email/resend-verification both require a session, so this
          step only ever runs while already signed in. */}
      <Route
        path="/verify-email"
        element={
          <ProtectedRoute>
            <VerifyEmailPage />
          </ProtectedRoute>
        }
      />

      {/* Any signed-in role manages their own password/email here. */}
      <Route
        path="/account"
        element={
          <ProtectedRoute>
            <AccountSettingsPage />
          </ProtectedRoute>
        }
      />

      {/* Browse + checkout flow: any signed-in user, no role restriction. */}
      <Route
        path="/plans"
        element={
          <ProtectedRoute>
            <BrowsePlansPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscription-success"
        element={
          <ProtectedRoute>
            <SubscriptionSuccessPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscription-cancel"
        element={
          <ProtectedRoute>
            <SubscriptionCancelPage />
          </ProtectedRoute>
        }
      />

      <Route path="/not-authorized" element={<NotAuthorizedPage />} />

      <Route path="/about" element={<PlaceholderPage title="About" />} />
      <Route path="/contact" element={<PlaceholderPage title="Contact" />} />
      <Route path="/privacy" element={<PlaceholderPage title="Privacy" />} />

      {/* Catch-all — must stay last so it only matches unknown routes. */}
      <Route path="*" element={<NotFoundPage />} />
    </RouteTransition>
  )
}

export default App
