// noinspection JSUnusedGlobalSymbols

let video;
let playButton;
let pauseButton;
let videoSeeked = false;

export const init = () => {
    video = document.getElementById('videoPlayer');
    playButton = document.getElementById('playButton');
    pauseButton = document.getElementById('pauseButton');
    
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
        }
    );
    video.addEventListener('pause', () => {
            if (pauseButton != null) {
                pauseButton.disabled = true;
            }
            if (playButton != null) {
                playButton.disabled = false;
            }
        }
    );
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