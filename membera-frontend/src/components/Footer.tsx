import { Link } from 'react-router-dom'
import { LogoMark } from '@/components/LogoMark'

const links = [
  { label: 'About', to: '/about' },
  { label: 'Contact', to: '/contact' },
  { label: 'Privacy', to: '/privacy' },
]

export default function Footer() {
  return (
    <footer className="border-t border-neutral-200/80 bg-[#f7f9fc]">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 py-10 sm:flex-row sm:px-6 lg:px-8">
        <div className="flex items-center gap-2 text-sm font-semibold text-neutral-900">
          <LogoMark className="h-7 w-7" />
          Membera
        </div>

        <nav aria-label="Footer">
          <ul className="flex flex-wrap items-center justify-center gap-x-6 gap-y-2">
            {links.map((link) => (
              <li key={link.label}>
                <Link
                  to={link.to}
                  className="rounded text-sm font-medium text-neutral-500 transition-colors hover:text-neutral-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
                >
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        <p className="text-sm text-neutral-500">© 2026 Membera</p>
      </div>
    </footer>
  )
}
