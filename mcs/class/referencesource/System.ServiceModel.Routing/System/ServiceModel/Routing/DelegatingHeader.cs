//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Runtime;

    class DelegatingHeader : MessageHeader
    {
        int index;
        MessageHeaderInfo info;
        MessageHeaders originalHeaders;

        public DelegatingHeader(MessageHeaderInfo info, MessageHeaders originalHeaders)
        {
            Fx.Assert(info != null, "info is required");
            Fx.Assert(originalHeaders != null, "originalHeaders is required");

            this.info = info;
            this.originalHeaders = originalHeaders;
            this.index = -1;
        }

        void EnsureIndex()
        {
            if (this.index < 0)
            {
                this.index = this.originalHeaders.FindHeader(this.info.Name, this.info.Namespace);
                if (this.index < 0)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SourceHeaderNotFound(this.info.Name, this.info.Namespace)));
                }
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.EnsureIndex();
            this.originalHeaders.WriteHeaderContents(index, writer);
            this.index = -1;
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.EnsureIndex();
            this.originalHeaders.WriteStartHeader(this.index, writer);
        }

        public override string Name
        {
            get { return this.info.Name; }
        }

        public override string Namespace
        {
            get { return this.info.Namespace; }
        }

        public override bool MustUnderstand
        {
            get { return this.info.MustUnderstand; }
        }

        public override string Actor
        {
            get { return this.info.Actor; }
        }

        public override bool IsReferenceParameter
        {
            get { return this.info.IsReferenceParameter; }
        }

        public override bool Relay
        {
            get { return base.Relay; }
        }
    }
}
