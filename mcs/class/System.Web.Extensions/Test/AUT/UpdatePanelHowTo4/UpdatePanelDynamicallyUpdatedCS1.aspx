<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
    protected void Page_Load(object sender, EventArgs e)
    {
        TheScriptManager.RegisterAsyncPostBackControl(DropDownList1);
    }

    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (DropDownList1.SelectedValue)
        {
            case "DefaultView":
                MultiView1.SetActiveView(DefaultView);
                break;
            case "NewsView":
                MultiView1.SetActiveView(NewsView);
                break;
        }
        UpdatePanel1.Update();
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Update Method Example</title>
    <style type="text/css">
    a
    {
        font-family: Arial;
        font-size:small;
    }
    label.LabelHeaderStyle
    {
        font-size: medium;
        font-weight: bold;
        font-family: Arial;
    }
    div.PanelStyle
    {
        width: 300px;
        border-color: #404040;
        border-style: double;
        height: 150px;
    }
    #DefaultViewPanel
    {
        background-color: #C0C0FF;
    }
    #NewsViewPanel
    {
        background-color: #C0FFC0;
    }    
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="TheScriptManager" 
                               runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1" 
                             UpdateMode="Conditional"
                             runat="server">
                <ContentTemplate>
                    <asp:MultiView ID="MultiView1"
                                   ActiveViewIndex="0"
                                   runat="Server">
                        <asp:View ID="DefaultView" 
                                  runat="Server">
                            <asp:Panel ID="DefaultViewPanel" 
                                       CssClass="PanelStyle"
                                       runat="Server" >
                                <asp:Label ID="DefaultLabel1"
                                           Text="The Default View" 
                                           AssociatedControlID="DefaultView"
                                           CssClass="LabelHeaderStyle"
                                           runat="Server">
                                </asp:Label>
                                <asp:BulletedList ID="DefaultBulletedList1"
                                                  BulletStyle="Disc"
                                                  DisplayMode="Hyperlink"
                                                  Target="_blank"
                                                  runat="Server">
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's Weather</asp:ListItem>
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's Stock Quotes</asp:ListItem>
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's News Headlines</asp:ListItem>
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's Featured Shopping</asp:ListItem>
                                </asp:BulletedList>
                            </asp:Panel>
                        </asp:View>
                        <asp:View ID="NewsView"
                                  runat="Server">
                            <asp:Panel ID="NewsViewPanel" 
                                       CssClass="PanelStyle"
                                       runat="Server">
                                <asp:Label ID="NewsLabel1"
                                           Text="The News View" 
                                           AssociatedControlID="NewsView"
                                           CssClass="LabelHeaderStyle"
                                           runat="Server">                    
                                </asp:Label>
                                <asp:BulletedList ID="NewsBulletedlist1" 
                                                  BulletStyle="Disc" 
                                                  DisplayMode="Hyperlink"
                                                  Target="_blank" 
                                                  runat="Server">
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's International Headlines</asp:ListItem>
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's National Headlines</asp:ListItem>
                                    <asp:ListItem Value="http://www.microsoft.com">
                                    Today's Local News</asp:ListItem>
                                </asp:BulletedList>
                            </asp:Panel>
                        </asp:View>
                    </asp:MultiView>
                </ContentTemplate>
            </asp:UpdatePanel>
            <br />
        </div>
        <asp:DropDownList ID="DropDownList1"
                          AutoPostBack="True"
                          OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" 
                          runat="server">
            <asp:ListItem Value="DefaultView">
            Default View</asp:ListItem>
            <asp:ListItem Value="NewsView">
            News View</asp:ListItem>
        </asp:DropDownList>
    </form>
</body>
</html>
