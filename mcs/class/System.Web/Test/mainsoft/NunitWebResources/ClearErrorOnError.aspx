<%@ Page Language="C#"  %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<script runat="server" >
    protected void Page_Load(object sender, EventArgs e)
    {
        //System.Threading.Thread.CurrentThread.Abort ();
        throw new Exception ();
    }
    
    protected void Page_Error(object sender, EventArgs e)
    {
        Context.ClearError ();
    }
</script>

<html  >
<head id="Head1" runat="server">
    <title>Exception handling test</title>
</head>
<body>
    <form id="Form1" runat="server">
    <div>
    </div>
    </form>
</body>
</html>
