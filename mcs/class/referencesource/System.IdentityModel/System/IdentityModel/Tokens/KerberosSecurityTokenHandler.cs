//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Diagnostics.Application;
using System.Runtime;
using System.Runtime.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// SecurityTokenHandler for KerberosReceiverSecurityToken.
    /// </summary>
    public class KerberosSecurityTokenHandler : SecurityTokenHandler
    {
        static string[] _tokenTypeIdentifiers = new string[] { SecurityTokenTypes.Kerberos };

        /// <summary>
        /// Creates an instance of <see cref="KerberosSecurityTokenHandler"/>
        /// </summary>
        public KerberosSecurityTokenHandler()
        {
        }

        /// <summary>
        /// Gets the settings that indicate if the handler can validate tokens.
        /// Returns true by default.
        /// </summary>
        public override bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the System.Type of the token that this SecurityTokenHandler handles.
        /// Returns type of <see cref="KerberosReceiverSecurityToken"/> by default.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(KerberosReceiverSecurityToken); }
        }

        /// <summary>
        /// Gets the Kerberos Security token type defined in WS-Security Kerberos
        /// Security Token profile.
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return _tokenTypeIdentifiers;
        }

        /// <summary>
        /// Validates a <see cref="KerberosReceiverSecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="KerberosReceiverSecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from <see cref="KerberosReceiverSecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>                
        /// <exception cref="InvalidOperationException">The <see cref="WindowsIdentity"/> of the <see cref="KerberosReceiverSecurityToken"/>is null.</exception>                
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            KerberosReceiverSecurityToken kerbToken = token as KerberosReceiverSecurityToken;
            if (kerbToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(KerberosReceiverSecurityToken)));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {
                if (kerbToken.WindowsIdentity == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4026));
                }

                // KerberosReceiveSecurityToken is disposable, best to make a copy as Dispose() nulls out the WindowsIdentity. The AuthenticationType was set when the kerbToken was created.
                WindowsIdentity wi = new WindowsIdentity(kerbToken.WindowsIdentity.Token, kerbToken.WindowsIdentity.AuthenticationType);

                // PARTIAL TRUST: will fail when adding claims, AddClaim is SecurityCritical.
                wi.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated), ClaimValueTypes.DateTime));
                wi.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Windows, ClaimValueTypes.String));

                if (this.Configuration.SaveBootstrapContext)
                {
                    wi.BootstrapContext = new BootstrapContext(token, this);
                }

                this.TraceTokenValidationSuccess(token);

                List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                identities.Add(wi);
                return identities.AsReadOnly();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }
    }
}
