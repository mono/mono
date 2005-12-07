<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Web.Configuration" %>

<script runat="server" language="C#" >
const string PROVIDER =   "DataProtectionConfigurationProvider";    
   
protected void btnEncrypt_Click(
 object sender, EventArgs e)
{
   try
   {
      Configuration config = 
         WebConfigurationManager.OpenWebConfiguration(
         Request.ApplicationPath);

      ConnectionStringsSection sect = 
         config.ConnectionStrings;            

      sect.SectionInformation.ProtectSection(PROVIDER);

      config.Save();

      lblResult.Text ="Connection string" +   
         " section is now encrypted in " +
         "web.config file<br>";        
   }
   catch (Exception ex)
   {
      lblResult.Text = "Exception: " + 
         ex.Message;
   }

   //Note that when you read the encrypted 
   //connection string, it is 
   //automatically decrypted for you
   lblResult.Text+="Connection String:" + 
      ConfigurationManager.ConnectionStrings
      ["pubs"].ConnectionString;
}

protected void btnDecrypt_Click(
   object sender, EventArgs e)
{ 
   try
   {
      Configuration config = 
         WebConfigurationManager.OpenWebConfiguration(
         Request.ApplicationPath);
      ConnectionStringsSection sect = 
         config.ConnectionStrings;
      if (sect.SectionInformation.IsProtected)
      {
         sect.SectionInformation.UnprotectSection();           
         config.Save();
      }
      lblResult.Text="Connection string" +
         " is now decrypted in web.config" +
         " file";
   }
   catch (Exception ex)
   {
      lblResult.Text = "Exception: " + ex.Message;
   }        
}    
</script>
<html>
<head>
  <title>Encrypting and Decrypting Connection Strings</title>
</head>
<body>
<form id="form1" runat="server">
<div>
  
 <asp:Button ID="btnEncrypt" 
  Runat="server" Text="Encrypt" 
  Width="96px" Height="35px" 
  OnClick="btnEncrypt_Click" />
 <asp:Button ID="btnDecrypt" 
  Runat="server" Text="Decrypt" 
  Width="102px" Height="35px" 
  OnClick="btnDecrypt_Click" />
 <br/><br/><br/>
 <asp:Label ID="lblResult" 
  runat="server" Height="19px" 
  Width="435px"></asp:Label>
</div>
</form>
</body>
</html>
