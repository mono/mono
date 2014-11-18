//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    public static class ClaimTypes
    {
        const string claimTypeNamespace = XsiConstants.Namespace + "/claims";

        const string anonymous = claimTypeNamespace + "/anonymous";
        const string dns = claimTypeNamespace + "/dns";
        const string email = claimTypeNamespace + "/emailaddress";
        const string hash = claimTypeNamespace + "/hash";
        const string name = claimTypeNamespace + "/name";
        const string rsa = claimTypeNamespace + "/rsa";
        const string sid = claimTypeNamespace + "/sid";
        const string denyOnlySid = claimTypeNamespace + "/denyonlysid";
        const string spn = claimTypeNamespace + "/spn";
        const string system = claimTypeNamespace + "/system";
        const string thumbprint = claimTypeNamespace + "/thumbprint";
        const string upn = claimTypeNamespace + "/upn";
        const string uri = claimTypeNamespace + "/uri";
        const string x500DistinguishedName = claimTypeNamespace + "/x500distinguishedname";

        const string givenname = claimTypeNamespace + "/givenname";
        const string surname = claimTypeNamespace + "/surname";
        const string streetaddress = claimTypeNamespace + "/streetaddress";
        const string locality = claimTypeNamespace + "/locality";
        const string stateorprovince = claimTypeNamespace + "/stateorprovince";
        const string postalcode = claimTypeNamespace + "/postalcode";
        const string country = claimTypeNamespace + "/country";
        const string homephone = claimTypeNamespace + "/homephone";
        const string otherphone = claimTypeNamespace + "/otherphone";
        const string mobilephone = claimTypeNamespace + "/mobilephone";
        const string dateofbirth = claimTypeNamespace + "/dateofbirth";
        const string gender = claimTypeNamespace + "/gender";
        const string ppid = claimTypeNamespace + "/privatepersonalidentifier";
        const string webpage = claimTypeNamespace + "/webpage";
        const string nameidentifier = claimTypeNamespace + "/nameidentifier";
        const string authentication = claimTypeNamespace + "/authentication";
        const string authorizationdecision = claimTypeNamespace + "/authorizationdecision";

        static public string Anonymous { get { return anonymous; } }
        static public string DenyOnlySid { get { return denyOnlySid; } }
        static public string Dns { get { return dns; } }
        static public string Email { get { return email; } }
        static public string Hash { get { return hash; } }
        static public string Name { get { return name; } }
        static public string Rsa { get { return rsa; } }
        static public string Sid { get { return sid; } }
        static public string Spn { get { return spn; } }
        static public string System { get { return system; } }
        static public string Thumbprint { get { return thumbprint; } }
        static public string Upn { get { return upn; } }
        static public string Uri { get { return uri; } }
        static public string X500DistinguishedName { get { return x500DistinguishedName; } }
        static public string NameIdentifier { get { return nameidentifier; } }
        static public string Authentication { get { return authentication; } }
        static public string AuthorizationDecision { get { return authorizationdecision; } }

        // used in info card 
        static public string GivenName { get { return givenname; } }
        static public string Surname { get { return surname; } }
        static public string StreetAddress { get { return streetaddress; } }
        static public string Locality { get { return locality; } }
        static public string StateOrProvince { get { return stateorprovince; } }
        static public string PostalCode { get { return postalcode; } }
        static public string Country { get { return country; } }
        static public string HomePhone { get { return homephone; } }
        static public string OtherPhone { get { return otherphone; } }
        static public string MobilePhone { get { return mobilephone; } }
        static public string DateOfBirth { get { return dateofbirth; } }
        static public string Gender { get { return gender; } }
        static public string PPID { get { return ppid; } }
        static public string Webpage { get { return webpage; } }
    }
}
