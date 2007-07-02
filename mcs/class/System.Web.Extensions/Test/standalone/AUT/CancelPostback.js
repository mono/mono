var divElem = 'AlertDiv';
var messageElem = 'AlertMessage';

Sys.Application.add_load(ApplicationLoadHandler)
function ApplicationLoadHandler(sender, args)
{
     Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(CheckStatus);
}
function CheckStatus(sender, args)
{
  var prm = Sys.WebForms.PageRequestManager.getInstance();
  if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'CancelRefresh') {
     prm.abortPostBack();
  }
  else if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
     args.set_cancel(true);
     ActivateAlertDiv('visible', 'Still working on previous request.');
 }
  else if (!prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
     ActivateAlertDiv('visible', 'Retrieving headlines.');
  }
}
function ActivateAlertDiv(visString, msg)
{
     var adiv = $get(divElem);
     var aspan = $get(messageElem);
     adiv.style.visibility = visString;
     aspan.innerHTML = msg;
}
if(typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();