
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            int a = Int32.Parse(TextBox1.Text);
            int b = Int32.Parse(TextBox2.Text);
            int res = a / b;
            Label1.Text = res.ToString();
        }
        catch (Exception ex)
        {
            if (TextBox1.Text.Length > 0 && TextBox2.Text.Length > 0)
            {
                ex.Data["ExtraInfo"] = " You can't divide " +
                    TextBox1.Text + " by " + TextBox2.Text + ".";
            }
            throw ex;
        }
    }

    protected void ScriptManager1_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
    {
        if (e.Exception.Data["ExtraInfo"] != null)
        {
            ScriptManager1.AsyncPostBackErrorMessage =
                e.Exception.Message +
                e.Exception.Data["ExtraInfo"].ToString();
        }
        else
        {
            ScriptManager1.AsyncPostBackErrorMessage =
                "An unspecified error occurred.";
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Error Handling Example</title>
    <style type="text/css">
    #UpdatePanel1 {
      width: 200px; height: 50px;
      border: solid 1px gray;
    }
    #AlertDiv{
    left: 40%; top: 40%;
    position: absolute; width: 200px;
    padding: 12px; 
    border: #000000 1px solid;
    background-color: white; 
    text-align: left;
    visibility: hidden;
    z-index: 99;
    }
    #AlertButtons{
    position: absolute; right: 5%; bottom: 5%;
    }
    </style>
</head>
<body id="bodytag">
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
            </asp:ScriptManager>
            <script type="text/javascript" language="javascript">
            var divElem = 'AlertDiv';
            var messageElem = 'AlertMessage';
            var bodyTag = 'bodytag';
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
            function ToggleAlertDiv(visString)
            {
                 if (visString == 'hidden')
                 {
                     $get(bodyTag).style.backgroundColor = 'white';                         
                 }
                 else
                 {
                     $get(bodyTag).style.backgroundColor = 'gray';                         
                    
                 }
                 var adiv = $get(divElem);
                 adiv.style.visibility = visString;
                 
            }
            function ClearErrorState() {
                 $get(messageElem).innerHTML = '';
                 ToggleAlertDiv('hidden');                     
            }
            function EndRequestHandler(sender, args)
            {
               if (args.get_error() != undefined)
               {
                   var errorMessage;
                   if (args.get_response().get_statusCode() == '200')
                   {
                       errorMessage = args.get_error().message;
                   }
                   else
                   {
                       // Error occurred somewhere other than the server page.
                       errorMessage = 'An unspecified error occurred. ';
                   }
                   args.set_errorHandled(true);
                   ToggleAlertDiv('visible');
                   $get(messageElem).innerHTML = errorMessage;
               }
            }
            </script>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" Width="39px"></asp:TextBox>
                    /
                    <asp:TextBox ID="TextBox2" runat="server" Width="39px"></asp:TextBox>
                    =
                    <asp:Label ID="Label1" runat="server"></asp:Label><br />
                    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="calculate" />
                </ContentTemplate>
            </asp:UpdatePanel>
            <div id="AlertDiv">
                <div id="AlertMessage">
                </div>
                <br />
                <div id="AlertButtons">
                    <input id="OKButton" type="button" value="OK" runat="server" onclick="ClearErrorState()" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
