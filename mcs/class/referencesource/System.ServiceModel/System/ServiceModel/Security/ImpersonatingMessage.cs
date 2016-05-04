//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.Xml;

    sealed class ImpersonatingMessage : Message
    {
        Message innerMessage;

        public ImpersonatingMessage(Message innerMessage)
        {
            if (innerMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerMessage");
            }
            this.innerMessage = innerMessage;
        }

        public override bool IsEmpty
        {
            get
            {
                return this.innerMessage.IsEmpty;
            }
        }

        public override bool IsFault
        {
            get { return this.innerMessage.IsFault; }
        }

        public override MessageHeaders Headers
        {
            get { return this.innerMessage.Headers; }
        }

        public override MessageProperties Properties
        {
            get { return this.innerMessage.Properties; }
        }

        public override MessageVersion Version
        {
            get { return this.innerMessage.Version; }
        }

        internal override RecycledMessageState RecycledMessageState
        {
            get
            {
                return this.innerMessage.RecycledMessageState;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            this.innerMessage.Close();
        }

        //Runs impersonated.
        protected override IAsyncResult OnBeginWriteMessage(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            ImpersonateOnSerializingReplyMessageProperty impersonationProperty = null;
            IDisposable impersonationContext = null;
            IPrincipal originalPrincipal = null;
            bool isThreadPrincipalSet = false;

            if (!ImpersonateOnSerializingReplyMessageProperty.TryGet(this.innerMessage, out impersonationProperty))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToImpersonateWhileSerializingReponse)));
            }

            try
            {
                impersonationProperty.StartImpersonation(out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                return this.innerMessage.BeginWriteMessage(writer, callback, state);
            }
            finally
            {
                try
                {
                    impersonationProperty.StopImpersonation(impersonationContext, originalPrincipal, isThreadPrincipalSet);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch
                {
                    string message = null;
                    try
                    {
                        message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                    }
                    finally
                    {
                        DiagnosticUtility.FailFast(message);
                    }
                }
            }
        }

        //Runs impersonated.
        protected override void OnWriteMessage(XmlDictionaryWriter writer)
        {
            this.ImpersonateCall(() => this.innerMessage.WriteMessage(writer));
        }
     
        protected override void OnEndWriteMessage(IAsyncResult result)
        {
            this.innerMessage.EndWriteMessage(result);
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartEnvelope(writer);
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartHeaders(writer);
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            this.innerMessage.WriteStartBody(writer);
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            return this.innerMessage.GetBodyAttribute(localName, ns);
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            return this.innerMessage.CreateBufferedCopy(maxBufferSize);
        }
      
        protected override IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            ImpersonateOnSerializingReplyMessageProperty impersonationProperty = null;
            IDisposable impersonationContext = null;
            IPrincipal originalPrincipal = null;
            bool isThreadPrincipalSet = false;

            if (!ImpersonateOnSerializingReplyMessageProperty.TryGet(this.innerMessage, out impersonationProperty))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToImpersonateWhileSerializingReponse)));
            }

            try
            {
                impersonationProperty.StartImpersonation(out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                return this.innerMessage.BeginWriteBodyContents(writer, callback, state);
            }
            finally
            {
                try
                {
                    impersonationProperty.StopImpersonation(impersonationContext, originalPrincipal, isThreadPrincipalSet);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch
                {
                    string message = null;
                    try
                    {
                        message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                    }
                    finally
                    {
                        DiagnosticUtility.FailFast(message);
                    }
                }
            }
            
           
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
          this.ImpersonateCall( () => this.innerMessage.WriteBodyContents(writer));
        }

        protected override void OnEndWriteBodyContents(IAsyncResult result)
        {
            this.innerMessage.EndWriteBodyContents(result);
        }

        //Runs impersonated.
        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            this.ImpersonateCall(() => this.innerMessage.BodyToString(writer));
        }

        void ImpersonateCall(Action callToImpersonate)
        {
            ImpersonateOnSerializingReplyMessageProperty impersonationProperty = null;
            IDisposable impersonationContext = null;
            IPrincipal originalPrincipal = null;
            bool isThreadPrincipalSet = false;

            if (!ImpersonateOnSerializingReplyMessageProperty.TryGet(this.innerMessage, out impersonationProperty))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToImpersonateWhileSerializingReponse)));
            }

            try
            {
                impersonationProperty.StartImpersonation(out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                callToImpersonate();
            }
            finally
            {
                try
                {
                    impersonationProperty.StopImpersonation(impersonationContext, originalPrincipal, isThreadPrincipalSet);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch
                {
                    string message = null;
                    try
                    {
                        message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                    }
                    finally
                    {
                        DiagnosticUtility.FailFast(message);
                    }
                }
            }
        }
    }
}
