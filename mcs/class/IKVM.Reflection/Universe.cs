/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
using System.IO;
using System.Security;
using System.Text;
using System.Diagnostics;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	public sealed class ResolveEventArgs : EventArgs
	{
		private readonly string name;
		private readonly Assembly requestingAssembly;

		public ResolveEventArgs(string name)
			: this(name, null)
		{
		}

		public ResolveEventArgs(string name, Assembly requestingAssembly)
		{
			this.name = name;
			this.requestingAssembly = requestingAssembly;
		}

		public string Name
		{
			get { return name; }
		}

		public Assembly RequestingAssembly
		{
			get { return requestingAssembly; }
		}
	}

	public enum AssemblyComparisonResult
	{
		Unknown = 0,
		EquivalentFullMatch = 1,
		EquivalentWeakNamed = 2,
		EquivalentFXUnified = 3,
		EquivalentUnified = 4,
		NonEquivalentVersion = 5,
		NonEquivalent = 6,
		EquivalentPartialMatch = 7,
		EquivalentPartialWeakNamed = 8,
		EquivalentPartialUnified = 9,
		EquivalentPartialFXUnified = 10,
		NonEquivalentPartialVersion = 11,
	}

	public delegate Assembly ResolveEventHandler(object sender, ResolveEventArgs args);

	public sealed class Universe : IDisposable
	{
		internal readonly Dictionary<Type, Type> canonicalizedTypes = new Dictionary<Type, Type>();
		private readonly List<Assembly> assemblies = new List<Assembly>();
		private readonly List<AssemblyBuilder> dynamicAssemblies = new List<AssemblyBuilder>();
		private readonly Dictionary<string, Assembly> assembliesByName = new Dictionary<string, Assembly>();
		private readonly Dictionary<System.Type, Type> importedTypes = new Dictionary<System.Type, Type>();
		private Type typeof_System_Object;
		private Type typeof_System_ValueType;
		private Type typeof_System_Enum;
		private Type typeof_System_Void;
		private Type typeof_System_Boolean;
		private Type typeof_System_Char;
		private Type typeof_System_SByte;
		private Type typeof_System_Byte;
		private Type typeof_System_Int16;
		private Type typeof_System_UInt16;
		private Type typeof_System_Int32;
		private Type typeof_System_UInt32;
		private Type typeof_System_Int64;
		private Type typeof_System_UInt64;
		private Type typeof_System_Single;
		private Type typeof_System_Double;
		private Type typeof_System_String;
		private Type typeof_System_IntPtr;
		private Type typeof_System_UIntPtr;
		private Type typeof_System_TypedReference;
		private Type typeof_System_Type;
		private Type typeof_System_Array;
		private Type typeof_System_DateTime;
		private Type typeof_System_DBNull;
		private Type typeof_System_Decimal;
		private Type typeof_System_NonSerializedAttribute;
		private Type typeof_System_SerializableAttribute;
		private Type typeof_System_AttributeUsageAttribute;
		private Type typeof_System_Reflection_AssemblyCultureAttribute;
		private Type typeof_System_Runtime_InteropServices_DllImportAttribute;
		private Type typeof_System_Runtime_InteropServices_FieldOffsetAttribute;
		private Type typeof_System_Runtime_InteropServices_InAttribute;
		private Type typeof_System_Runtime_InteropServices_MarshalAsAttribute;
		private Type typeof_System_Runtime_InteropServices_UnmanagedType;
		private Type typeof_System_Runtime_InteropServices_VarEnum;
		private Type typeof_System_Runtime_InteropServices_OutAttribute;
		private Type typeof_System_Runtime_InteropServices_StructLayoutAttribute;
		private Type typeof_System_Runtime_InteropServices_OptionalAttribute;
		private Type typeof_System_Runtime_InteropServices_PreserveSigAttribute;
		private Type typeof_System_Runtime_InteropServices_ComImportAttribute;
		private Type typeof_System_Runtime_CompilerServices_DecimalConstantAttribute;
		private Type typeof_System_Runtime_CompilerServices_SpecialNameAttribute;
		private Type typeof_System_Runtime_CompilerServices_MethodImplAttribute;
		private Type typeof_System_Security_SuppressUnmanagedCodeSecurityAttribute;
		private Type typeof_System_Reflection_AssemblyCopyrightAttribute;
		private Type typeof_System_Reflection_AssemblyTrademarkAttribute;
		private Type typeof_System_Reflection_AssemblyProductAttribute;
		private Type typeof_System_Reflection_AssemblyCompanyAttribute;
		private Type typeof_System_Reflection_AssemblyDescriptionAttribute;
		private Type typeof_System_Reflection_AssemblyTitleAttribute;
		private Type typeof_System_Reflection_AssemblyInformationalVersionAttribute;
		private Type typeof_System_Reflection_AssemblyFileVersionAttribute;
		private Type typeof_System_Security_Permissions_CodeAccessSecurityAttribute;
		private Type typeof_System_Security_Permissions_HostProtectionAttribute;
		private Type typeof_System_Security_Permissions_PermissionSetAttribute;
		private Type typeof_System_Security_Permissions_SecurityAction;
		private List<ResolveEventHandler> resolvers = new List<ResolveEventHandler>();

		internal Assembly Mscorlib
		{
			get { return Load("mscorlib"); }
		}

		private Type ImportMscorlibType(System.Type type)
		{
			return Mscorlib.GetTypeImpl(type.FullName);
		}

		internal Type System_Object
		{
			get { return typeof_System_Object ?? (typeof_System_Object = ImportMscorlibType(typeof(System.Object))); }
		}

		internal Type System_ValueType
		{
			get { return typeof_System_ValueType ?? (typeof_System_ValueType = ImportMscorlibType(typeof(System.ValueType))); }
		}

		internal Type System_Enum
		{
			get { return typeof_System_Enum ?? (typeof_System_Enum = ImportMscorlibType(typeof(System.Enum))); }
		}

		internal Type System_Void
		{
			get { return typeof_System_Void ?? (typeof_System_Void = ImportMscorlibType(typeof(void))); }
		}

		internal Type System_Boolean
		{
			get { return typeof_System_Boolean ?? (typeof_System_Boolean = ImportMscorlibType(typeof(System.Boolean))); }
		}

		internal Type System_Char
		{
			get { return typeof_System_Char ?? (typeof_System_Char = ImportMscorlibType(typeof(System.Char))); }
		}

		internal Type System_SByte
		{
			get { return typeof_System_SByte ?? (typeof_System_SByte = ImportMscorlibType(typeof(System.SByte))); }
		}

		internal Type System_Byte
		{
			get { return typeof_System_Byte ?? (typeof_System_Byte = ImportMscorlibType(typeof(System.Byte))); }
		}

		internal Type System_Int16
		{
			get { return typeof_System_Int16 ?? (typeof_System_Int16 = ImportMscorlibType(typeof(System.Int16))); }
		}

		internal Type System_UInt16
		{
			get { return typeof_System_UInt16 ?? (typeof_System_UInt16 = ImportMscorlibType(typeof(System.UInt16))); }
		}

		internal Type System_Int32
		{
			get { return typeof_System_Int32 ?? (typeof_System_Int32 = ImportMscorlibType(typeof(System.Int32))); }
		}

		internal Type System_UInt32
		{
			get { return typeof_System_UInt32 ?? (typeof_System_UInt32 = ImportMscorlibType(typeof(System.UInt32))); }
		}

		internal Type System_Int64
		{
			get { return typeof_System_Int64 ?? (typeof_System_Int64 = ImportMscorlibType(typeof(System.Int64))); }
		}

		internal Type System_UInt64
		{
			get { return typeof_System_UInt64 ?? (typeof_System_UInt64 = ImportMscorlibType(typeof(System.UInt64))); }
		}

		internal Type System_Single
		{
			get { return typeof_System_Single ?? (typeof_System_Single = ImportMscorlibType(typeof(System.Single))); }
		}

		internal Type System_Double
		{
			get { return typeof_System_Double ?? (typeof_System_Double = ImportMscorlibType(typeof(System.Double))); }
		}

		internal Type System_String
		{
			get { return typeof_System_String ?? (typeof_System_String = ImportMscorlibType(typeof(System.String))); }
		}

		internal Type System_IntPtr
		{
			get { return typeof_System_IntPtr ?? (typeof_System_IntPtr = ImportMscorlibType(typeof(System.IntPtr))); }
		}

		internal Type System_UIntPtr
		{
			get { return typeof_System_UIntPtr ?? (typeof_System_UIntPtr = ImportMscorlibType(typeof(System.UIntPtr))); }
		}

		internal Type System_TypedReference
		{
			get { return typeof_System_TypedReference ?? (typeof_System_TypedReference = ImportMscorlibType(typeof(System.TypedReference))); }
		}

		internal Type System_Type
		{
			get { return typeof_System_Type ?? (typeof_System_Type = ImportMscorlibType(typeof(System.Type))); }
		}

		internal Type System_Array
		{
			get { return typeof_System_Array ?? (typeof_System_Array = ImportMscorlibType(typeof(System.Array))); }
		}

		internal Type System_DateTime
		{
			get { return typeof_System_DateTime ?? (typeof_System_DateTime = ImportMscorlibType(typeof(System.DateTime))); }
		}

		internal Type System_DBNull
		{
			get { return typeof_System_DBNull ?? (typeof_System_DBNull = ImportMscorlibType(typeof(System.DBNull))); }
		}

		internal Type System_Decimal
		{
			get { return typeof_System_Decimal ?? (typeof_System_Decimal = ImportMscorlibType(typeof(System.Decimal))); }
		}

		internal Type System_NonSerializedAttribute
		{
			get { return typeof_System_NonSerializedAttribute ?? (typeof_System_NonSerializedAttribute = ImportMscorlibType(typeof(System.NonSerializedAttribute))); }
		}

		internal Type System_SerializableAttribute
		{
			get { return typeof_System_SerializableAttribute ?? (typeof_System_SerializableAttribute = ImportMscorlibType(typeof(System.SerializableAttribute))); }
		}

		internal Type System_AttributeUsageAttribute
		{
			get { return typeof_System_AttributeUsageAttribute ?? (typeof_System_AttributeUsageAttribute = ImportMscorlibType(typeof(System.AttributeUsageAttribute))); }
		}

		internal Type System_Reflection_AssemblyCultureAttribute
		{
			get { return typeof_System_Reflection_AssemblyCultureAttribute ?? (typeof_System_Reflection_AssemblyCultureAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyCultureAttribute))); }
		}

		internal Type System_Runtime_InteropServices_DllImportAttribute
		{
			get { return typeof_System_Runtime_InteropServices_DllImportAttribute ?? (typeof_System_Runtime_InteropServices_DllImportAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.DllImportAttribute))); }
		}

		internal Type System_Runtime_InteropServices_FieldOffsetAttribute
		{
			get { return typeof_System_Runtime_InteropServices_FieldOffsetAttribute ?? (typeof_System_Runtime_InteropServices_FieldOffsetAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.FieldOffsetAttribute))); }
		}

		internal Type System_Runtime_InteropServices_InAttribute
		{
			get { return typeof_System_Runtime_InteropServices_InAttribute ?? (typeof_System_Runtime_InteropServices_InAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.InAttribute))); }
		}

		internal Type System_Runtime_InteropServices_MarshalAsAttribute
		{
			get { return typeof_System_Runtime_InteropServices_MarshalAsAttribute ?? (typeof_System_Runtime_InteropServices_MarshalAsAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.MarshalAsAttribute))); }
		}

		internal Type System_Runtime_InteropServices_UnmanagedType
		{
			get { return typeof_System_Runtime_InteropServices_UnmanagedType ?? (typeof_System_Runtime_InteropServices_UnmanagedType = ImportMscorlibType(typeof(System.Runtime.InteropServices.UnmanagedType))); }
		}

		internal Type System_Runtime_InteropServices_VarEnum
		{
			get { return typeof_System_Runtime_InteropServices_VarEnum ?? (typeof_System_Runtime_InteropServices_VarEnum = ImportMscorlibType(typeof(System.Runtime.InteropServices.VarEnum))); }
		}

		internal Type System_Runtime_InteropServices_OutAttribute
		{
			get { return typeof_System_Runtime_InteropServices_OutAttribute ?? (typeof_System_Runtime_InteropServices_OutAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.OutAttribute))); }
		}

		internal Type System_Runtime_InteropServices_StructLayoutAttribute
		{
			get { return typeof_System_Runtime_InteropServices_StructLayoutAttribute ?? (typeof_System_Runtime_InteropServices_StructLayoutAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.StructLayoutAttribute))); }
		}

		internal Type System_Runtime_InteropServices_OptionalAttribute
		{
			get { return typeof_System_Runtime_InteropServices_OptionalAttribute ?? (typeof_System_Runtime_InteropServices_OptionalAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.OptionalAttribute))); }
		}

		internal Type System_Runtime_InteropServices_PreserveSigAttribute
		{
			get { return typeof_System_Runtime_InteropServices_PreserveSigAttribute ?? (typeof_System_Runtime_InteropServices_PreserveSigAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.PreserveSigAttribute))); }
		}

		internal Type System_Runtime_InteropServices_ComImportAttribute
		{
			get { return typeof_System_Runtime_InteropServices_ComImportAttribute ?? (typeof_System_Runtime_InteropServices_ComImportAttribute = ImportMscorlibType(typeof(System.Runtime.InteropServices.ComImportAttribute))); }
		}

		internal Type System_Runtime_CompilerServices_DecimalConstantAttribute
		{
			get { return typeof_System_Runtime_CompilerServices_DecimalConstantAttribute ?? (typeof_System_Runtime_CompilerServices_DecimalConstantAttribute = ImportMscorlibType(typeof(System.Runtime.CompilerServices.DecimalConstantAttribute))); }
		}

		internal Type System_Runtime_CompilerServices_SpecialNameAttribute
		{
			get { return typeof_System_Runtime_CompilerServices_SpecialNameAttribute ?? (typeof_System_Runtime_CompilerServices_SpecialNameAttribute = ImportMscorlibType(typeof(System.Runtime.CompilerServices.SpecialNameAttribute))); }
		}

		internal Type System_Runtime_CompilerServices_MethodImplAttribute
		{
			get { return typeof_System_Runtime_CompilerServices_MethodImplAttribute ?? (typeof_System_Runtime_CompilerServices_MethodImplAttribute = ImportMscorlibType(typeof(System.Runtime.CompilerServices.MethodImplAttribute))); }
		}

		internal Type System_Security_SuppressUnmanagedCodeSecurityAttribute
		{
			get { return typeof_System_Security_SuppressUnmanagedCodeSecurityAttribute ?? (typeof_System_Security_SuppressUnmanagedCodeSecurityAttribute = ImportMscorlibType(typeof(System.Security.SuppressUnmanagedCodeSecurityAttribute))); }
		}

		internal Type System_Reflection_AssemblyCopyrightAttribute
		{
			get { return typeof_System_Reflection_AssemblyCopyrightAttribute ?? (typeof_System_Reflection_AssemblyCopyrightAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyCopyrightAttribute))); }
		}

		internal Type System_Reflection_AssemblyTrademarkAttribute
		{
			get { return typeof_System_Reflection_AssemblyTrademarkAttribute ?? (typeof_System_Reflection_AssemblyTrademarkAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyTrademarkAttribute))); }
		}

		internal Type System_Reflection_AssemblyProductAttribute
		{
			get { return typeof_System_Reflection_AssemblyProductAttribute ?? (typeof_System_Reflection_AssemblyProductAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyProductAttribute))); }
		}

		internal Type System_Reflection_AssemblyCompanyAttribute
		{
			get { return typeof_System_Reflection_AssemblyCompanyAttribute ?? (typeof_System_Reflection_AssemblyCompanyAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyCompanyAttribute))); }
		}

		internal Type System_Reflection_AssemblyDescriptionAttribute
		{
			get { return typeof_System_Reflection_AssemblyDescriptionAttribute ?? (typeof_System_Reflection_AssemblyDescriptionAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyDescriptionAttribute))); }
		}

		internal Type System_Reflection_AssemblyTitleAttribute
		{
			get { return typeof_System_Reflection_AssemblyTitleAttribute ?? (typeof_System_Reflection_AssemblyTitleAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyTitleAttribute))); }
		}

		internal Type System_Reflection_AssemblyInformationalVersionAttribute
		{
			get { return typeof_System_Reflection_AssemblyInformationalVersionAttribute ?? (typeof_System_Reflection_AssemblyInformationalVersionAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyInformationalVersionAttribute))); }
		}

		internal Type System_Reflection_AssemblyFileVersionAttribute
		{
			get { return typeof_System_Reflection_AssemblyFileVersionAttribute ?? (typeof_System_Reflection_AssemblyFileVersionAttribute = ImportMscorlibType(typeof(System.Reflection.AssemblyFileVersionAttribute))); }
		}

		internal Type System_Security_Permissions_CodeAccessSecurityAttribute
		{
			get { return typeof_System_Security_Permissions_CodeAccessSecurityAttribute ?? (typeof_System_Security_Permissions_CodeAccessSecurityAttribute = ImportMscorlibType(typeof(System.Security.Permissions.CodeAccessSecurityAttribute))); }
		}

		internal Type System_Security_Permissions_HostProtectionAttribute
		{
			get { return typeof_System_Security_Permissions_HostProtectionAttribute ?? (typeof_System_Security_Permissions_HostProtectionAttribute = ImportMscorlibType(typeof(System.Security.Permissions.HostProtectionAttribute))); }
		}

		internal Type System_Security_Permissions_PermissionSetAttribute
		{
			get { return typeof_System_Security_Permissions_PermissionSetAttribute ?? (typeof_System_Security_Permissions_PermissionSetAttribute = ImportMscorlibType(typeof(System.Security.Permissions.PermissionSetAttribute))); }
		}

		internal Type System_Security_Permissions_SecurityAction
		{
			get { return typeof_System_Security_Permissions_SecurityAction ?? (typeof_System_Security_Permissions_SecurityAction = ImportMscorlibType(typeof(System.Security.Permissions.SecurityAction))); }
		}

		internal bool HasMscorlib
		{
			get { return GetLoadedAssembly("mscorlib") != null; }
		}

		public event ResolveEventHandler AssemblyResolve
		{
			add { resolvers.Add(value); }
			remove { resolvers.Remove(value); }
		}

		public Type Import(System.Type type)
		{
			Type imported;
			if (!importedTypes.TryGetValue(type, out imported))
			{
				imported = ImportImpl(type);
				if (imported != null)
				{
					importedTypes.Add(type, imported);
				}
			}
			return imported;
		}

		private Type ImportImpl(System.Type type)
		{
			if (type.Assembly == typeof(IKVM.Reflection.Type).Assembly)
			{
				throw new ArgumentException("Did you really want to import " + type.FullName + "?");
			}
			if (type.HasElementType)
			{
				if (type.IsArray)
				{
					if (type.Name.EndsWith("[]"))
					{
						return Import(type.GetElementType()).MakeArrayType();
					}
					else
					{
						return Import(type.GetElementType()).MakeArrayType(type.GetArrayRank());
					}
				}
				else if (type.IsByRef)
				{
					return Import(type.GetElementType()).MakeByRefType();
				}
				else if (type.IsPointer)
				{
					return Import(type.GetElementType()).MakePointerType();
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			else if (type.IsGenericParameter)
			{
				if (type.DeclaringMethod != null)
				{
					throw new NotImplementedException();
				}
				else
				{
					return Import(type.DeclaringType).GetGenericArguments()[type.GenericParameterPosition];
				}
			}
			else if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				System.Type[] args = type.GetGenericArguments();
				Type[] importedArgs = new Type[args.Length];
				for (int i = 0; i < args.Length; i++)
				{
					importedArgs[i] = Import(args[i]);
				}
				return Import(type.GetGenericTypeDefinition()).MakeGenericType(importedArgs);
			}
			else
			{
				return Import(type.Assembly).GetType(type.FullName);
			}
		}

		private Assembly Import(System.Reflection.Assembly asm)
		{
			return Load(asm.FullName);
		}

		public RawModule OpenRawModule(string path)
		{
			path = Path.GetFullPath(path);
			return OpenRawModule(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), path);
		}

		public RawModule OpenRawModule(Stream stream, string location)
		{
			if (!stream.CanRead || !stream.CanSeek)
			{
				throw new NotSupportedException();
			}
			return new RawModule(new ModuleReader(null, this, stream, location));
		}

		public Assembly LoadAssembly(RawModule module)
		{
			string refname = module.GetAssemblyName().FullName;
			Assembly asm = GetLoadedAssembly(refname);
			if (asm == null)
			{
				asm = module.ToAssembly();
				assemblies.Add(asm);
			}
			return asm;
		}

		public Assembly LoadFile(string path)
		{
			try
			{
				using (RawModule module = OpenRawModule(path))
				{
					return LoadAssembly(module);
				}
			}
			catch (IOException x)
			{
				throw new FileNotFoundException(x.Message, x);
			}
			catch (UnauthorizedAccessException x)
			{
				throw new FileNotFoundException(x.Message, x);
			}
		}

		private Assembly GetLoadedAssembly(string refname)
		{
			Assembly asm;
			if (!assembliesByName.TryGetValue(refname, out asm))
			{
				for (int i = 0; i < assemblies.Count; i++)
				{
					AssemblyComparisonResult result;
					// We won't allow FX unification here, because our own (non-Fusion) implementation of CompareAssemblyIdentity doesn't support it and
					// we don't want to create a fundamental functional difference based on that.
					if (CompareAssemblyIdentity(refname, false, assemblies[i].FullName, false, out result) && result != AssemblyComparisonResult.EquivalentFXUnified)
					{
						asm = assemblies[i];
						assembliesByName.Add(refname, asm);
						break;
					}
				}
			}
			return asm;
		}

		private Assembly GetDynamicAssembly(string refname)
		{
			foreach (AssemblyBuilder asm in dynamicAssemblies)
			{
				AssemblyComparisonResult result;
				// We won't allow FX unification here, because our own (non-Fusion) implementation of CompareAssemblyIdentity doesn't support it and
				// we don't want to create a fundamental functional difference based on that.
				if (CompareAssemblyIdentity(refname, false, asm.FullName, false, out result) && result != AssemblyComparisonResult.EquivalentFXUnified)
				{
					return asm;
				}
			}
			return null;
		}

		public Assembly Load(string refname)
		{
			return Load(refname, null, true);
		}

		internal Assembly Load(string refname, Assembly requestingAssembly, bool throwOnError)
		{
			Assembly asm = GetLoadedAssembly(refname);
			if (asm != null)
			{
				return asm;
			}
			if (resolvers.Count == 0)
			{
				asm = DefaultResolver(refname, throwOnError);
			}
			else
			{
				ResolveEventArgs args = new ResolveEventArgs(refname, requestingAssembly);
				foreach (ResolveEventHandler evt in resolvers)
				{
					asm = evt(this, args);
					if (asm != null)
					{
						break;
					}
				}
				if (asm == null)
				{
					asm = GetDynamicAssembly(refname);
				}
			}
			if (asm != null)
			{
				string defname = asm.FullName;
				if (refname != defname)
				{
					assembliesByName.Add(refname, asm);
				}
				return asm;
			}
			if (throwOnError)
			{
				throw new FileNotFoundException(refname);
			}
			return null;
		}

		private Assembly DefaultResolver(string refname, bool throwOnError)
		{
			Assembly asm = GetDynamicAssembly(refname);
			if (asm != null)
			{
				return asm;
			}
			string fileName;
			if (throwOnError)
			{
				try
				{
					fileName = System.Reflection.Assembly.ReflectionOnlyLoad(refname).Location;
				}
				catch (System.BadImageFormatException x)
				{
					throw new BadImageFormatException(x.Message, x);
				}
			}
			else
			{
				try
				{
					fileName = System.Reflection.Assembly.ReflectionOnlyLoad(refname).Location;
				}
				catch (System.BadImageFormatException x)
				{
					throw new BadImageFormatException(x.Message, x);
				}
				catch (FileNotFoundException)
				{
					// we intentionally only swallow the FileNotFoundException, if the file exists but isn't a valid assembly,
					// we should throw an exception
					return null;
				}
			}
			return LoadFile(fileName);
		}

		public Type GetType(string assemblyQualifiedTypeName)
		{
			// to be more compatible with Type.GetType(), we could call Assembly.GetCallingAssembly(),
			// import that assembly and pass it as the context, but implicitly importing is considered evil
			return GetType(null, assemblyQualifiedTypeName, false);
		}

		public Type GetType(string assemblyQualifiedTypeName, bool throwOnError)
		{
			// to be more compatible with Type.GetType(), we could call Assembly.GetCallingAssembly(),
			// import that assembly and pass it as the context, but implicitly importing is considered evil
			return GetType(null, assemblyQualifiedTypeName, throwOnError);
		}

		// note that context is slightly different from the calling assembly (System.Type.GetType),
		// because context is passed to the AssemblyResolve event as the RequestingAssembly
		public Type GetType(Assembly context, string assemblyQualifiedTypeName, bool throwOnError)
		{
			TypeNameParser parser = TypeNameParser.Parse(assemblyQualifiedTypeName, throwOnError);
			if (parser.Error)
			{
				return null;
			}
			return parser.GetType(this, context, throwOnError, assemblyQualifiedTypeName);
		}

		public Assembly[] GetAssemblies()
		{
			Assembly[] array = new Assembly[assemblies.Count + dynamicAssemblies.Count];
			assemblies.CopyTo(array);
			for (int i = 0, j = assemblies.Count; j < array.Length; i++, j++)
			{
				array[j] = dynamicAssemblies[i];
			}
			return array;
		}

		// this is equivalent to the Fusion CompareAssemblyIdentity API
		public bool CompareAssemblyIdentity(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			return Fusion.CompareAssemblyIdentity(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out result);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
		{
			return DefineDynamicAssemblyImpl(name, access, null, null, null, null);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			return DefineDynamicAssemblyImpl(name, access, dir, null, null, null);
		}

#if NET_4_0
		[Obsolete]
#endif
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			return DefineDynamicAssemblyImpl(name, access, dir, requiredPermissions, optionalPermissions, refusedPermissions);
		}

		private AssemblyBuilder DefineDynamicAssemblyImpl(AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			AssemblyBuilder asm = new AssemblyBuilder(this, name, dir, requiredPermissions, optionalPermissions, refusedPermissions);
			dynamicAssemblies.Add(asm);
			return asm;
 		}

		internal void RenameAssembly(Assembly assembly, AssemblyName oldName)
		{
			List<string> remove = new List<string>();
			foreach (KeyValuePair<string, Assembly> kv in assembliesByName)
			{
				if (kv.Value == assembly)
				{
					remove.Add(kv.Key);
				}
			}
			foreach (string key in remove)
			{
				assembliesByName.Remove(key);
			}
		}

		public void Dispose()
		{
			foreach (Assembly asm in assemblies)
			{
				foreach (Module mod in asm.GetLoadedModules())
				{
					mod.Dispose();
				}
			}
			foreach (AssemblyBuilder asm in dynamicAssemblies)
			{
				foreach (Module mod in asm.GetLoadedModules())
				{
					mod.Dispose();
				}
			}
		}

		public Assembly CreateMissingAssembly(string assemblyName)
		{
			return new MissingAssembly(this, assemblyName);
		}
	}
}
