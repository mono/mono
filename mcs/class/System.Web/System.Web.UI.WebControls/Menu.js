var __rootMenuItem;
var __menuInterval;
var __scrollPanel;
var __disappearAfter = 500;

 // resets the global menu timer
function Menu_ClearInterval() {
    if (__menuInterval) {
        window.clearInterval(__menuInterval);
    }
}

 // collapses a menu item
function Menu_Collapse(item) {
//  document.body.insertAdjacentHTML("beforeEnd", "Collapsing " + item.innerText + "<br/>");
    // item is the A tag
    Menu_SetRoot(item);
    if (__rootMenuItem) {
        Menu_ClearInterval();
        if (__disappearAfter >= 0) {
            __menuInterval = window.setInterval("Menu_HideItems()", __disappearAfter);
        }
    }
}

 // expands a menu item
function Menu_Expand(item, horizontalOffset, verticalOffset, hideScrollers) {
//  document.body.insertAdjacentHTML("beforeEnd", "Expanding " + item.innerText + "<br/>");
    // item is the A tag
    Menu_ClearInterval();
    var tr = item.parentNode.parentNode.parentNode.parentNode.parentNode;
    var horizontal = true;
    if (!tr.id) {
        horizontal = false;
        tr = tr.parentNode;
    }
    var child = Menu_FindSubMenu(item);
    if (child) {
//      document.body.insertAdjacentHTML("beforeEnd", "- Child: " + child.id + "<br/>");
        var data = Menu_GetData(item);
        if (!data) {
            return null;
        }
        child.rel = tr.id;
        child.x = horizontalOffset;
        child.y = verticalOffset;
        if (horizontal) child.pos = "bottom";
        PopOut_Show(child.id, hideScrollers, data);
    }
    Menu_SetRoot(item);
    if (child) {
        if (!document.body.__oldOnClick && document.body.onclick) {
            document.body.__oldOnClick = document.body.onclick;
        }
        if (__rootMenuItem) {
//          document.body.insertAdjacentHTML("beforeEnd", "- Root: " + __rootMenuItem.id + "<br/>");
            document.body.onclick = Menu_HideItems;
        }
    }
    Menu_ResetSiblings(tr);
    return child;
}

 // finds the menu for an item
function Menu_FindMenu(item) {
//  document.body.insertAdjacentHTML("beforeEnd", "looking at " + (item && item.tagName ? item.tagName : "null") + "<br/>");
    // item is the A tag
    // Look for cached Menu on the item
    if (item && item.menu) return item.menu;
    // the id is on the tr or the td
//  document.body.insertAdjacentHTML("beforeEnd", "item is " + item.tagName + "<br/>");
    var tr = item.parentNode.parentNode.parentNode.parentNode.parentNode;
    if (!tr.id) {
//      document.body.insertAdjacentHTML("beforeEnd", "tr is " + tr.tagName + "<br/>");
        tr = tr.parentNode;
    }
//  document.body.insertAdjacentHTML("beforeEnd", "Done. Getting menu for " + tr.id + "<br/>");
    for (var i = tr.id.length - 1; i >= 0; i--) {
        if (tr.id.charAt(i) < '0' || tr.id.charAt(i) > '9') {
//          document.body.insertAdjacentHTML("beforeEnd", "Looking for " + tr.id.substr(0, i) + "<br/>");
            var menu = WebForm_GetElementById(tr.id.substr(0, i));
            if (menu) {
                item.menu = menu;
                return menu;
            }
        }
    }
    return null;
}

 // Finds the next item at the same level
function Menu_FindNext(item) {
    // Find the A tag for the item for future comparison
    var a = WebForm_GetElementByTagName(item, "A");
    var parent = Menu_FindParentContainer(item);
//  document.all.debug.value += "Parent: " + parent.id + "\r\n";
    var first = null;
    if (parent) {
        var links = WebForm_GetElementsByTagName(parent, "A");
        var match = false;
        for (var i = 0; i < links.length; i++) {
            var link = links[i];
            if (Menu_IsSelectable(link)) {
                // Marking the index for use in Menu_Key
                //link.MenuIndex = i;
                // Is the link a child of the same parent item?
                if (Menu_FindParentContainer(link) == parent) {
                    if (match) {
                        // If we already found item, return this item
                        return link;
                    }
                    else if (!first) {
                        // If we haven't memorized a first item, this must be it
                        first = link;
                    }
                }
                if (!match && link == a) {
                    // Found a match. Remember it.
                    match = true;
                }
            }
        }
    }
    // If nothing was found, just return the first item
    return first;
}

 function Menu_FindParentContainer(item) {
    if (item.menu_ParentContainerCache) return item.menu_ParentContainerCache;
    var a = (item.tagName.toLowerCase() == "a") ? item : WebForm_GetElementByTagName(item, "A");
    var menu = Menu_FindMenu(a);
    if (menu) {
        // Go up until you find a div or the root menu
        var parent = item;
        while (parent && parent.tagName &&
            parent.id != menu.id &&
            parent.tagName.toLowerCase() != "div") {

             parent = parent.parentNode;
//          document.all.debug.value += "Exploring " + parent.tagName + "(id=" + parent.id + ")" + "\r\n";
        }
        item.menu_ParentContainerCache = parent;
        return parent;
    }
}

 function Menu_FindParentItem(item) {
    var parentContainer = Menu_FindParentContainer(item);
    var parentContainerID = parentContainer.id;
    var len = parentContainerID.length;
    if (parentContainerID && parentContainerID.substr(len - 5) == "Items") {
        var parentItemID = parentContainerID.substr(0, len - 5);
//      document.all.debug.value += "Parent node ID is " + parentItemID + "\r\n";
        return WebForm_GetElementById(parentItemID);
    }
    return null;
}

 // Finds the previous item at the same level
