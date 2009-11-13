<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="TableSections_Bug551666.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Bug #551666</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
       
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView ID="GridView1" runat="server">
            
        </asp:GridView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    </form>
</body>
</html>
