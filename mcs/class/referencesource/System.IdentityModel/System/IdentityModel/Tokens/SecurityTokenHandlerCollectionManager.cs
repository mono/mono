    //-----------------------------------------------------------------------
// <copyright file="SecurityTokenHandlerCollectionManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Selectors;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A class which manages multiple named <see cref="SecurityTokenHandlerCollection"/>.
    /// </summary>
    public class SecurityTokenHandlerCollectionManager
    {
        private Dictionary<string, SecurityTokenHandlerCollection> collections = new Dictionary<string, SecurityTokenHandlerCollection>();
        private string serviceName = ConfigurationStrings.DefaultServiceName;

        /// <summary>
        /// Initialize an instance of <see cref="SecurityTokenHandlerCollectionManager"/> for a given named service.
        /// </summary>
        /// <param name="serviceName">A <see cref="String"/> indicating the name of the associated service.</param>
        public SecurityTokenHandlerCollectionManager(string serviceName)
        {
            if (serviceName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceName");
            }

            this.serviceName = serviceName;
        }

        /// <summary>
        /// Initialized with default service configuration.
        /// </summary>
        private SecurityTokenHandlerCollectionManager()
            : this(ConfigurationStrings.DefaultServiceName)
        {
        }

        /// <summary>
        /// Gets a count of the number of SecurityTokenHandlerCollections in this 
        /// SecurityTokenHandlerCollectionManager.
        /// </summary>
        public int Count
        {
            get { return this.collections.Count; }
        }

        /// <summary>
        ///  Gets the service name.
        /// </summary>
        public string ServiceName
        {
            get { return this.serviceName; }
        }

        /// <summary>
        /// Gets an enumeration over the SecurityTokenHandlerCollection list.
        /// </summary>
        public IEnumerable<SecurityTokenHandlerCollection> SecurityTokenHandlerCollections
        {
            get
            {
                return this.collections.Values;
            }
        }

        /// <summary>
        /// The SecurityTokenHandlerCollection for the specified usage.
        /// </summary>
        /// <param name="usage">The usage name for the SecurityTokenHandlerCollection.</param>
        /// <returns>A SecurityTokenHandlerCollection</returns>
        /// <remarks>
        /// Behaves like a dictionary in that it will throw an exception if there is no
        /// value for the specified key.
        /// </remarks>
        public SecurityTokenHandlerCollection this[string usage]
        {
            get
            {
                // Empty String is valid (Usage.Default)
                if (null == usage)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("usage");
                }

                return this.collections[usage];
            }

            set
            {
                // Empty String is valid (Usage.Default)
                if (null == usage)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("usage");
                }

                this.collections[usage] = value;
            }
        }

        /// <summary>
        /// No token handlers are created.
        /// </summary>
        /// <returns>An empty token handler collection manager.</returns>
        public static SecurityTokenHandlerCollectionManager CreateEmptySecurityTokenHandlerCollectionManager()
        {
            return new SecurityTokenHandlerCollectionManager(ConfigurationStrings.DefaultConfigurationElementName);
        }

        /// <summary>
        /// Creates the default set of SecurityTokenHandlers.
        /// </summary>
        /// <returns>A SecurityTokenHandlerCollectionManager with a default collection of token handlers.</returns>
        public static SecurityTokenHandlerCollectionManager CreateDefaultSecurityTokenHandlerCollectionManager()
        {
            SecurityTokenHandlerCollection defaultHandlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            SecurityTokenHandlerCollectionManager defaultManager = new SecurityTokenHandlerCollectionManager(ConfigurationStrings.DefaultServiceName);

            defaultManager.collections.Clear();
            defaultManager.collections.Add(SecurityTokenHandlerCollectionManager.Usage.Default, defaultHandlers);

            return defaultManager;
        }

        /// <summary>
        /// Checks if a SecurityTokenHandlerCollection exists for the given usage.
        /// </summary>
        /// <param name="usage">A string that represents the usage of the SecurityTokenHandlerCollection.</param>
        /// <returns>Whether or not a token handler collection exists for the given usage.</returns>
        public bool ContainsKey(string usage)
        {
            return this.collections.ContainsKey(usage);
        }

        /// <summary>
        /// Defines standard collection names used by the framework.
        /// </summary>
        public static class Usage
        {
            /// <summary>
            /// Used to reference the default collection of handlers.
            /// </summary>
            public const string Default = "";

            /// <summary>
            /// Used to reference a collection of handlers for ActAs element processing.
            /// </summary>
            public const string ActAs = "ActAs";

            /// <summary>
            /// Used to reference a collection of handlers for OnBehalfOf element processing.
            /// </summary>
            public const string OnBehalfOf = "OnBehalfOf";
        }
    }
}
