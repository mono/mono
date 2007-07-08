// Hook up Application event handlers.
var app = Sys.Application;
app.add_load(ApplicationLoad);
app.add_init(ApplicationInit);
app.add_disposing(ApplicationDisposing);
app.add_unload(ApplicationUnload);


// Application event handlers for component developers.
function ApplicationInit(sender) {
  var prm = Sys.WebForms.PageRequestManager.getInstance();
  if (!prm.get_isInAsyncPostBack())
  {
      prm.add_initializeRequest(InitializeRequest);
      prm.add_beginRequest(BeginRequest);
      prm.add_pageLoading(PageLoading);
      prm.add_pageLoaded(PageLoaded);
      prm.add_endRequest(EndRequest);
  }
  $get('ClientEvents').innerHTML += "APP:: Application init. <br/>";
}
function ApplicationLoad(sender, args) {
  $get('ClientEvents').innerHTML += "APP:: Application load. ";
  $get('ClientEvents').innerHTML += "(isPartialLoad = " + args.get_isPartialLoad() + ")<br/>";
}
function ApplicationUnload(sender) {
  alert('APP:: Application unload.');
}
function ApplicationDisposing(sender) {
  $get('ClientEvents').innerHTML += "APP:: Application disposing. <br/>";
  
}
// Application event handlers for page developers.
function pageLoad() {
  $get('ClientEvents').innerHTML += "PAGE:: Load.<br/>";
}

function pageUnload() {
  alert('Page:: Page unload.');
}

// PageRequestManager event handlers.
function InitializeRequest(sender, args) {
  $get('ClientEvents').innerHTML += "<hr/>";
  $get('ClientEvents').innerHTML += "PRM:: Initializing async request.<br/>";  
}
function BeginRequest(sender, args) {
  $get('ClientEvents').innerHTML += "PRM:: Begin processing async request.<br/>";
}
function PageLoading(sender, args) {
  $get('ClientEvents').innerHTML += "PRM:: Loading results of async request.<br/>";
  var updatedPanels = printArray("PanelsUpdating", args.get_panelsUpdating());
  var deletedPanels = printArray("PanelsDeleting", args.get_panelsDeleting());
  
  var message = "-->" + updatedPanels + "<br/>-->" + deletedPanels + "<br/>";
  
  document.getElementById("ClientEvents").innerHTML += message;
}
function PageLoaded(sender, args) {
  $get('ClientEvents').innerHTML += "PRM:: Finished loading results of async request.<br/>";
  var updatedPanels = printArray("PanelsUpdated", args.get_panelsUpdated());
  var createdPanels = printArray("PaneslCreated", args.get_panelsCreated());
    
  var message = "-->" + updatedPanels + "<br/>-->" + createdPanels + "<br/>";
        
  document.getElementById("ClientEvents").innerHTML += message;
}
function EndRequest(sender, args) {
  $get('ClientEvents').innerHTML += "PRM:: End of async request.<br/>";
}

// Helper functions.
function Clear()
{
  $get('ClientEvents').innerHTML = "";
}
function printArray(name, arr)
{
    var panels = name + '=' + arr.length;
    if(arr.length > 0)
    {
        panels += "(";
        for(var i = 0; i < arr.length; i++)
        {
            panels += arr[i].id + ',';
        }
        panels = panels.substring(0, panels.length - 1);
        panels += ")";
    }
    return panels;
}
