/*
 * Authors:
 *	Marek Habersack <grendel@twistedcode.net>
 *
 * (C) 2010 Novell, Inc (http: *novell.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * This code serves only the List rendering mode of the Menu control in Mono
 *
 */
if (!window.Sys) { window.Sys = {}; }
if (!Sys.WebForms) { Sys.WebForms = {}; }

Sys.WebForms.Menu = function (options)
{
	if (options == null)
		throw "Sys.WebForms.Menu constructor requires options to be not null.";

	if (options.element == null)
		throw "options.element is required.";

	if (options.orientation == null)
		throw "options.orientation is required.";

	if (typeof (options.element) == "string")
		this.menuID = options.element;
	else
		this.mainElement = options.element;

	this.disappearAfter = options.disappearAfter || 500;
	this.orientation = options.orientation;
	this.tabIndex = options.tabIndex || 0;
	this.disabled = options.disabled || false;
	this.level = options.level || 0;
	this.menuItemIndex = 0;

	if (this.level != 0) {
		if (options.parentMenu == null)
			throw "options.parentMenu is required for all submenus.";

		this.parentMenu = options.parentMenu;
	}

	if (this.mainElement == null) {
		this.mainElement = document.getElementById (this.menuID);
		if (this.mainElement == null)
			throw "Unable to find menu element with id '" + this.menuID + "'.";

		if (this.mainElement.tagName != "DIV")
			throw "This script must be used only when the menu containing element is DIV.";
	}

	/* Due to the way we generate the menu in the list mode, every submenu other than the root one is dynamic */
	if (this.level > 1) {
		this.menuType = "dynamic";
		if (options.parentMenu == null)
			throw "options.parent is required for all submenus.";

		var subMenuId = Sys.WebForms.Menu.Helpers.getNextSubMenuId ();
		this.parentMenu = options.parentMenu;
		this.orientation = this.parentMenu.orientation;
		this.path = this.parentMenu.path + subMenuId;
		this.menuID = this.parentMenu.menuID;
		if (this.mainElement.id == null || this.mainElement.id == "")
			this.mainElement.id = this.menuID + ":submenu:" + subMenuId;
	} else {
		this.menuType = "static";
		if (this.level == 1) {
			this.menuID = this.parentMenu.menuID;
			this.orientation = this.parentMenu.orientation;
		}
		this.parentMenu = null;
		this.path = "0";
		this.mainElement.setAttribute ("tabindex", this.tabIndex);
		this.mainElement.setAttribute ("role", this.orientation == "vertical" ? "menu" : "menubar");
		with (this.mainElement.style) {
			width = "auto";
			position = "relative";
		}
	}

	if (this.level > 0) {
		Sys.WebForms.Menu.Helpers.appendCssClass (this.mainElement, this.menuType);
	}

	if (this.level <= 1)
		Sys.WebForms.Menu.Helpers.setFloat (this.mainElement, "left");

	this.loadItems ();
}

Sys.WebForms.Menu.Helpers = {
	__subMenuCounter: 0,
	__menuItems: [],
	__popupToClose: null,

	setPopupToClose: function (element) {
		this.__popupToClose = element;
	},

	getPopupToClose: function () {
		return this.__popupToClose;
	},

	setFloat: function (element, side) {
		/* For standards-compliant browsers */
		element.style.cssFloat = "left";

		/* For IE */
		element.style.styleFloat = "left";
	},

	appendCssClass: function (element, className) {
		if (element == null || className == null)
			return;

		var cname = element.className;
		if (cname == null || cname == "")
			cname = className;
		else
			cname += " " + className;

		element.className = cname;
	},

	getNextSubMenuId: function () {
		return ++this.__subMenuCounter;
	},

	addMenuItem: function (item) {
		if (item == null)
			return;

		if (!(item instanceof Sys.WebForms.MenuItem))
			throw "item must be an instance of Sys.WebForms.MenuItem";

		if (this.__menuItems [item.path] != null)
			throw "item already exists (path " + item.path + ")";

		this.__menuItems [item.path] = item;
	},

	getMenuItem: function (element) {
		if (element == null)
			return null;

		var itemPath = element ["__MonoMenuItemPath"];
		if (itemPath == null)
			return null;

		return this.__menuItems [itemPath];
	},

	addEventHandler: function (element, eventType, handler, capture) {
		/* There's also element.attachEvent, but it changes handler semantics on IE, so we don't
		 * even take it into consideration.
		 */
		if (element.addEventListener)
			element.addEventListener(eventType, handler, !!capture);
		else
			element ["on" + eventType] = handler;
	}
};

