//
// GroupingNodeFinder.cs: Groups nodes into categories, e.g. fields, methods...
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Mono.TypeReflector
{
	/**
	 * GroupingNode is a decorator over INodeFinder that groups child nodes of
	 * similar type in the resulting tree.
	 */
	public class GroupingNodeFinder : INodeFinder {

		private static BooleanSwitch info = 
			new BooleanSwitch ("grouping-node-finder", "GroupingNodeFinder messages");

		private sealed class GroupingInfo {
			public string Group;
			public NodeInfo Info;

			public GroupingInfo (string g, NodeInfo i)
			{
				Group = g;
				Info = i;
			}

			public override string ToString ()
			{
				return Group;
			}
		}

		private const string baseNode = "Base:";
		private const string interfacesNode = "Interfaces:";
		private const string constructorsNode = "Constructors:";
		private const string methodsNode = "Methods:";
		private const string fieldsNode = "Fields:";
		private const string propertiesNode = "Properties:";
		private const string eventsNode = "Events:";
		private const string nestedClasses = "Nested Classes:";

		private IDictionary nodes = null;

		private INodeFinder finder;

		public GroupingNodeFinder (INodeFinder finder)
		{
			this.finder = finder;
		}

		public NodeInfoCollection GetChildren (NodeInfo root)
		{
			Trace.WriteLineIf (info.Enabled, "GroupingNodeFinder.GetChildren");
			NodeInfoCollection collection = null;

			switch (root.NodeType) {
			case NodeTypes.Type:
				GroupChildNodes (root);
				collection = new NodeInfoCollection ();
				collection.AddRange ((NodeInfoCollection)nodes[""]);
				AddGroup (nestedClasses, collection, root);
				AddGroup (baseNode, collection, root);
				AddGroup (interfacesNode, collection, root);
				AddGroup (constructorsNode, collection, root);
				AddGroup (methodsNode, collection, root);
				AddGroup (fieldsNode, collection, root);
				AddGroup (propertiesNode, collection, root);
				AddGroup (eventsNode, collection, root);
				return collection;
			case NodeTypes.Other:
				if (root.Description is GroupingInfo) {
					Trace.WriteLineIf (info.Enabled, "Found GroupingInfo");
					GroupingInfo g = (GroupingInfo) root.Description;
					collection = (NodeInfoCollection) nodes[g.Group];
					return collection;
				}
				break;
			}

			return finder.GetChildren (root);
		}

		private void GroupChildNodes (NodeInfo root)
		{
			if (nodes != null)
				return;

			nodes = new Hashtable ();
			nodes[""] = new NodeInfoCollection();
			nodes[baseNode] = new NodeInfoCollection();
			nodes[interfacesNode] = new NodeInfoCollection();
			nodes[constructorsNode] = new NodeInfoCollection();
			nodes[methodsNode] = new NodeInfoCollection();
			nodes[fieldsNode] = new NodeInfoCollection();
			nodes[propertiesNode] = new NodeInfoCollection();
			nodes[eventsNode] = new NodeInfoCollection();
			nodes[nestedClasses] = new NodeInfoCollection();

			foreach (NodeInfo n in finder.GetChildren (root)) {
				string c = null;
				switch (n.NodeType) {
					case NodeTypes.BaseType:
						c = baseNode;
						break;
					case NodeTypes.Interface:
						c = interfacesNode;
						break;
					case NodeTypes.Field:
						c = fieldsNode;
						break;
					case NodeTypes.Constructor:
						c = constructorsNode;
						break;
					case NodeTypes.Method:
						c = methodsNode;
						break;
					case NodeTypes.Property:
						c = propertiesNode;
						break;
					case NodeTypes.Event:
						c = eventsNode;
						break;
					case NodeTypes.Type:
						c = nestedClasses;
						break;
					default:
						c = "";
						break;
				}
				((NodeInfoCollection)nodes[c]).Add (n);
			}
		}

		private void AddGroup (string name, NodeInfoCollection collection, NodeInfo root)
		{
			NodeInfoCollection nic = (NodeInfoCollection) nodes[name];
			if (nic.Count > 0)
				collection.Add (CreateGroupingNode (name, root));
		}

		private static NodeInfo CreateGroupingNode (string group, NodeInfo node)
		{
			return new NodeInfo (node, new GroupingInfo (group, node));
		}
	}
}

