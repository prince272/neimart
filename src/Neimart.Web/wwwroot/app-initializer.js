(function () {
    setupComponents(document);
})();

$(function () {
    initComponents(document);
});

function setupComponents(owner) {
    initJSPrototypes(owner);
    initJQPrototypes(owner);
    initValidator(owner);
    initSharer(owner);
    initInterface(owner);
}

function initComponents(owner) {
    initAsterisks(owner);
    initStatePreserver(owner);
    initDoubleSelector(owner);
    initSelect2(owner);
    initCleave(owner);
    initFormSubmit(owner);
    initBSPaginator(owner);
    initLozad(owner);
    initFocus(owner);
    initTooltip(owner);
    initClipboard(owner);
    initIntlTelInput(owner);
    initPerfectScrollbar(owner);
    initAnchorLink(owner);
    initRateyo(owner);
    initBSSlider(owner);
    initCollapser(owner);
    initCRS(owner);
    initAnimatedHeadline(owner);
    initNavNowrap(owner);
}

function initValidator(owner) {
    $.validator.addMethod('equaltovalue', function (value, element, params) {
        if ($(element).is(':checkbox')) {
            if ($(element).is(':checked')) {
                return value && value.toLowerCase() === 'true';
            } else {
                return value && value.toLowerCase() === 'false';
            }
        }

        return false;
    });

    if ($.validator.unobtrusive) {
        $.validator.unobtrusive.adapters.addBool('equaltovalue');
    }

    $.validator.setDefaults({
        // This will ignore all hidden elements alongside `contenteditable` elements
        // that have no `name` attribute
        ignore: ':hidden, [contenteditable="true"]:not([name])'
    });
}

function initSharer(owner) {
    window.Sharer.init();
}

function initInterface(owner) {
    // source: https://stackoverflow.com/questions/19305821/multiple-modals-overlay
    $(document).on('show.bs.modal', '.modal', function () {
        var zIndex = 2040 + (10 * $('.modal').length);
        var model = $(this);
        model.css('z-index', zIndex);
        model.attr('data-z-index', zIndex);
    });

    $(document).on('shown.bs.modal', '.modal', function () {
        var model = $(this);
        var zIndex = model.attr('data-z-index');

        model = model.next('.modal-backdrop.show');

        if (!model.length) {
            model = $('.modal-backdrop.show');
        }

        model.css('z-index', zIndex - 1);
    });

    var $scrolltop = $('#scroll-to-top');

    $(window).on('scroll', function () {
        if ($(this).scrollTop() > ($(this).height() * 3)) {
            $scrolltop.fadeIn(400);
        } else {
            $scrolltop.fadeOut(400);
        }
    });

    $scrolltop.on('click', function (event) {
        event.preventDefault();
        $("html, body").animate({
            scrollTop: 0
        }, 1500, 'easeInOutExpo');
    });


    // Open the modal when the document is ready from the modal url in the url of the requested page.
    $(function () {
        var modalUrl = getQueryParameter('modalUrl');

        if (modalUrl) {
            showAjaxModal(modalUrl);
        }
    });

    // jQuery Horizontal scroll center - JSFiddle - Code Playground
    // source: https://jsfiddle.net/onigetoc/6bfuspkq/
    $('.list-group', owner).each(function (index, el) {
        var element = $(el);

        if (element.find('.list-group-item-action.active').length != 0) {
            element.scrollCenterORI(element.find('.active'), 0);
        }
    });

    // disable mousewheel on a input number field when in focus
    // (to prevent Cromium browsers change the value when scrolling)
    $(owner).on('focus', 'input[type=number]', function (e) {
        $(this).on('wheel.disableScroll', function (e) {
            e.preventDefault();
        });
    });

    $(owner).on('blur', 'input[type=number]', function (e) {
        $(this).off('wheel.disableScroll');
    });

    // Reparse validator unobtrusive.
    $('form', owner).each(function (i, e) {
        var form = $(e);

        if (!form.data('validator')) {
            reparseFormValidator(form);
        }
    });

    // Prevent dropdown from closing when clicked on.
    $('.dropdown-menu.dropdown-static', owner).on('click', function (event) {
        event.stopPropagation();
    });

    // Ensure scrollbar adjust to the active element.
    var navScrollCenter = $('.nav.nav-scroll-center', owner);
    if (navScrollCenter.has('.active').length != 0) {
        navScrollCenter.scrollCenter(navScrollCenter.find('.active'), 0);
    }
}

function reparseFormValidator(form) {
    var validator = $(form).validate();

    if (validator) {
        validator.destroy();
    }

    $.validator.unobtrusive.parse(form);
};

