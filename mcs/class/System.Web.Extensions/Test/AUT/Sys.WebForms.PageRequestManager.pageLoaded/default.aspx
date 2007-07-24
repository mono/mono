
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void LinkButton_Click(object sender, EventArgs e)
    {
        TextBox1.Text = DateTime.Now.ToString();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>PageRequestManager pageLoaded Event Example</title>
    <style type="text/css">
        body {
            font-family: Tahoma;
        }
        .FieldSetStyle
        {
            width: 300px;
            height: 100px;
        }
        .UpdatePanelContainer
        {
            width: 330px;
            height:110px;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <script type="text/javascript">
        Type.registerNamespace("ScriptLibrary");
        ScriptLibrary.BorderAnimation = function(color, duration) {
            this._color = color;
            this._duration = duration;
        }
        ScriptLibrary.BorderAnimation.prototype = {
            animatePanel: function(panelElement) {
                var s = panelElement.style;
                s.borderWidth = '1px';
                s.borderColor = this._color;
                s.borderStyle = 'solid';
                window.setTimeout(
                    function() {{ s.borderWidth = 0; }},
                    this._duration
                );
            }
        }
        ScriptLibrary.BorderAnimation.registerClass('ScriptLibrary.BorderAnimation', null);
        
        var panelUpdatedAnimation = new ScriptLibrary.BorderAnimation('blue', 1000);
        var postbackElement;
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(beginRequest);
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);
        
        function beginRequest(sender, args) {
            postbackElement = args.get_postBackElement();
        }
        function pageLoaded(sender, args) {
            var updatedPanels = args.get_panelsUpdated();
            if (typeof(postbackElement) === "undefined") {
                return;
            } 
            else if (postbackElement.id.toLowerCase().indexOf('external') > -1) {
                for (i=0; i < updatedPanels.length; i++) {            
                    panelUpdatedAnimation.animatePanel(updatedPanels[i]);
                }
            }
        
        }
        </script>

        <p>
            <asp:LinkButton ID="ExternalButton" runat="server" OnClick="LinkButton_Click">
            Update the panel with animation.
            </asp:LinkButton>
        </p>
        <hr />
        <div class="UpdatePanelContainer">
            <asp:UpdatePanel runat="server" ID="UpdatePanel1" UpdateMode="Conditional">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ExternalButton" />
                </Triggers>
                <ContentTemplate>
                    <fieldset id="FieldSet1" class="FieldSetStyle" runat="server">
                        <legend>UpdatePanel</legend>
                        <asp:TextBox runat="server" ID="TextBox1" />
                        <br />
                        <asp:LinkButton ID="InternalButton" runat="server" OnClick="LinkButton_Click">
                        Update the panel with no animation.
                        </asp:LinkButton>
                    </fieldset>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
