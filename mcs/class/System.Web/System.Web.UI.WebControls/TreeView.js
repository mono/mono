
function TreeView_ToggleExpand (treeId, nodeId, nodeValue, nodeImageUrl, nodeNavigateUrl, nodeTarget) {
	var tree = getTree (treeId);
	if (tree == null)
	    return;
	var spanId = treeId + "_" + nodeId;
	var node = document.getElementById (spanId);
	var expand = node.style.display == "none";
	
	if (tree.populateFromClient && expand && node.populated != true && (node.innerHTML.length == 0 || node.hasError)) {
	    tree.PopulateNode (nodeId, nodeValue, nodeImageUrl, nodeNavigateUrl, nodeTarget);
		return;
	}
	
	if (!node.hasError)
		node.populated = true;
	
	node.style.display = expand ? "block" : "none";
	
	var inputStates = tree.form [treeId + "_ExpandStates"];
	TreeView_SetNodeFlag (inputStates, nodeId, expand);
	
	if (tree.showImage) {
		var image = document.getElementById (spanId + "_img");
		if (expand) image.src = tree.collapseImage;
		else image.src = tree.expandImage;
		var alt;
		if (expand) alt = tree.collapseAlt;
		else alt = tree.expandAlt;

		if (alt.indexOf ("{0}") != -1) {
			var txt = document.getElementById (spanId + "_txt").innerText;
			alt = alt.replace ("{0}", txt);
		}
		image.alt = alt;
	}
}

function TreeView_PopulateNode (uid, treeId, nodeId, nodeValue, nodeImageUrl, nodeNavigateUrl, nodeTarget)
{
    var arg = nodeId + "|" + nodeValue + "|" + nodeImageUrl + "|" + nodeNavigateUrl + "|" + nodeTarget;
    window.WebForm_DoCallback (uid, arg, TreeView_PopulateCallback, treeId + " " + nodeId, TreeView_PopulateCallback, false);
}

function TreeView_SetNodeFlag (flagInput, nodeId, set)
{
	if (!set) flagInput.value = flagInput.value.replace ("|" + nodeId + "|", "|");
	else flagInput.value = flagInput.value + nodeId + "|";
}

function TreeView_PopulateCallback (data, ids)
{
	var idArray = ids.split (" ");
	var tree = getTree (idArray[0]);
	if (tree == null)
	    return;
	var spanId = idArray[0] + "_" + idArray[1];
	var node = document.getElementById (spanId);
	node.populated = true;
	if (data != "*") {
		node.innerHTML = data;
	    TreeView_ToggleExpand (idArray[0], idArray[1]);
	    TreeView_SetNodeFlag (tree.form [idArray[0] + "_PopulatedStates"], idArray[1], true);
	} else {
		if (tree.showImage && tree.noExpandImage != null) {
			var image = document.getElementById (spanId + "_img");
			image.src = tree.noExpandImage;
		}
	}
}

function getTree (treeId) { try { return eval (treeId + "_data"); } catch(e) { return null; } }
function getNodeLink (node) { return node.childNodes[node.childNodes.length - 1]; }

function TreeView_HoverNode (treeId, node)
{
	var tree = getTree (treeId);
	if (tree == null)
	    return;
	if (tree.hoverClass != null) {
	    if (node.normalClass == null)
		    node.normalClass = node.className;
	    node.className = node.normalClass + " " + tree.hoverClass;
    }
	if (tree.hoverLinkClass != null) {
	    var nodeLink = getNodeLink(node);
	    if (nodeLink.normalClass == null)
		    nodeLink.normalClass = nodeLink.className;
	    nodeLink.className = nodeLink.normalClass + " " + tree.hoverLinkClass;
	}
}

function TreeView_UnhoverNode (node) {
	if (node != null && node.normalClass != null)
		node.className = node.normalClass;
	var nodeLink = getNodeLink(node);
	if (nodeLink != null && nodeLink.normalClass != null)
		nodeLink.className = nodeLink.normalClass;
}
