<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Reflection</title>
</head>

<body>
    <form id="Main" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager" />
    </form>

    <div>
        <p>This example tests the Demo.Trees.GrannySmith class 
            against various reflection APIs.</p>

        <input id="Button1" value="Check Type" 
            type="button" onclick="return OnButton1Click()" />
        <input id="Button2" value="Check Inheritance" 
            type="button" onclick="return OnButton2Click()" />
        <input id="Button3" value="Check Interface" 
            type="button" onclick="return OnButton3Click()" />
        
    </div>

    <script type="text/javascript" src="Interface.js"></script>
    <script type="text/javascript" language="JavaScript">

    var g = new Demo.Trees.GrannySmith();
    var gt = Demo.Trees.GrannySmith;
    var a = new Array(
        Demo.Trees.Apple, 
        Demo.Trees.Tree, 
        Demo.Trees.Pine,
        Demo.Trees.IFruitTree,
        Sys.IContainer);
        
    function OnButton1Click() 
    {
        for (var i = 0; i < a.length; i ++)
        {
            if (a[i].isInstanceOfType(g))
            {
                alert(gt.getName() + " is a " + a[i].getName() + ".");
            }
            else alert(gt.getName() + " is not a " + a[i].getName() + ".");
        }
    }
    
    function OnButton2Click() 
    {
        for (var i = 0; i < a.length; i ++)
        {
            if (gt.inheritsFrom(a[i]))
            {
                alert(gt.getName() + " inherits from " + a[i].getName() + ".");
            }
            else alert(gt.getName() + " does not inherit from " + a[i].getName() + ".");
        }
    }

    function OnButton3Click() 
    {
        for (var i = 0; i < a.length; i ++)
        {
            if (Type.isInterface(a[i]))
            {
                if (gt.implementsInterface(a[i]))
                {
                    alert(gt.getName() + " implements the " + a[i].getName() + " interface.");
                }
                else alert(gt.getName() + " does not implement the " + a[i].getName() + " interface.");
            }
            else alert(a[i].getName() + " is not an interface.");
        }
    }
    </script>
</body>
</html>
