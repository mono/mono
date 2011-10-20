using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using C = Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	public class FieldInfoMirror : Mirror {

		TypeMirror parent;
		string name;
		TypeMirror type;
		FieldAttributes attrs;
		CustomAttributeDataMirror[] cattrs;
		C.FieldDefinition meta;

		public FieldInfoMirror (TypeMirror parent, long id, string name, TypeMirror type, FieldAttributes attrs) : base (parent.VirtualMachine, id) {
			this.parent = parent;
			this.name = name;
			this.type = type;
			this.attrs = attrs;
		}

		public TypeMirror DeclaringType {
			get {
				return parent;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public TypeMirror FieldType {
			get {
				return type;
			}
		}

		public FieldAttributes Attributes {
			get {
				return attrs;
			}
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

		public CustomAttributeDataMirror[] GetCustomAttributes (bool inherit) {
			return GetCAttrs (null, inherit);
		}

		public CustomAttributeDataMirror[] GetCustomAttributes (TypeMirror attributeType, bool inherit) {
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");
			return GetCAttrs (attributeType, inherit);
		}

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

		CustomAttributeDataMirror[] GetCAttrs (TypeMirror type, bool inherit) {
			if (cattrs == null && Metadata != null && !Metadata.HasCustomAttributes)
				cattrs = new CustomAttributeDataMirror [0];

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
	}
}

