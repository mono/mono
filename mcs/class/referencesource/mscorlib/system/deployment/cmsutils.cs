using Microsoft.Win32;
using System;
using System.Deployment.Internal;
using System.Deployment.Internal.Isolation;
using System.Deployment.Internal.Isolation.Manifest;
using System.IO;
using System.Globalization;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Runtime.Versioning;
using System.Diagnostics.Contracts;

namespace System.Deployment.Internal.Isolation.Manifest {
    [System.Security.SecuritySafeCritical]  // auto-generated
    [SecurityPermissionAttribute(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal static class CmsUtils {

        internal static void GetEntryPoint (ActivationContext activationContext, out string fileName, out string parameters) {
            parameters = null;
            fileName = null;

            ICMS appManifest = activationContext.ApplicationComponentManifest;
            if (appManifest == null || appManifest.EntryPointSection == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_NoMain"));

            IEnumUnknown refEnum = (IEnumUnknown) appManifest.EntryPointSection._NewEnum;
            uint count = 0;
            Object[] entries = new Object[1];
            // Look for the first entry point. ClickOnce semantic validation ensures exactly one entry point is present.
            if (refEnum.Next(1, entries, ref count) == 0 && count == 1) {
                IEntryPointEntry iref= (IEntryPointEntry) entries[0];
                EntryPointEntry reference = iref.AllData;
                if (reference.CommandLine_File != null && reference.CommandLine_File.Length > 0) {
                    fileName = reference.CommandLine_File;
                } else {
                    // Locate the dependent assembly that is being refered to. Well-formed manifests should have an identity.
                    IAssemblyReferenceEntry refEntry = null;
                    object assemblyObj = null;
                    if (reference.Identity != null) {
                        ((ISectionWithReferenceIdentityKey)appManifest.AssemblyReferenceSection).Lookup(reference.Identity, out assemblyObj);
                        refEntry = (IAssemblyReferenceEntry) assemblyObj;
                        fileName = refEntry.DependentAssembly.Codebase;
                    }
                }
                parameters = reference.CommandLine_Parameters;
            }
        }

        internal static IAssemblyReferenceEntry[] GetDependentAssemblies(ActivationContext activationContext)
        {
            IAssemblyReferenceEntry[] entries = null;
            ICMS appManifest = activationContext.ApplicationComponentManifest;
            if (appManifest == null)
                return null;
            
            ISection dependencySection =  appManifest.AssemblyReferenceSection;
            uint count = (dependencySection != null) ? dependencySection.Count : 0;
            if (count > 0)
            {
                uint fetched = 0;
                entries = new IAssemblyReferenceEntry[count];
                IEnumUnknown dependencyEnum = (IEnumUnknown)dependencySection._NewEnum;
                int hr = dependencyEnum.Next(count, entries, ref fetched);
                if (fetched != count || hr < 0)
                    return null; //
            }
            return entries;
        }
                    

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetEntryPointFullPath (ActivationArguments activationArguments) {
            return GetEntryPointFullPath(activationArguments.ActivationContext);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetEntryPointFullPath (ActivationContext activationContext) {
            string file, parameters;
            GetEntryPoint(activationContext, out file, out parameters);

            if (!String.IsNullOrEmpty(file)) {
                string directoryName = activationContext.ApplicationDirectory;
                if (directoryName == null || directoryName.Length == 0) {
                    // If we were passed a relative path, assume the app base is the current working directory
                    StringBuilder sb = new StringBuilder(Path.MAX_PATH + 1);
                    if (Win32Native.GetCurrentDirectory(sb.Capacity, sb) == 0)
                        System.IO.__Error.WinIOError();
                    directoryName = sb.ToString();
                }

                file = Path.Combine(directoryName, file);
            }

            return file;
        }

        internal static bool CompareIdentities (ActivationContext activationContext1, ActivationContext activationContext2) {
            if (activationContext1 == null || activationContext2 == null)
                return activationContext1 == activationContext2;
            return IsolationInterop.AppIdAuthority.AreDefinitionsEqual(0, activationContext1.Identity.Identity, activationContext2.Identity.Identity);
        }

        internal static bool CompareIdentities (ApplicationIdentity applicationIdentity1, ApplicationIdentity applicationIdentity2, ApplicationVersionMatch versionMatch) {
            if (applicationIdentity1 == null || applicationIdentity2 == null)
                return applicationIdentity1 == applicationIdentity2;
            uint flags;
            switch (versionMatch) {
            case ApplicationVersionMatch.MatchExactVersion:
                flags = 0;
                break;
            case ApplicationVersionMatch.MatchAllVersions:
                flags = (uint) IAPPIDAUTHORITY_ARE_DEFINITIONS_EQUAL_FLAGS.IAPPIDAUTHORITY_ARE_DEFINITIONS_EQUAL_FLAG_IGNORE_VERSION;
                break;
            default:
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)versionMatch), "versionMatch");
            }
            return IsolationInterop.AppIdAuthority.AreDefinitionsEqual(flags, applicationIdentity1.Identity, applicationIdentity2.Identity);
        }

        internal static string GetFriendlyName (ActivationContext activationContext) {
            ICMS deplManifest = activationContext.DeploymentComponentManifest;
            IMetadataSectionEntry metadataSectionEntry = (IMetadataSectionEntry) deplManifest.MetadataSectionEntry;
            IDescriptionMetadataEntry descriptionMetadataEntry = metadataSectionEntry.DescriptionData;
            string friendlyName = String.Empty;
            if (descriptionMetadataEntry != null) {
                DescriptionMetadataEntry entry = descriptionMetadataEntry.AllData;
                friendlyName = (entry.Publisher != null ? String.Format("{0} {1}", entry.Publisher, entry.Product) : entry.Product);
            }
            return friendlyName;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static void CreateActivationContext (string fullName, string[] manifestPaths, bool useFusionActivationContext, out ApplicationIdentity applicationIdentity, out ActivationContext activationContext) {
            applicationIdentity = new ApplicationIdentity(fullName);
            activationContext = null;
            if (useFusionActivationContext) {
                if (manifestPaths != null)
                    activationContext = new ActivationContext(applicationIdentity, manifestPaths);
                else
                    activationContext = new ActivationContext(applicationIdentity);
            }
        }

        //
        // Helper method to create an application evidence used in app model activation.
        // There are basically 2 cases where this method is called:
        //   a) It is called in CreateInstanceHelper. In this case, it gathers 
        //      the application evidence passed to the CreateDomainHelper call.
        //   b) It is also called in the server domain. In that case, the domain could
        //      be either the default domain (in which case the input evidence is null)
        //      or a domain created via CreateDomainHelper in which case the application
        //      evidence already contains the application identity and possibly the activation
        //      context.
        //

        internal static Evidence MergeApplicationEvidence (Evidence evidence, ApplicationIdentity applicationIdentity, ActivationContext activationContext, string[] activationData)
        {
            return MergeApplicationEvidence(evidence,
                                            applicationIdentity,
                                            activationContext,
                                            activationData,
                                            null);
        }

        internal static Evidence MergeApplicationEvidence(Evidence evidence,
                                                          ApplicationIdentity applicationIdentity,
                                                          ActivationContext activationContext,
                                                          string[] activationData,
                                                          ApplicationTrust applicationTrust)
        {
            Evidence appEvidence = new Evidence();

            ActivationArguments activationArgs = (activationContext == null ? new ActivationArguments(applicationIdentity, activationData) : new ActivationArguments(activationContext, activationData));
            appEvidence = new Evidence();
            appEvidence.AddHostEvidence(activationArgs);

            if (applicationTrust != null)
                appEvidence.AddHostEvidence(applicationTrust);

            if (activationContext != null)
            {
                Evidence asiEvidence = new ApplicationSecurityInfo(activationContext).ApplicationEvidence;
                if (asiEvidence != null)
                    appEvidence.MergeWithNoDuplicates(asiEvidence);
            }

            if (evidence != null)
                appEvidence.MergeWithNoDuplicates(evidence);

            return appEvidence;
        }
    }
}

