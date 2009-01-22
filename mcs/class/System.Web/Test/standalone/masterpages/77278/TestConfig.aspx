<%@ Page Language="C#" MasterPageFile="simple.master" %>

<script runat="server">


override protected void OnPreInit(EventArgs e)
{

    //this is another bug if you uncomment this
    //it works as expected on windows but
    //causes a null reference in the page load event on mono
    
    base.OnPreInit(e);
    this.MasterPageFile = "simple2.master";
}

override protected void OnInit(EventArgs e)
{

    base.OnInit(e);
}


private void Page_PreInit (object sender, EventArgs e)
{
	this.MasterPageFile = "simple2.master";
}


private void Page_Load (object sender, EventArgs e)
{
	lblHelloWeb.Text = "Hello Web!";
	
	//TestKey

}

private void btnTestConfig_Click (object sender, EventArgs e)
{
	//this displays the value on windows but not on mono  r55575
	this.lblTestConfig.Text = System.Configuration.ConfigurationSettings.AppSettings.Get("TestKey");
	
	

}


</script>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="server">

Contents go here
<hr />
<asp:Label id="lblHelloWeb" runat="server" />
<hr />
<asp:Button id="btnTestConfig" runat="server" OnClick="btnTestConfig_Click" Text="Test Config"  />
<asp:Label id="lblTestConfig" runat="server" />

</asp:Content>
