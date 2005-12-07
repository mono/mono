<%@ Page Language="C#" %>
<html>
<head>
 </head>
<script language="javascript" type="text/javascript">
function get_elem(id) {
	return (document.getElementById) ? document.getElementById (id) :
					((document.all) ? document.all [id] : null);
}

function check_src () {
	var elem = get_elem ("imagen");
	if (elem) {
		var out_come = get_elem ("outcome");
		if (elem.src.indexOf("~/img.jpg") != -1) {
			out_come.innerHTML = "Test failed: " + elem.src;
			out_come.className = "failed";
		} else {
			out_come.innerHTML = "Test passed: " + elem.src;
			out_come.className = "passed";
		}
	}
}
</script>
<style type="text/css" media="screen">
<!--
  .passed { background-color: green; color: white;}
  .failed { background-color: red; color: white;}
-->
</style>
 <body onload="check_src()">
<img id ="imagen" src="~/img.jpg" Alt="Image 1" runat="server" />
<div id="outcome" class="">Default text. Should not be seen.</div>
 </body>
 </html>

