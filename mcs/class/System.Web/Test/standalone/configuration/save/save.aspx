<%@ Import Namespace="System.Web.Configuration" %>

<script runat="server" language="C#" >

protected void add_Click (object sender, EventArgs e)
{
   try
   {
      Configuration config = 
         WebConfigurationManager.OpenWebConfiguration(
         Request.ApplicationPath);

      ConnectionStringsSection sect = 
         config.ConnectionStrings;            

      if (sect.ConnectionStrings ["test"] != null) {
		lblResult.Text = "Connection string already exists for 'test'";
      }
      else {
		sect.ConnectionStrings.Add (new ConnectionStringSettings ("test", "test=foo;", "testProvider"));
		config.Save();
		lblResult.Text = "Connection string added";
      }
   }
   catch (Exception ex)
   {
      lblResult.Text = "Exception: " + ex.Message;
   }
//   lblResult.Text+="Connection String:" + 
//      ConfigurationManager.ConnectionStrings
//      ["test"].ConnectionString;
}

protected void remove_Click (object sender, EventArgs e)
{ 
   try
   {
      Configuration config = 
         WebConfigurationManager.OpenWebConfiguration(
         Request.ApplicationPath);
      ConnectionStringsSection sect = 
         config.ConnectionStrings;
      if (sect.ConnectionStrings ["test"] == null) {
		lblResult.Text = "connection string not present";
	}
	else {
		sect.ConnectionStrings.Remove ("test");
		config.Save();
		lblResult.Text = "connection string has been removed";
	}	
   }
   catch (Exception ex)
   {
      lblResult.Text = "Exception: " + ex.Message;
   }        
}    
</script>
<html>
<head>
  <title>Adding and Removing Connection Strings</title>
</head>
<body>
<form id="form1" runat="server">
<div>
  
 <asp:Button ID="add" runat="server" Text="Add" OnClick="add_Click" />
 <asp:Button ID="remove" runat="server" Text="Remove" OnClick="remove_Click" />
 <br/><br/><br/>
 <asp:Label ID="lblResult" runat="server"></asp:Label>
</div>
</form>
</body>
</html>
