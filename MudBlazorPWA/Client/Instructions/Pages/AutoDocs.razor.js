// noinspection JSUnusedGlobalSymbols

let iframe

export function init () {
  iframe = document.getElementById('pdf-viewer')

  iframe.onload = function () {
    console.log('iframe loaded')
    addScrollbarStyle()
  }

  // TODO: function doesnt add style to correct head under iframe
  function addScrollbarStyle () {
  // get the document object from the iframe
    const doc = iframe.contentDocument || iframe.contentWindow.document

    console.log('doc', doc)

    // get the style element from head of the document and add css to it
    const style = doc.createElement('style')
    style.innerHTML = `
    ::-webkit-scrollbar {
      width: 0.5rem;
    }
    ::-webkit-scrollbar-track {
      background: #f1f1f1;
    }
    ::-webkit-scrollbar-thumb {
      background: #888;
    }
    ::-webkit-scrollbar-thumb:hover {
      background: #555;
    }
  `
    doc.head.appendChild(style)
  }
}
