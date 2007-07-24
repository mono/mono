<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            Label1.Text = "Initial page rendered at " + 
                DateTime.Now.ToString();
        else
            Label1.Text = "Page refreshed at " + 
                DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Example - No UpdatePanel</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
       <fieldset>
               <asp:Label ID="Label1" 
                          runat="server"/>
               <br />
               <asp:Button ID="Button1"
                           Text="Refresh UpdatePanel"
                           runat="server" />       
       </fieldset>
       <asp:Button ID="Button2"
                   Text="Refresh Page"
                   runat="server" />
    </div>
    </form>
</body>
</html>
