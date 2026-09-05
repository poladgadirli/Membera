import CtaBanner from '../components/CtaBanner'
import Footer from '../components/Footer'
import HowItWorks from '../components/HowItWorks'
import WhoItsFor from '../components/WhoItsFor'
import { HeroFinancial } from '@/components/ui/hero-financial'

export default function LandingPage() {
  return (
    <div className="flex min-h-screen flex-col bg-[#f7f9fc]">
      <main className="flex-1">
        <HeroFinancial />
        <HowItWorks />
        <WhoItsFor />
        <CtaBanner />
      </main>
      <Footer />
    </div>
  )
}
