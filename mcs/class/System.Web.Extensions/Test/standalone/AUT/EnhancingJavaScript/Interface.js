Type.registerNamespace("Demo.Trees");

Demo.Trees.IFruitTree = function() {}
Demo.Trees.IFruitTree.Prototype = {
    bearFruit: function(){}
}
Demo.Trees.IFruitTree.registerInterface('Demo.Trees.IFruitTree');


Demo.Trees.Tree = function(name) {
    this._name = name;
}
Demo.Trees.Tree.prototype = {
    returnName: function() {
        return this._name;
    },
    
    toStringCustom: function() {
        return this.returnName();
    },
    
    makeLeaves: function() {}
}
Demo.Trees.Tree.registerClass('Demo.Trees.Tree');


Demo.Trees.FruitTree = function(name, description) {
    Demo.Trees.FruitTree.initializeBase(this, [name]);
    this._description = description;
}
Demo.Trees.FruitTree.prototype.bearFruit = function() {
        return this._description;
}
Demo.Trees.FruitTree.registerClass('Demo.Trees.FruitTree', Demo.Trees.Tree, Demo.Trees.IFruitTree);

Demo.Trees.Apple = function() {
    Demo.Trees.Apple.initializeBase(this, ['Apple', 'red and crunchy']);
}
Demo.Trees.Apple.prototype = {
    makeLeaves: function() {
        alert('Medium-sized and desiduous');
    },
    toStringCustom: function() {
        return 'FruitTree ' + Demo.Trees.Apple.callBaseMethod(this, 'toStringCustom');
    }
}
Demo.Trees.Apple.registerClass('Demo.Trees.Apple', Demo.Trees.FruitTree);

Demo.Trees.GrannySmith = function() {
    Demo.Trees.GrannySmith.initializeBase(this);
    // You must set the _description feild after initializeBase
    // or you will get the base value.
    this._description = 'green and sour';
}
Demo.Trees.GrannySmith.prototype.toStringCustom = function() {
    return Demo.Trees.GrannySmith.callBaseMethod(this, 'toStringCustom') + ' ... its GrannySmith!';
}
Demo.Trees.GrannySmith.registerClass('Demo.Trees.GrannySmith', Demo.Trees.Apple);


Demo.Trees.Banana = function(description) {
    Demo.Trees.Banana.initializeBase(this, ['Banana', 'yellow and squishy']);
}
Demo.Trees.Banana.prototype.makeLeaves = function() {
    alert('Big and green');
}
Demo.Trees.Banana.registerClass('Demo.Trees.Banana', Demo.Trees.FruitTree);



Demo.Trees.Pine = function() {
    Demo.Trees.Pine.initializeBase(this, ['Pine']);
}
Demo.Trees.Pine.prototype.makeLeaves = function() {
    alert('Needles in clusters');
}
Demo.Trees.Pine.registerClass('Demo.Trees.Pine', Demo.Trees.Tree);

