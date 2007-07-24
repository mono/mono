<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Button_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(3000);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>UpdateProgress Example</title>
    <style type="text/css">
    #UpdatePanel1 { 
      width:200px; height:200px; position: relative;
      float: left; margin-left: 10px; margin-top: 10px;
      border-right: gray 1px solid; border-top: gray 1px solid; 
      border-left: gray 1px solid; border-bottom: gray 1px solid;
     }
     #UpdateProgress1 {
      width: 200px; background-color: #FFC080;
      position: absolute; bottom: 0px; left: 0px;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
    <%=DateTime.Now.ToString() %> <br />
    <asp:Button ID="Button1" runat="server" Text="Refresh Panel" OnClick="Button_Click" />    
    </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
    <ProgressTemplate>
      An update is in progress...
    </ProgressTemplate>
    </asp:UpdateProgress>
    </div>
    </form>
</body>
</html>
