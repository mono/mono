<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Import Namespace="Tests" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        
        bool[] ba = new bool [3];
        ba [0] = true;
        gridView.DataSource = ba;
        gridView.DataBind();
    }

    protected void OnGridViewInit(object sender, EventArgs e)
    {
        CustomCheckBoxColumn column = new CustomCheckBoxColumn("1");
        column.DataField = "!";
        gridView.Columns.Add(column);
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" method="GET" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView runat="server" ID="gridView" OnInit="OnGridViewInit" AutoGenerateColumns="False" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
