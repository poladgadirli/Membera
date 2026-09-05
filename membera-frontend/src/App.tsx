import { Route, Routes } from 'react-router-dom'
import LandingPage from './pages/LandingPage'
import LoginPage from './pages/LoginPage'
import PlaceholderPage from './pages/PlaceholderPage'
import RegisterPage from './pages/RegisterPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/signup" element={<RegisterPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/about" element={<PlaceholderPage title="About" />} />
      <Route path="/contact" element={<PlaceholderPage title="Contact" />} />
      <Route path="/privacy" element={<PlaceholderPage title="Privacy" />} />
      <Route path="*" element={<PlaceholderPage title="Page not found" />} />
    </Routes>
  )
}

export default App
