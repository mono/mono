//------------------------------------------------------------------------------
// <copyright file="OleDbException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable] 
    public sealed class OleDbException : System.Data.Common.DbException {
        private OleDbErrorCollection oledbErrors;

        internal OleDbException(string message, OleDbHResult errorCode, Exception inner) : base(message, inner) {
            HResult = (int)errorCode;
            this.oledbErrors = new OleDbErrorCollection(null);
        }

        internal OleDbException(OleDbException previous, Exception inner) : base(previous.Message, inner) {
            HResult = previous.ErrorCode;
            this.oledbErrors = previous.oledbErrors;
        }   

        private OleDbException(string message, Exception inner, string source, OleDbHResult errorCode, OleDbErrorCollection errors) : base(message, inner) { // MDAC 84364
            Debug.Assert(null != errors, "OleDbException without OleDbErrorCollection");
            Source = source;
            HResult = (int)errorCode;
            this.oledbErrors = errors;
        }

        // runtime will call even if private...
        private OleDbException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
            oledbErrors = (OleDbErrorCollection) si.GetValue("oledbErrors", typeof(OleDbErrorCollection));
        }

        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        override public void GetObjectData(SerializationInfo si, StreamingContext context) { // MDAC 72003
            if (null == si) {
                throw new ArgumentNullException("si");
            }
            si.AddValue("oledbErrors", oledbErrors, typeof(OleDbErrorCollection));
            base.GetObjectData(si, context);
        }

        [
        TypeConverterAttribute(typeof(ErrorCodeConverter))
        ]
        override public int ErrorCode {
            get {
                return base.ErrorCode;
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public OleDbErrorCollection Errors {
            get {
                OleDbErrorCollection errors = this.oledbErrors;
                return ((null != errors) ? errors : new OleDbErrorCollection(null));
            }
        }

        internal bool ShouldSerializeErrors() { // MDAC 65548
            OleDbErrorCollection errors = this.oledbErrors;
            return ((null != errors) && (0 < errors.Count));
        }

        static internal OleDbException CreateException(UnsafeNativeMethods.IErrorInfo errorInfo, OleDbHResult errorCode, Exception inner) { // MDAC 84364
            OleDbErrorCollection errors = new OleDbErrorCollection(errorInfo);
            string message = null;
            string source = null;
            OleDbHResult hr = 0;

            if (null != errorInfo) {
                hr = errorInfo.GetDescription(out message);
                Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS|RET> %08X{HRESULT}, Description='%ls'\n", hr, message);

                hr = errorInfo.GetSource(out source);
                Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS|RET> %08X{HRESULT}, Source='%ls'\n", hr, source);
            }

            int count = errors.Count;
            if (0 < errors.Count) {
                StringBuilder builder = new StringBuilder();

                if ((null != message) && (message != errors[0].Message)) { // WebData 103032
                    builder.Append(message.TrimEnd(ODB.ErrorTrimCharacters)); // MDAC 73707
                    if (1 < count) {
                        builder.Append(Environment.NewLine);
                    }
                }
                for (int i = 0; i < count; ++i) {
                    if (0 < i) {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(errors[i].Message.TrimEnd(ODB.ErrorTrimCharacters)); // MDAC 73707
                }
                message = builder.ToString();
            }
            if (ADP.IsEmpty(message)) {
                message = ODB.NoErrorMessage(errorCode); // MDAC 71170
            }
            return new OleDbException(message, inner, source, errorCode, errors);
        }

        static internal OleDbException CombineExceptions(List<OleDbException> exceptions) {
            Debug.Assert(0 < exceptions.Count, "missing exceptions");
            if (1 < exceptions.Count) {
                OleDbErrorCollection errors = new OleDbErrorCollection(null);
                StringBuilder builder = new StringBuilder();
                
                foreach(OleDbException exception in exceptions) {
                    errors.AddRange(exception.Errors);
                    builder.Append(exception.Message);
                    builder.Append(Environment.NewLine);
                }
                return new OleDbException(builder.ToString(), null, exceptions[0].Source, (OleDbHResult)exceptions[0].ErrorCode, errors);
            }
            else {
                return exceptions[0];
            }                
        }

        sealed internal class ErrorCodeConverter : Int32Converter { // MDAC 68557

            // converter classes should have public ctor
            public ErrorCodeConverter() {
            }

            override public object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == null) {
                    throw ADP.ArgumentNull("destinationType");
                }
                if ((destinationType == typeof(string)) && (value != null) && (value is Int32)) {
                    return ODB.ELookup((OleDbHResult) value);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
