<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (ConfigurationManager.AppSettings["RenameID"] != null)
            {
                bool doRename = bool.Parse(ConfigurationManager.AppSettings["RenameID"]);
                if (doRename)
                {
                    //Button1.ID = "abcd:" + Button1.ID;
                }
            }
        }
    }
    
    protected void Button1_Click(object sender, EventArgs e)
    {
        xyz_Label1.Text = "Refreshed at " +
            DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <style type="text/css">
    #UpdatePanel1 { 
      width:300px; height:100px;
     }
    </style>
</head>
<body runat="server">
    <form id="xyz_form1" runat="server">
    <div style="padding-top: 10px">
        <asp:ScriptManager ID="xyz_ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="xyz_UpdatePanel1" runat="server">
            <ContentTemplate>
                <fieldset>
                <legend>UpdatePanel</legend>
                <asp:Label ID="xyz_Label1" runat="server" Text="Panel created."></asp:Label><br />
                <asp:Button ID="xyz_Button1" runat="server" OnClick="Button1_Click" Text="Button" />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        </div>

    </div>
        <input type="hidden" id="__NAMESPACE" value="xyz_" />
    </form>
</body>
</html>