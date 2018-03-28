//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IdentityModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.Xml;
using SysAuthorizationContext = System.IdentityModel.Policy.AuthorizationContext;

namespace System.ServiceModel.Security
{
    /// <summary>
    /// Custom ServiceAuthorizationManager implementation. This class substitues the WCF 
    /// generated IAuthorizationPolicies with 
    /// <see cref="System.IdentityModel.Tokens.AuthorizationPolicy"/>. These
    /// policies do not participate in the EvaluationContext and hence will render an 
    /// empty WCF AuthorizationConext. Once this AuthorizationManager is substitued to
    /// a ServiceHost, only <see cref="System.Security.Claims.ClaimsPrincipal"/>
    /// will be available for Authorization decisions.
    /// </summary>
    class IdentityModelServiceAuthorizationManager : ServiceAuthorizationManager
    {
        /// <summary>
        /// Authorization policy for anonymous authentication.
        /// </summary>
        protected static readonly ReadOnlyCollection<IAuthorizationPolicy> AnonymousAuthorizationPolicy
            = new ReadOnlyCollection<IAuthorizationPolicy>(
                new List<IAuthorizationPolicy>() { new AuthorizationPolicy(new ClaimsIdentity()) });

        /// <summary>
        /// Override of the base class method. Substitues WCF IAuthorizationPolicy with
        /// <see cref="System.IdentityModel.Tokens.AuthorizationPolicy"/>.
        /// </summary>
        /// <param name="operationContext">Current OperationContext that contains all the IAuthorizationPolicies.</param>
        /// <returns>Read-Only collection of <see cref="IAuthorizationPolicy"/> </returns>
        protected override ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies(OperationContext operationContext)
        {
            //
            // Make sure we always return at least one claims identity, if there are no auth policies
            // that contain any identities, then return an anonymous identity wrapped in an authorization policy.
            //
            // If we do not, then Thread.CurrentPrincipal may end up being null inside service operations after the
            // authorization polices are evaluated since ServiceCredentials.ConfigureServiceHost will
            // turn the PrincipalPermissionMode knob to Custom.
            //

            ReadOnlyCollection<IAuthorizationPolicy> baseAuthorizationPolicies = base.GetAuthorizationPolicies(operationContext);
            if (baseAuthorizationPolicies == null)
            {
                return AnonymousAuthorizationPolicy;
            }
            else
            {
                ServiceCredentials sc = GetServiceCredentials();
                AuthorizationPolicy transformedPolicy = TransformAuthorizationPolicies(baseAuthorizationPolicies,
                                                                                        sc.IdentityConfiguration.SecurityTokenHandlers,
                                                                                        true);
                if (transformedPolicy == null || transformedPolicy.IdentityCollection.Count == 0)
                {
                    return AnonymousAuthorizationPolicy;
                }
                return (new List<IAuthorizationPolicy>() { transformedPolicy }).AsReadOnly();
            }
        }

