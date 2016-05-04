//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Runtime.Serialization;

    [DataContract]
    public class ExceptionDetail
    {
        string helpLink;
        ExceptionDetail innerException;
        string message;
        string stackTrace;
        string type;

        public ExceptionDetail(Exception exception)
        {
            if (exception == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exception");
            }

            this.helpLink = exception.HelpLink;
            this.message = exception.Message;
            this.stackTrace = exception.StackTrace;
            this.type = exception.GetType().ToString();

            if (exception.InnerException != null)
            {
                this.innerException = new ExceptionDetail(exception.InnerException);
            }
        }

        [DataMember]
        public string HelpLink
        {
            get { return this.helpLink; }
            set { this.helpLink = value; }
        }

        [DataMember]
        public ExceptionDetail InnerException
        {
            get { return this.innerException; }
            set { this.innerException = value; }
        }

        [DataMember]
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        [DataMember]
        public string StackTrace
        {
            get { return this.stackTrace; }
            set { this.stackTrace = value; }
        }

        [DataMember]
        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", SR.GetString(SR.SFxExceptionDetailFormat), this.ToStringHelper(false));
        }

        string ToStringHelper(bool isInner)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}: {1}", this.Type, this.Message);
            if (this.InnerException != null)
            {
                sb.AppendFormat(" ----> {0}", this.InnerException.ToStringHelper(true));
            }
            else
            {
                sb.Append("\n");
            }
            sb.Append(this.StackTrace);
            if (isInner)
            {
                sb.AppendFormat("\n   {0}\n", SR.GetString(SR.SFxExceptionDetailEndOfInner));
            }
            return sb.ToString();
        }
    }
}