function Menu_FindPrevious(item) {
    // Find the A tag for the item for future comparison
    var a = WebForm_GetElementByTagName(item, "A");
    var parent = Menu_FindParentContainer(item);
    var last = null;
    if (parent) {
        var links = WebForm_GetElementsByTagName(parent, "A");
        for (var i = 0; i < links.length; i++) {
            var link = links[i];
            if (Menu_IsSelectable(link)) {
                // Marking the index for use in Menu_Key
                //link.MenuIndex = i;
                // if we found our link and we have memorized an item before it, return it.
                if (link == a && last) {
                    return last;
                }
                // Check the found link is a child item of the item's parent
                if (Menu_FindParentContainer(link) == parent) {
                    last = link;
                }
            }
        }
    }
    // If no item was found, just return the last one
    return last;
}

 function Menu_FindSubMenu(item) {
    // item is the A tag
    // the id is on the tr or the td
    var tr = item.parentNode.parentNode.parentNode.parentNode.parentNode;
    if (!tr.id) {
        tr=tr.parentNode;
    }
    // child is what we want to show (the pop-out panel)
    return WebForm_GetElementById(tr.id + "Items");
}

 function Menu_Focus(item) {
    if (item && item.focus) {
        // Check if the item is in the viewable area of the submenu
        var pos = WebForm_GetElementPosition(item);
        var parentContainer = Menu_FindParentContainer(item);
        if (!parentContainer.offset) {
            parentContainer.offset = 0;
        }
        var posParent = WebForm_GetElementPosition(parentContainer);
        var delta;
        if (pos.y + pos.height > posParent.y + parentContainer.offset + parentContainer.clippedHeight) {
            delta = pos.y + pos.height - posParent.y - parentContainer.offset - parentContainer.clippedHeight;
            PopOut_Scroll(parentContainer, delta);
        }
        else if (pos.y < posParent.y + parentContainer.offset) {
            delta = posParent.y + parentContainer.offset - pos.y;
            PopOut_Scroll(parentContainer, -delta);
        }
        PopOut_HideScrollers(parentContainer);
        item.focus();
    }
}

 function Menu_GetData(item) {
    if (!item.data) {
        var a = (item.tagName.toLowerCase() == "a" ? item : WebForm_GetElementByTagName(item, "a"));
        var menu = Menu_FindMenu(a);
//      document.all.debug.value += "Menu: " + menu.tagName + " " + menu.id + "\r\n";
        try {
            item.data = eval(menu.id + "_Data");
        }
        catch(e) {}
    }
    return item.data;
}

 // recursively hides the items in a given panel
function Menu_HideItems(items) {
    // Restore the original onclick handler
    if (document.body.__oldOnClick) {
        document.body.onclick = document.body.__oldOnClick;
        document.body.__oldOnClick = null;
    }
    // items is a pop-out panel containing menu items
    Menu_ClearInterval();
    // Get to the table which contains the submenus to reset
    // On IE, the items parameter will be undefined or an HTML element,
    // which will stop the bool evaluation at !items or typeof(items.tagName) == "undefined".
    // On other browsers, the parameter will be an Event or an HTML element.
    if (!items || ((typeof(items.tagName) == "undefined") && (items instanceof Event))) {
        items = __rootMenuItem;
    }
    //document.body.insertAdjacentHTML("beforeEnd", items.tagName + " " + items.id + " (0)<br/>");
    var table = items;
    if ((typeof(table) == "undefined") || (table == null) || !table.tagName || (table.tagName.toLowerCase() != "table")) {
        table = WebForm_GetElementByTagName(table, "TABLE");
    }
    // Return if this is still not a table
    if ((typeof(table) == "undefined") || (table == null) || !table.tagName || (table.tagName.toLowerCase() != "table")) {
        return;
    }
    //document.body.appendChild(document.createTextNode(table.rows.length + "|"));
    // Reset submenus
    var rows = table.rows ? table.rows : table.firstChild.rows;
    //document.body.appendChild(document.createTextNode(rows.length + "|"));

     var isVertical = false;
    for (var r = 0; r < rows.length; r++) {
        if (rows[r].id) {
            isVertical = true;
            break;
        }
    }

     var i, child, nextLevel;
    if (isVertical) {
        // We have a vertical submenu (id is on a tr tag)
        for(i = 0; i < rows.length; i++) {
            if (rows[i].id) {
                //document.body.appendChild(document.createTextNode(rows[i].id + " v|"));
                //document.all.debug.value += rows[i].tagName + " " + rows[i].id + " (1)\r\n";
                child = WebForm_GetElementById(rows[i].id + "Items");
                if (child) {
                    //document.all.debug.value += "Calling Menu_HideItems(" + rows[i].id + "Items)\r\n";
                    Menu_HideItems(child);
                }
            }
            else if (rows[i].cells[0]) {
                // Static submenu: drill into the next level
                nextLevel = WebForm_GetElementByTagName(rows[i].cells[0], "TABLE");
                if (nextLevel) {
                    //document.body.appendChild(document.createTextNode(rows[i].cells[0].innerHTML + " v|"));
                    Menu_HideItems(nextLevel);
                }
            }
        }
    }
    else if (rows[0]) {
        // The rows[0] check is to ensure that we've not torn down the DOM corresponding to the
        // menu we're trying to unhover.  This is very likely to happen if the last active menu was
        // in an UpdatePanel and got replaced.

         // We have a horizontal submenu (id is on a td tag)
        for(i = 0; i < rows[0].cells.length; i++) {
            if (rows[0].cells[i].id) {
                //document.body.appendChild(document.createTextNode(rows[0].cells[i].id + " h|"));
                //document.all.debug.value += rows[0].cells[i].tagName + " " + rows[0].cells[i].id + "\r\n";
                child = WebForm_GetElementById(rows[0].cells[i].id + "Items");
                if (child) {
                    Menu_HideItems(child);
                }
            }
            else {
                // Static submenu: drill into the next level
                nextLevel = WebForm_GetElementByTagName(rows[0].cells[i], "TABLE");
                if (nextLevel) {
                    Menu_HideItems(rows[0].cells[i].firstChild);
                }
            }
        }
    }
    if (items && items.id) {
        PopOut_Hide(items.id);
    }
}

 function Menu_HoverDisabled(item) {
//  document.all.debug.value += "Menu_HoverDynamic " + item.outerHTML + "\r\n";
    // Get to the TD
    var node = (item.tagName.toLowerCase() == "td") ?
        item:
        item.cells[0];

     // Find the data structure:
    var data = Menu_GetData(item);
    if (!data) return;

     node = WebForm_GetElementByTagName(node, "table").rows[0].cells[0].childNodes[0];
    // Set disappearAfter
    if (data.disappearAfter >= 200) {
        __disappearAfter = data.disappearAfter;
    }
    Menu_Expand(node, data.horizontalOffset, data.verticalOffset); 
}

 // sets styles and expands dynamic items
