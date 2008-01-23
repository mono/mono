<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">

    <style type="text/css">
        button {border: solid 1px black}
        #HoverLabel {color: blue}
    </style>
    <title>Application Demo</title>
</head>
<body style="background-color:Aqua; font-size:medium">
<form id="form1" runat="server">
    <asp:ScriptManager runat="server" ID="ScriptManager01">
        <scripts>
           <asp:ScriptReference Path="HoverButton.js" />
           <asp:ScriptReference Path="HighVis.js" />
        </scripts>
    </asp:ScriptManager>
    <p><strong>Sys.Application Sample</strong></p>
    <script type="text/javascript">
// Attach a handler to the init event.
Sys.Application.add_init(applicationInitHandler);

function applicationInitHandler() {
    // Add two custom controls to the application.
    $create(Demo.HoverButton, {text: 'A HoverButton Control'}, 
        {click: start, hover: doSomethingOnHover, 
        unhover: doSomethingOnUnHover},
        null, $get('Button1'));
    $create(Demo.HighVis, null, null, null, $get('Button2'));
}

/*        
// Attach a handler to the load event.
Sys.Application.add_load(applicationLoadHandler);

function applicationLoadHandler() {
    // Redirect to alternate page if not business hours.
    var d = new Date();
    if (!(8 < d.getHours() < 17)) {
        window.location = "AfterHours.aspx";
    }
}
// Attach a handler to the unLoad event.
Sys.Application.add_unload(applicationUnloadHandler);

function applicationUnloadHandler() {
    // Redirect user to a survey form.
    window.open("SurveyForm.aspx");
}

function pageLoad() {
    // Make sure the scripts read in the proper order.
    Sys.Application.queueScriptReference("StopWatch.js");
    Sys.Application.queueScriptReference("StatusBar.js");
    Sys.Application.queueScriptReference("DownloadTracker.js");
    
    // Add custom controls to the application.
    $create(Demo.DownloadTracker, null, null, null, $get('div1'));
}
*/      

function doSomethingOnHover() {
    var hoverMessage = "The mouse is over the button.";
    $get('HoverLabel').innerHTML = hoverMessage;
    $get('hoverTrace').value = hoverMessage;
}

function doSomethingOnUnHover() {
   $get('HoverLabel').innerHTML = "";
}

function start() {
   alert("The start function handled the HoverButton click event.");
}

function tick() {
    var d = new Date();
    while($get('DataBox')) {
        window.setInterval(checkTime(d), 100);
    }
}

function checkTime(d) {
    if (!(8 < d.getHours() < 17)) {
        Sys.Application.removeComponent(DataBox);
    }
}
function checkComponent() {
    if (!($find('MyComponent', div1))) {
        div1.innerHTML = 'MyComponent is not available.';
    }
}
function listComponents() {
    var c = Sys.Application.getComponents();
    var s = "";
    for (var i=0; i<c.length; i++) {
        var id = c[i].get_id();
        var type = Object.getType(c[i]).getName();
        s += 'Item ' + i + ': id=' + id + ', type=' + type + '.<br />';
    }
    //div1.innerHTML = s;
    alert(s);
}
    </script>

    <button type="button" id="Button1"> </button>
    <br />
    <div id="HoverLabel">
    </div>
    <br />
    <input id="hoverTrace">
    <br />
    <button type="button" id="Button2"> </button> 
    <br /> 
    This text will change appearance when you click the button above.
    <br />
    <button type="button" id="Button3" onclick="listComponents()"> </button> 
    <br /> 
    <div id="DataBox">This is my DataBox</div>
    <div id="div1"></div>
    <div id="div2"></div>
    <div id="div3">
    </div>
    <input id="backgroundColor" />
    <input id="fontSize" />
</form>

</body>
</html>