Sys.WebForms.Menu.prototype.loadItems = function ()
{
	var children = this.mainElement.childNodes;
	var count = children.length;
	var child;

	for (var i = 0; i < count; i++) {
		child = children [i];
		if (child.nodeType != 1)
			continue;

		if (child.tagName == "UL") {
			var submenu = new Sys.WebForms.Menu ({ element: child, disappearAfter: this.disappearAfter, orientation: this.orientation,
							       disabled: this.disabled, level: this.level + 1, tabIndex: this.tabIndex, parentMenu: this});
		} else if (child.tagName == "LI") {
			var menuItem = new Sys.WebForms.MenuItem ({ element: child, menuType: this.menuType, disappearAfter: this.disappearAfter, orientation: this.orientation,
								    disabled: this.disabled, level: this.level + 1, tabIndex: this.tabIndex, parentMenu: this});
		}
	}
}

Sys.WebForms.Menu.prototype.getNextMenuItemId = function ()
{
	return ++this.menuItemIndex;
}

Sys.WebForms.MenuItem = function (options)
{
	if (options == null)
		throw "Sys.WebForms.MenuItem constructor requires options to be not null.";

	if (options.element == null)
		throw "options.element must be set.";

	if (options.menuType == null)
		throw "options.menuType must be set.";

	if (options.parentMenu == null)
		throw "options.parentMenu is required.";

	this.element = options.element;
	this.menuType = options.menuType;
	this.parentMenu = options.parentMenu;
	this.path = this.parentMenu.path + this.parentMenu.getNextMenuItemId ();

	var children = this.element.childNodes;
	var child;
	var subMenu = null;

	for (var i = 0; i < children.length; i++) {
		child = children [i];
		if (child.nodeType != 1)
			continue;

		switch (child.tagName) {
			case "A":
				Sys.WebForms.Menu.Helpers.appendCssClass (child, this.menuType);
				child.setAttribute ("tabindex", "-1");
				break;

			case "UL":
				this.subMenu = new Sys.WebForms.Menu ({ element: child, disappearAfter: options.disappearAfter, orientation: options.orientation,
									disabled: options.disabled, level: options.level, tabIndex: options.tabIndex, parentMenu: options.parentMenu});
				if (this.subMenu.menuType == "dynamic") {
					var topValue;
					var leftValue;

					if (this.subMenu.orientation == "horizontal" && this.subMenu.parentMenu != null && this.subMenu.parentMenu.menuType == "static") {
						topValue = "100%";
						leftValue = "0px";
					} else {
						topValue = "0px";
						leftValue = "100%";
					}

					with (this.subMenu.mainElement.style) {
						display = "none";
						position = "absolute";
						top = topValue;
						left = leftValue;
					}
				}

				Sys.WebForms.Menu.Helpers.appendCssClass (this.element, "has-popup");
				this.element.setAttribute ("aria-haspopup", this.subMenu.mainElement.id);
				Sys.WebForms.Menu.Helpers.addEventHandler (this.element, "mouseover", this.mouseOverHandler);
				Sys.WebForms.Menu.Helpers.addEventHandler (this.element, "mouseout", this.mouseOutHandler);
				break;
		}
	}

	Sys.WebForms.Menu.Helpers.appendCssClass (this.element, this.menuType);
	this.element.style.position = "relative";
	this.element.setAttribute ("role", "menuitem");
	this.element ["__MonoMenuItemPath"] = this.path;

	if (this.parentMenu.orientation == "horizontal" && this.parentMenu.menuType == "static")
		Sys.WebForms.Menu.Helpers.setFloat (this.element, "left");

	Sys.WebForms.Menu.Helpers.addMenuItem (this);
}

