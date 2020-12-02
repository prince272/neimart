function parseObject(str) {
    try {
        return JSON.parse(str);
    }
    catch (e) {
        // nothing special
    }

    return null;
}

function phraseError(error) {
    var message = '';

    if (typeof error === 'object') {
        if (error && error.detail) {
            message = error.detail;
        }
        else if (error && error.status != null) {
            error = error.status;
        }
    }

    if (error == 0) message = 'Not connected. Please verify your network connection. (0)';
    else if (error == 400) message = 'Bad Request. (400)';
    else if (error == 401) message = 'Unauthorized. (401)';
    else if (error == 402) message = 'Payment Required. (402)';
    else if (error == 403) message = 'Forbidden. (403)';
    else if (error == 404) message = 'Not Found. (404)';
    else if (error == 405) message = 'Method Not Allowed. (405)';
    else if (error == 406) message = 'Not Acceptable. (406)';
    else if (error == 407) message = 'Proxy Authentication Required. (407)';
    else if (error == 408) message = 'Request Timeout. (408)';
    else if (error == 409) message = 'Conflict. (409)';
    else if (error == 410) message = 'Gone. (410)';
    else if (error == 411) message = 'Length Required. (411)';
    else if (error == 412) message = 'Precondition Failed. (412)';
    else if (error == 413) message = 'Payload Too Large. (413)';
    else if (error == 413) message = 'Payload Too Large. (413)';
    else if (error == 414) message = 'URI Too Long. (414)';
    else if (error == 414) message = 'URI Too Long. (414)';
    else if (error == 415) message = 'Unsupported Media Type. (415)';
    else if (error == 416) message = 'Range Not Satisfiable. (416)';
    else if (error == 416) message = 'Range Not Satisfiable. (416)';
    else if (error == 417) message = 'Expectation Failed. (417)';
    else if (error == 418) message = 'I&#39;m a teapot. (418)';
    else if (error == 419) message = 'Authentication Timeout. (419)';
    else if (error == 421) message = 'Misdirected Request. (421)';
    else if (error == 422) message = 'Unprocessable Entity. (422)';
    else if (error == 423) message = 'Locked. (423)';
    else if (error == 424) message = 'Failed Dependency. (424)';
    else if (error == 426) message = 'Upgrade Required. (426)';
    else if (error == 428) message = 'Precondition Required. (428)';
    else if (error == 429) message = 'Too Many Requests. (429)';
    else if (error == 431) message = 'Request Header Fields Too Large. (431)';
    else if (error == 451) message = 'Unavailable For Legal Reasons. (451)';
    else if (error == 500) message = 'Internal Server Error. (500)';
    else if (error == 501) message = 'Not Implemented. (501)';
    else if (error == 502) message = 'Bad Gateway. (502)';
    else if (error == 503) message = 'Service Unavailable. (503)';
    else if (error == 504) message = 'Gateway Timeout. (504)';
    else if (error == 505) message = 'HTTP Version Not Supported. (505)';
    else if (error == 506) message = 'Variant Also Negotiates. (506)';
    else if (error == 507) message = 'Insufficient Storage. (507)';
    else if (error == 508) message = 'Loop Detected. (508)';
    else if (error == 510) message = 'Not Extended. (510)';
    else if (error == 511) message = 'Network Authentication Required. (511)';
    else if (error === 'parsererror') message = 'Requested JSON parse failed.';
    else if (error === 'timeout') message = 'Time out error.';
    else if (error === 'abort') message = 'Ajax request aborted.';

    var defaultMessage = 'An error has occurred. Please try again.';
    return message ? message : defaultMessage;
}

// How do I remove all null and empty string values from a json object?
// source: https://stackoverflow.com/questions/286141/remove-blank-attributes-from-an-object-in-javascript#answer-38340730
function cleanObject(obj) {
    var newObj = {};

    Object.keys(obj).forEach(key => {
        if (obj[key] && typeof obj[key] === "object") {
            newObj[key] = cleanObject(obj[key]); // recurse
        } else if (obj[key] !== undefined && obj[key] !== null && obj[key] !== '') {
            newObj[key] = obj[key]; // copy value
        }
    });

    return newObj;
}

// Will remove all falsy values: undefined, null, 0, false, NaN and "" (empty string)
function cleanArray(arr) {
    var newArray = new Array();
    for (var i = 0; i < arr.length; i++) {
        if (arr[i]) {
            newArray.push(arr[i]);
        }
    }
    return newArray;
}

