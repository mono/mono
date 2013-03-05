//
// AssociatedMetadataTypeTypeDescriptionProvider.cs
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
//

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
#if !MOBILE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
	class AssociatedMetadataTypeTypeDescriptor : CustomTypeDescriptor
	{
		Type type;
		Type associatedMetadataType;
		bool associatedMetadataTypeChecked;
		PropertyDescriptorCollection properties;
		
		Type AssociatedMetadataType {
			get {
				if (!associatedMetadataTypeChecked && associatedMetadataType == null)
					associatedMetadataType = FindMetadataType ();

				return associatedMetadataType;
			}
		}
		
		public AssociatedMetadataTypeTypeDescriptor (ICustomTypeDescriptor parent, Type type)
			: this (parent, type, null)
		{
		}

		public AssociatedMetadataTypeTypeDescriptor (ICustomTypeDescriptor parent, Type type, Type associatedMetadataType)
			: base (parent)
		{
			this.type = type;
			this.associatedMetadataType = associatedMetadataType;
		}

		void CopyAttributes (object[] from, List <Attribute> to)
		{
			foreach (object o in from) {
				Attribute a = o as Attribute;
				if (a == null)
					continue;

				to.Add (a);
			}
		}
		
		public override AttributeCollection GetAttributes ()
		{
			var attributes = new List <Attribute> ();
			CopyAttributes (type.GetCustomAttributes (true), attributes);
			
			Type metaType = AssociatedMetadataType;
			if (metaType != null) 
				CopyAttributes (metaType.GetCustomAttributes (true), attributes);
			
			return new AttributeCollection (attributes.ToArray ());
		}

		public override PropertyDescriptorCollection GetProperties ()
		{
			// Code partially copied from TypeDescriptor.TypeInfo.GetProperties
			if (properties != null)
                                return properties;

			Dictionary <string, MemberInfo> metaMembers = null;
                        var propertiesHash = new Dictionary <string, bool> (); // name - null
                        var propertiesList = new List <AssociatedMetadataTypePropertyDescriptor> ();
                        Type currentType = type;
			Type metaType = AssociatedMetadataType;

			if (metaType != null) {
				metaMembers = new Dictionary <string, MemberInfo> ();
				MemberInfo[] members = metaType.GetMembers (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

				foreach (MemberInfo member in members) {
					switch (member.MemberType) {
						case MemberTypes.Field:
						case MemberTypes.Property:
							break;

						default:
							continue;
					}

					string name = member.Name;
					if (metaMembers.ContainsKey (name))
						continue;

					metaMembers.Add (name, member);
				}
			}
			
                        // Getting properties type by type, because in the case of a property in the child type, where
                        // the "new" keyword is used and also the return type is changed Type.GetProperties returns 
                        // also the parent property. 
                        // 
                        // Note that we also have to preserve the properties order here.
                        // 
                        while (currentType != null && currentType != typeof (object)) {
                                PropertyInfo[] props = currentType.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                                foreach (PropertyInfo property in props) {
					string propName = property.Name;
					
                                        if (property.GetIndexParameters ().Length == 0 && property.CanRead && !propertiesHash.ContainsKey (propName)) {
						MemberInfo metaMember;

						if (metaMembers != null)
							metaMembers.TryGetValue (propName, out metaMember);
						else
							metaMember = null;
                                                propertiesList.Add (new AssociatedMetadataTypePropertyDescriptor (property, metaMember));
                                                propertiesHash.Add (propName, true);
                                        }
                                }
                                currentType = currentType.BaseType;
                        }

                        properties = new PropertyDescriptorCollection ((PropertyDescriptor[]) propertiesList.ToArray (), true);
                        return properties;
		}
		
		Type FindMetadataType ()
		{
			associatedMetadataTypeChecked = true;
			if (type == null)
				return null;
			
			object[] attrs = type.GetCustomAttributes (typeof (MetadataTypeAttribute), true);
			if (attrs == null || attrs.Length == 0)
				return null;

			var attr = attrs [0] as MetadataTypeAttribute;
			if (attr == null)
				return null;

			return attr.MetadataClassType;
		}
	}
}
#endif
