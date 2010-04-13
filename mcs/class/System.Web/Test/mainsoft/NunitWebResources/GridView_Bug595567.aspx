<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        
        gridView.DataSource = new int[3];
        gridView.DataBind();

        GridViewRow row = gridView.FooterRow;
        if (row.RowType == DataControlRowType.Footer && !gridView.ShowFooter && row.Visible)
            throw new InvalidOperationException("Unexpected state: GridView.ShowFooter is False but the Footer row is Visible!");
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
	<%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView runat="server" ID="gridView" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