        internal static AuthorizationPolicy TransformAuthorizationPolicies(
            ReadOnlyCollection<IAuthorizationPolicy> baseAuthorizationPolicies,
            SecurityTokenHandlerCollection securityTokenHandlerCollection,
            bool includeTransportTokens)
        {
            List<ClaimsIdentity> identities = new List<ClaimsIdentity>();
            List<IAuthorizationPolicy> uncheckedAuthorizationPolicies = new List<IAuthorizationPolicy>();

            //
            // STEP 1: Filter out the IAuthorizationPolicy that WCF generated. These
            //         are generated as IDFx does not have a proper SecurityTokenHandler
            //         to handle these. For example, SSPI at message layer and all token
            //         types at the Transport layer.
            //
            foreach (IAuthorizationPolicy authPolicy in baseAuthorizationPolicies)
            {
                if ((authPolicy is SctAuthorizationPolicy) ||
                    (authPolicy is EndpointAuthorizationPolicy))
                {
                    //
                    // We ignore the SctAuthorizationPolicy if any found as they were created
                    // as wrapper policies to hold the primary identity claim during a token renewal path.
                    // WCF would otherwise fault thinking the token issuance and renewal identities are 
                    // different. This policy should be treated as a dummy policy and thereby should not be transformed.
                    //
                    // We ignore EndpointAuthorizationPolicy as well. This policy is used only to carry
                    // the endpoint Identity and there is no useful claims that this policy contributes.
                    //
                    continue;
                }

                AuthorizationPolicy idfxAuthPolicy = authPolicy as AuthorizationPolicy;

                if (idfxAuthPolicy != null)
                {
                    // Identities obtained from the Tokens in the message layer would 
                    identities.AddRange(idfxAuthPolicy.IdentityCollection);
                }
                else
                {
                    uncheckedAuthorizationPolicies.Add(authPolicy);
                }
            }

            //
            // STEP 2: Generate IDFx claims from the transport token
            //
            if (includeTransportTokens && (OperationContext.Current != null) &&
                (OperationContext.Current.IncomingMessageProperties != null) &&
                (OperationContext.Current.IncomingMessageProperties.Security != null) &&
                (OperationContext.Current.IncomingMessageProperties.Security.TransportToken != null))
            {
                SecurityToken transportToken =
                    OperationContext.Current.IncomingMessageProperties.Security.TransportToken.SecurityToken;

                ReadOnlyCollection<IAuthorizationPolicy> policyCollection =
                    OperationContext.Current.IncomingMessageProperties.Security.TransportToken.SecurityTokenPolicies;
                bool isWcfAuthPolicy = true;

                foreach (IAuthorizationPolicy policy in policyCollection)
                {
                    //
                    // Iterate over each of the policies in the policyCollection to make sure
                    // we don't have an idfx policy, if we do we will not consider this as
                    // a wcf auth policy: Such a case will be hit for the SslStreamSecurityBinding over net tcp
                    //

                    if (policy is AuthorizationPolicy)
                    {
                        isWcfAuthPolicy = false;
                        break;
                    }
                }

                if (isWcfAuthPolicy)
                {
                    ReadOnlyCollection<ClaimsIdentity> tranportTokenIdentities = GetTransportTokenIdentities(transportToken);
                    identities.AddRange(tranportTokenIdentities);

                    //
                    // NOTE: In the below code, we are trying to identify the IAuthorizationPolicy that WCF
                    // created for the Transport token and eliminate it. This assumes that any client Security  
                    // Token that came in the Security header would have been validated by the SecurityTokenHandler 
                    // and hence would have created a IDFx AuthorizationPolicy. 
                    // For example, if X.509 Certificate was used to authenticate the client at the transport layer 
                    // and then again at the Message security layer we depend on our TokenHandlers to have been in
                    // place to validate the X.509 Certificate at the message layer. This would clearly distinguish
                    // which policy was created for the Transport token by WCF. 
                    //
                    EliminateTransportTokenPolicy(transportToken, tranportTokenIdentities, uncheckedAuthorizationPolicies);
                }
            }

            //
            // STEP 3: Process any uncheckedAuthorizationPolicies here. Convert these to IDFx 
            //         Claims.
            //
            if (uncheckedAuthorizationPolicies.Count > 0)
            {
                identities.AddRange(ConvertToIDFxIdentities(uncheckedAuthorizationPolicies, securityTokenHandlerCollection));
            }

            //
            // STEP 4: Create an AuthorizationPolicy with all the ClaimsIdentities.
            //
            AuthorizationPolicy idfxAuthorizationPolicy = null;
            if (identities.Count == 0)
            {
                //
                // No IDFx ClaimsIdentity was found. Return AnonymousIdentity.
                //
                idfxAuthorizationPolicy = new AuthorizationPolicy(new ClaimsIdentity());
            }
            else
            {
                idfxAuthorizationPolicy = new AuthorizationPolicy(identities.AsReadOnly());
            }

            return idfxAuthorizationPolicy;
        }

