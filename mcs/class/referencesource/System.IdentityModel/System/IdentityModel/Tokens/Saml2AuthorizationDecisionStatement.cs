//-----------------------------------------------------------------------
// <copyright file="Saml2AuthorizationDecisionStatement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Represents the AuthzDecisionStatement specified in [Saml2Core, 2.7.4].
    /// </summary>
    public class Saml2AuthorizationDecisionStatement : Saml2Statement
    {
        /// <summary>
        /// The empty URI reference, which may be used with the meaning 
        /// "the start of the current document" for the Resource property.
        /// </summary>
        public static readonly Uri EmptyResource = new Uri(string.Empty, UriKind.Relative);

        private Collection<Saml2Action> actions = new Collection<Saml2Action>();
        private Saml2Evidence evidence;
        private SamlAccessDecision decision;
        private Uri resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml2AuthorizationDecisionStatement"/> class from
        /// a resource and decision.
        /// </summary>
        /// <param name="resource">The <see cref="Uri"/> of the resource to be authorized.</param>
        /// <param name="decision">The <see cref="SamlAccessDecision"/> in use.</param>
        public Saml2AuthorizationDecisionStatement(Uri resource, SamlAccessDecision decision)
            : this(resource, decision, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml2AuthorizationDecisionStatement"/> class from
        /// a resource and decision.
        /// </summary>
        /// <param name="resource">The <see cref="Uri"/> of the resource to be authorized.</param>
        /// <param name="decision">The <see cref="SamlAccessDecision"/> in use.</param>
        /// <param name="actions">Collection of <see cref="Saml2Action"/> specifications.</param>
        public Saml2AuthorizationDecisionStatement(Uri resource, SamlAccessDecision decision, IEnumerable<Saml2Action> actions)
        {
            if (null == resource)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resource");
            }

            // This check is making sure the resource is either a well-formed absolute uri or
            // an empty relative uri before passing through to the rest of the constructor.
            if (!(resource.IsAbsoluteUri || resource.Equals(EmptyResource)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("resource", SR.GetString(SR.ID4121));
            }

            if (decision < SamlAccessDecision.Permit || decision > SamlAccessDecision.Indeterminate)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("decision"));
            }

            this.resource = resource;
            this.decision = decision;

            if (null != actions)
            {
                foreach (Saml2Action action in actions)
                {
                    this.actions.Add(action);
                }
            }
        }

        /// <summary>
        /// Gets of set the set of <see cref="Saml2Action"/> authorized to be performed on the specified
        /// resource. [Saml2Core, 2.7.4]
        /// </summary>
        public Collection<Saml2Action> Actions
        {
            get { return this.actions; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SamlAccessDecision"/> rendered by the SAML authority with respect to the 
        /// specified resource. [Saml2Core, 2.7.4]
        /// </summary>
        public SamlAccessDecision Decision
        {
            get
            { 
                return this.decision; 
            }

            set
            {
                if (value < SamlAccessDecision.Permit || value > SamlAccessDecision.Indeterminate)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.decision = value;
            }
        }

        /// <summary>
        /// Gets or sets a set of <see cref="Saml2Evidence"/> that the SAML authority relied on in making 
        /// the decision. [Saml2Core, 2.7.4]
        /// </summary>
        public Saml2Evidence Evidence
        {
            get { return this.evidence; }
            set { this.evidence = value; }
        }

        /// <summary>
        /// Gets or sets a URI reference identifying the resource to which access 
        /// authorization is sought. [Saml2Core, 2.7.4]
        /// </summary>
        /// <remarks>
        /// In addition to any absolute URI, the Resource may also be the 
        /// empty URI reference, and the meaning is defined to be "the start
        /// of the current document". [Saml2Core, 2.7.4]
        /// </remarks>
        public Uri Resource
        {
            get 
            { 
                return this.resource; 
            }

            set
            {
                if (null == value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (!(value.IsAbsoluteUri || value.Equals(EmptyResource)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4121));
                }

                this.resource = value;
            }
        }
    }
}
