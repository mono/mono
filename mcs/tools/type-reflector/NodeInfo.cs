//
// NodeInfo.cs: 
//
// A NodeInfo object represents a node in Type tree, such as a Constructor,
// Field, or pseudo-collections like GroupingNodeFinder's "Methods:" branch.
//
// Additionally, it's used to get additional information about the node, such
// as invoke methods to get additional information.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002-2003 Jonathan Pryor
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
	public class NodeInfo {
		private NodeInfo  	parent;
		private NodeTypes   type;
		private object      reflectionObject = null;
		private object      reflectionInstance = null;
		private object      description = null;

		public NodeInfo (NodeInfo parent, NodeTypes type, object reflectionObject, 
				object reflectionInstance, object description)
		{
			this.parent = parent;
			this.type = type;
			this.reflectionObject = reflectionObject;
			this.reflectionInstance = reflectionInstance;
			this.description = description;
		}

		public NodeInfo (NodeInfo parent, NodeTypes type, object reflectionObject, 
				object reflectionInstance)
			: this (parent, type, reflectionObject, reflectionInstance, null)
		{
		}

		public NodeInfo (NodeInfo parent, NodeTypes type)
			: this (parent, type, null, null, null)
		{
		}

		public NodeInfo (NodeInfo parent, object description)
			: this (parent, NodeTypes.Other, null, null, description)
		{
		}

		public NodeInfo (NodeInfo parent, MemberInfo mi)
			: this (parent, mi, null)
		{
		}

		public NodeInfo (NodeInfo parent, MemberInfo mi, object instance)
			: this (parent, NodeTypes.Other, mi, instance, null)
		{
			// set type
			switch (mi.MemberType) {
			case MemberTypes.NestedType:
			case MemberTypes.TypeInfo:
				this.type = NodeTypes.Type;
				break;
			case MemberTypes.Constructor:
				this.type = NodeTypes.Constructor;
				break;
			case MemberTypes.Event:
				this.type = NodeTypes.Event;
				break;
			case MemberTypes.Field:
				this.type = NodeTypes.Field;
				break;
			case MemberTypes.Method:
				this.type = NodeTypes.Method;
				break;
			case MemberTypes.Property:
				this.type = NodeTypes.Property;
				break;
			default:
				this.type = NodeTypes.Other;
				break;
			}
		}

		/// <summary>
		/// This is the type of the NodeInfo object.
		/// </summary>
		public NodeTypes NodeType {
			get {return type;}
			set {type = value;}
		}

		/// <summary>
		/// This is the parent NodeInfo object.  This is used to help represent
		/// the tree.
		/// </summary>
		public NodeInfo Parent {
			get {return parent;}
		}

		/// <summary>
		/// If non-null, this is the underlying System.Reflection object, such as
		/// System.Reflection.MemberInfo or System.Reflection.ParameterInfo.
		/// </summary>
		public object ReflectionObject {
			get {return reflectionObject;}
			set {reflectionObject = value;}
		}

		/// <summary>
		/// This is the object instance to pass to System.Reflection methods using
		/// the ReflectionObject.
		/// </summary>
		public object ReflectionInstance {
			get {return reflectionInstance;}
			set {reflectionInstance = value;}
		}

		/// <summary>
		/// This is an arbitrary description that can be attached to the node.
		/// This is only useful if ReflectionObject is null or NodeType is a value
		/// that doesn't match against a "real" System.Reflection type, such as
		/// NodeTypes.Other.
		/// </summary>
		public object Description {
			get {return description;}
			set {description = value;}
		}

		/*
		public override string ToString ()
		{
			Console.WriteLine ("** (NodeInfo (parent {0}) (type {1}) (rObj {2}) (rIns {3}) (description {4}))",
					parent, type, reflectionObject, reflectionInstance, description);
			return base.ToString ();
		}
		 */
	}

	public sealed class NodeInfoCollection : CollectionBase {

		internal NodeInfoCollection ()
		{
		}

		public NodeInfo this [int index] {
			get {return (NodeInfo) InnerList[index];}
			set {InnerList[index] = value;}
		}

		public int Add (NodeInfo value)
		{
			return InnerList.Add (value);
		}

		public void AddRange (NodeInfo[] values)
		{
			foreach (NodeInfo n in values)
				Add (n);
		}

		public void AddRange (NodeInfoCollection values)
		{
			foreach (NodeInfo n in values)
				Add (n);
		}
	}
}

