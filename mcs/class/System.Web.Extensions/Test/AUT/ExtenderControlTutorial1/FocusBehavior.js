// Register the namespace for the control.
Type.registerNamespace('Samples');

//
// Define the behavior properties.
//
Samples.FocusBehavior = function(element) { 
    Samples.FocusBehavior.initializeBase(this, [element]);

    this._highlightCssClass = null;
    this._nohighlightCssClass = null;
}

//
// Create the prototype for the behavior.
//

Samples.FocusBehavior.prototype = {


    initialize : function() {
        Samples.FocusBehavior.callBaseMethod(this, 'initialize');
        
        $addHandlers(this.get_element(), 
                     { 'focus' : this._onFocus,
                       'blur' : this._onBlur },
                     this);
        
        this.get_element().className = this._nohighlightCssClass;
    },
    
    dispose : function() {
        $clearHandlers(this.get_element());
        
        Samples.FocusBehavior.callBaseMethod(this, 'dispose');
    },

    //
    // Event delegates
    //
    
    _onFocus : function(e) {
        if (this.get_element() && !this.get_element().disabled) {
            this.get_element().className = this._highlightCssClass;          
        }
    },
    
    _onBlur : function(e) {
        if (this.get_element() && !this.get_element().disabled) {
            this.get_element().className = this._nohighlightCssClass;          
        }
    },


    //
    // Behavior properties
    //
    
    get_highlightCssClass : function() {
        return this._highlightCssClass;
    },

    set_highlightCssClass : function(value) {
        if (this._highlightCssClass !== value) {
            this._highlightCssClass = value;
            this.raisePropertyChanged('highlightCssClass');
        }
    },
    
    get_nohighlightCssClass : function() {
        return this._nohighlightCssClass;
    },

    set_nohighlightCssClass : function(value) {
        if (this._nohighlightCssClass !== value) {
            this._nohighlightCssClass = value;
            this.raisePropertyChanged('nohighlightCssClass');
        }
    }
}

// Optional descriptor for JSON serialization.
Samples.FocusBehavior.descriptor = {
    properties: [   {name: 'highlightCssClass', type: String},
                    {name: 'nohighlightCssClass', type: String} ]
}

// Register the class as a type that inherits from Sys.UI.Control.
Samples.FocusBehavior.registerClass('Samples.FocusBehavior', Sys.UI.Behavior);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
