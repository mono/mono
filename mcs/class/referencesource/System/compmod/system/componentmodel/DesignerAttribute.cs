//------------------------------------------------------------------------------
// <copyright file="DesignerAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para>Specifies the class to use to implement design-time services.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class DesignerAttribute : Attribute {
        private readonly string designerTypeName;
        private readonly string designerBaseTypeName;
        private string typeId;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignerAttribute'/> class using the name of the type that
        ///       provides design-time services.
        ///    </para>
        /// </devdoc>
        public DesignerAttribute(string designerTypeName) {
            string temp = designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            Debug.Assert(temp.IndexOf(".DLL") == -1, "Came across: " + designerTypeName + " . Please remove the .dll extension");
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = typeof(IDesigner).FullName;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignerAttribute'/> class using the type that provides
        ///       design-time services.
        ///    </para>
        /// </devdoc>
        public DesignerAttribute(Type designerType) {
            this.designerTypeName = designerType.AssemblyQualifiedName;
            this.designerBaseTypeName = typeof(IDesigner).FullName;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignerAttribute'/> class using the designer type and the
        ///       base class for the designer.
        ///    </para>
        /// </devdoc>
        public DesignerAttribute(string designerTypeName, string designerBaseTypeName) {
            string temp = designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            Debug.Assert(temp.IndexOf(".DLL") == -1, "Came across: " + designerTypeName + " . Please remove the .dll extension");
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = designerBaseTypeName;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignerAttribute'/> class, using the name of the designer
        ///       class and the base class for the designer.
        ///    </para>
        /// </devdoc>
        public DesignerAttribute(string designerTypeName, Type designerBaseType) {
            string temp = designerTypeName.ToUpper(CultureInfo.InvariantCulture);
            Debug.Assert(temp.IndexOf(".DLL") == -1, "Came across: " + designerTypeName + " . Please remove the .dll extension");
            this.designerTypeName = designerTypeName;
            this.designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignerAttribute'/> class using the types of the designer and
        ///       designer base class.
        ///    </para>
        /// </devdoc>
        public DesignerAttribute(Type designerType, Type designerBaseType) {
            this.designerTypeName = designerType.AssemblyQualifiedName;
            this.designerBaseTypeName = designerBaseType.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the name of the base type of this designer.
        ///    </para>
        /// </devdoc>
        public string DesignerBaseTypeName {
            get {
                return designerBaseTypeName;
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets the name of the designer type associated with this designer attribute.
        ///    </para>
        /// </devdoc>
        public string DesignerTypeName {
            get {
                return designerTypeName;
            }
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       This defines a unique ID for this attribute type. It is used
        ///       by filtering algorithms to identify two attributes that are
        ///       the same type. For most attributes, this just returns the
        ///       Type instance for the attribute. DesignerAttribute overrides
        ///       this to include the type of the designer base type.
        ///    </para>
        /// </devdoc>
        public override object TypeId {
            get {
                if (typeId == null) {
                    string baseType = designerBaseTypeName;
                    int comma = baseType.IndexOf(',');
                    if (comma != -1) {
                        baseType = baseType.Substring(0, comma);
                    }
                    typeId = GetType().FullName + baseType;
                }
                return typeId;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            DesignerAttribute other = obj as DesignerAttribute;

            return (other != null) && other.designerBaseTypeName == designerBaseTypeName && other.designerTypeName == designerTypeName;
        }

        public override int GetHashCode() {
            return designerTypeName.GetHashCode() ^ designerBaseTypeName.GetHashCode();
        }
    }
}

