<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" %>
<%@ Register Src="~/Folder/ResolveUrl.ascx" TagName="ResolveUrl" TagPrefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
    <head>
        <title>UrlProperty test</title>
    </head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:ResolveUrl ID="WebUserControl1" runat="server"/>
    </div>
    </form>
</body>
</html>