function Menu_HoverDynamic(item) {
//  document.all.debug.value += "Menu_HoverDynamic " + item.outerHTML + "\r\n";
    // Get to the TD
    var node = (item.tagName.toLowerCase() == "td") ?
        item:
        item.cells[0];

     // Find the data structure:
    var data = Menu_GetData(item);
    if (!data) return;

     var nodeTable = WebForm_GetElementByTagName(node, "table");
    if (data.hoverClass) {
        // Merge the hover style class onto the TD
        nodeTable.hoverClass = data.hoverClass;
        WebForm_AppendToClassName(nodeTable, data.hoverClass);
    }

     node = nodeTable.rows[0].cells[0].childNodes[0];
    if (data.hoverHyperLinkClass) {
        // Merge the hyperlink hover style class
        node.hoverHyperLinkClass = data.hoverHyperLinkClass;
        WebForm_AppendToClassName(node, data.hoverHyperLinkClass);
    }
    // Set disappearAfter
    if (data.disappearAfter >= 200) {
        __disappearAfter = data.disappearAfter;
    }
    Menu_Expand(node, data.horizontalOffset, data.verticalOffset); 
}

 // sets styles for static items before the last level
function Menu_HoverRoot(item) {
//  document.all.debug.value += "Menu_HoverRoot " + item.outerHTML + "\r\n";
    // Get to the TD
    var node = (item.tagName.toLowerCase() == "td") ?
        item:
        item.cells[0];

     // Find the data structure:
    var data = Menu_GetData(item);
    if (!data) {
        return null;
    }

     var nodeTable = WebForm_GetElementByTagName(node, "table");
    if (data.staticHoverClass) {
        // Merge the hover style class onto the TD
        nodeTable.hoverClass = data.staticHoverClass;
        WebForm_AppendToClassName(nodeTable, data.staticHoverClass);
    }

     node = nodeTable.rows[0].cells[0].childNodes[0];
    if (data.staticHoverHyperLinkClass) {
        // Merge the hyperlink hover style class
        node.hoverHyperLinkClass = data.staticHoverHyperLinkClass;
        WebForm_AppendToClassName(node, data.staticHoverHyperLinkClass);
    }

     return node;
}

 // sets styles and expands the item for the last static level
function Menu_HoverStatic(item) {
//  document.all.debug.value += "Menu_HoverStatic " + item.outerHTML + "\r\n";
    var node = Menu_HoverRoot(item);

     // Find the data structure:
    var data = Menu_GetData(item);
    if (!data) return;

     // Set disappearAfter
    __disappearAfter = data.disappearAfter;
    Menu_Expand(node, data.horizontalOffset, data.verticalOffset); 
}

 // Returns true if the item is in a horizontal submenu
function Menu_IsHorizontal(item) {
    if (item) {
        var a = ((item.tagName && (item.tagName.toLowerCase == "a")) ? item : WebForm_GetElementByTagName(item, "A"));
        if (!a) {
            return false;
        }
        // Try to determine if the menu is horizontal (the id is on the tr for vertical):
        var td = a.parentNode.parentNode.parentNode.parentNode.parentNode;
        if (td.id) {
            return true;
        }
    }
    return false;
}

 function Menu_IsSelectable(link) {
    // Menu item is selectable if it is a link that has an href
    return (link && link.href)
}

 // interprets key strokes
