//------------------------------------------------------------------------------
// <copyright file="Authorization.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    /// <devdoc>
    ///    <para>Used for handling and completing a custom authorization.</para>
    /// </devdoc>
    public class Authorization {

        private string                  m_Message;
        private bool                    m_Complete;
        private string[]                m_ProtectionRealm;
        private string                  m_ConnectionGroupId;
        private bool                    m_MutualAuth;

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.Authorization'/> class with the specified
        ///       authorization token.
        ///    </para>
        /// </devdoc>
        public Authorization(string token) {
            m_Message = ValidationHelper.MakeStringNull(token);
            m_Complete = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.Authorization'/> class with the specified
        ///       authorization token and completion status.
        ///    </para>
        /// </devdoc>
        public Authorization(string token, bool finished) {
            m_Message = ValidationHelper.MakeStringNull(token);
            m_Complete = finished;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.Authorization'/> class with the specified
        ///       authorization token, completion status, and connection m_ConnectionGroupId identifier.
        ///    </para>
        /// </devdoc>
        public Authorization(string token, bool finished, string connectionGroupId): this(token, finished, connectionGroupId, false) {
        }
        //
        internal Authorization(string token, bool finished, string connectionGroupId, bool mutualAuth) {
            m_Message = ValidationHelper.MakeStringNull(token);
            m_ConnectionGroupId = ValidationHelper.MakeStringNull(connectionGroupId);
            m_Complete = finished;
            m_MutualAuth = mutualAuth;
        }

        /// <devdoc>
        ///    <para>Gets
        ///       the response returned to the server in response to an authentication
        ///       challenge.</para>
        /// </devdoc>
        public string Message {
            get { return m_Message;}
        }

        // used to specify if this Authorization needs a special private server connection,
        //  identified by this string
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ConnectionGroupId {
            get { return m_ConnectionGroupId; }
        }

        /// <devdoc>
        ///    <para>Gets the completion status of the authorization.</para>
        /// </devdoc>
        public bool Complete {
            get { return m_Complete;}
        }
        internal void SetComplete(bool complete) {
            m_Complete = complete;
        }

        /// <devdoc>
        /// <para>Gets or sets the prefix for Uris that can be authenticated with the <see cref='System.Net.Authorization.Message'/> property.</para>
        /// </devdoc>
        public string[] ProtectionRealm {
            get { return m_ProtectionRealm;}
            set {
                string[] newValue = ValidationHelper.MakeEmptyArrayNull(value);
                m_ProtectionRealm = newValue;
            }
        }

        //
        //
        public bool MutuallyAuthenticated {
            get {
                return Complete && m_MutualAuth;
            }
            set {
                m_MutualAuth = value;
            }
        }

    } // class Authorization


} // namespace System.Net
