// AssemblyLoader.cs
// John Barnette (jbarn@httcb.net)
// Adam Treat (manyoso@yahoo.com)
// 
// Copyright (c) 2002 John Barnette
// Copyright (c) 2002 Adam Treat
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Mono.Doc.Core
{
	public class AssemblyLoader
	{
		#region Instance Fields

		private Assembly assem;

		#endregion // Instance Fields

		#region Constructors and Destructors

		// TODO: add a constructor that allows specification of type and member visibility
		public AssemblyLoader(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new ApplicationException(
					"Cannot find assembly file: " + fileName
					);
			}

			FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, (int)fs.Length);
			fs.Close();

			assem = Assembly.Load(buffer);
		}

		#endregion // Constructors and Destructors

		#region Private Instance Methods

		// TODO: visibility should be configurable in constructor
		private bool MustDocumentType(Type type)
		{
			if (type != null) 
			{
				return ((type.IsPublic ||
					type.IsNestedPublic) &&
					!type.FullName.Equals("Driver") &&
					!type.FullName.Equals("Profile")
					);
			} 
		
			return false;
		}

		// TODO: visibility should be configurable in constructor
		private bool MustDocumentMethod(MethodBase method)
		{
			return (method.IsPublic);
		}

		// TODO: visibility should be configurable in constructor
		private bool MustDocumentField(FieldInfo field)
		{
			return (field.IsPublic);
		}

		private bool IsAlsoAnEvent(Type type, string fullName)
		{
			bool isEvent = false;

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags)) 
			{

				if (eventInfo.EventHandlerType.FullName == fullName) 
				{

					isEvent = true;
					break;
				}
			}

			return isEvent;
		}

		private bool IsAlsoAnEvent(FieldInfo field)
		{
			return IsAlsoAnEvent(field.DeclaringType, field.FieldType.FullName);
		}

		private bool IsAlsoAnEvent(PropertyInfo property)
		{
			return IsAlsoAnEvent(property.DeclaringType, property.PropertyType.FullName);
		}

		#endregion // Private Instance Methods

		#region Public Static Methods

		public static bool IsClass(Type t)
		{
			return (t.IsClass && !IsDelegate(t)) ? true : false;
		}

		public static bool IsDelegate(Type t)
		{
			return t.BaseType.FullName == "System.Delegate" ||
				t.BaseType.FullName == "System.MulticastDelegate";
		}

		public static bool IsStruct(Type t)
		{
			return (t.IsValueType && !t.IsEnum) ? true : false;
		}

		#endregion

		#region Public Instance Methods

		public Type[] GetTypes()
		{
			ArrayList list = new ArrayList();

			foreach (Module m in assem.GetModules())
			{
				Type[] moduleTypes = m.GetTypes();

				foreach (Type type in moduleTypes) 
				{
					if (MustDocumentType(type)) 
					{
						list.Add(type);
					}
				}
			}

			Type[] types = new Type[list.Count];
			int    i     = 0;
			
			foreach (Type type in list) 
			{
				types[i++] = type;
			}

			return types;
		}

		public ConstructorInfo[] GetConstructors(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags = 
				BindingFlags.Instance  | 
				BindingFlags.Public    |
				BindingFlags.NonPublic ;

			ArrayList ctorList = new ArrayList();

			foreach (ConstructorInfo ctor in t.GetConstructors(bindingFlags))
			{
				if (MustDocumentMethod(ctor))
				{
					ctorList.Add(ctor);
				}
			}

			ConstructorInfo[] constructors = new ConstructorInfo[ctorList.Count];
			int               i            = 0;

			foreach (ConstructorInfo c in ctorList)
			{
				constructors[i++] = c;
			}

			return constructors;
		}

		public FieldInfo[] GetFields(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags  = 
				BindingFlags.Instance  | 
				BindingFlags.Static    |
				BindingFlags.Public    |
				BindingFlags.NonPublic ;

			ArrayList   fieldList = new ArrayList();

			foreach (FieldInfo field in t.GetFields(bindingFlags))
			{
				if (MustDocumentField(field))
				{
					fieldList.Add(field);
				}
			}

			FieldInfo[] fields = new FieldInfo[fieldList.Count];
			int         i      = 0;

			foreach (FieldInfo f in fieldList)
			{
				fields[i++] = f;
			}

			return fields;
		}

		public PropertyInfo[] GetProperties(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags  =
				BindingFlags.Instance  |
				BindingFlags.Static    |
				BindingFlags.Public    |
				BindingFlags.NonPublic ;

			ArrayList propertyList = new ArrayList();

			foreach (PropertyInfo property in t.GetProperties(bindingFlags))
			{
				bool hasAccessor = (property.GetGetMethod(true) != null);
				bool hasMutator  = (property.GetSetMethod(true) != null);

				if ((hasAccessor || hasMutator) && !IsAlsoAnEvent(property))
				{
					propertyList.Add(property);
				}
			}

			PropertyInfo[] properties = new PropertyInfo[propertyList.Count];
			int            i          = 0;

			foreach (PropertyInfo p in propertyList)
			{
				properties[i++] = p;
			}

			return properties;
		}

		public MethodInfo[] GetMethods(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags  =
				BindingFlags.Instance  |
				BindingFlags.Static    |
				BindingFlags.Public    |
				BindingFlags.NonPublic ;

			ArrayList methodList = new ArrayList();

			foreach (MethodInfo method in t.GetMethods(bindingFlags))
			{
				if (!(method.Name.StartsWith("get_"))    &&
					!(method.Name.StartsWith("set_"))    &&
					!(method.Name.StartsWith("add_"))    &&
					!(method.Name.StartsWith("remove_")) &&
					!(method.Name.StartsWith("op_"))     &&
					MustDocumentMethod(method))
				{
					methodList.Add(method);
				}
			}

			MethodInfo[] methods = new MethodInfo[methodList.Count];
			int          i       = 0;

			foreach (MethodInfo m in methodList)
			{
				methods[i++] = m;
			}

			return methods;
		}

		public MethodInfo[] GetOperators(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags  =
				BindingFlags.Instance  |
				BindingFlags.Static    |
				BindingFlags.Public    |
				BindingFlags.NonPublic ;

			ArrayList operatorList = new ArrayList();

			foreach (MethodInfo operatorMethod in t.GetMethods(bindingFlags))
			{
				if (operatorMethod.Name.StartsWith("op_") && MustDocumentMethod(operatorMethod))
				{
					operatorList.Add(operatorMethod);
				}
			}

			MethodInfo[] operators = new MethodInfo[operatorList.Count];
			int          i         = 0;

			foreach (MethodInfo m in operatorList)
			{
				operators[i++] = m;
			}

			return operators;
		}

		public EventInfo[] GetEvents(Type t)
		{
			// TODO: should be configurable in constructor
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			ArrayList eventList = new ArrayList();

			foreach (EventInfo eventInfo in t.GetEvents(bindingFlags)) 
			{

				MethodInfo addMethod = eventInfo.GetAddMethod(true);

				if (addMethod != null && MustDocumentMethod(addMethod)) 
				{
					eventList.Add(eventInfo);
				}
			}

			EventInfo[] events = new EventInfo[eventList.Count];
			int         i      = 0;

			foreach (EventInfo e in eventList)
			{
				events[i++] = e;
			}

			return events;
		}

		#endregion // Public Instance Methods

		#region Public Instance Properties

		public Assembly Assembly
		{
			get { return assem; }
		}

		#endregion // Public Instance Properties
	}
}
