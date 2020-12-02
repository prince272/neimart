"use strict";
(function ($, undefined) {

    // Crudable object
    var Crudable = function (element, options) {
        this.element = $(element);

        var parentElement = $(element).parent();

        if (parentElement.attr('data-crudable') == null)
            parentElement.attr('data-crudable', $('<div />').append($(element).clone()).html());

        this.elementHtml = parentElement.attr('data-crudable');

        this._processOptions(options);
        this._attachEvent();
    };

    // Crudable prototype
    Crudable.prototype = {
        constructor: Crudable,
        _processOptions: function (options) {
            // store raw option for reference
            this._o = $.extend({}, this._o, options);
            // processed options
            this.o = $.extend({}, this._o);
        },
        _appendElement: function (element) {
            var thisValue = this;
            return function () {
                thisValue._appendNewElement(element);
            }
        },
        _deleteElement: function (element) {
            var thisValue = this;
            return function () {
                if (thisValue.o.beforeDelete !== $.noop) { thisValue.o.beforeDelete(element); }
                if (element.siblings("." + thisValue.o.crudableLabel).length == 0)
                    thisValue._appendNewElement(element);
                element.remove();
                if (thisValue.o.afterDelete !== $.noop) { thisValue.o.afterDelete(); }
            }
        },
        _appendNewElement: function (element) {
            var placeholder, label, valueDefault,
                newElement = $(this.elementHtml),
                $inputs = newElement.find('input');
            $inputs.each(function () {
                var $this = $(this);
                valueDefault = $this.data('crudable-default');
                if (valueDefault !== "undefined")
                    $this.val(valueDefault);
            });
            if (this.o.beforeCreate !== $.noop) { this.o.beforeCreate(newElement); }
            element.after(newElement);
            if (this.o.afterCreate !== $.noop) { this.o.afterCreate(newElement); }
            newElement.crudable(this._o);
        },
        _attachEvent: function () {
            this.element.find('.' + this.o.createLabel).click(this._appendElement(this.element));
            this.element.find('.' + this.o.deleteLabel).click(this._deleteElement(this.element));
        }

    }

    // Registering crudable with jquery
    $.fn.crudable = function (option) {
        var args = Array.prototype.slice.call(arguments, 1);
        args.shift;
        var internal_return;
        this.each(function () {
            var $this = $(this),
                data = $this.data('crudable'),
                options = typeof option === 'object' && option;
            if (!data) {
                $this.data('crudable', new Crudable(this, $.extend({}, defaults, options)));
            }
            if (typeof (option) === 'string' && typeof (data[option] === 'function')) {
                internal_return = data[option].apply(data, args);
                if (internal_return !== undefined)
                    return false;
            }
        });
        if (internal_return !== undefined)
            return internal_return;
        else
            return this;

    };

    var defaults = $.fn.crudable.defaults = {
        createLabel: "crudable-create",
        deleteLabel: "crudable-delete",
        crudableLabel: "crudable",
        beforeCreate: $.noop,
        afterCreate: $.noop,
        beforeDelete: $.noop,
        afterDelete: $.noop,
    };
})(jQuery)