function initJQPrototypes() {
    // jQuery Horizontal scroll center - JSFiddle - Code Playground
    // source: https://jsfiddle.net/onigetoc/6bfuspkq/
    $.fn.scrollCenter = function (elem, speed) {

        // this = #timepicker
        // elem = .active

        var active = jQuery(this).find(elem); // find the active element
        //var activeWidth = active.width(); // get active width
        var activeWidth = active.width() / 2; // get active width center

        //alert(activeWidth)

        //var pos = jQuery('#timepicker .active').position().left; //get left position of active li
        // var pos = jQuery(elem).position().left; //get left position of active li
        //var pos = jQuery(this).find(elem).position().left; //get left position of active li
        var pos = active.position().left + activeWidth; //get left position of active li + center position
        var elpos = jQuery(this).scrollLeft(); // get current scroll position
        var elW = jQuery(this).width(); //get div width
        //var divwidth = jQuery(elem).width(); //get div width
        pos = pos + elpos - elW / 2; // for center position if you want adjust then change this

        jQuery(this).animate({
            scrollLeft: pos
        }, speed == undefined ? 1000 : speed);
        return this;
    };

    // http://podzic.com/wp-content/plugins/podzic/include/js/podzic.js
    $.fn.scrollCenterORI = function (elem, speed) {
        jQuery(this).animate({
            scrollLeft: jQuery(this).scrollLeft() - jQuery(this).offset().left + jQuery(elem).offset().left
        }, speed == undefined ? 1000 : speed);
        return this;
    };

    // Generic way to detect if html form is edited
    // source: https://stackoverflow.com/questions/959670/generic-way-to-detect-if-html-form-is-edited
    $.fn.extend({
        trackChanges: function () {
            $(':input', this).change(function () {
                $(this.form).data('changed', true);
            });
        }
        ,
        hasChanged: function () {
            return this.data('changed');
        }
    });
}

function initJSPrototypes() {
    String.prototype.toCamelCase = function () {
        return this.replace(/(?:^\w|[A-Z]|\b\w|\s+)/g, function (match, index) {
            if (+match === 0) return ""; // or if (/\s+/.test(match)) for white spaces
            return index === 0 ? match.toLowerCase() : match.toUpperCase();
        });
    }
}

function initCRS(owner) {
    crs.init();

    $('.crs-country', owner).each(function (index, el) {
        var countryCodeSelect = $(el);
        var regionCodeSelect = $('#' + countryCodeSelect.attr('data-region-id'), owner);

        var countryNameInput = $('#' + countryCodeSelect.attr('data-country-name-id'), owner);
        var regionNameInput = $('#' + countryCodeSelect.attr('data-region-name-id'), owner);

        function updateChanges() {
            countryNameInput.val(countryCodeSelect.find('option:selected').text());
            regionNameInput.val(regionCodeSelect.find('option:selected').text());
        }

        updateChanges();

        countryCodeSelect.change(function (e) {
            updateChanges();
        });
        regionCodeSelect.change(function (e) {
            updateChanges();
        });
    });
}

function initAnimatedHeadline(owner) {
    $('[data-toggle="ah-headline"]', owner).each(function (i, el) {

        var element = $(el);

        element.animatedHeadline({
            animationType: element.attr('data-animation-type') || 'slide',
        });
    });
}

function initFocus(owner) {
    // Set focus.
    var errorInput = $('.input-validation-error:first', owner);

    if (errorInput.length) {
        errorInput.focus();
    }
    else if (!window.location.hash) {
        var inputSelector = 'input[type=text],input[type=password],input[type=radio],input[type=checkbox],textarea,select,input[type=tel],input[type=email],input[type=number]';
        $(inputSelector, owner)
            .filter(':not(:hidden,:disabled,.ignore-focus)').first().focus();
    }
  
}
// Bootstrap 4 - Nav - Hiding extra menu items
// source: https://stackoverflow.com/questions/46477802/bootstrap-4-nav-hiding-extra-menu-items
function initNavNowrap(owner) {
    var navNowrap = function (menu, maxHeight) {

        var nav = $(menu);
        var navItems = nav.children('.nav-item:not(.dropdown:last)').toArray();

        var navHeight = nav.height();
        var maxHeight = nav.children('.nav-item:first').height();

        var dropdown = nav.children('.nav-item.dropdown:last');
        var dropdownMenu = dropdown.find('.dropdown-menu:first');
        var dropdownItems = dropdownMenu.children('.dropdown-item,.dropdown-toggle').toArray();

        if (navHeight > maxHeight) {

            while (navHeight > maxHeight && navItems.length > 0) {

                 // removes the last element of the array, and returns that element.
                var navItem = $(navItems.pop()); 

                if (navItem.length) {

                    if (!navItem.is('.dropdown')) {
                        var navLink = navItem.find(':first');
                        navLink.unwrap();
                        navLink.removeClass('nav-link').addClass('dropdown-item');
                        navLink.prependTo(dropdownMenu);
                    } else {
                        navItem.removeClass('nav-item dropdown').addClass('dropdown-toggle').addClass('position-static');
                        navItem.find(':first').removeClass('nav-link dropdown-toggle').addClass('dropdown-item');
                        navItem.prependTo(dropdownMenu);
                    }
                }

                navHeight = nav.outerHeight();
            }
        }
        else {

            while (navHeight <= maxHeight && dropdownItems.length > 0) {

                // removes the first element of the array, and returns that element.
                var dropdownItem = $(dropdownItems.shift());

                if (dropdownItem.length) {

                    if (dropdownItem.is('.dropdown-item')) {
                        dropdownItem.removeClass('dropdown-item').addClass('nav-link');
                        dropdownItem.wrap('<li class="nav-item"></li>').parent().insertBefore(dropdown);

                    }
                    else if (dropdownItem.is('.dropdown-toggle')) {
                        dropdownItem.removeClass('dropdown-toggle').removeClass('position-static').addClass('nav-item dropdown');
                        dropdownItem.find(':first').removeClass('dropdown-item').addClass('nav-link dropdown-toggle');
                        dropdownItem.insertBefore(dropdown);
                    }
                }

                navHeight = nav.outerHeight();
            }

            if (navHeight > maxHeight) {
                navNowrap(menu, maxHeight);
            }
        }

        var dropdownItems = dropdownMenu.children('.dropdown-item,.dropdown-toggle').toArray();
        if (dropdownItems.length == 0) dropdown.addClass('d-none');
        else dropdown.removeClass('d-none');
    };

    $('.nav-nowrap', owner).each(function (index, el) {
        var element = $(el);
        element.removeClass('text-nowrap flex-nowrap overflow-hidden');

        navNowrap(element);

        $(window).on('resize', function () {
            navNowrap(element);
            navNowrap(element);
        });
    });
}

