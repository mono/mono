//-----------------------------------------------------------------------
// <copyright file="WSTrust13ConstantsAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    internal class WSTrust13ConstantsAdapter : WSTrustConstantsAdapter
    {
        private static WSTrust13ConstantsAdapter instance;
        private static WSTrust13ElementNames trust13ElementNames;
        private static WSTrust13Actions trust13ActionNames;
        private static WSTrust13ComputedKeyAlgorithm trust13ComputedKeyAlgorithm;
        private static WSTrust13KeyTypes trust13KeyTypes;
        private static WSTrust13RequestTypes trust13RequestTypes;

        protected WSTrust13ConstantsAdapter()
        {
            NamespaceURI = WSTrust13Constants.NamespaceURI;
            Prefix = WSTrust13Constants.Prefix;
        }
        
        internal static WSTrust13ConstantsAdapter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WSTrust13ConstantsAdapter();
                }

                return instance;
            }
        }

        internal override WSTrustActions Actions
        {
            get
            {
                if (trust13ActionNames == null)
                {
                    trust13ActionNames = new WSTrust13Actions();
                }

                return trust13ActionNames;
            }
        }

        internal override WSTrustComputedKeyAlgorithm ComputedKeyAlgorithm
        {
            get
            {
                if (trust13ComputedKeyAlgorithm == null)
                {
                    trust13ComputedKeyAlgorithm = new WSTrust13ComputedKeyAlgorithm();
                }

                return trust13ComputedKeyAlgorithm;
            }
        }

        internal override WSTrustElementNames Elements
        {
            get
            {
                if (trust13ElementNames == null)
                {
                    trust13ElementNames = new WSTrust13ElementNames();
                }

                return trust13ElementNames;
            }
        }

        internal override WSTrustKeyTypes KeyTypes
        {
            get
            {
                if (trust13KeyTypes == null)
                {
                    trust13KeyTypes = new WSTrust13KeyTypes();
                }

                return trust13KeyTypes;
            }
        }

        internal override WSTrustRequestTypes RequestTypes
        {
            get
            {
                if (trust13RequestTypes == null)
                {
                    trust13RequestTypes = new WSTrust13RequestTypes();
                }

                return trust13RequestTypes;
            }
        }

        internal class WSTrust13ElementNames : WSTrustElementNames
        {
            private string keyWrapAlgorithm = WSTrust13Constants.ElementNames.KeyWrapAlgorithm;
            private string secondaryParameters = WSTrust13Constants.ElementNames.SecondaryParameters;
            private string requestSecurityTokenResponseCollection = WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection;
            private string validateTarget = WSTrust13Constants.ElementNames.ValidateTarget;

            internal string KeyWrapAlgorithm
            {
                get { return this.keyWrapAlgorithm; }
            }

            internal string SecondaryParamters
            {
                get { return this.secondaryParameters; }
            }

            internal string RequestSecurityTokenResponseCollection
            {
                get { return this.requestSecurityTokenResponseCollection; }
            }

            internal string ValidateTarget
            {
                get { return this.validateTarget; }
            }
        }

        internal class WSTrust13Actions : WSTrustActions
        {
            internal WSTrust13Actions()
            {
                Cancel = WSTrust13Constants.Actions.Cancel;
                CancelResponse = WSTrust13Constants.Actions.CancelResponse;
                Issue = WSTrust13Constants.Actions.Issue;
                IssueResponse = WSTrust13Constants.Actions.IssueResponse;
                Renew = WSTrust13Constants.Actions.Renew;
                RenewResponse = WSTrust13Constants.Actions.RenewResponse;
                RequestSecurityContextToken = WSTrust13Constants.Actions.RequestSecurityContextToken;
                RequestSecurityContextTokenCancel = WSTrust13Constants.Actions.RequestSecurityContextTokenCancel;
                RequestSecurityContextTokenResponse = WSTrust13Constants.Actions.RequestSecurityContextTokenResponse;
                RequestSecurityContextTokenResponseCancel = WSTrust13Constants.Actions.RequestSecurityContextTokenResponseCancel;
                Validate = WSTrust13Constants.Actions.Validate;
                ValidateResponse = WSTrust13Constants.Actions.ValidateResponse;
            }
        }

        internal class WSTrust13ComputedKeyAlgorithm : WSTrustComputedKeyAlgorithm
        {
            internal WSTrust13ComputedKeyAlgorithm()
            {
                Psha1 = WSTrust13Constants.ComputedKeyAlgorithms.PSHA1;
            }
        }

        internal class WSTrust13KeyTypes : WSTrustKeyTypes
        {
            internal WSTrust13KeyTypes()
            {
                Asymmetric = WSTrust13Constants.KeyTypes.Asymmetric;
                Bearer = WSTrust13Constants.KeyTypes.Bearer;
                Symmetric = WSTrust13Constants.KeyTypes.Symmetric;
            }
        }

        internal class WSTrust13RequestTypes : WSTrustRequestTypes
        {
            internal WSTrust13RequestTypes()
            {
                Cancel = WSTrust13Constants.RequestTypes.Cancel;
                Issue = WSTrust13Constants.RequestTypes.Issue;
                Renew = WSTrust13Constants.RequestTypes.Renew;
                Validate = WSTrust13Constants.RequestTypes.Validate;
            }
        }
    }
}
