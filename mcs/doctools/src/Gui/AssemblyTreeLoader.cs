// AssemblyTreeLoader.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
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
			// FIXME: this is a property of the current project
			Assembly assem = AssemblyLoader.Load(fileName);

			tree.ImageList = AssemblyTreeImages.List;

			tree.BeginUpdate();
			Cursor.Current = Cursors.WaitCursor;

			// create root element
			TreeNode root           = new TreeNode(assem.GetName().Name);
			root.ImageIndex         = AssemblyTreeImages.AssemblyClosed;
			root.SelectedImageIndex = AssemblyTreeImages.AssemblyClosed;

			tree.Nodes.Add(root);

			// dictionary of namespaces
			Hashtable namespaces = new Hashtable();

			foreach (Type t in assem.GetTypes())
			{
				// FIXME: this is overly simple, and should be configurable
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
			Cursor.Current = Cursors.Default;
			tree.EndUpdate();
		}

		private static TreeNode GetNodeForType(Type t)
		{
			string nodeName   = t.Name + " ";
			TreeNode typeNode = new TreeNode();
			typeNode.Tag      =
				TypeNameHelper.GetNameForMemberInfo(t, NamingFlags.TypeSpecifier | NamingFlags.FullName);

			if (t.IsClass) // FIXME: delegates?
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
				// FIXME: placeholder
				typeNode.Text = nodeName + "OTHER";
			}

			return typeNode;
		}

		private static TreeNode GetConstructorsNode(Type t)
		{
			TreeNode cNode           = new TreeNode("Constructors");
			cNode.ImageIndex         = AssemblyTreeImages.Constructor;
			cNode.SelectedImageIndex = AssemblyTreeImages.Constructor;

			// FIXME: create nodes for constructors

			return cNode;
		}

		private static TreeNode GetFieldsNode(Type t)
		{
			TreeNode fNode = new TreeNode("Fields");
			fNode.ImageIndex         = AssemblyTreeImages.Field;
			fNode.SelectedImageIndex = AssemblyTreeImages.Field;

			// FIXME: create nodes for fields

			return fNode;
		}

		private static TreeNode GetPropertiesNode(Type t)
		{
			TreeNode pNode = new TreeNode("Properties");
			pNode.ImageIndex         = AssemblyTreeImages.Property;
			pNode.SelectedImageIndex = AssemblyTreeImages.Property;

			// FIXME: create nodes for properties

			return pNode;
		}

		private static TreeNode GetMethodsNode(Type t)
		{
			TreeNode mNode = new TreeNode("Methods");
			mNode.ImageIndex         = AssemblyTreeImages.Method;
			mNode.SelectedImageIndex = AssemblyTreeImages.Method;

			// FIXME: create nodes for methods

			return mNode;
		}

		private static TreeNode GetOperatorsNode(Type t)
		{
			TreeNode oNode = new TreeNode("Operators");
			oNode.ImageIndex         = AssemblyTreeImages.Operator;
			oNode.SelectedImageIndex = AssemblyTreeImages.Operator;

			// FIXME: create nodes for operators

			return oNode;
		}

		private static TreeNode GetEventsNode(Type t)
		{
			TreeNode eNode = new TreeNode("Events");
			eNode.ImageIndex         = AssemblyTreeImages.Event;
			eNode.SelectedImageIndex = AssemblyTreeImages.Event;

			// FIXME: create nodes for properties

			return eNode;
		}
	}
}
