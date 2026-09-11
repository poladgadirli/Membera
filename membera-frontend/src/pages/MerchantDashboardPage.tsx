import { useState } from 'react'
import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import {
  MerchantProfileSection,
  type MerchantProfileStatus,
} from '@/components/merchant/MerchantProfileSection'
import { PlansSection } from '@/components/merchant/PlansSection'
import { RedemptionSection } from '@/components/merchant/RedemptionSection'
import { useAuth } from '@/hooks/useAuth'
import { INFO_BANNER } from '@/lib/ui'

export default function MerchantDashboardPage() {
  const { user } = useAuth()
  const firstName = user?.firstName?.trim()

  // Redemption and Plans both assume a merchant profile exists (they call
  // endpoints that 400 without one), so neither mounts until
  // MerchantProfileSection confirms one is there.
  const [profileStatus, setProfileStatus] = useState<MerchantProfileStatus>('loading')
  const hasProfile = profileStatus === 'ready'

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Merchant dashboard"
        title={firstName ? `Welcome, ${firstName}!` : 'Welcome!'}
        description="Manage your business profile and the subscription plans your customers redeem by QR code."
      />

      <div className="mt-10">
        {hasProfile && <RedemptionSection />}

        {profileStatus === 'setup' && (
          <p className={`${INFO_BANNER} mb-4`}>
            Set up your business profile to start creating subscription plans
            and accepting redemptions.
          </p>
        )}

        <MerchantProfileSection onStatusChange={setProfileStatus} />

        {hasProfile && <PlansSection />}
      </div>
    </DashboardShell>
  )
}
