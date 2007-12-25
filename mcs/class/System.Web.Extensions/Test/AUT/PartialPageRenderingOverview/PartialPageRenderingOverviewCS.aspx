<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected override void OnLoad(EventArgs e)
    {
		Calendar.TodaysDate = new DateTime (2007, 7, 1);
        if (IsPostBack)
            Label1.Text = "This page posted back";
        base.OnLoad(e);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Partial-Page Rendering Server-Side Syntax</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:ScriptManager ID="ScriptManager1" runat="server" />
      <asp:UpdatePanel ID="UpdatePanel1" runat="server">
      <ContentTemplate>
        <fieldset>
        <legend>UpdatePanel</legend>
        Content to update incrementally without a full
        page refresh. 
        <br />
         <asp:label ID="lbl" runat="server">Last update:  <%=DateTime.Now.ToString() %></asp:label>
        <br />
        <asp:Calendar ID="Calendar" runat="server"/>
        </fieldset>
      </ContentTemplate>
      </asp:UpdatePanel>
      
      <asp:Label ID="Label1" runat="server"></asp:Label>
      <script type="text/javascript" language="javascript">
      var prm = Sys.WebForms.PageRequestManager.getInstance();
      prm.add_pageLoaded(PageLoadedEventHandler);
      function PageLoadedEventHandler() {
         // custom script
      }
      </script>
    </div>
    </form>
</body>
</html>
