//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Defines settings for a AudienceRestriction verification.
    /// </summary>
    public class AudienceRestriction
    {
        AudienceUriMode _audienceMode = AudienceUriMode.Always;
        Collection<Uri> _audience = new Collection<Uri>();

        /// <summary>
        /// Creates an instance of <see cref="AudienceRestriction"/>
        /// </summary>
        public AudienceRestriction()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="AudienceRestriction"/>
        /// </summary>
        /// <param name="audienceMode">Specifies the mode in which AudienceUri restriction is applied.</param>
        public AudienceRestriction( AudienceUriMode audienceMode )
        {
            _audienceMode = audienceMode;
        }

        /// <summary>
        /// Gets/Sets the mode in which Audience URI restriction is applied.
        /// </summary>
        public AudienceUriMode AudienceMode
        {
            get { return _audienceMode; }
            set { _audienceMode = value; }
        }

        /// <summary>
        /// Gets the list of Allowed Audience URIs.
        /// </summary>
        public Collection<Uri> AllowedAudienceUris
        {
            get { return _audience; }
        }
    }

}