function initPerfectScrollbar(owner) {
    $('[data-toggle="scrollbar"]', owner).each(function (i, el) {
        var element = $(el);

        new PerfectScrollbar(element[0]);
    });
}

function initAnchorLink(owner) {
    // Select all links with hashes
    $('.anchor-link', owner)
        // Remove links that don't actually link to anything
        .not('[href="#"]')
        .not('[href="#0"]')
        .click(function (event) {
            // On-page links
            if (
                location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '')
                &&
                location.hostname == this.hostname
            ) {
                // Figure out element to scroll to
                var target = $(this.hash);
                target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
                // Does a scroll target exist?
                if (target.length) {
                    // Only prevent default if animation is actually gonna happen
                    event.preventDefault();
                    $('html, body').animate({
                        scrollTop: target.offset().top
                    }, 1500, 'easeInOutExpo');
                }
            }
        });
}

function initTooltip(owner) {
    $('[data-toggle="tooltip"]', owner).each(function (i, el) {
        var element = $(el);

        element.tooltip({});
    });
}

function showTooltip(element, message) {
    element.tooltip('hide')
        .attr('data-original-title', message)
        .tooltip('show');
}

function setTooltip(element, message) {
    element.tooltip()
        .attr('data-original-title', message);
}

function hideTooltip(element, timeout, callback) {
    timeout = timeout || 0;
    setTimeout(function () {
        element.tooltip('hide');
        if (callback) callback();
    }, timeout);
}

function initClipboard(owner) {

    $('[data-toggle="clipboard"]', owner).each(function (i, el) {
        var element = $(el);

        if (!Clipboard.isSupported()) {
            element.prop('disabled', true);
        }

        setTooltip(element, 'Copy');

        var clipboard = new Clipboard(element[0]);

        function showClipboardEvent(event, message) {
            showTooltip($(event.trigger), message);
            hideTooltip($(event.trigger), 5000, () => setTooltip(element, 'Copy'));
        }

        clipboard.on('success', function (e) {
            showClipboardEvent(e, 'Copied!')
        });

        clipboard.on('error', function (e) {
            showClipboardEvent(e, 'Failed!')
        });
    });
}

function initSelect2(owner) {
    $('[data-toggle="select2"]', owner).each(function (index, el) {
        var element = $(el);

        function faIconTemplate(option) {
            if (!option.id) {
                return option.text;
            }

            var icon = $(option.element).attr('data-icon');
            var text = option.text;
            return $(`<i class="fad fa-${icon}"></i>&nbsp;&nbsp;&nbsp;<span>${text}</span>`);
        }

        function faBrandTemplate(option) {
            if (!option.id) {
                return option.text;
            }

            return $(`<i class="fab fa-${option.id}"></i>&nbsp;&nbsp;&nbsp;<span>${option.text}</span>`);
        };

        var templateType = element.attr('data-template-type');

        // Select element 100% width bug if option with long text is selected (Select2 bootstrap theme)
        // source: https://stackoverflow.com/questions/31831064/select-element-100-width-bug-if-option-with-long-text-is-selected-select2-boot/34544106
        element.attr('style', 'width: 100%');
        element.wrap('<div class="position-relative"></div>')
            .select2({
                placeholder: element.attr('placeholder'),
                dropdownParent: element.parent(),
                minimumResultsForSearch: parseObject(element.attr('data-minimum-results-for-search')) || -1,
                tags: parseObject(element.attr('data-tags')) || false,
                tokenSeparators: [','],
                dropdownCssClass: 'text-nowrap w-auto',
                templateResult: templateType == 'fa-icon' ? faIconTemplate :
                    templateType == 'fa-brand' ? faBrandTemplate : undefined,
                templateSelection: templateType == 'fa-icon' ? faIconTemplate :
                    templateType == 'fa-brand' ? faBrandTemplate : undefined,
            })
            .change(function () {

                var input = $(this);
                var form = $(this).closest('form');

                if (form && form.data('validator')) {
                    input.valid();
                }
            });
    });
}

