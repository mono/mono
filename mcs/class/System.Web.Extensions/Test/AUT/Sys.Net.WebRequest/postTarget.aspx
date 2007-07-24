<%@ Page Language="C#" AutoEventWireup="true"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Post Target</title>
    
    <script language="C#" runat="server">
        
        protected void Page_Load(object sender, EventArgs e)
        {
             if (HttpContext.Current.Request.Form["Message"] != null)
                LabelID.Text = 
                    HttpContext.Current.Request.Form["Message"].ToString();
        }
        
    </script>

</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>WebRequestPost Target</h1>
       
        
        <p>
            <asp:Textbox id="LabelID"  Text="test" runat="server"/>
        </p>
        
         ... Yes I hear you.
        
    </div>
    </form>
</body>
</html>
