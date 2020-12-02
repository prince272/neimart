/*!
 * FilePondPluginGetFile 1.0.3
 * Licensed under MIT, https://opensource.org/licenses/MIT/
 * Please visit undefined for details.
 */

/* eslint-disable */

!function(e,t){"object"==typeof exports&&"undefined"!=typeof module?module.exports=t():"function"==typeof define&&define.amd?define(t):(e=e||self).FilePondPluginGetFile=t()}(this,(function(){"use strict";const e=e=>{let t=document.createElement("span");return t.className="filepond--download-icon",t.title=e,t},t=(e,t)=>{if(t&&e.getMetadata("url"))location.href=e.getMetadata("url");else{const t=document.createElement("a"),n=window.URL.createObjectURL(e.file);document.body.appendChild(t),t.style.display="none",t.href=n,t.download=e.file.name,t.click(),window.URL.revokeObjectURL(n),t.remove()}},n=n=>{const{addFilter:o,utils:i}=n,{Type:d,createRoute:l}=i;return o("CREATE_VIEW",n=>{const{is:o,view:i,query:d}=n;if(!o("file"))return;i.registerWriter(l({DID_LOAD_ITEM:({root:n,props:o})=>{const{id:i}=o,l=d("GET_ITEM",i);if(!l||l.archived)return;const r=n.query("GET_LABEL_BUTTON_DOWNLOAD_ITEM"),c=n.query("GET_ALLOW_DOWNLOAD_BY_URL");((n,o,i,d)=>{const l=o.querySelector(".filepond--file-info-main"),r=e(i);l.prepend(r),r.addEventListener("click",()=>t(n,d))})(l,n.element,r,c)}},({root:e,props:t})=>{const{id:n}=t;d("GET_ITEM",n);e.rect.element.hidden}))}),{options:{labelButtonDownloadItem:["Download file",d.STRING],allowDownloadByUrl:[!1,d.BOOLEAN]}}};return"undefined"!=typeof window&&void 0!==window.document&&document.dispatchEvent(new CustomEvent("FilePond:pluginloaded",{detail:n})),n}));
