//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.Runtime.Serialization;
    using System.ComponentModel;

    [Flags]
    public enum XmlDictionaryReaderQuotaTypes
    {
        MaxDepth = 0x01,
        MaxStringContentLength = 0x02,
        MaxArrayLength = 0x04,
        MaxBytesPerRead = 0x08,
        MaxNameTableCharCount = 0x10
    }

    public sealed class XmlDictionaryReaderQuotas
    {
        bool readOnly;
        int maxStringContentLength;
        int maxArrayLength;
        int maxDepth;
        int maxNameTableCharCount;
        int maxBytesPerRead;

        XmlDictionaryReaderQuotaTypes modifiedQuotas = 0x00; 

        const int DefaultMaxDepth = 32;
        const int DefaultMaxStringContentLength = 8192;
        const int DefaultMaxArrayLength = 16384;
        const int DefaultMaxBytesPerRead = 4096;
        const int DefaultMaxNameTableCharCount = 16384;

        static XmlDictionaryReaderQuotas defaultQuota = new XmlDictionaryReaderQuotas(DefaultMaxDepth, DefaultMaxStringContentLength, DefaultMaxArrayLength, DefaultMaxBytesPerRead, DefaultMaxNameTableCharCount, 0x00);
        static XmlDictionaryReaderQuotas maxQuota = new XmlDictionaryReaderQuotas(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            XmlDictionaryReaderQuotaTypes.MaxDepth | XmlDictionaryReaderQuotaTypes.MaxStringContentLength | XmlDictionaryReaderQuotaTypes.MaxArrayLength | XmlDictionaryReaderQuotaTypes.MaxBytesPerRead | XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount);

        public XmlDictionaryReaderQuotas()
        {
            defaultQuota.CopyTo(this);
        }

        XmlDictionaryReaderQuotas(int maxDepth, int maxStringContentLength, int maxArrayLength, int maxBytesPerRead, int maxNameTableCharCount, XmlDictionaryReaderQuotaTypes modifiedQuotas)
        {
            this.maxDepth = maxDepth;
            this.maxStringContentLength = maxStringContentLength;
            this.maxArrayLength = maxArrayLength;
            this.maxBytesPerRead = maxBytesPerRead;
            this.maxNameTableCharCount = maxNameTableCharCount;
            this.modifiedQuotas = modifiedQuotas; 
            MakeReadOnly();
        }

        static public XmlDictionaryReaderQuotas Max
        {
            get
            {
                return maxQuota;
            }
        }

        public void CopyTo(XmlDictionaryReaderQuotas quotas)
        {
            if (quotas == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("quotas"));
            if (quotas.readOnly)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaCopyReadOnly)));

            InternalCopyTo(quotas); 
        }

        internal void InternalCopyTo(XmlDictionaryReaderQuotas quotas)
        {
            quotas.maxStringContentLength = this.maxStringContentLength;
            quotas.maxArrayLength = this.maxArrayLength;
            quotas.maxDepth = this.maxDepth;
            quotas.maxNameTableCharCount = this.maxNameTableCharCount;
            quotas.maxBytesPerRead = this.maxBytesPerRead;
            quotas.modifiedQuotas = this.modifiedQuotas; 
        }

        [DefaultValue(DefaultMaxStringContentLength)]
        public int MaxStringContentLength
        {
            get
            {
                return maxStringContentLength;
            }
            set
            {
                if (readOnly)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaIsReadOnly, "MaxStringContentLength")));
                if (value <= 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.QuotaMustBePositive), "value"));
                
                maxStringContentLength = value;
                this.modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxStringContentLength; 
            }
        }

        [DefaultValue(DefaultMaxArrayLength)]
        public int MaxArrayLength
        {
            get
            {
                return maxArrayLength;
            }
            set
            {
                if (readOnly)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaIsReadOnly, "MaxArrayLength")));
                if (value <= 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.QuotaMustBePositive), "value"));
                
                maxArrayLength = value;
                this.modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxArrayLength; 
            }
        }

        [DefaultValue(DefaultMaxBytesPerRead)]
        public int MaxBytesPerRead
        {
            get
            {
                return maxBytesPerRead;
            }
            set
            {
                if (readOnly)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaIsReadOnly, "MaxBytesPerRead")));
                if (value <= 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.QuotaMustBePositive), "value"));
                
                maxBytesPerRead = value;
                this.modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxBytesPerRead; 
            }
        }

        [DefaultValue(DefaultMaxDepth)]
        public int MaxDepth
        {
            get
            {
                return maxDepth;
            }
            set
            {
                if (readOnly)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaIsReadOnly, "MaxDepth")));
                if (value <= 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.QuotaMustBePositive), "value"));
                
                maxDepth = value;
                this.modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxDepth; 
            }
        }

        [DefaultValue(DefaultMaxNameTableCharCount)]
        public int MaxNameTableCharCount
        {
            get
            {
                return maxNameTableCharCount;
            }
            set
            {
                if (readOnly)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QuotaIsReadOnly, "MaxNameTableCharCount")));
                if (value <= 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.QuotaMustBePositive), "value"));
                
                maxNameTableCharCount = value;
                this.modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount; 
            }
        }

        public XmlDictionaryReaderQuotaTypes ModifiedQuotas
        {
            get
            {
                return this.modifiedQuotas; 
            }
        }

        internal void MakeReadOnly()
        {
            this.readOnly = true;
        }
    }
}
