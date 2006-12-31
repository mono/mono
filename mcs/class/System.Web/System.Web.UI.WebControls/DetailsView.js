function DetailsView_ClientEvent (ctrlId, evnt)
{
	var gridData = eval (ctrlId + "_data");
	var clientData = gridData.pageIndex + '|' + evnt;
	WebForm_DoCallback (gridData.uid, clientData, DetailsView_ClientRender, ctrlId, DetailsView_ClientRender_Error);
}

function DetailsView_ClientRender (data, ctx)
{
	var gridData = eval (ctx + "_data");
	var grid = document.getElementById (ctx + "_div");
	var i = data.indexOf ("|");
	gridData.pageIndex = parseInt (data.substring (0, i));
	grid.innerHTML = data.substr (i+1);
	
	var page = document.getElementById(ctx + "_Page");
	page.value = gridData.pageIndex;
}

function DetailsView_ClientRender_Error (data, ctx)
{
}
