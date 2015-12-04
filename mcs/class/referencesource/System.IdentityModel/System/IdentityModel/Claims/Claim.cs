//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Mail;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    // Examples:
    //    ClaimType        ResourceValue     ResourceRight
    //    ---------------  ----------------  ------------------
    //    "File"           "boot.ini"        "Read"
    //    "HairColor"      "Brown"           "PossessProperty"
    //    "UserName"       "[....]"          "PossessProperty"
    //    "Service"        "MailService"     "Access"
    //    "Operation"      "ReadMail"        "Invoke"
    // ClaimType:
    //    DESC: The type of resource for which rights are granted
    //    XrML: ClaimSet/Resource
    //    SAML: SamlAttributeStatement/Attribute/@Name/..., SamlAuthorizationDecisionStatement/Action/@Namespace/...
    // ResourceValue:
    //    DESC: Value identifying the resource for which rights are granted
    //    XrML: ClaimSet/Resource/...
    //    SAML: SamlAttributeStatement/Attribute/..., SamlAuthorizationDecisionStatement/@Resource/...
    // Right:
    //    DESC: Rights expressed about a resource
    //    XRML: ClaimSet/Right
    //    SAML: SamlAttributeStatement (aka. "PossessProperty") or, SamlAuthorizationDecisionStatement/Action/...

    [DataContract(Namespace = XsiConstants.Namespace)]
    public class Claim
    {
        static Claim system;

        [DataMember(Name = "ClaimType")]
        string claimType;
        [DataMember(Name = "Resource")]
        object resource;
        [DataMember(Name = "Right")]
        string right;

        IEqualityComparer<Claim> comparer;

        Claim(string claimType, object resource, string right, IEqualityComparer<Claim> comparer)
        {
            if (claimType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            if (claimType.Length <= 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", SR.GetString(SR.ArgumentCannotBeEmptyString));
            if (right == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("right");
            if (right.Length <= 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("right", SR.GetString(SR.ArgumentCannotBeEmptyString));

            this.claimType = StringUtil.OptimizeString(claimType);
            this.resource = resource;
            this.right = StringUtil.OptimizeString(right);
            this.comparer = comparer;
        }

        public Claim(string claimType, object resource, string right) : this(claimType, resource, right, null)
        {
        }

        public static IEqualityComparer<Claim> DefaultComparer
        {
            get
            {
                return EqualityComparer<Claim>.Default;
            }
        }

        public static Claim System
        {
            get
            {
                if (system == null)
                    system = new Claim(ClaimTypes.System, XsiConstants.System, Rights.Identity);

                return system;
            }
        }

        public object Resource
        {
            get { return this.resource; }
        }

        public string ClaimType
        {
            get { return this.claimType; }
        }

        public string Right
        {
            get { return this.right; }
        }

        // Turn key claims
        public static Claim CreateDnsClaim(string dns)
        {
            if (dns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dns");

            return new Claim(ClaimTypes.Dns, dns, Rights.PossessProperty, ClaimComparer.Dns);
        }

        public static Claim CreateDenyOnlyWindowsSidClaim(SecurityIdentifier sid)
        {
            if (sid == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");

            return new Claim(ClaimTypes.DenyOnlySid, sid, Rights.PossessProperty);
        }

        public static Claim CreateHashClaim(byte[] hash)
        {
            if (hash == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("hash");

            return new Claim(ClaimTypes.Hash, SecurityUtils.CloneBuffer(hash), Rights.PossessProperty, ClaimComparer.Hash);
        }

        public static Claim CreateMailAddressClaim(MailAddress mailAddress)
        {
            if (mailAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("mailAddress");

            return new Claim(ClaimTypes.Email, mailAddress, Rights.PossessProperty);
        }

        public static Claim CreateNameClaim(string name)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");

            return new Claim(ClaimTypes.Name, name, Rights.PossessProperty);
        }

        public static Claim CreateRsaClaim(RSA rsa)
        {
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");

            return new Claim(ClaimTypes.Rsa, rsa, Rights.PossessProperty, ClaimComparer.Rsa);
        }

        public static Claim CreateSpnClaim(string spn)
        {
            if (spn == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spn");

            return new Claim(ClaimTypes.Spn, spn, Rights.PossessProperty);
        }

        public static Claim CreateThumbprintClaim(byte[] thumbprint)
        {
            if (thumbprint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("thumbprint");

            return new Claim(ClaimTypes.Thumbprint, SecurityUtils.CloneBuffer(thumbprint), Rights.PossessProperty, ClaimComparer.Thumbprint);
        }

        public static Claim CreateUpnClaim(string upn)
        {
            if (upn == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upn");

            return new Claim(ClaimTypes.Upn, upn, Rights.PossessProperty, ClaimComparer.Upn);
        }

        public static Claim CreateUriClaim(Uri uri)
        {
            if (uri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");

            return new Claim(ClaimTypes.Uri, uri, Rights.PossessProperty);
        }

        public static Claim CreateWindowsSidClaim(SecurityIdentifier sid)
        {
            if (sid == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");

            return new Claim(ClaimTypes.Sid, sid, Rights.PossessProperty);
        }

        public static Claim CreateX500DistinguishedNameClaim(X500DistinguishedName x500DistinguishedName)
        {
            if (x500DistinguishedName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("x500DistinguishedName");

            return new Claim(ClaimTypes.X500DistinguishedName, x500DistinguishedName, Rights.PossessProperty, ClaimComparer.X500DistinguishedName);
        }

        public override bool Equals(object obj)
        {
            if (comparer == null)
                comparer = ClaimComparer.GetComparer(this.claimType);
            return comparer.Equals(this, obj as Claim);
        }

        public override int GetHashCode()
        {
            if (comparer == null)
                comparer = ClaimComparer.GetComparer(this.claimType);
            return comparer.GetHashCode(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}: {1}", this.right, this.claimType);
        }
    }
}
