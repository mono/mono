//-----------------------------------------------------------------------
// <copyright file="Saml2Attribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Attribute element specified in [Saml2Core, 2.7.3.1].
    /// </summary>
    public class Saml2Attribute
    {
        private string friendlyName;
        private string name;
        private Uri nameFormat;
        private Collection<string> values = new Collection<string>();
        private string originalIssuer;
        private string attributeValueXsiType = System.Security.Claims.ClaimValueTypes.String;

        /// <summary>
        /// Initializes a new instance of the Saml2Attribute class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        public Saml2Attribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            this.name = StringUtil.OptimizeString(name);
        }

        /// <summary>
        /// Initializes a new instance of the Saml2Attribute class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="values">The collection of values that define the attribute.</param>
        public Saml2Attribute(string name, IEnumerable<string> values)
            : this(name)
        {
            if (null == values)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("values");
            }

            foreach (string value in values)
            {
                this.values.Add(value);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the Saml2Attribute class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public Saml2Attribute(string name, string value)
            : this(name, new string[] { value })
        {
        }

        /// <summary>
        /// Gets or sets a string that provides a more human-readable form of the attribute's 
        /// name. [Saml2Core, 2.7.3.1]
        /// </summary>
        public string FriendlyName
        {
            get { return this.friendlyName; }
            set { this.friendlyName = XmlUtil.NormalizeEmptyString(value); }
        }

        /// <summary>
        /// Gets or sets the name of the attribute. [Saml2Core, 2.7.3.1]
        /// </summary>
        public string Name
        {
            get 
            { 
                return this.name; 
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }

                this.name = StringUtil.OptimizeString(value);
            }
        }

        /// <summary>
        /// Gets or sets a URI reference representing the classification of the attribute 
        /// name for the purposes of interpreting the name. [Saml2Core, 2.7.3.1]
        /// </summary>
        public Uri NameFormat
        {
            get 
            { 
                return this.nameFormat; 
            }

            set
            {
                if (null != value && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("error", SR.GetString(SR.ID0013));
                }

                this.nameFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that represents the OriginalIssuer of the this SAML Attribute.
        /// </summary>
        public string OriginalIssuer
        {
            get 
            { 
                return this.originalIssuer; 
            }

            set
            {
                if (value == String.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4251));
                }

                this.originalIssuer = StringUtil.OptimizeString(value);
            }
        }

        /// <summary>
        /// Gets or sets the xsi:type of the values contained in the SAML Attribute.
        /// </summary>
        public string AttributeValueXsiType
        {
            get 
            {
                return this.attributeValueXsiType;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                int indexOfHash = value.IndexOf('#');
                if (indexOfHash == -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                string prefix = value.Substring(0, indexOfHash);
                if (prefix.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                string suffix = value.Substring(indexOfHash + 1);
                if (suffix.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID4254));
                }

                this.attributeValueXsiType = value;
            }
        }

        /// <summary>
        /// Gets the values of the attribute.
        /// </summary>
        public Collection<string> Values
        {
            get { return this.values; }
        }
    }
}