// Equivalent of String.format in jQuery
// source: https://stackoverflow.com/questions/1038746/equivalent-of-string-format-in-jquery
function formatString(str, obj) {
    obj = typeof obj === 'object' ? obj : Array.prototype.slice.call(arguments, 1);

    return str.replace(/\{\{|\}\}|\{(\w+)\}/g, function (m, n) {
        if (m == "{{") { return "{"; }
        if (m == "}}") { return "}"; }
        return obj[n];
    });
}

// Regex to get string between curly braces “{I want what's between the curly braces}”
// source: https://stackoverflow.com/questions/413071/regex-to-get-string-between-curly-braces-i-want-whats-between-the-curly-brace
function formatQuantity(str, quantity) {
    return str.replace(/{(.*?)}/, function (m, n) {
        if (n.indexOf('||') != -1) {
            var forms = n.split('||');
            var form = quantity == 1 ? 0 : 1;
            return forms[form];
        }
        return m;
    });
}

// JavaScript: How to decode an encode HTML-entities
// source: https://medium.com/@tertiumnon/js-how-to-decode-html-entities-8ea807a140e5
function encodeHTMLEntities(text) {
    return $("<textarea/>")
        .text(text)
        .html();
}

function decodeHTMLEntities(text) {
    return $("<textarea/>")
        .html(text)
        .text();
}

function setLocation(url, target) {
    var anchor;
    anchor = $('<a /></a>', {
        href: url,
        target: target,
        style: 'display: none;'
    });

    anchor.appendTo('body')[0].click();
}

function serializeObject(obj, keyName, valueName) {
    keyName = keyName || 'name';
    valueName = valueName || 'value';

    var o = {};
    var a = obj;
    $.each(a, function () {
        if (o[this[keyName]]) {
            if (!o[this[keyName]].push) {
                o[this[keyName]] = [o[this[keyName]]];
            }
            o[this[keyName]].push(this[valueName] || '');
        } else {
            o[this[keyName]] = this[valueName] || '';
        }
    });
    return o;
}

function reloadLocation(method) {
    if (method == 'get') {
        var loc = window.location;
        window.location = loc.protocol + '//' + loc.host + loc.pathname + loc.search;
    }
    else {
        window.location.reload();
    }
}

// jQuery non-AJAX POST
// source: https://stackoverflow.com/questions/5524045/jquery-non-ajax-post
function submitForm(action, method, obj) {

    method = method.toLowerCase();

    var form;
    form = $('<form />', {
        action: action,
        method: method,
        style: 'display: none;'
    });
    if (typeof obj !== 'undefined') {

        $.each(obj, function (name, value) {

            if (typeof value === 'object') {

                $.each(value, function (objName, objValue) {

                    $('<input />', {
                        type: 'hidden',
                        name: name + '[]',
                        value: objValue
                    }).appendTo(form);
                });
            }
            else {

                $('<input />', {
                    type: 'hidden',
                    name: name,
                    value: value
                }).appendTo(form);
            }
        });
    }

    if (method != 'get') {
        $('<input />', {
            type: 'hidden',
            name: xsrf.formFieldName,
            value: xsrf.requestToken
        }).appendTo(form);

        form.appendTo('body').submit();
    }
    else {
        setLocation(composeUrl(action, obj));
    }
}

// How to get URL parameter using jQuery or plain JavaScript?
// source: https://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript
function getQueryParameter(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

// Query-string encoding of a Javascript Object
// source: https://stackoverflow.com/questions/1714786/query-string-encoding-of-a-javascript-object#answer-1714899
function parseQueryString(obj) {
    var str = [];
    for (var p in obj)
        if (obj.hasOwnProperty(p)) {
            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    var query = str.join("&");
    return query ? '?' + query : '';
}

function composeUrl(url, obj) {
    return url + (obj ? parseQueryString(obj) : '');
}

function getFiliationStyle(element, propertyName) {
    var propertyValue = element.css(propertyName);
    var defaultPropertyValue = $('<div></div>').appendTo('body').css(propertyName);
    if (propertyValue === defaultPropertyValue) {
        return element.parents().filter(function () {
            return $(this).css(propertyName) != defaultPropertyValue
        }).first().css(propertyName);
    } else {
        return propertyValue
    }
}

// How to create GUID / UUID?
// source: https://stackoverflow.com/questions/105034/how-to-create-guid-uuid
function generateUUID() { // Public Domain/MIT
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}