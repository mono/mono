//------------------------------------------------------------------------------
// <copyright file="CodeTypeDeclaration.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Represents a
    ///       class or nested class.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeTypeDeclaration : CodeTypeMember {


        private TypeAttributes attributes = Reflection.TypeAttributes.Public | Reflection.TypeAttributes.Class;
        private CodeTypeReferenceCollection baseTypes = new CodeTypeReferenceCollection();
        private CodeTypeMemberCollection members = new CodeTypeMemberCollection();
                
        private bool isEnum;    
        private bool isStruct;
        private int  populated = 0x0;
        private const int BaseTypesCollection = 0x1;
        private const int MembersCollection = 0x2;

        // Need to be made optionally serializable
        [OptionalField]
        private CodeTypeParameterCollection typeParameters;
        [OptionalField]
        private bool isPartial = false;


        /// <devdoc>
        ///    <para>
        ///       An event that will be fired the first time the BaseTypes Collection is accessed.  
        ///    </para>
        /// </devdoc>
        public event EventHandler PopulateBaseTypes;
        
        /// <devdoc>
        ///    <para>
        ///       An event that will be fired the first time the Members Collection is accessed.  
        ///    </para>
        /// </devdoc>
        public event EventHandler PopulateMembers;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeTypeDeclaration'/>.
        ///    </para>
        /// </devdoc>
        public CodeTypeDeclaration() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeTypeDeclaration'/> with the specified name.
        ///    </para>
        /// </devdoc>
        public CodeTypeDeclaration(string name) {
            Name = name;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the attributes of the class.
        ///    </para>
        /// </devdoc>
        public TypeAttributes TypeAttributes {
            get {
                return attributes;
            }
            set {
                attributes = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the base types of the class.
        ///    </para>
        /// </devdoc>
        public CodeTypeReferenceCollection BaseTypes {
            get {
                if (0 == (populated & BaseTypesCollection)) {
                    populated |= BaseTypesCollection;
                    if (PopulateBaseTypes != null) PopulateBaseTypes(this, EventArgs.Empty);
                }
                return baseTypes;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the class is a class.
        ///    </para>
        /// </devdoc>
        public bool IsClass {
            get {
                return(attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class && !isEnum && !isStruct;
            }
            set {                  
                if (value) {
                    attributes &= ~TypeAttributes.ClassSemanticsMask;
                    attributes |= TypeAttributes.Class;
                    isStruct = false;
                    isEnum = false;                       
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the class is a struct.
        ///    </para>
        /// </devdoc>
        public bool IsStruct {
            get {
                return isStruct;
            }
            set {        
                if (value) {
                    attributes &= ~TypeAttributes.ClassSemanticsMask;
                    isStruct = true;
                    isEnum = false;                       
                }                          
                else {
                    isStruct = false;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the class is an enumeration.
        ///    </para>
        /// </devdoc>
        public bool IsEnum {
            get {
                return isEnum;
            }
            set {
                if (value) {
                    attributes &= ~TypeAttributes.ClassSemanticsMask;
                    isStruct = false;
                    isEnum = true;                       
                }  
                else {
                    isEnum = false;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the class is an interface.
        ///    </para>
        /// </devdoc>
        public bool IsInterface {
            get {
                return(attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
            }
            set {
                if (value) {
                    attributes &= ~TypeAttributes.ClassSemanticsMask;
                    attributes |= TypeAttributes.Interface;
                    isStruct = false;
                    isEnum = false;                       
                }
                else {
                    attributes &= ~TypeAttributes.Interface;
                }
            }
        }

        public bool IsPartial {
            get {
                return isPartial;
            }
            set {
                isPartial = value;
                }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the class member collection members.
        ///    </para>
        /// </devdoc>
        public CodeTypeMemberCollection Members {
            get {
                if (0 == (populated & MembersCollection)) {
                    populated |= MembersCollection;
                    if (PopulateMembers != null) PopulateMembers(this, EventArgs.Empty);
                }
                return members;
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public CodeTypeParameterCollection TypeParameters {  
            get {
                if( typeParameters == null) {
                    typeParameters = new CodeTypeParameterCollection();
                }
                return typeParameters;
            }
        }
    }
}
