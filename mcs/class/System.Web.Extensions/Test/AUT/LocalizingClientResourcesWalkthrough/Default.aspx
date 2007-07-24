<%@ Page Language="C#" AutoEventWireup="true" UICulture="auto" Culture="auto" %>
<%@ Register TagPrefix="Samples" Namespace="LocalizingScriptResources" Assembly="SystemWebExtensionsAUT" %>
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
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
<head runat="server">
    <title>Client Localization Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:DropDownList runat="server" AutoPostBack="true" ID="selectLanguage" OnSelectedIndexChanged="selectLanguage_SelectedIndexChanged">
            <asp:ListItem Text="English" Value="en"></asp:ListItem>
            <asp:ListItem Text="Italian" Value="it"></asp:ListItem>
        </asp:DropDownList>
        <br /><br />
        <asp:ScriptManager ID="ScriptManager1" EnableScriptLocalization="true" runat="server">
        <Scripts>
            <asp:ScriptReference Assembly="SystemWebExtensionsAUT" Name="SystemWebExtensionsAUT.LocalizingClientResourcesWalkthrough.CheckAnswer.js" />
        </Scripts>
        </asp:ScriptManager>
        <div>
        <Samples:ClientVerification runat="server" ></Samples:ClientVerification>
        </div>
    </form>
</body>
</html>
