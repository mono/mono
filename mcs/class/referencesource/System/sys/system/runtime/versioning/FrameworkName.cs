using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace System.Runtime.Versioning {

    [Serializable]
    public sealed class FrameworkName : IEquatable<FrameworkName> {

        // ---- SECTION:  members supporting exposed properties -------------*
        #region members supporting exposed properties
        readonly String m_identifier = null;
        readonly Version m_version = null;
        readonly String m_profile = null;
        String m_fullName = null;

        const Char c_componentSeparator = ',';
        const Char c_keyValueSeparator = '=';
        const Char c_versionValuePrefix = 'v';
        const String c_versionKey = "Version";
        const String c_profileKey = "Profile";
        #endregion members supporting exposed properties


        // ---- SECTION: public properties --------------*
        #region public properties
        public String Identifier {
            get {
                Contract.Assert(m_identifier != null);
                return m_identifier;
            }
        }

        public Version Version {
            get {
                Contract.Assert(m_version != null);
                return m_version;
            }
        }

        public String Profile {
            get {
                Contract.Assert(m_profile != null);
                return m_profile;
            }
        }

        public String FullName {
            get {
                if (m_fullName == null) {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(Identifier);
                    sb.Append(c_componentSeparator);
                    sb.Append(c_versionKey).Append(c_keyValueSeparator);
                    sb.Append(c_versionValuePrefix);
                    sb.Append(Version);
                    if (!String.IsNullOrEmpty(Profile)) {
                        sb.Append(c_componentSeparator);
                        sb.Append(c_profileKey).Append(c_keyValueSeparator);
                        sb.Append(Profile);
                    }                    
                    m_fullName = sb.ToString();
                }
                Contract.Assert(m_fullName != null);
                return m_fullName;
            }
        }
        #endregion public properties


        // ---- SECTION: public instance methods --------------*
        #region public instance methods

        public override Boolean Equals(Object obj) {
            return Equals(obj as FrameworkName);
        }

        public Boolean Equals(FrameworkName other) {
            if (Object.ReferenceEquals(other, null)) {
                return false;
            }

            return Identifier == other.Identifier &&
                Version == other.Version &&
                Profile == other.Profile;
        }

        public override Int32 GetHashCode() {
            return Identifier.GetHashCode() ^ Version.GetHashCode() ^ Profile.GetHashCode();
        }

        public override String ToString() {
            return FullName;
        }
        #endregion public instance methods


        // -------- SECTION: constructors -----------------*
        #region constructors

        public FrameworkName(String identifier, Version version)
            : this(identifier, version, null) {}

        public FrameworkName(String identifier, Version version, String profile) {
            if (identifier == null) {
                throw new ArgumentNullException("identifier");
            }
            if (identifier.Trim().Length == 0) {
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall, "identifier"), "identifier");
            }
            if (version == null) {
                throw new ArgumentNullException("version");
            }
            Contract.EndContractBlock();

            m_identifier = identifier.Trim();
            m_version = (Version)version.Clone();
            m_profile = (profile == null) ? String.Empty : profile.Trim();
        }

        // Parses strings in the following format: "<identifier>, Version=[v|V]<version>, Profile=<profile>"
        //  - The identifier and version is required, profile is optional
        //  - Only three components are allowed.
        //  - The version string must be in the System.Version format; an optional "v" or "V" prefix is allowed
        public FrameworkName(String frameworkName) {
            if (frameworkName == null) {
                throw new ArgumentNullException("frameworkName");
            }
            if (frameworkName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall, "frameworkName"), "frameworkName");
            }
            Contract.EndContractBlock();

            string[] components = frameworkName.Split(c_componentSeparator);

            // Identifer and Version are required, Profile is optional.
            if (components.Length < 2 || components.Length > 3) {
                throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameTooShort), "frameworkName");
            }

            //
            // 1) Parse the "Identifier", which must come first. Trim any whitespace
            //
            m_identifier = components[0].Trim();

            if (m_identifier.Length == 0) {
                    throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameInvalid), "frameworkName");
            }          

            bool versionFound = false;
            m_profile = String.Empty;

            // 
            // The required "Version" and optional "Profile" component can be in any order
            //
            for (int i = 1; i < components.Length; i++) {
                // Get the key/value pair separated by '='
                string[] keyValuePair = components[i].Split(c_keyValueSeparator);

                if (keyValuePair.Length != 2) {
                    throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameInvalid), "frameworkName");
                }

                // Get the key and value, trimming any whitespace
                string key = keyValuePair[0].Trim();
                string value = keyValuePair[1].Trim();

                //
                // 2) Parse the required "Version" key value
                //
                if (key.Equals(c_versionKey, StringComparison.OrdinalIgnoreCase)) {
                    versionFound = true;

                    // Allow the version to include a 'v' or 'V' prefix...
                    if (value.Length > 0 && (value[0] == c_versionValuePrefix || value[0] == 'V')) {
                        value = value.Substring(1);
                    }
                    try {
                        m_version = new Version(value);
                    }
                    catch (Exception e) {
                        throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameInvalidVersion), "frameworkName", e);
                    }
                }
                //
                // 3) Parse the optional "Profile" key value
                //
                else if (key.Equals(c_profileKey, StringComparison.OrdinalIgnoreCase)) {
                    if (!String.IsNullOrEmpty(value)) {
                        m_profile = value;
                    }
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameInvalid), "frameworkName");
                }
            }

            if (!versionFound) {
                throw new ArgumentException(SR.GetString(SR.Argument_FrameworkNameMissingVersion), "frameworkName");
            }
        }
        #endregion constructors


        // -------- SECTION: public static methods -----------------*
        #region public static methods
        public static Boolean operator ==(FrameworkName left, FrameworkName right) {
            if (Object.ReferenceEquals(left, null)) {
                return Object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static Boolean operator !=(FrameworkName left, FrameworkName right) {
            return !(left == right);
        }
        #endregion public static methods
    }
}