function Menu_Key(item) {
    // Gecko browsers will communicate the event as a parameter and we'll have to find out what triggered it from there
    // Whereas IE will pass the item that triggered the event as the parameter and will get the key from window.event.

     // NB not returning true or false from this method to manage event bubbling in IE 4 because IE 4 uses the adapter.
    var event;
    if (item.currentTarget) {
        event = item;
        item = event.currentTarget;
    }
    else {
        event = window.event;        
    }
    var key = (event ? event.keyCode : -1);
//  document.body.appendChild(document.createTextNode(item.id + "/" + key + "|"));
//  document.body.insertAdjacentHTML("beforeEnd", "Pressed " + key + "<br/>");
    var data = Menu_GetData(item);
    if (!data) return;
    // Try to determine if the menu is horizontal (the id is on the tr for vertical):
    var horizontal = Menu_IsHorizontal(item);
    var a = WebForm_GetElementByTagName(item, "A");
    var nextItem, parentItem, previousItem;
    if ((!horizontal && key == 38) || (horizontal && key == 37)) {
        // Up/vertical or left/horizontal
        previousItem = Menu_FindPrevious(item);
        while (previousItem && previousItem.disabled) {
            previousItem = Menu_FindPrevious(previousItem);
        }
        if (previousItem) {
            Menu_Focus(previousItem);
            Menu_Expand(previousItem, data.horizontalOffset, data.verticalOffset, true);
            event.cancelBubble = true;
            if (event.stopPropagation) event.stopPropagation();
            return;
        }
    }
    if ((!horizontal && key == 40) || (horizontal && key == 39)) {
        // Down/vertical or right/horizontal
        if (horizontal) {
            // Find out if the submenu is shown
            var subMenu = Menu_FindSubMenu(a);
            if (subMenu && subMenu.style && subMenu.style.visibility && 
                subMenu.style.visibility.toLowerCase() == "hidden") {

                 // expand the submenu instead of going to the next item (see VSWhidbey 256947)
                Menu_Expand(a, data.horizontalOffset, data.verticalOffset, true);
                event.cancelBubble = true;
                if (event.stopPropagation) event.stopPropagation();
                return;
            }
        }
        nextItem = Menu_FindNext(item);
        while (nextItem && nextItem.disabled) {
            nextItem = Menu_FindNext(nextItem);
        }
        if (nextItem) {
            Menu_Focus(nextItem);
            Menu_Expand(nextItem, data.horizontalOffset, data.verticalOffset, true);
            event.cancelBubble = true;
            if (event.stopPropagation) event.stopPropagation();
            return;
        }
    }
    if ((!horizontal && key == 39) || (horizontal && key == 40)) {
        // Right/vertical or down/horizontal
        var children = Menu_Expand(a, data.horizontalOffset, data.verticalOffset, true);
        if (children) {
            var firstChild;
            children = WebForm_GetElementsByTagName(children, "A");
            for (var i = 0; i < children.length; i++) {
                if (!children[i].disabled && Menu_IsSelectable(children[i])) {
                    firstChild = children[i];
                    break;
                }
            }
            if (firstChild) {
                Menu_Focus(firstChild);
                Menu_Expand(firstChild, data.horizontalOffset, data.verticalOffset, true);
                event.cancelBubble = true;
                if (event.stopPropagation) event.stopPropagation();
                return;
            }
        }
        else {
            // No children, we want to switch to the next static horizontal submenu if there is one
            parentItem = Menu_FindParentItem(item);
            while (parentItem && !Menu_IsHorizontal(parentItem)) {
                parentItem = Menu_FindParentItem(parentItem);
            }
            if (parentItem) {
                // Found a horizontal parent
                nextItem = Menu_FindNext(parentItem);
                while (nextItem && nextItem.disabled) {
                    nextItem = Menu_FindNext(nextItem);
                }
                if (nextItem) {
                    Menu_Focus(nextItem);
                    Menu_Expand(nextItem, data.horizontalOffset, data.verticalOffset, true);
                    event.cancelBubble = true;
                    if (event.stopPropagation) event.stopPropagation();
                    return;
                }
            }
        }
    }
    if ((!horizontal && key == 37) || (horizontal && key == 38)) {
        // Left/vertical or up/horizontal
        parentItem = Menu_FindParentItem(item);
        if (parentItem) {
            // If the parent is horizontal, we want to go to its previous sibling
            if (Menu_IsHorizontal(parentItem)) {
                previousItem = Menu_FindPrevious(parentItem);
                while (previousItem && previousItem.disabled) {
                    previousItem = Menu_FindPrevious(previousItem);
                }
                if (previousItem) {
                    Menu_Focus(previousItem);
                    Menu_Expand(previousItem, data.horizontalOffset, data.verticalOffset, true);
                    event.cancelBubble = true;
                    if (event.stopPropagation) event.stopPropagation();
                    return;
                }
            }
            var parentA = WebForm_GetElementByTagName(parentItem, "A");
            if (parentA) {
                Menu_Focus(parentA);
            }
            Menu_ResetSiblings(parentItem);
            event.cancelBubble = true;
            if (event.stopPropagation) event.stopPropagation();
            return;
        }
    }
    if (key == 27) {
        // Esc key
        Menu_HideItems();
        event.cancelBubble = true;
        if (event.stopPropagation) event.stopPropagation();
        return;
    }
}

 // collapses the siblings of a menu item so that only one is open at the same time
function Menu_ResetSiblings(item) {
    // item is the tr tag or the td, table is its parent table
    var table = (item.tagName.toLowerCase() == "td") ?
        item.parentNode.parentNode.parentNode :
        item.parentNode.parentNode;

     var isVertical = false;
    for (var r = 0; r < table.rows.length; r++) {
        if (table.rows[r].id) {
            isVertical = true;
            break;
        }
    }
    var i, child, childNode;
    if (isVertical) {
        for(i = 0; i < table.rows.length; i++) {
            childNode = table.rows[i];
            if (childNode != item) {
                child = WebForm_GetElementById(childNode.id + "Items");
                if (child) {
                    Menu_HideItems(child);
                }
            }
        }
    }
    else {
        // This must be a horizontal root menu: ids are on the tds
        for(i = 0; i < table.rows[0].cells.length; i++) {
            childNode = table.rows[0].cells[i];
            if (childNode != item) {
                child = WebForm_GetElementById(childNode.id + "Items");
                if (child) {
                    Menu_HideItems(child);
                }
            }
        }
    }
    // Check other siblings (multiple static display levels case)
    Menu_ResetTopMenus(table, table, 0, true);
}

 // Explores the menu item tree to reset all siblings, on all branches
