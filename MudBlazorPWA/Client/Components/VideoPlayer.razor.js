// noinspection JSUnusedGlobalSymbols

let video;
let playButton;
let pauseButton;
let refreshButton;
let seekForwardButton;
let seekBackwardButton;
let videoSeeked = false;

export const init = (videoPlayerId, buttonIds) => {
	console.log('initializing...')
	video = document.getElementById(videoPlayerId);
	playButton = document.getElementById(buttonIds[0]);
	pauseButton = document.getElementById(buttonIds[1]);
	refreshButton = document.getElementById(buttonIds[2]);
	seekForwardButton = document.getElementById(buttonIds[3]);
	seekBackwardButton = document.getElementById(buttonIds[4]);

	console.log(video.id);
	console.log(playButton.id);
	console.log(pauseButton.id);
	console.log(refreshButton.id);
	console.log(seekForwardButton.id);
	console.log(seekBackwardButton.id);


	video.volume = 0.1;
	video.addEventListener('seeked', () => {
			videoSeeked = true;
			console.log('seeked');
		}
	);
	video.addEventListener('play', () => {
			if (pauseButton != null) {
				pauseButton.disabled = false;
			}
			if (playButton != null) {
				playButton.disabled = true;
			}
			if (seekBackwardButton != null) {
				seekBackwardButton.disabled = false;
			}
			if (seekForwardButton != null) {
				seekForwardButton.disabled = false;
			}
		}
	);
	video.addEventListener('pause', () => {
			if (pauseButton != null) {
				pauseButton.disabled = true;
			}
			if (playButton != null) {
				playButton.disabled = false;
			}
			if (seekBackwardButton != null) {
				seekBackwardButton.disabled = true;
			}
			if (seekForwardButton != null) {
				seekForwardButton.disabled = true;
			}
		}
	);

	playButton.disabled=false;
	pauseButton.disabled=true;
	refreshButton.disabled=false;

};

export const pauseVideo = () => {
	video.pause();
	pauseButton.disabled = true;
	playButton.disabled = false;

};
export const restartVideo = () => {
	video.pause();
	video.load();
	video.currentTime = 0;
	video.play();
};
export const playVideo = () => {
	video.play();
	playButton.disabled = true;
	pauseButton.disabled = false
};
export const seekVideo = (time, direction) => {
	// if mathtype is 0 then subtract time from current time
	// if mathtype is 1 then add time to current time
	if (direction === false) {
		video.currentTime -= time;
	}
	if (direction === true) {
		video.currentTime += time;
	}


}

// return the duration of the video in seconds
export const getVideoDuration = () => parseInt(video.duration);