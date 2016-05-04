//------------------------------------------------------------------------------
// <copyright file="ControlBuilderAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.Diagnostics.CodeAnalysis;


    /// <devdoc>
    /// <para>Allows a control to specify a custom <see cref='System.Web.UI.ControlBuilder'/> object
    ///    for building that control within the ASP.NET parser.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ControlBuilderAttribute : Attribute {


        /// <internalonly/>
        /// <devdoc>
        /// <para>The default <see cref='System.Web.UI.ControlBuilderAttribute'/> object is a 
        /// <see langword='null'/> builder. This field is read-only.</para>
        /// </devdoc>
        public static readonly ControlBuilderAttribute Default = new ControlBuilderAttribute(null);

        private Type builderType = null;



        /// <devdoc>
        /// </devdoc>
        public ControlBuilderAttribute(Type builderType) {
            this.builderType = builderType;
        }


        /// <devdoc>
        ///    <para> Indicates XXX. This property is read-only.</para>
        /// </devdoc>
        public Type BuilderType {
            get {
                return builderType;
            }
        }



        /// <internalonly/>
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "The type is a ControlBuilder which is never going to be an interop class.")]
        public override int GetHashCode()
        {
            return ((BuilderType != null) ? BuilderType.GetHashCode() : 0);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if ((obj != null) && (obj is ControlBuilderAttribute)) {
                return((ControlBuilderAttribute)obj).BuilderType == builderType;
            }

            return false;
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
