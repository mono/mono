<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
	void Page_Load (object sender, EventArgs e)
	{
//		Literal2.Text = "Test";
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Bug #568631</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <img alt='<asp:Literal ID="Literal2" runat="server" text="<%$Resources:Resource1, TestString  %>"/>' src="../images/ok_16x16.png" />
            
    </div>
    </form>
</body>
</html>