        /// <summary>
        /// Creates ClaimsIdentityCollection for the given Transport SecurityToken.
        /// </summary>
        /// <param name="transportToken">Client SecurityToken provided at the Transport layer.</param>
        /// <returns>ClaimsIdentityCollection built from the Transport SecurityToken</returns>
        static ReadOnlyCollection<ClaimsIdentity> GetTransportTokenIdentities(SecurityToken transportToken)
        {
            if (transportToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportToken");
            }

            ServiceCredentials serviceCreds = GetServiceCredentials();

            List<ClaimsIdentity> transportTokenIdentityCollection = new List<ClaimsIdentity>();

            //////////////////////////////////////////////////////////////////////////////////////////
            // 
            // There are 5 well-known Client Authentication types at the transport layer. Each of these will
            // result either in a WindowsSecurityToken, X509SecurityToken or UserNameSecurityToken.
            // All other type of credentials (like OAuth token) result other token that will be passed trough regular validation process.
            //
            //      ClientCredential Type     ||        Transport Token Type
            // -------------------------------------------------------------------
            //          Basic                 ->        UserNameSecurityToken (In Self-hosted case)
            //          Basic                 ->        WindowsSecurityToken (In Web-Hosted case)
            //          NTLM                  ->        WindowsSecurityToken
            //          Negotiate             ->        WindowsSecurityToken
            //          Windows               ->        WindowsSecurityToken
            //          Certificate           ->        X509SecurityToken
            //
            //////////////////////////////////////////////////////////////////////////////////////////
            
            WindowsSecurityToken windowsSecurityToken = transportToken as WindowsSecurityToken;
            if ( windowsSecurityToken != null )
            {
                WindowsIdentity claimsIdentity = new WindowsIdentity( windowsSecurityToken.WindowsIdentity.Token,
                    AuthenticationTypes.Windows );
                AddAuthenticationMethod( claimsIdentity, AuthenticationMethods.Windows );
                AddAuthenticationInstantClaim(claimsIdentity, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated));

                // Just reflect on the wrapped WindowsIdentity and build the WindowsClaimsIdentity class.
                transportTokenIdentityCollection.Add(claimsIdentity);
            }
            else
            {
                // WCF does not call our SecurityTokenHandlers for the Transport token. So run the token through
                // the SecurityTokenHandler and generate claims for this token.
                transportTokenIdentityCollection.AddRange(serviceCreds.IdentityConfiguration.SecurityTokenHandlers.ValidateToken( transportToken ));
            }

            return transportTokenIdentityCollection.AsReadOnly();
        }

