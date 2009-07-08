<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ListView_DynamicControl_02.aspx.cs" Inherits="ListView_DynamicControl_02" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html >
<head id="Head1" runat="server">
  <title>DynamicControl Sample</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:DynamicDataManager ID="DynamicDataManager2" runat="server" AutoLoadForeignKeys="true" />
      <asp:ListView ID="ListView2" runat="server" DataSourceID="DynamicDataSource2">
        <LayoutTemplate>
          <div runat="server" id="itemPlaceholder" />
        </LayoutTemplate>
        <ItemTemplate>
	<div>
		<test:PokerDynamicControl runat="server" DataField="FirstName" id="FirstName2"/> 
		<test:PokerDynamicControl runat="server" DataField="LastName" id="LastName2" UIHint="MyCustomUIHintTemplate_Text"/>
        </div>
        </ItemTemplate>
      </asp:ListView>

	<test:DynamicDataSource runat="server" id="DynamicDataSource2" />
    </div>
    </form>
</body>
</html>
