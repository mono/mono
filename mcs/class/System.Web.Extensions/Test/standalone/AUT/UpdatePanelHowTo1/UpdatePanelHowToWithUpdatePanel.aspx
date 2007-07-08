<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            Label1.Text = "Initial panel contents rendered at " + 
                DateTime.Now.ToString();
        else
            Label1.Text = "Pane contents refreshed at " + 
                DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdatePanel Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
       <asp:ScriptManager ID="SriptManager1"
                          runat="server" />
       
       <fieldset>
       <asp:UpdatePanel ID="UpdatePanel1" 
                          runat="server">
           <ContentTemplate>
               <asp:Label ID="Label1" 
                          runat="server"/>
               <br />
               <asp:Button ID="Button1"
                           Text="Refresh UpdatePanel"
                           runat="server" />       
           </ContentTemplate>
       </asp:UpdatePanel>
       </fieldset>
       <asp:Button ID="Button2"
                   Text="Refresh Page"
                   runat="server" />
    </div>
    </form>
</body>
</html>
