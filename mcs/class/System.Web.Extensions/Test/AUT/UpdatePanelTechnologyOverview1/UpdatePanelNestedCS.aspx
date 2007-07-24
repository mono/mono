
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanelUpdateMode Example</title>
    <style type="text/css">
    div.NestedPanel
    {
      position: relative;
      margin: 2% 5% 2% 5%;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager" 
                               runat="server" />
            <asp:UpdatePanel ID="OuterPanel" 
                             UpdateMode="Conditional" 
                             runat="server">
                <ContentTemplate>
                    <div>
                        <fieldset>
                            <legend>Outer Panel </legend>
                            <br />
                            <asp:Button ID="OPButton1" 
                                        Text="Outer Panel Button" 
                                        runat="server" />
                            <br />
                            Last updated on
                            <%= DateTime.Now.ToString() %>
                            <br />
                            <br />
                            <asp:UpdatePanel ID="NestedPanel1" 
                                               UpdateMode="Conditional"
                                               runat="server">
                                <ContentTemplate>
                                    <div class="NestedPanel">
                                        <fieldset>
                                            <legend>Nested Panel 1</legend>
                                            <br />
                                            Last updated on
                                            <%= DateTime.Now.ToString() %>
                                            <br />
                                            <asp:Button ID="NPButton1"
                                                        Text="Nested Panel 1 Button" 
                                                        runat="server" />
                                        </fieldset>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </fieldset>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