function Menu_ResetTopMenus(table, doNotReset, level, up) {
//  document.all.debug.value += "Menu_ResetTopMenus(" + table.id + ", doNotReset, " + level + ", " + up + ")\r\n";
    var i, child, childNode;
    if (up && table.id == "") {
        var parentTable = table.parentNode.parentNode.parentNode.parentNode;
        if (parentTable.tagName.toLowerCase() == "table") {
            // Go up one more level
            Menu_ResetTopMenus(parentTable, doNotReset, level + 1, true);
        }
    }
    else {
        // We've reached the top-level node
        if (level == 0 && table != doNotReset) {
            // This is a sibling: reset all its items
            if (table.rows[0].id) {
                for(i = 0; i < table.rows.length; i++) {
                    childNode = table.rows[i];
                    child = WebForm_GetElementById(childNode.id + "Items");
                    if (child) {
                        Menu_HideItems(child);
                    }
                }
            }
            else {
                // This must be a horizontal root menu: ids are on the tds
                for(i = 0; i < table.rows[0].cells.length; i++) {
                    childNode = table.rows[0].cells[i];
                    child = WebForm_GetElementById(childNode.id + "Items");
                    if (child) {
                        Menu_HideItems(child);
                    }
                }
            }
        }
        else if (level > 0) {
            // Explore all children
            for (i = 0; i < table.rows.length; i++) {
                for (var j = 0; j < table.rows[i].cells.length; j++) {
                    var subTable = table.rows[i].cells[j].firstChild;
                    if (subTable && subTable.tagName.toLowerCase() == "table") {
                        // Go down one level
                        Menu_ResetTopMenus(subTable, doNotReset, level - 1, false);
                    }
                }
            }
        }
    }
}

 function Menu_RestoreInterval() {
    if (__menuInterval && __rootMenuItem) {
        Menu_ClearInterval();
        __menuInterval = window.setInterval("Menu_HideItems()", __disappearAfter);
    }
}

 // sets the global current root to the root for this item
function Menu_SetRoot(item) {
    // item is the A tag
    var newRoot = Menu_FindMenu(item);
    if (newRoot) {
        if (__rootMenuItem && __rootMenuItem != newRoot) {
            // If multiple menus on page, close all before scheduling to close the current one
            Menu_HideItems();
        }
        __rootMenuItem = newRoot;
    }
}

 // resets styles and collapses the item
function Menu_Unhover(item) {
    // Get to the TD
    var node = (item.tagName.toLowerCase() == "td") ?
        item:
        item.cells[0];
    var nodeTable = WebForm_GetElementByTagName(node, "table");
    // Remove the hover style class from the TD
    if (nodeTable.hoverClass) {
        WebForm_RemoveClassName(nodeTable, nodeTable.hoverClass);
    }

     node = nodeTable.rows[0].cells[0].childNodes[0];
    // Remove the hyperlink hover style class
    if (node.hoverHyperLinkClass) {
        WebForm_RemoveClassName(node, node.hoverHyperLinkClass);
    }
    Menu_Collapse(node);
}

 function PopOut_Clip(element, y, height) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Clip(" + element.id + ", " + y + ", " + height + ")<br/>");
    if (element && element.style) {
        element.style.clip = "rect(" + y + "px auto " + (y + height) + "px auto)";
        element.style.overflow = "hidden";
    }
}

 // Scrolls down a pop-out panel given a reference to its down scroller div
function PopOut_Down(scroller) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Down(" + scroller + ")<br/>");
    Menu_ClearInterval();
    var panel;
    if (scroller) {
        panel = scroller.parentNode
    }
    else {
        panel = __scrollPanel;
    }
    if (panel && ((panel.offset + panel.clippedHeight) < panel.physicalHeight)) {
        // change clipping
        PopOut_Scroll(panel, 2)
        __scrollPanel = panel;
        // move scroller and update visibility
        PopOut_ShowScrollers(panel);
        PopOut_Stop();
        __scrollPanel.interval = window.setInterval("PopOut_Down()", 8);
    }
    else {
        PopOut_ShowScrollers(panel);
    }
}

 // Hides a pop-out
function PopOut_Hide(panelId) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Hide(" + panelId + ")<br/>");
    var panel = WebForm_GetElementById(panelId);
    if (panel && panel.tagName.toLowerCase() == "div") {
        panel.style.visibility = "hidden";
        panel.style.display = "none";
        panel.offset = 0;
        panel.scrollTop = 0;
        var table = WebForm_GetElementByTagName(panel, "TABLE");
        if (table) {
            WebForm_SetElementY(table, 0);
        }
        // For IE, we have to hide the IFrame that hides select elements
        if (window.navigator && window.navigator.appName == "Microsoft Internet Explorer" &&
            !window.opera) {

             var childFrameId = panel.id + "_MenuIFrame";
            var childFrame = WebForm_GetElementById(childFrameId);
            if (childFrame) {
                childFrame.style.display = "none";
            }
        }
    }
}

 // Hide scrolling divs for a pop-out panel
function PopOut_HideScrollers(panel) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_HideScrollers(" + panel.id + ")<br/>");
    if (panel && panel.style) {
        // Find the scroller divs
        var up = WebForm_GetElementById(panel.id + "Up");
        var dn = WebForm_GetElementById(panel.id + "Dn");
        if (up) {
            up.style.visibility = "hidden";
            up.style.display = "none";
        }
        if (dn) {
            dn.style.visibility = "hidden";
            dn.style.display = "none";
        }
    }
}

 // Smart position for a pop-out
