//
// System.Reflection/MonoAssembly.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Security.Policy;
using System.Security.Permissions;

namespace System.Reflection {

	abstract class RuntimeAssembly : Assembly
	{
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UnitySerializationHolder.GetUnitySerializationInfo (info,
                                                               UnitySerializationHolder.AssemblyUnity,
                                                               this.FullName,
                                                               this);
		}

		internal static RuntimeAssembly GetExecutingAssembly (ref StackCrawlMark stackMark)
		{
			// Mono runtime does not support StackCrawlMark, The easiest workaround is to replace use
			// of StackCrawlMark.LookForMyCaller with GetCallingAssembly
			throw new NotSupportedException ();
		}

        // Creates AssemblyName. Fills assembly if AssemblyResolve event has been raised.
        [System.Security.SecurityCritical]  // auto-generated
        internal static AssemblyName CreateAssemblyName(
            String assemblyString, 
            bool forIntrospection, 
            out RuntimeAssembly assemblyFromResolveEvent)
        {
            if (assemblyString == null)
                throw new ArgumentNullException("assemblyString");
            Contract.EndContractBlock();

            if ((assemblyString.Length == 0) ||
                (assemblyString[0] == '\0'))
                throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));

            if (forIntrospection)
                AppDomain.CheckReflectionOnlyLoadSupported();

            AssemblyName an = new AssemblyName();

            an.Name = assemblyString;
            assemblyFromResolveEvent = null; // instead of an.nInit(out assemblyFromResolveEvent, forIntrospection, true);
            return an;
        }

        internal static RuntimeAssembly InternalLoadAssemblyName(
            AssemblyName assemblyRef, 
            Evidence assemblySecurity,
            RuntimeAssembly reqAssembly,
            ref StackCrawlMark stackMark,
#if FEATURE_HOSTED_BINDER
            IntPtr pPrivHostBinder,
#endif
            bool throwOnFileNotFound, 
            bool forIntrospection,
            bool suppressSecurityChecks)
        {
            if (assemblyRef == null)
                throw new ArgumentNullException("assemblyRef");
            Contract.EndContractBlock();

            if (assemblyRef.CodeBase != null)
            {
                AppDomain.CheckLoadFromSupported();
            }

            assemblyRef = (AssemblyName)assemblyRef.Clone();
#if FEATURE_VERSIONING
            if (!forIntrospection &&
                (assemblyRef.ProcessorArchitecture != ProcessorArchitecture.None)) {
                // PA does not have a semantics for by-name binds for execution
                assemblyRef.ProcessorArchitecture = ProcessorArchitecture.None;
            }
#endif

            if (assemblySecurity != null)
            {
#if FEATURE_CAS_POLICY
                if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
#endif // FEATURE_CAS_POLICY

                if (!suppressSecurityChecks)
                {
#pragma warning disable 618
                    new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
#pragma warning restore 618
                }
            }

			return (RuntimeAssembly) Assembly.Load (assemblyRef);
		}

		internal static RuntimeAssembly LoadWithPartialNameInternal (String partialName, Evidence securityEvidence, ref StackCrawlMark stackMark)
		{
			AssemblyName an = new AssemblyName(partialName);
			return LoadWithPartialNameInternal (an, securityEvidence, ref stackMark);
		}

		internal static RuntimeAssembly LoadWithPartialNameInternal (AssemblyName an, Evidence securityEvidence, ref StackCrawlMark stackMark)
		{
			throw new NotImplementedException ("LoadWithPartialNameInternal");
		}
	}

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	class MonoAssembly : RuntimeAssembly
	{
		public
		override
		Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			Type res;
			if (name == null)
				throw new ArgumentNullException (name);
			if (name.Length == 0)
			throw new ArgumentException ("name", "Name cannot be empty");

			res = InternalGetType (null, name, throwOnError, ignoreCase);
			return res;
		}

		public
		override
		Module GetModule (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Name can't be empty");

			Module[] modules = GetModules (true);
			foreach (Module module in modules) {
				if (module.ScopeName == name)
					return module;
			}

			return null;
		}

		public
		override
		AssemblyName[] GetReferencedAssemblies () {
			return GetReferencedAssemblies (this);
		}

		public
		override
		Module[] GetModules (bool getResourceModules) {
			Module[] modules = GetModulesInternal ();

			if (!getResourceModules) {
				var result = new List<Module> (modules.Length);
				foreach (Module m in modules)
					if (!m.IsResource ())
						result.Add (m);
				return result.ToArray ();
			}
			else
				return modules;
		}

		[MonoTODO ("Always returns the same as GetModules")]
		public
		override
		Module[] GetLoadedModules (bool getResourceModules)
		{
			return GetModules (getResourceModules);
		}

		public
		override
		Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			return GetSatelliteAssembly (culture, null, true);
		}

		public
		override
		Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			return GetSatelliteAssembly (culture, version, true);
		}

		//FIXME remove GetManifestModule under v4, it's a v2 artifact
		[ComVisible (false)]
		public
		override
		Module ManifestModule {
			get {
				return GetManifestModule ();
			}
		}

		public
		override
		bool GlobalAssemblyCache {
			get {
				return get_global_assembly_cache ();
			}
		}
	}
}


