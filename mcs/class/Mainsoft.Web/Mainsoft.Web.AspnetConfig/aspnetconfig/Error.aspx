<%--
// Mainsoft.Web.AspnetConfig - Site administration utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
// Mainsoft.Web.AspnetConfig - Site administration utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Theme="" StylesheetTheme="" Inherits="Mainsoft.Web.AspnetConfig.Error" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>ASP.Net Web Application Administration</title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="font-weight: bold; font-size: 20pt; color: red">
        Error occured on this request ! Please try again !
    </div>
    <div>
        <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="Default.aspx">Administration utility home page</asp:HyperLink>
    </div>
    </form>
</body>
</html>
