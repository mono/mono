<%@ Page Language="C#" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="_default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
	Text Box: <asp:TextBox runat="server" ID="textBox1" /><br />
	<asp:Repeater runat="server" ID="repeater1" OnItemDataBound="OnItemDataBound_Repeater1">
		<ItemTemplate>
			<asp:Label runat="server" ID="label1" Text="<%# Container.DataItem %>" />
			<asp:Repeater runat="server" ID="innerRepeater1" OnItemDataBound="OnItemDataBound_InnerRepeater1">
				<ItemTemplate>
					<blockquote><asp:Label runat="server" ID="innerLabel1" Text="<%# Container.DataItem %>" /></blockquote>
				</ItemTemplate>
			</asp:Repeater>
		</ItemTemplate>

		<SeparatorTemplate>
			<hr />
		</SeparatorTemplate>
	</asp:Repeater>
    </div>
    <div>Log:</div>
    <%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><pre runat="server" id="log"></pre><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
    </form>
</body>
</html>
