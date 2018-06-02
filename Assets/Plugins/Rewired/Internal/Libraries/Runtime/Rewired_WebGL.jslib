// Copyright (c) 2017 Augie R. Maddox, Guavaman Enterprises. All rights reserved.
mergeInto(LibraryManager.library,{Rewired_Initialize:function(){if(window.guavamanEnterprisesRewired!==undefined)return;var e={IsGamepadAPIAvailable:function(){var e=navigator.getGamepads||navigator.webkitGamepads||navigator.mozGamepads||navigator.gamepads||navigator.webkitGetGamepads;return e!==undefined},GetGamepads:function(){var e=navigator.getGamepads||navigator.webkitGamepads||navigator.mozGamepads||navigator.gamepads||navigator.webkitGetGamepads;return e!==undefined?e.apply(navigator):[]},GetCurrentGamepadCount:function(){var e=this.GetGamepads();if(e===null)return 0;var t=0;for(var n=0;n<e.length;n++)e[n]&&e[n].connected&&t++;return t},GamepadExists:function(e){return this.GetGamepad(e)!==null},GetGamepad:function(e){var t=this.GetGamepads();if(t===null)return null;for(var n=0;n<t.length;n++)if(t[n]&&t[n].index==e)return t[n];return null},CheckEvent:function(){return this.eventReceived===!1?!1:(this.eventReceived=!1,!0)},GetClientInfo:function(){var e="-",t=navigator.appVersion,n=navigator.userAgent,r=navigator.appName,i=""+parseFloat(navigator.appVersion),s=parseInt(navigator.appVersion,10),o,u,a;(u=n.indexOf("Opera"))!=-1&&(r="Opera",i=n.substring(u+6),(u=n.indexOf("Version"))!=-1&&(i=n.substring(u+8))),(u=n.indexOf("OPR"))!=-1?(r="Opera",i=n.substring(u+4)):(u=n.indexOf("Edge"))!=-1?(r="Microsoft Edge",i=n.substring(u+5)):(u=n.indexOf("MSIE"))!=-1?(r="Microsoft Internet Explorer",i=n.substring(u+5)):(u=n.indexOf("Chrome"))!=-1?(r="Chrome",i=n.substring(u+7)):(u=n.indexOf("Safari"))!=-1?(r="Safari",i=n.substring(u+7),(u=n.indexOf("Version"))!=-1&&(i=n.substring(u+8))):(u=n.indexOf("Firefox"))!=-1?(r="Firefox",i=n.substring(u+8)):n.indexOf("Trident/")!=-1?(r="Microsoft Internet Explorer",i=n.substring(n.indexOf("rv:")+3)):(o=n.lastIndexOf(" ")+1)<(u=n.lastIndexOf("/"))&&(r=n.substring(o,u),i=n.substring(u+1),r.toLowerCase()==r.toUpperCase()&&(r=navigator.appName)),(a=i.indexOf(";"))!=-1&&(i=i.substring(0,a)),(a=i.indexOf(" "))!=-1&&(i=i.substring(0,a)),(a=i.indexOf(")"))!=-1&&(i=i.substring(0,a)),s=parseInt(""+i,10),isNaN(s)&&(i=""+parseFloat(navigator.appVersion),s=parseInt(navigator.appVersion,10));var f=/Mobile|mini|Fennec|Android|iP(ad|od|hone)/.test(t),l=e,c=[{s:"Windows 10",r:/(Windows 10.0|Windows NT 10.0)/},{s:"Windows 8.1",r:/(Windows 8.1|Windows NT 6.3)/},{s:"Windows 8",r:/(Windows 8|Windows NT 6.2)/},{s:"Windows 7",r:/(Windows 7|Windows NT 6.1)/},{s:"Windows Vista",r:/Windows NT 6.0/},{s:"Windows Server 2003",r:/Windows NT 5.2/},{s:"Windows XP",r:/(Windows NT 5.1|Windows XP)/},{s:"Windows 2000",r:/(Windows NT 5.0|Windows 2000)/},{s:"Windows ME",r:/(Win 9x 4.90|Windows ME)/},{s:"Windows 98",r:/(Windows 98|Win98)/},{s:"Windows 95",r:/(Windows 95|Win95|Windows_95)/},{s:"Windows NT 4.0",r:/(Windows NT 4.0|WinNT4.0|WinNT|Windows NT)/},{s:"Windows CE",r:/Windows CE/},{s:"Windows 3.11",r:/Win16/},{s:"Android",r:/Android/},{s:"Open BSD",r:/OpenBSD/},{s:"Sun OS",r:/SunOS/},{s:"Linux",r:/(Linux|X11)/},{s:"iOS",r:/(iPhone|iPad|iPod)/},{s:"Mac OS X",r:/Mac OS X/},{s:"Mac OS",r:/(MacPPC|MacIntel|Mac_PowerPC|Macintosh)/},{s:"QNX",r:/QNX/},{s:"UNIX",r:/UNIX/},{s:"BeOS",r:/BeOS/},{s:"OS/2",r:/OS\/2/},{s:"Search Bot",r:/(nuhk|Googlebot|Yammybot|Openbot|Slurp|MSNBot|Ask Jeeves\/Teoma|ia_archiver)/}];for(var h in c){var p=c[h];if(p.r.test(n)){l=p.s;break}}var d=e;/Windows/.test(l)&&(d=/Windows (.*)/.exec(l)[1],l="Windows");switch(l){case"Mac OS X":d=/Mac OS X (10[\.\_\d]+)/.exec(n)[1];break;case"Android":d=/Android ([\.\_\d]+)/.exec(n)[1];break;case"iOS":d=/OS (\d+)_(\d+)_?(\d+)?/.exec(t),d=d[1]+"."+d[2]+"."+(d[3]|0)}return{browser:r,browserVersion:i,browserMajorVersion:s,mobile:f,os:l,osVersion:d}},GetHashForString:function(e){var t=0,n,r;if(e.length===0)return t;for(n=0;n<e.length;n++)r=e.charCodeAt(n),t=(t<<5)-t+r,t|=0;return t},lengthBytesUTF8:function(e){var t=0;for(var n=0;n<e.length;++n){var r=e.charCodeAt(n);r>=55296&&r<=57343&&(r=65536+((r&1023)<<10)|e.charCodeAt(++n)&1023),r<=127?++t:r<=2047?t+=2:r<=65535?t+=3:r<=2097151?t+=4:r<=67108863?t+=5:t+=6}return t},stringToUTF8Array:function(e,t,n,r){if(r>0){var i=n,s=n+r-1;for(var o=0;o<e.length;++o){var u=e.charCodeAt(o);u>=55296&&u<=57343&&(u=65536+((u&1023)<<10)|e.charCodeAt(++o)&1023);if(u<=127){if(n>=s)break;t[n++]=u}else if(u<=2047){if(n+1>=s)break;t[n++]=192|u>>6,t[n++]=128|u&63}else if(u<=65535){if(n+2>=s)break;t[n++]=224|u>>12,t[n++]=128|u>>6&63,t[n++]=128|u&63}else if(u<=2097151){if(n+3>=s)break;t[n++]=240|u>>18,t[n++]=128|u>>12&63,t[n++]=128|u>>6&63,t[n++]=128|u&63}else if(u<=67108863){if(n+4>=s)break;t[n++]=248|u>>24,t[n++]=128|u>>18&63,t[n++]=128|u>>12&63,t[n++]=128|u>>6&63,t[n++]=128|u&63}else{if(n+5>=s)break;t[n++]=252|u>>30,t[n++]=128|u>>24&63,t[n++]=128|u>>18&63,t[n++]=128|u>>12&63,t[n++]=128|u>>6&63,t[n++]=128|u&63}}return t[n]=0,n-i}return 0},stringToUTF8:function(e,t,n){return this.stringToUTF8Array(e,HEAPU8,t,n)}};window.guavamanEnterprisesRewired=e},Rewired_GetClientInfo:function(){if(window.guavamanEnterprisesRewired===undefined)return null;var e=JSON.stringify(window.guavamanEnterprisesRewired.GetClientInfo()),t=window.guavamanEnterprisesRewired.lengthBytesUTF8(e)+1,n=_malloc(t);return window.guavamanEnterprisesRewired.stringToUTF8(e,n,t),n},Rewired_IsGamepadAPIAvailable:function(){return window.guavamanEnterprisesRewired===undefined?!1:window.guavamanEnterprisesRewired.IsGamepadAPIAvailable()},Rewired_GetGamepadCount:function(){return window.guavamanEnterprisesRewired===undefined?0:window.guavamanEnterprisesRewired.GetCurrentGamepadCount()},Rewired_GetMaxGamepadId:function(){if(window.guavamanEnterprisesRewired===undefined)return-1;var e=window.guavamanEnterprisesRewired.GetGamepads();if(e===null)return-1;var t=-1;for(var n=0;n<e.length;n++)e[n]&&e[n].connected&&e[n].index>t&&(t=e[n].index);return t},Rewired_GetGamepadName:function(e){if(window.guavamanEnterprisesRewired===undefined)return null;var t=guavamanEnterprisesRewired.GetGamepad(e);if(t===null)return null;var n=t.id;if(n===null)return null;var r=window.guavamanEnterprisesRewired.lengthBytesUTF8(n)+1,i=_malloc(r);return window.guavamanEnterprisesRewired.stringToUTF8(n,i,r),i},Rewired_GetGamepadNameHash:function(e){if(window.guavamanEnterprisesRewired===undefined)return 0;var t=guavamanEnterprisesRewired.GetGamepad(e);return t===null?0:window.guavamanEnterprisesRewired.GetHashForString(t.id)},Rewired_GetGamepadMapping:function(e){if(window.guavamanEnterprisesRewired===undefined)return 0;var t=guavamanEnterprisesRewired.GetGamepad(e);if(t===null)return 0;var n=t.mapping;return n==="standard"?1:0},Rewired_GetGamepadMappingString:function(e){if(window.guavamanEnterprisesRewired===undefined)return null;var t=guavamanEnterprisesRewired.GetGamepad(e);if(t===null)return null;var n=t.mapping;if(n===null)return null;var r=window.guavamanEnterprisesRewired.lengthBytesUTF8(n)+1,i=_malloc(r);return window.guavamanEnterprisesRewired.stringToUTF8(n,i,r),i},Rewired_GetGamepadConnected:function(e){if(window.guavamanEnterprisesRewired===undefined)return!1;var t=guavamanEnterprisesRewired.GetGamepad(e);return t===null?!1:t.connected?!0:!1},Rewired_GetGamepadTimestamp:function(e){if(window.guavamanEnterprisesRewired===undefined)return 0;var t=guavamanEnterprisesRewired.GetGamepad(e);return t===null?0:t.timestamp},Rewired_GetGamepadButtonCount:function(e){if(window.guavamanEnterprisesRewired===undefined)return 0;var t=guavamanEnterprisesRewired.GetGamepad(e);return t===null?0:t.buttons.length},Rewired_GetGamepadAxisCount:function(e){if(window.guavamanEnterprisesRewired===undefined)return 0;var t=guavamanEnterprisesRewired.GetGamepad(e);return t===null?0:t.axes.length},Rewired_GetGamepadButtonValue:function(e,t){if(window.guavamanEnterprisesRewired===undefined)return 0;var n=guavamanEnterprisesRewired.GetGamepad(e);if(n===null)return 0;if(t<0||t>=n.buttons.length)return 0;var r=n.buttons[t];return typeof r=="object"?r.value:typeof r=="number"?r:0},Rewired_GetGamepadButtonIsPressed:function(e,t){if(window.guavamanEnterprisesRewired===undefined)return!1;var n=guavamanEnterprisesRewired.GetGamepad(e);if(n===null)return!1;if(t<0||t>=n.buttons.length)return!1;var r=n.buttons[t];return typeof r=="object"?r.pressed:typeof r=="number"?r>0:!1},Rewired_GetGamepadAxisValue:function(e,t){if(window.guavamanEnterprisesRewired===undefined)return 0;var n=guavamanEnterprisesRewired.GetGamepad(e);return n===null?0:t<0||t>=n.axes.length?0:n.axes[t]}});