//------------------------------------------------------------------------------
// <copyright file="UrlEncodedParameterWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Text;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;

    /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class UrlEncodedParameterWriter : MimeParameterWriter {
        ParameterInfo[] paramInfos;
        int numberEncoded;
        Encoding encoding;

        /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter.RequestEncoding"]/*' />
        public override Encoding RequestEncoding {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter.GetInitializer"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            if (!ValueCollectionParameterReader.IsSupported(methodInfo)) return null;
            return methodInfo.InParameters;
        }

        /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter.Initialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Initialize(object initializer) {
            this.paramInfos = (ParameterInfo[])initializer;
        }

        /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter.Encode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void Encode(TextWriter writer, object[] values) {
            numberEncoded = 0;
            for (int i = 0; i < paramInfos.Length; i++) {
                ParameterInfo paramInfo = paramInfos[i];
                if (paramInfo.ParameterType.IsArray) {
                    Array array = (Array)values[i];
                    for (int j = 0; j < array.Length; j++) {
                        Encode(writer, paramInfo.Name, array.GetValue(j));
                    }
                }
                else {
                    Encode(writer, paramInfo.Name, values[i]);
                }
            }
        }

        /// <include file='doc\UrlEncodedParameterWriter.uex' path='docs/doc[@for="UrlEncodedParameterWriter.Encode1"]/*' />
        protected void Encode(TextWriter writer, string name, object value) {
            if (numberEncoded > 0) writer.Write('&');
            writer.Write(UrlEncode(name));
            writer.Write('=');
            writer.Write(UrlEncode(ScalarFormatter.ToString(value)));
            numberEncoded++;
        }

        string UrlEncode(string value) {
            if (encoding != null)
                return UrlEncoder.UrlEscapeString(value, encoding);
            else
                return UrlEncoder.UrlEscapeStringUnicode(value);
        }
    }

}
