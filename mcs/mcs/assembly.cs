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

namespace Mono.CSharp {

	public abstract class CommonAssemblyModulClass : Attributable, IMemberContext
	{
		public void AddAttributes (List<Attribute> attrs, IMemberContext context)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this, context);

			if (attributes == null) {
				attributes = new Attributes (attrs);
				return;
			}
			attributes.AddAttributes (attrs);
		}

		public virtual void Emit (TypeContainer tc) 
		{
			if (OptAttributes == null)
				return;

			OptAttributes.Emit ();
		}

		protected Attribute ResolveAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search (a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		#region IMemberContext Members

		public CompilerContext Compiler {
			get { return RootContext.ToplevelTypes.Compiler; }
		}

		public TypeSpec CurrentType {
			get { return null; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return null; }
		}

		public MemberCore CurrentMemberDefinition {
			get { return RootContext.ToplevelTypes; }
		}

		public string GetSignatureForError ()
		{
			return "<module>";
		}

		public bool HasUnresolvedConstraints {
			get { return false; }
		}

		public bool IsObsolete {
			get { return false; }
		}

		public bool IsUnsafe {
			get { return false; }
		}

		public bool IsStatic {
			get { return false; }
		}

		public IList<MethodSpec> LookupExtensionMethod (TypeSpec extensionType, string name, int arity, ref NamespaceEntry scope)
		{
			throw new NotImplementedException ();
		}

		public FullNamedExpression LookupNamespaceOrType (string name, int arity, Location loc, bool ignore_cs0104)
		{
			return RootContext.ToplevelTypes.LookupNamespaceOrType (name, arity, loc, ignore_cs0104);
		}

		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			return null;
		}

		#endregion
	}
                
	public class AssemblyClass : CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		bool is_cls_compliant;
		bool wrap_non_exception_throws;

		public Attribute ClsCompliantAttribute;

		Dictionary<SecurityAction, PermissionSet> declarative_security;
		bool has_extension_method;		
		public AssemblyName Name;
		MethodInfo add_type_forwarder;
		Dictionary<ITypeDefinition, Attribute> emitted_forwarders;

		// Module is here just because of error messages
		static string[] attribute_targets = new string [] { "assembly", "module" };

		public AssemblyClass ()
		{
			wrap_non_exception_throws = true;
		}

		public bool HasExtensionMethods {
			set {
				has_extension_method = value;
			}
		}

		public bool IsClsCompliant {
			get {
				return is_cls_compliant;
			}
		}

		public bool WrapNonExceptionThrows {
			get {
				return wrap_non_exception_throws;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Assembly;
			}
		}

		public override bool IsClsComplianceRequired ()
		{
			return is_cls_compliant;
		}

		Report Report {
			get { return Compiler.Report; }
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
				g.AttachTo (this, this);

				if (g.Resolve () != null) {
					declarative_security = new Dictionary<SecurityAction, PermissionSet> ();
					g.ExtractSecurityPermissionSet (declarative_security);
				}
			}

			if (OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!OptAttributes.CheckTargets())
				return;

			ClsCompliantAttribute = ResolveAttribute (Compiler.PredefinedAttributes.CLSCompliant);

			if (ClsCompliantAttribute != null) {
				is_cls_compliant = ClsCompliantAttribute.GetClsCompliantAttributeValue ();
			}

			Attribute a = ResolveAttribute (Compiler.PredefinedAttributes.RuntimeCompatibility);
			if (a != null) {
				var val = a.GetPropertyValue ("WrapNonExceptionThrows") as BoolConstant;
				if (val != null)
					wrap_non_exception_throws = val.Value;
			}
		}

		// fix bug #56621
		private void SetPublicKey (AssemblyName an, byte[] strongNameBlob) 
		{
			try {
				// check for possible ECMA key
				if (strongNameBlob.Length == 16) {
					// will be rejected if not "the" ECMA key
					an.SetPublicKey (strongNameBlob);
				}
				else {
					// take it, with or without, a private key
					RSA rsa = CryptoConvert.FromCapiKeyBlob (strongNameBlob);
					// and make sure we only feed the public part to Sys.Ref
					byte[] publickey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
					
					// AssemblyName.SetPublicKey requires an additional header
					byte[] publicKeyHeader = new byte [12] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00 };

					byte[] encodedPublicKey = new byte [12 + publickey.Length];
					Buffer.BlockCopy (publicKeyHeader, 0, encodedPublicKey, 0, 12);
					Buffer.BlockCopy (publickey, 0, encodedPublicKey, 12, publickey.Length);
					an.SetPublicKey (encodedPublicKey);
				}
			}
			catch (Exception) {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' is incorrectly encoded");
				Environment.Exit (1);
			}
		}

		void Error_ObsoleteSecurityAttribute (Attribute a, string option)
		{
			Report.Warning (1699, 1, a.Location,
				"Use compiler option `{0}' or appropriate project settings instead of `{1}' attribute",
				option, a.Name);
		}

		// TODO: rewrite this code (to kill N bugs and make it faster) and use standard ApplyAttribute way.
		public AssemblyName GetAssemblyName (string name, string output) 
		{
			if (OptAttributes != null) {
				foreach (Attribute a in OptAttributes.Attrs) {
					// cannot rely on any resolve-based members before you call Resolve
					if (a.ExplicitTarget == null || a.ExplicitTarget != "assembly")
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
								RootContext.StrongNameKeyFile = value;
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
								RootContext.StrongNameKeyContainer = value;
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

						RootContext.StrongNameDelaySign = b;
						break;
					}
				}
			}
			
			AssemblyName an = new AssemblyName ();
			an.Name = Path.GetFileNameWithoutExtension (name);

			// note: delay doesn't apply when using a key container
			if (RootContext.StrongNameKeyContainer != null) {
				an.KeyPair = new StrongNameKeyPair (RootContext.StrongNameKeyContainer);
				return an;
			}

			// strongname is optional
			if (RootContext.StrongNameKeyFile == null)
				return an;

			string AssemblyDir = Path.GetDirectoryName (output);

			// the StrongName key file may be relative to (a) the compiled
			// file or (b) to the output assembly. See bugzilla #55320
			// http://bugzilla.ximian.com/show_bug.cgi?id=55320

			// (a) relative to the compiled file
			string filename = Path.GetFullPath (RootContext.StrongNameKeyFile);
			bool exist = File.Exists (filename);
			if ((!exist) && (AssemblyDir != null) && (AssemblyDir != String.Empty)) {
				// (b) relative to the outputed assembly
				filename = Path.GetFullPath (Path.Combine (AssemblyDir, RootContext.StrongNameKeyFile));
				exist = File.Exists (filename);
			}

			if (exist) {
				using (FileStream fs = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
					byte[] snkeypair = new byte [fs.Length];
					fs.Read (snkeypair, 0, snkeypair.Length);

					if (RootContext.StrongNameDelaySign) {
						// delayed signing - DO NOT include private key
						SetPublicKey (an, snkeypair);
					}
					else {
						// no delay so we make sure we have the private key
						try {
							CryptoConvert.FromCapiPrivateKeyBlob (snkeypair);
							an.KeyPair = new StrongNameKeyPair (snkeypair);
						}
						catch (CryptographicException) {
							if (snkeypair.Length == 16) {
								// error # is different for ECMA key
								Report.Error (1606, "Could not sign the assembly. " + 
									"ECMA key can only be used to delay-sign assemblies");
							}
							else {
								Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not have a private key");
							}
							return null;
						}
					}
				}
			}
			else {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not exist");
				return null;
			}
			return an;
		}

		void Error_AssemblySigning (string text)
		{
			Report.Error (1548, "Error during assembly signing. " + text);
		}

		bool CheckInternalsVisibleAttribute (Attribute a)
		{
			string assembly_name = a.GetString ();
			if (assembly_name.Length == 0)
				return false;
				
			AssemblyName aname = null;
			try {
				aname = new AssemblyName (assembly_name);
			} catch (FileLoadException) {
			} catch (ArgumentException) {
			}
				
			// Bad assembly name format
			if (aname == null)
				Report.Warning (1700, 3, a.Location, "Assembly reference `" + assembly_name + "' is invalid and cannot be resolved");
			// Report error if we have defined Version or Culture
			else if (aname.Version != null || aname.CultureInfo != null)
				throw new Exception ("Friend assembly `" + a.GetString () + 
						"' is invalid. InternalsVisibleTo cannot have version or culture specified.");
			else if (aname.GetPublicKey () == null && Name.GetPublicKey () != null && Name.GetPublicKey ().Length != 0) {
				Report.Error (1726, a.Location, "Friend assembly reference `" + aname.FullName + "' is invalid." +
						" Strong named assemblies must specify a public key in their InternalsVisibleTo declarations");
				return false;
			}

			return true;
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

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
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
				flags |= ((uint) cdata[pos + 1]) << 8;
				flags |= ((uint) cdata[pos + 2]) << 16;
				flags |= ((uint) cdata[pos + 3]) << 24;

				// Ignore set PublicKey flag if assembly is not strongnamed
				if ((flags & (uint) AssemblyNameFlags.PublicKey) != 0 && (Builder.GetName ().KeyPair == null))
					flags &= ~(uint)AssemblyNameFlags.PublicKey;

				try {
					var fi = typeof (AssemblyBuilder).GetField ("flags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
					fi.SetValue (Builder, flags);
				} catch {
					Report.RuntimeMissingSupport (a.Location, "AssemblyFlagsAttribute setting");
				}

				return;
			}

			if (a.Type == pa.InternalsVisibleTo && !CheckInternalsVisibleAttribute (a))
				return;

			if (a.Type == pa.TypeForwarder) {
				TypeSpec t = a.GetArgumentType ();
				if (t == null || TypeManager.HasElementType (t)) {
					Report.Error (735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}

				if (emitted_forwarders == null) {
					emitted_forwarders = new Dictionary<ITypeDefinition, Attribute>  ();
				} else if (emitted_forwarders.ContainsKey (t.MemberDefinition)) {
					Report.SymbolRelatedToPreviousError(emitted_forwarders[t.MemberDefinition].Location, null);
					Report.Error(739, a.Location, "A duplicate type forward of type `{0}'",
						TypeManager.CSharpName(t));
					return;
				}

				emitted_forwarders.Add(t.MemberDefinition, a);

				if (t.Assembly == Builder) {
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

			Builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public override void Emit (TypeContainer tc)
		{
			base.Emit (tc);

			if (has_extension_method)
				Compiler.PredefinedAttributes.Extension.EmitAttribute (Builder);

			PredefinedAttribute pa = tc.Compiler.PredefinedAttributes.RuntimeCompatibility;
			if (pa.IsDefined && (OptAttributes == null || !OptAttributes.Contains (pa))) {
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
				}
				catch {
					Report.RuntimeMissingSupport (Location.Null, "assembly permission setting");
				}
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
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
