// noinspection JSUnusedGlobalSymbols

let originalAppBarHeight

export function removeAppBar () {
  if (!originalAppBarHeight) {
    originalAppBarHeight = getComputedStyle(document.documentElement).getPropertyValue('--mud-appbar-height')
  }
  document.documentElement.style.setProperty('--mud-appbar-height', '0px')
}

export function restoreAppBar () {
  if (originalAppBarHeight) {
    document.documentElement.style.setProperty('--mud-appbar-height', originalAppBarHeight)
  }
}