function initCleave(owner) {
    $('[data-toggle="cleave"]', owner).each(function (index, el) {

        var element = $(el);

        var numeral = parseObject(element.attr('data-numeral'));

        if (numeral) {
            var cleave = new Cleave(element[0], {
                numeral: true,
                numeralThousandsGroupStyle: element.attr('data-numeral-thousands-group-style') || 'none',
                numeralDecimalScale: element.attr('data-numeral-decimal-scale') || 2
            });

            // TODO: This code isn't well trusted.
            if (numeral) {
                var value = parseFloat(element.val());
                if (!isNaN(value)) element.val(value);
            }
        }
    });
}

function initRateyo(owner) {
    $('[data-toggle="rateyo"]', owner).each(function (i, el) {
        var element = $(el);

        var divElement = element.is('input') ? $('<div></div>', owner).insertAfter(element) : element;

        divElement.rateYo({
            starWidth: element.attr('data-star-width') || '16px',
            readOnly: parseObject(element.attr('data-readonly')) || false,
            fullStar: parseObject(element.attr('data-full-star')) || false,
            numStars: element.attr('data-num-stars') || 5,
            rating: element.is('input') ? (element.val() || 0) : (element.attr('data-rating') || 0),
            normalFill: themeColors.light,
            ratedFill: themeColors.primary,
            starSvg: '<svg><use href="#fas-star" /></svg>'
        });

        divElement.rateYo().on('rateyo.change', function (e, data) {

            var rating = data.rating;
            if (element.is('input')) {
                element.val(rating).trigger('change');
            }
        });
    });
}

function initBSSlider(owner) {
    $('[data-toggle="slider"]', owner).each(function (i, el) {
        var element = $(el);

        element.slider({
            tooltip_split: true,
            formatter: function (value) {
                if (element.attr('data-slider-formatter')) {
                    var value = formatString(element.attr('data-slider-formatter') || '', { value: value });
                    return value;
                }

                return value;
            }
        });
    });
}

function initFormSubmit(owner) {
    $('[data-submit]', owner).each(function (index, el) {
        var element = $(el);
        var submit = element.attr('data-submit');

        var ignoreClosest = element.attr('data-ignore-closest');
        var ignoreTarget = element.attr('data-ignore-target');

        if (submit == 'click') {

            element.click(function (e) {

                var targetElement = $(e.target).closest(ignoreClosest, element);

                // allow all input, textarea, select and button elements action to be triggered.
                if (targetElement.has(ignoreTarget).length) {

                } else {
                    // e.preventDefault();

                    submitFormFromElement(element);
                }
            });
        }

        if (submit == 'change') {

            element.change(function (e) {
                e.preventDefault();

                element.attr('data-value', element.val());
                submitFormFromElement(element);
            });
        }

        if (submit == 'form') {

            element.submit(function (e) {
                e.preventDefault();

                submitFormFromElement(element);
            });
        }

        if (submit == 'pageClick') {
            element.bootstrapPaginator({
                onPageClicked: function (e, originalEvent, type, page) {
                    element.attr('data-value', page);

                    submitFormFromElement(element);
                }
            });
        }
    });
}

function submitFormFromElement(element) {

    var action = element.attr('data-action');
    var method = element.attr('data-method') || 'get';
    var name = element.attr('data-name');
    var value = cleanArray((element.attr('data-value') || '').split(','));
    var obj = cleanObject(parseObject(element.attr('data-object')) || {});

    if (name) {
        obj = $.extend(obj, { [name]: value });

        // delete the name from the object if the value is empty.
        if (!value.length) delete obj[name];
    }

    if (element.is('form')) {
        obj = cleanObject($.extend(obj, serializeObject($(element).serializeArray())));
    }

    var title = element.attr('data-title');
    var message = element.attr('data-message');
    var dialog = element.attr('data-dialog') || 'none';
    var mode = element.attr('data-mode') || 'form'; // eg: form, ajax

    if (dialog == 'confirm') {
        var valueLength = value.length;
        var formatDisplay = parseObject(element.attr('data-format-display')) || false;

        bootbox.confirm({
            className: 'modal-fullscreen',
            title: !formatDisplay ? title : formatQuantity(formatString(title, { valueLength: valueLength }), valueLength),
            message: !formatDisplay ? message : formatQuantity(formatString(message, { valueLength: valueLength }), valueLength),
            buttons: {
                confirm: {
                    label: element.attr('data-confirm-label') || 'Proceed',
                    className: element.attr('data-confirm-class-name') || 'btn-primary'
                },
                cancel: {
                    label: element.attr('data-cancel-label') || 'Cancel',
                    className: element.attr('data-cancel-class-name') || 'btn-default'
                }
            },
            callback: function (result) {
                if (result) {
                    if (mode == 'form') {
                        submitForm(action, method, obj);
                    }
                    else if (mode == 'ajax') {
                        submitAjax(action, method, obj, $(element.attr('data-target')));
                    }
                }
            }
        });
    }
    else if (dialog == 'alert') {
        bootbox.alert({
            className: 'modal-fullscreen',
            title: title,
            message: message
        });
    }
    else if (dialog == 'none') {

        if (mode == 'form') {
            submitForm(action, method, obj);
        }
        else if (mode == 'ajax') {
            submitAjax(action, method, obj, $(element.attr('data-target')));
        }
    }
    else if (dialog == 'modal') {
        showAjaxModal(composeUrl(action, obj));
    }
}

