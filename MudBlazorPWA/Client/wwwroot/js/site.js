/* eslint-disable no-unused-vars */
// noinspection JSUnusedGlobalSymbols

function openFilePreview (url) {
  window.open(url, '_blank')
}

function checkOverflowingElements () {
  const selector = '#folder-selector .drop-zone-item > * p'
  const elements = document.querySelectorAll(selector)

  elements.forEach((element) => {
    if (element.offsetWidth < element.scrollWidth) {
      element.parentElement.parentElement.classList.add('overflowing')
    }
  })
}

// Run the function on page load