function PopOut_Position(panel, hideScrollers) {
    // Opera has this weird bug where if you position a div that is a child of a nested table cell too much on the right,
    // it will be clipped 2 pixels wide (only the border is visible). VSWhidbey 458988.
    // To solve that problem, we move the div outside of the table.
    if (window.opera) {
        panel.parentNode.removeChild(panel);
        document.forms[0].appendChild(panel);
    }
    var rel = WebForm_GetElementById(panel.rel);
    // Get cross-browser positions for the panel and its offset origin element
    // We want the coordinates of the element's table if we can find it
    var relTable = WebForm_GetElementByTagName(rel, "TABLE");
    var relCoordinates = WebForm_GetElementPosition(relTable ? relTable : rel);
    var panelCoordinates = WebForm_GetElementPosition(panel);
    // caching the original height and position of the panel before we resize and reposition it
    var panelHeight = ((typeof(panel.physicalHeight) != "undefined") && (panel.physicalHeight != null)) ?
        panel.physicalHeight :
        panelCoordinates.height;
    panel.physicalHeight = panelHeight;
    var panelParentCoordinates;
    if (panel.offsetParent) {
        panelParentCoordinates = WebForm_GetElementPosition(panel.offsetParent);
    }
    else {
        panelParentCoordinates = new Object();
        panelParentCoordinates.x = 0;
        panelParentCoordinates.y = 0;
    }
    // VSWhidbey 289016:
    // There's no way in IE to get the actual size of the client area.
    // We can only get the height of body, which can be smaller, in particular
    // if the menu is the last thing in the document.
    // So let's generate an img big enough so that clientheight is actually
    // the height of the client and not the height of the document.
    var overflowElement = WebForm_GetElementById("__overFlowElement");
    if (!overflowElement) {
        overflowElement = document.createElement("img");
        overflowElement.id="__overFlowElement";
        WebForm_SetElementWidth(overflowElement, 1);
        document.body.appendChild(overflowElement);
    }
    WebForm_SetElementHeight(overflowElement, panelHeight + relCoordinates.y + parseInt(panel.y ? panel.y : 0));
    overflowElement.style.visibility = "visible";
    overflowElement.style.display = "inline";
    // Get the total size of the client area
    var clientHeight = 0;
    var clientWidth = 0;
    if (window.innerHeight) {
        // Some non-IE browsers
        clientHeight = window.innerHeight;
        clientWidth = window.innerWidth;
    }
    else if (document.documentElement && document.documentElement.clientHeight) {
        // This is IE6 in standard mode
        clientHeight = document.documentElement.clientHeight;
        clientWidth = document.documentElement.clientWidth;
    }
    else if (document.body && document.body.clientHeight) {
        // IE non-standard
        clientHeight = document.body.clientHeight;
        clientWidth = document.body.clientWidth;
    }
    // Get the scrolling position of the window
    var scrollTop = 0;
    var scrollLeft = 0;
    if (typeof(window.pageYOffset) != "undefined") {
        // Some non-IE browsers
        scrollTop = window.pageYOffset;
        scrollLeft = window.pageXOffset;
//      document.body.appendChild(document.createTextNode("NS " + scrollTop + "|"));
    }
    else if (document.documentElement && (typeof(document.documentElement.scrollTop) != "undefined")) {
        // This is IE6 in standard mode
        scrollTop = document.documentElement.scrollTop;
        scrollLeft = document.documentElement.scrollLeft;
//      document.body.appendChild(document.createTextNode("IE standard " + scrollTop + "|"));
    }
    else if (document.body && (typeof(document.body.scrollTop) != "undefined")) {
        // IE non-standard & DOM
        scrollTop = document.body.scrollTop;
        scrollLeft = document.body.scrollLeft;
//      document.body.appendChild(document.createTextNode("DOM " + scrollTop + "|"));
    }
    // Hide the overflow element
    overflowElement.style.visibility = "hidden";
    overflowElement.style.display = "none";
    // Get border positions
    var bottomWindowBorder = clientHeight + scrollTop;
    var rightWindowBorder = clientWidth + scrollLeft;
    // Get the position of the panel relative to itset origin element
    var position = panel.pos;
    if ((typeof(position) == "undefined") || (position == null) || (position == "")) {
        position = (WebForm_GetElementDir(rel) == "rtl" ? "middleleft" : "middleright");
    }
    position = position.toLowerCase();
    // Compute top position
    var y = relCoordinates.y + parseInt(panel.y ? panel.y : 0) - panelParentCoordinates.y;
    // Adjust for border width
//  document.body.insertAdjacentHTML("beforeEnd", "rel is " + rel.tagName + "<br/>");
    var borderParent = (rel && rel.parentNode && rel.parentNode.parentNode && rel.parentNode.parentNode.parentNode
        && rel.parentNode.parentNode.parentNode.tagName.toLowerCase() == "div") ?
        rel.parentNode.parentNode.parentNode : null;
//  document.body.insertAdjacentHTML("beforeEnd", "borderParent is " + (borderParent ? borderParent.id : "null") + "<br/>");
//  document.all.debug.value += panel.tagName + " " + panel.id + "\r\n" + "y=" + y + "\r\n";
//  document.all.debug.value += "panelParentCoordinates.y=" + panelParentCoordinates.y + "\r\n";
//  document.all.debug.value += "panelHeight             =" + panelHeight + "\r\n";
//  document.all.debug.value += "relCoordinates.height   =" + relCoordinates.height + "\r\n";
//  document.all.debug.value += "bottomWindowBorder      =" + bottomWindowBorder + "\r\n";
    WebForm_SetElementY(panel, y);
    PopOut_SetPanelHeight(panel, panelHeight, true);
    var clip = false;
    var overflow;
    if (position.indexOf("top") != -1) {
        // Panel is on top
        y -= panelHeight;
        WebForm_SetElementY(panel, y); 
        if (y < -panelParentCoordinates.y) {
            // Panel doesn't fit
            y = -panelParentCoordinates.y;
            WebForm_SetElementY(panel, y); 
            if (panelHeight > clientHeight - 2) {
                // Still doesn't fit, we need vertical scrolling
                clip = true;
                PopOut_SetPanelHeight(panel, clientHeight - 2);
            }
        }
    }
    else {
        if (position.indexOf("bottom") != -1) {
            // Panel is at the bottom
            y += relCoordinates.height;
            WebForm_SetElementY(panel, y); 
        }
        // Panel is at the same height as the relative
        overflow = y + panelParentCoordinates.y + panelHeight - bottomWindowBorder;
        if (overflow > 0) {
            // Panel doesn't fit
            y -= overflow;
            WebForm_SetElementY(panel, y); 
            if (y < -panelParentCoordinates.y) {
                // Still doesn't fit: too large, we need vertical scrolling
                y = 2 - panelParentCoordinates.y + scrollTop;
                WebForm_SetElementY(panel, y); 
                clip = true;
                PopOut_SetPanelHeight(panel, clientHeight - 2);
            }
        }
    }
    // Clip
    if (!clip) {
        PopOut_SetPanelHeight(panel, panel.clippedHeight, true);
    }

     var panelParentOffsetY = 0;
    if (panel.offsetParent) {
        panelParentOffsetY = WebForm_GetElementPosition(panel.offsetParent).y;
    }
    var panelY = ((typeof(panel.originY) != "undefined") && (panel.originY != null)) ?
        panel.originY :
        y - panelParentOffsetY;
    panel.originY = panelY;
//  document.all.debug.value += panel.tagName + " " + panel.id + " physicalHeight=" + panelHeight + " originY=" + panelY + "\r\n";
//  document.all.debug.value += panel.tagName + " " + panel.id + " y=" + y + " offset=" + panel.offset + "\r\n";
//  document.all.debug.value += panel.tagName + " " + panel.id + " current y=" + WebForm_GetElementPosition(panel).y + "\r\n";
//  document.all.debug.value += "hideScrollers=" + hideScrollers + "\r\n";
    if (!hideScrollers) {
        PopOut_ShowScrollers(panel);
    }
    else {
        PopOut_HideScrollers(panel);
    }

     // Compute left position
    var x = relCoordinates.x + parseInt(panel.x ? panel.x : 0) - panelParentCoordinates.x;
    // Correct for parent menu border
    if (borderParent && borderParent.clientLeft) {
        x += 2 * borderParent.clientLeft;
    }
//  document.all.debug.value += panel.tagName + " " + panel.id + " x=" + x + "\r\n";
    WebForm_SetElementX(panel, x);
    if (position.indexOf("left") != -1) {
        // Panel is on the left
        x -= panelCoordinates.width;
        WebForm_SetElementX(panel, x);
        if (x < -panelParentCoordinates.x) {
            // Panel doesn't fit
            WebForm_SetElementX(panel, -panelParentCoordinates.x);
        }
    }
    else {
        if (position.indexOf("right") != -1) {
            // Panel is on the right
            x += relCoordinates.width;
            WebForm_SetElementX(panel, x);
        }
        overflow = x + panelParentCoordinates.x + panelCoordinates.width - rightWindowBorder;
        if (overflow > 0) {
            // Panel doesn't fit
            // Is there room on the left (don't do that for horizontal layout menu)?
            if (position.indexOf("bottom") == -1 && relCoordinates.x > panelCoordinates.width) {
                x -= relCoordinates.width + panelCoordinates.width;
            }
            else {
                x -= overflow;
            }
            WebForm_SetElementX(panel, x);
            if (x < -panelParentCoordinates.x) {
                // Still doesn't fit
                WebForm_SetElementX(panel, -panelParentCoordinates.x);
            }
        }
    }
}

 // positions and clips a pop-out panel with new scrolling parameters set by PopOut_Up or PopOut_Down
