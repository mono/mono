<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
  void Page_Load(object sender, EventArgs e)
    {
      if (Application["UserCount"] != null)
      {
          lblUserCount.Text = Application["UserCount"].ToString();
          lblCurrentUser.Text = Request.AnonymousID;
      }
  }    
</script>


<html>
<head runat="server">
    <title>AnonymousID Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Number of users: 
        <asp:Label ID="lblUserCount" Runat="server"></asp:Label><br />
    Current user:
        <asp:Label ID="lblCurrentUser" Runat="server"></asp:Label><br />
    </div>
    </form>
</body>
</html>
