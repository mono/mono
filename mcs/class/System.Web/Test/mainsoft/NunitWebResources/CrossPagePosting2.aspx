<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
    protected override void OnInit (EventArgs e)
    {
        base.OnInit (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - OnInit");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - OnInit");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnLoad (EventArgs e)
    {
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - Load");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - Load");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnLoadComplete (EventArgs e)
    {
        base.OnLoadComplete (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - LoadComplete");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - LoadComplete");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnPreRender (EventArgs e)
    {
        base.OnPreRender (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - PreRender");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - PreRender");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnUnload (EventArgs e)
    {
        base.OnUnload (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - Unload");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - Unload");
            WebTest.CurrentTest.UserData = list;
        }
    }

    protected override void OnSaveStateComplete (EventArgs e)
    {
        base.OnSaveStateComplete (e);
        if (WebTest.CurrentTest.UserData == null) {
            ArrayList list = new ArrayList ();
            list.Add ("Page2 - SaveStateComplete");
            WebTest.CurrentTest.UserData = list;
        }
        else {
            ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
            if (list == null)
                throw new NullReferenceException ();
            list.Add ("Page2 - SaveStateComplete");
            WebTest.CurrentTest.UserData = list;
        }
    }
    
    </script>
</head>

    
    


<body>
    <form id="form1" runat="server">
    <div>
        CrossedPostbackPage
    </div>
    </form>
</body>
</html>
