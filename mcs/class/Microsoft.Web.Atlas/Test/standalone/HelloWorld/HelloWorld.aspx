<%@ Page Language="C#" %>

<html>
<head>
<script type="text/javascript" src="../../ScriptLibrary/AtlasCore.js"></script>
<script type="text/javascript" src="/HelloWorld/HelloWorldService.asmx/js"></script>

<script type="text/javascript">
function onMethodComplete (result)
{
  document.getElementById ('label').innerHTML = result;
}

function button_click ()
{
   Samples.AspNet.HelloWorldService.HelloWorld ("hi there", onMethodComplete);
}
</script>

</head>

<body>
  <span id='label'>foo</span>
  <button id='button' onclick='button_click()'>click me</button>
</body>
</html>