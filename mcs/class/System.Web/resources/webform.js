
function WebForm_DoPostback (ctrl, par, url, apb, pval, tf, csubm, vg)
{
	if (pval && typeof(Page_ClientValidate) == "function" && !Page_ClientValidate())
		return;

	if (url != null)
		theForm.action = url;
		
	if (!csubm)
		__doPostBack (ctrl, par);
}

