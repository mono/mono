using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

#if ENABLE_CECIL
using C = Mono.Cecil;
#endif

namespace Mono.Debugger.Soft
{
	public class PropertyInfoMirror : Mirror {

		TypeMirror parent;
		string name;
		PropertyAttributes attrs;
		MethodMirror get_method, set_method;
		CustomAttributeDataMirror[] cattrs;

#if ENABLE_CECIL
		C.PropertyDefinition meta;
#endif

		public PropertyInfoMirror (TypeMirror parent, long id, string name, MethodMirror get_method, MethodMirror set_method, PropertyAttributes attrs) : base (parent.VirtualMachine, id) {
			this.parent = parent;
			this.name = name;
			this.attrs = attrs;
			this.get_method = get_method;
			this.set_method = set_method;
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

		public TypeMirror PropertyType {
			get {
				if (get_method != null)
					return get_method.ReturnType;
				else {
					ParameterInfoMirror[] parameters = set_method.GetParameters ();
					
					return parameters [parameters.Length - 1].ParameterType;
				}
			}
		}

		public PropertyAttributes Attributes {
			get {
				return attrs;
			}
		}

		public bool IsSpecialName {
			get {return (Attributes & PropertyAttributes.SpecialName) != 0;}
		}

		public MethodMirror GetGetMethod ()
		{
			return GetGetMethod (false);
		}

		public MethodMirror GetGetMethod (bool nonPublic)
		{
			if (get_method != null && (nonPublic || get_method.IsPublic))
				return get_method;
			else
				return null;
		}

		public MethodMirror GetSetMethod ()
		{
			return GetSetMethod (false);
		}

		public MethodMirror GetSetMethod (bool nonPublic)
		{
			if (set_method != null && (nonPublic || set_method.IsPublic))
				return set_method;
			else
				return null;
		}

		public ParameterInfoMirror[] GetIndexParameters()
		{
			if (get_method != null)
				return get_method.GetParameters ();
			return new ParameterInfoMirror [0];
		}

#if ENABLE_CECIL
		public C.PropertyDefinition Metadata {		
			get {
				if (parent.Metadata == null)
					return null;
				// FIXME: Speed this up
				foreach (var def in parent.Metadata.Properties) {
					if (def.Name == Name) {
						meta = def;
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

		public CustomAttributeDataMirror[] GetCustomAttributes (bool inherit) {
			return GetCAttrs (null, inherit);
		}

		public CustomAttributeDataMirror[] GetCustomAttributes (TypeMirror attributeType, bool inherit) {
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");
			return GetCAttrs (attributeType, inherit);
		}

		CustomAttributeDataMirror[] GetCAttrs (TypeMirror type, bool inherit) {

#if ENABLE_CECIL
			if (cattrs == null && Metadata != null && !Metadata.HasCustomAttributes)
				cattrs = new CustomAttributeDataMirror [0];
#endif

			// FIXME: Handle inherit
			if (cattrs == null) {
				CattrInfo[] info = vm.conn.Type_GetPropertyCustomAttributes (DeclaringType.Id, id, 0, false);
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

