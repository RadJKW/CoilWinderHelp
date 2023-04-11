// noinspection JSUnusedGlobalSymbols

let video;

export function init(videoPlayerId) {
  console.log("initializing...");
  // wait for iframe to load
  video = document.getElementById(videoPlayerId);
  video.volume = 0.1;
  video.addEventListener("ended", videoEnded);
}

export function videoEnded() {
  console.log("video ended");
  video.autoplay = false;
  video.load();
}

export function reload(videoUrl) {
  // stop the current video,
  // update the src
  // reload with autoplay = true
  video.pause();
  video.autoplay = true;
  video.src = videoUrl;
  video.load();
}
