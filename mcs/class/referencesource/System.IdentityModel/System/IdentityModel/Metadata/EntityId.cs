//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Describes the entity id.
    /// </summary>
    public class EntityId
    {
        const int MaximumLength = 1024;
        string _id;

        /// <summary>
        /// The empty constructor.
        /// </summary>
        public EntityId()
            : this(null)
        {
        }

        /// <summary>
        /// Constructs an entity id with the id.
        /// </summary>
        /// <param name="id">The id for this instance.</param>
        public EntityId(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <exception cref="ArgumentException">If length of the id is larger than the maximum length.</exception>
        public string Id
        {
            get { return _id; }
            set
            {
                if (value != null)
                {
                    if (value.ToString().Length > MaximumLength)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID3199));
                    }
                }

                _id = value;
            }
        }
    }
}
