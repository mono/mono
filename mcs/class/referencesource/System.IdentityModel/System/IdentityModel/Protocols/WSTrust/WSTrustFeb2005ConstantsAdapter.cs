//-----------------------------------------------------------------------
// <copyright file="WSTrustFeb2005ConstantsAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    internal class WSTrustFeb2005ConstantsAdapter : WSTrustConstantsAdapter
    {
        private static WSTrustFeb2005ConstantsAdapter instance;
        private static WSTrustFeb2005Actions trustFeb2005Actions;
        private static WSTrustFeb2005ComputedKeyAlgorithm trustFeb2005ComputedKeyAlgorithm;
        private static WSTrustFeb2005KeyTypes trustFeb2005KeyTypes;
        private static WSTrustFeb2005RequestTypes trustFeb2005RequestTypes;

        protected WSTrustFeb2005ConstantsAdapter()
        {
            NamespaceURI = WSTrustFeb2005Constants.NamespaceURI;
            Prefix = WSTrustFeb2005Constants.Prefix;
        }

        internal static WSTrustFeb2005ConstantsAdapter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WSTrustFeb2005ConstantsAdapter();
                }

                return instance;
            }
        }

        internal override WSTrustActions Actions
        {
            get
            {
                if (trustFeb2005Actions == null)
                {
                    trustFeb2005Actions = new WSTrustFeb2005Actions();
                }

                return trustFeb2005Actions;
            }
        }

        internal override WSTrustComputedKeyAlgorithm ComputedKeyAlgorithm
        {
            get
            {
                if (trustFeb2005ComputedKeyAlgorithm == null)
                {
                    trustFeb2005ComputedKeyAlgorithm = new WSTrustFeb2005ComputedKeyAlgorithm();
                }

                return trustFeb2005ComputedKeyAlgorithm;
            }
        }

        internal override WSTrustKeyTypes KeyTypes
        {
            get
            {
                if (trustFeb2005KeyTypes == null)
                {
                    trustFeb2005KeyTypes = new WSTrustFeb2005KeyTypes();
                }

                return trustFeb2005KeyTypes;
            }
        }

        internal override WSTrustRequestTypes RequestTypes
        {
            get
            {
                if (trustFeb2005RequestTypes == null)
                {
                    trustFeb2005RequestTypes = new WSTrustFeb2005RequestTypes();
                }

                return trustFeb2005RequestTypes;
            }
        }

        internal class WSTrustFeb2005Actions : WSTrustActions
        {
            internal WSTrustFeb2005Actions()
            {
                this.Cancel = WSTrustFeb2005Constants.Actions.Cancel;
                this.CancelResponse = WSTrustFeb2005Constants.Actions.CancelResponse;
                this.Issue = WSTrustFeb2005Constants.Actions.Issue;
                this.IssueResponse = WSTrustFeb2005Constants.Actions.IssueResponse;
                this.Renew = WSTrustFeb2005Constants.Actions.Renew;
                this.RenewResponse = WSTrustFeb2005Constants.Actions.RenewResponse;
                this.RequestSecurityContextToken = WSTrustFeb2005Constants.Actions.RequestSecurityContextToken;
                this.RequestSecurityContextTokenCancel = WSTrustFeb2005Constants.Actions.RequestSecurityContextTokenCancel;
                this.RequestSecurityContextTokenResponse = WSTrustFeb2005Constants.Actions.RequestSecurityContextTokenResponse;
                this.RequestSecurityContextTokenResponseCancel = WSTrustFeb2005Constants.Actions.RequestSecurityContextTokenResponseCancel;
                this.Validate = WSTrustFeb2005Constants.Actions.Validate;
                this.ValidateResponse = WSTrustFeb2005Constants.Actions.ValidateResponse;
            }
        }

        internal class WSTrustFeb2005ComputedKeyAlgorithm : WSTrustComputedKeyAlgorithm
        {
            internal WSTrustFeb2005ComputedKeyAlgorithm()
            {
                this.Psha1 = WSTrustFeb2005Constants.ComputedKeyAlgorithms.PSHA1;
            }
        }

        internal class WSTrustFeb2005KeyTypes : WSTrustKeyTypes
        {
            internal WSTrustFeb2005KeyTypes()
            {
                this.Asymmetric = WSTrustFeb2005Constants.KeyTypes.Asymmetric;
                this.Bearer = WSTrustFeb2005Constants.KeyTypes.Bearer;
                this.Symmetric = WSTrustFeb2005Constants.KeyTypes.Symmetric;
            }
        }

        internal class WSTrustFeb2005RequestTypes : WSTrustRequestTypes
        {
            internal WSTrustFeb2005RequestTypes()
            {
                this.Cancel = WSTrustFeb2005Constants.RequestTypes.Cancel;
                this.Issue = WSTrustFeb2005Constants.RequestTypes.Issue;
                this.Renew = WSTrustFeb2005Constants.RequestTypes.Renew;
                this.Validate = WSTrustFeb2005Constants.RequestTypes.Validate;
            }
        }
    }
}
