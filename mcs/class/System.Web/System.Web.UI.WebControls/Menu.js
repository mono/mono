
function Menu_OverItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	var subm = getSubMenu (menuId, itemId);
	if (subm.parentMenu == null && parentId != null)
		subm.parentMenu = getSubMenu (menuId, parentId);
	
	if (subm.firstShown != true) {
		var item = getMenuItem (menuId, itemId);
		var offx; var offy;
		if (subm.parentMenu != null) {
			offx = parseInt (subm.parentMenu.style.left);
			offy = parseInt (subm.parentMenu.style.top);
		} else {
			offx = offy = 0;
		}
		
		if (menu.dho != null) offx += menu.dho;
		if (menu.dvo != null) offy += menu.dvo;
		
		if (menu.vertical || parentId != null)
			Menu_Reposition (item, subm, item.offsetWidth + offx, offy);
		else
			Menu_Reposition (item, subm, offx, item.offsetHeight + offy);
		subm.firstShown = true;
	}
	
	Menu_SetActive (menu, subm);
	Menu_ShowMenu (subm);
	if (parentId != null && menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.dynamicHover);
	else if (parentId == null && menu.staticHover != null)
		Menu_HilighItem (menuId, itemId, menu.staticHover);
}

function Menu_OverDynamicLeafItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	var subm = getSubMenu (menuId, parentId);
	Menu_SetActive (menu, subm);
	Menu_ShowMenu (subm);
	if (menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.dynamicHover);
}

function Menu_OverStaticLeafItem (menuId, itemId) {
	var menu = getMenu (menuId);
	Menu_SetActive (menu, null);
	if (menu.dynamicHover != null)
		Menu_HilighItem (menuId, itemId, menu.staticHover);
}

function Menu_HilighItem (menuId, itemId, hoverClass)
{
	var item = getMenuItem (menuId, itemId);
	if (item.normalClass == null)
		item.normalClass = item.className;
	item.className = item.normalClass + " " + hoverClass;
}

function Menu_OutItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	var subm = getSubMenu (menuId, itemId);
	if (subm == null && parentId != null)
		subm = getSubMenu (menuId, parentId);
	if (subm != null)
		Menu_HideMenu (menu, subm, menu.disappearAfter);
	var item = getMenuItem (menuId, itemId);
	if (item != null && item.normalClass != null)
		item.className = item.normalClass;
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
//	subm.style.display = "none";
	subm.style.visibility = "hidden";
}

function Menu_ShowMenu (subm)
{
	if (subm.timer != null)
		clearTimeout (subm.timer);
		
//	subm.style.display = "block";
	subm.style.visibility = "visible";

	if (subm.parentMenu != null)
		Menu_ShowMenu (subm.parentMenu);
}

function Menu_Reposition (it, elem, offx, offy)
{
	var le = 0;
	var to = 0;
	while (it != null && it.style.position != "absolute") {
		le += it.offsetLeft;
		to += it.offsetTop;
		it = it.offsetParent;
	}
	elem.style.left = (le + offx) + "px";
	elem.style.top = (to + offy) + "px";
}

function getMenu (menuId) { return eval (menuId + "_data"); }
function getSubMenu (menuId, itemId) { return document.getElementById (menuId + "_" + itemId + "s"); }
function getMenuItem (menuId, itemId) { return document.getElementById (menuId + "_" + itemId + "i"); }

