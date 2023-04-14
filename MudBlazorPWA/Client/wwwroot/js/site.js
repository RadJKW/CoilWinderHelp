// noinspection JSUnusedGlobalSymbols

//region: JSInterop functions
function openFilePreview(url) {
  window.open(url, "_blank");
  console.log("openFilePreview @ -> ", url);
}
function toggleDropItemsDense(bool) {
  const dropZones = document.querySelectorAll(
    "#folder-selector div.drop-zone-gallery"
  );
  if (bool) {
    dropZones.forEach((item) => {
      item.classList.add("flex-column");
    });
  } else {
    dropZones.forEach((item) => {
      item.classList.remove("flex-column");
    });
  }
}
function checkOverflowingElements() {
  const selector = "#drop-zone-chip > * p";
  const elements = document.querySelectorAll(selector);

  elements.forEach((element) => {
    if (element.offsetWidth < element.scrollWidth) {
      element.parentElement.parentElement.classList.add("overflowing");
    }
  });
  addAttributeToElements();
}
function createCustomTooltip() {
  const tooltip = document.createElement("div");
  // get the first parent of the element that has a class of 'drop-zone-gallery'
  tooltip.classList.add("custom-tooltip");
  // Append the tooltip to the body instead of the chip element to avoid being constrained by the chip's container
  document.body.appendChild(tooltip);
}
function getVideoThumbnail(videoURL) {
  console.log("getVideoThumbnail @ -> ", videoURL);
  return new Promise(async (resolve) => {
    const video = document.createElement("video");
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    video.src = videoURL;
    video.currentTime = 0;

    video.onloadedmetadata = async () => {
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;

      await new Promise((resolveSeek) => {
        video.onseeked = () => {
          resolveSeek();
        };
      });

      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      resolve(canvas.toDataURL());
    };
  });
}
function setImageSrc(image, src) {
  image.src = src;
}
//endregion

//region: Methods

/* create a function that
 * 1. checks to see if the there is a div element with id="folder-selector"
 * 2. if there is, then we want to first check to see if the div.mud-drop-zone.drop-zone-gallery elements in
 *   the folder-selector div is overflowing (ie: has a scroll bar)
 * 3. if it is overflowing, then we want to add a class of 'overflowing' to the div.mud-drop-zone.drop-zone-gallery
 * 4. if it is not overflowing, then we want to remove the class of 'overflowing' from the div.mud-drop-zone.drop-zone-gallery
 *    if it exists
 * // the above loop will be ran when the function is called form Blazor JSRuntime when the page loads
 * // after the loop finished, we want to add a listener to the page so that if any changes are made to the DOM
 * // the loop will be ran again.
 * // the listener can be added to the div#folder-selector
 */
function watchFolderSelector() {
  const folderSelector = document.querySelector("#folder-selector");
  console.log("folderSelector -> ", folderSelector);
  if (!folderSelector) return;
  const dropZones = folderSelector.querySelectorAll(
    "div.mud-list-drop-content"
  );
  console.log("dropZones -> ", dropZones);
  dropZones.forEach((item) => {
    if (item.scrollHeight > item.clientHeight) {
      item.classList.add("overflowing");
    } else {
      item.classList.remove("overflowing");
    }
  });
  folderSelector.addEventListener("DOMSubtreeModified", () => {
    checkOverflowingElements();
  });
}

function addAttributeToElements() {
  const selector = "#folder-selector .drop-zone-gallery #drop-zone-chip";
  const elements = document.querySelectorAll(selector);

  elements.forEach((element) => {
    // if the element does not have a [data-title] attribute,
    // or if the data-title doesnt have any text, dont transfer it to the parent
    const tooltipText = element.attributes["data-title"]?.value;
    if (!tooltipText) return;
    AddTooltipEventListeners(element, tooltipText);
    // add the attribute to the parent
  });
}
function AddTooltipEventListeners(element, tooltipText) {
  const tooltip = document.querySelector(".custom-tooltip");
  tooltip.innerHTML = tooltipText;
  element.addEventListener("mouseenter", () => {
    const elementRect = element.getBoundingClientRect();
    const elementRight = elementRect.right;
    const elementLeft = elementRect.left;
    const elementTop = elementRect.top;

    tooltip.innerHTML = tooltipText;
    tooltip.style.left = `${elementLeft}px`;
    tooltip.style.right = `${elementRight}px`;
    tooltip.style.top = `${elementTop + element.offsetHeight}px`; // Adjust the value as needed for vertical positioning
    tooltip.style.visibility = "visible";
    tooltip.style.maxWidth = `${element.offsetWidth}px`;
  });

  element.addEventListener("mouseleave", () => {
    tooltip.style.visibility = "hidden";
  });
}
//endregion