function submitAjax(action, method, obj, target) {

    showBlock(target.parent());

    $.ajax({
        type: method,
        url: action,
        data: obj,
        headers: {
            [xsrf.headerName]: xsrf.requestToken,
        },
        success: function (response) {
            if (target && target.is(':input')) {
                target.val(response).trigger('change');
            }

            hideBlock(target.parent());
        },
        error: function () {
            hideBlock(target.parent());
        }
    });
}

function initLozad(owner) {
    $('[data-toggle="lozad"]', owner).each(function (index, el) {
        var element = $(el);

        var appLogoDataUrl = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' style='margin: auto; display: block; shape-rendering: auto;' width='200px' height='200px' viewBox='0 0 100 100' preserveAspectRatio='xMidYMid'%3E%3Ccircle cx='50' cy='50' fill='none' stroke='${encodeURIComponent(themeColors.primary)}' stroke-width='5' r='26' stroke-dasharray='122.52211349000194 42.840704496667314'%3E%3CanimateTransform attributeName='transform' type='rotate' repeatCount='indefinite' dur='0.5988023952095808s' values='0 50 50;360 50 50' keyTimes='0;1'/%3E%3C/circle%3E%3C/svg%3E") transparent center/25% no-repeat`;


        if (element.is('img')) {
            element.css('background', appLogoDataUrl);
            element.attr('src', "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='512' height='512' viewBox='0 0 3 2'%3E%3C/svg%3E");
        }

        element.on('error', function () {
            if (!element.hasClass('img-blank').length) {
                element.attr('src', "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='512' height='512' viewBox='0 0 3 2'%3E%3C/svg%3E");
                element.addClass('img-blank');
            }
        });

        var observer = lozad(element[0], {
            loaded: function (el) {
                if (element.is('img')) {
                    element.css('background', '');
                }
            }});
        observer.observe();
    });
}

function initBSPaginator(owner) {
    $('[data-toggle="paginator"]', owner).each(function (i, el) {
        var element = $(el);

        element.bootstrapPaginator({
            useBootstrapTooltip: true,
            currentPage: parseObject(element.attr('data-page')),
            numberOfPages: 5,
            totalPages: parseObject(element.attr('data-total-pages')),
            itemTexts: function (type, page, current) {
                switch (type) {
                    case 'first': return '<i class="fal fa-angle-double-left"></i>';
                    case 'last': return '<i class="fal fa-angle-double-right"></i>';
                    case 'prev': return '<i class="fal fa-angle-left"></i>';
                    case 'next': return '<i class="fal fa-angle-right"></i>';
                    case 'page': return page;
                }
            },
        });

          // fix bugs 'bootstrapPaginator'.
        $('li', element).each(function (i, e) {
            if ($(this).hasClass('active') && $(this).hasClass('disabled')) $(this).removeClass('active');
        });
    });
}

function initAsterisks(owner) {
    $('input[type=text],input[type=password],input[type=radio],input[type=checkbox],textarea,select,input[type=tel],input[type=email],input[type=number]', owner).each(function () {

        if (!$(this).hasClass('exclude-asterisks')) {
            var req = $(this).attr('data-val-required');
            var exclude = $(this).attr('data-exclude');
            if (undefined != req && undefined == exclude) {
                var label = $('label[for="' + $(this).attr('name') + '"]', owner);
                var text = label.text();
                if (text.length > 0) {
                    label.append(`<span style="color:${themeColors.danger}"> *</span>`);
                }
            }
        }
    });
}

function initCollapser(owner) {
    $('[data-toggle="collapser"]', owner).each(function (i, el) {
        var element = $(el);

        element.collapser({
            mode: element.attr('data-mode'),
            truncate: parseObject(element.attr('data-truncate')),
            ellipsis: '...',
            showText: '<span class="font-weight-semibold">Show more</span>',
            hideText: '<span class="font-weight-semibold">Show less</span>',
        });
    });
}

