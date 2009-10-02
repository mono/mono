<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DynamicValidator_02.aspx.cs" Inherits="DynamicValidator_02" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head id="Head1" runat="server">
    <title>DynamicValidator Sample</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:DynamicDataManager ID="DynamicDataManager1" runat="server" AutoLoadForeignKeys="true" />
        <asp:ListView ID="ListView1" runat="server" DataSourceID="DynamicDataSource1">
            <LayoutTemplate>
                <div runat="server" id="itemPlaceholder" />
            </LayoutTemplate>
            <ItemTemplate>
                <asp:Button runat="server" CommandName="Edit" Text="Edit" ID="editMe" CausesValidation="false"/>
                <div>
                    <test:PokerDynamicControl runat="server" DataField="Column1" ID="Column1" />
                </div>
            </ItemTemplate>
            <EditItemTemplate>
                <div>
                    <test:PokerDynamicControl runat="server" DataField="Column1" ID="Column1" Mode="Edit" />
                </div>
            </EditItemTemplate>
        </asp:ListView>
        <test:DynamicDataSource runat="server" ID="DynamicDataSource1" />
    </div>
    </form>
</body>
</html>
