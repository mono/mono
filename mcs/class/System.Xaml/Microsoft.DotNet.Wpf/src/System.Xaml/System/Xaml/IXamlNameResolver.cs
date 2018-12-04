// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.ComponentModel;

namespace System.Xaml
{
    public interface IXamlNameResolver
    {
        /// <summary>
        /// Returns true if GetFixupToken is implemented and will return a non-null token
        /// when called at this time.  For example GetFixupToken will not return a token
        /// when the Type Converter or Markup Extension is called for the second reparse.
        /// </summary>
        bool IsFixupTokenAvailable { get; }

        /// <summary>
        /// Returns a reference to the named object.
        /// </summary>
        /// <param name="name">name of object</param>
        /// <returns></returns>
        object Resolve(string name);

        /// <summary>
        /// Returns a reference to the named object.
        /// </summary>
        /// <param name="name">name of object</param>
        /// <param name="isFullyInitialized">whether or not the object has any dependencies on unresolved references</param>
        /// <returns></returns>
        object Resolve(string name, out bool isFullyInitialized);

        /// <summary>
        /// Creates a token for a Type Converter or Markup Extension to return when called
        /// from the System, for the currently unresolvable names.  When all the forward
        /// referenced names are resolvable the user code will be called back for a "reparse".
        /// </summary>
        /// <param name="names">currently unresolvable names</param>
        /// <returns></returns>
        object GetFixupToken(IEnumerable<string> names);

        /// <summary>
        /// Creates a token for a Type Converter or Markup Extension to return when called
        /// from the System, for the currently unresolvable names.  When all the forward
        /// referenced names are resolvable the user code will be called back for a "reparse".
        /// </summary>
        /// <param name="names">currently unresolvable (forward reference) names</param>
        /// <param name="canAssignDirectly">If true, do not call the user code for a reparse,
        /// instead immediately assign the resolved name reference to the target property</param>
        /// <returns></returns>
        object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly);

        IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope();

        event EventHandler OnNameScopeInitializationComplete;
    }
}
