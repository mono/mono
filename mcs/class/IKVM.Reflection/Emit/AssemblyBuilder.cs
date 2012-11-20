/*
  Copyright (C) 2008-2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Security.Cryptography;
using System.Security;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class AssemblyBuilder : Assembly
	{
		private readonly string name;
		private ushort majorVersion;
		private ushort minorVersion;
		private ushort buildVersion;
		private ushort revisionVersion;
		private string culture;
		private AssemblyNameFlags flags;
		private AssemblyHashAlgorithm hashAlgorithm;
		private StrongNameKeyPair keyPair;
		private byte[] publicKey;
		internal readonly string dir;
		private readonly PermissionSet requiredPermissions;
		private readonly PermissionSet optionalPermissions;
		private readonly PermissionSet refusedPermissions;
		private PEFileKinds fileKind = PEFileKinds.Dll;
		private MethodInfo entryPoint;
		private VersionInfo versionInfo;
		private byte[] win32icon;
		private byte[] win32manifest;
		private byte[] win32resources;
		private string imageRuntimeVersion;
		internal int mdStreamVersion = 0x20000;
		private Module pseudoManifestModule;
		private readonly List<ResourceFile> resourceFiles = new List<ResourceFile>();
		private readonly List<ModuleBuilder> modules = new List<ModuleBuilder>();
		private readonly List<Module> addedModules = new List<Module>();
		private readonly List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();
		private readonly List<CustomAttributeBuilder> declarativeSecurity = new List<CustomAttributeBuilder>();
		private readonly List<Type> typeForwarders = new List<Type>();

		private struct ResourceFile
		{
			internal string Name;
			internal string FileName;
			internal ResourceAttributes Attributes;
			internal ResourceWriter Writer;
		}

		internal AssemblyBuilder(Universe universe, AssemblyName name, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
			: base(universe)
		{
			this.name = name.Name;
			SetVersionHelper(name.Version);
			if (!string.IsNullOrEmpty(name.Culture))
			{
				this.culture = name.Culture;
			}
			this.flags = name.RawFlags;
			this.hashAlgorithm = name.HashAlgorithm;
			if (this.hashAlgorithm == AssemblyHashAlgorithm.None)
			{
				this.hashAlgorithm = AssemblyHashAlgorithm.SHA1;
			}
			this.keyPair = name.KeyPair;
			if (this.keyPair != null)
			{
				this.publicKey = this.keyPair.PublicKey;
			}
			else
			{
				byte[] publicKey = name.GetPublicKey();
				if (publicKey != null && publicKey.Length != 0)
				{
					this.publicKey = (byte[])publicKey.Clone();
				}
			}
			this.dir = dir ?? ".";
			this.requiredPermissions = requiredPermissions;
			this.optionalPermissions = optionalPermissions;
			this.refusedPermissions = refusedPermissions;
			if (universe.HasMscorlib && !universe.Mscorlib.__IsMissing && universe.Mscorlib.ImageRuntimeVersion != null)
			{
				this.imageRuntimeVersion = universe.Mscorlib.ImageRuntimeVersion;
			}
			else
			{
				this.imageRuntimeVersion = typeof(object).Assembly.ImageRuntimeVersion;
			}
		}

		private void SetVersionHelper(Version version)
		{
			if (version == null)
			{
				majorVersion = 0;
				minorVersion = 0;
				buildVersion = 0;
				revisionVersion = 0;
			}
			else
			{
				majorVersion = (ushort)version.Major;
				minorVersion = (ushort)version.Minor;
				buildVersion = version.Build == -1 ? (ushort)0 : (ushort)version.Build;
				revisionVersion = version.Revision == -1 ? (ushort)0 : (ushort)version.Revision;
			}
		}

		private void Rename(AssemblyName oldName)
		{
			this.fullName = null;
			universe.RenameAssembly(this, oldName);
		}

		public void __SetAssemblyVersion(Version version)
		{
			AssemblyName oldName = GetName();
			SetVersionHelper(version);
			Rename(oldName);
		}

		public void __SetAssemblyCulture(string cultureName)
		{
			AssemblyName oldName = GetName();
			this.culture = cultureName;
			Rename(oldName);
		}

		public void __SetAssemblyKeyPair(StrongNameKeyPair keyPair)
		{
			AssemblyName oldName = GetName();
			this.keyPair = keyPair;
			if (keyPair != null)
			{
				this.publicKey = keyPair.PublicKey;
			}
			Rename(oldName);
		}

		// this is used in combination with delay signing
		public void __SetAssemblyPublicKey(byte[] publicKey)
		{
			AssemblyName oldName = GetName();
			this.publicKey = publicKey == null ? null : (byte[])publicKey.Clone();
			Rename(oldName);
		}

		public void __SetAssemblyAlgorithmId(AssemblyHashAlgorithm hashAlgorithm)
		{
			this.hashAlgorithm = hashAlgorithm;
		}

		[Obsolete("Use __AssemblyFlags property instead.")]
		public void __SetAssemblyFlags(AssemblyNameFlags flags)
		{
			this.__AssemblyFlags = flags;
		}

		protected override AssemblyNameFlags GetAssemblyFlags()
		{
			return flags;
		}

		public new AssemblyNameFlags __AssemblyFlags
		{
			get { return flags; }
			set
			{
				AssemblyName oldName = GetName();
				this.flags = value;
				Rename(oldName);
			}
		}

		internal string Name
		{
			get { return name; }
		}

		public override AssemblyName GetName()
		{
			AssemblyName n = new AssemblyName();
			n.Name = name;
			n.Version = new Version(majorVersion, minorVersion, buildVersion, revisionVersion);
			n.Culture = culture ?? "";
			n.HashAlgorithm = hashAlgorithm;
			n.RawFlags = flags;
			n.SetPublicKey(publicKey != null ? (byte[])publicKey.Clone() : Empty<byte>.Array);
			n.KeyPair = keyPair;
			return n;
		}

		public override string Location
		{
			get { throw new NotSupportedException(); }
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			return DefineDynamicModule(name, fileName, false);
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo)
		{
			ModuleBuilder module = new ModuleBuilder(this, name, fileName, emitSymbolInfo);
			modules.Add(module);
			return module;
		}

		public ModuleBuilder GetDynamicModule(string name)
		{
			foreach (ModuleBuilder module in modules)
			{
				if (module.Name == name)
				{
					return module;
				}
			}
			return null;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			customAttributes.Add(customBuilder);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			declarativeSecurity.Add(customBuilder);
		}

		public void __AddTypeForwarder(Type type)
		{
			typeForwarders.Add(type);
		}

		public void SetEntryPoint(MethodInfo entryMethod)
		{
			SetEntryPoint(entryMethod, PEFileKinds.ConsoleApplication);
		}

		public void SetEntryPoint(MethodInfo entryMethod, PEFileKinds fileKind)
		{
			this.entryPoint = entryMethod;
			this.fileKind = fileKind;
		}

		public void __Save(Stream stream, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek || stream.Position != 0)
			{
				throw new ArgumentException("Stream must support read/write/seek and current position must be zero.", "stream");
			}
			if (modules.Count != 1)
			{
				throw new NotSupportedException("Saving to a stream is only supported for single module assemblies.");
			}
			SaveImpl(modules[0].fileName, stream, portableExecutableKind, imageFileMachine);
		}

		public void Save(string assemblyFileName)
		{
			Save(assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
		}

		public void Save(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			SaveImpl(assemblyFileName, null, portableExecutableKind, imageFileMachine);
		}

		private void SaveImpl(string assemblyFileName, Stream streamOrNull, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			ModuleBuilder manifestModule = null;

			foreach (ModuleBuilder moduleBuilder in modules)
			{
				moduleBuilder.SetIsSaved();
				moduleBuilder.PopulatePropertyAndEventTables();

				if (manifestModule == null
					&& string.Compare(moduleBuilder.fileName, assemblyFileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					manifestModule = moduleBuilder;
				}
			}

			if (manifestModule == null)
			{
				manifestModule = DefineDynamicModule("RefEmit_OnDiskManifestModule", assemblyFileName, false);
			}

			AssemblyTable.Record assemblyRecord = new AssemblyTable.Record();
			assemblyRecord.HashAlgId = (int)hashAlgorithm;
			assemblyRecord.Name = manifestModule.Strings.Add(name);
			assemblyRecord.MajorVersion = majorVersion;
			assemblyRecord.MinorVersion = minorVersion;
			assemblyRecord.BuildNumber = buildVersion;
			assemblyRecord.RevisionNumber = revisionVersion;
			if (publicKey != null)
			{
				assemblyRecord.PublicKey = manifestModule.Blobs.Add(ByteBuffer.Wrap(publicKey));
				assemblyRecord.Flags = (int)(flags | AssemblyNameFlags.PublicKey);
			}
			else
			{
				assemblyRecord.Flags = (int)(flags & ~AssemblyNameFlags.PublicKey);
			}
			if (culture != null)
			{
				assemblyRecord.Culture = manifestModule.Strings.Add(culture);
			}
			int token = 0x20000000 + manifestModule.AssemblyTable.AddRecord(assemblyRecord);

#pragma warning disable 618
			// this values are obsolete, but we already know that so we disable the warning
			System.Security.Permissions.SecurityAction requestMinimum = System.Security.Permissions.SecurityAction.RequestMinimum;
			System.Security.Permissions.SecurityAction requestOptional = System.Security.Permissions.SecurityAction.RequestOptional;
			System.Security.Permissions.SecurityAction requestRefuse = System.Security.Permissions.SecurityAction.RequestRefuse;
#pragma warning restore 618
			if (requiredPermissions != null)
			{
				manifestModule.AddDeclarativeSecurity(token, requestMinimum, requiredPermissions);
			}
			if (optionalPermissions != null)
			{
				manifestModule.AddDeclarativeSecurity(token, requestOptional, optionalPermissions);
			}
			if (refusedPermissions != null)
			{
				manifestModule.AddDeclarativeSecurity(token, requestRefuse, refusedPermissions);
			}

			ResourceSection unmanagedResources = versionInfo != null || win32icon != null || win32manifest != null || win32resources != null
				? new ResourceSection()
				: null;

			if (versionInfo != null)
			{
				versionInfo.SetName(GetName());
				versionInfo.SetFileName(assemblyFileName);
				foreach (CustomAttributeBuilder cab in customAttributes)
				{
					// .NET doesn't support copying blob custom attributes into the version info
					if (!cab.HasBlob)
					{
						versionInfo.SetAttribute(cab);
					}
				}
				ByteBuffer versionInfoData = new ByteBuffer(512);
				versionInfo.Write(versionInfoData);
				unmanagedResources.AddVersionInfo(versionInfoData);
			}

			if (win32icon != null)
			{
				unmanagedResources.AddIcon(win32icon);
			}

			if (win32manifest != null)
			{
				unmanagedResources.AddManifest(win32manifest, fileKind == PEFileKinds.Dll ? (ushort)2 : (ushort)1);
			}

			if (win32resources != null)
			{
				unmanagedResources.ExtractResources(win32resources);
			}

			foreach (CustomAttributeBuilder cab in customAttributes)
			{
				// we intentionally don't filter out the version info (pseudo) custom attributes (to be compatible with .NET)
				manifestModule.SetCustomAttribute(0x20000001, cab);
			}

			manifestModule.AddDeclarativeSecurity(0x20000001, declarativeSecurity);

			foreach (Type type in typeForwarders)
			{
				manifestModule.AddTypeForwarder(type);
			}

			foreach (ResourceFile resfile in resourceFiles)
			{
				if (resfile.Writer != null)
				{
					resfile.Writer.Generate();
					resfile.Writer.Close();
				}
				int fileToken = AddFile(manifestModule, resfile.FileName, 1 /*ContainsNoMetaData*/);
				ManifestResourceTable.Record rec = new ManifestResourceTable.Record();
				rec.Offset = 0;
				rec.Flags = (int)resfile.Attributes;
				rec.Name = manifestModule.Strings.Add(resfile.Name);
				rec.Implementation = fileToken;
				manifestModule.ManifestResource.AddRecord(rec);
			}

			int entryPointToken = 0;

			foreach (ModuleBuilder moduleBuilder in modules)
			{
				moduleBuilder.FillAssemblyRefTable();
				moduleBuilder.EmitResources();
				if (moduleBuilder != manifestModule)
				{
					int fileToken;
					if (entryPoint != null && entryPoint.Module == moduleBuilder)
					{
						ModuleWriter.WriteModule(null, null, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, moduleBuilder.unmanagedResources, entryPoint.MetadataToken);
						entryPointToken = fileToken = AddFile(manifestModule, moduleBuilder.fileName, 0 /*ContainsMetaData*/);
					}
					else
					{
						ModuleWriter.WriteModule(null, null, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, moduleBuilder.unmanagedResources, 0);
						fileToken = AddFile(manifestModule, moduleBuilder.fileName, 0 /*ContainsMetaData*/);
					}
					moduleBuilder.ExportTypes(fileToken, manifestModule);
				}
			}

			foreach (Module module in addedModules)
			{
				int fileToken = AddFile(manifestModule, module.FullyQualifiedName, 0 /*ContainsMetaData*/);
				module.ExportTypes(fileToken, manifestModule);
			}

			if (entryPointToken == 0 && entryPoint != null)
			{
				entryPointToken = entryPoint.MetadataToken;
			}

			// finally, write the manifest module
			ModuleWriter.WriteModule(keyPair, publicKey, manifestModule, fileKind, portableExecutableKind, imageFileMachine, unmanagedResources ?? manifestModule.unmanagedResources, entryPointToken, streamOrNull);
		}

		private int AddFile(ModuleBuilder manifestModule, string fileName, int flags)
		{
			SHA1Managed hash = new SHA1Managed();
			string fullPath = fileName;
			if (dir != null)
			{
				fullPath = Path.Combine(dir, fileName);
			}
			using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
			{
				using (CryptoStream cs = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
				{
					byte[] buf = new byte[8192];
					ModuleWriter.HashChunk(fs, cs, buf, (int)fs.Length);
				}
			}
			return manifestModule.__AddModule(flags, Path.GetFileName(fileName), hash.Hash);
		}

		public void AddResourceFile(string name, string fileName)
		{
			AddResourceFile(name, fileName, ResourceAttributes.Public);
		}

		public void AddResourceFile(string name, string fileName, ResourceAttributes attribs)
		{
			ResourceFile resfile = new ResourceFile();
			resfile.Name = name;
			resfile.FileName = fileName;
			resfile.Attributes = attribs;
			resourceFiles.Add(resfile);
		}

		public IResourceWriter DefineResource(string name, string description, string fileName)
		{
			return DefineResource(name, description, fileName, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource(string name, string description, string fileName, ResourceAttributes attribute)
		{
			// FXBUG we ignore the description, because there is no such thing

			string fullPath = fileName;
			if (dir != null)
			{
				fullPath = Path.Combine(dir, fileName);
			}
			ResourceWriter rw = new ResourceWriter(fullPath);
			ResourceFile resfile;
			resfile.Name = name;
			resfile.FileName = fileName;
			resfile.Attributes = attribute;
			resfile.Writer = rw;
			resourceFiles.Add(resfile);
			return rw;
		}

		public void DefineVersionInfoResource()
		{
			if (versionInfo != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			versionInfo = new VersionInfo();
		}

		public void DefineVersionInfoResource(string product, string productVersion, string company, string copyright, string trademark)
		{
			if (versionInfo != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			versionInfo = new VersionInfo();
			versionInfo.product = product;
			versionInfo.informationalVersion = productVersion;
			versionInfo.company = company;
			versionInfo.copyright = copyright;
			versionInfo.trademark = trademark;
		}

		public void __DefineIconResource(byte[] iconFile)
		{
			if (win32icon != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			win32icon = (byte[])iconFile.Clone();
		}

		public void __DefineManifestResource(byte[] manifest)
		{
			if (win32manifest != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			win32manifest = (byte[])manifest.Clone();
		}

		public void __DefineUnmanagedResource(byte[] resource)
		{
			if (versionInfo != null || win32icon != null || win32manifest != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			// The standard .NET DefineUnmanagedResource(byte[]) is useless, because it embeds "resource" (as-is) as the .rsrc section,
			// but it doesn't set the PE file Resource Directory entry to point to it. That's why we have a renamed version, which behaves
			// like DefineUnmanagedResource(string).
			win32resources = (byte[])resource.Clone();
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			// This method reads the specified resource file (Win32 .res file) and converts it into the appropriate format and embeds it in the .rsrc section,
			// also setting the Resource Directory entry.
			__DefineUnmanagedResource(File.ReadAllBytes(resourceFileName));
		}

		public override Type[] GetTypes()
		{
			List<Type> list = new List<Type>();
			foreach (ModuleBuilder module in modules)
			{
				module.GetTypesImpl(list);
			}
			foreach (Module module in addedModules)
			{
				module.GetTypesImpl(list);
			}
			return list.ToArray();
		}

		internal override Type FindType(TypeName typeName)
		{
			foreach (ModuleBuilder mb in modules)
			{
				Type type = mb.FindType(typeName);
				if (type != null)
				{
					return type;
				}
			}
			foreach (Module module in addedModules)
			{
				Type type = module.FindType(typeName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			foreach (ModuleBuilder mb in modules)
			{
				Type type = mb.FindTypeIgnoreCase(lowerCaseName);
				if (type != null)
				{
					return type;
				}
			}
			foreach (Module module in addedModules)
			{
				Type type = module.FindTypeIgnoreCase(lowerCaseName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		public override string ImageRuntimeVersion
		{
			get { return imageRuntimeVersion; }
		}

		public void __SetImageRuntimeVersion(string imageRuntimeVersion, int mdStreamVersion)
		{
			this.imageRuntimeVersion = imageRuntimeVersion;
			this.mdStreamVersion = mdStreamVersion;
		}

		public override Module ManifestModule
		{
			get
			{
				if (pseudoManifestModule == null)
				{
					pseudoManifestModule = new ManifestModule(this);
				}
				return pseudoManifestModule;
			}
		}

		public override MethodInfo EntryPoint
		{
			get { return entryPoint; }
		}

		public override AssemblyName[] GetReferencedAssemblies()
		{
			return Empty<AssemblyName>.Array;
		}

		public override Module[] GetLoadedModules(bool getResourceModules)
		{
			return GetModules(getResourceModules);
		}

		public override Module[] GetModules(bool getResourceModules)
		{
			List<Module> list = new List<Module>();
			foreach (ModuleBuilder module in modules)
			{
				if (getResourceModules || !module.IsResource())
				{
					list.Add(module);
				}
			}
			foreach (Module module in addedModules)
			{
				if (getResourceModules || !module.IsResource())
				{
					list.Add(module);
				}
			}
			return list.ToArray();
		}

		public override Module GetModule(string name)
		{
			foreach (ModuleBuilder module in modules)
			{
				if (module.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return module;
				}
			}
			foreach (Module module in addedModules)
			{
				if (module.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return module;
				}
			}
			return null;
		}

		public Module __AddModule(RawModule module)
		{
			Module mod = module.ToModule(this);
			addedModules.Add(mod);
			return mod;
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			throw new NotSupportedException();
		}

		public override string[] GetManifestResourceNames()
		{
			throw new NotSupportedException();
		}

		public override Stream GetManifestResourceStream(string resourceName)
		{
			throw new NotSupportedException();
		}

		public override bool IsDynamic
		{
			get { return true; }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			foreach (CustomAttributeBuilder cab in customAttributes)
			{
				if (attributeType == null || attributeType.IsAssignableFrom(cab.Constructor.DeclaringType))
				{
					list.Add(cab.ToData(this));
				}
			}
			return list;
		}

		internal bool IsWindowsRuntime
		{
			get { return (flags & (AssemblyNameFlags)0x200) != 0; }
		}
	}

	sealed class ManifestModule : NonPEModule
	{
		private readonly AssemblyBuilder assembly;
		private readonly Guid guid = Guid.NewGuid();

		internal ManifestModule(AssemblyBuilder assembly)
			: base(assembly.universe)
		{
			this.assembly = assembly;
		}

		public override int MDStreamVersion
		{
			get { return assembly.mdStreamVersion; }
		}

		public override Assembly Assembly
		{
			get { return assembly; }
		}

		internal override Type FindType(TypeName typeName)
		{
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			return null;
		}

		internal override void  GetTypesImpl(List<Type> list)
		{
		}

		public override string FullyQualifiedName
		{
			get { return Path.Combine(assembly.dir, "RefEmit_InMemoryManifestModule"); }
		}

		public override string Name
		{
			get { return "<In Memory Module>"; }
		}

		public override Guid ModuleVersionId
		{
			get { return guid; }
		}

		public override string ScopeName
		{
			get { return "RefEmit_InMemoryManifestModule"; }
		}

		protected override Exception NotSupportedException()
		{
			return new InvalidOperationException();
		}
	}
}
