function DetailsView_ClientEvent (ctrlId, evnt)
{
	var gridData = getDetails (ctrlId);
	if (!gridData)
	    return null;
	var clientData = gridData.pageIndex + '|' + evnt;
	WebForm_DoCallback (gridData.uid, clientData, DetailsView_ClientRender, ctrlId, DetailsView_ClientRender_Error, false, gridData.form);
}

function DetailsView_ClientRender (data, ctx)
{
	var gridData = getDetails (ctx);
	if (!gridData)
	    return;
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

function getDetails (detailsId) { try { return eval (detailsId + "_data"); } catch(e) { return null; } }

