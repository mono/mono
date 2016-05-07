//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [MessageContract(IsWrapped = false)]
    public class RefreshResponseInfo
    {
        [DataContract(Name = "RefreshResponseInfo", Namespace = PeerStrings.Namespace)]
        class RefreshResponseInfoDC
        {
            [DataMember(Name = "RegistrationLifetime")]
            public TimeSpan RegistrationLifetime;

            [DataMember(Name = "Result")]
            public RefreshResult Result;

            public RefreshResponseInfoDC(TimeSpan registrationLifetime, RefreshResult result)
            {
                this.RegistrationLifetime = registrationLifetime;
                this.Result = result;
            }
        }

        public RefreshResponseInfo() : this(TimeSpan.Zero, RefreshResult.RegistrationNotFound) { }

        public RefreshResponseInfo(TimeSpan registrationLifetime, RefreshResult result)
        {
            this.body = new RefreshResponseInfoDC(registrationLifetime, result);
        }

        public TimeSpan RegistrationLifetime
        {
            get { return body.RegistrationLifetime; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.body.RegistrationLifetime = value;
            }
        }

        public RefreshResult Result
        {
            get { return body.Result; }
            set { this.body.Result = value; }
        }

        [MessageBodyMember(Name = "RefreshResponse", Namespace = PeerStrings.Namespace)]
        RefreshResponseInfoDC body;

        public bool HasBody()
        {
            return body != null;
        }
    }
}

