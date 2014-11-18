//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using SysClaimTypes = System.IdentityModel.Claims.ClaimTypes;

    /// <summary>
    /// This class represents the displayable claim object. Usually, the display tag
    /// and the description are localized. And claimType identifies different claim 
    /// types. The display value is the string representation of the claim.Resource.
    /// </summary>
    public class DisplayClaim
    {
        static Dictionary<string, string> claimDescriptionMap = PopulateClaimDescriptionMap();
        static Dictionary<string, string> claimTagMap = PopulateClaimTagMap();

        string claimType;           // required, should map to claim.ClaimType
        string displayTag;          // should map to claim's friendly name, sometime called display name
        string displayValue;        // should map to claim.Resource
        string description;         // should map to claim's decription
        bool optional;              // The Optional attribute

        static Dictionary<string, string> PopulateClaimTagMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            // populate _claimTagMap with known values
            map.Add(ClaimTypes.Country, SR.GetString(SR.CountryText));
            map.Add(ClaimTypes.DateOfBirth, SR.GetString(SR.DateOfBirthText));
            map.Add(ClaimTypes.Email, SR.GetString(SR.EmailAddressText));
            map.Add(ClaimTypes.Gender, SR.GetString(SR.GenderText));
            map.Add(ClaimTypes.GivenName, SR.GetString(SR.GivenNameText));
            map.Add(ClaimTypes.HomePhone, SR.GetString(SR.HomePhoneText));
            map.Add(ClaimTypes.Locality, SR.GetString(SR.LocalityText));
            map.Add(ClaimTypes.MobilePhone, SR.GetString(SR.MobilePhoneText));
            map.Add(ClaimTypes.Name, SR.GetString(SR.NameText));
            map.Add(ClaimTypes.OtherPhone, SR.GetString(SR.OtherPhoneText));
            map.Add(ClaimTypes.PostalCode, SR.GetString(SR.PostalCodeText));
            map.Add(SysClaimTypes.PPID, SR.GetString(SR.PPIDText));
            map.Add(ClaimTypes.StateOrProvince, SR.GetString(SR.StateOrProvinceText));
            map.Add(ClaimTypes.StreetAddress, SR.GetString(SR.StreetAddressText));
            map.Add(ClaimTypes.Surname, SR.GetString(SR.SurnameText));
            map.Add(ClaimTypes.Webpage, SR.GetString(SR.WebPageText));
            map.Add(ClaimTypes.Role, SR.GetString(SR.RoleText));

            return map;
        }

        static Dictionary<string, string> PopulateClaimDescriptionMap()
        {
            // populate _claimDescriptionMap with known values
            Dictionary<string, string> map = new Dictionary<string, string>();

            map.Add(ClaimTypes.Country, SR.GetString(SR.CountryDescription));
            map.Add(ClaimTypes.DateOfBirth, SR.GetString(SR.DateOfBirthDescription));
            map.Add(ClaimTypes.Email, SR.GetString(SR.EmailAddressDescription));
            map.Add(ClaimTypes.Gender, SR.GetString(SR.GenderDescription));
            map.Add(ClaimTypes.GivenName, SR.GetString(SR.GivenNameDescription));
            map.Add(ClaimTypes.HomePhone, SR.GetString(SR.HomePhoneDescription));
            map.Add(ClaimTypes.Locality, SR.GetString(SR.LocalityDescription));
            map.Add(ClaimTypes.MobilePhone, SR.GetString(SR.MobilePhoneDescription));
            map.Add(ClaimTypes.Name, SR.GetString(SR.NameDescription));
            map.Add(ClaimTypes.OtherPhone, SR.GetString(SR.OtherPhoneDescription));
            map.Add(ClaimTypes.PostalCode, SR.GetString(SR.PostalCodeDescription));
            map.Add(SysClaimTypes.PPID, SR.GetString(SR.PPIDDescription));
            map.Add(ClaimTypes.StateOrProvince, SR.GetString(SR.StateOrProvinceDescription));
            map.Add(ClaimTypes.StreetAddress, SR.GetString(SR.StreetAddressDescription));
            map.Add(ClaimTypes.Surname, SR.GetString(SR.SurnameDescription));
            map.Add(ClaimTypes.Webpage, SR.GetString(SR.WebPageDescription));
            map.Add(ClaimTypes.Role, SR.GetString(SR.RoleDescription));

            return map;
        }

        static string ClaimTagForClaimType(string claimType)
        {
            string tag = null;
            claimTagMap.TryGetValue(claimType, out tag);
            return tag;
        }

        static string ClaimDescriptionForClaimType(string claimType)
        {
            string description = null;
            claimDescriptionMap.TryGetValue(claimType, out description);
            return description;
        }

        /// <summary>
        /// Creates a display claim from a given claim type and sets default values
        /// for DisplayTag and Description properities.
        /// </summary>
        /// <param name="claimType">The unique uri identifier of a claim type</param>
        public static DisplayClaim CreateDisplayClaimFromClaimType(string claimType)
        {
            DisplayClaim displayClaim = new DisplayClaim(claimType);
            displayClaim.DisplayTag = ClaimTagForClaimType(claimType);
            displayClaim.Description = ClaimDescriptionForClaimType(claimType);
            return displayClaim;
        }

        /// <summary>
        /// Constructs a display claim object if claimType is known
        /// </summary>
        /// <param name="claimType">The unique uri identifier of a claim type</param>
        public DisplayClaim(string claimType)
            : this(claimType, null, null, null)
        {
        }

        /// <summary>
        /// Instantiates a DisplayClaim object. Use this constructor if the actual value of the claim is unknown.
        /// </summary>
        /// <param name="claimType">claim.ClaimType, e.g http://.../claims/EmailAddr </param>
        /// <param name="displayTag">friendly name sometime called display name, e.g. Email address</param>
        /// <param name="description">the description of this claim, e.g. If a person possess this email address</param>
        public DisplayClaim(string claimType, string displayTag, string description)
            : this(claimType, displayTag, description, null)
        {
        }

        /// <summary>
        /// Instantiates a DisplayClaim object. Use this constructor if the actual value of the claim is known.
        /// </summary>
        /// <param name="claimType">claim.ClaimType, e.g http://.../claims/EmailAddr </param>
        /// <param name="displayTag">friendly name sometime called display name, e.g. Email address</param>
        /// <param name="description">the description of this claim, e.g. If a person possess this email address</param>
        /// <param name="displayValue">claim.Resource, e.g. joe@fabrikam.com</param>
        public DisplayClaim(string claimType, string displayTag, string description, string displayValue)
            : this(claimType, displayTag, description, displayValue, true)
        {
        }

        /// <summary>
        /// Instantiates a DisplayClaim object. Use this constructor if the actual value of the claim is known.
        /// </summary>
        /// <param name="claimType">claim.ClaimType, e.g http://.../claims/EmailAddr </param>
        /// <param name="displayTag">friendly name sometime called display name, e.g. Email address</param>
        /// <param name="description">the description of this claim, e.g. If a person possess this email address</param>
        /// <param name="displayValue">claim.Resource, e.g. joe@fabrikam.com</param>
        /// <param name="optional">If the claim is optional.</param>
        /// <exception cref="ArgumentNullException">If the claim type is empty or null.</exception>
        public DisplayClaim(string claimType, string displayTag, string description, string displayValue, bool optional)
        {
            if (string.IsNullOrEmpty(claimType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            }

            this.claimType = claimType;
            this.displayTag = displayTag;
            this.description = description;
            this.displayValue = displayValue;
            this.optional = optional;
        }

        /// <summary>
        /// This required attribute provides the unique identifier (URI) 
        /// of the individual claim returned in the security token
        /// </summary>
        public string ClaimType
        {
            get { return this.claimType; }
        }

        /// <summary>
        /// This optional element provides a friendly name for the claim 
        /// returned in the security token
        /// </summary>
        public string DisplayTag
        {
            get { return this.displayTag; }
            set { this.displayTag = value; }
        }

        /// <summary>
        /// This optional element provides one or more 
        /// displayable values for the claim returned in the security token
        /// </summary>
        public string DisplayValue
        {
            get { return this.displayValue; }
            set { this.displayValue = value; }
        }

        /// <summary>
        /// This optional element provides a description of the semantics 
        /// for the claim returned in the security token.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets or sets the optional attribute.
        /// </summary>
        public bool Optional
        {
            get { return this.optional; }
            set { this.optional = value; }
        }

        /// <summary>
        /// Gets or sets whether the optional attribute will be serialized. The default value is false.
        /// </summary>
        public bool WriteOptionalAttribute
        {
            get;
            set;
        }
    }
}
