// AssemblyTreeLoader.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using Mono.Doc.Core;
using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	public class AssemblyTreeLoader
	{
		// this class cannot be instantiated
		private AssemblyTreeLoader()
		{
		}

		public static void LoadTree(TreeView tree, string fileName)
		{
			Assembly assem = AssemblyLoader.Load(fileName);

			tree.ImageList = AssemblyTreeImages.List;

			// create root element
			TreeNode root           = new TreeNode(assem.GetName().Name + " Assembly");
			root.ImageIndex         = AssemblyTreeImages.AssemblyClosed;
			root.SelectedImageIndex = AssemblyTreeImages.AssemblyClosed;

			tree.Nodes.Add(root);

			// dictionary of namespaces
			Hashtable namespaces = new Hashtable();

			foreach (Type t in assem.GetTypes())
			{
				// TODO: this is overly simple, and should be configurable
				if (t.IsPublic)
				{
					// namespace
					TreeNode nsNode = (TreeNode) namespaces[t.Namespace];

					if (nsNode == null)
					{
						nsNode                    = new TreeNode(t.Namespace);
						nsNode.ImageIndex         = AssemblyTreeImages.Namespace;
						nsNode.SelectedImageIndex = AssemblyTreeImages.Namespace;
						nsNode.Tag                = "N:" + t.Namespace;
						namespaces[t.Namespace]   = nsNode;
					}

					TreeNode typeNode = GetNodeForType(t);
					
					nsNode.Nodes.Add(typeNode);
				}
			}

			// add namespace nodes to root
			// TODO: sort by name
			foreach (TreeNode nsNode in namespaces.Values)
			{
				root.Nodes.Add(nsNode);
			}
			
			root.Expand();
		}

		private static TreeNode GetNodeForType(Type t)
		{
			string nodeName   = t.Name + " ";
			TreeNode typeNode = new TreeNode();
			typeNode.Tag      =
				TypeNameHelper.GetNameForMemberInfo(t, NamingFlags.TypeSpecifier | NamingFlags.FullName);

			if (t.IsClass) // TODO: delegates?
			{
				typeNode.ImageIndex         = AssemblyTreeImages.Class;
				typeNode.SelectedImageIndex = AssemblyTreeImages.Class;
				typeNode.Text               = nodeName + "Class";
				
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
			else 
			{
				// TODO: placeholder
				typeNode.Text = nodeName + "OTHER";
			}

			return typeNode;
		}

		private static TreeNode GetConstructorsNode(Type t)
		{
			TreeNode cNode           = new TreeNode("Constructors");
			cNode.ImageIndex         = AssemblyTreeImages.Constructor;
			cNode.SelectedImageIndex = AssemblyTreeImages.Constructor;

			// TODO: create nodes for constructors

			return cNode;
		}

		private static TreeNode GetFieldsNode(Type t)
		{
			TreeNode fNode = new TreeNode("Fields");
			fNode.ImageIndex         = AssemblyTreeImages.Field;
			fNode.SelectedImageIndex = AssemblyTreeImages.Field;

			// TODO: create nodes for fields

			return fNode;
		}

		private static TreeNode GetPropertiesNode(Type t)
		{
			TreeNode pNode = new TreeNode("Properties");
			pNode.ImageIndex         = AssemblyTreeImages.Property;
			pNode.SelectedImageIndex = AssemblyTreeImages.Property;

			// TODO: create nodes for properties

			return pNode;
		}

		private static TreeNode GetMethodsNode(Type t)
		{
			TreeNode mNode = new TreeNode("Methods");
			mNode.ImageIndex         = AssemblyTreeImages.Method;
			mNode.SelectedImageIndex = AssemblyTreeImages.Method;

			// TODO: create nodes for methods

			return mNode;
		}

		private static TreeNode GetOperatorsNode(Type t)
		{
			TreeNode oNode = new TreeNode("Operators");
			oNode.ImageIndex         = AssemblyTreeImages.Operator;
			oNode.SelectedImageIndex = AssemblyTreeImages.Operator;

			// TODO: create nodes for operators

			return oNode;
		}

		private static TreeNode GetEventsNode(Type t)
		{
			TreeNode eNode = new TreeNode("Events");
			eNode.ImageIndex         = AssemblyTreeImages.Event;
			eNode.SelectedImageIndex = AssemblyTreeImages.Event;

			// TODO: create nodes for properties

			return eNode;
		}
	}
}
