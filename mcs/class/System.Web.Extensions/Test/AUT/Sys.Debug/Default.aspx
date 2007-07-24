<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>

</head>
<body>
<form id="form1" name="form1" runat="server">
    <h2>Sys.Debug Methods Test Page</h2>
    <asp:ScriptManager ID="ScriptManager1" 
        runat="server" />
    <p><b>Use these buttons to demonstrate the assert() and fail() 
    methods:</b><br />
    <input id="btnAssert" type="button" value="Assert" 
        style="width: 100px" 
        onclick="return btnAssert_onclick()" /> &nbsp
    <input id="btnFail" type="button" value="Fail" 
        style="width: 100px" onclick="return btnFail_onclick()" />
    </p><hr />
    <b>Use the textbox and buttons below to demonstrate tracing.</b>
    <br />
    <p>Enter your name here:<br />
    <input id="text1" maxlength="50" type="text" />
    <br />
    <br />
    <input id="btnTrace" type="button" value="Trace" 
        style="width: 100px" onclick="return btnTrace_onclick()" /><br />
    <input id="btnDump" type="button" value="TraceDump" 
        style="width: 100px" onclick="return btnDump_onclick()" /><br />
    <input id="btnClear" type="button" value="ClearTrace" 
        style="width: 100px" onclick="return btnClear_onclick()" /><br />
    <br /></p>
    View output in the TraceConsole textarea below.
    <br />
    <textarea id='TraceConsole' rows="10" cols="50" 
        title="TraceConsole"></textarea>
</form>
<script language="javascript" type="text/javascript">
function btnAssert_onclick() {
    var n;
    // Insert code intended to set n to a positive integer.
    if (false) n = 3;
    // Assert if n is not greater than 0.
    Sys.Debug.assert(n > 0, "n must be set to a positive integer.");
}

function btnFail_onclick() {
    var n;
    // Insert code intended to set n to a numeric value.
    if (false) n = 3;
    // Fail if n is not numeric.
    if (isNaN(n)) Sys.Debug.fail("The value of n must be a number.");
}

function btnTrace_onclick() {
    v = theForm.text1.value;
    Sys.Debug.trace("Name set to " + "\"" + v + "\".");
    alert("Hello " + v + ".");
}

function btnDump_onclick() {
    Sys.Debug.traceDump(theForm.text1, "Name textbox");
    alert("Hello " + theForm.text1.value + ".");
}

function btnClear_onclick() {
    Sys.Debug.clearTrace()
    alert("Trace console cleared.");
}
</script>
</body>
</html>
