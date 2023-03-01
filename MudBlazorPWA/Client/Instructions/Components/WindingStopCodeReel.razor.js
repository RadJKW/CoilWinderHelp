// noinspection JSUnusedGlobalSymbols

export function addHorizontalScrolling (id) {
  const container = document.getElementById(id)
  if (container) {
    container.addEventListener('wheel',
      (event) => {
        if (event.deltaY !== 0) {
          event.preventDefault()
          container.scrollLeft += event.deltaY
        }
      },
      { passive: false })
  }
}

export function removeHorizontalScrolling (id) {
  const container = document.getElementById(id)
  container?.removeEventListener('wheel')
}
