function TreeView_HoverNode(data, node) {
    // Don't try to continue if the startup scripts have not run to conclusion or if the disposal
    // script has already run.
    if (!data) {
        return;
    }

     // Merge the hover style class onto the TD
    node.hoverClass = data.hoverClass;
    WebForm_AppendToClassName(node, data.hoverClass);

     // Merge the hyperlink hover style class
    if (__nonMSDOMBrowser) {
        node = node.childNodes[node.childNodes.length - 1];
    }
    else {
        node = node.children[node.children.length - 1];
    }
    node.hoverHyperLinkClass = data.hoverHyperLinkClass;
    WebForm_AppendToClassName(node, data.hoverHyperLinkClass);
}

 function TreeView_GetNodeText(node) {
    var trNode = WebForm_GetParentByTagName(node, "TR");
    //document.body.insertAdjacentHTML("beforeEnd", "td count=" + trNode.childNodes.length + "<BR/>");
    //document.body.insertAdjacentHTML("beforeEnd", "last td child[0] nodetype=" + trNode.childNodes[trNode.childNodes.length - 1].childNodes[0].nodeType + "<BR/>");
    //for(var i = 0; i < trNode.childNodes.length; i++) document.body.insertAdjacentHTML("beforeEnd", "td[" + i + "]:" + trNode.childNodes[i].childNodes[0].nodeValue + "<BR/>");
    var outerNodes;
    if (trNode.childNodes[trNode.childNodes.length - 1].getElementsByTagName) {
        outerNodes = trNode.childNodes[trNode.childNodes.length - 1].getElementsByTagName("A");
        if (!outerNodes || outerNodes.length == 0) {
            outerNodes = trNode.childNodes[trNode.childNodes.length - 1].getElementsByTagName("SPAN");
        }
    }
    //document.body.insertAdjacentHTML("beforeEnd", "outerNodes.length=" + outerNodes.length + "<BR/>");
    var textNode = (outerNodes && outerNodes.length > 0) ?
        outerNodes[0].childNodes[0] :
        trNode.childNodes[trNode.childNodes.length - 1].childNodes[0];
    return (textNode && textNode.nodeValue) ? textNode.nodeValue : "";
}

 function TreeView_PopulateNode(data, index, node, selectNode, selectImageNode, lineType, text, path, databound, datapath, parentIsLast) {
    // Don't try to continue if the startup scripts have not run to conclusion or if the disposal
    // script has already run.
    if (!data) {
        return;
    }

     var context = new Object();
    context.data = data;
    context.node = node;
    context.selectNode = selectNode;
    context.selectImageNode = selectImageNode;
    context.lineType = lineType;
    context.index = index;
    context.isChecked = "f";
    // Find the parent tr for the node, then the checkbox in it
    var tr = WebForm_GetParentByTagName(node, "TR");
    if (tr) {
        var checkbox = tr.getElementsByTagName("INPUT");
        if (checkbox && (checkbox.length > 0)) {
            for (var i = 0; i < checkbox.length; i++) {
                if (checkbox[i].type.toLowerCase() == "checkbox") {
                    if (checkbox[i].checked) {
                        context.isChecked = "t";
                    }
                    break;
                }
            }
        }
    }
    var param = index + "|" + data.lastIndex + "|" + databound + context.isChecked + parentIsLast + "|" +
        text.length + "|" + text + datapath.length + "|" + datapath + path;
    TreeView_PopulateNodeDoCallBack(context, param);
}

 function TreeView_ProcessNodeData(result, context) {
    var treeNode = context.node;
    if (result.length > 0) {
        //document.body.insertAdjacentHTML("beforeEnd", result + "<BR/>");
        var ci =  result.indexOf("|", 0);
        context.data.lastIndex = result.substring(0, ci);

         ci = result.indexOf("|", ci + 1);
        var newExpandState = result.substring(context.data.lastIndex.length + 1, ci);
        context.data.expandState.value += newExpandState;

         var chunk = result.substr(ci + 1);
        var newChildren, table;
        if (__nonMSDOMBrowser) {
            var newDiv = document.createElement("div");
            newDiv.innerHTML = chunk;
            table = WebForm_GetParentByTagName(treeNode, "TABLE");
            newChildren = null;
            if ((typeof(table.nextSibling) == "undefined") || (table.nextSibling == null)) {
                table.parentNode.insertBefore(newDiv.firstChild, table.nextSibling);
                newChildren = table.previousSibling;
            }
            else {
                table = table.nextSibling;
                table.parentNode.insertBefore(newDiv.firstChild, table);
                newChildren = table.previousSibling;
            }

             newChildren = document.getElementById(treeNode.id + "Nodes");
        }
        else {
            table = WebForm_GetParentByTagName(treeNode, "TABLE");
            table.insertAdjacentHTML("afterEnd", chunk);

             newChildren = document.all[treeNode.id + "Nodes"];
        }
        if ((typeof(newChildren) != "undefined") && (newChildren != null)) {
            TreeView_ToggleNode(context.data, context.index, treeNode, context.lineType, newChildren);
            treeNode.href = document.getElementById ?
                "javascript:TreeView_ToggleNode(" + context.data.name + "," + context.index + ",document.getElementById('" + treeNode.id + "'),'" + context.lineType + "',document.getElementById('" + newChildren.id + "'))" :
                "javascript:TreeView_ToggleNode(" + context.data.name + "," + context.index + "," + treeNode.id + ",'" + context.lineType + "'," + newChildren.id + ")";
            // Also change the javascript on the node's text if it was an expander
            if ((typeof(context.selectNode) != "undefined") && (context.selectNode != null) && context.selectNode.href &&
                (context.selectNode.href.indexOf("javascript:TreeView_PopulateNode", 0) == 0)) {
                context.selectNode.href = treeNode.href;
            }

             // Also change the javascript on the node's image if it was an expander
            if ((typeof(context.selectImageNode) != "undefined") && (context.selectImageNode != null) && context.selectNode.href &&
                (context.selectImageNode.href.indexOf("javascript:TreeView_PopulateNode", 0) == 0)) {
                context.selectImageNode.href = treeNode.href;
            }
        }

         context.data.populateLog.value += context.index + ",";
    }
    else {
        var img = treeNode.childNodes ? treeNode.childNodes[0] : treeNode.children[0];
        // Only try to change the image if there was an image (if ShowExpandCollapse is false, there isn't one)
        if ((typeof(img) != "undefined") && (img != null)) {
            var lineType = context.lineType;
            if (lineType == "l") {
                img.src = context.data.images[13];
            }
            else if (lineType == "t") {
                img.src = context.data.images[10];
            }
            else if (lineType == "-") {
                img.src = context.data.images[16];
            }
            else {
                img.src = context.data.images[3];
            }
            var pe;
            if (__nonMSDOMBrowser) {
                pe = treeNode.parentNode;
                pe.insertBefore(img, treeNode);
                pe.removeChild(treeNode);
            }
            else {
                pe = treeNode.parentElement;
                treeNode.style.visibility="hidden";
                treeNode.style.display="none";
                pe.insertAdjacentElement("afterBegin", img);
            }
        }
    }
}

 function TreeView_SelectNode(data, node, nodeId) {
    // Don't try to continue if the startup scripts have not run to conclusion or if the disposal
    // script has already run.
    if (!data) {
        return;
    }

     // If there was a selected node style, apply it (and remove it from the previously selected node)
    if ((typeof(data.selectedClass) != "undefined") && (data.selectedClass != null)) {
        // Find the old selected node and remove it's selected style
        var id = data.selectedNodeID.value;

         if (id.length > 0) {
            var selectedNode = document.getElementById(id);

             if ((typeof(selectedNode) != "undefined") && (selectedNode != null)) {
                // Remove the selected node style from the hyperlink
                WebForm_RemoveClassName(selectedNode, data.selectedHyperLinkClass);

                 // Remove the selected node style from the TD
                selectedNode = WebForm_GetParentByTagName(selectedNode, "TD");
                WebForm_RemoveClassName(selectedNode, data.selectedClass);
            }
        }

         // Add the selected node style to the hyperlink
        WebForm_AppendToClassName(node, data.selectedHyperLinkClass);

         // Remove the selected node style to the TD
        node = WebForm_GetParentByTagName(node, "TD");
        WebForm_AppendToClassName(node, data.selectedClass)
    }

     data.selectedNodeID.value = nodeId;
}

 function TreeView_ToggleNode(data, index, node, lineType, children) {
    // Don't try to continue if the startup scripts have not run to conclusion or if the disposal
    // script has already run.
    if (!data) {
        return;
    }

     var img = node.childNodes[0];
    var newExpandState;
    try {
        if (children.style.display == "none") {
            children.style.display = "block";

             newExpandState = "e";

             // Only try to change the image if there was an image (if ShowExpandCollapse is false, there isn't one)
            if ((typeof(img) != "undefined") && (img != null)) {
                if (lineType == "l") {
                    img.src = data.images[15];
                }
                else if (lineType == "t") {
                    img.src = data.images[12];
                }
                else if (lineType == "-") {
                    img.src = data.images[18];
                }
                else {
                    img.src = data.images[5];
                }
                // Change tooltip
                img.alt = data.collapseToolTip.replace(/\{0\}/, TreeView_GetNodeText(node));
            }
        }
        else {
            children.style.display = "none";

             newExpandState = "c";

             // Only try to change the image if there was an image (if ShowExpandCollapse is false, there isn't one)
            if ((typeof(img) != "undefined") && (img != null)) {
                if (lineType == "l") {
                    img.src = data.images[14];
                }
                else if (lineType == "t") {
                    img.src = data.images[11];
                }
                else if (lineType == "-") {
                    img.src = data.images[17];
                }
                else {
                    img.src = data.images[4];
                }
                // Change tooltip
                img.alt = data.expandToolTip.replace(/\{0\}/, TreeView_GetNodeText(node));
            }
        }
    }
    catch(e) {}
    data.expandState.value =  data.expandState.value.substring(0, index) + newExpandState + data.expandState.value.slice(index + 1);
}

 function TreeView_UnhoverNode(node) {
    // Don't try to continue if the startup scripts have not run to conclusion or if the disposal
    // script has already run.
    if (!node.hoverClass) {
        return;
    }

     // Remove the hover style class onto the TD
    WebForm_RemoveClassName(node, node.hoverClass);

     // Remove the hyperlink hover style class
    if (__nonMSDOMBrowser) {
        node = node.childNodes[node.childNodes.length - 1];
    }
    else {
        node = node.children[node.children.length - 1];
    }
    WebForm_RemoveClassName(node, node.hoverHyperLinkClass);
}