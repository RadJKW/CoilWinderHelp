// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

let video

export function init (videoPlayerId) {
  console.log('initializing...')
  // wait for iframe to load
  video = document.getElementById(videoPlayerId)
  video.volume = 0.1
}
