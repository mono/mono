<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BoundField_Bug646505.aspx.cs" Inherits="MonoBoundFieldCompatibilityIssue._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView runat="server" ID="gridView" 
            OnInit="OnGridViewInit" 
            AutoGenerateColumns="False"
            AutoGenerateEditButton="True"
            OnRowUpdating="OnGridViewEditCancelling"
            OnRowCancelingEdit="OnGridViewEditCancelling" 
            OnRowEditing="OnGridViewRowEditing" /><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
