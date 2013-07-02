function Menu_OverItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	var subm = getSubMenu (menuId, itemId);
	if (subm.parentMenu == null && parentId != null)
		subm.parentMenu = getSubMenu (menuId, parentId);

	if (parentId != null && menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.dynamicHover, menu.dynamicLinkHover);
	else if (parentId == null && menu.staticHover != null)
		Menu_HilighItem (menuId, itemId, menu.staticHover, menu.staticLinkHover);
	
	if (subm.firstShown != true) {		
		var item = getMenuItem (menuId, itemId);
		var offx = 0;
		var offy = 0;
		
		if (menu.dho != null) offx += menu.dho;
		if (menu.dvo != null) offy += menu.dvo;
		
		if (menu.vertical || parentId != null)
			Menu_Reposition (menu, item, subm, item.offsetWidth + offx, offy);
		else
			Menu_Reposition (menu, item, subm, offx, item.offsetHeight + offy);
			
		subm.initialLeft = subm.style.left;
		subm.initialTop = subm.style.top;
		subm.initialContentHeight = getMenuScrollBox (menuId, itemId, "b").offsetHeight;
		subm.scrollButtonsHeight = subm.offsetHeight - subm.initialContentHeight;
		var submMargin = subm.offsetHeight - subm.clientHeight;
		subm.initialOffsetHeight = subm.offsetHeight - subm.scrollButtonsHeight + submMargin;
		subm.firstShown = true;
		
	}
	
	Menu_SetActive (menu, subm);
	Menu_ShowMenu (subm);
	Menu_Resize (subm, menuId, itemId);
}

function Menu_OverDynamicLeafItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	var subm = getSubMenu (menuId, parentId);
	Menu_SetActive (menu, subm);
	Menu_ShowMenu (subm);
	if (menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.dynamicHover, menu.dynamicLinkHover);
}

function Menu_OverStaticLeafItem (menuId, itemId) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	Menu_SetActive (menu, null);
	if (menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.staticHover, menu.staticLinkHover);
}

function Menu_HilighItem (menuId, itemId, hoverClass, hoverLinkClass)
{
	var item = getMenuItem (menuId, itemId);
	if (item.normalClass == null)
		item.normalClass = item.className;
	item.className = item.normalClass + " " + hoverClass;

	var itemLink = getMenuItemLink (menuId, itemId);
	if (itemLink.normalClass == null)
		itemLink.normalClass = itemLink.className;
	itemLink.className = itemLink.normalClass + " " + hoverLinkClass;
}

function Menu_OutItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	var subm = getSubMenu (menuId, itemId);
	if (subm == null && parentId != null)
		subm = getSubMenu (menuId, parentId);
	if (subm != null)
		Menu_HideMenu (menu, subm, menu.disappearAfter);
	var item = getMenuItem (menuId, itemId);
	if (item != null && item.normalClass != null)
		item.className = item.normalClass;
	var itemLink = getMenuItemLink (menuId, itemId);
	if (itemLink != null && itemLink.normalClass != null)
		itemLink.className = itemLink.normalClass;
}

function Menu_OverScrollBtn (menuId, parentId, updown) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	var subm = getSubMenu (menuId, parentId);
	Menu_SetActive (menu, subm);
	Menu_ShowMenu (subm);
	if (subm.scrollThread != null)
		clearInterval (subm.scrollThread);
	var box = getMenuScrollBox (menuId, parentId, "b");
	subm.scrollThread = setInterval ("Menu_ScrollMenu ('" + box.id + "','" + updown + "')", 60);
}

function Menu_OutScrollBtn (menuId, parentId, updown) {
	var menu = getMenu (menuId);
	if (menu == null)
	    return;
	var subm = getSubMenu (menuId, parentId);
	if (subm.scrollThread != null)
		clearInterval (subm.scrollThread);
	Menu_HideMenu (menu, subm, menu.disappearAfter);
}

function Menu_ScrollMenu (boxId, updown) {
	var box = document.getElementById (boxId);
	if (updown == "u") box.scrollTop -= 5;
	else box.scrollTop += 5;
}


function Menu_SetActive (menu, subm) {
	if (menu.active != null && subm != menu.active)
		Menu_HideMenu (menu, menu.active, 0);
	menu.active = subm;
}

function Menu_HideMenu (menu, subm, time)
{
	if (subm.timer != null) clearTimeout (subm.timer);
	if (time > 0) subm.timer = setTimeout ("Menu_HideMenuCallback ('" + subm.id + "')", time);
	else Menu_HideMenuCallback (subm.id);
	
	if (subm.parentMenu != null)
		Menu_HideMenu (menu, subm.parentMenu, time);
}

function Menu_HideMenuCallback (spanId)
{
	var subm = document.getElementById (spanId);
	subm.style.visibility = "hidden";
}

function Menu_ShowMenu (subm)
{
	if (subm.timer != null)
		clearTimeout (subm.timer);
		
	subm.style.visibility = "visible";

	if (subm.parentMenu != null)
		Menu_ShowMenu (subm.parentMenu);
}

function Menu_Reposition (menu, it, elem, offx, offy)
{
	var itPos = menu.webForm.WebForm_GetElementPosition(it);
	var elemPos = menu.webForm.WebForm_GetElementPosition(elem);
	elem.style.left = (elem.offsetLeft - elemPos.x + itPos.x + offx) + "px";
	elem.style.top = (elem.offsetTop - elemPos.y + itPos.y + offy) + "px";
}

function Menu_Resize (subm, menuId, itemId)
{
	var parent = subm.offsetParent;
	var box = getMenuScrollBox (menuId, itemId, "b");
	box.scrollTop = 0;
	var bottom = subm.offsetTop + subm.initialOffsetHeight - parent.scrollTop;
	var displayScroll;

	/*
	 * This is a workaround for an IE bug. IE recalculates the box offsetWidth when
	 * the box _height_ is set below - which in case of boxes with overflowing content
	 * results in a value that's just slightly smaller than the client window width.
	 * In effect, a long submenu will also be very wide, which isn't desirable.
	 */
	var newWidth = box.offsetWidth;
	
	if (bottom > window.innerHeight) {
		var overflow = bottom - window.innerHeight;
		var freeTop = subm.offsetTop - parent.scrollTop;
		if (overflow <= freeTop) {
			subm.style.top = (subm.offsetTop - overflow) + "px";
			displayScroll = "none";
			box.style.height = subm.initialContentHeight + "px";
		} else {
			subm.style.top = (subm.offsetTop - freeTop) + "px";
			var bh = (window.innerHeight - subm.offsetTop + parent.scrollTop) - subm.scrollButtonsHeight;
			box.style.overflow = "hidden";
			box.style.height = bh + "px";
			displayScroll = "block";
			
		}
	} else {
		displayScroll = "none";
		box.style.height = subm.initialContentHeight + "px";
	}
	subm.style.width = newWidth + "px";

	var btn = getMenuScrollBox (menuId, itemId, "u");
	btn.style.display = displayScroll;
	btn = getMenuScrollBox (menuId, itemId, "d");
	btn.style.display = displayScroll;
}

function getMenu (menuId) { try { return eval (menuId + "_data"); } catch(e) { return null; } }
function getSubMenu (menuId, itemId) { return document.getElementById (menuId + "_" + itemId + "s"); }
function getMenuItem (menuId, itemId) { return document.getElementById (menuId + "_" + itemId + "i"); }
function getMenuItemLink (menuId, itemId) { return document.getElementById (menuId + "_" + itemId + "l"); }
function getMenuScrollBox (menuId, itemId, btn) { return document.getElementById (menuId + "_" + itemId + "c" + btn); }

