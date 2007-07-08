Type.registerNamespace("Demo");

// Constructor
Demo.Section = function(element) {

    Demo.Section.initializeBase(this, [element]);
}
Demo.Section.prototype = {
    
    // Create add and remove accessors fot the complete event.
    add_complete: function(handler) {
        this.get_events().addHandler("complete", handler);
    },
    remove_complete: function(handler) {
        this.get_events().removeHandler("complete", handler);
    },
    
    // Create a function to raise the complete event.
    raiseComplete: function() {
        var h = this.get_events().getHandler('complete');
        if (h) h(this);
    },
    
    // Release resources before control is disposed.
    dispose: function() {
        var element = this.get_element();
        $clearHandlers(element);
        Demo.Section.callBaseMethod(this, 'dispose');
    }
}
Demo.Section.registerClass('Demo.Section', Sys.UI.Control);

// Since this script is not loaded by System.Web.Handlers.ScriptResourceHandler
// invoke Sys.Application.notifyScriptLoaded to notify ScriptManager 
// that this is the end of the script.
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
