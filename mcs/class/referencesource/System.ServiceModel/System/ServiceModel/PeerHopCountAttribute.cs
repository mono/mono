//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Security;

    using System.ServiceModel.Channels;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PeerHopCountAttribute : MessageHeaderAttribute
    {
        public PeerHopCountAttribute()
            : base()
        {
            base.Name = PeerStrings.HopCountElementName;
            base.Namespace = PeerStrings.HopCountElementNamespace;
            base.ProtectionLevel = ProtectionLevel.None;
            base.MustUnderstand = false;
        }
        public new bool MustUnderstand
        {
            get { return base.MustUnderstand; }
        }

        public new bool Relay
        {
            get { return base.Relay; }
        }

        public new string Actor
        {
            get { return base.Actor; }
        }

        public new string Namespace
        {
            get { return base.Namespace; }
        }

        public new string Name
        {
            get { return base.Name; }
        }

        public new ProtectionLevel ProtectionLevel
        {
            get { return base.ProtectionLevel; }
        }
    }
}
