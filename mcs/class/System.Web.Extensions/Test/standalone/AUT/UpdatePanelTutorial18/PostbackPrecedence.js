Sys.Application.add_load(ApplicationLoadHandler)
function ApplicationLoadHandler(sender, args)
{
    if (!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack())
    {
      Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(InitializeRequest);
    }
}

var divElem = 'AlertDiv';
var messageElem = 'AlertMessage';
var exclusivePostBackElement = 'Button1';
var lastPostBackElement;
function InitializeRequest(sender, args)
{ 
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    if (prm.get_isInAsyncPostBack() && 
        args.get_postBackElement().id === exclusivePostBackElement) 
    {
        if (lastPostBackElement === exclusivePostBackElement)
        {
          args.set_cancel(true);
          ActivateAlertDiv('visible', 'A previous postback is still executing. The new postback has been canceled.');
          setTimeout("ActivateAlertDiv('hidden','')", 1500);
        }
        else if (lastPostBackElement !== exclusivePostBackElement)
        {
          prm.abortPostBack();
        }
    }
    else if (prm.get_isInAsyncPostBack() && 
             args.get_postBackElement().id !== exclusivePostBackElement)
    {
        if (lastPostBackElement === exclusivePostBackElement)
        {
            args.set_cancel(true);
            ActivateAlertDiv('visible', 'A previous postback is still executing. The new postback has been canceled.');
            setTimeout("ActivateAlertDiv('hidden','')", 1500);
        }       
    }
    lastPostBackElement = args.get_postBackElement().id;      
}

function ActivateAlertDiv(visString, msg)
{
     var adiv = $get(divElem);
     var aspan = $get(messageElem);
     adiv.style.visibility = visString;
     aspan.innerHTML = msg;
}
if(typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
