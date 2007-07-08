
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected DateTime LastUpdate
    {
        get
        {
            return (DateTime)(ViewState["LastUpdate"] ?? DateTime.Now);
        }
        set
        {
            ViewState["LastUpdate"] = value;
        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (LastUpdate.AddSeconds(5.0) < DateTime.Now)
        {
            UpdatePanel1.Update();
            LastUpdate = DateTime.Now;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        ScriptManager1.RegisterAsyncPostBackControl(Button1);   
        if (!IsPostBack)
        {
            LastUpdate = DateTime.Now;
        }
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
                       GroupingText="UpdatePanel1"
                       runat="server">
                <asp:UpdatePanel ID="UpdatePanel1"
                                 UpdateMode="Conditional"
                                 runat="server">
                    <ContentTemplate>
                        <p>
                            The content in this UpdatePanel only refreshes if five or more
                            seconds have passed since the last refresh and the button in
                            UpdatePanel2 was clicked. The time is checked
                            server-side and the UpdatePanel.Update() method is called. Last
                            updated: <strong>
                                <%= LastUpdate.ToString() %>
                            </strong>
                        </p>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
            <asp:Panel ID="Panel2"
                       GroupingText="UpdatePanel2"
                       runat="server">
                <asp:UpdatePanel ID="UpdatePanel2"
                                 runat="server">
                    <ContentTemplate>
                        <p>
                            This UpdatePanel always refreshes if the button is clicked.
                            Last updated: <strong>
                                <%= DateTime.Now.ToString() %>
                            </strong>
                        </p>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
            <asp:Button ID="Button1" Text="Button1" runat="server" OnClick="Button1_Click" />
        </div>
    </form>
</body>
</html>
