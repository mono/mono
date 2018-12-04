// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
// Description:
//   Provides methods used internally by the BamlReader to initialize a component
//   and connect Names and events on elements in its content. The markup compiler
//   generates an implementation of this interface for the sub-class of the root
//   markup element that it also generates.
//
//

using System.Runtime.CompilerServices;
namespace System.Windows.Markup
{
    /// <summary>
    /// Provides methods used internally by the BamlReader
    /// on compiled content.
    /// </summary>
    /// 
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IComponentConnector
    {
        /// <summary>
        /// Called by the BamlReader to attach events and Names on compiled content.
        /// </summary>
        void Connect(int connectionId, object target);

        /// <summary>
        /// Called by a component to load its compiled content.
        /// </summary>
        void InitializeComponent();
    }
}
