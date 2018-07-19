//------------------------------------------------------------------------------
// <copyright file="ValueCollectionParameterReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Services;
    using System.Text;
    using System.Security.Permissions;


    /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class ValueCollectionParameterReader : MimeParameterReader {
        ParameterInfo[] paramInfos;

        /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader.Initialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Initialize(object o) {
            paramInfos = (ParameterInfo[])o;
        }

        /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader.GetInitializer"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            if (!IsSupported(methodInfo)) return null;
            return methodInfo.InParameters;
        }

        /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected object[] Read(NameValueCollection collection) {
            object[] parameters = new object[paramInfos.Length];
            for (int i = 0; i < paramInfos.Length; i++) {
                ParameterInfo paramInfo = paramInfos[i];
                if (paramInfo.ParameterType.IsArray) {
                    string[] arrayValues = collection.GetValues(paramInfo.Name);
                    Type arrayType = paramInfo.ParameterType.GetElementType();
                    Array array = Array.CreateInstance(arrayType, arrayValues.Length);
                    for (int j = 0; j < arrayValues.Length; j++) {
                        string value = arrayValues[j];
                        array.SetValue(ScalarFormatter.FromString(value, arrayType), j);
                    }
                    parameters[i] = array;
                }
                else {
                    string value = collection[paramInfo.Name];
                    if (value == null) throw new InvalidOperationException(Res.GetString(Res.WebMissingParameter, paramInfo.Name));
                    parameters[i] = ScalarFormatter.FromString(value, paramInfo.ParameterType);
                }
            }
            return parameters;
        }

        /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader.IsSupported"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        static public bool IsSupported(LogicalMethodInfo methodInfo) {
            if (methodInfo.OutParameters.Length > 0)
                return false;
            ParameterInfo[] paramInfos = methodInfo.InParameters;
            for (int i = 0; i < paramInfos.Length; i++)
                if (!IsSupported(paramInfos[i]))
                    return false;
            return true;
        }

        /// <include file='doc\ValueCollectionParameterReader.uex' path='docs/doc[@for="ValueCollectionParameterReader.IsSupported1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        static public bool IsSupported(ParameterInfo paramInfo) {
            Type type = paramInfo.ParameterType;
            if (type.IsArray)
                type = type.GetElementType();
            return ScalarFormatter.IsTypeSupported(type);
        }
    }

}
