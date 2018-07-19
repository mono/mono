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
    public class RegisterResponseInfo
    {
        [DataContract(Name = "RegisterResponse", Namespace = PeerStrings.Namespace)]
        class RegisterResponseInfoDC
        {
            [DataMember(Name = "RegistrationLifetime")]
            public TimeSpan RegistrationLifetime;

            [DataMember(Name = "RegistrationId")]
            public Guid RegistrationId;

            public RegisterResponseInfoDC() { }
            public RegisterResponseInfoDC(Guid registrationId, TimeSpan registrationLifetime)
            {
                this.RegistrationLifetime = registrationLifetime;
                this.RegistrationId = registrationId;
            }
        }

        public RegisterResponseInfo(Guid registrationId, TimeSpan registrationLifetime)
        {
            body = new RegisterResponseInfoDC(registrationId, registrationLifetime);
        }

        public RegisterResponseInfo()
        {
            body = new RegisterResponseInfoDC();
        }

        public Guid RegistrationId
        {
            get { return this.body.RegistrationId; }
            set { this.body.RegistrationId = value; }
        }

        public TimeSpan RegistrationLifetime
        {
            get { return this.body.RegistrationLifetime; }
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

        [MessageBodyMember(Name = "Update", Namespace = PeerStrings.Namespace)]
        RegisterResponseInfoDC body;

        public bool HasBody()
        {
            return body != null;
        }
    }
}

