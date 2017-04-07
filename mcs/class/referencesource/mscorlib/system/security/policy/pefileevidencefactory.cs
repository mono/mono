// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Policy
{
    /// <summary>
    ///     Arguments to the ETW evidence generation event.  This enumeration should be kept in sync with
    ///     the VM enumeration EvidenceType in SecurityPolicy.h.
    /// </summary>
    internal enum EvidenceTypeGenerated
    {
        AssemblySupplied,
        Gac,
        Hash,
        PermissionRequest,
        Publisher,
        Site,
        StrongName,
        Url,
        Zone
    }

    /// <summary>
    ///     Factory class which can create evidence on demand for a VM PEFile
    /// </summary>
    internal sealed class PEFileEvidenceFactory : IRuntimeEvidenceFactory
    {
        [System.Security.SecurityCritical] // auto-generated
        private SafePEFileHandle m_peFile;

        private List<EvidenceBase> m_assemblyProvidedEvidence;

        // Since all three of these evidence objects are generated from the same source data, we'll generate
        // all three when we're asked for any one of them and save them around in case we're asked for the
        // others.
        bool m_generatedLocationEvidence;
        private Site m_siteEvidence;
        private Url m_urlEvidence;
        private Zone m_zoneEvidence;

        [SecurityCritical]
        private PEFileEvidenceFactory(SafePEFileHandle peFile)
        {
            Contract.Assert(peFile != null &&
                            !peFile.IsClosed &&
                            !peFile.IsInvalid);
            m_peFile = peFile;
        }

        /// <summary>
        ///     PEFile * that we generate evidence for
        /// </summary>
        internal SafePEFileHandle PEFile
        {
            [SecurityCritical]
            get { return m_peFile; }
        }

        /// <summary>
        ///     Object the supplied evidence is for
        /// </summary>
        public IEvidenceFactory Target
        {
            // Since the CLR does not have a PEFile abstraction and this PEFile may not have an associated
            // assembly if we're early in runtime startup, there is no valid target object to return here.
            get { return null; }
        }

        /// <summary>
        ///     Generate an evidence collection the PE file.  This is called from the the VM in
        ///     SecurityDescriptor::GetEvidenceForPEFile. 
        /// </summary>
        [SecurityCritical]
        private static Evidence CreateSecurityIdentity(SafePEFileHandle peFile,
                                                       Evidence hostProvidedEvidence)
        {

            PEFileEvidenceFactory evidenceFactory = new PEFileEvidenceFactory(peFile);
            Evidence evidence = new Evidence(evidenceFactory);

            // If the host (caller of Assembly.Load) provided evidence, merge it with the evidence we've just
            // created. The host evidence takes priority.
            if (hostProvidedEvidence != null)
            {
                evidence.MergeWithNoDuplicates(hostProvidedEvidence);
            }

            return evidence;
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void FireEvidenceGeneratedEvent(SafePEFileHandle peFile,
                                                              EvidenceTypeGenerated type);

        /// <summary>
        ///     Fire an ETW event indicating that a piece of evidence has been generated.  Evidence that is
        ///     generated in the VM fires this event without a seperate call to this method, however
        ///     evidence types generated in the BCL, such as GacInstalled, need to call this directly.
        /// </summary>
        [SecuritySafeCritical]
        internal void FireEvidenceGeneratedEvent(EvidenceTypeGenerated type)
        {
            FireEvidenceGeneratedEvent(m_peFile, type);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetAssemblySuppliedEvidence(SafePEFileHandle peFile,
                                                               ObjectHandleOnStack retSerializedEvidence);

        /// <summary>
        ///     Get any evidence that was serialized into the PE File
        /// </summary>
        [SecuritySafeCritical]
        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            if (m_assemblyProvidedEvidence == null)
            {
                byte[] serializedEvidence = null;
                GetAssemblySuppliedEvidence(m_peFile, JitHelpers.GetObjectHandleOnStack(ref serializedEvidence));

                m_assemblyProvidedEvidence = new List<EvidenceBase>();
                if (serializedEvidence != null)
                {
                    Evidence deserializedEvidence = new Evidence();

                    // Partial trust assemblies can provide their own evidence, so make sure that we have
                    // permission to deserialize it
                    new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();

                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        using (MemoryStream ms = new MemoryStream(serializedEvidence))
                        {
                            deserializedEvidence = (Evidence)formatter.Deserialize(ms);
                        }
                    }
                    catch { /* Ignore any errors deserializing */ }

                    CodeAccessPermission.RevertAssert();

                    // Enumerate the assembly evidence, ignoring any host evidence supplied.  Since we
                    // could be loading a Whidbey assembly, we need to use the old GetAssemblyEnumerator
                    // API and deal with objects instead of EvidenceBases.
                    if (deserializedEvidence != null)
                    {
                        IEnumerator enumerator = deserializedEvidence.GetAssemblyEnumerator();

                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current != null)
                            {
                                // If this is a legacy evidence object, we need to wrap it before
                                // returning it.
                                EvidenceBase currentEvidence = enumerator.Current as EvidenceBase;
                                if (currentEvidence == null)
                                {
                                    currentEvidence = new LegacyEvidenceWrapper(enumerator.Current);
                                }

                                m_assemblyProvidedEvidence.Add(currentEvidence);
                            }
                        }
                    }
                }
            }

            return m_assemblyProvidedEvidence;
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetLocationEvidence(SafePEFileHandle peFile,
                                                       [Out] out SecurityZone zone,
                                                       StringHandleOnStack retUrl);

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetPublisherCertificate(SafePEFileHandle peFile,
                                                           ObjectHandleOnStack retCertificate);

        /// <summary>
        ///     Called to generate different types of evidence on demand
        /// </summary>
        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            if (evidenceType == typeof(Site))
            {
                return GenerateSiteEvidence();
            }
            else if (evidenceType == typeof(Url))
            {
                return GenerateUrlEvidence();
            }
            else if (evidenceType == typeof(Zone))
            {
                return GenerateZoneEvidence();
            }
            else if (evidenceType == typeof(Publisher))
            {
                return GeneratePublisherEvidence();
            }

            return null;
        }

        /// <summary>
        ///     Generate Site, Url, and Zone evidence for this file.
        /// </summary>
        [SecuritySafeCritical]
        private void GenerateLocationEvidence()
        {
            if (!m_generatedLocationEvidence)
            {
                SecurityZone securityZone = SecurityZone.NoZone;
                string url = null;
                GetLocationEvidence(m_peFile, out securityZone, JitHelpers.GetStringHandleOnStack(ref url));

                if (securityZone != SecurityZone.NoZone)
                {
                    m_zoneEvidence = new Zone(securityZone);
                }

                if (!String.IsNullOrEmpty(url))
                {
                    m_urlEvidence = new Url(url, true);

                    // We only create site evidence if the URL does not with file:
                    if (!url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    {
                        m_siteEvidence = Site.CreateFromUrl(url);
                    }
                }

                m_generatedLocationEvidence = true;
            }
        }

        /// <summary>
        ///     Generate evidence for the file's Authenticode signature
        /// </summary>
        [SecuritySafeCritical]
        private Publisher GeneratePublisherEvidence()
        {
            byte[] certificate = null;
            GetPublisherCertificate(m_peFile, JitHelpers.GetObjectHandleOnStack(ref certificate));

            if (certificate == null)
            {
                return null;
            }

            return new Publisher(new X509Certificate(certificate));
        }

        /// <summary>
        ///     Generate evidence for the site this file was loaded from
        /// </summary>
        private Site GenerateSiteEvidence()
        {
            if (m_siteEvidence == null)
            {
                GenerateLocationEvidence();
            }

            return m_siteEvidence;
        }

        /// <summary>
        ///     Generate evidence for the URL this file was loaded from
        /// </summary>
        private Url GenerateUrlEvidence()
        {
            if (m_urlEvidence == null)
            {
                GenerateLocationEvidence();
            }

            return m_urlEvidence;
        }

        /// <summary>
        ///     Generate evidence for the zone this file was loaded from
        /// </summary>
        private Zone GenerateZoneEvidence()
        {
            if (m_zoneEvidence == null)
            {
                GenerateLocationEvidence();
            }

            return m_zoneEvidence;
        }
    }
}
