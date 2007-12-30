//-----------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------
// MicrosoftAjaxTimer.js
// Sys.UI._Timer component
Sys.UI._Timer = function Sys$UI$_Timer(element) {
    Sys.UI._Timer.initializeBase(this,[element]);
    this._interval = 60000;
    this._enabled = true;
    this._postbackPending = false;
    this._raiseTickDelegate = null;
    this._endRequestHandlerDelegate = null;
    this._timer = null;
    this._pageRequestManager = null;
    this._uniqueID = null;
}
    function Sys$UI$_Timer$get_enabled() {
        /// <value type="Boolean" locid="P:J#Sys.UI._Timer.enabled"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._enabled;
    }
    function Sys$UI$_Timer$set_enabled(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
        this._enabled = value;
    }
    function Sys$UI$_Timer$get_interval() {
        /// <value type="Number" locid="P:J#Sys.UI._Timer.interval"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._interval;
    }
    function Sys$UI$_Timer$set_interval(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;
        this._interval = value;
    }
    function Sys$UI$_Timer$get_uniqueID(){
        /// <value type="String" locid="P:J#Sys.UI._Timer.uniqueID"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._uniqueID;
    }
    function Sys$UI$_Timer$set_uniqueID(value){
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;
        this._uniqueID = value;
    }
    function Sys$UI$_Timer$dispose(){
       this._stopTimer();
       if(this._pageRequestManager !== null){
           this._pageRequestManager.remove_endRequest(this._endRequestHandlerDelegate);
       }
       Sys.UI._Timer.callBaseMethod(this,"dispose");
    }
    function Sys$UI$_Timer$_doPostback(){
        __doPostBack(this.get_uniqueID(),'');
    }
    function Sys$UI$_Timer$_handleEndRequest(sender, arg){
        var dataItem = arg.get_dataItems()[this.get_id()];
	    if (dataItem){
            this._update(dataItem[0],dataItem[1]);
	  	}
	  
	    if ((this._postbackPending === true) && (this._pageRequestManager !== null)&&(this._pageRequestManager.get_isInAsyncPostBack() === false)){
    	   	this._postbackPending = false;
            this._doPostback();
        }
	   
    }
    function Sys$UI$_Timer$initialize(){
        Sys.UI._Timer.callBaseMethod(this, 'initialize');
    	this._raiseTickDelegate = Function.createDelegate(this,this._raiseTick);
    	this._endRequestHandlerDelegate = Function.createDelegate(this,this._handleEndRequest);
    	if (Sys.WebForms && Sys.WebForms.PageRequestManager){
           this._pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();  
    	}
    	if (this._pageRequestManager !== null ){
    	    this._pageRequestManager.add_endRequest(this._endRequestHandlerDelegate);
    	}
        if(this.get_enabled()) {
            this._startTimer();
        }
    }
    function Sys$UI$_Timer$_raiseTick() {
        this._startTimer();
        if ((this._pageRequestManager === null) || (!this._pageRequestManager.get_isInAsyncPostBack())){
            this._doPostback();
            this._postbackPending = false;
        } 
        else {
            this._postbackPending = true;
        }
    }
    function Sys$UI$_Timer$_startTimer(){
        this._timer = window.setTimeout(Function.createDelegate(this,this._raiseTick),this.get_interval());
    }
    function Sys$UI$_Timer$_stopTimer(){
	    if (this._timer !== null){
	 	    window.clearTimeout(this._timer);
		    this._timer = null;
       } 	
    }
    function Sys$UI$_Timer$_update(enabled,interval) {
        var stopped = !this.get_enabled();
        var intervalChanged= (this.get_interval() !== interval);
	    if ((!stopped) && ((!enabled)||(intervalChanged))){
    	  	this._stopTimer();
    		stopped = true;
       	} 
    	this.set_enabled(enabled);
    	this.set_interval(interval);
    	if ((this.get_enabled()) && (stopped)){
    	    this._startTimer();
    	}
    }
Sys.UI._Timer.prototype = {
    get_enabled: Sys$UI$_Timer$get_enabled,
    set_enabled: Sys$UI$_Timer$set_enabled,
    get_interval: Sys$UI$_Timer$get_interval,
    set_interval: Sys$UI$_Timer$set_interval,
    get_uniqueID: Sys$UI$_Timer$get_uniqueID,
    set_uniqueID: Sys$UI$_Timer$set_uniqueID,
    dispose: Sys$UI$_Timer$dispose,
    _doPostback: Sys$UI$_Timer$_doPostback,
    _handleEndRequest: Sys$UI$_Timer$_handleEndRequest,
    initialize: Sys$UI$_Timer$initialize,
    _raiseTick: Sys$UI$_Timer$_raiseTick,
    _startTimer: Sys$UI$_Timer$_startTimer,
    _stopTimer: Sys$UI$_Timer$_stopTimer,
    _update: Sys$UI$_Timer$_update
}
Sys.UI._Timer.registerClass('Sys.UI._Timer', Sys.UI.Control);
if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();
