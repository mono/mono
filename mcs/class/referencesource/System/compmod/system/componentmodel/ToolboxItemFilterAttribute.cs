//------------------------------------------------------------------------------
// <copyright file="ToolboxItemFilterAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
 
    /// <devdoc>
    ///     This attribute allows you to configure a filter that is used enable or disable toolbox items
    ///     from being used on particular designers.  For example, you may have a class of components
    ///     that can only be used on a certain type of designer.  You can configure a toolbox item
    ///     filter to enforce that rule.  For example, take a report designer that uses a component
    ///     base class of ReportElement.  You may want to make ReportElement toolbox items enabled only
    ///     when a ReportDesigner is the active designer.  To do this, you would add the following
    ///     ToolboxItemFilter attributes to each class:
    ///
    ///     [ToolboxItemFilter("MyReportFilterString", ToolboxItemFilterType.Require)]
    ///     public class ReportElement : Component {}
    ///
    ///     [ToolboxItemFilter("MyReportFilterString", ToolboxItemFilterType.Require)]
    ///     public class ReportDesigner : Component {}
    ///     
    ///     These two filters specify that ReportElement toolbox items will only be 
    ///     enabled when a ReportDesigner is visible.  By specifying a filter type of
    ///     Require on the report designer class, this will disable any toolbox items
    ///     that are not report elements.  If the report designer specifed a filter type
    ///     of "Allow" instead of "Require", other components would be enabled when the
    ///     report designer was active.  ReportElements would still be disabled when 
    ///     other designers were active, however, because ReportElement requires designers
    ///     to have the given filter string.
    ///
    ///     Toolbox item filtering is a useful way to restrict toolbox item visibility to
    ///     cases where it is appropriate. This can help to avoid confusion for users, but
    ///     you should use caution not to make items unusually restrictive.  If you have a 
    ///     general purpose component, for example, you should allow the component to appear
    ///     on any designer.
    ///
    ///     The ASP.NET and Windows Forms designers both use filter attributes to prevent
    ///     each other's components from being enabled. This is a useful restriction because,
    ///     since each has several duplicate class names, it may be confusing to users and
    ///     they may not know which controls to choose.
    ///     
    /// </devdoc>
    [
    AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true),
    Serializable
    ]
    public sealed class ToolboxItemFilterAttribute : Attribute {
        private ToolboxItemFilterType filterType;
        private string                filterString;
        private string                typeId;

        /// <devdoc>
        ///     Initializes a new ToolboxItemFilterAttribute with the provide filter string and a filter type of
        ///     "Allow".
        /// </devdoc>
        public ToolboxItemFilterAttribute(string filterString) : this(filterString, ToolboxItemFilterType.Allow) {
        }
        
        /// <devdoc>
        ///     Initializes a new ToolboxItemFilterAttribute with the provide filter string and filter type.
        /// </devdoc>
        public ToolboxItemFilterAttribute(string filterString, ToolboxItemFilterType filterType) {
            if (filterString == null) filterString = String.Empty;
            
            this.filterString = filterString;
            this.filterType = filterType;
        }
        
        /// <devdoc>
        ///     Retrieves the filter string for this attribute.  The filter string is a user-defined string that
        ///     is used to identify matching attributes.
        /// </devdoc>
        public string FilterString {
            get {
                return filterString;
            }
        }
        
        /// <devdoc>
        ///     Retrieves the filter type for this attribute.  The filter type determines how the filter string should
        ///     be applied.
        /// </devdoc>
        public ToolboxItemFilterType FilterType { 
            get {
                return filterType;
            }
        }

        /// <devdoc>
        ///     The unique identifier for this attribute.  All ToolboxItemFilterAttributes with the same filter string
        ///     are considered the same, so they return the same TypeId.
        /// </devdoc>
        public override object TypeId {
            get {
                if (typeId == null) {
                    typeId = GetType().FullName + filterString;
                }
                return typeId;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            ToolboxItemFilterAttribute other = obj as ToolboxItemFilterAttribute;
            return (other != null && other.FilterType.Equals(FilterType) && other.FilterString.Equals(FilterString));
        }

        public override int GetHashCode() {
            // No need to hash on filter type as well; there shouldn't be that many duplicates.
            return filterString.GetHashCode();
        }
    
        public override bool Match(object obj) {
        
            ToolboxItemFilterAttribute other = obj as ToolboxItemFilterAttribute;
            if (other == null) {
                return false;
            }
            
            // different filter string kills a match immediately.
            //
            if (!other.FilterString.Equals(FilterString)) {
                return false;
            }
            
            return true;
        }

        public override string ToString() {
            return filterString + "," + Enum.GetName(typeof(ToolboxItemFilterType), filterType);
        }
    }
}

