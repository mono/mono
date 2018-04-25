namespace System.Web.ModelBinding {

    //TBD: This seems to be existing for unit testing in MVC. 
    //Question1: Are we going to use this at all for extending the existing web forms unit test ??
    //Question2: If yes, does it make more sense to use UnvalidatedRequestValues rather than the Base interface and Wrapper implementation ?
    internal delegate UnvalidatedRequestValuesBase UnvalidatedRequestValuesAccessor(ModelBindingExecutionContext modelBindingExecutionContext);

}
