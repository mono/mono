// Register the namespace for the control.
Type.registerNamespace('Samples');

//
// Define the control properties.
//
Samples.SampleTextBox = function(element) { 
    Samples.SampleTextBox.initializeBase(this, [element]);

    this._highlightCssClass = null;
    this._nohighlightCssClass = null;
}

//
// Create the prototype for the control.
//

Samples.SampleTextBox.prototype = {


    initialize : function() {
        Samples.SampleTextBox.callBaseMethod(this, 'initialize');
        
        this._onfocusHandler = Function.createDelegate(this, this._onFocus);
        this._onblurHandler = Function.createDelegate(this, this._onBlur);

        $addHandlers(this.get_element(), 
                     { 'focus' : this._onFocus,
                       'blur' : this._onBlur },
                     this);
        
        this.get_element().className = this._nohighlightCssClass;
    },
    
    dispose : function() {
        $clearHandlers(this.get_element());
        
        Samples.SampleTextBox.callBaseMethod(this, 'dispose');
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
    // Control properties
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
Samples.SampleTextBox.descriptor = {
    properties: [   {name: 'highlightCssClass', type: String},
                    {name: 'nohighlightCssClass', type: String} ]
}

// Register the class as a type that inherits from Sys.UI.Control.
Samples.SampleTextBox.registerClass('Samples.SampleTextBox', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