function initStatePreserver(owner) {
    $('[data-preserve]', owner).each(function (index, el) {

        // Check if web storage is supported.
        if (typeof (Storage) === 'undefined') {
            return;
        }

        var storage = sessionStorage;

        var element = $(el);
        var submitName = element.attr('data-submit');
        var preserveName = element.attr('data-preserve');
        var preserveKey = preserveName + element.attr('id');


        if (preserveName == 'collapse') {

            element.on('shown.bs.collapse', function () {
                storage.setItem(preserveKey, 'show');
            });

            element.on('hidden.bs.collapse', function () {
                storage.setItem(preserveKey, 'hide');
            });

            var preservedValue = storage.getItem(preserveKey);
            if (preservedValue == 'show' || preservedValue == 'hide') {
                element.collapse(preservedValue);
            }
        }
        else if (preserveName == 'scrollbar') {

            // Retain scrollbar position even after reloading using javascript.
            // source: https://stackoverflow.com/questions/34261365/retain-scrollbar-position-even-after-reloading-using-javascript

            if (storage.scrollTop != 'undefined' && storage.retainScrollbar == 'true') {
                $(window).scrollTop(storage.scrollTop);
                storage.retainScrollbar = 'false';
            }

            $(window).on('scroll resize', function () {
                storage.scrollTop = $(this).scrollTop();
            });

            if (submitName == 'click') {

                element.click(function (e) {
                    storage.retainScrollbar = 'true';
                });
            }

            if (submitName == 'change') {

                element.change(function (e) {
                    storage.retainScrollbar = 'true';
                });
            }
        }
        else if (preserveName == 'alert') {

            element.on('closed.bs.alert', function () {
                storage.setItem(preserveKey, 'dispose');
            });

            var preservedValue = storage.getItem(preserveKey);

            if (preservedValue == 'dispose') {
                element.remove();
            }
        }
    });
}

function initIntlTelInput(owner) {
    $('[data-toggle="intltel"]').each(function (i, el) {
        var element = $(el);

        var hiddenInput = element.attr('name');
        var countryCode = element.attr('data-country-code');

        element.attr('name', '');

        element.intlTelInput({
            preferredCountries: [],
            separateDialCode: true,
            hiddenInput: hiddenInput,
            initialCountry: 'auto',
            geoIpLookup: function (callback) {
                $.get('https://ipinfo.io?token=bc11535cd03a1b', function () { }, "jsonp").always(function (resp) {
                    countryCode = (resp && resp.country) ? resp.country : countryCode;
                    callback(countryCode);
                });
            },
        });

        element.on('countrychange input', function () {
            var phoneNumber = element.intlTelInput('getNumber');

            $(`[name="${hiddenInput}"]`).val(phoneNumber);
        }).trigger('countrychange');

        // Apply appwork styling...
        element.closest('.iti').find('.iti__selected-flag').addClass('bg-transparent');
        element.on('open:countrydropdown', function () {
            $('.iti__country-list').addClass('theme-border-white theme-bg-white');
        });
    });
}

