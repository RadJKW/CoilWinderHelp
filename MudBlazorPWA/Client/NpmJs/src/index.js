// Module Manager for registering the modules of the chart
import { ModuleManager } from 'igniteui-webcomponents-core'
// Radial Gauge Module
import { IgcRadialGaugeModule } from 'igniteui-webcomponents-gauges'

// register the modules
ModuleManager.register(
  IgcRadialGaugeModule
)

window.updateValue = function (value) {
  document.getElementById('rg').value = value
}

window.pauseVideo = function () {
  document.getElementById('videoPlayer').pause()
}

window.restartVideo = function () {
  const video = document.getElementById('videoPlayer')
  video.pause()
  video.currentTime = 0
  video.load()
  video.play()
}

window.playVideo = function () {
  document.getElementById('videoPlayer').play()
}

window.seekVideo = function (time) {
  document.getElementById('videoPlayer').currentTime = time
}

