<%@ Page Language="C#" %>

<%@ Implements Interface="System.Web.UI.ICallbackEventHandler" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	string _callbackResult;
	public string GetCallbackResult () {
		return _callbackResult;
	}
	public void RaiseCallbackEvent (string eventArgs) {
		if (eventArgs == "CCC")
			throw new Exception ("CCC");
		_callbackResult = eventArgs;
	}
	string _callbackEventRef_1;
	string _callbackEventRef_2;
	string _callbackEventRef_3;
	protected override void OnInit (EventArgs e) {
		base.OnInit (e);
		_callbackEventRef_1 = ClientScript.GetCallbackEventReference (this, "'AAA'", "clientCallback", null, "errorCallback", true);
		_callbackEventRef_2 = ClientScript.GetCallbackEventReference (this, "'BBB'", "clientCallback", null, "errorCallback", true);
		_callbackEventRef_3 = ClientScript.GetCallbackEventReference (this, "'CCC'", "clientCallback", null, "errorCallback", true);
	}

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
</head>
<body>

	<script type="text/javascript">

		function clientCallback(result, ctx)
		{
			alert(result);
		}
		function errorCallback(result, ctx)
		{
			alert("error");
			alert(result);
		}
	</script>

	<form id="form1" runat="server">
		<div>
			<a href="javascript:<%=_callbackEventRef_1 %>" id="A1">_callbackEventRef_1</a><br />
			<a href="javascript:<%=_callbackEventRef_2 %>" id="A2">_callbackEventRef_2</a><br />
			<a href="javascript:<%=_callbackEventRef_3 %>" id="A3">_callbackEventRef_3</a><br />
		</div>
	</form>
</body>
</html>
