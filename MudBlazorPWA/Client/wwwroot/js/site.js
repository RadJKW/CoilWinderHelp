// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

function openFilePreview (url) {
  window.open(url, '_blank')
}

function checkOverflowingElements () {
  console.log('checkOverflowingElements')
  const selector = '#drop-zone-chip > * p'
  const elements = document.querySelectorAll(selector)

  elements.forEach((element) => {
    if (element.offsetWidth < element.scrollWidth) {
      element.parentElement.parentElement.classList.add('overflowing')
    }
  })
  addAttributeToElements()
}

function addAttributeToElements () {
  const selector = '#folder-selector .drop-zone-gallery #drop-zone-chip'
  const elements = document.querySelectorAll(selector)

  elements.forEach((element) => {
    // if the element does not have a [data-title] attribute,
    // or if the data-title doesnt have any text, dont transfer it to the parent
    const tooltipText = element.attributes['data-title']?.value
    if (!tooltipText) return
    AddTooltipEventListeners(element, tooltipText)
    // add the attribute to the parent
  })
}

function createCustomTooltip () {
  const tooltip = document.createElement('div')
  // get the first parent of the element that has a class of 'drop-zone-gallery'
  tooltip.classList.add('custom-tooltip')
  // Append the tooltip to the body instead of the chip element to avoid being constrained by the chip's container
  document.body.appendChild(tooltip)
}
function AddTooltipEventListeners (element, tooltipText) {
  const tooltip = document.querySelector('.custom-tooltip')
  tooltip.innerHTML = tooltipText
  element.addEventListener('mouseenter', () => {
    const elementRect = element.getBoundingClientRect()
    const elementRight = elementRect.right
    const elementLeft = elementRect.left
    const elementBottom = elementRect.bottom
    const elementTop = elementRect.top

    tooltip.innerHTML = tooltipText
    tooltip.style.left = `${elementLeft}px`
    tooltip.style.right = `${elementRight}px`
    tooltip.style.top = `${elementTop + element.offsetHeight}px` // Adjust the value as needed for vertical positioning
    tooltip.style.visibility = 'visible'
    tooltip.style.maxWidth = `${element.offsetWidth}px`
  })

  element.addEventListener('mouseleave', () => {
    tooltip.style.visibility = 'hidden'
  })
}

// Run the function on page load
