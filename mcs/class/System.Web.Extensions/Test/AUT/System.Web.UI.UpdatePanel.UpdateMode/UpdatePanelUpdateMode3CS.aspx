
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (DropDownList1.SelectedValue)
        {
            case "1":
                UpdatePanel1.Update();
                break;
            case "2":
                UpdatePanel2.Update();
                break;
            case "3":
                UpdatePanel1.Update();
                UpdatePanel2.Update();
                break;
        }
        DropDownList1.SelectedIndex = 0;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ScriptManager1.RegisterAsyncPostBackControl(DropDownList1);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanelUpdateMode Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1"
                               runat="server" />
            <asp:Panel ID="Panel1"
                       GroupingText="Panel 1"
                       runat="server">
                <asp:UpdatePanel ID="UpdatePanel1"
                                 UpdateMode="Conditional"
                                 runat="server">
                    <ContentTemplate>
                        <p>
                            UpdatePanel.Update() method is called if this panel is selected
                            to be updated from DropDownList control. Last updated:
                            <%= DateTime.Now.ToString()%>
                        </p>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
            <asp:Panel ID="Panel2"
                       GroupingText="Panel 2"
                       runat="server">
                <asp:UpdatePanel ID="UpdatePanel2"
                                 UpdateMode="Conditional"
                                 runat="server">
                    <ContentTemplate>
                        <p>
                            UpdatePanel.Update() method is called if this panel is selected
                            to be updated from DropDownList control. Last updated:
                            <%= DateTime.Now.ToString() %>
                        </p>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
            <asp:DropDownList ID="DropDownList1"
                              AutoPostBack="True"
                              OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged"
                              runat="server">
                <asp:ListItem Text="Select a panel to update..."
                              Value="0"
                              Selected="True" />
                <asp:ListItem Text="Refresh Panel 1"
                              Value="1" />
                <asp:ListItem Text="Refresh Panel 2"
                              Value="2" />
                <asp:ListItem Text="Refresh Panel 1 + 2"
                              Value="3" />
            </asp:DropDownList>
        </div>
    </form>
</body>
</html>
