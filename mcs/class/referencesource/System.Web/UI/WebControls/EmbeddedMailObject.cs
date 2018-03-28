//------------------------------------------------------------------------------
// <copyright file="EmbeddedMailObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Web.Mail;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Drawing.Design;
    using System.Web;

    [TypeConverter(typeof(EmbeddedMailObjectTypeConverter))]
    public sealed class EmbeddedMailObject {
        private string _path;
        private string _name;

        public EmbeddedMailObject() {
        }

        public EmbeddedMailObject(string name, string path) {
            Name = name;
            Path = path;
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.EmbeddedMailObject_Name),
        NotifyParentProperty(true)
        ]
        public string Name {
            get {
                return (_name != null) ? _name : String.Empty;
            }
            set {
                _name = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.EmbeddedMailObject_Path),
        Editor("System.Web.UI.Design.MailFileEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        NotifyParentProperty(true),
        UrlProperty(),
        ]
        public string Path {
            get {
                return (_path == null) ? String.Empty : _path;
            }
            set {
                _path = value;
            }
        }

        private sealed class EmbeddedMailObjectTypeConverter : TypeConverter {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string)) {
                    return "EmbeddedMailObject";
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }

}
