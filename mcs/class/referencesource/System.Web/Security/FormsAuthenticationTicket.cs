//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationTicket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * FormsAuthenticationTicket class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Runtime.Serialization;


    /// <devdoc>
    ///    <para>This class encapsulates the information represented in
    ///       an authentication cookie as used by FormsAuthenticationModule.</para>
    /// </devdoc>
    [Serializable]
    public sealed class FormsAuthenticationTicket {

        /// <devdoc>
        ///    <para>A one byte version number for future
        ///       use.</para>
        /// </devdoc>
        public int       Version { get { return _Version;}}

        /// <devdoc>
        ///    The user name associated with the
        ///    authentication cookie. Note that, at most, 32 bytes are stored in the
        ///    cookie.
        /// </devdoc>
        public String    Name { get { return _Name;}}

        /// <devdoc>
        ///    The date/time at which the cookie
        ///    expires.
        /// </devdoc>
        public DateTime  Expiration { get { return _Expiration;}}

        /// <devdoc>
        ///    The time at which the cookie was originally
        ///    issued. This can be used for custom expiration schemes.
        /// </devdoc>
        public DateTime  IssueDate { get { return _IssueDate;}}

        /// <devdoc>
        ///    True if a durable cookie was issued.
        ///    Otherwise, the authentication cookie is scoped to the browser lifetime.
        /// </devdoc>
        public bool      IsPersistent { get { return _IsPersistent;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Expired {
            get {
                /*
                 * Two DateTime instances can only be compared if they are of the same DateTimeKind.
                 * Therefore we normalize everything to UTC to do the comparison. See comments on
                 * the ExpirationUtc property for more information
                 */
                return (ExpirationUtc < DateTime.UtcNow);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public String    UserData { get { return _UserData;}}


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public String    CookiePath { get { return _CookiePath;}}

        /*
         * We always prefer UTC expiration dates to work around issues like a daylight
         * saving time changes between the time the ticket was issued and the time the
         * ticket was checked. If we have a firm UTC expiration date, just use it
         * directly.
         * 
         * If we don't have a firm UTC expiration date, try converting the developer-
         * provided date to UTC before doing the comparison. There are three types of
         * DateTime, and the .NET Framework converts as so:
         * 
         * - The DateTime is already UTC, in which case it is returned unmodified.
         * - The DateTime is local, in which case the .NET Framework converts it to
         *   UTC. There is also a hidden bit in the DateTime struct which essentially
         *   states whether daylight saving time was active when this DateTime was
         *   generated, i.e. whether this was 2:02 AM PDT or 2:02 AM PST. The .NET
         *   framework handles round-tripping Local <-> UTC correctly, but comparisons
         *   can still fail as described in detail below.
         * - The DateTime is of an undefined type, in which case it is implicitly
         *   treated as local in a manner consistent with .NET 1.1.
         * 
         * However, this alone is insufficient to work around DST issues when comparing
         * local dates. For example, assume that a ticket is issued on Nov 6, 2011 at
         * 1:30 AM PDT (UTC -0700) with a timeout period of 20 minutes. The expiration
         * date is thus calculated to be Nov 6, 2011 at 1:50 AM PDT (UTC -0700). Now
         * say a request comes in 25 minutes after expiration; the time is currently
         * 1:15 AM PST (UTC -0800). [A DST boundary has been crossed.] Since this
         * request came in *after* the ticket expiration date, the ticket should be
         * rejected. And if we were doing all of our comparisons in UTC, this would
         * indeed be the case. However, since the DateTime struct doesn't have UTC
         * offset information embedded in it, comparisons of their dates are taken at
         * face value as simple wall time comparisons. Thus the current time is
         * interpreted just as "1:15 AM" and the expiration time is intepreted just as
         * "1:50 AM", and from this simple comparison the token is considered unexpired
         * and is accepted by the system.
         * 
         * To see this incorrect behavior in action, run the following on a machine
         * in the Pacific Time Zone. Contrast the behavior of the DateTimeOffset type
         * (which is designed to handle UTC offsets correctly) with the DateTime type, in
         * which the FromFileTime method implicitly does a local time conversion.
         * 
         * long ft1 = 129650430000000000; // Nov 6, 2011 1:50 AM PDT (UTC -0700)
         * long ft2 = 129650445000000000; // Nov 6, 2011 1:15 AM PST (UTC -0800)
         * DateTimeOffset.FromFileTime(ft1) < DateTimeOffset.FromFileTime(ft2) = true
         * DateTime.FromFileTime(ft1) < DateTime.FromFileTime(ft2) = false (INCORRECT!)
         * 
         * To be absolutely safe, we must perform comparisons *only* on DateTime instances
         * we know to have correct UTC information, or we must use an offset-aware type
         * like DateTimeOffset which just does the right thing automatically.
         * 
         * More info: http://msdn.microsoft.com/en-us/library/bb546099.aspx
         */
        internal DateTime ExpirationUtc {
            get { return (_ExpirationUtcHasValue) ? _ExpirationUtc : Expiration.ToUniversalTime(); }
        }

        internal DateTime IssueDateUtc {
            get { return (_IssueDateUtcHasValue) ? _IssueDateUtc : IssueDate.ToUniversalTime(); }
        }

        private int       _Version;
        private String    _Name;
        private DateTime  _Expiration;
        private DateTime  _IssueDate;
        private bool      _IsPersistent;
        private String    _UserData;
        private String    _CookiePath;

#pragma warning disable 0169 // unused field
        // These fields were added in .NET 4 but weren't actually used anywhere.
        // We can't remove them since they're part of the serialization contract.
        [OptionalField(VersionAdded = 2)]
        private int _InternalVersion;
        [OptionalField(VersionAdded = 2)]
        private Byte[] _InternalData;
#pragma warning restore 0169

        // Issue and expiration times as UTC.
        // We can't use nullable types since they didn't exist in v1.1, and this assists backporting fixes downlevel.
        [NonSerialized]
        private bool _ExpirationUtcHasValue;
        [NonSerialized]
        private DateTime _ExpirationUtc;
        [NonSerialized]
        private bool _IssueDateUtcHasValue;
        [NonSerialized]
        private DateTime _IssueDateUtc;


        /// <devdoc>
        ///    <para>This constructor creates a
        ///       FormsAuthenticationTicket instance with explicit values.</para>
        /// </devdoc>
        public FormsAuthenticationTicket(int version,
                                          String name,
                                          DateTime issueDate,
                                          DateTime expiration,
                                          bool isPersistent,
                                          String userData) {
            _Version = version;
            _Name = name;
            _Expiration = expiration;
            _IssueDate = issueDate;
            _IsPersistent = isPersistent;
            _UserData = userData;
            _CookiePath = FormsAuthentication.FormsCookiePath;
        }


        public FormsAuthenticationTicket(int version,
                                          String name,
                                          DateTime issueDate,
                                          DateTime expiration,
                                          bool isPersistent,
                                          String userData,
                                          String cookiePath) {
            _Version = version;
            _Name = name;
            _Expiration = expiration;
            _IssueDate = issueDate;
            _IsPersistent = isPersistent;
            _UserData = userData;
            _CookiePath = cookiePath;
        }



        /// <devdoc>
        ///    <para> This constructor creates
        ///       a FormsAuthenticationTicket instance with the specified name and cookie durability,
        ///       and default values for the other settings.</para>
        /// </devdoc>
        public FormsAuthenticationTicket(String name, bool isPersistent, Int32 timeout) {
            _Version = 2;
            _Name = name;
            _IssueDateUtcHasValue = true;
            _IssueDateUtc = DateTime.UtcNow;
            _IssueDate = DateTime.Now;
            _IsPersistent = isPersistent;
            _UserData = "";
            _ExpirationUtcHasValue = true;
            _ExpirationUtc = _IssueDateUtc.AddMinutes(timeout);
            _Expiration = _IssueDate.AddMinutes(timeout);
            _CookiePath = FormsAuthentication.FormsCookiePath;
        }

        internal static FormsAuthenticationTicket FromUtc(int version, String name, DateTime issueDateUtc, DateTime expirationUtc, bool isPersistent, String userData, String cookiePath) {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(version, name, issueDateUtc.ToLocalTime(), expirationUtc.ToLocalTime(), isPersistent, userData, cookiePath);

            ticket._IssueDateUtcHasValue = true;
            ticket._IssueDateUtc = issueDateUtc;
            ticket._ExpirationUtcHasValue = true;
            ticket._ExpirationUtc = expirationUtc;

            return ticket;
        }

    }
}
