/**
 * bootstrap-paginator.js v2.0
 * 
 * Modified by 
 * Copyright (c) 2019 Kislenko Mikhail (Kain Stropov)
 * Licensed under the MIT License
 * 
 * Original code is written by Yun Lai <lyonlai1984@gmail.com> under the Apache 2.0 License
 */

(function ($) {

	"use strict"; // jshint ;_;

	/* Private constants */

	const pageChangedEvent = "page-changed",
		pageClickedEvent = "page-clicked";

	/* Private functions */

	function getBorderPageNumbers() {
		let lastPage = this.currentPage + Math.floor(this.numberOfPages / 2);
		if (lastPage > this.totalPages) lastPage = this.totalPages;
		let startPage = lastPage - this.numberOfPages + 1;
		if (startPage < 1) {
			startPage = 1;
			lastPage = this.numberOfPages > this.totalPages ? this.totalPages : this.numberOfPages;
		}
		return {
			startPage,
			lastPage
		};
	}

	function ajaxCall() {
		const url = this.getValueFromOption(this.options.ajaxUrl, this.currentPage),
			data = this.getValueFromOption(this.options.ajaxData, this.currentPage),
			targetElement = this.getValueFromOption(this.options.targetElement),
			loadElement = this.getValueFromOption(this.options.loadingElement);
		let $target, $loadElement;
		if (typeof targetElement === "string" && !(targetElement.startsWith("#") || targetElement.startsWith(".")))
			$target = $(`#${targetElement}`);
		else
			$target = $(targetElement);
		if (typeof loadElement === "string" && !(loadElement.startsWith("#") || loadElement.startsWith(".")))
				$loadElement = $(`#${loadElement}`);
		else
				$loadElement = $(loadElement);
		//$target.empty();
		$loadElement.show();
		$target.load(url, data,() => $loadElement.hide());
	}

	function customAjaxCall() {
		const url = this.getValueFromOption(this.options.ajaxUrl, this.currentPage),
			data = this.getValueFromOption(this.options.ajaxData, this.currentPage),
			ajaxSettings = this.getValueFromOption(this.options.ajaxSettings, this.currentPage);
		if (!ajaxSettings || typeof ajaxSettings !== "object")
			throw new Error("Ajax Settings must be object or function that returns object");
		if (url)
			ajaxSettings.url = url;
		if (data)
			ajaxData.data = data;
		$.ajax(ajaxSettings);
	}

	/* Paginator PUBLIC CLASS DEFINITION
	 * ================================= */


	class BootstrapPaginator {
		/**
		* Bootstrap Paginator Constructor
		*
		* @param element element of the paginator
		* @param options the options to config the paginator
		*
		* */
		constructor(element, options) {
			this.init(element, options);
		}

		/**
		 * Initialization function of the paginator, accepting an element and the options as parameters
		 *
		 * @param element element of the paginator
		 * @param options the options to config the paginator
		 *
		 * */
		init(element, options) {

			this.$element = $(element);

			if (!this.$element.is("ul"))
				throw new Error("Element must be <ul>");

			this.currentPage = 1;

			this.lastPage = 1;

			this.setOptions(options);

			this.initialized = true;
		}

		/**
		 * Update the properties of the paginator element
		 *
		 * @param options options to config the paginator
		 * */
		setOptions(options) {

			this.options = $.extend({}, (this.options || $.fn.bootstrapPaginator.defaults), options);

			this.totalPages = parseInt(this.options.totalPages, 10);  //setup the total pages property.
			this.numberOfPages = parseInt(this.options.numberOfPages, 10); //setup the numberOfPages to be shown

			//move the set current page after the setting of total pages. otherwise it will cause out of page exception.
			if (options && typeof (options.currentPage) !== "undefined" && this.totalPages > 0) {

				this.setCurrentPage(options.currentPage);
			}

			this.listen();

			//render the paginator
			this.render();

			if (this.options.ajaxSettings && (typeof this.options.ajaxSettings === "object" || typeof this.options.ajaxSettings === "function")) {
				this.$element.on(pageChangedEvent, $.proxy(customAjaxCall, this));
			} else if (this.options.targetElement && this.options.ajaxUrl) {
				this.$element.on(pageChangedEvent, $.proxy(ajaxCall, this));
			}

			if (!this.initialized || this.lastPage !== this.currentPage) {
				this.$element.trigger(pageChangedEvent, [this.lastPage, this.currentPage]);
			}

		}

		/**
		 * Sets up the events listeners. Currently the pageclicked and pagechanged events are linked if available.
		 *
		 * */
		listen() {

			this.$element.off(pageClickedEvent);

			this.$element.off(pageChangedEvent);// unload the events for the element

			if (typeof (this.options.onPageClicked) === "function") {
				this.$element.bind(pageClickedEvent, this.options.onPageClicked);
			}

			if (typeof (this.options.onPageChanged) === "function") {
				this.$element.on(pageChangedEvent, this.options.onPageChanged);
			}

			this.$element.bind(pageClickedEvent, this.onPageClicked);
		}

		/**
		 *
		 *  Destroys the paginator element, it unload the event first, then empty the content inside.
		 *
		 * */
		destroy() {

			this.$element.off(pageClickedEvent);

			this.$element.off(pageChangedEvent);

			this.$element.removeData("bootstrapPaginator");

			this.$element.empty();

		}

		/**
		 * Shows the page
		 *
		 * */
		show(page) {

			this.setCurrentPage(page);

			if (this.lastPage === this.currentPage) return;

			const borderPages = getBorderPageNumbers.call(this);

			if (borderPages.lastPage !== this.lastPageNumber) {
				if (borderPages.startPage > this.lastPageNumber || borderPages.lastPage < this.startPageNumber) {
					this.render();
					this.$element.trigger(pageChangedEvent, [this.lastPage, this.currentPage]);
					return;
				}
				if (borderPages.lastPage > this.lastPageNumber) {
					for (let i = this.lastPageNumber + 1; i <= borderPages.lastPage; i++) {
						const p = this.buildPageItem("page", i);
						this.$lastPage.after(p);
						this.$lastPage = p;

						const f = this.$startPage;
						this.$startPage = f.next();
						f.remove();
					}
				} else {
					for (let i = this.startPageNumber-1; i >= borderPages.startPage; i--) {
						const l = this.$lastPage;
						this.$lastPage = l.prev();
						l.remove();

						const f = this.buildPageItem("page", i);
						this.$startPage.before(f);
						this.$startPage = f;
					}
				}

				this.startPageNumber = borderPages.startPage;
				this.lastPageNumber = borderPages.lastPage;
			}

			this.$element.find(".active").removeClass("active");
			this.$element.find("li").eq(2 + this.currentPage - this.startPageNumber).addClass("active");
			if (this.currentPage === 1) {
				this.$first.addClass("disabled");
				this.$prev.addClass("disabled");
				if (!this.getValueFromOption(this.options.shouldShowPageButton, "first", 1, this.currentPage))
					this.$first.hide();
				if (!this.getValueFromOption(this.options.shouldShowPageButton, "prev", (this.currentPage - 1) < 1 ? 1 : (this.currentPage - 1), this.currentPage))
					this.$prev.hide();
			} else {
				this.$first.removeClass("disabled");
				this.$prev.removeClass("disabled");
				if (this.options.pageUrl) {
					this.$prev.children("a").attr("href",this.getValueFromOption(this.options.pageUrl, "prev", (this.currentPage - 1) < 1 ? 1 : (this.currentPage - 1), this.currentPage))
				}
				if (this.getValueFromOption(this.options.shouldShowPageButton, "first", 1, this.currentPage))
					this.$first.show();
				if (this.getValueFromOption(this.options.shouldShowPageButton, "prev", (this.currentPage - 1) < 1 ? 1 : (this.currentPage - 1), this.currentPage))
					this.$prev.show();
			}
			if (this.currentPage === this.totalPages) {
				this.$last.addClass("disabled");
				this.$next.addClass("disabled");
				if (!this.getValueFromOption(this.options.shouldShowPageButton, "last", this.totalPages, this.currentPage))
					this.$last.hide();
				if (!this.getValueFromOption(this.options.shouldShowPageButton, "next", (this.currentPage + 1) > this.totalPages ? this.totalPages : (this.currentPage + 1), this.currentPage))
					this.$next.hide();
			} else {
				this.$last.removeClass("disabled");
				this.$next.removeClass("disabled");
				if (this.options.pageUrl) {
					this.$next.children("a").attr("href",this.getValueFromOption(this.options.pageUrl, "next", (this.currentPage + 1) > this.totalPages ? this.totalPages : (this.currentPage + 1), this.currentPage))
				}
				if (this.getValueFromOption(this.options.shouldShowPageButton, "last", this.totalPages, this.currentPage))
					this.$last.show();
				if (this.getValueFromOption(this.options.shouldShowPageButton, "next", (this.currentPage + 1) > this.totalPages ? this.totalPages : (this.currentPage + 1), this.currentPage))
					this.$next.show();
			}

			for (let i = this.startPageNumber; i <= this.lastPageNumber; i++){
				let $page = this.$element.find("li").eq(i-2);
				if (this.getValueFromOption(this.options.shouldShowPageButton, "last", this.totalPages, this.currentPage))
					$page.show();
				else
					$page.hide();
			}

			this.$element.trigger(pageChangedEvent, [this.lastPage, this.currentPage]);
		}

		/**
		 * Shows the next page
		 *
		 * */
		showNext() {
			this.show(this.currentPage + 1);
		}

		/**
		 * Shows the previous page
		 *
		 * */
		showPrevious() {
			this.show(this.currentPage - 1);
		}

		/**
		 * Shows the first page
		 *
		 * */
		showFirst() {
			this.show(1);
		}

		/**
		 * Shows the last page
		 *
		 * */
		showLast() {
			this.show(this.totalPages);
		}

		/**
		 * Internal on page item click handler, when the page item is clicked, change the current page to the corresponding page and
		 * trigger the pageclick event for the listeners.
		 *
		 *
		 * */
		onPageItemClicked(event) {

			const type = event.data.type;
			const page = event.data.page;

			this.$element.trigger(pageClickedEvent, [event, type, page]);
		}

		onPageClicked(event, originalEvent, type, page) {

			//show the corresponding page and retrieve the newly built item related to the page clicked before for the event return

			const currentTarget = $(event.currentTarget);

			switch (type) {
				case "first":
					currentTarget.bootstrapPaginator("showFirst");
					break;
				case "prev":
					currentTarget.bootstrapPaginator("showPrevious");
					break;
				case "next":
					currentTarget.bootstrapPaginator("showNext");
					break;
				case "last":
					currentTarget.bootstrapPaginator("showLast");
					break;
				case "page":
					currentTarget.bootstrapPaginator("show", page);
					break;
			}

		}

		/**
		 * Renders the paginator according to the internal properties and the settings.
		 *
		 *
		 * */
		render() {

			//fetch the container class and add them to the container
			var containerClass = this.getValueFromOption(this.options.containerClass, this.$element),
				size = this.options.size || "normal",
				first = null,
				prev = null,
				next = null,
				last = null,
				p = null;

			this.$element.prop("class", "");

			this.$element.addClass("pagination");

			switch (size.toLowerCase()) {
				case "large":
				case "small":
					this.$element.addClass($.fn.bootstrapPaginator.sizeArray[size.toLowerCase()]);
					break;
				default:
					break;
			}

			this.$element.addClass(containerClass);

			//empty the outter most container then add the listContainer inside.
			this.$element.empty();

			//----Add first button
			first = this.buildPageItem("first",1);
			if (this.currentPage === 1)
				first.addClass("disabled");

			this.$element.append(first);

			if (!this.getValueFromOption(this.options.shouldShowPageButton, "first", 1, this.currentPage)) {
				first.hide();
			}

			this.$first = first;
			//----------------------

			//----Add prev button
			prev = this.buildPageItem("prev",(this.currentPage - 1) < 1 ? 1 : (this.currentPage - 1));

			if (this.currentPage === 1)
				prev.addClass("disabled");

			this.$element.append(prev);

			if (!this.getValueFromOption(this.options.shouldShowPageButton, "prev", (this.currentPage - 1) < 1 ? 1 : (this.currentPage - 1), this.currentPage)) {
				prev.hide();
			}

			this.$prev = prev;
			//---------------------

			//----Add number buttons
			const borderPages = getBorderPageNumbers.call(this);
			for (let i = borderPages.startPage; i <= borderPages.lastPage; i++) {//fill the numeric pages.
				
				p = this.buildPageItem("page", i);

				this.$element.append(p);

				if (!this.getValueFromOption(this.options.shouldShowPageButton, "page", i, this.currentPage))
					p.hide();

				if (i === borderPages.startPage)
					this.$startPage = p;
				else if (i === borderPages.lastPage)
					this.$lastPage = p;
			}


			this.startPageNumber = borderPages.startPage;
			this.lastPageNumber = borderPages.lastPage;
			//----------------------

			//----Add next button
			next = this.buildPageItem("next",(this.currentPage + 1) > this.totalPages ? this.totalPages : (this.currentPage + 1));

			if (this.currentPage === this.totalPages)
				next.addClass("disabled");

			this.$element.append(next);

			if (!this.getValueFromOption(this.options.shouldShowPageButton, "next", (this.currentPage + 1) > this.totalPages ? this.totalPages : (this.currentPage + 1), this.currentPage)) {
				next.hide();
			}

			this.$next = next;
			//------------------

			//----Add last button

			last = this.buildPageItem("last", this.totalPages);

			if (this.currentPage === this.totalPages)
				last.addClass("disabled");

			this.$element.append(last);

			if (!this.getValueFromOption(this.options.shouldShowPageButton, "last", this.totalPages, this.currentPage)) {
				last.hide();
			}

			this.$last = last;
		}

		/**
			 *
			 * Creates a page item base on the type and page number given.
			 *
			 * @param page page number
			 * @param type type of the page, whether it is the first, prev, page, next, last
			 *
			 * @return Object the constructed page element
			 * */
		buildPageItem(type, page) {

			const itemContainer = $("<li></li>"),
				itemContent = $("<a></a>"),
				itemContentClass = this.getValueFromOption(this.options.itemContentClass, type, page, this.currentPage),
				lang = this.getValueFromOption(this.options.language),
				regional = $.extend({}, $.fn.bootstrapPaginator.regional.en,$.fn.bootstrapPaginator.regional[lang]);
			let text = "",
				title = "",
				itemContainerClass = this.getValueFromOption(this.options.itemContainerClass,type, page, this.currentPage);


			switch (type) {

				case "first":
					text = this.options.itemTexts(type, page, this.currentPage);
					title = regional.first;
					break;
				case "last":
					text = this.options.itemTexts(type, page, this.currentPage);
					title = regional.last;
					break;
				case "prev":
					text = this.options.itemTexts(type, page, this.currentPage);
					title = regional.prev;
					break;
				case "next":
					text = this.options.itemTexts(type, page, this.currentPage);
					title = regional.next;
					break;
				case "page":
					text = this.options.itemTexts(type, page, this.currentPage);
					title = (this.currentPage === page ? regional.current : regional.page).replace("${0}",page);
					break;
			}

			if (!itemContainerClass.includes("page-item")) 
				itemContainerClass = (itemContainerClass + " page-item").trim();

			itemContainer.addClass(itemContainerClass).append(itemContent);

			itemContent.addClass(itemContentClass).html(text).on("click", null, { type: type, page: page }, $.proxy(this.onPageItemClicked, this));

			if (this.options.pageUrl) {
				itemContent.attr("href", this.getValueFromOption(this.options.pageUrl, type, page, this.currentPage));
			}

			if (this.options.useBootstrapTooltip) {
				const tooltipOpts = $.extend({}, this.options.bootstrapTooltipOptions, { title: title });

				itemContent.tooltip(tooltipOpts);
			} else {
				itemContent.attr("title", title);
			}

			return itemContainer;
		}

		setCurrentPage(page) {
			if (page > this.totalPages || page < 1) {// if the current page is out of range, throw exception.

				throw new Error("Page out of range");

			}

			this.lastPage = this.currentPage;

			this.currentPage = parseInt(page, 10);
			this.options.currentPage = this.currentPage;
		}
		
		/**
		 * Gets the value from the options, this is made to handle the situation where value is the return value of a function.
		 *
		 * @return mixed value that depends on the type of parameters, if the given parameter is a function, then the evaluated result is returned. Otherwise the parameter itself will get returned.
		 * */
		getValueFromOption(value) {

			let output;
			const args = Array.prototype.slice.call(arguments, 1);

			if (typeof value === "function") {
				output = value.apply(this, args);
			} else {
				output = value;
			}

			return output;

		}

		reload() {
			/*if (this.options.ajaxSettings && (typeof this.options.ajaxSettings === "object" || typeof this.options.ajaxSettings === "function")) {
				customAjaxCall.call(this);
			} else if (this.options.targetElement && this.options.ajaxUrl) {
				ajaxCall.call(this);
			}*/
			this.$element.trigger(pageChangedEvent, [this.lastPage, this.currentPage]);
		}

		getOption(option) {
			if (option) {
				if (this.options.hasOwnProperty(option))
					return this.options[option];
				else
					return null;
			}
			return $.extend({},this.options);
		}
	};


	/* TYPEAHEAD PLUGIN DEFINITION
	 * =========================== */
	
	$.fn.bootstrapPaginator = function (option) {

		const args = arguments;
		let result = null;

		$(this).each(function (index, item) {
			let $this = $(item),
				data = $this.data("bootstrapPaginator");
			const options = (typeof option !== "object") ? null : option;

			if (!data) {
				data = new BootstrapPaginator(this, options);

				$this = $(data.$element);

				$this.data("bootstrapPaginator", data);

				return;
			}

			if (typeof option === "string") {

				if (data[option]) {
					result = data[option].apply(data, Array.prototype.slice.call(args, 1));
				} else {
					throw new Error(`Method ${option} does not exist`);
				}

			} else {
				result = data.setOptions(option);
			}
		});

		return result;
	};

	$.fn.bootstrapPaginator.sizeArray = {
		"large": "pagination-lg",
		"small": "pagination-sm"
	};

	$.fn.bootstrapPaginator.defaults = {
		language: () => {
			for (let lang in navigator.languages) {
				const l = navigator.languages[lang].substr(0, 2).toLowerCase();
				if ($.fn.bootstrapPaginator.regional.hasOwnProperty(l))
					return l;
			}
			return "en";
		},
		containerClass: "",
		size: "normal",
		itemContainerClass: function(type, page, current) {
			return (page === current) ? "active" : "";
		},
		itemContentClass: function(type, page, current) {
			return "page-link";
		},
		currentPage: 1,
		numberOfPages: 5,
		totalPages: 1,
		pageUrl: function(type, page, current) {
			return "javascript:void(0)";
		},
		onPageClicked: null,
		onPageChanged: null,
		useBootstrapTooltip: false,
		alwaysDisplayNextPrevButtons: true,
		alwaysDisplayFirstLastButtons: true,
		shouldShowPageButton: function(type, page, current) {
			switch (type) {
				case "first":
					return this.options.alwaysDisplayFirstLastButtons || (current !== 1);
				case "prev":
					return this.options.alwaysDisplayNextPrevButtons || (current !== 1);
				case "next":
					return this.options.alwaysDisplayNextPrevButtons || (current !== this.totalPages);
				case "last":
					return this.options.alwaysDisplayFirstLastButtons || (current !== this.totalPages);
				default:
					return true;
			}
		},
		itemTexts: function(type, page, current) {
			switch (type) {
			case "first":
				return "&lt;&lt;";
			case "prev":
				return "&lt;";
			case "next":
				return "&gt;";
			case "last":
				return "&gt;&gt;";
			default:
				return page;
			}
		},
		bootstrapTooltipOptions: {
			animation: true,
			html: true,
			placement: "top",
			selector: false,
			title: "",
			container: false
		},
		//----Ajax settings
		loadingElement: null,
		targetElement: null,
		ajaxUrl: null,
		ajaxData: null,
		ajaxSettings: null
	};

	$.fn.bootstrapPaginator.regional = {
		"en": {
			first: "Go to first page",
			prev: "Go to previous page",
			next: "Go to next page",
			last: "Go to last page",
			current: "Current page is ${0}",
			page: "Go to page ${0}"
		}
	};

	$.fn.bootstrapPaginator.Constructor = BootstrapPaginator;
}(window.jQuery));
