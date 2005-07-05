//
// Mapping.cs
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
using System.Reflection;
using System.Collections;

namespace Mono.Windows.Serialization {

	class Mapper {
		private Hashtable mappings = new Hashtable();
		public void AddMapping(Mapping mapping)
		{
			if (mappings.ContainsKey(mapping.XmlNamespace))
				{}// TODO: throw exception

			mappings[mapping.XmlNamespace] = mapping;
		}

		public Type Resolve(string clrNamespace, string classname)
		{
			return ((Mapping)mappings[clrNamespace]).Resolve(classname);
		}
	}

	class Mapping {
		string xmlNamespace;
		string clrNamespace;
		string assemblyName;

		// this function takes the processing instructions value, which
		// should be something like: 
		//   Assembly="Foo.dll" ClrNamespace="Foo" XmlNamespace="foo"
		public Mapping(string instruction)
		{
			string name = "", value = "";
			int i = 0;
			instruction = instruction.Trim();
			while (i != instruction.Length) {
				name = "";
				value = "";
				while (Char.IsWhiteSpace(instruction[i]))
					i++;
				while (instruction[i] != '=') {
					name += instruction[i];
					i++;
				}
				i++; // go past the = sign
				if (instruction[i] == '"')
					i++; // go past the "
				//TODO: else and exception
				while (instruction[i] != '"') {
					value += instruction[i];
					i++;
				}
				if (instruction[i] == '"')
					i++; // go past another "

				switch(name) {
				case "ClrNamespace":
					if (clrNamespace == null)
						clrNamespace = value;
					// TODO: else and exception
					break;
				case "Assembly":
					if (assemblyName == null)
						assemblyName = value;
					// TODO: else and exception
					break;
				case "XmlNamespace":
					if (xmlNamespace == null)
						xmlNamespace = value;
					// TODO: else and exception
					break;
				}
			}
			if (clrNamespace == null || 
					assemblyName == null || 
					xmlNamespace == null)
				throw new Exception("underspecified");
		}
	
		public Mapping(string clrNamespace, string assembly, string xmlNamespace) 
		{
			this.clrNamespace = clrNamespace;
			this.assemblyName = assembly;
			this.xmlNamespace = xmlNamespace;
		}

		public string XmlNamespace {
			get { return xmlNamespace; }
		}

		public Type Resolve(string className)
		{
			Assembly assembly = Assembly.Load(assemblyName);
			Type type = assembly.GetType(clrNamespace + "." + className);
			if (type == null) {
				throw new MappingException(className, XmlNamespace);
			} else {
				return type;
			}
		}
	}
}
