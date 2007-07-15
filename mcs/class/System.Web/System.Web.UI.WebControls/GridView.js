function GridView_ClientEvent (ctrlId, evnt)
{
	var gridData = getGrid (ctrlId);
	if (!gridData)
	    return null;
	var clientData = gridData.pageIndex + '|' + escape (gridData.sortExp) + '|' + gridData.sortDir + '|' + evnt;
	WebForm_DoCallback (gridData.uid, clientData, GridView_ClientRender, ctrlId, GridView_ClientRender_Error, false, gridData.form);
}

function GridView_ClientRender (data, ctx)
{
	var gridData = getGrid (ctx);
	if (!gridData)
	    return;
	var grid = document.getElementById (ctx + "_div");
	var i = data.indexOf ("|");
	var j = data.indexOf ("|", i+1);
	var k = data.indexOf ("|", j+1);
	gridData.pageIndex = parseInt (data.substring (0, i));
	gridData.sortExp = unescape (data.substring (i+1, j));
	gridData.sortDir = parseInt (data.substring (j+1, k));
	grid.innerHTML = data.substr (k+1);
	
	var page = document.getElementById(ctx + "_Page");
	page.value = gridData.pageIndex;
	var sortExp = document.getElementById(ctx + "_SortExpression");
	sortExp.value = gridData.sortExp;
	var sortDir = document.getElementById(ctx + "_SortDirection");
	sortDir.value = gridData.sortDir;
}

function GridView_ClientRender_Error (data, ctx)
{
}

function getGrid (gridId) { try { return eval (gridId + "_data"); } catch(e) { return null; } }

