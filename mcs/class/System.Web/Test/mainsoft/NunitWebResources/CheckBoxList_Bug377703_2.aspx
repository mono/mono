<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<script runat="server">
void ToggleCbxl2(object o, EventArgs a)
{
    cbxl2.Enabled = !cbxl2.Enabled;
}
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head><title>Bug #377703 part 2</title></head>
<body>
<form runat="server">
<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:CheckBoxList id="cbxl2" runat="server"/><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
<asp:Button runat="server" Text="Click to toggle enable status above" OnClick="ToggleCbxl2" />
<asp:Button runat="server" Text="Click to refresh page"/>
</form>
</body>
</html>
