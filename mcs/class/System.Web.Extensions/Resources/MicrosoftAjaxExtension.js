Sys._Application.prototype.getForm = function Sys$_Application$getForm() {
	return this._form;
}

Sys.Application._form = {
	_application : Sys.Application
}

Sys.Application = {
	_globalInstance : Sys.Application,
	notifyScriptLoaded : Sys.Application.notifyScriptLoaded,

	getInstance : function Sys$Application$getInstance(formElement) {
		/// <param name="formElement" domElement="true"></param>
		/// <returns type="Sys.Application"></returns>
		var e = Function._validateParams(arguments, [
			{name: "formElement", domElement: true}
		]);
		return formElement._application;
	},

	_initialize : function Sys$Application$_initialize(formElement) {
		if (formElement._application) {
			throw Error.invalidOperation('The Application cannot be initialized more than once.');
		}
		formElement._application = new Sys._Application();
		formElement._application._form = formElement;
	}
}

Sys.Component.prototype.registerWithApplication = function Sys$Component$registerWithApplication(application) {
	/// <param name="application" type="Sys._Application"></param>
	var e = Function._validateParams(arguments, [
		{name: "application", type: Sys._Application}
	]);
	if (e) throw e;
	if(this._application)
		return;
	this._application = application;
	application.registerDisposableObject(this)
}

Sys.Component.prototype.registerAsSingleton = function Sys$Component$registerAsSingleton() {
	if (arguments.length !== 0) throw Error.parameterCount();
	this.registerWithApplication(Sys.Application._globalInstance);
}

Sys.Component.prototype.getApplication = function Sys$Component$getApplication() {
	return this._application;
}

var $create = Sys.Component.create = function Sys$Component$createWithForm(formElement, type, properties, events, references, element) {
	/// <param name="formElement" domElement="true"></param>
	/// <param name="type" type="Type"></param>
	/// <param name="properties" optional="true" mayBeNull="true"></param>
	/// <param name="events" optional="true" mayBeNull="true"></param>
	/// <param name="references" optional="true" mayBeNull="true"></param>
	/// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
	/// <returns type="Sys.UI.Component"></returns>
	var e = Function._validateParams(arguments, [
		{name: "formElement", domElement: true},
		{name: "type", type: Type},
		{name: "properties", mayBeNull: true, optional: true},
		{name: "events", mayBeNull: true, optional: true},
		{name: "references", mayBeNull: true, optional: true},
		{name: "element", mayBeNull: true, domElement: true, optional: true}
	]);
	if (e) throw e;
    
	return Sys.Component._createInternal(formElement._application, type, properties, events, references, element);
}

var $find = null;
