function GridView_ClientEvent (ctrlId, evnt)
{
	var gridData = eval (ctrlId + "_data");
	var clientData = gridData.pageIndex + '|' + escape (gridData.sortExp) + '|' + evnt;
	WebForm_DoCallback (gridData.uid, clientData, GridView_ClientRender, ctrlId, GridView_ClientRender_Error);
}

function GridView_ClientRender (data, ctx)
{
	var gridData = eval (ctx + "_data");
	var grid = document.getElementById (ctx);
	var i = data.indexOf ("|");
	var j = data.indexOf ("|", i+1);
	gridData.pageIndex = parseInt (data.substring (0, i));
	gridData.sortExp = unescape (data.substring (i+1, j));
	grid.innerHTML = data.substr (j+1);
}

function GridView_ClientRender_Error (data, ctx)
{
}
