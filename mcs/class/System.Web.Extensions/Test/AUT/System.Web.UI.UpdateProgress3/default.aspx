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
    #UpdatePanel1, #UpdatePanel2, #UpdateProgress1 {
      border-right: gray 1px solid; border-top: gray 1px solid; 
      border-left: gray 1px solid; border-bottom: gray 1px solid;    
    }
    #UpdatePanel1, #UpdatePanel2 { 
      width:200px; height:200px; position: relative;
      float: left; margin-left: 10px; margin-top: 10px;
     }
     #UpdateProgress1 {
      width: 400px; background-color: #FFC080; 
      bottom: 0%; left: 0px; position: absolute;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <script type="text/javascript">
    function AbortPostBack() {
      var prm = Sys.WebForms.PageRequestManager.getInstance();
      if (prm.get_isInAsyncPostBack()) {
           prm.abortPostBack();
      }        
    }
    </script>
    <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
    <%=DateTime.Now.ToString() %> <br />
    <asp:Button ID="Button1" runat="server" Text="Refresh Panel" OnClick="Button_Click" />    
    <br />
    To stop the existing
    asynchronous postback without starting a new one, click the button
    provided in the ProgressTemplate.
    </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanel2" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
    <%=DateTime.Now.ToString() %> <br />
    <asp:Button ID="Button2" runat="server" Text="Refresh Panel" OnClick="Button_Click"/>    
    </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
    <ProgressTemplate>
      Update in progress...
      <input type="button" value="stop" onclick="AbortPostBack()" />
    </ProgressTemplate>
    </asp:UpdateProgress>
    </div>
    </form>
</body>
</html>
