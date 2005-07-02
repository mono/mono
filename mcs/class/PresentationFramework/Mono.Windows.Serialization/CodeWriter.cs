//
// CodeWriter.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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
using System.IO;
using System.Collections;

namespace Mono.Windows.Serialization {
	public class CodeWriter : XamlWriter {
		TextWriter writer;
		ArrayList objects = new ArrayList();
		Hashtable nameClashes = new Hashtable();
		int tempIndex = 0;
		
		public CodeWriter(TextWriter writer)
		{
			this.writer = writer;
		}
		
		public void CreateTopLevel(string parentName, string className)
		{
			Type parent = Type.GetType(parentName);
			writer.WriteLine(className + " extends " + 
					parent.Namespace + "." + parent.Name);	
			objects.Add("this");
		}
	
		public void CreateObject(string typeName)
		{
			Type type = Type.GetType(typeName);
			string varName = Char.ToLower(type.Name[0]) + type.Name.Substring(1);
			// make sure something sensible happens when class
			// names start with a lowercase letter
			if (varName == type.Name)
				varName = "_" + varName;

			if (!nameClashes.ContainsKey(varName))
				nameClashes[varName] = 0;
			else {
				nameClashes[varName] = 1 + (int)nameClashes[varName];
				varName += (int)nameClashes[varName];
			}
			
			writer.WriteLine(type.Name + " " + varName);
			writer.WriteLine(objects[objects.Count - 1] + ".AddChild(" + varName + ")");
			objects.Add(varName);
		}

		public void CreateProperty(string propertyName)
		{
			string varName = (string)objects[objects.Count - 1];
			string prop = varName + "." + propertyName;
			objects.Add(prop);
		}

		public void CreateAttachedProperty(string attachedTo, string propertyName, string typeName)
		{
			Type t = Type.GetType(typeName);
			objects.Add(attachedTo);
			objects.Add(propertyName);

			string name = "temp";
			if (tempIndex != 0)
				name += tempIndex;
			writer.WriteLine(t.Name + " " + name);
			objects.Add(name);
		}

		public void EndAttachedProperty()
		{
			string varName = (string)(objects[objects.Count - 1]);
			string propertyName = (string)(objects[objects.Count - 2]);
			string attachedTo = (string)(objects[objects.Count - 3]);
			objects.RemoveAt(objects.Count - 1);
			objects.RemoveAt(objects.Count - 1);
			objects.RemoveAt(objects.Count - 1);
			writer.WriteLine(attachedTo + ".Set" + propertyName + "(" + objects[objects.Count - 1] + ", " + varName + ");");
		}

		public void CreateElementText(string text)
		{
			writer.WriteLine(objects[objects.Count - 1] + ".AddText(\"" + text +"\")");
		}

		public void CreatePropertyText(string text)
		{
			writer.WriteLine(objects[objects.Count - 1] + " = " +
					"\"" + text + "\"");
		}
		public void CreateAttachedPropertyText(string text)
		{
			writer.WriteLine(objects[objects.Count - 1] + " = " + text);
		}
		
		public void EndObject()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void EndProperty()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void Finish()
		{
			writer.Close();
		}
	}
}
