<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Enumeration</title>
</head>

<body>
    <form id="Main" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager" />
    </form>

    <div>
        <p>This example creates an Enumeration of colors
            and applies them to page background.</p>

        <select id="ColorPicker" 
            onchange="ChangeColor(options[selectedIndex].value)">
            <option value="Red" label="Red">Red</option>
            <option value="Blue" label="Blue">Blue</option>
            <option value="Green" label="Green">Green</option>
            <option value="White" label="White">White</option>
        </select>
        
    </div>

    <script type="text/javascript" src="Enumeration.js"></script>
    <script type="text/javascript" language="JavaScript">

    function ChangeColor(value) 
    {
         document.body.bgColor = eval("Demo.Color." + value + ";");
    }

    </script>

</body>
</html>
