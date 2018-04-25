//------------------------------------------------------------------------------
// <copyright file="RegexRunnerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// This RegexRunnerFactory class is a base class for compiled regex code.
// we need to compile a factory because Type.CreateInstance is much slower
// than calling the constructor directly.

namespace System.Text.RegularExpressions {

    using System.ComponentModel;

#if !SILVERLIGHT   
    /// <internalonly/>
    [ EditorBrowsable(EditorBrowsableState.Never) ]
#endif
#if !SILVERLIGHT
    abstract public class RegexRunnerFactory {
#else
    abstract internal class RegexRunnerFactory {
#endif
        protected RegexRunnerFactory() {}
        abstract protected internal RegexRunner CreateInstance();
    }

}

