//------------------------------------------------------------------------------
// <copyright file="HelpKeywordAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.ComponentModel.Design {
    using System;
    using System.Security.Permissions;

    /// <devdoc>
    /// Allows specification of the context keyword that will be specified for this class or member.  By default,
    /// the help keyword for a class is the Type's full name, and for a member it's the full name of the type that declared the property,
    /// plus the property name itself.
    ///
    /// For example, consider System.Windows.Forms.Button and it's Text property:
    ///
    /// The class keyword is "System.Windows.Forms.Button", but the Text property keyword is "System.Windows.Forms.Control.Text", because the Text
    /// property is declared on the System.Windows.Forms.Control class rather than the Button class itself; the Button class inherits the property.  
    /// By contrast, the DialogResult property is declared on the Button so its keyword would be "System.Windows.Forms.Button.DialogResult".
    ///
    /// When the help system gets the keywords, it will first look at this attribute.  At the class level, it will return the string specified by the 
    /// HelpContextAttribute.  Note this will not be used for members of the Type in question.  They will still reflect the declaring Type's actual
    /// full name, plus the member name.  To override this, place the attribute on the member itself.
    ///
    /// Example:
    ///
    /// [HelpKeywordAttribute(typeof(Component))] 
    /// public class MyComponent : Component {
    /// 
    /// 
    ///     public string Property1 { get{return "";};
    ///
    ///     [HelpKeywordAttribute("SomeNamespace.SomeOtherClass.Property2")]
    ///     public string Property2 { get{return "";};
    ///
    /// }
    ///
    ///
    /// For the above class (default without attribution):
    ///
    /// Class keyword: "System.ComponentModel.Component" ("MyNamespace.MyComponent')
    /// Property1 keyword: "MyNamespace.MyComponent.Property1" (default)
    /// Property2 keyword: "SomeNamespace.SomeOtherClass.Property2" ("MyNamespace.MyComponent.Property2")
    ///
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Serializable]
    public sealed class HelpKeywordAttribute : Attribute {

        /// <devdoc>
        /// Default value for HelpKeywordAttribute, which is null.  
        /// </devdoc>
        public static readonly HelpKeywordAttribute Default = new HelpKeywordAttribute();   

        private string contextKeyword;

        /// <devdoc>
        /// Default constructor, which creates an attribute with a null HelpKeyword.
        /// </devdoc>
        public HelpKeywordAttribute() {
        }

        /// <devdoc>
        /// Creates a HelpKeywordAttribute with the value being the given keyword string.
        /// </devdoc>
        public HelpKeywordAttribute(string keyword) {
            if (keyword == null) {
                throw new ArgumentNullException("keyword");
            }
            this.contextKeyword = keyword;
        }

        /// <devdoc>
        /// Creates a HelpKeywordAttribute with the value being the full name of the given type.
        /// </devdoc>
        public HelpKeywordAttribute(Type t) {
            if (t == null) {
                throw new ArgumentNullException("t");
            }
            this.contextKeyword = t.FullName;
        }

        /// <devdoc>
        /// Retrieves the HelpKeyword this attribute supplies.
        /// </devdoc>
        public string HelpKeyword {
            get {
                return contextKeyword;
            }
        }


        /// <devdoc>
        /// Two instances of a HelpKeywordAttribute are equal if they're HelpKeywords are equal.
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if ((obj != null) && (obj is HelpKeywordAttribute)) {
                return ((HelpKeywordAttribute)obj).HelpKeyword == HelpKeyword;
            }

            return false;
        }

        /// <devdoc>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <devdoc>
        /// Returns true if this Attribute's HelpKeyword is null.
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}

