<%@ Control Language="C#" CodeFile="MyParent.ascx.cs" Inherits="MyParent" %>
<%@ Register Src="MyChild.ascx" TagName="MyChild" TagPrefix="uc1" %>
<uc1:MyChild id="Child1" runat="server" skinid="login"/>
