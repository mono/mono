//
// TypeDisplayerFactory.cs: Factory for TypeDisplayer objects
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
	public sealed class TypeDisplayerFactory
	{
		private static IDictionary entries = new Hashtable ();

		public static void Add (string key, Type value)
		{
			lock (entries) {
				entries.Add (key, value);
			}
		}

		private static TypeDisplayer CreateInstance (Type type, TextWriter writer)
		{
			// Will work when Activator.CreateInstance works properly.
#if HAVE_WORKING_ACTIVATOR_CREATE_INSTANCE
			return (TypeDisplayer) Activator.CreateInstance (type, 
					new object[]{writer});
#else
			// Look up constructor and invoke ourselves...
			ConstructorInfo ctor = 
				type.GetConstructor(new Type[]{typeof(TextWriter)});
			if (ctor == null)
				return null;
			return (TypeDisplayer) ctor.Invoke(new object[]{writer});
#endif
		}

		public static TypeDisplayer Create (string key, TextWriter writer)
		{
			try {
				Type type = null;
				lock (entries) {
					type = (Type) entries[key];
				}
				if (type == null)
					type = typeof (ExplicitTypeDisplayer);
				return CreateInstance (type, writer);
			}
			catch {
			}
			return  null;
		}

		public static void Remove (string key)
		{
			lock (entries) {
				entries.Remove (key);
			}
		}

		public static ICollection Keys {
			get {
				return entries.Keys;
			}
		}
	}
}

