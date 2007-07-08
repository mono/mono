var divElem = 'AlertDiv';
var messageElem = 'AlertMessage';
var bodyTag = 'bodytag';
function pageLoad() {
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
}
function ToggleAlertDiv(visString)
{
     if (visString == 'hidden')
     {
         $get(bodyTag).style.backgroundColor = 'white';                         
     }
     else
     {
         $get(bodyTag).style.backgroundColor = 'gray';                         
        
     }
     var adiv = $get(divElem);
     adiv.style.visibility = visString;
     
}
function ClearErrorState() {
     $get(messageElem).innerHTML = '';
     ToggleAlertDiv('hidden');                     
}
function EndRequestHandler(sender, args)
{
   if (args.get_error() != undefined)
   {
       var errorMessage;
       if (args.get_response().get_statusCode() == '200')
       {
           errorMessage = args.get_error().message;
       }
       else
       {
           // Error occurred somewhere other than the server page.
           errorMessage = 'An unspecified error occurred. ';
       }
       args.set_errorHandled(true);
       ToggleAlertDiv('visible');
       $get(messageElem).innerHTML = errorMessage;
   }
}
if(typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
