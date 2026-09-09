import { DashboardShell } from '@/components/DashboardShell'
import { MerchantProfileSection } from '@/components/merchant/MerchantProfileSection'
import { PlansSection } from '@/components/merchant/PlansSection'
import { useAuth } from '@/hooks/useAuth'

export default function MerchantDashboardPage() {
  const { user } = useAuth()
  const firstName = user?.firstName?.trim()

  return (
    <DashboardShell>
      <p className="text-sm font-medium text-muted-foreground">Merchant dashboard</p>
      <h1 className="mt-1 text-3xl font-semibold tracking-tight text-foreground">
        {firstName ? `Welcome, ${firstName}!` : 'Welcome!'}
      </h1>
      <p className="mt-2 max-w-prose text-sm text-muted-foreground">
        Manage your business profile and the subscription plans your customers
        redeem by QR code.
      </p>

      <div className="mt-8">
        <MerchantProfileSection />
        <PlansSection />
      </div>
    </DashboardShell>
  )
}
