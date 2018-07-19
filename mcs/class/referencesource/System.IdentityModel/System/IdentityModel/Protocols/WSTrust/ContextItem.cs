//-----------------------------------------------------------------------
// <copyright file="ContextItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Defines the auth:ContextItem element.
    /// </summary>
    public class ContextItem
    {
        Uri _name;
        Uri _scope;
        string _value;

        /// <summary>
        /// Initializes an instance of <see cref="ContextItem"/>
        /// </summary>
        /// <param name="name">Context item name.</param>
        public ContextItem(Uri name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ContextItem"/>
        /// </summary>
        /// <param name="name">Context item name.</param>
        /// <param name="value">Context item value. Can be null.</param>
        public ContextItem(Uri name, string value)
            : this(name, value, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ContextItem"/>
        /// </summary>
        /// <param name="name">Context item name.</param>
        /// <param name="value">Context item value. Can be null.</param>
        /// <param name="scope">Context item scope. Can be null.</param>
        /// <exception cref="ArgumentNullException">Input argument 'name' is null.</exception>
        /// <exception cref="ArgumentException">Input argument 'name' or 'scope' is not an absolute URI.</exception>
        public ContextItem(Uri name, string value, Uri scope)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (!name.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name", SR.GetString(SR.ID0013));
            }

            if ((scope != null) && !scope.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scope", SR.GetString(SR.ID0013));
            }

            _name = name;
            _scope = scope;
            _value = value;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public Uri Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the Scope of the item.
        /// </summary>
        public Uri Scope
        {
            get { return _scope; }
            set
            {
                if ((value != null) && !value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }
                _scope = value;
            }
        }

        /// <summary>
        /// Gets the value of the item.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
