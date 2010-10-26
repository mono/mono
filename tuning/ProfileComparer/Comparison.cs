//
// Comparison.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Text;

namespace GuiCompare {

	public enum ComparisonStatus {
		None,
		Missing,
		Extra,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (CompType type, string displayName)
		: this (type, displayName, null)
		{
		}
		
		public ComparisonNode (CompType type, string displayName, string typeName)
		: this (type, displayName, typeName, null)
		{
		}

		public ComparisonNode (CompType type, string displayName, string typeName, string extraInfo)
		{
			Type = type;
			Name = displayName;
			TypeName = typeName;
			ExtraInfo = extraInfo;
			Children = new List<ComparisonNode> ();
			Messages = new List<string> ();
			Todos = new List<string> ();
		}

		public void AddChild (ComparisonNode node)
		{
			Children.Add (node);
			node.Parent = this;
		}

		public void PropagateCounts ()
		{
			Todo = Todos.Count;
			Niex = ThrowsNIE ? 1 : 0;
			foreach (ComparisonNode n in Children) {
				n.PropagateCounts ();
				Extra += n.Extra + (n.Status == ComparisonStatus.Extra ? 1 : 0);
				Missing += n.Missing + (n.Status == ComparisonStatus.Missing ? 1 : 0);
				Present += n.Present; // XXX
				Todo += n.Todo;
				Niex += n.Niex;
				Warning += n.Warning + (n.Status == ComparisonStatus.Error ? 1 : 0);
			}
		}

		public void ResetCounts ()
		{
			foreach (ComparisonNode n in Children)
				n.ResetCounts ();

			Todo = 0;
			Niex = 0;
			Extra = 0;
			Missing = 0;
			Present = 0;
			Warning = 0;
		}

		public void AddError (string msg)
		{
			Status = ComparisonStatus.Error;
			Messages.Add (msg);
		}

		// TODO: detect user's locale and reflect that in the url
		public string MSDNUrl {
			get {
				if (msdnUrl != null)
					return msdnUrl;
				
				if (String.IsNullOrEmpty (TypeName)) {
					msdnUrl = ConstructMSDNUrl ();
					return msdnUrl;
				}
				
				if (msdnUrl == null)
					msdnUrl = MSDN_BASE_URL + TypeName.ToLower () + ".aspx";

				return msdnUrl;
			}
		}

		string FormatMyName ()
		{
			if (Parent==null) return Name;
			return Parent.Name + " :: " +Name;
			string name = Name;
			int start = name.IndexOf (' ') + 1;
			int end = name.IndexOf ('(');

			switch (Type) {
				case CompType.Method:
				case CompType.Attribute:
				case CompType.Delegate:
					return Parent.FormatMyName()+name;
					break;

				default:
					if (end < 0)
						end = name.Length;
					break;
			}
			int len = end - start;

			if (len <= 0)
				return name;
			
			return name.Substring (start, end - start).Trim ();
		}
		
		string ConstructMSDNUrl ()
		{
			ComparisonNode n = Parent;
			List <string> segments = new List <string> ();
			string name = FormatMyName ();			

			if (Type == CompType.Method && Parent.Type == CompType.Property && (name.StartsWith ("get_") || name.StartsWith ("set_")))
				return null;
			else if (Type == CompType.Method && Parent.Type == CompType.Event && (name.StartsWith ("add_") || name.StartsWith ("remove_")))
				return null;

			segments.Add ("aspx");
			segments.Insert (0, name.ToLower ());
			n = Parent;
			while (n != null) {
				name = n.Name.ToLower ();
				if (name.EndsWith (".dll"))
					break;
				
				segments.Insert (0, n.Name.ToLower ());
				n = n.Parent;
			}

			string[] path = segments.ToArray ();
			return MSDN_BASE_URL + String.Join (".", path);
		}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			//if (Status != ComparisonStatus.None)
			if (!IsInteresting(this)) return "";
			sb.AppendLine("<li>Node: "+FormatMyName()+" Status: "+Status+"<br>");
			if (Messages.Count>0)
			{	
				sb.AppendLine("Messages: <br><ul>");
				foreach(var msg in Messages)
					sb.AppendLine("<li>"+msg+"</li>");
				sb.AppendLine("</ul>");
			}
			if (Children.Count>0 && Status != ComparisonStatus.Missing)
			{
				sb.AppendLine("Children: <ul>");
				foreach(var child in Children)
					sb.Append(child.ToString());
				sb.Append("</ul>");
			}
			sb.Append("</li>");
			return sb.ToString();
		}
		public List<string> GetMissingTypesAndMethods()
		{
			Console.WriteLine("GMTM called on; "+FormatMyName()+" with status: "+Status+" and type: "+Type);
			var result = new List<string>();
			if (Status == ComparisonStatus.None)
			{
				foreach(var child in Children)
					result.AddRange(child.GetMissingTypesAndMethods());
				return result;
			}
			bool care = 	Type == CompType.Class ||
					Type == CompType.Interface ||
					Type == CompType.Method ||
					Type == CompType.Event ||
					Type == CompType.Property;

			if (Status == ComparisonStatus.Missing && care)
			{
				Console.WriteLine("registering as missing: "+FormatMyName());
				result.Add(FormatMyName());
			}
			return result;
		}

		bool IsInteresting(ComparisonNode node)
		{
			if (node.Status != ComparisonStatus.None) return true;
			foreach(var child in node.Children)
				if (IsInteresting(child)) return true;
			return false;			
		}


		public ComparisonStatus Status;
		public readonly CompType Type;

		public ComparisonNode Parent;

		public readonly string Name;
		public readonly string TypeName;
		public readonly string ExtraInfo;
		public readonly List<string> Messages;
		public readonly List<string> Todos;
		public bool ThrowsNIE;
		
		public int Extra;
		public int Missing;
		public int Present;
		public int Warning;
		public int Todo;
		public int Niex;

		public readonly List<ComparisonNode> Children;
		public bool HasChildren; // This is set when lazy-loading from the DB
		public bool HasMessages; // This is set when lazy-loading from the DB
		public object InternalID;

		string msdnUrl;

		const string MSDN_BASE_URL = "http://msdn.microsoft.com/en-us/library/";
	}
}
