
<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Page_PreRender(object sender, EventArgs e)
    {
        string script = @"
        function ToggleItem(id)
          {
            var elem = $get('div'+id);
            if (elem) 
            {
              if (elem.style.display != 'block') 
              {
                elem.style.display = 'block';
                elem.style.visibility = 'visible';
              } 
              else
              {
                elem.style.display = 'none';
                elem.style.visibility = 'hidden';
              }
            }
          }
        ";

        ScriptManager.RegisterClientScriptBlock(
            this,
            typeof(Page),
            "ToggleScript",
            script,
            true);
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>ScriptManager RegisterClientScriptInclude</title>
</head>
<body>
    <form id="Form1" runat="server">
        <div>
            <br />
            <asp:ScriptManager ID="ScriptManager1" 
                                 EnablePartialRendering="true"
                                 runat="server">
            </asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" 
                               UpdateMode="Conditional"
                               runat="server">
                <ContentTemplate>
                    <asp:XmlDataSource ID="XmlDataSource1"
                                       DataFile="~/App_Data/Contacts.xml"
                                       XPath="Contacts/Contact"
                                       runat="server"/>
                    <asp:DataList ID="DataList1" DataSourceID="XmlDataSource1"
                        BackColor="White" BorderColor="#E7E7FF" BorderStyle="None"
                        BorderWidth="1px" CellPadding="3" GridLines="Horizontal"
                        runat="server">
                        <ItemTemplate>
                            <div style="font-size:larger; font-weight:bold; cursor:pointer;" 
                                 onclick='ToggleItem(<%# Eval("ID") %>);'>
                                <span><%# Eval("Name") %></span>
                            </div>
                            <div id='div<%# Eval("ID") %>' 
                                 style="display: block; visibility: visible;">
                                <span><%# Eval("Company") %></span>
                                <br />
                                <a href='<%# Eval("URL") %>' 
                                   target="_blank" 
                                   title='<%# Eval("Name", "Link to the {0} Web site") %>'>
                                   <%# Eval("URL") %></a>
                                </asp:LinkButton>
                                <hr />
                            </div>
                        </ItemTemplate>
                        <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
                        <SelectedItemStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
                        <AlternatingItemStyle BackColor="#F7F7F7" />
                        <ItemStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" />
                        <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#F7F7F7" />
                    </asp:DataList>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
