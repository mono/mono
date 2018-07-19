//------------------------------------------------------------------------------
// <copyright file="PeerName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{

    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Microsoft.Win32;
    /// <remarks>
    /// The PeerName class represents the native PeerName construct.
    /// </remarks>
    [Serializable]
    public class PeerName : ISerializable, IEquatable<PeerName>
    {
        //===================================================
        //constants
        //===================================================
        private const int PEER_MAX_CLASSIFIER_LENGTH = 149;
        private const int SECURE_AUTHORITY_LENGTH = 40;
        private const int INSECURE_AUTHORITY_LENGTH = 1;
        private const int PEER_MAX_PEERNAME_LENGTH = SECURE_AUTHORITY_LENGTH + 1 + PEER_MAX_CLASSIFIER_LENGTH;
        private const string PEERNAME_UNSECURED_AUTHORITY = "0";

        //===================================================
        //private member variables
        //===================================================
        private string m_PeerName;
        private string m_Authority;
        private string m_Classifier;
        private string m_PeerHostName;

        // ===================================================
        //     Constructors
        // ===================================================

        static PeerName()
        {
            //-------------------------------------------------
            //Check for the availability of the simpler PNRP APIs
            //-------------------------------------------------
            if (!PeerToPeerOSHelper.SupportsP2P)
            {
                throw new PlatformNotSupportedException(SR.GetString(SR.P2P_NotAvailable));
            }
        }

        /// <summary>
        /// We use this constructor to create an PeerName 
        /// for internal implementation
        /// </summary>
        private PeerName(string peerName, string authority, string classifier) {
            m_PeerName = peerName;
            m_Classifier = classifier;
            m_Authority = authority;
        }

        /// <summary>
        /// This constructor is to create a peername from an 
        /// arbitrary string. The Identity is not necessarily 
        /// the same as the current user
        /// </summary>
        /// <param name="peerName">string form of the peer name</param>
        /// <remarks>
        ///     1. We assume that the peername is already normalized according to the 
        ///         Unicode rules
        ///     2. The PeerName given has nothig to do with the default Identity
        ///         It is just a way to create a PeerName instance given a string
        /// </remarks>
        public PeerName(string remotePeerName)
        {
            //-------------------------------------------------
            //Check arguments
            //-------------------------------------------------
            if (remotePeerName == null)
                throw new ArgumentNullException("remotePeerName");

            //-------------------------------------------------
            //Check PeerName format
            //NOTE: Currentlt thre is no native API to check
            //the format of the PeerName string. The 
            //StrongParsePeerName implements the logic
            //-------------------------------------------------
            string authority;
            string classifier;
            if (!StrongParsePeerName(remotePeerName, out authority, out classifier))
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerName string {0} failed the check for a valid peer name", remotePeerName);
                throw new ArgumentException(SR.GetString(SR.Pnrp_InvalidPeerName), "remotePeerName");
            }
            //authrority would have been lower cased
            //so we must ste the m_PeerName to authority + classifier
            if (classifier != null)
                m_PeerName = authority + "." + classifier;
            else
                m_PeerName = authority;
            m_Authority = authority;
            m_Classifier = classifier;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerName instance created - PeerName {0} Authority {1} Classfier {2}", m_PeerName, m_Authority, ((m_Classifier == null)? "null" : m_Classifier));
        }

        /// <summary>
        /// Given a classifier creates a secured or unsecured name
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="secured"></param>
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerCreatePeerName(System.String,System.String,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerIdentityGetDefault(System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <ReferencesCritical Name="Local shNewPeerName of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Local shDefaultIdentity of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: SafePeerData.get_UnicodeString():System.String" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public PeerName(string classifier, PeerNameType peerNameType)
        {
            //-------------------------------------------------
            //Check arguments
            //-------------------------------------------------
            if ((classifier == null || classifier.Length == 0)&& 
                peerNameType == PeerNameType.Unsecured)
            {
                throw new ArgumentNullException("classifier");
            }
            if (classifier != null && classifier.Length > PEER_MAX_CLASSIFIER_LENGTH)
            {
                throw new ArgumentException(SR.GetString(SR.Pnrp_InvalidClassifier), "classifier");
            }
			//--------------------------------------------------
			//Normalize using NFC
			//--------------------------------------------------
            if (classifier != null && classifier.Length > 0)
            {
                classifier = classifier.Normalize(NormalizationForm.FormC);
            }
            //-------------------------------------------------
            //call the helper to create the PeerName
            //-------------------------------------------------
            Int32 result;
            SafePeerData shNewPeerName = null;
            SafePeerData shDefaultIdentity = null;
            try
            {
                if (peerNameType == PeerNameType.Unsecured)
                {
                    result = UnsafeP2PNativeMethods.PeerCreatePeerName((string)null, classifier, out shNewPeerName);
                    if (result != 0)
                    {
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotCreateUnsecuredPeerName), result);
                    }
                    m_Authority = PEERNAME_UNSECURED_AUTHORITY;
                }
                else
                {
                    result = UnsafeP2PNativeMethods.PeerIdentityGetDefault(out shDefaultIdentity);
                    if (result != 0)
                    {
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotGetDefaultIdentity), result);
                    }
                    m_Authority = shDefaultIdentity.UnicodeString;              
                    //}

                    result = UnsafeP2PNativeMethods.PeerCreatePeerName(m_Authority, classifier, out shNewPeerName);
                    if (result != 0)
                    {
                        throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotCreateSecuredPeerName), result);
                    }
                }
                m_PeerName = shNewPeerName.UnicodeString;
                m_Classifier = classifier;
            }
            finally
            {
                if (shNewPeerName != null) shNewPeerName.Dispose();
                if (shDefaultIdentity != null) shDefaultIdentity.Dispose();
            }
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerName instance created - PeerName {0} Authority {1} Classfier {2}", m_PeerName, m_Authority, m_Classifier);
        }

        /// <summary>
        /// Given a peerHostName string [the dns safe version of the PeerName]
        /// Get the PeerName for it
        /// </summary>
        /// <param name="peerHostName">string form (dns safe form) of the peer name</param>
        /// <returns>a PeerName instance</returns>
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerHostNameToPeerName(System.String,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <ReferencesCritical Name="Local shPeerName of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: SafePeerData.get_UnicodeString():System.String" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerName CreateFromPeerHostName(string peerHostName)
        {
            //-------------------------------------------------
            //Check arguments
            //-------------------------------------------------
            if (peerHostName == null)
                throw new ArgumentNullException("peerHostName");
            if (peerHostName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Pnrp_InvalidPeerHostName), "peerHostName");

            Int32 result;
            SafePeerData shPeerName = null;
            string peerName = null;
            try
            {
                result = UnsafeP2PNativeMethods.PeerHostNameToPeerName(peerHostName, out shPeerName);
                if (result != 0)
                {
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotGetPeerNameFromPeerHostName), result);
                }
                peerName = shPeerName.UnicodeString;
            }
            finally
            {
                if (shPeerName != null) shPeerName.Dispose();
            }

            string authority;
            string classifier;
            WeakParsePeerName(peerName, out authority, out classifier);
            PeerName p = new PeerName(peerName, authority, classifier);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerName created from PeerHostName - PeerHostName {0} to PeerName PeerName {1} Authority {2} Classfier {3}", peerHostName, peerName, authority, classifier);
            return p; 
        }
          
        /// <summary>
        /// Given a PeerName, change the classifier. 
        /// This is simply replacing the classifier, but we will
        /// let the native APIs do the right thing since they
        /// might change the concept in future
        /// </summary>
        /// <param name="peerName">a PeerName instance</param>
        /// <param name="classifier">a new classfier string</param>
        /// <returns></returns>
        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
        // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerCreatePeerName(System.String,System.String,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
        // <ReferencesCritical Name="Local shNewPeerName of type: SafePeerData" Ring="1" />
        // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
        // <ReferencesCritical Name="Method: SafePeerData.get_UnicodeString():System.String" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        public static PeerName CreateRelativePeerName(
                                        PeerName peerName,
                                        string classifier)
        {
            //-------------------------------------------------
            //Check arguments
            //-------------------------------------------------
            if (peerName == null)
            {
                throw new ArgumentNullException("peerName", SR.GetString(SR.Pnrp_PeerNameCantBeNull));
            }
            if (!peerName.IsSecured && (classifier == null || classifier.Length == 0))
            {
                throw new ArgumentException(SR.GetString(SR.Pnrp_InvalidClassifier), "classifier");
            }
            if (classifier != null && classifier.Length > PEER_MAX_CLASSIFIER_LENGTH)
            {
                throw new ArgumentException(SR.GetString(SR.Pnrp_InvalidClassifier), "classifier");
            }

			//--------------------------------------------------
			//Normalize using NFC
			//--------------------------------------------------
            if (classifier != null && classifier.Length > 0)
            {
                classifier = classifier.Normalize(NormalizationForm.FormC);
            }

            Int32 result;
            SafePeerData shNewPeerName = null;
            string newPeerName = null;
            try
            {
                //Here there is change made on the native side 
                //when passing secured peer names, it takes string of the form [40hexdigits].claasisifer and a newclassifier 
                //returns [40hexdigits.newclassifier]
                //But for unsecured peer names it does not take 0.clasfier and newclassfier to return 0.newclassfier. 
                //It expects NULL as the first param. To satisfy this broken finctionality, we are passing null if the 
                //peer name is unsecured. 
                result = UnsafeP2PNativeMethods.PeerCreatePeerName(peerName.IsSecured? peerName.m_PeerName : null, classifier, out shNewPeerName);
                if (result != 0)
                {
                    throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotCreateRelativePeerName), result);
                }
                newPeerName = shNewPeerName.UnicodeString;
            }
            finally
            {
                if (shNewPeerName != null) shNewPeerName.Dispose();
            }

            string authority;
            string newClassifier;
            WeakParsePeerName(newPeerName, out authority, out newClassifier);
            PeerName p = new PeerName(newPeerName, authority, newClassifier);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "A new PeerName created from existing PeerName with a new classfier. Existing PeerName {0} Classifier {1} New PeerName {2}", peerName, classifier, p);
            return p; 
        }

        /// <summary>
        /// Friendly string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_PeerName;
        }

        /// <summary>
        /// Notionb of equals is based on same peer name string content
        /// </summary>
        /// <param name="comparand"></param>
        /// <returns></returns>
        public bool Equals(PeerName other)
        {
            if(other == null) return false;
            return string.Compare(other.m_PeerName, m_PeerName, StringComparison.Ordinal) == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PeerName other = obj as PeerName;
            if (other == null) return false;
            return Equals(other);
      
        }
        /// <summary>
        /// Hash code comes from m_PeerName since the others are just components of this
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_PeerName.GetHashCode();
        }

        /// <summary>
        /// Authroity
        /// </summary>
        public string Authority
        {
            get
            {
                return m_Authority;
            }
        }
        /// <summary>
        /// Classifier
        /// </summary>
        public string Classifier
        {
            get
            {
                return m_Classifier;
            }
        }

        /// <summary>
        /// A DNS Safe version of the Peer Name
        /// </summary>
        public string PeerHostName
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <SatisfiesLinkDemand Name="SafeHandle.Dispose():System.Void" />
            // <CallsSuppressUnmanagedCode Name="UnsafeP2PNativeMethods.PeerNameToPeerHostName(System.String,System.Net.PeerToPeer.SafePeerData&):System.Int32" />
            // <ReferencesCritical Name="Local shPeerHostName of type: SafePeerData" Ring="1" />
            // <ReferencesCritical Name="Method: PeerToPeerException.CreateFromHr(System.String,System.Int32):System.Net.PeerToPeer.PeerToPeerException" Ring="1" />
            // <ReferencesCritical Name="Method: SafePeerData.get_UnicodeString():System.String" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            get
            {
                if (m_PeerHostName == null)
                {
                    Int32 result;
                    SafePeerData shPeerHostName = null;
                    try
                    {
						//This API gives HRESULT > 0 for success instead of S_OK == 0
						//WINDOWS OS 

                        result = UnsafeP2PNativeMethods.PeerNameToPeerHostName( m_PeerName, out shPeerHostName);
                        if (result < 0)
                        {
                            throw PeerToPeerException.CreateFromHr(SR.GetString(SR.Pnrp_CouldNotGetPeerHostNameFromPeerName), result);
                        }
                        m_PeerHostName = shPeerHostName.UnicodeString;
                    }
                    finally
                    {
                        if (shPeerHostName != null) shPeerHostName.Dispose();
                    }
                }
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerHostName created for PeerName PeerHostName {0} PeerName {1}", m_PeerHostName, this);
                return m_PeerHostName;
            }
        }

        /// <summary>
        /// Basically if the authority == 0 or not [unsecured or secured respectively]
        /// </summary>
        public bool IsSecured
        {
            get
            {
                return m_Authority != PEERNAME_UNSECURED_AUTHORITY;
            }
        }


        // ===================================================
        // Private methods
        // ===================================================
        /// <summary>
        /// I have considered using regular expressions. However, the regular expressions offer 
        /// poor performance and or startup cost. Really there is no substiture for custom 
        /// parsing logic. I decided to write this piece of code to parse the peername for now 
        /// - Microsoft 6/6/2005
        /// </summary>
        /// <param name="peerName"></param>
        /// <param name="authority"></param>
        /// <param name="classifier"></param>
        /// <returns></returns>
        private static bool StrongParsePeerName(string peerName, out string authority, out string classifier)
        {
            authority = null;
            classifier = null;
            
            //Rule 0. The max length must not be exeeded
            if (peerName == null ||
                peerName.Length == 0 ||
                peerName.Length > PEER_MAX_PEERNAME_LENGTH) return false;

            //Rule 1. The length of the string must be at least 3
            //as in 0.C for unsecured PeerName with classifier "C"
            if (peerName.Length < 3) return false;

            string tempAuthority = null;
            int IndexOfPeriod = peerName.IndexOf('.');
            if (IndexOfPeriod == 0) return false;
            if (IndexOfPeriod < 0)
            {
                //Rule 2. Secure PeerNames can have just the authority 
                //or Authority. or Authority.Classifier
                //if there is no period, we have to treat the entire string as the authority
                tempAuthority = peerName;
            }
            else if (IndexOfPeriod == peerName.Length - 1)
            {
                //May be this is of the form Authority.
                tempAuthority = peerName.Substring(0, peerName.Length - 1);
            }
            else
            {
                //Rule 2B. Unsecure peer names must have a classifier
                //There must be a period separating the authority and classifier
                //and must not be the first character and it must not be the last character
                tempAuthority = peerName.Substring(0, IndexOfPeriod);
            }

            //Rule 3. Authority is either SECURE_AUTHORITY_LENGTH hex characters or "0"
            if (tempAuthority.Length != SECURE_AUTHORITY_LENGTH &&
                tempAuthority.Length != INSECURE_AUTHORITY_LENGTH) return false;

            //Rule 4. If it is length 1 it must be 0
            if (tempAuthority.Length == INSECURE_AUTHORITY_LENGTH && tempAuthority != PEERNAME_UNSECURED_AUTHORITY)
                return false;

            //Rule 5. the authority must be 40 hex characters
            if(tempAuthority.Length == SECURE_AUTHORITY_LENGTH)
            {
                foreach (char c in tempAuthority)
                {
                    if (!IsHexDigit(c)) return false;
                }
            }
            //Rule 6. The maximum length of the classfier is PEER_MAX_CLASSIFIER_LENGTH
            string tempClassifier = null;
            if (IndexOfPeriod != peerName.Length - 1 && IndexOfPeriod > 0)
            {
                tempClassifier = peerName.Substring(IndexOfPeriod + 1);
            }
            if (tempClassifier != null && tempClassifier.Length > PEER_MAX_CLASSIFIER_LENGTH) return false;

            //Finish
            authority = tempAuthority.ToLower(CultureInfo.InvariantCulture); //Safe for Hex digits
            classifier = tempClassifier;
            return true;
        }
        private static bool IsHexDigit(char character)
        {
            return ((character >= '0') && (character <= '9'))
                || ((character >= 'A') && (character <= 'F'))
                || ((character >= 'a') && (character <= 'f'));
        }

        /// <summary>
        /// WARNING: Don't call this unless you are sure that 
        /// the PeerName is a valid string. This is invoked only when 
        /// we are sure that the peername is valid and we need to components
        /// </summary>
        /// <param name="peerName">in: peerName to parse</param>
        /// <param name="authority">out: parsed authority value</param>
        /// <param name="classifier">out: parsed classfier value</param>
        /// <returns>none</returns>
        private static void WeakParsePeerName(string peerName, out string authority, out string classifier)
        {
            authority = null;
            classifier = null;
            int indexOfPeriod = peerName.IndexOf('.');
            if (indexOfPeriod < 0)
            {
                authority = peerName;
            }
            else if (indexOfPeriod == peerName.Length - 1)
            {
                //May be this is of the form Authority.
                authority = peerName.Substring(0, peerName.Length - 1);
            }
            else
            {
                authority = peerName.Substring(0, indexOfPeriod);
            }

            if (indexOfPeriod != peerName.Length - 1 && indexOfPeriod > 0)
            {
                classifier = peerName.Substring(indexOfPeriod + 1);
            }
            authority = authority.ToLower(CultureInfo.InvariantCulture); //safe for hex strings
        }

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerName(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            m_PeerName = info.GetString("_PeerName");
            m_Authority = info.GetString("_Authority");
            m_Classifier = info.GetString("_Classifier");
        }


        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="GetObjectData(SerializationInfo, StreamingContext):Void" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// This is made virtual so that derived types can be implemented correctly
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_PeerName", m_PeerName);
            info.AddValue("_Authority", m_Authority);
            info.AddValue("_Classifier", m_Classifier);
        }
		
    }
}
