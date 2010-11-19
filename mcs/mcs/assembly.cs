//
// assembly.cs: Assembly declaration and specifications
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004 Novell, Inc.
//


using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace Mono.CSharp
{
	public interface IAssemblyDefinition
	{
		string FullName { get; }
		bool HasExtensionMethod { get; }
		bool IsCLSCompliant { get; }
		string Name { get; }

		byte[] GetPublicKeyToken ();
	}
                
	public class AssemblyDefinition : IAssemblyDefinition
	{
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		bool is_cls_compliant;
		bool wrap_non_exception_throws;
		bool wrap_non_exception_throws_custom;

		ModuleContainer module;
		string name;
		string file_name;
		byte[] public_key, public_key_token;
		bool private_key_exists;
		bool delay_sign;

		Attribute cls_attribute;

		Dictionary<SecurityAction, PermissionSet> declarative_security;
		MethodInfo add_type_forwarder;
		Dictionary<ITypeDefinition, Attribute> emitted_forwarders;

		//
		// In-memory only assembly container
		//
		public AssemblyDefinition (ModuleContainer module, string name)
		{
			this.module = module;
			this.name = Path.GetFileNameWithoutExtension (name);

			wrap_non_exception_throws = true;

			delay_sign = RootContext.StrongNameDelaySign;

			//
			// Load strong name key early enough for assembly importer to be able to
			// use the keys for InternalsVisibleTo
			// This should go somewhere close to ReferencesLoading but don't have the place yet
			//
			if (RootContext.StrongNameKeyFile != null || RootContext.StrongNameKeyContainer != null) {
				LoadPublicKey (RootContext.StrongNameKeyFile, RootContext.StrongNameKeyContainer);
			}
		}

		//
		// Assembly container with file output
		//
		public AssemblyDefinition (ModuleContainer module, string name, string fileName)
			: this (module, name)
		{
			this.file_name = fileName;
		}

		#region Properties

		public Attribute CLSCompliantAttribute {
			get {
				return cls_attribute;
			}
		}

		public CompilerContext Compiler {
			get {
				return module.Compiler;
			}
		}

		public string FullName {
			get {
				return Builder.FullName;
			}
		}

		public bool HasExtensionMethod {
			get {
				return module.HasExtensionMethod;
			}
		}

		public bool HasCLSCompliantAttribute {
			get {
				return cls_attribute != null;
			}
		}

		public bool IsCLSCompliant {
			get {
				return is_cls_compliant;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public bool WrapNonExceptionThrows {
			get {
				return wrap_non_exception_throws;
			}
		}

		Report Report {
			get {
				return Compiler.Report;
			}
		}

		#endregion

		public void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == pa.AssemblyCulture) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				if (RootContext.Target == Target.Exe) {
					a.Error_AttributeEmitError ("The executables cannot be satelite assemblies, remove the attribute or keep it empty");
					return;
				}

				try {
					var fi = typeof (AssemblyBuilder).GetField ("culture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (Builder, value == "neutral" ? "" : value);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyCultureAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyVersion) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				var vinfo = IsValidAssemblyVersion (value.Replace ('*', '0'));
				if (vinfo == null) {
					a.Error_AttributeEmitError (string.Format ("Specified version `{0}' is not valid", value));
					return;
				}

				try {
					var fi = typeof (AssemblyBuilder).GetField ("version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (Builder, vinfo);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyVersionAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyAlgorithmId) {
				const int pos = 2; // skip CA header
				uint alg = (uint) cdata [pos];
				alg |= ((uint) cdata [pos + 1]) << 8;
				alg |= ((uint) cdata [pos + 2]) << 16;
				alg |= ((uint) cdata [pos + 3]) << 24;

				try {
					var fi = typeof (AssemblyBuilder).GetField ("algid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (Builder, alg);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyAlgorithmIdAttribute setting");
				}

				return;
			}

			if (a.Type == pa.AssemblyFlags) {
				const int pos = 2; // skip CA header
				uint flags = (uint) cdata[pos];
				flags |= ((uint) cdata [pos + 1]) << 8;
				flags |= ((uint) cdata [pos + 2]) << 16;
				flags |= ((uint) cdata [pos + 3]) << 24;

				// Ignore set PublicKey flag if assembly is not strongnamed
				if ((flags & (uint) AssemblyNameFlags.PublicKey) != 0 && public_key == null)
					flags &= ~(uint) AssemblyNameFlags.PublicKey;

				try {
					var fi = typeof (AssemblyBuilder).GetField ("flags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (Builder, flags);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyFlagsAttribute setting");
				}

				return;
			}

			if (a.Type == pa.TypeForwarder) {
				TypeSpec t = a.GetArgumentType ();
				if (t == null || TypeManager.HasElementType (t)) {
					Report.Error (735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}

				if (emitted_forwarders == null) {
					emitted_forwarders = new Dictionary<ITypeDefinition, Attribute> ();
				} else if (emitted_forwarders.ContainsKey (t.MemberDefinition)) {
					Report.SymbolRelatedToPreviousError (emitted_forwarders[t.MemberDefinition].Location, null);
					Report.Error (739, a.Location, "A duplicate type forward of type `{0}'",
						TypeManager.CSharpName (t));
					return;
				}

				emitted_forwarders.Add (t.MemberDefinition, a);

				if (t.MemberDefinition.DeclaringAssembly == this) {
					Report.SymbolRelatedToPreviousError (t);
					Report.Error (729, a.Location, "Cannot forward type `{0}' because it is defined in this assembly",
						TypeManager.CSharpName (t));
					return;
				}

				if (t.IsNested) {
					Report.Error (730, a.Location, "Cannot forward type `{0}' because it is a nested type",
						TypeManager.CSharpName (t));
					return;
				}

				if (add_type_forwarder == null) {
					add_type_forwarder = typeof (AssemblyBuilder).GetMethod ("AddTypeForwarder",
						BindingFlags.NonPublic | BindingFlags.Instance);

					if (add_type_forwarder == null) {
						Report.RuntimeMissingSupport (a.Location, "TypeForwardedTo attribute");
						return;
					}
				}

				add_type_forwarder.Invoke (Builder, new object[] { t.GetMetaInfo () });
				return;
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			if (a.Type == pa.InternalsVisibleTo) {
				string assembly_name = a.GetString ();
				if (assembly_name.Length == 0)
					return;

				AssemblyName aname = null;
				try {
					aname = new AssemblyName (assembly_name);
				} catch (Exception) {
					Report.Warning (1700, 3, a.Location, "Assembly reference `{0}' is invalid and cannot be resolved",
						assembly_name);
					return;
				}

				if (aname.Version != null || aname.CultureInfo != null || aname.ProcessorArchitecture != ProcessorArchitecture.None) {
					Report.Error (1725, a.Location,
						"Friend assembly reference `{0}' is invalid. InternalsVisibleTo declarations cannot have a version, culture or processor architecture specified",
						assembly_name);

					return;
				}

				// TODO: GetPublicKey () does not work on .NET when AssemblyName is constructed from a string
				if (public_key != null && aname.GetPublicKey () == null) {
					Report.Error (1726, a.Location,
						"Friend assembly reference `{0}' is invalid. Strong named assemblies must specify a public key in their InternalsVisibleTo declarations",
						assembly_name);
					return;
				}
			} else if (a.Type == pa.RuntimeCompatibility) {
				wrap_non_exception_throws_custom = true;
			}

			Builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		//
		// When using assembly public key attributes InternalsVisibleTo key
		// was not checked, we have to do it later when we actually know what
		// our public key token is
		//
		void CheckReferencesPublicToken ()
		{
			// TODO: It should check only references assemblies but there is
			// no working SRE API
			foreach (var a in Compiler.GlobalRootNamespace.Assemblies) {
				if (public_key != null && !a.HasStrongName) {
					Report.Error (1577, "Referenced assembly `{0}' does not have a strong name",
						a.FullName);
				}

				if (!a.IsFriendAssemblyTo (this))
					continue;

				var attr = a.GetAssemblyVisibleToName (this);
				var atoken = attr.GetPublicKeyToken ();

				if (ArrayComparer.IsEqual (GetPublicKeyToken (), atoken))
					continue;

				Report.Error (281,
					"Friend access was granted to `{0}', but the output assembly is named `{1}'. Try adding a reference to `{0}' or change the output assembly name to match it",
					attr.FullName, FullName);
			}
		}

		//
		// Initializes the code generator
		//
		public bool Create (AppDomain domain, AssemblyBuilderAccess access)
		{
			ResolveAssemblySecurityAttributes ();

			var an = new AssemblyName (name);

			if (public_key != null) {
				if (!delay_sign && !private_key_exists) {
					if (public_key.Length == 16) {
						Report.Error (1606, "Could not sign the assembly. ECMA key can only be used to delay-sign assemblies");
					} else {
						Error_AssemblySigning ("The specified key file does not have a private key");
					}
				}

				an.SetPublicKey (public_key);
			}

			try {
				Builder = file_name == null ?
					domain.DefineDynamicAssembly (an, access) :
					domain.DefineDynamicAssembly (an, access, Dirname (file_name));
			} catch (ArgumentException) {
				// specified key may not be exportable outside it's container
				if (RootContext.StrongNameKeyContainer != null) {
					Report.Error (1548, "Could not access the key inside the container `" +
						RootContext.StrongNameKeyContainer + "'.");
				}
				throw;
			}

			return true;
		}

		public ModuleBuilder CreateModuleBuilder ()
		{
			// Creates transient module
			if (file_name == null)
				return Builder.DefineDynamicModule (name, false);

			ModuleBuilder mbuilder = null;

			try {
				var module_name = Path.GetFileName (file_name);
				mbuilder = Builder.DefineDynamicModule (module_name, module_name, RootContext.GenerateDebugInfo);

#if !MS_COMPATIBLE
				// TODO: We should use SymbolWriter from DefineDynamicModule
				if (RootContext.GenerateDebugInfo && !SymbolWriter.Initialize (mbuilder, file_name)) {
					Report.Error (40, "Unexpected debug information initialization error `{0}'",
						"Could not find the symbol writer assembly (Mono.CompilerServices.SymbolWriter.dll)");
				}
#endif
			} catch (ExecutionEngineException e) {
				Report.Error (40, "Unexpected debug information initialization error `{0}'",
					e.Message);
			}

			return mbuilder;
		}

		static string Dirname (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (0, pos);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (0, pos);

			return ".";
		}

		public void Emit ()
		{
			module.Emit ();

			if (module.HasExtensionMethod)
				Compiler.PredefinedAttributes.Extension.EmitAttribute (Builder);

			PredefinedAttribute pa = Compiler.PredefinedAttributes.RuntimeCompatibility;
			if (pa.IsDefined && !wrap_non_exception_throws_custom) {
				var ci = TypeManager.GetPredefinedConstructor (pa.Type, Location.Null, TypeSpec.EmptyTypes);
				PropertyInfo [] pis = new PropertyInfo [1];

				pis [0] = TypeManager.GetPredefinedProperty (pa.Type,
					"WrapNonExceptionThrows", Location.Null, TypeManager.bool_type).MetaInfo;
				object [] pargs = new object [1];
				pargs [0] = true;
				Builder.SetCustomAttribute (new CustomAttributeBuilder ((ConstructorInfo) ci.GetMetaInfo (), new object[0], pis, pargs));
			}

			if (declarative_security != null) {

				MethodInfo add_permission = typeof (AssemblyBuilder).GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);
				object builder_instance = Builder;

				try {
					// Microsoft runtime hacking
					if (add_permission == null) {
						var assembly_builder = typeof (AssemblyBuilder).Assembly.GetType ("System.Reflection.Emit.AssemblyBuilderData");
						add_permission = assembly_builder.GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);

						FieldInfo fi = typeof (AssemblyBuilder).GetField ("m_assemblyData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
						builder_instance = fi.GetValue (Builder);
					}

					var args = new PermissionSet [3];
					declarative_security.TryGetValue (SecurityAction.RequestMinimum, out args [0]);
					declarative_security.TryGetValue (SecurityAction.RequestOptional, out args [1]);
					declarative_security.TryGetValue (SecurityAction.RequestRefuse, out args [2]);
					add_permission.Invoke (builder_instance, args);
				} catch {
					Report.RuntimeMissingSupport (Location.Null, "assembly permission setting");
				}
			}

			CheckReferencesPublicToken ();
		}

		public byte[] GetPublicKeyToken ()
		{
			if (public_key == null || public_key_token != null)
				return public_key_token;

			HashAlgorithm ha = SHA1.Create ();
			byte[] hash = ha.ComputeHash (public_key);
			// we need the last 8 bytes in reverse order
			public_key_token = new byte[8];
			Array.Copy (hash, (hash.Length - 8), public_key_token, 0, 8);
			Array.Reverse (public_key_token, 0, 8);
			return public_key_token;
		}

		//
		// Either keyFile or keyContainer has to be non-null
		//
		void LoadPublicKey (string keyFile, string keyContainer)
		{
			if (keyContainer != null) {
				try {
					public_key = new StrongNameKeyPair (keyContainer).PublicKey;
					private_key_exists = true;
				} catch {
					Error_AssemblySigning ("The specified key container `" + keyContainer + "' does not exist");
				}

				return;
			}

			bool key_file_exists = File.Exists (keyFile);

			//
			// For attribute based KeyFile do additional lookup
			// in output assembly path
			//
			if (!key_file_exists && RootContext.StrongNameKeyFile == null) {
				//
				// The key file can be relative to output assembly
				//
				string test_path = Path.Combine (Path.GetDirectoryName (file_name), keyFile);
				key_file_exists = File.Exists (test_path);
			}

			if (!key_file_exists) {
				Error_AssemblySigning ("The specified key file `" + keyFile + "' does not exist");
				return;
			}

			using (FileStream fs = new FileStream (keyFile, FileMode.Open, FileAccess.Read)) {
				byte[] snkeypair = new byte[fs.Length];
				fs.Read (snkeypair, 0, snkeypair.Length);

				// check for ECMA key
				if (snkeypair.Length == 16) {
					public_key = snkeypair;
					return;
				}

				try {
					// take it, with or without, a private key
					RSA rsa = CryptoConvert.FromCapiKeyBlob (snkeypair);
					// and make sure we only feed the public part to Sys.Ref
					byte[] publickey = CryptoConvert.ToCapiPublicKeyBlob (rsa);

					// AssemblyName.SetPublicKey requires an additional header
					byte[] publicKeyHeader = new byte[12] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00 };

					// Encode public key
					public_key = new byte[12 + publickey.Length];
					Buffer.BlockCopy (publicKeyHeader, 0, public_key, 0, 12);
					Buffer.BlockCopy (publickey, 0, public_key, 12, publickey.Length);
				} catch {
					Error_AssemblySigning ("The specified key file `" + keyFile + "' has incorrect format");
					return;
				}

				if (delay_sign)
					return;

				try {
					// TODO: Is there better way to test for a private key presence ?
					CryptoConvert.FromCapiPrivateKeyBlob (snkeypair);
					private_key_exists = true;
				} catch { }
			}
		}

		public void Resolve ()
		{
			if (RootContext.Unsafe) {
				//
				// Emits [assembly: SecurityPermissionAttribute (SecurityAction.RequestMinimum, SkipVerification = true)]
				// when -unsafe option was specified
				//
				
				Location loc = Location.Null;

				MemberAccess system_security_permissions = new MemberAccess (new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Security", loc), "Permissions", loc);

				Arguments pos = new Arguments (1);
				pos.Add (new Argument (new MemberAccess (new MemberAccess (system_security_permissions, "SecurityAction", loc), "RequestMinimum")));

				Arguments named = new Arguments (1);
				named.Add (new NamedArgument ("SkipVerification", loc, new BoolLiteral (true, loc)));

				GlobalAttribute g = new GlobalAttribute (new NamespaceEntry (Compiler, null, null, null), "assembly",
					new MemberAccess (system_security_permissions, "SecurityPermissionAttribute"),
					new Arguments[] { pos, named }, loc, false);
				g.AttachTo (module, module);

				if (g.Resolve () != null) {
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
					g.ExtractSecurityPermissionSet (declarative_security);
				}
			}

			if (module.OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!module.OptAttributes.CheckTargets())
				return;

			cls_attribute = module.ResolveAssemblyAttribute (Compiler.PredefinedAttributes.CLSCompliant);

			if (cls_attribute != null) {
				is_cls_compliant = cls_attribute.GetClsCompliantAttributeValue ();
			}

			Attribute a = module.ResolveAssemblyAttribute (Compiler.PredefinedAttributes.RuntimeCompatibility);
			if (a != null) {
				var val = a.GetPropertyValue ("WrapNonExceptionThrows") as BoolConstant;
				if (val != null)
					wrap_non_exception_throws = val.Value;
			}
		}

		void ResolveAssemblySecurityAttributes ()
		{
			string key_file = null;
			string key_container = null;

			if (module.OptAttributes != null) {
				foreach (Attribute a in module.OptAttributes.Attrs) {
					// cannot rely on any resolve-based members before you call Resolve
					if (a.ExplicitTarget != "assembly")
						continue;

					// TODO: This code is buggy: comparing Attribute name without resolving is wrong.
					//       However, this is invoked by CodeGen.Init, when none of the namespaces
					//       are loaded yet.
					// TODO: Does not handle quoted attributes properly
					switch (a.Name) {
					case "AssemblyKeyFile":
					case "AssemblyKeyFileAttribute":
					case "System.Reflection.AssemblyKeyFileAttribute":
						if (RootContext.StrongNameKeyFile != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keyfile", "System.Reflection.AssemblyKeyFileAttribute");
						} else {
							string value = a.GetString ();
							if (!string.IsNullOrEmpty (value)) {
								Error_ObsoleteSecurityAttribute (a, "keyfile");
								key_file = value;
							}
						}
						break;
					case "AssemblyKeyName":
					case "AssemblyKeyNameAttribute":
					case "System.Reflection.AssemblyKeyNameAttribute":
						if (RootContext.StrongNameKeyContainer != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keycontainer", "System.Reflection.AssemblyKeyNameAttribute");
						} else {
							string value = a.GetString ();
							if (!string.IsNullOrEmpty (value)) {
								Error_ObsoleteSecurityAttribute (a, "keycontainer");
								key_container = value;
							}
						}
						break;
					case "AssemblyDelaySign":
					case "AssemblyDelaySignAttribute":
					case "System.Reflection.AssemblyDelaySignAttribute":
						bool b = a.GetBoolean ();
						if (b) {
							Error_ObsoleteSecurityAttribute (a, "delaysign");
						}

						delay_sign = b;
						break;
					}
				}
			}

			// We came here only to report assembly attributes warnings
			if (public_key != null)
				return;

			//
			// Load the strong key file found in attributes when no
			// command line key was given
			//
			if (key_file != null || key_container != null) {
				LoadPublicKey (key_file, key_container);
			} else if (delay_sign) {
				Report.Warning (1607, 1, "Delay signing was requested but no key file was given");
			}
		}

		public void Save ()
		{
			PortableExecutableKinds pekind;
			ImageFileMachine machine;

			switch (RootContext.Platform) {
			case Platform.X86:
				pekind = PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU:
			default:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.I386;
				break;
			}
			try {
				Builder.Save (module.Builder.ScopeName, pekind, machine);
			} catch (System.Runtime.InteropServices.COMException) {
				if ((RootContext.StrongNameKeyFile == null) || (!RootContext.StrongNameDelaySign))
					throw;

				// FIXME: it seems Microsoft AssemblyBuilder doesn't like to delay sign assemblies 
				Report.Error (1548, "Couldn't delay-sign the assembly with the '" +
					RootContext.StrongNameKeyFile +
					"', Use MCS with the Mono runtime or CSC to compile this assembly.");
			} catch (System.IO.IOException io) {
				Report.Error (16, "Could not write to file `" + name + "', cause: " + io.Message);
				return;
			} catch (System.UnauthorizedAccessException ua) {
				Report.Error (16, "Could not write to file `" + name + "', cause: " + ua.Message);
				return;
			} catch (System.NotImplementedException nie) {
				Report.RuntimeMissingSupport (Location.Null, nie.Message);
				return;
			}
		}

		void Error_ObsoleteSecurityAttribute (Attribute a, string option)
		{
			Report.Warning (1699, 1, a.Location,
				"Use compiler option `{0}' or appropriate project settings instead of `{1}' attribute",
				option, a.Name);
		}

		void Error_AssemblySigning (string text)
		{
			Report.Error (1548, "Error during assembly signing. " + text);
		}

		static string IsValidAssemblyVersion (string version)
		{
			Version v;
			try {
				v = new Version (version);
			} catch {
				try {
					int major = int.Parse (version, CultureInfo.InvariantCulture);
					v = new Version (major, 0);
				} catch {
					return null;
				}
			}

			foreach (int candidate in new int [] { v.Major, v.Minor, v.Build, v.Revision }) {
				if (candidate > ushort.MaxValue)
					return null;
			}

			return new Version (v.Major, System.Math.Max (0, v.Minor), System.Math.Max (0, v.Build), System.Math.Max (0, v.Revision)).ToString (4);
		}


		// Wrapper for AssemblyBuilder.AddModule
		static MethodInfo adder_method;
		static public MethodInfo AddModule_Method {
			get {
				if (adder_method == null)
					adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				return adder_method;
			}
		}
		public Module AddModule (string module)
		{
			MethodInfo m = AddModule_Method;
			if (m == null) {
				Report.RuntimeMissingSupport (Location.Null, "/addmodule");
				Environment.Exit (1);
			}

			try {
				return (Module) m.Invoke (Builder, new object [] { module });
			} catch (TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}		
	}
}
