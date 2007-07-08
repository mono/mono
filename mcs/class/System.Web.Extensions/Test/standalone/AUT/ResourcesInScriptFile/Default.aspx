<%@ Page Language="C#" AutoEventWireup="true" UICulture="auto" Culture="auto" %>
<script runat="server">
    
    
    protected void Page_Load(object sender, EventArgs e)
    {   
        int _firstInt;
        int _secondInt;
        
        Random random = new Random();
        _firstInt = random.Next(0, 20);
        _secondInt = random.Next(0, 20);

        firstNumber.Text = _firstInt.ToString();
        secondNumber.Text = _secondInt.ToString();
        
        if (IsPostBack)
        {
            userAnswer.Text = "";
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(selectLanguage.SelectedValue);
        }
        else
        {
            selectLanguage.Items.FindByValue(System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName).Selected = true;
        }
    }

    protected void selectLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(selectLanguage.SelectedValue);
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Client Localization Example</title>
</head>
<body>
    <form id="form1" runat="server" >
        <asp:DropDownList runat="server" AutoPostBack="true" ID="selectLanguage" OnSelectedIndexChanged="selectLanguage_SelectedIndexChanged">
            <asp:ListItem Text="English" Value="en"></asp:ListItem>
            <asp:ListItem Text="Italian" Value="it"></asp:ListItem>
        </asp:DropDownList>
        <br /><br />
        <asp:ScriptManager ID="ScriptManager1" EnableScriptLocalization="true" runat="server">
        <Scripts>
            <asp:ScriptReference Path="scripts/CheckAnswer.js" ResourceUICultures="it-IT" />
        </Scripts>
        </asp:ScriptManager>
        <div>
        <asp:Label ID="firstNumber" runat="server"></asp:Label>
        +
        <asp:Label ID="secondNumber" runat="server"></asp:Label>
        =
        <asp:TextBox ID="userAnswer" runat="server"></asp:TextBox>
        <asp:Button ID="Button1" runat="server" OnClientClick="return CheckAnswer()" />
        <br />
        <asp:Label ID="labeltest" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>
