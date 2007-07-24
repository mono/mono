<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        if (ScriptManager1.IsInAsyncPostBack)
        {
            System.Web.Script.Serialization.JavaScriptSerializer json =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            ScriptManager1.RegisterDataItem(Label1, DateTime.Now.ToString());
            ScriptManager1.RegisterDataItem(Label2, json.Serialize("more data"), true);
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>ScriptManager RegisterDataItem Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <script type="text/javascript" language="javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(PageLoadingHandler);
        function PageLoadingHandler(sender, args) {
            var dataItems = args.get_dataItems();
            if ($get('Label1') !== null)
                $get('Label1').innerHTML = dataItems['Label1'];
            if ($get('Label2') !== null)
                $get('Label2').innerHTML = dataItems['Label2'];
        }
    </script>
    <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
       UpdatePanel content.
       <asp:Button ID="Button1" Text="Submit" runat="server" />
    </ContentTemplate>
    </asp:UpdatePanel>
    <hr />
    <asp:Label ID="Label1" runat="server" /> <br />
    <asp:Label ID="Label2" runat="server" />
    </div>
    </form>
</body>
</html>
