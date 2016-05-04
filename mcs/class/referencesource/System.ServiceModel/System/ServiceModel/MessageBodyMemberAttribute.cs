//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    [AttributeUsage(ServiceModelAttributeTargets.MessageMember, Inherited = false)]
    public class MessageBodyMemberAttribute : MessageContractMemberAttribute
    {
        int order = -1;
        internal const string OrderPropertyName = "Order";
        public int Order
        {
            get { return order; }
            set
            {
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                order = value;
            }
        }

    }
}

