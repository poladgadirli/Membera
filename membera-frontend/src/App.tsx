import { Route, Routes } from 'react-router-dom'
import LandingPage from './pages/LandingPage'
import PlaceholderPage from './pages/PlaceholderPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<PlaceholderPage title="Log In" />} />
      <Route path="/signup" element={<PlaceholderPage title="Sign Up" />} />
      <Route path="/about" element={<PlaceholderPage title="About" />} />
      <Route path="/contact" element={<PlaceholderPage title="Contact" />} />
      <Route path="/privacy" element={<PlaceholderPage title="Privacy" />} />
      <Route path="*" element={<PlaceholderPage title="Page not found" />} />
    </Routes>
  )
}

export default App
