// AssemblyTreeLoader.cs
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
//
// Much of the reflection guts of this class come from the original Monodoc
// XML stub generator, by Adam Treat, and much of the guts of *that* were
// derived from NDoc (http://ndoc.sourceforge.net).

using Mono.Doc.Core;
using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	public class AssemblyTreeLoader
	{
		#region Instance Fields

		private AssemblyLoader loader;

		#endregion // Instance Fields

		#region Constructors and Destructors

		public AssemblyTreeLoader(AssemblyLoader loader)
		{
			this.loader = loader;
		}

		#endregion // Constructors and Destructors

		#region Private Instance Methods

		private TreeNode GetNodeForType(Type t)
		{
			string nodeName   =  GetTypeDisplayName(t) + " ";
			TreeNode typeNode = new TreeNode();
			typeNode.Tag      =
				TypeNameHelper.GetName(t, NamingFlags.TypeSpecifier | NamingFlags.FullName);

			if (t.IsClass && !IsDelegate(t))
			{
				// Type is class
				typeNode.ImageIndex         = AssemblyTreeImages.Class;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Class;
				typeNode.Text               = nodeName + "Class"; // TODO: i18n
				
				// Constructors, Fields, Properties, Methods, Operators, Events
				TreeNode[] categories = new TreeNode[6];
				categories[0]         = GetConstructorsNode(t);
				categories[1]         = GetFieldsNode(t);
				categories[2]         = GetPropertiesNode(t);
				categories[3]         = GetMethodsNode(t);
				categories[4]         = GetOperatorsNode(t);
				categories[5]         = GetEventsNode(t);

				foreach (TreeNode cat in categories)
				{
					if (cat != null) 
					{
						typeNode.Nodes.Add(cat);
					}
				}
			}
			else if (t.IsInterface)
			{
				// Type is interface
				typeNode.ImageIndex         = AssemblyTreeImages.Interface;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Interface;
				typeNode.Text               = nodeName + "Interface"; // TODO: i18n

				// Methods, properties, events
				TreeNode methodsNode = GetMethodsNode(t);
				TreeNode propsNode   = GetPropertiesNode(t);
				TreeNode eventsNode  = GetEventsNode(t);

				if (methodsNode != null)
				{
					typeNode.Nodes.Add(methodsNode);
				}

				if (propsNode != null)
				{
					typeNode.Nodes.Add(propsNode);
				}

				if (eventsNode != null)
				{
					typeNode.Nodes.Add(eventsNode);
				}
			}
			else if (t.IsValueType && !t.IsEnum)
			{
				// Type is struct
				typeNode.ImageIndex         = AssemblyTreeImages.Struct;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Struct;
				typeNode.Text               = nodeName + "Structure"; // TODO: i18n

				// Constructors, Fields, Properties, Methods, Operators, Events
				TreeNode[] categories = new TreeNode[6];
				categories[0]         = GetConstructorsNode(t);
				categories[1]         = GetFieldsNode(t);
				categories[2]         = GetPropertiesNode(t);
				categories[3]         = GetMethodsNode(t);
				categories[4]         = GetOperatorsNode(t);
				categories[5]         = GetEventsNode(t);

				foreach (TreeNode cat in categories)
				{
					if (cat != null) 
					{
						typeNode.Nodes.Add(cat);
					}
				}
			}
			else if (t.IsEnum)
			{
				// Type is enumeration
				typeNode.ImageIndex         = AssemblyTreeImages.Enum;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Enum;
				typeNode.Text               = nodeName + "Enumeration"; // TODO: i18n
			}
			else if (t.IsClass && IsDelegate(t))
			{
				// Type is delegate
				typeNode.ImageIndex         = AssemblyTreeImages.Delegate;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Delegate;
				typeNode.Text               = nodeName + "Delegate"; // TODO: i18n
			}
			else 
			{
				MessageBox.Show(
					"Encountered unexpected type during tree load: " + t.FullName,
					"What the heck is this?"
					);
			}

			return typeNode;
		}

		private TreeNode GetConstructorsNode(Type t)
		{
			ConstructorInfo[] ctors = loader.GetConstructors(t);

			if (ctors.Length > 0)
			{
				TreeNode cNode           = new TreeNode("Constructors");
				cNode.ImageIndex         = AssemblyTreeImages.Constructor;
				cNode.SelectedImageIndex = AssemblyTreeImages.Constructor;

				foreach (ConstructorInfo ctor in ctors)
				{
					TreeNode ctorNode           = new TreeNode();
					ctorNode.ImageIndex         = AssemblyTreeImages.Constructor;
					ctorNode.SelectedImageIndex = AssemblyTreeImages.Constructor;
					ctorNode.Tag                = GetTag(ctor);
					ctorNode.Text               = TypeNameHelper.GetName(ctor,
						NamingFlags.ForceMethodParams | NamingFlags.ShortParamTypes).
							Replace("#ctor", t.Name + " Constructor ").
								Replace(",", ", ");

					cNode.Nodes.Add(ctorNode);
				}

				return cNode;
			}
			else
			{
				return null;
			}
		}

		private TreeNode GetFieldsNode(Type t)
		{
			FieldInfo[] fields = loader.GetFields(t);

			if (fields.Length > 0)
			{
				TreeNode fNode           = new TreeNode("Fields");
				fNode.ImageIndex         = AssemblyTreeImages.Field;
				fNode.SelectedImageIndex = AssemblyTreeImages.Field;

				foreach (FieldInfo field in fields)
				{
					TreeNode fieldNode           = new TreeNode();
					fieldNode.ImageIndex         = AssemblyTreeImages.Field;
					fieldNode.SelectedImageIndex = AssemblyTreeImages.Field;
					fieldNode.Tag                = GetTag(field);
					fieldNode.Text               = field.Name + " Field";

					fNode.Nodes.Add(fieldNode);
				}

				return fNode;
			}
			else
			{
				return null;
			}
		}

		private TreeNode GetPropertiesNode(Type t)
		{
			PropertyInfo[] properties = loader.GetProperties(t);

			if (properties.Length > 0)
			{
				TreeNode pNode           = new TreeNode("Properties");
				pNode.ImageIndex         = AssemblyTreeImages.Property;
				pNode.SelectedImageIndex = AssemblyTreeImages.Property;
				
				foreach (PropertyInfo property in properties)
				{
					TreeNode propertyNode           = new TreeNode();
					propertyNode.ImageIndex         = AssemblyTreeImages.Property;
					propertyNode.SelectedImageIndex = AssemblyTreeImages.Property;
					propertyNode.Tag                = GetTag(property);
					propertyNode.Text               = property.Name + " Property";

					pNode.Nodes.Add(propertyNode);
				}

				return pNode;
			}
			else
			{
				return null;
			}
		}

		private TreeNode GetMethodsNode(Type t)
		{
			MethodInfo[] methods = loader.GetMethods(t);

			if (methods.Length > 0)
			{
				TreeNode mNode           = new TreeNode("Methods");
				mNode.ImageIndex         = AssemblyTreeImages.Method;
				mNode.SelectedImageIndex = AssemblyTreeImages.Method;
				Hashtable overloads      = new Hashtable();

				// group method overloads by name
				foreach (MethodInfo method in methods)
				{
					ArrayList overloadList = overloads[method.Name] as ArrayList;

					if (overloadList == null)
					{
						overloadList           = new ArrayList();
						overloads[method.Name] = overloadList;
						
						overloadList.Add(method);
					}
					else
					{
						overloadList.Add(method);
					}
				}

				// create nodes, grouping overloaded methods
				foreach (string methodName in overloads.Keys)
				{
					ArrayList overloadList = overloads[methodName] as ArrayList;

					if (overloadList.Count > 1)
					{
						// overloaded method
						TreeNode overloadNode = new TreeNode();
						overloadNode.ImageIndex = AssemblyTreeImages.Method;
						overloadNode.SelectedImageIndex = AssemblyTreeImages.Method;
						overloadNode.Text = methodName + " Method";
						overloadNode.Tag = "TODO:OVERLOAD";

						foreach (MethodInfo m in overloadList)
						{
							overloadNode.Nodes.Add(GetOverloadedMethodNode(m));
						}

						mNode.Nodes.Add(overloadNode);
					}
					else
					{
						// not overloaded
						MethodInfo m                  = overloadList[0] as MethodInfo;
						TreeNode methodNode           = new TreeNode();
						methodNode.ImageIndex         = AssemblyTreeImages.Method;
						methodNode.SelectedImageIndex = AssemblyTreeImages.Method;
						methodNode.Tag                = GetTag(m);
						methodNode.Text               = m.Name +
							TypeNameHelper.GetName(m, NamingFlags.HideMethodParams).
							Replace(m.Name, " ") + " Method";

						mNode.Nodes.Add(methodNode);
					}
				}

				return mNode;
			}
			else
			{
				return null;
			}
		}

		private TreeNode GetOverloadedMethodNode(MethodInfo m)
		{
			TreeNode mNode = new TreeNode();
			mNode.Tag      = GetTag(m);
			mNode.Text     = m.Name + TypeNameHelper.GetName(m,
				NamingFlags.ForceMethodParams | NamingFlags.ShortParamTypes).
				Replace(",", ", ").Replace(m.Name, " ") + " Method";

			return mNode;
		}

		private TreeNode GetOperatorsNode(Type t)
		{
			MethodInfo[] operatorMethods = loader.GetOperators(t);
			
			if (operatorMethods.Length > 0)
			{
				TreeNode oNode = new TreeNode("Operators");
				oNode.ImageIndex         = AssemblyTreeImages.Operator;
				oNode.SelectedImageIndex = AssemblyTreeImages.Operator;

				foreach (MethodInfo om in operatorMethods)
				{
					TreeNode omNode           = new TreeNode();
					omNode.ImageIndex         = AssemblyTreeImages.Operator;
					omNode.SelectedImageIndex = AssemblyTreeImages.Operator;
					omNode.Tag                = GetTag(om);
					omNode.Text               = om.Name.Replace("op_", "") + " Operator";

					oNode.Nodes.Add(omNode);
				}

				return oNode;
			}
			else
			{
				return null;
			}
		}

		private TreeNode GetEventsNode(Type t)
		{
			EventInfo[] events = loader.GetEvents(t);

			if (events.Length > 0)
			{
				TreeNode eNode           = new TreeNode("Events");
				eNode.ImageIndex         = AssemblyTreeImages.Event;
				eNode.SelectedImageIndex = AssemblyTreeImages.Event;

				foreach (EventInfo e in events)
				{
					TreeNode eiNode          = new TreeNode();
					eiNode.ImageIndex         = AssemblyTreeImages.Event;
					eiNode.SelectedImageIndex = AssemblyTreeImages.Event;
					eiNode.Tag                = GetTag(e);
					eiNode.Text               = e.Name + " Event";

					eNode.Nodes.Add(eiNode);
				}

				return eNode;
			}
			else
			{
				return null;
			}
		}

		private bool IsDelegate(Type type)
		{
			return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
		}

		private string GetTypeDisplayName(Type type)
		{
			string[] s  = type.FullName.Split(new char[] {'.'});
			string name = s[s.Length - 1];

			return name.Replace('+', '.');
		}

		private string GetTag(MemberInfo m)
		{
			return TypeNameHelper.GetName(m, NamingFlags.TypeSpecifier | NamingFlags.FullName);
		}

		#endregion // Private Instance Methods

		#region Public Instance Methods

		public void LoadNode(TreeNode node)
		{
			// create root element
			TreeNode root           = new TreeNode(loader.Assembly.GetName().Name + " Assembly");
			root.ImageIndex         = AssemblyTreeImages.AssemblyOpen;
			root.SelectedImageIndex = AssemblyTreeImages.AssemblyOpen;
			root.Tag                = "A:" + loader.Assembly.GetName().Name;

			node.Nodes.Add(root);

			// dictionary of namespaces
			Hashtable namespaces = new Hashtable();

			foreach (Type t in loader.GetTypes())
			{
				TreeNode nsNode = (TreeNode) namespaces[t.Namespace];

				if (nsNode == null)
				{
					nsNode                    = new TreeNode(t.Namespace);
					nsNode.ImageIndex         = AssemblyTreeImages.Namespace;
					nsNode.SelectedImageIndex = AssemblyTreeImages.Namespace;
					nsNode.Tag                = "N:" + t.Namespace;
					namespaces[t.Namespace]   = nsNode;

				}

				// TODO: sort types before adding
				TreeNode typeNode = GetNodeForType(t);
					
				nsNode.Nodes.Add(typeNode);
			}
			

			// add sorted namespace nodes to root
			foreach (string nsName in new SortedStringValues(namespaces.Keys))
			{
				root.Nodes.Add((TreeNode) namespaces[nsName]);
			}
		}

		#endregion // Public Instance Methods
	}
}
