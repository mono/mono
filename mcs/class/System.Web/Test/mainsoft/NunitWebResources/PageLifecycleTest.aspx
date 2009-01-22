<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="MonoTests.SystemWeb.Framework" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        protected override void OnInit (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnInit");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnInit");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnInit (e);
        }

        protected override void OnInitComplete (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnInitComplete");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnInitComplete");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnInitComplete (e);
        }

        protected override void OnLoad (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnLoad");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnLoad");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnLoad (e);
        }

        protected override void OnLoadComplete (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnLoadComplete");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnLoadComplete");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnLoadComplete (e);
        }

        protected override void OnPreInit (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnPreInit");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnPreInit");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnPreInit (e);
        }

        protected override void OnPreLoad (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnPreLoad");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnPreLoad");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnPreLoad (e);
        }

        protected override void OnPreRender (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnPreRender");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnPreRender");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnPreRender (e);
        }

        protected override void OnPreRenderComplete (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnPreRenderComplete");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnPreRenderComplete");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnPreRenderComplete (e);
        }

        protected override void OnSaveStateComplete (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnSaveStateComplete");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnSaveStateComplete");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnSaveStateComplete (e);
        }

        protected override void OnUnload (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnUnload");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnUnload");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnUnload (e);
        }

        protected override void OnError (EventArgs e)
        {
            if (WebTest.CurrentTest.UserData == null) {
                ArrayList list = new ArrayList ();
                list.Add ("OnError");
                WebTest.CurrentTest.UserData = list;
            }
            else {
                ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
                if (list == null)
                    throw new NullReferenceException ();
                list.Add ("OnError");
                WebTest.CurrentTest.UserData = list;
            }
            base.OnError (e);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
