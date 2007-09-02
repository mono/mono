Sys.WebForms.PageRequestManager.getInstance = function Sys$WebForms$PageRequestManager$getInstance(formElement) {
    /// <param name="formElement" domElement="true"></param>
    /// <returns type="Sys.WebForms.PageRequestManager"></returns>
    var e = Function._validateParams(arguments, [
        {name: "formElement", domElement: true}
    ]);
	return formElement._pageRequestManager;
}

Sys.WebForms.PageRequestManager._initialize = function Sys$WebForms$PageRequestManager$_initialize(scriptManagerID, formElement) {
    if (formElement._pageRequestManager) {
        throw Error.invalidOperation(Sys.WebForms.Res.PRM_CannotRegisterTwice);
    }
    formElement._pageRequestManager = new Sys.WebForms.PageRequestManager();
    formElement._pageRequestManager._application = formElement._application;
    formElement._pageRequestManager._initializeInternal(scriptManagerID, formElement);
}
