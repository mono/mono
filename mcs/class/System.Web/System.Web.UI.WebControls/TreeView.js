
function TreeView_ToggleExpand (treeId, nodeId) {
	var tree = getTree (treeId);
	var spanId = treeId + "_" + nodeId;
	var node = document.getElementById (spanId);
	var expand = node.style.display == "none";
	
	if (tree.populateFromClient && expand && node.populated != true && (node.innerHTML.length == 0 || node.hasError)) {
		WebForm_DoCallback (treeId, nodeId, TreeView_PopulateCallback, treeId + " " + nodeId, TreeView_ErrorCallback)
		return;
	}
	
	if (!node.hasError)
		node.populated = true;
	
	node.style.display = expand ? "block" : "none";
	
	var inputStates = document.forms[0][treeId + "_ExpandStates"];
	TreeView_SetNodeFlag (inputStates, nodeId, expand);
	
	if (tree.showImage) {
		var image = document.getElementById (spanId + "_img");
		if (tree.defaultImages) {
			if (expand) image.src = image.src.replace ("plus","minus");
			else image.src = image.src.replace ("minus","plus");
		} else {
			if (expand) image.src = tree.collapseImage;
			else image.src = tree.expandImage;
		}
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

function TreeView_SetNodeFlag (flagInput, nodeId, set)
{
	if (!set) flagInput.value = flagInput.value.replace ("|" + nodeId + "|", "|");
	else flagInput.value = flagInput.value + nodeId + "|";
}

function TreeView_PopulateCallback (data, ids)
{
	var idArray = ids.split (" ");
	var tree = getTree (idArray[0]);
	var spanId = idArray[0] + "_" + idArray[1];
	var node = document.getElementById (spanId);
	node.populated = true;
	if (data != "*") {
		node.innerHTML = data;
		TreeView_ToggleExpand (idArray[0], idArray[1]);
	} else {
		if (tree.showImage && tree.noExpandImage != null) {
			var image = document.getElementById (spanId + "_img");
			image.src = tree.noExpandImage;
		}
	}
}

function TreeView_ErrorCallback (data, ids)
{
	var idArray = ids.split (" ");
	var node = document.getElementById (idArray[0] + "_" + idArray[1]);
	node.innerHTML = data;
	node.populated = true;
	TreeView_ToggleExpand (idArray[0], idArray[1]);
	node.populated = false;
	node.hasError = true;
}

function getTree (treeId) { return eval (treeId + "_data"); }
