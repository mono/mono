//
// ReflectionNodeFinder.cs: Uses reflection to find child nodes.
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

namespace Mono.TypeReflector.Finders
{
	public class ReflectionNodeFinder : NodeFinder {

		private class Observed
		{
			private ArrayList seen = new ArrayList ();
			public void Add (object value)
			{
				seen.Add (value);
				// seen.Sort ();
			}

			public bool Found (object value)
			{
				return seen.Contains (value);
				// return seen.BinarySearch (value) != -1;
			}
		}

		private Observed seenReflectionObjects = new Observed ();

		private sealed class MemberInfoNameComparer : IComparer {

			public int Compare (object a, object b)
			{
				if (a == null && b == null)
					return 0;

				MemberInfo x = a as MemberInfo;
				MemberInfo y = b as MemberInfo;
				if (a == null && b == null)
					throw new ArgumentException ();
				return x.Name.CompareTo (y.Name);
			}
		}

		private static readonly IComparer NameComparer = new MemberInfoNameComparer ();

		protected override void AddTypeChildren (NodeInfoCollection c, NodeInfo parent, Type type)
		{
			object instance = parent.ReflectionInstance;

			foreach (MemberInfo mi in GetMembers (type)) {
				AddNode (c, parent, mi, instance);
			}
		}

		private IList GetMembers (Type type)
		{
			ArrayList members = new ArrayList ();
			members.AddRange (type.GetMembers (BindingFlags));
			members.Sort (NameComparer);
			return members;
		}

		private void AddNode (NodeInfoCollection c, NodeInfo parent, MemberInfo mi, object instance)
		{
			// FIXME: there has to be a cleaner way than this...
			// Don't add node if we don't want to display them.
			bool quit = false;
			switch (mi.MemberType) {
				case MemberTypes.Constructor:
					quit = !ShowConstructors;
					break;
				case MemberTypes.Event:
					quit = !ShowEvents;
					break;
				case MemberTypes.Field:
					quit = !ShowFields;
					break;
				case MemberTypes.Method:
					quit = !ShowMethods;
					break;
				case MemberTypes.Property:
					quit = !ShowProperties;
					break;
				case MemberTypes.TypeInfo:
					// either a Base type or an Interface
					// this is bound to produce buggy behavior...
					quit = !ShowBase && !ShowInterfaces;
					break;

				// case MemberTypes.NestedType
				// we don't break out nested types yet
			}

			if (quit)
				return;

			if (!seenReflectionObjects.Found (mi)) {
				seenReflectionObjects.Add (mi);
				NodeInfo n = new NodeInfo (parent, mi, instance);
				c.Add (n);
			}
			else {
				NodeInfo n = new NodeInfo (parent, "Seen: " + mi.ToString());
				n.NodeType = NodeTypes.Alias;
				c.Add (n);
			}
		}

		private void AddSubnodes (NodeInfoCollection c, NodeInfo parent, Type type, object instance)
		{
			foreach (MemberInfo mi in GetMembers (type)) {
				AddNode (c, parent, mi, instance);
			}
		}

		protected override void AddFieldChildren (NodeInfoCollection c, NodeInfo parent, FieldInfo field)
		{
			AddSubnodes (c, parent, field.GetType(), field);
		}

		protected override void AddConstructorChildren (NodeInfoCollection c, NodeInfo parent, ConstructorInfo ctor)
		{
			GetMethodBaseChildren (c, parent, ctor);
		}

		protected override void AddMethodChildren (NodeInfoCollection c, NodeInfo parent, MethodInfo method)
		{
			GetMethodBaseChildren (c, parent, method);
		}

		private void GetMethodBaseChildren (NodeInfoCollection c, NodeInfo parent, MethodBase mb)
		{
			AddSubnodes (c, parent, mb.GetType(), mb);
		}

		protected override void AddParameterChildren (NodeInfoCollection c, NodeInfo parent, ParameterInfo param)
		{
			AddSubnodes (c, parent, param.GetType(), param);
		}

		protected override void AddPropertyChildren (NodeInfoCollection c, NodeInfo parent, PropertyInfo pi)
		{
			AddSubnodes (c, parent, pi.GetType(), pi);
		}

		protected override void AddEventChildren (NodeInfoCollection c, NodeInfo parent, EventInfo e)
		{
			AddSubnodes (c, parent, e.GetType(), e);
		}

		protected override void AddReturnValueChildren (NodeInfoCollection c, NodeInfo parent)
		{
			if (parent.ReflectionObject != null)
				AddTypeChildren (c, parent, (Type) parent.ReflectionObject);
		}
	}
}

