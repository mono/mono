//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    [AttributeUsage(ServiceModelAttributeTargets.MessageMember, AllowMultiple = false, Inherited = false)]
    public class MessageHeaderAttribute : MessageContractMemberAttribute
    {
        bool mustUnderstand;
        bool isMustUnderstandSet;
        bool relay;
        bool isRelaySet;
        string actor;

        public bool MustUnderstand
        {
            get { return mustUnderstand; }
            set { mustUnderstand = value; isMustUnderstandSet = true; }
        }

        public bool Relay
        {
            get { return relay; }
            set { relay = value; isRelaySet = true; }
        }

        public string Actor
        {
            get { return actor; }
            set { actor = value; }
        }

        internal bool IsMustUnderstandSet
        {
            get { return isMustUnderstandSet; }
        }

        internal bool IsRelaySet
        {
            get { return isRelaySet; }
        }
    }
}
