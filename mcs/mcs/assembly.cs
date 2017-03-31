//
// assembly.cs: Assembly declaration and specifications
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004-2011 Novell, Inc.
// Copyright 2011-2013 Xamarin Inc
//


using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using Mono.Security.Cryptography;
using Mono.CompilerServices.SymbolWriter;
using System.Linq;

#if STATIC
using IKVM.Reflection;
using IKVM.Reflection.Emit;
using SecurityType = System.Collections.Generic.List<IKVM.Reflection.Emit.CustomAttributeBuilder>;
#else
using SecurityType = System.Collections.Generic.Dictionary<System.Security.Permissions.SecurityAction, System.Security.PermissionSet>;
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.CSharp
{
	public interface IAssemblyDefinition
	{
		string FullName { get; }
		bool IsCLSCompliant { get; }
		bool IsMissing { get; }
		string Name { get; }

		byte[] GetPublicKeyToken ();
		bool IsFriendAssemblyTo (IAssemblyDefinition assembly);
	}

	public class AssemblyReferenceMessageInfo
	{
		public AssemblyReferenceMessageInfo (AssemblyName dependencyName, Action<Report> reportMessage)
		{
			this.DependencyName = dependencyName;
			this.ReportMessage = reportMessage;
		}

		public AssemblyName DependencyName { get; private set; }
		public Action<Report> ReportMessage { get; private set; }
	}
                
	public abstract class AssemblyDefinition : IAssemblyDefinition
	{
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		protected AssemblyBuilderExtension builder_extra;
		MonoSymbolFile symbol_writer;

		bool is_cls_compliant;
		bool wrap_non_exception_throws;
		bool wrap_non_exception_throws_custom;
		bool has_user_debuggable;

		protected ModuleContainer module;
		readonly string name;
		protected readonly string file_name;

		byte[] public_key, public_key_token;
		bool delay_sign;

		// Holds private/public key pair when private key
		// was available
		StrongNameKeyPair private_key;	

		Attribute cls_attribute;
		Method entry_point;

		protected List<ImportedModuleDefinition> added_modules;
		SecurityType declarative_security;
		Dictionary<ITypeDefinition, Attribute> emitted_forwarders;
		AssemblyAttributesPlaceholder module_target_attrs;

		// Win32 version info values
		string vi_product, vi_product_version, vi_company, vi_copyright, vi_trademark;
		string pa_file_version, pa_assembly_version;

		protected AssemblyDefinition (ModuleContainer module, string name)
		{
			this.module = module;
			this.name = Path.GetFileNameWithoutExtension (name);

			wrap_non_exception_throws = true;

			delay_sign = Compiler.Settings.StrongNameDelaySign;

			//
			// Load strong name key early enough for assembly importer to be able to
			// use the keys for InternalsVisibleTo
			// This should go somewhere close to ReferencesLoading but don't have the place yet
			//
			if (Compiler.Settings.HasKeyFileOrContainer) {
				LoadPublicKey (Compiler.Settings.StrongNameKeyFile, Compiler.Settings.StrongNameKeyContainer);
			}
		}

		protected AssemblyDefinition (ModuleContainer module, string name, string fileName)
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

		//
		// Assembly entry point, aka Main method
		//
		public Method EntryPoint {
			get {
				return entry_point;
			}
			set {
				entry_point = value;
			}
		}

		public string FullName {
			get {
				return Builder.FullName;
			}
		}

		public bool HasCLSCompliantAttribute {
			get {
				return cls_attribute != null;
			}
		}

		// TODO: This should not exist here but will require more changes
		public MetadataImporter Importer {
		    get; set;
		}

		public bool IsCLSCompliant {
			get {
				return is_cls_compliant;
			}
		}

		bool IAssemblyDefinition.IsMissing {
			get {
				return false;
			}
		}

		public bool IsSatelliteAssembly { get; private set; }

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

		protected Report Report {
			get {
				return Compiler.Report;
			}
		}

		public MonoSymbolFile SymbolWriter {
			get {
				return symbol_writer;
			}
		}

		#endregion

		public void AddModule (ImportedModuleDefinition module)
		{
			if (added_modules == null) {
				added_modules = new List<ImportedModuleDefinition> ();
				added_modules.Add (module);
			}
		}

		public void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				a.ExtractSecurityPermissionSet (ctor, ref declarative_security);
				return;
			}

			if (a.Type == pa.AssemblyCulture) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				if (Compiler.Settings.Target == Target.Exe) {
					Report.Error (7059, a.Location, "Executables cannot be satellite assemblies. Remove the attribute or keep it empty");
					return;
				}

				if (value == "neutral")
					value = "";

				if (Compiler.Settings.Target == Target.Module) {
					SetCustomAttribute (ctor, cdata);
				} else {
					builder_extra.SetCulture (value, a.Location);
				}

				IsSatelliteAssembly = true;
				return;
			}

			if (a.Type == pa.AssemblyVersion) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				var vinfo = IsValidAssemblyVersion (value, true);
				if (vinfo == null) {
					Report.Error (7034, a.Location, "The specified version string `{0}' does not conform to the required format - major[.minor[.build[.revision]]]",
						value);
					return;
				}

				if (Compiler.Settings.Target == Target.Module) {
					SetCustomAttribute (ctor, cdata);
				} else {
					builder_extra.SetVersion (vinfo, a.Location);
					pa_assembly_version = vinfo.ToString ();
				}

				return;
			}

			if (a.Type == pa.AssemblyAlgorithmId) {
				const int pos = 2; // skip CA header
				uint alg = (uint) cdata [pos];
				alg |= ((uint) cdata [pos + 1]) << 8;
				alg |= ((uint) cdata [pos + 2]) << 16;
				alg |= ((uint) cdata [pos + 3]) << 24;

				if (Compiler.Settings.Target == Target.Module) {
					SetCustomAttribute (ctor, cdata);
				} else {
					builder_extra.SetAlgorithmId (alg, a.Location);
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

				if (Compiler.Settings.Target == Target.Module) {
					SetCustomAttribute (ctor, cdata);
				} else {
					builder_extra.SetFlags (flags, a.Location);
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
						t.GetSignatureForError ());
					return;
				}

				emitted_forwarders.Add (t.MemberDefinition, a);

				if (t.MemberDefinition.DeclaringAssembly == this) {
					Report.SymbolRelatedToPreviousError (t);
					Report.Error (729, a.Location, "Cannot forward type `{0}' because it is defined in this assembly",
						t.GetSignatureForError ());
					return;
				}

				if (t.IsNested) {
					Report.Error (730, a.Location, "Cannot forward type `{0}' because it is a nested type",
						t.GetSignatureForError ());
					return;
				}

				AddTypeForwarders (t, a.Location);
				return;
			}

			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			if (a.Type == pa.InternalsVisibleTo) {
				string assembly_name = a.GetString ();
				if (assembly_name == null) {
					Report.Error (7030, a.Location, "Friend assembly reference cannot have `null' value");
					return;
				}

				if (assembly_name.Length == 0)
					return;
#if STATIC
				ParsedAssemblyName aname;
				ParseAssemblyResult r = Fusion.ParseAssemblyName (assembly_name, out aname);
				if (r != ParseAssemblyResult.OK) {
					Report.Warning (1700, 3, a.Location, "Friend assembly reference `{0}' is invalid and cannot be resolved",
						assembly_name);
					return;
				}

				if (aname.Version != null || aname.Culture != null || aname.ProcessorArchitecture != ProcessorArchitecture.None) {
					Report.Error (1725, a.Location,
						"Friend assembly reference `{0}' is invalid. InternalsVisibleTo declarations cannot have a version, culture or processor architecture specified",
						assembly_name);

					return;
				}

				if (public_key != null && !aname.HasPublicKey) {
					Report.Error (1726, a.Location,
						"Friend assembly reference `{0}' is invalid. Strong named assemblies must specify a public key in their InternalsVisibleTo declarations",
						assembly_name);
					return;
				}
#endif
			} else if (a.Type == pa.RuntimeCompatibility) {
				wrap_non_exception_throws_custom = true;
			} else if (a.Type == pa.AssemblyFileVersion) {
				pa_file_version = a.GetString ();
				if (string.IsNullOrEmpty (pa_file_version) || IsValidAssemblyVersion (pa_file_version, false) == null) {
					Report.Warning (7035, 1, a.Location, "The specified version string `{0}' does not conform to the recommended format major.minor.build.revision",
					                pa_file_version, a.Name);
					return;
				}

				// File version info decoding from blob is not supported
				var cab = new CustomAttributeBuilder ((ConstructorInfo)ctor.GetMetaInfo (), new object [] { pa_file_version });
				Builder.SetCustomAttribute (cab);
				return;
			} else if (a.Type == pa.AssemblyProduct) {
				vi_product = a.GetString ();
			} else if (a.Type == pa.AssemblyCompany) {
				vi_company = a.GetString ();
			} else if (a.Type == pa.AssemblyCopyright) {
				vi_copyright = a.GetString ();
			} else if (a.Type == pa.AssemblyTrademark) {
				vi_trademark = a.GetString ();
			} else if (a.Type == pa.Debuggable) {
				has_user_debuggable = true;
			} else if (a.Type == pa.AssemblyInformationalVersion) {
				vi_product_version = a.GetString ();
			}

			//
			// Win32 version info attributes AssemblyDescription and AssemblyTitle cannot be
			// set using public API and because we have blob like attributes we need to use
			// special option DecodeVersionInfoAttributeBlobs to support values extraction
			//

			SetCustomAttribute (ctor, cdata);
		}

		void AddTypeForwarders (TypeSpec type, Location loc)
		{
			builder_extra.AddTypeForwarder (type.GetDefinition (), loc);

			var ntypes = MemberCache.GetDeclaredNestedTypes (type);
			if (ntypes == null)
				return;
			
			foreach (var nested in ntypes) {
				if (nested.IsPrivate)
					continue;

				AddTypeForwarders (nested, loc);
			}
		}

		//
		// When using assembly public key attributes InternalsVisibleTo key
		// was not checked, we have to do it later when we actually know what
		// our public key token is
		//
		void CheckReferencesPublicToken ()
		{
			var references = builder_extra.GetReferencedAssemblies ();
			foreach (var an in references) {
				if (public_key != null && an.GetPublicKey ().Length == 0) {
					Report.Error (1577, "Referenced assembly `{0}' does not have a strong name",
						an.FullName);
				}

				var ci = an.CultureInfo;
				if (!ci.Equals (CultureInfo.InvariantCulture)) {
					Report.Warning (8009, 1, "Referenced assembly `{0}' has different culture setting of `{1}'",
						an.Name, ci.Name);
				}

				var ia = Importer.GetImportedAssemblyDefinition (an);
				if (ia == null)
					continue;

				var an_references = GetNotUnifiedReferences (an);
				if (an_references != null) {
					foreach (var r in an_references) {
						//
						// Secondary check when assembly references is resolved but not used. For example
						// due to type-forwarding
						//
						if (references.Any (l => l.Name == r.DependencyName.Name)) {
							r.ReportMessage (Report);
						}
					}
				}

				if (!ia.IsFriendAssemblyTo (this))
					continue;
				
				var attr = ia.GetAssemblyVisibleToName (this);
				var atoken = attr.GetPublicKeyToken ();

				if (ArrayComparer.IsEqual (GetPublicKeyToken (), atoken))
					continue;

				Report.SymbolRelatedToPreviousError (ia.Location);
				Report.Error (281,
					"Friend access was granted to `{0}', but the output assembly is named `{1}'. Try adding a reference to `{0}' or change the output assembly name to match it",
					attr.FullName, FullName);
			}
		}

		protected AssemblyName CreateAssemblyName ()
		{
			var an = new AssemblyName (name);

			if (public_key != null && Compiler.Settings.Target != Target.Module) {
				if (delay_sign) {
					an.SetPublicKey (public_key);
				} else {
					if (public_key.Length == 16) {
						Report.Error (1606, "Could not sign the assembly. ECMA key can only be used to delay-sign assemblies");
					} else if (private_key == null) {
						Error_AssemblySigning ("The specified key file does not have a private key");
					} else {
						an.KeyPair = private_key;
					}
				}
			}

			return an;
		}

		public virtual ModuleBuilder CreateModuleBuilder ()
		{
			if (file_name == null)
				throw new NotSupportedException ("transient module in static assembly");

			var module_name = Path.GetFileName (file_name);

			// Always initialize module without symbolInfo. We could be framework dependent
			// but returned ISymbolWriter does not have all what we need therefore some
			// adaptor will be needed for now we alwayas emit MDB format when generating
			// debug info
			return Builder.DefineDynamicModule (module_name, module_name, false);
		}

		public virtual void Emit ()
		{
			if (Compiler.Settings.Target == Target.Module) {
				module_target_attrs = new AssemblyAttributesPlaceholder (module, name);
				module_target_attrs.CreateContainer ();
				module_target_attrs.DefineContainer ();
				module_target_attrs.Define ();
				module.AddCompilerGeneratedClass (module_target_attrs);
			} else if (added_modules != null) {
				ReadModulesAssemblyAttributes ();
			}

			if (Compiler.Settings.GenerateDebugInfo) {
				symbol_writer = new MonoSymbolFile ();
			}

			module.EmitContainer ();

			if (module.HasExtensionMethod) {
				var pa = module.PredefinedAttributes.Extension;
				if (pa.IsDefined) {
					SetCustomAttribute (pa.Constructor, AttributeEncoder.Empty);
				}
			}

			if (!IsSatelliteAssembly) {
				if (!has_user_debuggable && Compiler.Settings.GenerateDebugInfo) {
					var pa = module.PredefinedAttributes.Debuggable;
					if (pa.IsDefined) {
						var modes = System.Diagnostics.DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints;
						if (!Compiler.Settings.Optimize)
							modes |= System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations;

						pa.EmitAttribute (Builder, modes);
					}
				}

				if (!wrap_non_exception_throws_custom) {
					PredefinedAttribute pa = module.PredefinedAttributes.RuntimeCompatibility;
					if (pa.IsDefined && pa.ResolveBuilder ()) {
						var prop = module.PredefinedMembers.RuntimeCompatibilityWrapNonExceptionThrows.Get ();
						if (prop != null) {
							AttributeEncoder encoder = new AttributeEncoder ();
							encoder.EncodeNamedPropertyArgument (prop, new BoolLiteral (Compiler.BuiltinTypes, true, Location.Null));
							SetCustomAttribute (pa.Constructor, encoder.ToArray ());
						}
					}
				}

				if (declarative_security != null) {
#if STATIC
					foreach (var entry in declarative_security) {
						Builder.__AddDeclarativeSecurity (entry);
					}
#else
					throw new NotSupportedException ("Assembly-level security");
#endif
				}
			}

			CheckReferencesPublicToken ();

			SetEntryPoint ();
		}

		public byte[] GetPublicKeyToken ()
		{
			if (public_key == null || public_key_token != null)
				return public_key_token;

			HashAlgorithm ha = SHA1.Create ();
			byte[] hash = ha.ComputeHash (public_key);
			// we need the last 8 bytes in reverse order
			public_key_token = new byte[8];
			Buffer.BlockCopy (hash, hash.Length - 8, public_key_token, 0, 8);
			Array.Reverse (public_key_token, 0, 8);
			return public_key_token;
		}

		protected virtual List<AssemblyReferenceMessageInfo> GetNotUnifiedReferences (AssemblyName assemblyName)
		{
			return null;
		}

		//
		// Either keyFile or keyContainer has to be non-null
		//
		void LoadPublicKey (string keyFile, string keyContainer)
		{
			if (keyContainer != null) {
				try {
					private_key = new StrongNameKeyPair (keyContainer);
					public_key = private_key.PublicKey;
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
			if (!key_file_exists && Compiler.Settings.StrongNameKeyFile == null) {
				//
				// The key file can be relative to output assembly
				//
				string test_path = Path.Combine (Path.GetDirectoryName (file_name), keyFile);
				key_file_exists = File.Exists (test_path);
				if (key_file_exists)
					keyFile = test_path;
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
					byte[] publicKeyHeader = new byte[8] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00 };

					// Encode public key
					public_key = new byte[12 + publickey.Length];
					Buffer.BlockCopy (publicKeyHeader, 0, public_key, 0, publicKeyHeader.Length);

					// Length of Public Key (in bytes)
					int lastPart = public_key.Length - 12;
					public_key[8] = (byte) (lastPart & 0xFF);
					public_key[9] = (byte) ((lastPart >> 8) & 0xFF);
					public_key[10] = (byte) ((lastPart >> 16) & 0xFF);
					public_key[11] = (byte) ((lastPart >> 24) & 0xFF);

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
					private_key = new StrongNameKeyPair (snkeypair);
				} catch { }
			}
		}

		void ReadModulesAssemblyAttributes ()
		{
			foreach (var m in added_modules) {
				var cattrs = m.ReadAssemblyAttributes ();
				if (cattrs == null)
					continue;

				module.OptAttributes.AddAttributes (cattrs);
			}
		}

		public void Resolve ()
		{
			if (Compiler.Settings.Unsafe && module.PredefinedTypes.SecurityAction.Define ()) {
				//
				// Emits [assembly: SecurityPermissionAttribute (SecurityAction.RequestMinimum, SkipVerification = true)]
				// when -unsafe option was specified
				//
				Location loc = Location.Null;

				MemberAccess system_security_permissions = new MemberAccess (new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Security", loc), "Permissions", loc);

				var req_min = module.PredefinedMembers.SecurityActionRequestMinimum.Resolve (loc);

				Arguments pos = new Arguments (1);
				pos.Add (new Argument (req_min.GetConstant (null)));

				Arguments named = new Arguments (1);
				named.Add (new NamedArgument ("SkipVerification", loc, new BoolLiteral (Compiler.BuiltinTypes, true, loc)));

				Attribute g = new Attribute ("assembly",
					new MemberAccess (system_security_permissions, "SecurityPermissionAttribute"),
					new Arguments[] { pos, named }, loc, false);
				g.AttachTo (module, module);

				// Disable no-location warnings (e.g. obsolete) for compiler generated attribute
				Compiler.Report.DisableReporting ();
				try {
					var ctor = g.Resolve ();
					if (ctor != null) {
						g.ExtractSecurityPermissionSet (ctor, ref declarative_security);
					}
				} finally {
					Compiler.Report.EnableReporting ();
				}
			}

			if (module.OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!module.OptAttributes.CheckTargets())
				return;

			cls_attribute = module.ResolveAssemblyAttribute (module.PredefinedAttributes.CLSCompliant);

			if (cls_attribute != null) {
				is_cls_compliant = cls_attribute.GetClsCompliantAttributeValue ();
			}

			if (added_modules != null && Compiler.Settings.VerifyClsCompliance && is_cls_compliant) {
				foreach (var m in added_modules) {
					if (!m.IsCLSCompliant) {
						Report.Error (3013,
							"Added modules must be marked with the CLSCompliant attribute to match the assembly",
							m.Name);
					}
				}
			}

			Attribute a = module.ResolveAssemblyAttribute (module.PredefinedAttributes.RuntimeCompatibility);
			if (a != null) {
				var val = a.GetNamedValue ("WrapNonExceptionThrows") as BoolConstant;
				if (val != null)
					wrap_non_exception_throws = val.Value;
			}
		}

		protected void ResolveAssemblySecurityAttributes ()
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
						if (Compiler.Settings.StrongNameKeyFile != null) {
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
						if (Compiler.Settings.StrongNameKeyContainer != null) {
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

		public void EmbedResources ()
		{
			//
			// Add Win32 resources
			//
			if (Compiler.Settings.Win32ResourceFile != null) {
				Builder.DefineUnmanagedResource (Compiler.Settings.Win32ResourceFile);
			} else {
				Builder.DefineVersionInfoResource (vi_product, 
				                                   vi_product_version ?? pa_file_version ?? pa_assembly_version,
				                                   vi_company,
				                                   vi_copyright,
				                                   vi_trademark);
			}

			if (Compiler.Settings.Win32IconFile != null) {
				builder_extra.DefineWin32IconResource (Compiler.Settings.Win32IconFile);
			}

			if (Compiler.Settings.Resources != null) {
				if (Compiler.Settings.Target == Target.Module) {
					Report.Error (1507, "Cannot link resource file when building a module");
				} else {
					int counter = 0;
					foreach (var res in Compiler.Settings.Resources) {
						if (!File.Exists (res.FileName)) {
							Report.Error (1566, "Error reading resource file `{0}'", res.FileName);
							continue;
						}

						if (res.IsEmbeded) {
							Stream stream;
							if (counter++ < 10) {
								stream = File.OpenRead (res.FileName);
							} else {
								// TODO: SRE API requires resource stream to be available during AssemblyBuilder::Save
								// we workaround it by reading everything into memory to compile projects with
								// many embedded resource (over 3500) references
								stream = new MemoryStream (File.ReadAllBytes (res.FileName));
							}

							module.Builder.DefineManifestResource (res.Name, stream, res.Attributes);
						} else {
							Builder.AddResourceFile (res.Name, Path.GetFileName (res.FileName), res.Attributes);
						}
					}
				}
			}
		}

		public void Save ()
		{
			PortableExecutableKinds pekind = PortableExecutableKinds.ILOnly;
			ImageFileMachine machine;

			switch (Compiler.Settings.Platform) {
			case Platform.X86:
				pekind |= PortableExecutableKinds.Required32Bit;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind |= PortableExecutableKinds.PE32Plus;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				machine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU32Preferred:
#if STATIC
				pekind |= PortableExecutableKinds.Preferred32Bit;
				machine = ImageFileMachine.I386;
				break;
#else
				throw new NotSupportedException ();
#endif
			case Platform.Arm:
#if STATIC
				machine = ImageFileMachine.ARM;
				break;
#else
				throw new NotSupportedException ();
#endif
			case Platform.AnyCPU:
			default:
				machine = ImageFileMachine.I386;
				break;
			}

			Compiler.TimeReporter.Start (TimeReporter.TimerType.OutputSave);
			try {
				if (Compiler.Settings.Target == Target.Module) {
					SaveModule (pekind, machine);
				} else {
					Builder.Save (module.Builder.ScopeName, pekind, machine);
				}
			} catch (ArgumentOutOfRangeException) {
				Report.Error (16, "Output file `{0}' exceeds the 4GB limit", name);
			} catch (Exception e) {
				Report.Error (16, "Could not write to file `{0}'. {1}", name, e.Message);
			}
			Compiler.TimeReporter.Stop (TimeReporter.TimerType.OutputSave);

			// Save debug symbols file
			if (symbol_writer != null && Compiler.Report.Errors == 0) {
				// TODO: it should run in parallel
				Compiler.TimeReporter.Start (TimeReporter.TimerType.DebugSave);

				var filename = file_name + ".mdb";
				try {
					// We mmap the file, so unlink the previous version since it may be in use
					File.Delete (filename);
				} catch {
					// We can safely ignore
				}

				module.WriteDebugSymbol (symbol_writer);

				using (FileStream fs = new FileStream (filename, FileMode.Create, FileAccess.Write)) {
					symbol_writer.CreateSymbolFile (module.Builder.ModuleVersionId, fs);
				}

				Compiler.TimeReporter.Stop (TimeReporter.TimerType.DebugSave);
			}
		}

		protected virtual void SaveModule (PortableExecutableKinds pekind, ImageFileMachine machine)
		{
			Report.RuntimeMissingSupport (Location.Null, "-target:module");
		}

		void SetCustomAttribute (MethodSpec ctor, byte[] data)
		{
			if (module_target_attrs != null)
				module_target_attrs.AddAssemblyAttribute (ctor, data);
			else
				Builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), data);
		}

		void SetEntryPoint ()
		{
			if (!Compiler.Settings.NeedsEntryPoint) {
				if (Compiler.Settings.MainClass != null)
					Report.Error (2017, "Cannot specify -main if building a module or library");

				return;
			}

			PEFileKinds file_kind;

			switch (Compiler.Settings.Target) {
			case Target.Library:
			case Target.Module:
				file_kind = PEFileKinds.Dll;
				break;
			case Target.WinExe:
				file_kind = PEFileKinds.WindowApplication;
				break;
			default:
				file_kind = PEFileKinds.ConsoleApplication;
				break;
			}

			if (entry_point == null) {
				string main_class = Compiler.Settings.MainClass;
				if (main_class != null) {
					// TODO: Handle dotted names
					var texpr = module.GlobalRootNamespace.LookupType (module, main_class, 0, LookupMode.Probing, Location.Null);
					if (texpr == null) {
						Report.Error (1555, "Could not find `{0}' specified for Main method", main_class);
						return;
					}

					var mtype = texpr.MemberDefinition as ClassOrStruct;
					if (mtype == null) {
						Report.Error (1556, "`{0}' specified for Main method must be a valid class or struct", main_class);
						return;
					}

					Report.Error (1558, mtype.Location, "`{0}' does not have a suitable static Main method", mtype.GetSignatureForError ());
				} else {
					string pname = file_name == null ? name : Path.GetFileName (file_name);
					Report.Error (5001, "Program `{0}' does not contain a static `Main' method suitable for an entry point",
						pname);
				}

				return;
			}

			Builder.SetEntryPoint (entry_point.MethodBuilder, file_kind);
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

		public bool IsFriendAssemblyTo (IAssemblyDefinition assembly)
		{
			return false;
		}

		static Version IsValidAssemblyVersion (string version, bool allowGenerated)
		{
			string[] parts = version.Split ('.');
			if (parts.Length < 1 || parts.Length > 4)
				return null;

			var values = new int[4];
			for (int i = 0; i < parts.Length; ++i) {
				if (!int.TryParse (parts[i], out values[i])) {
					if (parts[i].Length == 1 && parts[i][0] == '*' && allowGenerated) {
						if (i == 2) {
							// Nothing can follow *
							if (parts.Length > 3)
								return null;

							// Generate Build value based on days since 1/1/2000
							TimeSpan days = DateTime.Today - new DateTime (2000, 1, 1);
							values[i] = System.Math.Max (days.Days, 0);
							i = 3;
						}

						if (i == 3) {
							// Generate Revision value based on every other second today
							var seconds = DateTime.Now - DateTime.Today;
							values[i] = (int) seconds.TotalSeconds / 2;
							continue;
						}
					}

					return null;
				}

				if (values[i] > ushort.MaxValue)
					return null;
			}

			return new Version (values[0], values[1], values[2], values[3]);
		}
	}

	public class AssemblyResource : IEquatable<AssemblyResource>
	{
		public AssemblyResource (string fileName, string name)
			: this (fileName, name, false)
		{
		}

		public AssemblyResource (string fileName, string name, bool isPrivate)
		{
			FileName = fileName;
			Name = name;
			Attributes = isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public;
		}

		public ResourceAttributes Attributes { get; private set; }
		public string Name { get; private set; }
		public string FileName { get; private set; }
		public bool IsEmbeded { get; set; }

		#region IEquatable<AssemblyResource> Members

		public bool Equals (AssemblyResource other)
		{
			return Name == other.Name;
		}

		#endregion
	}

	//
	// A placeholder class for assembly attributes when emitting module
	//
	class AssemblyAttributesPlaceholder : CompilerGeneratedContainer
	{
		static readonly string TypeNamePrefix = "<$AssemblyAttributes${0}>";
		public static readonly string AssemblyFieldName = "attributes";

		Field assembly;

		public AssemblyAttributesPlaceholder (ModuleContainer parent, string outputName)
			: base (parent, new MemberName (GetGeneratedName (outputName)), Modifiers.STATIC | Modifiers.INTERNAL)
		{
			assembly = new Field (this, new TypeExpression (parent.Compiler.BuiltinTypes.Object, Location), Modifiers.PUBLIC | Modifiers.STATIC,
				new MemberName (AssemblyFieldName), null);

			AddField (assembly);
		}

		public void AddAssemblyAttribute (MethodSpec ctor, byte[] data)
		{
			assembly.SetCustomAttribute (ctor, data);
		}

		public static string GetGeneratedName (string outputName)
		{
			return string.Format (TypeNamePrefix, outputName);
		}
	}

	//
	// Extension to System.Reflection.Emit.AssemblyBuilder to have fully compatible
	// compiler. This is a default implementation for framework System.Reflection.Emit
	// which does not implement any of the methods
	//
	public class AssemblyBuilderExtension
	{
		protected readonly CompilerContext ctx;

		public AssemblyBuilderExtension (CompilerContext ctx)
		{
			this.ctx = ctx;
		}

		public virtual System.Reflection.Module AddModule (string module)
		{
			ctx.Report.RuntimeMissingSupport (Location.Null, "-addmodule");
			return null;
		}

		public virtual void AddPermissionRequests (PermissionSet[] permissions)
		{
			ctx.Report.RuntimeMissingSupport (Location.Null, "assembly declarative security");
		}

		public virtual void AddTypeForwarder (TypeSpec type, Location loc)
		{
			ctx.Report.RuntimeMissingSupport (loc, "TypeForwardedToAttribute");
		}

		public virtual void DefineWin32IconResource (string fileName)
		{
			ctx.Report.RuntimeMissingSupport (Location.Null, "-win32icon");
		}

		public virtual AssemblyName[] GetReferencedAssemblies ()
		{
			return null;
		}

		public virtual void SetAlgorithmId (uint value, Location loc)
		{
			ctx.Report.RuntimeMissingSupport (loc, "AssemblyAlgorithmIdAttribute");
		}

		public virtual void SetCulture (string culture, Location loc)
		{
			ctx.Report.RuntimeMissingSupport (loc, "AssemblyCultureAttribute");
		}

		public virtual void SetFlags (uint flags, Location loc)
		{
			ctx.Report.RuntimeMissingSupport (loc, "AssemblyFlagsAttribute");
		}

		public virtual void SetVersion (Version version, Location loc)
		{
			ctx.Report.RuntimeMissingSupport (loc, "AssemblyVersionAttribute");
		}
	}

	abstract class AssemblyReferencesLoader<T> where T : class
	{
		protected readonly CompilerContext compiler;

		protected readonly List<string> paths;

		protected AssemblyReferencesLoader (CompilerContext compiler)
		{
			this.compiler = compiler;

			paths = new List<string> ();
			paths.Add (Directory.GetCurrentDirectory ());
			paths.AddRange (compiler.Settings.ReferencesLookupPaths);
		}

		public abstract T HasObjectType (T assembly);
		protected abstract string[] GetDefaultReferences ();
		public abstract T LoadAssemblyFile (string fileName, bool isImplicitReference);
		public abstract void LoadReferences (ModuleContainer module);

		protected void Error_FileNotFound (string fileName)
		{
			compiler.Report.Error (6, "Metadata file `{0}' could not be found", fileName);
		}

		protected void Error_FileCorrupted (string fileName)
		{
			compiler.Report.Error (9, "Metadata file `{0}' does not contain valid metadata", fileName);
		}

		protected void Error_AssemblyIsModule (string fileName)
		{
			compiler.Report.Error (1509,
				"Referenced assembly file `{0}' is a module. Consider using `-addmodule' option to add the module",
				fileName);
		}

		protected void Error_ModuleIsAssembly (string fileName)
		{
			compiler.Report.Error (1542,
				"Added module file `{0}' is an assembly. Consider using `-r' option to reference the file",
				fileName);
		}

		protected void LoadReferencesCore (ModuleContainer module, out T corlib_assembly, out List<Tuple<RootNamespace, T>> loaded)
		{
			compiler.TimeReporter.Start (TimeReporter.TimerType.ReferencesLoading);

			loaded = new List<Tuple<RootNamespace, T>> ();

			//
			// Load mscorlib.dll as the first
			//
			if (module.Compiler.Settings.StdLib) {
				corlib_assembly = LoadAssemblyFile ("mscorlib.dll", true);
			} else {
				corlib_assembly = default (T);
			}

			T a;
			foreach (string r in module.Compiler.Settings.AssemblyReferences) {
				a = LoadAssemblyFile (r, false);
				if (a == null || EqualityComparer<T>.Default.Equals (a, corlib_assembly))
					continue;

				var key = Tuple.Create (module.GlobalRootNamespace, a);
				if (loaded.Contains (key))
					continue;

				loaded.Add (key);
			}

			if (corlib_assembly == null) {
				//
				// Requires second pass because HasObjectType can trigger assembly load event
				//
				for (int i = 0; i < loaded.Count; ++i) {
					var assembly = loaded [i];

					//
					// corlib assembly is the first referenced assembly which contains System.Object
					//
					corlib_assembly = HasObjectType (assembly.Item2);
					if (corlib_assembly != null) {
						if (corlib_assembly != assembly.Item2) {
							var ca = corlib_assembly;
							i = loaded.FindIndex (l => l.Item2 == ca);
						}

						if (i >= 0)
							loaded.RemoveAt (i);

						break;
					}
				}
			}

			foreach (var entry in module.Compiler.Settings.AssemblyReferencesAliases) {
				a = LoadAssemblyFile (entry.Item2, false);
				if (a == null)
					continue;

				var key = Tuple.Create (module.CreateRootNamespace (entry.Item1), a);
				if (loaded.Contains (key))
					continue;

				loaded.Add (key);
			}

			if (compiler.Settings.LoadDefaultReferences) {
				foreach (string r in GetDefaultReferences ()) {
					a = LoadAssemblyFile (r, true);
					if (a == null)
						continue;

					var key = Tuple.Create (module.GlobalRootNamespace, a);
					if (loaded.Contains (key))
						continue;

					loaded.Add (key);
				}
			}

			compiler.TimeReporter.Stop (TimeReporter.TimerType.ReferencesLoading);
		}
	}
}
