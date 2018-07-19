//------------------------------------------------------------------------------
// <copyright file="FileLevelControlBuilderAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI
{

    using System;
    using System.Diagnostics.CodeAnalysis;


    /// <devdoc>
    /// <para>Allows a TemplateControl (e.g. Page or UserControl) derived class to specify
    //     the control builder used at the top level ofthe builder tree when parsing the file.

    /// for building that control within the ASP.NET parser.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FileLevelControlBuilderAttribute : Attribute
    {


        /// <internalonly/>
        /// <devdoc>
        /// <para>The default <see cref='System.Web.UI.FileLevelControlBuilderAttribute'/> object is a 
        /// <see langword='null'/> builder. This field is read-only.</para>
        /// </devdoc>
        public static readonly FileLevelControlBuilderAttribute Default = new FileLevelControlBuilderAttribute(null);

        private Type builderType = null;



        /// <devdoc>
        /// </devdoc>
        public FileLevelControlBuilderAttribute(Type builderType)
        {
            this.builderType = builderType;
        }


        /// <devdoc>
        ///    <para> Indicates XXX. This property is read-only.</para>
        /// </devdoc>
        public Type BuilderType
        {
            get
            {
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
            return builderType.GetHashCode();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if ((obj != null) && (obj is FileLevelControlBuilderAttribute))
            {
                return ((FileLevelControlBuilderAttribute)obj).BuilderType == builderType;
            }

            return false;
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }
    }
}
