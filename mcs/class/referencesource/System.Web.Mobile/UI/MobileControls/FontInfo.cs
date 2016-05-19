//------------------------------------------------------------------------------
// <copyright file="FontInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * FontInfo class.
     * Encapsulates all of the Style font properties into a single class.
     */
    /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo"]/*' />
    [
        TypeConverterAttribute(typeof(ExpandableObjectConverter))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class FontInfo
    {

        private Style _style;

        // 


        internal FontInfo(Style style)
        {
            _style = style;
        }

        /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo.Name"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Drawing.Design.FontNameEditor), typeof(UITypeEditor)),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.FontInfo_Name),
            NotifyParentProperty(true),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.FontNameConverter)),
        ]
        public String Name
        {
            get
            {
                return _style.FontName;
            }
            set
            {
                _style.FontName = value;
            }
        }

        /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo.Bold"]/*' />
        [
            Bindable(true),
            DefaultValue(BooleanOption.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.FontInfo_Bold),
            NotifyParentProperty(true)
        ]
        public BooleanOption Bold
        {
            get
            {
                return _style.Bold;
            }
            set
            {
                _style.Bold = value;
            }
        }

        /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo.Italic"]/*' />
        [
            Bindable(true),
            DefaultValue(BooleanOption.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.FontInfo_Italic),
            NotifyParentProperty(true)
        ]
        public BooleanOption Italic
        {
            get
            {
                return _style.Italic;
            }
            set
            {
                _style.Italic = value;
            }
        }

        /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo.Size"]/*' />
        [
            Bindable(true),
            DefaultValue(FontSize.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.FontInfo_Size),
            NotifyParentProperty(true)
        ]
        public FontSize Size
        {
            get
            {
                return _style.FontSize;
            }
            set
            {
                _style.FontSize = value;
            }
        }

        /// <include file='doc\FontInfo.uex' path='docs/doc[@for="FontInfo.ToString"]/*' />
        /// <summary>
        /// </summary>
        public override String ToString()
        {
            String size = (this.Size.Equals(FontSize.NotSet) ? null : Enum.GetName(typeof(FontSize), this.Size));
            String s = this.Name;

            if (size != null)
            {
                if (s.Length != 0)
                {
                    s += ", " + size;
                }
                else {
                    s = size;
                }
            }
            return s;
        }
    }
}
