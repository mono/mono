//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    [Serializable]
    [KnownType(typeof(FaultException.FaultCodeData))]
    [KnownType(typeof(FaultException.FaultCodeData[]))]
    [KnownType(typeof(FaultException.FaultReasonData))]
    [KnownType(typeof(FaultException.FaultReasonData[]))]
    public class FaultException : CommunicationException
    {
        internal const string Namespace = "http://schemas.xmlsoap.org/Microsoft/WindowsCommunicationFoundation/2005/08/Faults/";

        string action;
        FaultCode code;
        FaultReason reason;
        MessageFault fault;

        public FaultException()
            : base(SR.GetString(SR.SFxFaultReason))
        {
            this.code = FaultException.DefaultCode;
            this.reason = FaultException.DefaultReason;
        }

        public FaultException(string reason)
            : base(reason)
        {
            this.code = FaultException.DefaultCode;
            this.reason = FaultException.CreateReason(reason);
        }

        public FaultException(FaultReason reason)
            : base(FaultException.GetSafeReasonText(reason))
        {
            this.code = FaultException.DefaultCode;
            this.reason = FaultException.EnsureReason(reason);
        }

        public FaultException(string reason, FaultCode code)
            : base(reason)
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.CreateReason(reason);
        }

        public FaultException(FaultReason reason, FaultCode code)
            : base(FaultException.GetSafeReasonText(reason))
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.EnsureReason(reason);
        }

        public FaultException(string reason, FaultCode code, string action)
            : base(reason)
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.CreateReason(reason);
            this.action = action;
        }

        internal FaultException(string reason, FaultCode code, string action, Exception innerException)
            : base(reason, innerException)
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.CreateReason(reason);
            this.action = action;
        }

        public FaultException(FaultReason reason, FaultCode code, string action)
            : base(FaultException.GetSafeReasonText(reason))
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.EnsureReason(reason);
            this.action = action;
        }

        internal FaultException(FaultReason reason, FaultCode code, string action, Exception innerException)
            : base(FaultException.GetSafeReasonText(reason), innerException)
        {
            this.code = FaultException.EnsureCode(code);
            this.reason = FaultException.EnsureReason(reason);
            this.action = action;
        }

        public FaultException(MessageFault fault)
            : base(FaultException.GetSafeReasonText(GetReason(fault)))
        {
            if (fault == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");

            this.code = FaultException.EnsureCode(fault.Code);
            this.reason = FaultException.EnsureReason(fault.Reason);
            this.fault = fault;
        }

        public FaultException(MessageFault fault, string action)
            : base(FaultException.GetSafeReasonText(GetReason(fault)))
        {
            if (fault == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");

            this.code = fault.Code;
            this.reason = fault.Reason;
            this.fault = fault;
            this.action = action;
        }

        protected FaultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.code = this.ReconstructFaultCode(info, "code");
            this.reason = this.ReconstructFaultReason(info, "reason");
            this.fault = (MessageFault)info.GetValue("messageFault", typeof(MessageFault));
            this.action = (string)info.GetString("action");
        }

        public string Action
        {
            get { return this.action; }
        }

        public FaultCode Code
        {
            get { return this.code; }
        }

        static FaultReason DefaultReason
        {
            get { return new FaultReason(SR.GetString(SR.SFxFaultReason)); }
        }

        static FaultCode DefaultCode
        {
            get { return new FaultCode("Sender"); }
        }

        public override string Message
        {
            get { return FaultException.GetSafeReasonText(this.Reason); }
        }

        public FaultReason Reason
        {
            get { return this.reason; }
        }

        internal MessageFault Fault
        {
            get { return this.fault; }
        }

        internal void AddFaultCodeObjectData(SerializationInfo info, string key, FaultCode code)
        {
            info.AddValue(key, FaultCodeData.GetObjectData(code));
        }

        internal void AddFaultReasonObjectData(SerializationInfo info, string key, FaultReason reason)
        {
            info.AddValue(key, FaultReasonData.GetObjectData(reason));
        }

        static FaultCode CreateCode(string code)
        {
            return (code != null) ? new FaultCode(code) : DefaultCode;
        }

        public static FaultException CreateFault(MessageFault messageFault, params Type[] faultDetailTypes)
        {
            return CreateFault(messageFault, null, faultDetailTypes);
        }

        public static FaultException CreateFault(MessageFault messageFault, string action, params Type[] faultDetailTypes)
        {
            if (messageFault == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageFault");
            }

            if (faultDetailTypes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultDetailTypes");
            }
            DataContractSerializerFaultFormatter faultFormatter = new DataContractSerializerFaultFormatter(faultDetailTypes);
            return faultFormatter.Deserialize(messageFault, action);
        }

        public virtual MessageFault CreateMessageFault()
        {
            if (this.fault != null)
            {
                return this.fault;
            }
            else
            {
                return MessageFault.CreateFault(this.code, this.reason);
            }
        }

        static FaultReason CreateReason(string reason)
        {
            return (reason != null) ? new FaultReason(reason) : DefaultReason;
        }

#pragma warning disable 688 // This is a Level1 assembly: a Level2 [SecurityCrital] on public members are turned into [SecuritySafeCritical] + LinkDemand
        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.",
            Safe = "Replicates the LinkDemand.")]
        [SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            this.AddFaultCodeObjectData(info, "code", this.code);
            this.AddFaultReasonObjectData(info, "reason", this.reason);
            info.AddValue("messageFault", this.fault);
            info.AddValue("action", this.action);
        }
