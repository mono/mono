/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

namespace Microsoft.Scripting {
    /// <summary>
    /// This attribute is used to mark a parameter that can accept any keyword parameters that
    /// are not bound to normal arguments.  The extra keyword parameters will be
    /// passed in a dictionary which is created for the call.
    /// 
    /// Most languages which support params dictionaries will support the following types:
    ///     IDictionary&lt;string, anything&gt;
    ///     IDictionary&lt;object, anything&gt;
    ///     Dictionary&lt;string, anything&gt;
    ///     Dictionary&lt;object, anything&gt;
    ///     IDictionary
    ///     IAttributesCollection (deprecated)
    /// 
    /// For languages which don't have language level support the user will be required to
    /// create and populate the dictionary by hand.
    /// 
    /// This attribute is the dictionary equivalent of the System.ParamArrayAttribute.
    /// </summary>
    /// <example>
    /// public static void KeywordArgFunction([ParamsDictionary]IDictionary&lt;string, object&gt; dict) {
    ///     foreach (var v in dict) {
    ///         Console.WriteLine("Key: {0} Value: {1}", v.Key, v.Value);
    ///     }
    /// }
    /// 
    /// Called from Python:
    /// 
    /// KeywordArgFunction(a = 2, b = "abc")
    /// 
    /// will print:
    ///     Key: a Value = 2
    ///     Key: b Value = abc
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ParamDictionaryAttribute : Attribute {
    }
}
