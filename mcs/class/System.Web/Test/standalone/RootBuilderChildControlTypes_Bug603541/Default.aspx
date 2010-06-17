<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
	<a runat="server" id="anchor" href="/">Anchor</a><br />
	<button runat="server" id="button" value="Button" /><br />
	<img runat="server" id="img" src="http://google.com/favico.png" /><br />
	<link runat="server" id="link" rel="test" /><br />
	<meta runat="server" id="meta" name="test" /><br />
	<select runat="server" id="testSelect"></select><br />
	<table runat="server" id="table">
		<tr runat="server" id="tr">
			<th runat="server" id="th"></th>
			<td runat="server" id="td"></td>
		</tr>
	</table><br />
	<textarea runat="server" id="textarea"></textarea>
	
	<input runat="server" type="button" id="inputButton" /><br />
	<input runat="server" type="submit" id="inputSubmit" /><br />
	<input runat="server" type="reset" id="inputReset" /><br />
	<input runat="server" type="checkbox" id="inputCheckbox" /><br />
	<input runat="server" type="file" id="inputFile" /><br />
	<input runat="server" type="hidden" id="inputHidden" /><br />
	<input runat="server" type="image" id="inputImage" /><br />
	<input runat="server" type="radio" id="inputRadio" /><br />
	<input runat="server" type="text" id="inputText" /><br />
	<input runat="server" type="password" id="inputPassword" /><br />

	<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><pre runat="server" id="log" /><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </div>
    </form>
</body>
</html>
