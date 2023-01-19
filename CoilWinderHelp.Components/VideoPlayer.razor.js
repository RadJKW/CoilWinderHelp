// noinspection JSUnusedGlobalSymbols
/* eslint-disable no-unused-vars */

let video

export function init (videoPlayerId) {
  console.log('initializing...')
  video = document.getElementById(videoPlayerId)
  video.volume = 0.1
  // log the video metadata
  video.addEventListener('loadedmetadata', () => {
    console.log('metadata loaded')
    console.log('duration: ' + video.duration)
    console.log('videoWidth: ' + video.videoWidth)
    console.log('videoHeight: ' + video.videoHeight)
    console.log('video..: ' + video)
  })
}

export function getVideDuration () {
  return video.duration
}
