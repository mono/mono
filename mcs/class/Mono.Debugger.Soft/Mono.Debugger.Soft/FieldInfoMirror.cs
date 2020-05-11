using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

#if ENABLE_CECIL
using C = Mono.Cecil;
#endif

namespace Mono.Debugger.Soft
{
	public class FieldInfoMirror : Mirror {

		TypeMirror parent;
		string name;
		TypeMirror type;
		FieldAttributes attrs;
		CustomAttributeDataMirror[] cattrs;
		bool inited;
		int len_fixed_size_array;

#if ENABLE_CECIL
		C.FieldDefinition meta;
#endif

		public FieldInfoMirror (TypeMirror parent, long id, string name, TypeMirror type, FieldAttributes attrs) : base (parent.VirtualMachine, id) {
			this.parent = parent;
			this.name = name;
			this.type = type;
			this.attrs = attrs;
			this.len_fixed_size_array = -1;
			inited = true;
		}

		public FieldInfoMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public TypeMirror DeclaringType {
			get {
				if (!inited)
					GetInfo ();
				return parent;
			}
		}

		public string Name {
			get {
				if (!inited)
					GetInfo ();
				return name;
			}
		}

		public TypeMirror FieldType {
			get {
				if (!inited)
					GetInfo ();
				return type;
			}
		}

		public FieldAttributes Attributes {
			get {
				if (!inited)
					GetInfo ();
				return attrs;
			}
		}

		void GetInfo () {
			if (inited)
				return;
			var info = vm.conn.Field_GetInfo (id);
			name = info.Name;
			parent = vm.GetType (info.Parent);
			type = vm.GetType (info.TypeId);
			attrs = (FieldAttributes)info.Attrs;
			inited = true;
		}

		public bool IsLiteral
		{
			get {return (Attributes & FieldAttributes.Literal) != 0;}
		} 

		public bool IsStatic
		{
			get {return (Attributes & FieldAttributes.Static) != 0;}
		} 

		public bool IsInitOnly
		{
			get {return (Attributes & FieldAttributes.InitOnly) != 0;}
		}
 
		public Boolean IsPublic
		{ 
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
			}
		}

		public Boolean IsPrivate
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
			}
		}

		public Boolean IsFamily
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
			}
		}

		public Boolean IsAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
			}
		}

		public Boolean IsFamilyAndAssembly
		{
			get {
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;
			}
		}

		public Boolean IsFamilyOrAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;
			}
		}

		public Boolean IsPinvokeImpl
		{
			get
			{
				return (Attributes & FieldAttributes.PinvokeImpl) == FieldAttributes.PinvokeImpl;
			}
		}

		public Boolean IsSpecialName
		{
			get
			{
				return (Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;
			}
		}

		public Boolean IsNotSerialized
		{
			get
			{
				return (Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized;
			}
		}

		public int FixedSize
		{
			get
			{
				if (len_fixed_size_array == -1) {
					if (!vm.Version.AtLeast (2, 53) || !type.IsValueType) {
						len_fixed_size_array = 0;
					}
					else {
						var fbas = this.GetCustomAttributes (true);
						for (int j = 0 ; j < fbas.Length; ++j) {
							if (fbas [j].Constructor.DeclaringType.FullName.Equals("System.Runtime.CompilerServices.FixedBufferAttribute")){
								len_fixed_size_array  = (int) fbas [j].ConstructorArguments[1].Value;
								break;
							}
						}
					}
				}
				return len_fixed_size_array;
			}
		}

		public CustomAttributeDataMirror[] GetCustomAttributes (bool inherit) {
			return GetCAttrs (null, inherit);
		}

		public CustomAttributeDataMirror[] GetCustomAttributes (TypeMirror attributeType, bool inherit) {
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");
			return GetCAttrs (attributeType, inherit);
		}

#if ENABLE_CECIL
		public C.FieldDefinition Metadata {		
			get {
				if (parent.Metadata == null)
					return null;
				// FIXME: Speed this up
				foreach (var fd in parent.Metadata.Fields) {
					if (fd.Name == Name) {
						meta = fd;
						break;
					}
				}
				if (meta == null)
					/* Shouldn't happen */
					throw new NotImplementedException ();
				return meta;
			}
		}
#endif

		CustomAttributeDataMirror[] GetCAttrs (TypeMirror type, bool inherit) {

#if ENABLE_CECIL
			if (cattrs == null && Metadata != null && !Metadata.HasCustomAttributes)
				cattrs = new CustomAttributeDataMirror [0];
#endif

			// FIXME: Handle inherit
			if (cattrs == null) {
				CattrInfo[] info = vm.conn.Type_GetFieldCustomAttributes (DeclaringType.Id, id, 0, false);
				cattrs = CustomAttributeDataMirror.Create (vm, info);
			}
			var res = new List<CustomAttributeDataMirror> ();
			foreach (var attr in cattrs)
				if (type == null || attr.Constructor.DeclaringType == type)
					res.Add (attr);
			return res.ToArray ();
		}

		public string FullName {
			get {
				string type_namespace = DeclaringType.Namespace;
				string type_name = DeclaringType.Name;
				StringBuilder sb = new StringBuilder ();
				if (type_namespace != String.Empty)
					sb.Append (type_namespace).Append (".");
				sb.Append (type_name);
				sb.Append (":");
				sb.Append (Name);
				return sb.ToString ();
			}
		}
	}
}

