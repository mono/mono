
function Menu_OverItem (menuId, itemId, parentId) {
	var menu = getMenu (menuId);
	var subm = getSubMenu (menuId, itemId);
	if (subm.parentMenu == null && parentId != null)
		subm.parentMenu = getSubMenu (menuId, parentId);
	var item = getMenuItem (menuId, itemId);
	
	var offx; var offy;
	if (subm.parentMenu != null) {
		offx = parseInt (subm.parentMenu.style.left);
		offy = parseInt (subm.parentMenu.style.top);
	} else {
		offx = offy = 0;
	}
	
	if (menu.vertical)
		Menu_Reposition (item, subm, item.offsetWidth + offx, offy);
	else
		Menu_Reposition (item, subm, offx, item.offsetHeight + offy);
	Menu_ShowMenu (subm);
}

function Menu_OverLeafItem (menuId, parentId) {
	Menu_ShowMenu (getSubMenu (menuId, parentId));
}

function Menu_OutItem (menuId, itemId) {
	Menu_HideMenu (menuId, getSubMenu (menuId, itemId));
}

function Menu_HideMenu (menuId, subm)
{
	var menu = getMenu (menuId);
	if (subm.timer != null) clearTimeout (subm.timer);
	subm.timer = setTimeout ("Menu_HideMenuCallback ('" + subm.id + "')", menu.disappearAfter);
	
	if (subm.parentMenu != null)
		Menu_HideMenu (menuId, subm.parentMenu);
}

function Menu_HideMenuCallback (spanId)
{
	var subm = document.getElementById (spanId);
	subm.style.display = "none";
	subm.style.visibility = "hidden";
}

function Menu_ShowMenu (subm)
{
	if (subm.timer != null)
		clearTimeout (subm.timer);
		
	subm.style.display = "block";
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

