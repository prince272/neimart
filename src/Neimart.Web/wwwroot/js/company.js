(function () {
    $(window).on('scroll resize', function () {
        var navbar = $('#company-navbar');

        var navbarTransparentClass = 'navbar-dark bg-transparent py-4';
        var navbarSoildClass = 'navbar-light bg-navbar-theme shadow-sm';

        var navbarBreakpoint = bootstrapDetectBreakpoint();
        var navbarSection = navbar.next('.navbar-section');

        if (!navbarSection.length) {
            navbar.parent().addClass('mt-5');
        }

        if (navbarSection.length && (navbarBreakpoint.index > 2) && ($(window).scrollTop() <= 80)) {
            navbar.addClass(navbarTransparentClass);
            navbar.removeClass(navbarSoildClass);
        } else {
            navbar.removeClass(navbarTransparentClass);
            navbar.addClass(navbarSoildClass);
        }
    }).trigger('scroll');
})();