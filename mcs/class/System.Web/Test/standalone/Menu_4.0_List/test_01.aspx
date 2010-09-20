<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
	
	    <!-- START --><%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><asp:Menu ID="MyMenu1" runat="server" RenderingMode="List" DataSourceID="MenuDataSource" StaticDisplayLevels="1">
	    <DataBindings>
	    <asp:MenuItemBinding DataMember="MenuRoot" Depth="0" TextField="title" NavigateUrlField="url" ToolTipField="description" />
	    <asp:MenuItemBinding DataMember="MenuNode" Depth="1" TextField="title" NavigateUrlField="url" ToolTipField="description" />
	    <asp:MenuItemBinding DataMember="MenuNode" Depth="2" TextField="title" NavigateUrlField="url" ToolTipField="description" />
	    <asp:MenuItemBinding DataMember="MenuNode" Depth="3" TextField="title" NavigateUrlField="url" ToolTipField="description" />
	    </DataBindings>
	    </asp:Menu><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %><!-- END -->
	    <asp:XmlDataSource ID="MenuDataSource" runat="server" DataFile="~/App_Data/MenuData.xml" />
    </div>
    </form>
</body>
</html>
