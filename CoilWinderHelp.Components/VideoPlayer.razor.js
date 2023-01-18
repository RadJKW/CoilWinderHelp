// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

let video

export function init (videoPlayerId) {
  console.log('initializing...')
  document.getElementById(videoPlayerId).volume = 0.1
}

export function getVideDuration () {
  return video.duration
}
