//------------------------------------------------------------------------------
// <copyright file="IComponent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;

    /*
     * A "component" is an object that can be placed in a container.
     *
     * In this context, "containment" refers to logical containment, not visual
     * containment.  Components and containers can be used in a variety of
     * scenarios, including both visual and non-visual scenarios.
     *
     * To be a component, a class implements the IComponent interface, and provides
     * a parameter-less constructor.
     *
     * A component interacts with its container primarily through a container-
     * provided "site".
     */

    // Interfaces don't need to be serializable
    /// <devdoc>
    ///    <para>Provides functionality required by all components.</para>
    /// </devdoc>
    [
#pragma warning disable 0618
        RootDesignerSerializer("System.ComponentModel.Design.Serialization.RootCodeDomSerializer, " + AssemblyRef.SystemDesign, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + AssemblyRef.SystemDesign, true),
#pragma warning restore 0618
        Designer("System.ComponentModel.Design.ComponentDesigner, " + AssemblyRef.SystemDesign, typeof(IDesigner)),
        Designer("System.Windows.Forms.Design.ComponentDocumentDesigner, " + AssemblyRef.SystemDesign, typeof(IRootDesigner)),
        TypeConverter(typeof(ComponentConverter)),
        System.Runtime.InteropServices.ComVisible(true)
    ]
    public interface IComponent : IDisposable {

        // The site of the component.
        /// <devdoc>
        ///    <para>When implemented by a class, gets or sets
        ///       the <see cref='System.ComponentModel.ISite'/> associated
        ///       with the <see cref='System.ComponentModel.IComponent'/>.</para>
        /// </devdoc>
        ISite Site {
            get;
            set;
        }

        /// <devdoc>
        ///    <para>Adds a event handler to listen to the Disposed event on the component.</para>
        /// </devdoc>
        event EventHandler Disposed;
    }
}
