import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import BrowsePlansPage from './pages/BrowsePlansPage'
import DashboardPage from './pages/DashboardPage'
import LandingPage from './pages/LandingPage'
import LoginPage from './pages/LoginPage'
import NotAuthorizedPage from './pages/NotAuthorizedPage'
import PlaceholderPage from './pages/PlaceholderPage'
import RegisterPage from './pages/RegisterPage'
import SubscriptionCancelPage from './pages/SubscriptionCancelPage'
import SubscriptionSuccessPage from './pages/SubscriptionSuccessPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/signup" element={<RegisterPage />} />
      <Route path="/register" element={<RegisterPage />} />

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
      <Route path="*" element={<PlaceholderPage title="Page not found" />} />
    </Routes>
  )
}

export default App
