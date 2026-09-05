import CtaBanner from '../components/CtaBanner'
import Footer from '../components/Footer'
import Hero from '../components/Hero'
import HowItWorks from '../components/HowItWorks'
import Navbar from '../components/Navbar'
import WhoItsFor from '../components/WhoItsFor'

export default function LandingPage() {
  return (
    <div className="flex min-h-screen flex-col bg-canvas">
      <Navbar />
      <main className="flex-1">
        <Hero />
        <HowItWorks />
        <WhoItsFor />
        <CtaBanner />
      </main>
      <Footer />
    </div>
  )
}
