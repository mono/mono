<%@ Control Language="C#" AutoEventWireup="true" EnableTheming="false" CodeBehind="Searcher.ascx.cs" Inherits="Mainsoft.Web.AspnetConfig.Searcher" %>
<%@ Register tagprefix="custom" namespace="Mainsoft.Web.AspnetConfig" Assembly="Mainsoft.Web.AspnetConfig"  %>

<table width="100%" cellpadding="0" cellspacing="0">
    <tr>
    <td width="100%" class="controlheader">
           Edit roles:
    </td>
    </tr>
    <tr>
        <td width= "100%" >
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4"
                ForeColor="#333333" GridLines="None" Width="100%">
                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                <Columns>
                    <asp:TemplateField HeaderText="Edit roles">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBox2" runat="server" AutoPostBack="True" Checked='<%# Bind("IsInRole") %>'
                                OnCheckedChanged="Roles_Changed" Text='<%# Bind("Role") %>' />
                        </ItemTemplate>
                        <HeaderTemplate>
                            Edit roles for user : <%# User %>
                        </HeaderTemplate>
                    </asp:TemplateField>
                </Columns>
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                <EditRowStyle BackColor="#999999" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            </asp:GridView>
        </td>        
    </tr>
</table>