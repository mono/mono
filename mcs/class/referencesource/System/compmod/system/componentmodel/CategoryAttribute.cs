//------------------------------------------------------------------------------
// <copyright file="CategoryAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies the category in which the property or event will be displayed in a
    ///       visual designer.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute {
        private static volatile CategoryAttribute appearance;
        private static volatile CategoryAttribute asynchronous;
        private static volatile CategoryAttribute behavior;
        private static volatile CategoryAttribute data;
        private static volatile CategoryAttribute design;
        private static volatile CategoryAttribute action;
        private static volatile CategoryAttribute format;
        private static volatile CategoryAttribute layout;
        private static volatile CategoryAttribute mouse;
        private static volatile CategoryAttribute key;
        private static volatile CategoryAttribute focus;
        private static volatile CategoryAttribute windowStyle;
        private static volatile CategoryAttribute dragDrop;
        private static volatile CategoryAttribute defAttr;
        
        private bool localized;

        /// <devdoc>
        ///    <para>
        ///       Provides the actual category name.
        ///    </para>
        /// </devdoc>
        private string categoryValue;

        /// <devdoc>
        ///    <para>Gets the action category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Action {
            get {
                if (action == null) {
                    action = new CategoryAttribute("Action");
                }
                return action;
            }
        }

        /// <devdoc>
        ///    <para>Gets the appearance category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Appearance {
            get {
                if (appearance == null) {
                    appearance = new CategoryAttribute("Appearance");
                }
                return appearance;
            }
        }

        /// <devdoc>
        ///    <para>Gets the asynchronous category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Asynchronous {
            get {
                if (asynchronous == null) {
                    asynchronous = new CategoryAttribute("Asynchronous");
                }
                return asynchronous;
            }
        }

        /// <devdoc>
        ///    <para>Gets the behavior category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Behavior {
            get {
                if (behavior == null) {
                    behavior = new CategoryAttribute("Behavior");
                }
                return behavior;
            }
        }

        /// <devdoc>
        ///    <para>Gets the data category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Data {
            get {
                if (data == null) {
                    data = new CategoryAttribute("Data");
                }
                return data;
            }
        }

        /// <devdoc>
        ///    <para>Gets the default category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Default {
            get {
                if (defAttr == null) {
                    defAttr = new CategoryAttribute();
                }
                return defAttr;
            }
        }
        
        /// <devdoc>
        ///    <para>Gets the design category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Design {
            get {
                if (design == null) {
                    design = new CategoryAttribute("Design");
                }
                return design;
            }
        }

        /// <devdoc>
        ///    <para>Gets the drag and drop category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute DragDrop {
            get {
                if (dragDrop == null) {
                    dragDrop = new CategoryAttribute("DragDrop");
                }
                return dragDrop;
            }
        }

        /// <devdoc>
        ///    <para>Gets the focus category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Focus {
            get {
                if (focus == null) {
                    focus = new CategoryAttribute("Focus");
                }
                return focus;
            }
        }

        /// <devdoc>
        ///    <para>Gets the format category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Format {
            get {
                if (format == null) {
                    format = new CategoryAttribute("Format");
                }
                return format;
            }
        }

        /// <devdoc>
        ///    <para>Gets the keyboard category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Key {
            get {
                if (key == null) {
                    key = new CategoryAttribute("Key");
                }
                return key;
            }
        }

        /// <devdoc>
        ///    <para>Gets the layout category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Layout {
            get {
                if (layout == null) {
                    layout = new CategoryAttribute("Layout");
                }
                return layout;
            }
        }

        /// <devdoc>
        ///    <para>Gets the mouse category attribute.</para>
        /// </devdoc>
        public static CategoryAttribute Mouse {
            get {
                if (mouse == null) {
                    mouse = new CategoryAttribute("Mouse");
                }
                return mouse;
            }
        }

        /// <devdoc>
        ///    <para> Gets the window style category 
        ///       attribute.</para>
        /// </devdoc>
        public static CategoryAttribute WindowStyle {
            get {
                if (windowStyle == null) {
                    windowStyle = new CategoryAttribute("WindowStyle");
                }
                return windowStyle;
            }
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.CategoryAttribute'/> 
        /// class with the default category.</para>
        /// </devdoc>
        public CategoryAttribute() : this("Default") {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.CategoryAttribute'/> class with
        ///    the specified category name.</para>
        /// </devdoc>
        public CategoryAttribute(string category) {
            this.categoryValue = category;
            this.localized = false;
        }

        /// <devdoc>
        ///    <para>Gets the name of the category for the property or event 
        ///       that this attribute is bound to.</para>
        /// </devdoc>
        public string Category {
            get {
                if (!localized) {
                    localized = true;
                    string localizedValue = GetLocalizedString(categoryValue);
                    if (localizedValue != null) {
                        categoryValue = localizedValue;
                    }
                }
                return categoryValue;
            }
        }

        /// <devdoc>
        /// </devdoc>
        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        /// <internalonly/>
        public override bool Equals(object obj){
            if (obj == this) {
                return true;
            }

            if (obj is CategoryAttribute){
                return Category.Equals(((CategoryAttribute)obj).Category);
            }
            return false;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return Category.GetHashCode();
        }

        /// <devdoc>
        ///    <para>Looks up the localized name of a given category.</para>
        /// </devdoc>
        protected virtual string GetLocalizedString(string value) {
#if !SILVERLIGHT
            return (string)SR.GetObject("PropertyCategory" + value);
#else
            bool usedFallback;
            string localizedString = SR.GetString("PropertyCategory" + value, out usedFallback);
            if (usedFallback) {
                return null;
            }
            return localizedString;
#endif
        }
        
#if !SILVERLIGHT
        /// <devdoc>
        /// </devdoc>
        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return Category.Equals(Default.Category);
        }
#endif
    }
}

