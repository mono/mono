<%@ Page Language="C#" %>
<%@ Register Namespace="SamplesCS" TagPrefix="Samples" Assembly="SystemWebExtensionsAUT" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>CreateContentTemplateContainer Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1"
                           runat="server" />
        <Samples:CustomUpdatePanel ID="UpdatePanel1"
                                   UpdateMode="Conditional"
                                   GroupingText="This is an UpdatePanel."
                                   runat="server">
            <ContentTemplate>
                <asp:Calendar ID="Calendar1"
                              runat="server" />
            </ContentTemplate>
        </Samples:CustomUpdatePanel>
    </div>
    </form>
</body>
</html>
