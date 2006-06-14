<%@ Page CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>

<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" >
<head id="Head1" runat="server">
	<title></title>
	<style type="text/css">
	    .menua {	
	        position: absolute;
	        right: 37px;
	        top: 17px;
	        text-transform: uppercase;
	        font-size: 10px;
        }
	</style>
</head>
<body>

	<form id="form1" runat="server">

			<asp:menu id="menua" runat="server" cssclass="menua" >
                <Items>
                    <asp:MenuItem Text="This must appear" Value="Item One">
			<asp:MenuItem Text="here" Value="Item one a"/>
			<asp:MenuItem Text="and here" Value="Item one b"/>
                    </asp:MenuItem>
                    <asp:MenuItem Text="On the right side" Value="Item Two"/>
                </Items>
            </asp:menu>

	</form>
	
</body>
</html>
