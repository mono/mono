//
// ExplicitNodeFinder.cs: finds sub-nodes for a given NodeInfo
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ExplicitNodeFinder : NodeFinder {

		private static BooleanSwitch info = 
			new BooleanSwitch ("explicit-node-finder", "ExplicitNodeFinder messages");

		// supported sub-groups
		private const string parameters = "GetParameters():";
		private const string customAttributes= "GetCustomAttributes(true):";
		private const string typeInformation = "System.Type Properties:";

		public override NodeInfoCollection GetChildren (NodeInfo root)
		{
			// We don't want an infinite loop; quite showing children
			Trace.WriteLineIf (info.Enabled, "GetChildren for root.NodeType=" + root.NodeType);
			if (!CanShowChildren (root))
				return new NodeInfoCollection();

			return base.GetChildren (root);
		}

		private static bool CanShowChildren (NodeInfo node)
		{
			Trace.WriteLineIf (info.Enabled, "CanShowChildren");
			if (node.Parent != null) {
				if (node.NodeType == NodeTypes.Parameter)
					return true;
				if (node.NodeType == NodeTypes.Other)
					return true;
				if (InHistory (2, node, NodeTypes.ReturnValue) && 
						(CountType(node, NodeTypes.ReturnValue) < 2))
					return true;

				NodeTypes t = node.Parent.NodeType;
				// Console.WriteLine ("** CanShowChildren: {0}", t);
				switch (t) {
				case NodeTypes.Type:
				// case NodeTypes.Other:
					return true;
				default:
					return false;
				}
			}
			return true;
		}

		static bool InHistory (int count, NodeInfo root, params NodeTypes[] types)
		{
			while ((root != null) && (count-- != 0)) {
				foreach (NodeTypes t in types)
					if (root.NodeType == t)
						return true;
				root = root.Parent;
			}
			return false;
		}

		static int CountType (NodeInfo root, NodeTypes type)
		{
			int count = 0;
			while (root != null) {
				if (root.NodeType == type)
					++count;
				root = root.Parent;
			}
			return count;
		}

		protected override void GetTypeChildren (NodeInfoCollection c, NodeInfo parent, Type type)
		{
			object instance = parent.ReflectionInstance;

			// System.Type information
			if (ShowTypeProperties)
				c.Add (new NodeInfo (parent, 
						new NodeGroup (typeInformation, type,
							new NodeGrouper (GetTypeInformationChildren))));

			// Base Type
			if (ShowBase)
				c.Add (new NodeInfo (parent, NodeTypes.BaseType, type.BaseType, type.BaseType));

			// Implemented Interfaces
			if (ShowInterfaces)
				foreach (Type t in type.GetInterfaces())
					c.Add (new NodeInfo (parent, NodeTypes.Interface, t, instance));

			// Constructors
			if (ShowConstructors)
				foreach (ConstructorInfo ci in type.GetConstructors (BindingFlags))
					c.Add (new NodeInfo (parent, ci, instance));

			// Methods
			if (ShowMethods)
				foreach (MethodInfo mi in type.GetMethods (BindingFlags))
					c.Add (new NodeInfo (parent, mi, instance));

			// Fields
			if (ShowFields)
				foreach (FieldInfo fi in type.GetFields (BindingFlags))
					c.Add (new NodeInfo (parent, fi, instance));

			// Properties
			if (ShowProperties)
				foreach (PropertyInfo pi in type.GetProperties (BindingFlags))
					c.Add (new NodeInfo (parent, pi, instance));

			// Events
			if (ShowEvents)
				foreach (EventInfo ei in type.GetEvents (BindingFlags))
					c.Add (new NodeInfo (parent, ei, instance));
		}

		private void GetTypeInformationChildren (NodeInfoCollection c, NodeInfo parent, object args)
		{
			Type type = (Type) args;
			AddMemberChildren (parent, type, c);
			AddMembers (parent, type, "Delimiter", c);
			AddMembers (parent, type, "EmptyTypes", c);
			AddMembers (parent, type, "FilterAttribute", c);
			AddMembers (parent, type, "FilterName", c);
			AddMembers (parent, type, "FilterNameIgnoreCase", c);
			AddMembers (parent, type, "Missing", c);

			AddMembers (parent, type, "Assembly", c);
			c.Add (new NodeInfo (parent,
						new NodeGroup ("Assembly Attributes", type.Assembly,
							new NodeGrouper (GetAssemblyChildren))));

			AddMembers (parent, type, "AssemblyQualifiedName", c);
			AddMembers (parent, type, "Attributes", c);
			AddMembers (parent, type, "BaseType", c);
			AddMembers (parent, type, "DeclaringType", c);
			AddMembers (parent, type, "DefaultBinder", c);
			AddMembers (parent, type, "FullName", c);
			AddMembers (parent, type, "GUID", c);
			AddMembers (parent, type, "HasElementType", c);
			AddMembers (parent, type, "IsAbstract", c);
			AddMembers (parent, type, "IsAnsiClass", c);
			AddMembers (parent, type, "IsArray", c);
			AddMembers (parent, type, "IsAutoClass", c);
			AddMembers (parent, type, "IsAutoLayout", c);
			AddMembers (parent, type, "IsByRef", c);
			AddMembers (parent, type, "IsClass", c);
			AddMembers (parent, type, "IsCOMObject", c);
			AddMembers (parent, type, "IsContextful", c);
			AddMembers (parent, type, "IsEnum", c);
			AddMembers (parent, type, "IsExplicitLayout", c);
			AddMembers (parent, type, "IsImport", c);
			AddMembers (parent, type, "IsInterface", c);
			AddMembers (parent, type, "IsLayoutSequential", c);
			AddMembers (parent, type, "IsMarshalByRef", c);
			AddMembers (parent, type, "IsNestedAssembly", c);
			AddMembers (parent, type, "IsNestedFamORAssem", c);
			AddMembers (parent, type, "IsNestedPrivate", c);
			AddMembers (parent, type, "IsNotPublic", c);
			AddMembers (parent, type, "IsPointer", c);
			AddMembers (parent, type, "IsPrimitive", c);
			AddMembers (parent, type, "IsPublic", c);
			AddMembers (parent, type, "IsSealed", c);
			AddMembers (parent, type, "IsSerializable", c);
			AddMembers (parent, type, "IsSpecialName", c);
			AddMembers (parent, type, "IsUnicodeClass", c);
			AddMembers (parent, type, "IsValueType", c);
			AddMembers (parent, type, "Module", c);
			AddMembers (parent, type, "Namespace", c);
			AddMembers (parent, type, "TypeHandle", c);
			AddMembers (parent, type, "TypeInitializer", c);
			AddMembers (parent, type, "UnderlyingSystemType", c);
		}

		private void GetAssemblyChildren (NodeInfoCollection c, NodeInfo parent, object args)
		{
			Assembly a = (Assembly) args;
			AddMembers (parent, a, "CodeBase", c);
			AddMembers (parent, a, "EntryPoint", c);
			AddMembers (parent, a, "EscapedCodeBase", c);
			AddMembers (parent, a, "Evidence", c);
			AddMembers (parent, a, "FullName", c);
			AddMembers (parent, a, "GlobalAssemblyCache", c);
			AddMembers (parent, a, "Location", c);
		}

		private void AddMembers (NodeInfo parent, object instance, string member, NodeInfoCollection c)
		{
			Type type = instance.GetType ();
			foreach (MemberInfo mi in type.GetMember (member, BindingFlags)) {
				c.Add (new NodeInfo (parent, mi, instance));
			}
		}

		private void AddMemberChildren (NodeInfo parent, MemberInfo mi, NodeInfoCollection c)
		{
			AddMembers (parent, mi, "DeclaringType", c);
			AddMembers (parent, mi, "MemberType", c);
			AddMembers (parent, mi, "Name", c);
			AddMembers (parent, mi, "ReflectedType", c);
			// AddMembers (parent, mi, "GetCustomAttributes", c);
			c.Add (new NodeInfo (parent,
						new NodeGroup (customAttributes,
							mi.GetCustomAttributes (true),
							new NodeGrouper (GetCustomAttributeProviderChildren)))); 
			// c.Add (new NodeInfo (parent, mi, customAttributes, NodeTypes.Other));
			// c.Add (new NodeInfo (parent, mi, mi, NodeTypes.CustomAttributeProvider));
		}

		private void GetCustomAttributeProviderChildren (NodeInfoCollection c, NodeInfo root, object args)
		{
			object[] attrs = (object[]) args;
			foreach (object attr in attrs) {
				// TODO: specify type?
				c.Add (new NodeInfo (root, attr));
			}
		}

		protected override void GetFieldChildren (NodeInfoCollection c, NodeInfo parent, FieldInfo field)
		{
			Trace.WriteLineIf (info.Enabled, "Getting Field Children");
			AddMemberChildren (parent, field, c);
			AddMembers (parent, field, "Attributes", c);
			AddMembers (parent, field, "FieldHandle", c);
			AddMembers (parent, field, "FieldType", c);
			AddMembers (parent, field, "IsAssembly", c);
			AddMembers (parent, field, "IsFamily", c);
			AddMembers (parent, field, "IsFamilyAndAssembly", c);
			AddMembers (parent, field, "IsFamilyOrAssembly", c);
			AddMembers (parent, field, "IsInitOnly", c);
			AddMembers (parent, field, "IsLiteral", c);
			AddMembers (parent, field, "IsNotSerialized", c);
			AddMembers (parent, field, "IsPinvokeImpl", c);
			AddMembers (parent, field, "IsPublic", c);
			AddMembers (parent, field, "IsSpecialName", c);
			AddMembers (parent, field, "IsStatic", c);

			try {
				object o = field.GetValue (parent.ReflectionInstance);
				c.Add (new NodeInfo (parent, NodeTypes.ReturnValue, o.GetType(), o, o));
			}
			catch (Exception e) {
				string r = string.Format ("{{can't get field value; through exception: {0}}}", e.Message);
				c.Add (new NodeInfo (parent, NodeTypes.ReturnValue, null, null, r));
			}
		}

		private void AddMethodBaseChildren (NodeInfo parent, MethodBase mb, NodeInfoCollection c)
		{
			AddMemberChildren (parent, mb, c);
			AddMembers (parent, mb, "Attributes", c);
			AddMembers (parent, mb, "CallingConvention", c);
			AddMembers (parent, mb, "IsAbstract", c);
			AddMembers (parent, mb, "IsAssembly", c);
			AddMembers (parent, mb, "IsConstructor", c);
			AddMembers (parent, mb, "IsFamily", c);
			AddMembers (parent, mb, "IsFamilyAndAssembly", c);
			AddMembers (parent, mb, "IsFamilyOrAssembly", c);
			AddMembers (parent, mb, "IsFinal", c);
			AddMembers (parent, mb, "IsHideBySig", c);
			AddMembers (parent, mb, "IsPrivate", c);
			AddMembers (parent, mb, "IsPublic", c);
			AddMembers (parent, mb, "IsSpecialName", c);
			AddMembers (parent, mb, "IsStatic", c);
			AddMembers (parent, mb, "IsVirtual", c);
			AddMembers (parent, mb, "MethodHandle", c);
			AddMembers (parent, mb, "GetMethodImplementationFlags", c);
			c.Add (new NodeInfo (parent, 
						new NodeGroup (parameters, mb,
							new NodeGrouper (GetParametersChildren))));
			// c.Add (new NodeInfo (parent, mb, parameters, NodeTypes.Other));
		}

		private void GetParametersChildren (NodeInfoCollection c, NodeInfo parent, object args)
		{
			MethodBase mb = (MethodBase) args;
			foreach (ParameterInfo pi in mb.GetParameters()) {
				c.Add (new NodeInfo (parent, NodeTypes.Parameter, pi, pi));
			}
		}

		protected override void GetParameterChildren (NodeInfoCollection c, NodeInfo parent, ParameterInfo pi)
		{
			// TODO: handle custom attributes...
			// c.Add (new NodeInfo (parent, pi, customAttributes, NodeTypes.Other));
			// c.Add (new NodeInfo (parent, pi, pi, NodeTypes.CustomAttributeProvider));
			AddMembers (parent, pi, "Attributes", c);
			AddMembers (parent, pi, "DefaultValue", c);
			AddMembers (parent, pi, "IsIn", c);
			AddMembers (parent, pi, "IsLcid", c);
			AddMembers (parent, pi, "IsOptional", c);
			AddMembers (parent, pi, "IsOut", c);
			AddMembers (parent, pi, "IsRetval", c);
			AddMembers (parent, pi, "Member", c);
			AddMembers (parent, pi, "Name", c);
			AddMembers (parent, pi, "ParameterType", c);
			AddMembers (parent, pi, "Position", c);
		}

		protected override void GetConstructorChildren (NodeInfoCollection c, NodeInfo parent, ConstructorInfo node)
		{
			AddMethodBaseChildren (parent, node, c);
		}

		protected override void GetEventChildren (NodeInfoCollection c, NodeInfo parent, EventInfo node)
		{
			AddMemberChildren (parent, node, c);
			AddMembers (parent, node, "Attributes", c);
			AddMembers (parent, node, "EventHandlerType", c);
			AddMembers (parent, node, "IsMulticast", c);
			AddMembers (parent, node, "IsSpecialName", c);
		}

		protected override void GetMethodChildren (NodeInfoCollection c, NodeInfo parent, MethodInfo node)
		{
			AddMethodBaseChildren (parent, node, c);
			AddMembers (parent, node, "ReturnType", c);
			AddMembers (parent, node, "ReturnTypeCustomAttributes", c);
			GetReturnValue (c, parent, node);
		}

		private void GetReturnValue (NodeInfoCollection c, NodeInfo parent, MethodInfo method)
		{
			if (method != null && method.GetParameters().Length == 0) {
				try {
					object o = method.Invoke (parent.ReflectionInstance, null);
					c.Add (new NodeInfo (parent, NodeTypes.ReturnValue, o.GetType(), o, o));
				}
				catch (Exception e) {
					string r = string.Format ("{{method has 0 args; through exception: {0}}}", e.Message);
					c.Add (new NodeInfo (parent, NodeTypes.ReturnValue, null, null, r));
				}
			}
		}

		protected override void GetPropertyChildren (NodeInfoCollection c, NodeInfo parent, PropertyInfo node)
		{
			AddMemberChildren (parent, node, c);
			AddMembers (parent, node, "Attributes", c);
			AddMembers (parent, node, "CanRead", c);
			AddMembers (parent, node, "CanWrite", c);
			AddMembers (parent, node, "IsSpecialName", c);
			AddMembers (parent, node, "PropertyType", c);
			GetReturnValue (c, parent, node.GetGetMethod());
		}
	}
}

