// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */
let ul
let buttons
export function init (elementId) {
  ul = document.getElementById(elementId)
  buttons = ul.getElementsByTagName('button')
  console.log(buttons)
}

export function disableButtons () {
  for (let i = 0; i < buttons.length; i++) {
    buttons[i].disabled = true
  }
  console.log('buttons disabled')
  console.log(buttons)
}

export function setBackgroundColor () {
  const lis = ul.getElementsByTagName('li')
  const color1 = 'red'
  const color2 = 'purple'
  let currentColor = color1
  console.log(lis)

  for (let i = 0; i < lis.length; i++) {
    lis[i].backgroundColor = currentColor

    if (currentColor === color1) {
      currentColor = color2
    } else {
      currentColor = color1
    }

    const nestedUl = lis[i].getElementsByTagName('ul')
    console.log(nestedUl)
    if (nestedUl.length > 0) {
      setBackgroundColorForNestedUl(nestedUl, color1, color2)
    }
  }
}

function setBackgroundColorForNestedUl (nestedUl, color1, color2) {
  let currentColor = color1
  for (let j = 0; j < nestedUl.length; j++) {
    const nestedLis = nestedUl[j].getElementsByTagName('li')
    for (let k = 0; k < nestedLis.length; k++) {
      nestedLis[k].backgroundColor = currentColor
      if (currentColor === color1) {
        currentColor = color2
      } else {
        currentColor = color1
      }
    }
  }
}
