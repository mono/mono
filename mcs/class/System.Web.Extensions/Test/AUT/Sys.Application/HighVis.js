Type.registerNamespace("Demo");

// Constructor
Demo.HighVis = function(element) {

    Demo.HighVis.initializeBase(this, [element]);

    this._highMode = false;
    this._oldStyle = null;
    var element = this.get_element()
    element.innerHTML = "Set to High Visibility Mode";
    element.name = "High";
    element.title = "HighVis Button";
    element.style.backgroundColor = document.body.style.backgroundColor;
    element.style.fontSize = document.body.style.fontSize;
}
Demo.HighVis.prototype = {

    initialize: function() {

        var element = this.get_element();

        if (!element.tabIndex) element.tabIndex = 0;

        Sys.UI.DomEvent.addHandler(element, 'click', this.toggleMode);

        Demo.HighVis.callBaseMethod(this, 'initialize');

    },
    
    toggleMode: function()   {
        if (this.name == "Standard") {
            document.body.style.backgroundColor = this.style.backgroundColor;
            document.body.style.fontSize = this.style.fontSize;
            this.name = "High";
        }
        else {
            document.body.style.backgroundColor = "white"; 
            document.body.style.fontSize = "x-large";
            this.name = "Standard";
            $get('backgroundColor').value = "white"; 
            $get('fontSize').value = "x-large";
        }
        this.innerHTML = "Set to " + this.name + " Visibility mode";
    }                
}
Demo.HighVis.registerClass('Demo.HighVis', Sys.UI.Control);

// Notify ScriptManager that this is the end of the script.
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();


