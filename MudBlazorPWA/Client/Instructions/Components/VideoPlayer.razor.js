// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

let iframe
let video

export function init (videoPlayerId) {
  console.log('initializing...')
  // wait for iframe to load
  video = document.getElementById(videoPlayerId)
  video.volume = 0.1
}

// window.addEventListener('message', event => {
//   if (event.data.volume) {
//     video.volume = event.data.volume
//   }
// })

export function getVideDuration () {
  return video.duration
}
