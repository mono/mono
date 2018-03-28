//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;

using System.ServiceModel.Channels;

using System.ServiceModel;

using System.ServiceModel.Description;
using System.Xml;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Globalization;

namespace System.ServiceModel.Security.Tokens
{
    public sealed class RecipientServiceModelSecurityTokenRequirement : ServiceModelSecurityTokenRequirement 
    {
        public RecipientServiceModelSecurityTokenRequirement()
            : base()
        {
            Properties.Add(IsInitiatorProperty, (object)false);
        }

        public Uri ListenUri
        {
            get 
            {
                return GetPropertyOrDefault<Uri>(ListenUriProperty, null);
            }
            set 
            {
                this.Properties[ListenUriProperty] = value;
            }
        }

        public AuditLogLocation AuditLogLocation
        {
            get
            {
                return GetPropertyOrDefault<AuditLogLocation>(AuditLogLocationProperty, ServiceSecurityAuditBehavior.defaultAuditLogLocation);
            }
            set
            {
                this.Properties[AuditLogLocationProperty] = value;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return GetPropertyOrDefault<bool>(SuppressAuditFailureProperty, ServiceSecurityAuditBehavior.defaultSuppressAuditFailure);
            }
            set
            {
                this.Properties[SuppressAuditFailureProperty] = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return GetPropertyOrDefault<AuditLevel>(MessageAuthenticationAuditLevelProperty, ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel);
            }
            set
            {
                this.Properties[MessageAuthenticationAuditLevelProperty] = value;
            }
        }

        public override string ToString()
        {
            return InternalToString();
        }
    }
}
