Type.registerNamespace("Demo");

// Constructor
Demo.Question = function(element) {

    Demo.Question.initializeBase(this, [element]);

    // Create a delegate for the select event.
    this._selectDelegate = null;
}
Demo.Question.prototype = {

    // correct property accessors
    get_correct: function() {
        return this.get_element().name - 1;
    },
    set_correct: function(value) {
        this.get_element().name = value;
    },

    // Bind and unbind to select event.
    add_select: function(handler) {
        this.get_events().addHandler('select', handler);
    },
    remove_select: function(handler) {
        this.get_events().removeHandler('select', handler);
    },

    // Release resources before control is disposed.
    dispose: function() {

        var element = this.get_element();

        if (this._selectDelegate) {
            $clearHandlers(element);
            delete this._selectDelegate;
        }

        Demo.Question.callBaseMethod(this, 'dispose');
    },

    initialize: function() {

        var element = this.get_element();

        // Make sure no option is selected.
        element.value = ""; 
        
        // Attach delegate to select event.
        if (this._selectDelegate === null) {
            this._selectDelegate = Function.createDelegate(this, this._selectHandler);
        }
        Sys.UI.DomEvent.addHandler(element, 'change', this._selectDelegate);

        Demo.Question.callBaseMethod(this, 'initialize');

    },
  
    _selectHandler: function(event) {
        var h = this.get_events().getHandler('select');
        if (h) h(this, Sys.EventArgs.Empty);
    }
}
Demo.Question.registerClass('Demo.Question', Sys.UI.Control);

// Since this script is not loaded by System.Web.Handlers.ScriptResourceHandler
// invoke Sys.Application.notifyScriptLoaded to notify ScriptManager 
// that this is the end of the script.
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
