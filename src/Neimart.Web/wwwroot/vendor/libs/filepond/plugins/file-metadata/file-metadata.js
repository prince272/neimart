/*!
 * FilePondPluginFileMetadata 1.0.6
 * Licensed under MIT, https://opensource.org/licenses/MIT/
 * Please visit https://pqina.nl/filepond/ for details.
 */

/* eslint-disable */

!function(e,t){"object"==typeof exports&&"undefined"!=typeof module?module.exports=t():"function"==typeof define&&define.amd?define(t):(e=e||self).FilePondPluginFileMetadata=t()}(this,function(){"use strict";var e=function(e){var t=e.addFilter,n=e.utils.Type;return t("SET_ATTRIBUTE_TO_OPTION_MAP",function(e){return Object.assign(e,{"^fileMetadata":{group:"fileMetadataObject"}})}),t("DID_LOAD_ITEM",function(e,t){var n=t.query;return new Promise(function(t){if(!n("GET_ALLOW_FILE_METADATA"))return t(e);var i=n("GET_FILE_METADATA_OBJECT");"object"==typeof i&&null!==i&&Object.keys(i).forEach(function(t){e.setMetadata(t,i[t],!0)}),t(e)})}),{options:{allowFileMetadata:[!0,n.BOOLEAN],fileMetadataObject:[null,n.OBJECT]}}};return"undefined"!=typeof window&&void 0!==window.document&&document.dispatchEvent(new CustomEvent("FilePond:pluginloaded",{detail:e})),e});
