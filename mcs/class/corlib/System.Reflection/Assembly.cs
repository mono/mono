using System;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Reflection.Emit;
using System.IO;
using System.Globalization;

namespace System.Reflection {

	public class Assembly : System.Object, System.Reflection.ICustomAttributeProvider,
		System.Security.IEvidenceFactory, System.Runtime.Serialization.ISerializable {

		private string codebase;
		private string copiedcodebase;
		private string fullname;
		private string location;
		private Evidence evidence;
		private MethodInfo entrypoint;
	
		public virtual string CodeBase { get {return codebase;} }

		public virtual string CopiedCodeBase { get {return copiedcodebase;} } 

		public virtual string FullName { get {return fullname;} }

		public MethodInfo EntryPoint { get {return entrypoint;} }

		public virtual Evidence Evidence { get {return evidence;} }

		public String Location { get {return location;} }

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public virtual Boolean IsDefined(Type attributeType)
		{
			return false;
		}

		public virtual bool IsDefined (Type attribute_type, bool inherit)
		{
			return false;
		}

		public virtual Object[] GetCustomAttributes()
		{
			return null;
		}

		public virtual Object[] GetCustomAttributes(Type attributeType)
		{
			return null;
		}
		
		public virtual object [] GetCustomAttributes (bool inherit)
		{
			return null;
		}

		public virtual object [] GetCustomAttributes (Type attribute_type, bool inherit)
		{
			return null;
		}

		public virtual void RemoveOnTypeResolve(ResolveEventHandler handler)
		{
		}

		public virtual void AddOnTypeResolve(ResolveEventHandler handler)
		{
		}

		public virtual void RemoveOnResourceResolve(ResolveEventHandler handler)
		{
		}
		
		public virtual void AddOnResourceResolve(ResolveEventHandler handler)
		{
		}

		public virtual ModuleBuilder DefineDynamicModule(String name, Boolean emitSymbolInfo)
		{
			return null;
		}

		public virtual ModuleBuilder DefineDynamicModule(String name)
		{
			return null;
		}

		public virtual FileStream[] GetFiles()
		{
			return null;
		}

		public virtual FileStream GetFile(String name)
		{
			return null;
		}

		public virtual Stream GetManifestResourceStream(String name)
		{
			return null;
		}

		public virtual Stream GetManifestResourceStream(Type type, String name)
		{
			return null;
		}

		public virtual Type[] GetTypes()
		{
			return null;
		}

		public virtual Type[] GetExportedTypes()
		{
			return null;
		}

		public virtual Type GetType(String name, Boolean throwOnError)
		{
			return null;
		}

		public virtual Type GetType(String name)
		{
			return GetType (name, true);
		}
		
		public virtual AssemblyName GetName(Boolean copiedName)
		{
			return null;
		}

		public virtual AssemblyName GetName()
		{
			return null;
		}

		public override String ToString()
		{
			return "FIXME: assembly";
		}
		
		public static String CreateQualifiedName(String assemblyName, String typeName) 
		{
			return "FIXME: assembly";
		}

		public static String nCreateQualifiedName(String assemblyName, String typeName)
		{
			return "FIXME: assembly";
		}

		public static Assembly GetAssembly(Type type)
		{
			return null;
		}

		public Type GetType(String name, Boolean throwOnError, Boolean ignoreCase)
		{
			return null;
		}

		public Assembly GetSatelliteAssembly(CultureInfo culture)
		{
			return null;
		}

		public static Assembly LoadFrom(String assemblyFile)
		{
			return null;
		}

		public static Assembly LoadFrom(String assemblyFile, Evidence securityEvidence)
		{
			return null;
		}

		public static Assembly Load(String assemblyString)
		{
			return null;
		}
		
		public static Assembly Load(String assemblyString, Evidence assemblySecurity)
		{
			return null;
		}

		public static Assembly Load(AssemblyName assemblyRef)
		{
			return null;
		}

		public static Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			return null;
		}

		public static Assembly Load(Byte[] rawAssembly)
		{
			return null;
		}

		public static Assembly Load(Byte[] rawAssembly, Byte[] rawSymbolStore)
		{
			return null;
		}

		public static Assembly Load(Byte[] rawAssembly, Byte[] rawSymbolStore, Evidence securityEvidence)
		{
			return null;
		}

		public Object CreateInstance(String typeName) 
		{
			return null;
		}

		public Object CreateInstance(String typeName, Boolean ignoreCase)
		{
			return null;
		}

		public Object CreateInstance(String typeName, Boolean ignoreCase, BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes)
		{
		 	return null;
		}

		public Module[] GetLoadedModules()
		{
			return null;
		}

		public Module[] GetModules()
		{
			return null;
		}

		public Module GetModule(String name)
		{
			return null;
		}

		public String[] GetManifestResourceNames()
		{
			return null;
		}

		public static Assembly GetExecutingAssembly()
		{
			return null;
		}

		public AssemblyName[] GetReferencedAssemblies()
		{
			return null;
		}

		public ManifestResourceInfo GetManifestResourceInfo(String resourceName)
		{
			return null;
		}

		public static Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity, String callerLocation)
		{
			return null;
		}

		public static Assembly Load(String assemblyString, Evidence assemblySecurity, String callerLocation)
		{
			return null;
		}

	}
}