function initDoubleSelector(owner) {
    $('.double-selector', owner).each(function (index, el) {
        var element = $(el);
        var elementType = element.attr('data-entities');

        var entities = elementType == 'regions' ? { "Ahafo": ["Goaso", "Hwidiem", "Kenyasi", "Kukuom"], "Ashanti": ["Abrepo Junction", "Abuakwa", "Adum", "Afrancho", "Agogo", "Agona", "Ahodwo", "Airport", "Akumadan", "Amakom", "Anomangye", "Aputuogya", "Asafo", "Ash-Town", "Asokore Mampong", "Asokwa", "Asuofia", "Atonsu", "Ayeduasi", "Ayigya", "Bantama", "Barekese", "Bekwai", "Boadi", "Bomso", "Breman", "Brewery", "Buoho", "Buokrom Estate", "Daban", "Dakwadwom", "Deduako", "Denyame", "Effiduase", "Ejisu", "Ejura", "Emina", "Esreso", "Fawode", "Gyinyasi", "Jachie Pramso", "Kaasi", "KNUST", "Kodie", "Komfo Anokye", "Konongo", "Kotei", "Krofrom", "Kumasi", "Maakro", "Mampong", "Mampongteng", "Mankranso", "Manso Nkwanta", "New Edubiase", "Nkawie", "North Suntreso", "Nyinahin", "Obuasi", "Oforikrom", "Pankrono", "Santasi", "Sokoban", "South Suntreso", "Suame", "Tafo", "Takwa-Maakro", "Tanoso", "Tepa", "TUC"], "Bono": ["Banda Ahenkro", "Brekum", "Dormaa Ahenkro", "New Drobo", "Nsawkaw", "Sampa", "Sunyani"], "Bono East": ["Atebubu", "Buipe", "Jema", "Kintampo", "Kwame Danso", "Nkoranza", "Prang", "Techiman", "Wenchi", "Yeji"], "Central": ["Agona Swedru", "Amanfrom", "Anomabu", "Apam", "Bawjiase", "Breman Asikuma", "Budumburam", "Cape Coast", "Domeabra", "Elmina", "Foso", "Kasoa Ofaakor", "Kasoa Zongo", "Liberia Camp", "Mankessim", "Millennium city", "Mumford", "Nyakrom", "Nyananor", "Nyanyano ", "Oduponkpehe", "Opeikuma", "Pentecost Seminary", "Saltpond", "Winneba"], "Eastern": ["Aburi", "Akim Oda", "Akim Swedru", "Akosombo", "Akropong", "Akwatia", "Asamankese", "Begoro", "Brekusu", "Kade", "Kibi", "Kitase", "Koforidua", "Mpraeso", "Nkawkaw", "Nsawam", "Somanya", "Suhum"], "Greater Accra": ["Abelemkpe", "Ablekuma", "Abokobi", "Abossey Okai", "Accra Newtown", "Achimota", "Adabraka", "Adenta", "Afienya", "Agbogba", "Airport", "Amasaman", "Anyaa", "Ashiaman", "Ashongman", "Aslyum Down", "Awoshie", "Baatsona - Spintex", "Bortianor", "Bubuashie", "Cantonment", "Dansoman", "Darkuman", "Dawhenya", "Dodowa", "Dome", "Dzorwulu", "East Legon", "Gbawe", "Haatso", "James Town", "Kanda", "Kaneshie", "Kasoa", "Kissieman", "Kokrobite", "Korle Bu", "Kpone", "Kwabenya", "Kwashieman", "Labadi", "Labone", "Lapaz", "Lartebiokorshie", "Lashibi", "Legon", "Madina", "Makola", "Mallam", "McCarthy Hill", "Michel Camp", "Nima", "Nungua", "Oblogo Mallam", "Odoponkpehe", "Odorkor", "Osu", "Oyarifa", "Pantang", "Prampram", "Ridge", "Roman Ridge", "Sakumono", "Santa Maria", "Sowutuom", "Taifa", "Tema", "Tema New Town", "Tesano", "Teshie", "Tetegu", "Tieman", "Tudu", "Weija", "Westhills"], "North East": ["Chereponi", "Gambaga", "Nalerigu", "Walewale"], "Northern": ["Bimbila", "Gushiegu", "Kpandae", "Salaga", "Tamale", "Yendi"], "Oti": ["Chinderi", "Jasikan", "Kadjebi", "Kete Krachi", "Kpassa", "Nkonya"], "Savannah": ["Bole", "Buipe", "Damango", "Salaga", "Sawla", "Tolon"], "Upper East": ["Bawku", "Bolgatanga", "Bongo", "Navrongo", "Paga", "Tongo"], "Upper West": ["Jirapa", "Lawra", "Tumu", "Wa."], "Volta": ["Adaklu Waya", "Adidome", "Aflao", "Akatsi", "Ave Dakpa", "Ho.", "Hohoe", "Keta", "Kpando", "Kpetoe", "Kpeve", "Sogakope"], "Western": ["Aboso", "Asankragua", "Axim", "Bogoso", "Elubo", "Half Assini", "Inchaban", "Prestea", "Samreboi", "Sekondi", "Shama", "Takoradi", "Tarkwa"], "Western North": ["Akontombra", "Bibiani", "Dadieso", "Enchi", "Juaboso", "Sewhi Anhwiaso"] } : null;

        var elementDefaultText = element.attr('data-default-text');
        var elementSelectedText = element.attr('data-value');

        var targetElement = $(element.attr('data-target'));
        var targetDefaultText = targetElement.attr('data-default-text');
        var targetSelectedText = targetElement.attr('data-value');

        element.on('change', function () {
            if (targetDefaultText != null)
                targetElement.children().not(':first').remove();
            else
                targetElement.children().remove();

            Object.entries(entities[element.val()] || {}).forEach(([index, entity]) => {
                targetElement.append(`<option value="${entity}">${entity}</option>`);
            });
        });

        function prepareSourceSelection() {
            Object.entries(entities).forEach(([entity, index]) => {
                element.append(`<option value="${entity}">${entity}</option>`);
            });
        }

        function prepareDefaultSelection() {
            if (elementDefaultText != null) {
                element.append(`<option value="">${elementDefaultText}</option>`);
                element[0].selectedIndex = 0;
            }

            if (targetDefaultText != null) {
                targetElement.append(`<option value="">${targetDefaultText}</option>`);
                targetElement[0].selectedIndex = 0;
            }
        }

        function displaySelection() {
            if (elementSelectedText != null) {
                element.val(elementSelectedText).trigger('change');
            }
            if (targetSelectedText != null) {
                targetElement.val(targetSelectedText).trigger('change');
            }
        }

        prepareDefaultSelection();
        prepareSourceSelection();
        displaySelection();
    });
}

