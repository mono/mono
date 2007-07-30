<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml">

<head id="Head1" runat="server">
    <title>Example</title>
    <style type="text/css">
    .redBackgroundColor { 
      background-color:Red;
     }
    .blueBackgroundColor { 
      background-color:Green;
     }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="Panel1" runat="server" GroupingText="Update Panel">
                   <b>DomElement Methods</b>
                   <br />
                   <asp:Button ID="Button1" runat="server" Text="Toggle CssClass" />
                   <asp:Button ID="Button2" runat="server" Text="Remove CssClass" />
                   <p></p>
                   <b>DomElement Properties</b>
                   <br />
                   <asp:Label ID="Label1" runat="server" BackColor="Black" ForeColor="White" Text="Label1" Width="102px"></asp:Label>
                   <br />
                   <asp:Label ID="Label2" runat="server"></asp:Label>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>

<script type="text/javascript">
    // Add handler using the getElementById method
    $addHandler(Sys.UI.DomElement.getElementById("Button1"), "click", toggleCssClassMethod);
    // Add handler using the shortcut to the getElementById method
    $addHandler($get("Button2"), "click", removeCssClassMethod);

    // Add CSS class
    Sys.UI.DomElement.addCssClass($get("Button1"), "redBackgroundColor");
    Sys.UI.DomElement.addCssClass($get("Button2"), "blueBackgroundColor");

    // Method called when Button1 is clicked
    function toggleCssClassMethod(eventElement) {
       // Toggle CSS class
       Sys.UI.DomElement.toggleCssClass(eventElement.target, "redBackgroundColor");
    }
 
    // Method called when Button2 is clicked
    function removeCssClassMethod(eventElement) {
       // Remove CSS class
        Sys.UI.DomElement.removeCssClass(eventElement.target, "blueBackgroundColor");
    }


    // Get the bounds of the element
    var elementRef = $get("Label1");
    var elementBounds = Sys.UI.DomElement.getBounds(elementRef);
    var result = '';
    result += "Label1 bounds x = " + elementBounds.x + "<br/>";
    result += "Label1 bounds y = " + elementBounds.y + "<br/>";
    result += "Label1 bounds width = " + elementBounds.width + "<br/>";
    result += "Label1 bounds height = " + elementBounds.height + "<p/>";
    

    // Get the location of the element
    var elementLoc = Sys.UI.DomElement.getLocation(elementRef);
    result += "Before move - Label1 location (x,y) = (" + 
               elementLoc.x + "," + elementLoc.y + ")<br/>";
    // Move the element
    Sys.UI.DomElement.setLocation(elementRef, 100, elementLoc.y);
    elementLoc = Sys.UI.DomElement.getLocation(elementRef);
    result += "After move  - Label1 location (x,y) = (" + 
               elementLoc.x + "," + elementLoc.y + ")<br/>";

    // Prepare the results
    $get('Label2').innerHTML = result;
</script>