Sys.WebForms.MenuItem.prototype.log = function (msg)
{
	if (console && console.log)
		console.log (msg);
}

Sys.WebForms.MenuItem.prototype.hide = function (popup, leaveParentOpen)
{
	if (popup == null || popup.mainElement == null || popup.menuType == "static")
		return;

	var current = popup;
	while (current != null) {
		if (current.menuType == "static" || (leaveParentOpen && current == this.parentMenu))
			break;

		if (current.mainElement != null)
			current.mainElement.style.display = "none";

		if (current.hideTimerId != null) {
			window.clearTimeout (current.hideTimerId);
			current.hideTimerId = null;
		}

		current = current.parentMenu;
	}
}

Sys.WebForms.MenuItem.prototype.onMouseOver = function (popupId)
{
	var cur = Sys.WebForms.Menu.Helpers.getPopupToClose ();
	if (cur != null) {
		if (cur.hideTimerId != null) {
			window.clearTimeout (cur.hideTimerId);
			cur.hideTimerId = null;
		}
		this.hide (cur, true);
		Sys.WebForms.Menu.Helpers.setPopupToClose (null);
	}
	if (popupId == null || popupId == "")
		return;

	var popup = document.getElementById (popupId);
	if (popup == null)
		throw "Popup with id '" + popupId + "' could not be found.";

	this.hide (cur, true);
	popup.style.display = "block";
}

Sys.WebForms.MenuItem.prototype.onMouseOut = function (popupId)
{
	if (popupId == null || popupId == "")
		return;

	var popup = document.getElementById (popupId);
	if (popup == null)
		throw "Popup with id '" + popupId + "' could not be found.";

	var cur = this.subMenu;
	if (cur != null) {
		var myself = this;
		cur.hideTimerId = window.setTimeout (function () {
							     myself.hide (cur, false);
						     },
						     this.subMenu.disappearAfter);

	}
	Sys.WebForms.Menu.Helpers.setPopupToClose (cur);
}

Sys.WebForms.MenuItem.prototype.mouseOverHandler = function (e)
{
	var menuItem = Sys.WebForms.Menu.Helpers.getMenuItem (this);
	if (menuItem == null || !(menuItem instanceof Sys.WebForms.MenuItem)) {
		e.returnResult = false;
		e.cancelBuble = false;
		throw "MenuItem could not be found in mouseover handler.";
	}

	menuItem.onMouseOver (this.getAttribute ("aria-haspopup"));
	menuItem.finalizeEvent (e);
}

Sys.WebForms.MenuItem.prototype.mouseOutHandler = function (e)
{
	var menuItem = Sys.WebForms.Menu.Helpers.getMenuItem (this);

	if (menuItem == null || !(menuItem instanceof Sys.WebForms.MenuItem)) {
		e.returnResult = false;
		e.cancelBuble = false;
		throw "MenuItem could not be found in mouseout handler.";
	}
	menuItem.onMouseOut (this.getAttribute ("aria-haspopup"));
	menuItem.finalizeEvent (e);
}

Sys.WebForms.MenuItem.prototype.finalizeEvent = function (e)
{
	/* For standards-compliant browsers */
	if (e != null) {
		if (e.preventDefault)
			e.preventDefault();
		else
			e.returnResult = false;

		if (e.stopPropagation)
			e.stopPropagation();
		else
			e.cancelBubble = true;
	}

	/* For IE */
	if (window.event != null)
		window.event.cancelBubble = true;
}