#pragma warning restore 688

        static FaultReason GetReason(MessageFault fault)
        {
            if (fault == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }
            return fault.Reason;
        }

        internal static string GetSafeReasonText(MessageFault messageFault)
        {
            if (messageFault == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageFault");

            return GetSafeReasonText(messageFault.Reason);
        }

        internal static string GetSafeReasonText(FaultReason reason)
        {
            if (reason == null)
                return SR.GetString(SR.SFxUnknownFaultNullReason0);

            try
            {
                return reason.GetMatchingTranslation(System.Globalization.CultureInfo.CurrentCulture).Text;
            }
            catch (ArgumentException)
            {
                if (reason.Translations.Count == 0)
                {
                    return SR.GetString(SR.SFxUnknownFaultZeroReasons0);
                }
                else
                {
                    return SR.GetString(SR.SFxUnknownFaultNoMatchingTranslation1, reason.Translations[0].Text);
                }
            }
        }

        static FaultCode EnsureCode(FaultCode code)
        {
            return (code != null) ? code : DefaultCode;
        }

        static FaultReason EnsureReason(FaultReason reason)
        {
            return (reason != null) ? reason : DefaultReason;
        }

        internal FaultCode ReconstructFaultCode(SerializationInfo info, string key)
        {
            FaultCodeData[] data = (FaultCodeData[])info.GetValue(key, typeof(FaultCodeData[]));
            return FaultCodeData.Construct(data);
        }

        internal FaultReason ReconstructFaultReason(SerializationInfo info, string key)
        {
            FaultReasonData[] data = (FaultReasonData[])info.GetValue(key, typeof(FaultReasonData[]));
            return FaultReasonData.Construct(data);
        }

        [Serializable]
        internal class FaultCodeData
        {
            string name;
            string ns;

            internal static FaultCode Construct(FaultCodeData[] nodes)
            {
                FaultCode code = null;

                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    code = new FaultCode(nodes[i].name, nodes[i].ns, code);
                }

                return code;
            }

            internal static FaultCodeData[] GetObjectData(FaultCode code)
            {
                FaultCodeData[] array = new FaultCodeData[FaultCodeData.GetDepth(code)];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new FaultCodeData();
                    array[i].name = code.Name;
                    array[i].ns = code.Namespace;
                    code = code.SubCode;
                }

                if (code != null)
                {
                    Fx.Assert("FaultException.FaultCodeData.GetObjectData: (code != null)");
                }
                return array;
            }

            static int GetDepth(FaultCode code)
            {
                int depth = 0;

                while (code != null)
                {
                    depth++;
                    code = code.SubCode;
                }

                return depth;
            }
        }

        [Serializable]
        internal class FaultReasonData
        {
            string xmlLang;
            string text;

            internal static FaultReason Construct(FaultReasonData[] nodes)
            {
                FaultReasonText[] reasons = new FaultReasonText[nodes.Length];

                for (int i = 0; i < nodes.Length; i++)
                {
                    reasons[i] = new FaultReasonText(nodes[i].text, nodes[i].xmlLang);
                }

                return new FaultReason(reasons);
            }

            internal static FaultReasonData[] GetObjectData(FaultReason reason)
            {
                SynchronizedReadOnlyCollection<FaultReasonText> translations = reason.Translations;
                FaultReasonData[] array = new FaultReasonData[translations.Count];

                for (int i = 0; i < translations.Count; i++)
                {
                    array[i] = new FaultReasonData();
                    array[i].xmlLang = translations[i].XmlLang;
                    array[i].text = translations[i].Text;
                }

                return array;
            }
        }
    }

    [Serializable]
    public class FaultException<TDetail> : FaultException
    {
        TDetail detail;

        public FaultException(TDetail detail)
            : base()
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason)
            : base(reason)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, FaultReason reason)
            : base(reason)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason, FaultCode code)
            : base(reason, code)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, FaultReason reason, FaultCode code)
            : base(reason, code)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, string reason, FaultCode code, string action)
            : base(reason, code, action)
        {
            this.detail = detail;
        }

        public FaultException(TDetail detail, FaultReason reason, FaultCode code, string action)
            : base(reason, code, action)
        {
            this.detail = detail;
        }

        protected FaultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.detail = (TDetail)info.GetValue("detail", typeof(TDetail));
        }

        public TDetail Detail
        {
            get { return this.detail; }
        }

        public override MessageFault CreateMessageFault()
        {
            return MessageFault.CreateFault(this.Code, this.Reason, this.detail);
        }

#pragma warning disable 688 // This is a Level1 assembly: a Level2 [SecurityCrital] on public members are turned into [SecuritySafeCritical] + LinkDemand
        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.",
            Safe = "Replicates the LinkDemand.")]
        [SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("detail", this.detail);
        }
#pragma warning restore 688

        public override string ToString()
        {
            return SR.GetString(SR.SFxFaultExceptionToString3, this.GetType(), this.Message, this.detail != null ? this.detail.ToString() : String.Empty);
        }
    }
}
