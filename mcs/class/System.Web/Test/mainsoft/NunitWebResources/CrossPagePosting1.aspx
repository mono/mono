<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void LinkButton1_Click (object sender, EventArgs e)
    {
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("LinkButton1_Click");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("LinkButton1_Click");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected void Page_Load (object sender, EventArgs e)
    {
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page1 - Load");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page1 - Load");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnLoadComplete (EventArgs e)
    {
        base.OnLoadComplete (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page1 - LoadComplete");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page1 - LoadComplete");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnPreRender (EventArgs e)
    {
        base.OnPreRender (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page1 - PreRender");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page1 - PreRender");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnUnload (EventArgs e)
    {
        base.OnUnload (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page1 - Unload");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page1 - Unload");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnSaveStateComplete (EventArgs e)
    {
        base.OnSaveStateComplete (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page1 - SaveStateComplete");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page1 - SaveStateComplete");
            WebTest.CurrentTest.UserData = list;
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:LinkButton ID="LinkButton1" runat="server" PostBackUrl="~/CrossPagePosting2.aspx" OnClick="LinkButton1_Click">LinkButtonText</asp:LinkButton>&nbsp;
    </div>
    </form>
</body>
</html>
