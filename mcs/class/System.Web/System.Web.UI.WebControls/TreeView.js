
function TreeView_ToggleExpand (treeId, nodeId) {
	var node = document.getElementById (treeId + "_" + nodeId);
	var expand = node.style.display == "none";
	node.style.display = expand ? "block" : "none";
	
	var inputStates = document.forms[0][treeId + "_ExpandStates"];
	var states = inputStates.value;
	var i = states.indexOf ("|" + nodeId + "|");
	if (node.style.display == "none") states = states.replace ("|" + nodeId + "|", "|");
	else states = states + nodeId + "|";
	inputStates.value = states;
	
	var tree = eval (treeId + "_data");
	if (tree.showImage) {
		var image = document.getElementById (treeId + "_img_" + nodeId);
		if (tree.defaultImages) {
			if (expand)
				image.src = image.src.replace ("plus","minus");
			else
				image.src = image.src.replace ("minus","plus");
		} else {
			if (expand)
				image.src = tree.collapseImage;
			else
				image.src = tree.expandImage;
		}
	}
}

