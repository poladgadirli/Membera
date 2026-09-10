import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import { MerchantProfileSection } from '@/components/merchant/MerchantProfileSection'
import { PlansSection } from '@/components/merchant/PlansSection'
import { useAuth } from '@/hooks/useAuth'

export default function MerchantDashboardPage() {
  const { user } = useAuth()
  const firstName = user?.firstName?.trim()

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Merchant dashboard"
        title={firstName ? `Welcome, ${firstName}!` : 'Welcome!'}
        description="Manage your business profile and the subscription plans your customers redeem by QR code."
      />

      <div className="mt-10">
        <MerchantProfileSection />
        <PlansSection />
      </div>
    </DashboardShell>
  )
}
