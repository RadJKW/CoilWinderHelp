// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

let iframe
let video

export function init (videoPlayerId) {
  console.log('initializing...')
  // wait for iframe to load
  iframe = document.getElementById(videoPlayerId)
  iframe.onload = () => {
    video = iframe.contentDocument.getElementsByTagName('video')[0]
    console.log(video)
    video.volume = 0.1
    // iframe.contentWindow.postMessage({ volume: 0.1 }, '*')
  }
}

// window.addEventListener('message', event => {
//   if (event.data.volume) {
//     video.volume = event.data.volume
//   }
// })

export function getVideDuration () {
  return video.duration
}
