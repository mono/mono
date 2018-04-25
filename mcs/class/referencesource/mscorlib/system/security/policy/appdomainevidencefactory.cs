// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace System.Security.Policy
{
    /// <summary>
    ///     Factory class which creates evidence on demand for an AppDomain
    /// </summary>
    internal sealed class AppDomainEvidenceFactory : IRuntimeEvidenceFactory
    {
        private AppDomain m_targetDomain;
        private Evidence m_entryPointEvidence;

        internal AppDomainEvidenceFactory(AppDomain target)
        {
            Contract.Assert(target != null);
            Contract.Assert(target == AppDomain.CurrentDomain, "AppDomainEvidenceFactory should not be used across domains.");

            m_targetDomain = target;
        }

        /// <summary>
        ///     AppDomain this factory generates evidence for
        /// </summary>
        public IEvidenceFactory Target
        {
            get { return m_targetDomain; }
        }

        /// <summary>
        ///     Return any evidence supplied by the AppDomain itself
        /// </summary>
        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            // AppDomains do not contain serialized evidence
            return new EvidenceBase[] { };
        }

        /// <summary>
        ///     Generate evidence on demand for an AppDomain
        /// </summary>
        [SecuritySafeCritical]
        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            // For v1.x compatibility, the default AppDomain has the same evidence as the entry point
            // assembly.  Since other AppDomains inherit their evidence from the default AppDomain by
            // default, they also use the entry point assembly.
            BCLDebug.Assert(m_targetDomain == AppDomain.CurrentDomain, "AppDomainEvidenceFactory should not be used across domains.");

            if (m_targetDomain.IsDefaultAppDomain())
            {
                // If we don't already know the evidence for the entry point assembly, get that now.  If we
                // have a RuntimeAssembly go directly to its EvidenceNoDemand property to avoid the full
                // demand that it will do on access to its Evidence property.
                if (m_entryPointEvidence == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    RuntimeAssembly entryRuntimeAssembly = entryAssembly as RuntimeAssembly;

                    if (entryRuntimeAssembly != null)
                    {
                        m_entryPointEvidence = entryRuntimeAssembly.EvidenceNoDemand.Clone();
                    }
                    else if (entryAssembly != null)
                    {
                        m_entryPointEvidence = entryAssembly.Evidence;
                    }
                }

                // If the entry point assembly provided evidence, then we use that for the AppDomain
                if (m_entryPointEvidence != null)
                {
                    return m_entryPointEvidence.GetHostEvidence(evidenceType);
                }
            }
            else
            {
                // If we're not the default domain, then we should inherit our evidence from the default
                // domain -- so ask it what evidence it has of this type.
                AppDomain defaultDomain = AppDomain.GetDefaultDomain();
                return defaultDomain.GetHostEvidence(evidenceType);
            }

            // AppDomains do not generate any evidence on demand
            return null;
        }
    }
}
