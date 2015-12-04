//---------------------------------------------------------------------
// <copyright file="SchemaEnumMember.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Represents enum Member element from the CSDL.
    /// </summary>
    internal class SchemaEnumMember : SchemaElement
    {
        /// <summary>
        /// Value for this member.
        /// </summary>
        long? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaEnumMember"/> class.
        /// </summary>
        /// <param name="parentElement">
        /// Parent element.
        /// </param>
        public SchemaEnumMember(SchemaElement parentElement)
            : base(parentElement)
        { }

        /// <summary>
        /// Gets the value of this enum member. Possibly null if not specified in the CSDL.
        /// </summary>
        public long? Value
        {
            get 
            {
                return _value;
            }

            set
            {
                Debug.Assert(value != null, "value != null");

                _value = value;
            }
        }

        /// <summary>
        /// Generic handler for the Member element attributes
        /// </summary>
        /// <param name="reader">Xml reader positioned on an attribute.</param>
        /// <c>true</c> if the attribute is a known attribute and was handled. Otherwise <c>false</c>
        protected override bool HandleAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            bool handled = base.HandleAttribute(reader);
            if (!handled && (handled = CanHandleAttribute(reader, XmlConstants.Value)))
            {
                HandleValueAttribute(reader);
            }
            
            return handled;
        }

        /// <summary>
        /// Handler for the Member Value attribute.
        /// </summary>
        /// <param name="reader">XmlReader positioned on the Member Value attribute.</param>
        private void HandleValueAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            // xsd validation will report an error if the value is not a valid xs:long number. If the number is valid
            // xs:long number then long.TryParse will succeed.
            long tmpValue;
            if (long.TryParse(reader.Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out tmpValue))
            {
                _value = tmpValue;
            }
        }
    }
}
