import { Link } from 'react-router-dom'

const links = [
  { label: 'About', to: '/about' },
  { label: 'Contact', to: '/contact' },
  { label: 'Privacy', to: '/privacy' },
]

export default function Footer() {
  return (
    <footer className="border-t border-line bg-canvas">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 py-10 sm:flex-row sm:px-6 lg:px-8">
        <div className="flex items-center gap-2 text-sm font-semibold text-ink">
          <span
            aria-hidden="true"
            className="grid h-6 w-6 place-items-center rounded-md bg-primary font-display text-xs text-on-primary"
          >
            M
          </span>
          Membera
        </div>

        <nav aria-label="Footer">
          <ul className="flex flex-wrap items-center justify-center gap-x-6 gap-y-2">
            {links.map((link) => (
              <li key={link.label}>
                <Link
                  to={link.to}
                  className="rounded text-sm text-body transition-colors hover:text-ink focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                >
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        <p className="text-sm text-body">© 2026 Membera</p>
      </div>
    </footer>
  )
}
