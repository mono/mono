<!-- This is a helper file to grab snippets from -->
<!-- This is not meant to be run -->
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Update Method Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
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
        </div>
    </form>
</body>
</html>
