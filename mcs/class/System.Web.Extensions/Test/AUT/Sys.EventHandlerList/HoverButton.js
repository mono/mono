
/* 

Introductory boilerplate:
The following example shows how to use the EventHandlerList class in context
of a custom ajax_current_short control. To create a custom control and 
add it to your application, see Creating Custom ajax_current_short Client 
Controls, which includes the complete control this simplified example is based 
on.

*/

// Register namespace.
Type.registerNamespace("Demo");

Demo.HoverButton = function(element) {

    Demo.HoverButton.initializeBase(this, [element]);
    
    // Create delegates in the Constructor.
    this._clickDelegate = null;
}

Demo.HoverButton.prototype = {
    
    // Bind and unbind to click event.
    add_click: function(handler) {
        this.get_events().addHandler('click', handler);
    },
    remove_click: function(handler) {
        this.get_events().removeHandler('click', handler);
    },
    
    initialize: function() {
        var element = this.get_element();
        
        // Bind handler to delegate.
        if (this._clickDelegate === null) {
            this._clickDelegate = Function.createDelegate(this, this._clickHandler);
        }
        Sys.UI.DomEvent.addHandler(element, 'click', this._clickDelegate);
        Demo.HoverButton.callBaseMethod(this, 'initialize');
    },
    _clickHandler: function(event) {
        var h = this.get_events().getHandler('click');
        if (h) h(this, Sys.EventArgs.Empty);
    },

    // Release resources before control is disposed.
    dispose: function() {
        var element = this.get_element();
        if (this._clickDelegate) {
            Sys.UI.DomEvent.removeHandler(element, 'click', this._clickDelegate);
            delete this._clickDelegate;
        }
        Demo.HoverButton.callBaseMethod(this, 'dispose');
    }
}

// Register the class.
Demo.HoverButton.registerClass('Demo.HoverButton', Sys.UI.Control);

// Notify the ScriptManager that this is the end of the script.
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();


