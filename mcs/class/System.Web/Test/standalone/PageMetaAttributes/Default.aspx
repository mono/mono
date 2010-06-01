<%@ Page Language="C#" AutoEventWireup="true"
 MetaDescription="<%$ Resources:TestStrings, MetaDescription %>"
 MetaKeywords="<%$ Resources:TestStrings, MetaKeywords %>"
 Title="Test"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<%= AppDomain.CurrentDomain.GetData ("BEGIN_CODE_MARKER") %><head runat="server">
    <title></title>
</head><%= AppDomain.CurrentDomain.GetData ("END_CODE_MARKER") %>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
