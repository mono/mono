//------------------------------------------------------------------------------
// <copyright file="DesignerSerializerAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {
    using System.Security.Permissions;

    /// <devdoc>
    ///     This attribute can be placed on a class to indicate what serialization
    ///     object should be used to serialize the class at design time.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class DesignerSerializerAttribute : Attribute {
        private string serializerTypeName;
        private string serializerBaseTypeName;
        private string typeId;
    
        /// <devdoc>
        ///     Creates a new designer serialization attribute.
        /// </devdoc>
        public DesignerSerializerAttribute(Type serializerType, Type baseSerializerType) {
            this.serializerTypeName = serializerType.AssemblyQualifiedName;
            this.serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
        }
    
        /// <devdoc>
        ///     Creates a new designer serialization attribute.
        /// </devdoc>
        public DesignerSerializerAttribute(string serializerTypeName, Type baseSerializerType) {
            this.serializerTypeName = serializerTypeName;
            this.serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
        }
    
        /// <devdoc>
        ///     Creates a new designer serialization attribute.
        /// </devdoc>
        public DesignerSerializerAttribute(string serializerTypeName, string baseSerializerTypeName) {
            this.serializerTypeName = serializerTypeName;
            this.serializerBaseTypeName = baseSerializerTypeName;
        }
    
        /// <devdoc>
        ///     Retrieves the fully qualified type name of the serializer.
        /// </devdoc>
        public string SerializerTypeName {
            get {
                return serializerTypeName;
            }
        }
    
        /// <devdoc>
        ///     Retrieves the fully qualified type name of the serializer base type.
        /// </devdoc>
        public string SerializerBaseTypeName {
            get {
                return serializerBaseTypeName;
            }
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       This defines a unique ID for this attribute type. It is used
        ///       by filtering algorithms to identify two attributes that are
        ///       the same type. For most attributes, this just returns the
        ///       Type instance for the attribute. EditorAttribute overrides
        ///       this to include the type of the editor base type.
        ///    </para>
        /// </devdoc>
        public override object TypeId {
            get {
                if (typeId == null) {
                    string baseType = serializerBaseTypeName;
                    int comma = baseType.IndexOf(',');
                    if (comma != -1) {
                        baseType = baseType.Substring(0, comma);
                    }
                    typeId = GetType().FullName + baseType;
                }
                return typeId;
            }
        }
    }
}

