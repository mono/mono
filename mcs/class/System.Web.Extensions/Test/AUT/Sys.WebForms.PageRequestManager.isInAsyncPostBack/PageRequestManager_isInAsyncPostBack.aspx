
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

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

    void Page_Load()
    {
        if (!IsPostBack)
        {
            LastUpdate = DateTime.Now;
        }
    }

    protected void SlowProcessClick_Handler(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(10000);
        LastUpdate = DateTime.Now;
    }
    protected void FastProcessClick_Handler(object sender, EventArgs e)
    {
        LastUpdate = DateTime.Now;
    }
	
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>PageRequestManager get_inPostBack Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    div.AlertStyle
    {
      background-color: #FFC080;
      top: 95%;
      left: 1%;
      height: 20px;
      width: 270px;
      position: absolute;
      visibility: hidden;
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />

            <script type="text/javascript" language="javascript">

            Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(CheckStatus);

            function CheckStatus(sender, arg)
            {
              var prm = Sys.WebForms.PageRequestManager.getInstance();
              if (prm.get_isInAsyncPostBack()) {
                 arg.set_cancel(true);
                 ChangeAlertDivVisibility('visible');
                 setTimeout("ChangeAlertDivVisibility('hidden')", 1000);
              }
            }
            function ChangeAlertDivVisibility(visstring)
            {
                 var adiv = $get('AlertDiv');
                 adiv.style.visibility = visstring;
            }
            </script>

            <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                    <asp:Panel ID="Panel1" runat="server" GroupingText="Update Panel">
                        Last update:
                        <%= LastUpdate.ToString() %>
                        .
                        <br />
                        <asp:Button ID="Button1" 
                                    Text="Submit for Slow Process"
                                    OnClick="SlowProcessClick_Handler"
                                    runat="server" />
                        <asp:Button ID="Button2" 
                                    Text="Submit for Fast Process"
                                    OnClick="FastProcessClick_Handler"
                                    runat="server" />
                        <br />
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Panel ID="AlertDiv" 
                       CssClass="AlertStyle"
                       runat="server" >
                still processing previous request...
            </asp:Panel>
        </div>
    </form>
</body>
</html>

