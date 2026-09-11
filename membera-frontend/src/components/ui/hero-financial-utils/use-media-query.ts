import { useEffect, useState } from 'react'

/**
 * Subscribes to a CSS media query and returns whether it currently matches.
 * SSR/first-render safe: starts `false` and syncs on mount.
 */
export function useMediaQuery(query: string): boolean {
  const [matches, setMatches] = useState(false)

  useEffect(() => {
    const media = window.matchMedia(query)
    const update = () => setMatches(media.matches)

    update()
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [query])

  return matches
}