function PopOut_Scroll(panel, offsetDelta) {
    var table = WebForm_GetElementByTagName(panel, "TABLE");
    if (!table) return;
    table.style.position = "relative";
    var tableY = (table.style.top ? parseInt(table.style.top) : 0);
    panel.offset += offsetDelta;
    WebForm_SetElementY(table, tableY - offsetDelta);
//  document.body.insertAdjacentHTML("beforeEnd", " panel.offset=" + panel.offset+ "<br/>");
//  document.body.insertAdjacentHTML("beforeEnd", " panel.scrollTop=" + panel.scrollTop + "<br/>");
}

 // Set the height and clips a pop-out panel
function PopOut_SetPanelHeight(element, height, doNotClip) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_SetPanelHeight(" + element.id + ", " + height + ", /* doNotClip */" + doNotClip + ")<br/>");
    if (element && element.style) {
        var size = WebForm_GetElementPosition(element);
        element.physicalWidth = size.width;
        element.clippedHeight = height;
        WebForm_SetElementHeight(element, height - (element.clientTop ? (2 * element.clientTop) : 0));
        if (doNotClip && element.style) {
//            document.body.insertAdjacentHTML("beforeEnd", "Clipping auto " + element.tagName + "<br/>");
            element.style.clip = "rect(auto auto auto auto)";
        }
        else {
            PopOut_Clip(element, 0, height);
        }
    }
}

 // Positions and shows a pop-out
