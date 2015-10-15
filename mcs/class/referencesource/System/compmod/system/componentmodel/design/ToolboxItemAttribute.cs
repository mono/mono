//------------------------------------------------------------------------------
// <copyright file="ToolboxItemAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// SECREVIEW: remove this attribute once 
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods", Scope="member", Target="System.ComponentModel.ToolboxItemAttribute.get_ToolboxItemType():System.Type")]

namespace System.ComponentModel {
    
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;
 
    /// <devdoc>
    ///    <para>
    ///       Specifies attributes for a toolbox item.
    ///    </para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public class ToolboxItemAttribute : Attribute {

        private Type toolboxItemType;
        private string toolboxItemTypeName;

        /// <devdoc>
        ///    <para>
        ///    Initializes a new instance of ToolboxItemAttribute and sets the type to
        ///    IComponent.
        ///    </para>
        /// </devdoc>
        public static readonly ToolboxItemAttribute Default = new ToolboxItemAttribute("System.Drawing.Design.ToolboxItem, " + AssemblyRef.SystemDrawing);

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of ToolboxItemAttribute and sets the type to
        ///    <see langword='null'/>.
        ///    </para>
        /// </devdoc>
        public static readonly ToolboxItemAttribute None = new ToolboxItemAttribute(false);

        /// <devdoc>
        ///    <para>
        ///       Gets whether the attribute is the default attribute.
        ///    </para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of ToolboxItemAttribute and
        ///       specifies if default values should be used.
        ///    </para>
        /// </devdoc>
        public ToolboxItemAttribute(bool defaultType) {
            if (defaultType) {
                toolboxItemTypeName = "System.Drawing.Design.ToolboxItem, " + AssemblyRef.SystemDrawing;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of ToolboxItemAttribute and
        ///       specifies the name of the type.
        ///    </para>
        /// </devdoc>
        public ToolboxItemAttribute(string toolboxItemTypeName) {
            string temp = toolboxItemTypeName.ToUpper(CultureInfo.InvariantCulture);
            Debug.Assert(temp.IndexOf(".DLL") == -1, "Came across: " + toolboxItemTypeName + " . Please remove the .dll extension");
            this.toolboxItemTypeName = toolboxItemTypeName;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of ToolboxItemAttribute and
        ///       specifies the type of the toolbox item.
        ///    </para>
        /// </devdoc>
        public ToolboxItemAttribute(Type toolboxItemType) {
            this.toolboxItemType = toolboxItemType;
            this.toolboxItemTypeName = toolboxItemType.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the toolbox item's type.
        ///    </para>
        /// </devdoc>
        public Type ToolboxItemType {
            get{
                if (toolboxItemType == null) {
                    if (toolboxItemTypeName != null) {
                        try {
                            toolboxItemType = Type.GetType(toolboxItemTypeName, true);
                        }
                        catch (Exception ex) {
                            throw new ArgumentException(SR.GetString(SR.ToolboxItemAttributeFailedGetType, toolboxItemTypeName), ex);
                        }
                    }
                }
                return toolboxItemType;
            }
        }

        public string ToolboxItemTypeName {
            get {
                if (toolboxItemTypeName == null) {
                    return String.Empty;
                }
                return toolboxItemTypeName;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            ToolboxItemAttribute other = obj as ToolboxItemAttribute;
            return (other != null) && (other.ToolboxItemTypeName == ToolboxItemTypeName);
        }

        public override int GetHashCode() {
            if (toolboxItemTypeName != null) {
                return toolboxItemTypeName.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}