        /// <summary>
        /// Given a collection of IAuthorizationPolicies this method will eliminate the IAuthorizationPolicy
        /// that was created for the given transport Security Token. The method modifies the given collection
        /// of IAuthorizationPolicy.
        /// </summary>
        /// <param name="transportToken">Client's Security Token provided at the transport layer.</param>
        /// <param name="tranportTokenIdentities"></param>
        /// <param name="baseAuthorizationPolicies">Collection of IAuthorizationPolicies that were created by WCF.</param>
        static void EliminateTransportTokenPolicy(
            SecurityToken transportToken,
            IEnumerable<ClaimsIdentity> tranportTokenIdentities,
            List<IAuthorizationPolicy> baseAuthorizationPolicies)
        {
            if (transportToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportToken");
            }

            if (tranportTokenIdentities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tranportTokenIdentities");
            }

            if (baseAuthorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAuthorizationPolicy");
            }

            if (baseAuthorizationPolicies.Count == 0)
            {
                // This should never happen in our current configuration. IDFx token handlers do not validate
                // client tokens present at the transport level. So we should atleast have one IAuthorizationPolicy
                // that WCF generated for the transport token.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAuthorizationPolicy", SR.GetString(SR.ID0020));
            }

            //
            // We will process one IAuthorizationPolicy at a time. Transport token will have been authenticated
            // by WCF and would have created a IAuthorizationPolicy for the same. If the transport token is a X.509
            // SecurityToken and 'mapToWindows' was set to true then the IAuthorizationPolicy that was created
            // by WCF will have two Claimsets, a X509ClaimSet and a WindowsClaimSet. We need to prune out this case
            // and ignore both these Claimsets as we have made a call to the token handler to authenticate this
            // token above. If we create a AuthorizationContext using all the IAuthorizationPolicies then all
            // the claimsets are merged and it becomes hard to identify this case. 
            //
            IAuthorizationPolicy policyToEliminate = null;
            foreach (IAuthorizationPolicy authPolicy in baseAuthorizationPolicies)
            {
                if (DoesPolicyMatchTransportToken(transportToken, tranportTokenIdentities, authPolicy))
                {
                    policyToEliminate = authPolicy;
                    break;
                }
            }

            if (policyToEliminate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4271, transportToken));
            }

            baseAuthorizationPolicies.Remove(policyToEliminate);
        }

        /// <summary>
        /// Returns true if the IAuthorizationPolicy could have been created from the given Transport token.
        /// The method can handle only X509SecurityToken and WindowsSecurityToken.
        /// </summary>
        /// <param name="transportToken">Client's Security Token provided at the transport layer.</param>
        /// <param name="tranportTokenIdentities">A collection of <see cref="ClaimsIdentity"/> to match.</param>
        /// <param name="authPolicy">IAuthorizationPolicy to check.</param>
        /// <returns>True if the IAuthorizationPolicy could have been created from the given Transpor token.</returns>
        static bool DoesPolicyMatchTransportToken(
            SecurityToken transportToken,
            IEnumerable<ClaimsIdentity> tranportTokenIdentities,
            IAuthorizationPolicy authPolicy
            )
        {
            if (transportToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportToken");
            }

            if (tranportTokenIdentities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tranportTokenIdentities");
            }

            if (authPolicy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authPolicy");
            }

            //////////////////////////////////////////////////////////////////////////////////////////
            // 
            // There are 5 Client Authentication types at the transport layer. Each of these will
            // result either in a WindowsSecurityToken, X509SecurityToken or UserNameSecurityToken.
            //
            //      ClientCredential Type     ||        Transport Token Type
            // -------------------------------------------------------------------
            //          Basic                 ->        UserNameSecurityToken (In Self-hosted case)
            //          Basic                 ->        WindowsSecurityToken (In Web-Hosted case)
            //          NTLM                  ->        WindowsSecurityToken
            //          Negotiate             ->        WindowsSecurityToken
            //          Windows               ->        WindowsSecurityToken
            //          Certificate           ->        X509SecurityToken
            //
            //////////////////////////////////////////////////////////////////////////////////////////
            X509SecurityToken x509SecurityToken = transportToken as X509SecurityToken;

            SysAuthorizationContext defaultAuthContext = SysAuthorizationContext.CreateDefaultAuthorizationContext(new List<IAuthorizationPolicy>() { authPolicy });

            foreach (System.IdentityModel.Claims.ClaimSet claimset in defaultAuthContext.ClaimSets)
            {
                if (x509SecurityToken != null)
                {
                    // Check if the claimset contains a claim that matches the X.509 certificate thumbprint.
                    if (claimset.ContainsClaim(new System.IdentityModel.Claims.Claim(
                            System.IdentityModel.Claims.ClaimTypes.Thumbprint,
                            x509SecurityToken.Certificate.GetCertHash(),
                            System.IdentityModel.Claims.Rights.PossessProperty)))
                    {
                        return true;
                    }
                }
                else
                {
                    // For WindowsSecurityToken and UserNameSecurityToken check that IClaimsdentity.Name 
                    // matches the Name Claim in the ClaimSet.
                    // In most cases, we will have only one Identity in the ClaimsIdentityCollection 
                    // generated from transport token. 
                    foreach (ClaimsIdentity transportTokenIdentity in tranportTokenIdentities)
                    {
                        if (claimset.ContainsClaim(new System.IdentityModel.Claims.Claim(
                                System.IdentityModel.Claims.ClaimTypes.Name,
                                transportTokenIdentity.Name,
                                System.IdentityModel.Claims.Rights.PossessProperty), new ClaimStringValueComparer()))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a given set of WCF IAuthorizationPolicy to WIF ClaimIdentities.
        /// </summary>
        /// <param name="authorizationPolicies">Set of AuthorizationPolicies to convert to IDFx.</param>
        /// <param name="securityTokenHandlerCollection">The SecurityTokenHandlerCollection to use.</param>
        /// <returns>ClaimsIdentityCollection</returns>
        static ReadOnlyCollection<ClaimsIdentity> ConvertToIDFxIdentities(IList<IAuthorizationPolicy> authorizationPolicies,
                                                                 SecurityTokenHandlerCollection securityTokenHandlerCollection)
        {
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }

            if (securityTokenHandlerCollection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenHandlerCollection");
            }

            List<ClaimsIdentity> identities = new List<ClaimsIdentity>();

            SecurityTokenSpecification kerberosTokenSpecification = null;
            SysAuthorizationContext kerberosAuthContext = null;
            if ((OperationContext.Current != null) &&
                 (OperationContext.Current.IncomingMessageProperties != null) &&
                 (OperationContext.Current.IncomingMessageProperties.Security != null))
            {
                SecurityMessageProperty securityMessageProperty = OperationContext.Current.IncomingMessageProperties.Security;
                foreach (SecurityTokenSpecification tokenSpecification in new SecurityTokenSpecificationEnumerable(securityMessageProperty))
                {
                    if (tokenSpecification.SecurityToken is KerberosReceiverSecurityToken)
                    {
                        kerberosTokenSpecification = tokenSpecification;
                        kerberosAuthContext = SysAuthorizationContext.CreateDefaultAuthorizationContext(kerberosTokenSpecification.SecurityTokenPolicies);
                        break;
                    }
                }
            }

            bool hasKerberosTokenPolicyMatched = false;

            foreach (IAuthorizationPolicy policy in authorizationPolicies)
            {
                bool authPolicyHandled = false;

                if ((kerberosTokenSpecification != null) && !hasKerberosTokenPolicyMatched)
                {
                    if (kerberosTokenSpecification.SecurityTokenPolicies.Contains(policy))
                    {
                        hasKerberosTokenPolicyMatched = true;
                    }
                    else
                    {
                        SysAuthorizationContext authContext = SysAuthorizationContext.CreateDefaultAuthorizationContext(new List<IAuthorizationPolicy>() { policy });                        
                        // Kerberos creates only one ClaimSet. So any more ClaimSet would mean that this is not a Policy created from Kerberos.
                        if (authContext.ClaimSets.Count == 1)
                        {
                            bool allClaimsMatched = true;
                            foreach (System.IdentityModel.Claims.Claim c in authContext.ClaimSets[0])
                            {
                                if (!kerberosAuthContext.ClaimSets[0].ContainsClaim(c))
                                {
                                    allClaimsMatched = false;
                                    break;
                                }
                            }
                            hasKerberosTokenPolicyMatched = allClaimsMatched;
                        }
                    }

                    if (hasKerberosTokenPolicyMatched)
                    {
                        SecurityTokenHandler tokenHandler = securityTokenHandlerCollection[kerberosTokenSpecification.SecurityToken];
                        if ((tokenHandler != null) && tokenHandler.CanValidateToken)
                        {
                            identities.AddRange(tokenHandler.ValidateToken(kerberosTokenSpecification.SecurityToken));
                            authPolicyHandled = true;
                        }
                    }
                }

                if (!authPolicyHandled)
                {

                    SysAuthorizationContext defaultAuthContext = SysAuthorizationContext.CreateDefaultAuthorizationContext(new List<IAuthorizationPolicy>() { policy });
                    //
                    // Merge all ClaimSets to IClaimsIdentity.
                    //

                    identities.Add(ConvertToIDFxIdentity(defaultAuthContext.ClaimSets, securityTokenHandlerCollection.Configuration));
                }

            }

            return identities.AsReadOnly();
        }

        /// <summary>
        /// Converts a given set of WCF ClaimSets to IDFx ClaimsIdentity.
        /// </summary>
        /// <param name="claimSets">Collection of <see cref="ClaimSet"/> to convert to IDFx.</param>
        /// <param name="securityTokenHandlerConfiguration">The SecurityTokenHandlerConfiguration to use.</param>
        /// <returns>ClaimsIdentity</returns>
        static ClaimsIdentity ConvertToIDFxIdentity(IList<ClaimSet> claimSets, SecurityTokenHandlerConfiguration securityTokenHandlerConfiguration)
        {
            if (claimSets == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimSets");
            }

            ClaimsIdentity claimsIdentity = null;
            foreach (System.IdentityModel.Claims.ClaimSet claimSet in claimSets)
            {
                WindowsClaimSet windowsClaimSet = claimSet as WindowsClaimSet;
                if (windowsClaimSet != null)
                {
                    // 
                    // The ClaimSet in the authorizationContext is simply a reflection of the NT Token.
                    // The WindowsClaimsIdentity will generate that information properly. So ignore the ClaimSets.
                    //
                    //
                    // WCF does not propogate the WindowsIdentity.AuthenticationType properly.
                    // To avoid WindowsClaimsIdentity.AuthenticationType from throwing, specify
                    // this authenticationType value. Since we only have to handle SPNEGO specify Negotiate.
                    // 
                    claimsIdentity = MergeClaims(claimsIdentity, new WindowsIdentity(windowsClaimSet.WindowsIdentity.Token,
                                                                                             AuthenticationTypes.Negotiate));

                    AddAuthenticationMethod(claimsIdentity, AuthenticationMethods.Windows);
                    AddAuthenticationInstantClaim(claimsIdentity, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated));
                }
                else
                {
                    claimsIdentity = MergeClaims(claimsIdentity, ClaimsConversionHelper.CreateClaimsIdentityFromClaimSet(claimSet));
                    AddAuthenticationInstantClaim(claimsIdentity, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated));
                }

            }

            return claimsIdentity;
        }

        /// <summary>
        /// Gets the ServiceCredentials from the OperationContext.
        /// </summary>
        /// <returns>ServiceCredentials</returns>
        static ServiceCredentials GetServiceCredentials()
        {
            ServiceCredentials serviceCredentials = null;

            if (OperationContext.Current != null &&
                OperationContext.Current.Host != null &&
                OperationContext.Current.Host.Description != null &&
                OperationContext.Current.Host.Description.Behaviors != null)
            {
                serviceCredentials = OperationContext.Current.Host.Description.Behaviors.Find<ServiceCredentials>();
            }

            return serviceCredentials;
        }

        // Adds an Authentication Method claims to the given ClaimsIdentity if one is not already present.
        static void AddAuthenticationMethod(ClaimsIdentity claimsIdentity, string authenticationMethod)
        {
            System.Security.Claims.Claim authenticationMethodClaim =
                        claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.AuthenticationMethod);

            if (authenticationMethodClaim == null)
            {
                // AuthenticationMethod claims does not exist. Add one.
                claimsIdentity.AddClaim(
                    new System.Security.Claims.Claim(
                        System.Security.Claims.ClaimTypes.AuthenticationMethod, authenticationMethod));
            }
        }

        // Adds an Authentication Method claims to the given ClaimsIdentity if one is not already present.
        static void AddAuthenticationInstantClaim(ClaimsIdentity claimsIdentity, string authenticationInstant)
        {
            // the issuer for this claim should always be the default issuer. 
            string issuerName = ClaimsIdentity.DefaultIssuer;
            System.Security.Claims.Claim authenticationInstantClaim =
                    claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.AuthenticationInstant);

            if (authenticationInstantClaim == null)
            {
                // AuthenticationInstance claims does not exist. Add one.
                claimsIdentity.AddClaim(
                    new System.Security.Claims.Claim(
                        System.Security.Claims.ClaimTypes.AuthenticationInstant, authenticationInstant, ClaimValueTypes.DateTime,
                        issuerName));
            }
        }

        // When a token creates more than one Identity we have to merge these identities. 
        // The below method takes two Identities and will return a single identity. If one of the 
        // Identities is a WindowsIdentity then all claims from the other identity are 
        // merged into the WindowsIdentity. If neither are WindowsIdentity then it
        // selects 'identity1' and merges all the claims from 'identity2' into 'identity1'.
        //
        // It is not clear how we can handler duplicate name claim types and delegates.
        // So, we are just cloning the claims from one identity and adding it to another. 
        internal static ClaimsIdentity MergeClaims(ClaimsIdentity identity1, ClaimsIdentity identity2)
        {
            if ((identity1 == null) && (identity2 == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4268));
            }

            if (identity1 == null)
            {
                return identity2;
            }

            if (identity2 == null)
            {
                return identity1;
            }

            WindowsIdentity windowsIdentity = identity1 as WindowsIdentity;
            if (windowsIdentity != null)
            {
                windowsIdentity.AddClaims(identity2.Claims);
                return windowsIdentity;
            }

            windowsIdentity = identity2 as WindowsIdentity;
            if (windowsIdentity != null)
            {
                windowsIdentity.AddClaims(identity1.Claims);
                return windowsIdentity;
            }

            identity1.AddClaims(identity2.Claims);

            return identity1;
        }

        /// <summary>
        /// Checks authorization for the given operation context based on policy evaluation.
        /// </summary>
        /// <param name="operationContext">The OperationContext for the current authorization request.</param>
        /// <returns>true if authorized, false otherwise</returns>
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (operationContext == null)
            {
                return false;
            }

            string action = string.Empty;

            // WebRequests will not always have an action specified in the operation context.
            // If action is null or empty, check the httpRequest.
            if (!string.IsNullOrEmpty(operationContext.IncomingMessageHeaders.Action))
            {
                action = operationContext.IncomingMessageHeaders.Action;
            }
            else
            {
                HttpRequestMessageProperty request = operationContext.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                if (request != null)
                {
                    action = request.Method;
                }
            }

            System.Uri resource = operationContext.IncomingMessageHeaders.To;
            ServiceCredentials credentials = GetServiceCredentials();

            if ((credentials == null) || string.IsNullOrEmpty(action) || (resource == null))
            {
                return false;
            }

            //
            // CheckAccess is called prior to impersonation in WCF, so we need to pull
            // the ClaimsPrincipal from the OperationContext.ServiceSecurityContext.AuthorizationContext.Properties[ "Principal" ].            
            //
            ClaimsPrincipal claimsPrincipal = operationContext.ServiceSecurityContext.AuthorizationContext.Properties[AuthorizationPolicy.ClaimsPrincipalKey] as ClaimsPrincipal;
            
            claimsPrincipal = credentials.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(resource.AbsoluteUri, claimsPrincipal);
            operationContext.ServiceSecurityContext.AuthorizationContext.Properties[AuthorizationPolicy.ClaimsPrincipalKey] = claimsPrincipal;

            if ((claimsPrincipal == null) || (claimsPrincipal.Identities == null))
            {
                return false;
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Information,
                    TraceCode.Security,
                    SR.GetString(SR.TraceAuthorize),
                    new System.IdentityModel.Diagnostics.AuthorizeTraceRecord(claimsPrincipal, resource.AbsoluteUri, action));
            }

            bool authorized = credentials.IdentityConfiguration.ClaimsAuthorizationManager.CheckAccess(
                new System.Security.Claims.AuthorizationContext(
                    claimsPrincipal, resource.AbsoluteUri, action
                    )
                );

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                if (authorized)
                {
                    System.IdentityModel.Diagnostics.TraceUtility.TraceString(
                        TraceEventType.Information,
                        SR.GetString(SR.TraceOnAuthorizeRequestSucceed));
                }
                else
                {
                    System.IdentityModel.Diagnostics.TraceUtility.TraceString(
                        TraceEventType.Information,
                        SR.GetString(SR.TraceOnAuthorizeRequestFailed));
                }
            }

            return authorized;
        }
    }

    class ClaimStringValueComparer : IEqualityComparer<System.IdentityModel.Claims.Claim>
    {
        #region IEqualityComparer<System.IdentityModel.Claims.Claim> Members

        public bool Equals(System.IdentityModel.Claims.Claim claim1, System.IdentityModel.Claims.Claim claim2)
        {
            if (ReferenceEquals(claim1, claim2))
            {
                return true;
            }

            if (claim1 == null || claim2 == null)
            {
                return false;
            }

            if (claim1.ClaimType != claim2.ClaimType || claim1.Right != claim2.Right)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(claim1.Resource, claim2.Resource);
        }

        public int GetHashCode(System.IdentityModel.Claims.Claim claim)
        {
            if (claim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }

            return claim.ClaimType.GetHashCode() ^ claim.Right.GetHashCode()
                ^ ((claim.Resource == null) ? 0 : claim.Resource.GetHashCode());
        }

        #endregion
    }

    class SecurityTokenSpecificationEnumerable : IEnumerable<SecurityTokenSpecification>
    {
        SecurityMessageProperty _securityMessageProperty;

        public SecurityTokenSpecificationEnumerable(SecurityMessageProperty securityMessageProperty)
        {
            if (securityMessageProperty == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityMessageProperty");
            }

            _securityMessageProperty = securityMessageProperty;
        }

        public IEnumerator<SecurityTokenSpecification> GetEnumerator()
        {
            if (_securityMessageProperty.InitiatorToken != null)
            {
                yield return _securityMessageProperty.InitiatorToken;
            }

            if (_securityMessageProperty.ProtectionToken != null)
            {
                yield return _securityMessageProperty.ProtectionToken;
            }

            if (_securityMessageProperty.HasIncomingSupportingTokens)
            {
                foreach (SecurityTokenSpecification tokenSpecification in _securityMessageProperty.IncomingSupportingTokens)
                {
                    if (tokenSpecification != null)
                    {
                        yield return tokenSpecification;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

    }

}