function showBlock(element) {

    var blockOptions = {
        message: '<div class="spinner-border text-primary" role="status"><span class="sr-only"></span></div>',

        css: {
            backgroundColor: 'transparent',
            border: '0',
            zIndex: 9999999,
            width: '40px',
            top: '50%',
            left: '50%',
            cursor: 'default'
        },
        overlayCSS: {
            backgroundColor: element ? getFiliationStyle(element, 'background-color') : '#181C21',
            opacity: .5,
            zIndex: 9999990,
            cursor: 'default'
        },
        cursorReset: 'default'
    };

    if (element) element.block(blockOptions);
    else $.blockUI(blockOptions);

    element = (element ? element : $('body'));

    var wasBlocked = element.hasClass('blocked');
    element.addClass('blocked');
    return !wasBlocked;
}

function hideBlock(element) {
    if (element) element.unblock();
    else $.unblockUI();

    element = (element ? element : $('body'));

    var wasBlocked = element.hasClass('blocked');
    element.removeClass('blocked');
    return wasBlocked;
}

function showAjaxModal(url) {
    var modalPlaceHolderElement = $('body');
    var modalElementId = null;
    var modalElement = null;

    function showModal(modalTemplate) {
        if ($('<div />').html(modalTemplate).find('.modal').length) {

            // Try to extract modal element id from it's template else generate a unique one.
            modalElementId = $(modalTemplate).attr('id') || generateUUID();

            // load the raw html data into the modal place holder.
            modalPlaceHolderElement.append($(modalTemplate).attr('id', modalElementId));
        }
        else {

            // Generate a unique modal element id.
            modalElementId = generateUUID();
            modalTemplate = `   <div class="modal" id="${modalElementId}">  ` +
                '       <div class="modal-dialog">  ' +
                '           <div class="modal-content">  ' +
                '               <div class="modal-header">  ' +
                '                   <div class="modal-title">  ' +
                '                       <h5 class="h5 mb-0">Modal</h5>  ' +
                '                   </div>  ' +
                '                   <button type="button" class="close" data-dismiss="modal" aria-label="Close">  ' +
                '                       <span aria-hidden="true">&times;</span>  ' +
                '                   </button>  ' +
                '               </div>  ' +
                '               <div class="modal-body pb-2">  ' + modalTemplate +
                '               </div>  ' +
                '               <div class="modal-footer">  ' +
                '                   <button type="button" class="btn btn-dark" data-dismiss="modal">Close</button>  ' +
                '               </div>  ' +
                '           </div>  ' +
                '       </div>  ' +
                '  </div>  ';

            modalPlaceHolderElement.append(modalTemplate);
        }

        modalElement = modalPlaceHolderElement.find('#' + modalElementId);
        var previousData;

        // bind events.
        modalElement.on('click', '[data-submit="modal"]', function (event) {
            event.preventDefault();

            var submitElement = $(this);
            var callback = parseObject(submitElement.attr('data-callback')) || false;
            var data = modalElement.serialize();

            // Validate the form before submitting it.
            if (modalElement.valid()) {

                // show the block for a callback request.
                showBlock();

                $.ajax({
                    type: 'POST',
                    url: url,
                    data: data,
                    beforeSend: function (xhr) {
                        if (callback) {
                            xhr.setRequestHeader('Alt-Referer', new URL(url, document.baseURI).href);
                        }
                    },
                    dataType: 'html',
                    success: function (result) {
                        var newContent = $('.modal-content', result);
                        modalElement.find('.modal-content').replaceWith(newContent);

                        submitElement = $('[data-submit="modal"]', modalElement);


                        // re-initialize components.
                        initComponents(modalElement.find('.modal-content'));

                        // hide the block for a post request.
                        hideBlock();

                        var valid = parseObject(newContent.attr('data-valid'));
                        var persistent = parseObject(newContent.attr('data-persistent'));
                        submitElement.attr('data-callback', valid); 

                        if (valid && !persistent) {
                            hideModal();
                            hideBlock();

                            var redirect = newContent.attr('data-redirect') || getQueryParameter('returnUrl', url);

                            if (redirect) setLocation(redirect);
                            else reloadLocation();
                        }
                    },
                    error: function (xhr) {
                        hideModal();
                        hideBlock();
                    }
                });
            }
        });

        modalElement.on('hidden.bs.modal', function (e) {
            destroyModal();
            hideBlock();
        })

        modalElement.modal({
            backdrop: 'static',
            keyboard: false  // to prevent closing with Esc button (if you want this too)
        });

        // show the modal.
        modalElement.modal('show');

        // initialize components.
        initComponents(modalElement.find('.modal-content'));
    }

    function destroyModal() {
        modalElement && modalElement.modal('dispose');
        modalElement && modalElement.remove();
        modalPlaceHolderElement.siblings('.modal-backdrop').remove();
    }

    function hideModal() {
        modalElement && modalElement.modal('hide');
    }

    // destroy existing modal and show the block.
    destroyModal();
    showBlock();

    $.get(url)
        .done(function (data) {
            hideBlock();
            showModal(data);
        })
        .fail(function (xhr) {
            destroyModal();
            hideBlock();
        });
}
