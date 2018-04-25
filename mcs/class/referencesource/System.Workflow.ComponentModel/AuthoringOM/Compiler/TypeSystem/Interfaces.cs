namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface ITypeProvider
    {
        Type GetType(string name);
        Type GetType(string name, bool throwOnError);
        Type[] GetTypes();
        Assembly LocalAssembly { get; }
        ICollection<Assembly> ReferencedAssemblies { get; }
        IDictionary<object, Exception> TypeLoadErrors { get; }
        event EventHandler TypesChanged;
        event EventHandler TypeLoadErrorsChanged;
    }
}
