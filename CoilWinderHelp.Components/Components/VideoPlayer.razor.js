let video;

export const init = (videoPlayerId) => {
    console.log('initializing...')
    video = document.getElementById(videoPlayerId);
    video.volume = 0.1;
};

// return the duration of the video in seconds
export const getVideoDuration = () => parseInt(video.duration);