function PopOut_Show(panelId, hideScrollers, data) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Show(" + panelId + ", /* hideScrollers */" + hideScrollers + ", data)<br/>");
    var panel = WebForm_GetElementById(panelId);
    if (panel && panel.tagName.toLowerCase() == "div") {
        panel.style.visibility = "visible";
        panel.style.display = "inline";
        if (!panel.offset || hideScrollers) {
            panel.scrollTop = 0;
            panel.offset = 0;
            var table = WebForm_GetElementByTagName(panel, "TABLE");
            if (table) {
                WebForm_SetElementY(table, 0);
            }
        }
        PopOut_Position(panel, hideScrollers);
        var z = 1;
        var isIE = window.navigator && window.navigator.appName == "Microsoft Internet Explorer" && !window.opera;
        if (isIE && data) {
            var childFrameId = panel.id + "_MenuIFrame";
            var childFrame = WebForm_GetElementById(childFrameId);
            var parent = panel.offsetParent;
            if (!childFrame) {
                childFrame = document.createElement("iframe");
                childFrame.id = childFrameId;
                childFrame.src = (data.iframeUrl ? data.iframeUrl : "about:blank");
                childFrame.style.position = "absolute";
                childFrame.style.display = "none";
                childFrame.scrolling = "no";
                childFrame.frameBorder = "0";
                if (parent.tagName.toLowerCase() == "html") {
                    document.body.appendChild(childFrame);
                }
                else {
                    parent.appendChild(childFrame);
                }
            }
            var pos = WebForm_GetElementPosition(panel);
            var parentPos = WebForm_GetElementPosition(parent);
            WebForm_SetElementX(childFrame, pos.x - parentPos.x);
            WebForm_SetElementY(childFrame, pos.y - parentPos.y);
            WebForm_SetElementWidth(childFrame, pos.width);
            WebForm_SetElementHeight(childFrame, pos.height);
            childFrame.style.display = "block";
            if (panel.currentStyle && panel.currentStyle.zIndex && panel.currentStyle.zIndex != "auto") {
                z = panel.currentStyle.zIndex;
            }
            else if (panel.style.zIndex) {
                z = panel.style.zIndex;
            }
        }
        // Other browsers hide the select elements natively and correctly, so we don't need to do anything.
        panel.style.zIndex = z;
    }
}

 // Show scrolling divs for a pop-out panel
function PopOut_ShowScrollers(panel) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_ShowScrollers(" + panel.id + ")<br/>");
    if (panel && panel.style) {
        // Find the scroller divs
        var up = WebForm_GetElementById(panel.id + "Up");
        var dn = WebForm_GetElementById(panel.id + "Dn");
        var cnt = 0;
        if (up && dn) {
            // Set visibility for the scrollers and initialize offset if necessary
            if (panel.offset && panel.offset > 0) {
                up.style.visibility = "visible";
                up.style.display = "inline";
                cnt++;
                // Set scroller divs' widths to the width of the panel to scroll
                if (panel.clientWidth) {
                    WebForm_SetElementWidth(up, panel.clientWidth
                        - (up.clientLeft ? (2 * up.clientLeft) : 0));
                }
                // Put the top scroller on top of the panel
                WebForm_SetElementY(up, 0);
            }
            else {
                up.style.visibility = "hidden";
                up.style.display = "none";
            }
            if (panel.offset + panel.clippedHeight + 2 <= panel.physicalHeight) {
                dn.style.visibility = "visible";
                dn.style.display = "inline";
                cnt++;
                // Set scroller divs' widths to the width of the panel to scroll
//              document.body.insertAdjacentHTML("beforeEnd", "panel " + panel.id + " clientLeft=" + panel.clientLeft + " clientWidth=" + panel.clientWidth + "<br/>");
                if (panel.clientWidth) {
                    WebForm_SetElementWidth(dn, panel.clientWidth
                        - (dn.clientLeft ? (2 * dn.clientLeft) : 0));
                }
                // Put the down scroller on the bottom of the panel
//              document.body.insertAdjacentHTML("beforeEnd", "dn " + dn.id + " clientTop=" + dn.clientTop + " panel.clientTop=" + panel.clientTop + "<br/>");
                WebForm_SetElementY(dn, panel.clippedHeight - WebForm_GetElementPosition(dn).height
                    - (panel.clientTop ? (2 * panel.clientTop) : 0));
            }
            else {
                dn.style.visibility = "hidden";
                dn.style.display = "none";
            }
            if (cnt == 0) {
                panel.style.clip = "rect(auto auto auto auto)";
            }
//          document.body.insertAdjacentHTML("beforeEnd", " panel.clippedHeight =" + panel.clippedHeight + "<br/>");
//          document.body.insertAdjacentHTML("beforeEnd", " panel.offset        =" + panel.offset + "<br/>");
//          document.body.insertAdjacentHTML("beforeEnd", " panel.physicalHeight=" + panel.physicalHeight + "<br/>");
//          document.body.insertAdjacentHTML("beforeEnd", " dn.height           =" + WebForm_GetElementPosition(dn).height + "<br/>");
//          document.body.insertAdjacentHTML("beforeEnd", "dn.y will be set to   " + (panel.offset + panel.clippedHeight - WebForm_GetElementPosition(dn).height) + "<br/>");
        }
    }
}

 function PopOut_Stop() {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Stop()<br/>");
    if (__scrollPanel && __scrollPanel.interval) {
        window.clearInterval(__scrollPanel.interval);
    }
    Menu_RestoreInterval();
}

 // Scrolls up a pop-out panel given a reference to its up scroller div
function PopOut_Up(scroller) {
//  document.body.insertAdjacentHTML("beforeEnd", "PopOut_Up(" + scroller + ")<br/>");
    Menu_ClearInterval();
    var panel;
    if (scroller) {
        panel = scroller.parentNode
    }
    else {
        panel = __scrollPanel;
    }
    if (panel && panel.offset && panel.offset > 0) {
        // change clipping
        PopOut_Scroll(panel, -2);
        __scrollPanel = panel;
        // move scroller and update visibility
        PopOut_ShowScrollers(panel);
        PopOut_Stop();
        __scrollPanel.interval = window.setInterval("PopOut_Up()", 8);
    }
}