<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Interface</title>
</head>

<body>
    <form id="Main" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager" />
    </form>
    
    <h2>Interface</h2>
    <p />

    <div>
        This file contains a Tree base class, and an IFruitTree interface.
        Apple and Banana, two derived classes implement that interface, whereas,
        Pine does not implement that interface.
        <p />
    </div>

    <script type="text/javascript" src="Interface.js"></script>

    <div>
        <ul>
                <li><a href="#" onclick="return OnTestNewClick()">Object Creation</a></li>
                <li><a href="#" onclick="return OnTestImplementsClick()">Implements Check</a></li>
                <li><a href="#" onclick="return OnTestInterfaceMethodClick()">Call interface method</a></li>
        </ul>
    </div>

    <script type="text/javascript" language="JavaScript">
    
    function OnTestNewClick() {
        var apple = new Demo.Trees.Apple('Apple');
        alert(apple.returnName());
        apple.makeLeaves();

        return false;
    }
    
    function OnTestImplementsClick() {
        var apple = new Demo.Trees.Apple();
        if (Demo.Trees.IFruitTree.isImplementedBy(apple)) {
            alert('Apple implements IFruitTree');
        }
        else {
            alert('Apple does not implement IFruitTree');
        }

        var pine = new Demo.Trees.Pine();
        if (Demo.Trees.IFruitTree.isImplementedBy(pine)) {
            alert('Pine implements IFruitTree');
        }
        else {
            alert('Pine does not implement IFruitTree');
        }

        return false;
    }
    
    function OnTestInterfaceMethodClick() {
        var apple = new Demo.Trees.Apple();
        ProcessTree(apple);

        var pine = new Demo.Trees.Pine();
        ProcessTree(pine);

        var banana = new Demo.Trees.Banana();
        ProcessTree(banana);
        
        var g = new Demo.Trees.GrannySmith();
        ProcessTree(g);

        return false;
    }
    
    function ProcessTree(tree) {
        alert('Current Tree ' + tree.returnName());
        alert(tree.toStringCustom());
        if (Demo.Trees.IFruitTree.isImplementedBy(tree)) {
            alert(tree.returnName() + ' implements IFruitTree; Fruit is ' + tree.bearFruit());
        }
    }
    </script>
</body>
</html>
