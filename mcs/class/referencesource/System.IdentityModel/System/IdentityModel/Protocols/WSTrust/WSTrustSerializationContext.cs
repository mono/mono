//-----------------------------------------------------------------------
// <copyright file="WSTrustSerializationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Defines the serialization context class.
    /// </summary>
    public class WSTrustSerializationContext
    {
        private SecurityTokenResolver securityTokenResolver;
        private SecurityTokenResolver useKeyTokenResolver;
        private SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager;

        /// <summary>
        /// Initializes an instance <see cref="WSTrustSerializationContext"/>
        /// </summary>
        public WSTrustSerializationContext()
            : this(SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager())
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustSerializationContext"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollectionManager">The security token handler collection manager.</param>
        public WSTrustSerializationContext(SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager)
            : this(securityTokenHandlerCollectionManager, EmptySecurityTokenResolver.Instance, EmptySecurityTokenResolver.Instance)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustSerializationContext"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollectionManager">
        /// The <see cref="SecurityTokenHandlerCollectionManager" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </param>
        /// <param name="securityTokenResolver">
        /// The <see cref="SecurityTokenResolver"/> used to resolve security token references found in most
        /// elements of WS-Trust messages.
        /// </param>
        /// <param name="useKeyTokenResolver">
        /// The <see cref="SecurityTokenResolver"/> used to resolve security token references found in the
        /// UseKey element of RST messages as well as the RenewTarget element found in RST messages.
        /// </param>
        public WSTrustSerializationContext(SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager, SecurityTokenResolver securityTokenResolver, SecurityTokenResolver useKeyTokenResolver)
        {
            if (securityTokenHandlerCollectionManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenHandlerCollectionManager");
            }

            if (securityTokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenResolver");
            }

            if (useKeyTokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("useKeyTokenResolver");
            }

            this.securityTokenHandlerCollectionManager = securityTokenHandlerCollectionManager;
            this.securityTokenResolver = securityTokenResolver;
            this.useKeyTokenResolver = useKeyTokenResolver;
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenResolver"/> used to resolve security token references found in most
        /// elements of WS-Trust messages.
        /// </summary>
        public SecurityTokenResolver TokenResolver
        {
            get { return this.securityTokenResolver; }
            set { this.securityTokenResolver = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenResolver"/> used to resolve security token references found in the
        /// UseKey element of RST messages as well as the RenewTarget element found in RST messages.
        /// </summary>
        public SecurityTokenResolver UseKeyTokenResolver
        {
            get { return this.useKeyTokenResolver; }
            set { this.useKeyTokenResolver = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenHandlerCollectionManager" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </summary>
        public SecurityTokenHandlerCollectionManager SecurityTokenHandlerCollectionManager
        {
            get { return this.securityTokenHandlerCollectionManager; }
            set { this.securityTokenHandlerCollectionManager = value; }
        }

        /// <summary>
        /// Gets the collection of <see cref="SecurityTokenHandler"/> objects used to serialize and
        /// validate security tokens found in WS-Trust messages.
        /// </summary>
        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            get { return this.securityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.Default]; }
        }
    }
}
