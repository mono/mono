//
// IdentificationTable.cs: Implementation of environments for jscript. Using a
// modified version of the algorithm and date structure presented by
// Andrew W. Appel in his book Modern compiler implementation in Java,
// second edition.
//
// Author:
//	Cesar Lopez Nataren (cnataren@novell.com)
//
// (C) 2003, Cesar Lopez Nataren
// Copyright (C) 2005 Novell Inc (http://novell.com)
//

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

namespace Microsoft.JScript {
	/// <summary>
	/// Class that encapsulates a string id for faster hashing purposes.
	/// </summary>
	internal class Symbol {
		private string name;
		private static Hashtable dict = new Hashtable ();

		internal string Value {
			get { return name; }
		}

		private Symbol (string name)
		{
			this.name = name;
		}

		public override string ToString ()
		{
			return name;
		}

		/// <summary>
		/// Return the unique symbol associated with a
		/// string. Repeated calls to CreateSymbol will return the
		/// same Symbol. 
		/// </summary>
		internal static Symbol CreateSymbol (string n)
		{
			n = LateBinding.MapToInternalName (n);
			string u = String.Intern (n);
			Symbol s = (Symbol) dict [u];
		
			if (s == null) {
				s = new Symbol (u);
				dict [u] = s;
			}
			return s;
		}
	}

	/// <summary>
	/// Associates a symbol to its declaring object.
	/// </summary>
	internal class Binder {
		object value;
		Symbol prev_top;

		/// <remarks>
		/// If the symbol is already in the environment, resolves
		/// collisions with external chaining. 
		/// </remarks>
		Binder tail;

		internal object Value {
			get { return value; }
			set { this.value = value; }
		}

		internal Binder Tail {
			get { return tail; }
		}

		internal Symbol PrevTop {
			get { return prev_top; }
		}

		internal Binder (object value, Symbol prev_top, Binder tail)
		{
			this.value = value;
			this.prev_top = prev_top;
			this.tail = tail;
		}
	}

	/// <summary>
	/// Environment implementation, each key must be a Symbol and we take
	/// care of scoping. 
	/// </summary>
	internal class IdentificationTable {
		private Hashtable dict = new Hashtable ();
		private Symbol top;
		private Binder marks;
		private Stack locals;
		private bool previous_scope = false;
		private bool catch_scope = false;
		internal bool CatchScope {
			get { return catch_scope; }
		}
		
		internal int depth;

		internal IdentificationTable ()
		{
			locals = new Stack ();
			locals.Push (new Hashtable ());
			depth = 0;
		}

		internal bool Contains (Symbol key)
		{
			Binder e = (Binder) dict [key];
			return e != null;
		}

		/// <summary>
		/// Gets the object associated to the symbol in the table
		/// </summary>
		internal object Get (Symbol key)
		{
			Binder e = (Binder) dict [key];

			if (e == null)
				return null;
			else
				return e.Value;
		}

		/// <summary>
		/// Bind a key
		/// </summary>
		internal void Enter (Symbol key, object value)
		{
 			Binder e = (Binder) dict [key];

			/// <remarks>
			/// If a Binder's Value is null means that it
			/// represents a in-transit binding, we must
			/// set its value to something useful.
			/// </remarks>
 			if (e != null && e.Value == null)
 				e.Value = value;
			else {
				//
				// If 'key' is already on the table we form a
				// Binder's chain, otherwise we include the new key 
				// represented with its association object.
				//
				dict [key] = new Binder (value, top, (Binder) dict [key]);

				// 
				// make 'key' the most recent symbol bound
				//
				top = key;					
			}
			Hashtable top_ht = (Hashtable) locals.Peek ();
			string val = key.Value;
			top_ht[val] = value;
		}

		/// <summary>
		/// Delete symbol from the table
		/// </summary>
		internal void Remove (Symbol key)
		{
			Binder e = (Binder) dict [key];
			if (e != null)
				if (e.Tail != null)
					dict [key] = e.Tail;
				else
					dict.Remove (key);
		}

		internal void BeginScope ()
		{
			BeginScope (false);
		}
		
		/// <summary>
		/// Remembers the current state of the table
		/// </summary>
		internal void BeginScope (bool catchScope)
		{
			previous_scope = catch_scope;
			catch_scope = catchScope;
			
			marks = new Binder (null, top, marks);
			top = null;

			locals.Push (new Hashtable ());
			depth++;
		}

		/// <summary>
		/// Restores the table to what it was at the most recent BeginScope
		/// that has not already been ended
		/// </summary>
		internal void EndScope ()
		{
			//
			// Delete all the elements until we find 
			// that top is null, that occurs when we find 
			// the scope marker.
			//
			while (top != null) {
				Binder e = (Binder) dict [top];

				//
				// If there's a chain we delete the first
				// element of it, otherwise remove the symbol
				// from the table.
				//
				if (e.Tail != null)
					dict [top] = e.Tail;
				else
					dict.Remove (top);

				top = e.PrevTop;
			}

			//
			// marks.PrevTop always contains the latest symbol 
			// which was bound before the new scope was created.
			// 
			top = marks.PrevTop;

			//
			// delete the latest scope mark
			//
			marks = marks.Tail;
			locals.Pop ();

			depth--;
			catch_scope = previous_scope;
		}

		internal object [] CurrentLocals {
			get {
				Stack stack = new Stack ();
				Symbol _top = top;

				while (_top != null) {
					Binder e = (Binder) dict [_top];
					stack.Push (e.Value);
					_top = e.PrevTop;
				}
				if (stack.Count == 0)
					return null;

				object [] res = new object [stack.Count];
				stack.CopyTo (res, 0);
				return res;
			}
		}

		internal bool InCurrentScope (Symbol id) 
		{
			Hashtable hash = (Hashtable) locals.Peek ();
			return hash.ContainsKey (id.Value) && hash [id.Value] != null;
		}

		internal void AddToCurrentLocals (string name, object o)
		{
			((Hashtable) locals.Peek ()).Add (name, o);
		}

		internal DictionaryEntry [] LocalsAtDepth (int depth)
		{
			object [] hashes = new object [locals.Count];
			locals.CopyTo (hashes, 0);
			Hashtable hash = (Hashtable) hashes [locals.Count - depth - 1];
			DictionaryEntry [] _locals = new DictionaryEntry [hash.Count];
			hash.CopyTo (_locals, 0);
			return _locals;
		}
	}


	//
	// Lookup table with namespace context.
	// 
	internal class Environment {

		private Hashtable namespaces;

		internal Environment ()
		{
			namespaces = new Hashtable ();

			//
			// global variables identification table
			//
			namespaces.Add (String.Empty, new IdentificationTable ());
		}

		internal Environment (ScriptBlock [] blocks)
			: this ()
		{
			//
			// FIXME: when we implement the package stm. For each package
			// name we must add a identification table that will store
			// id's defined inside it.
			//

			//
			// integrate the builtin functions, objects, ...
			//
			BuildGlobalEnv ();
		}

		//
		// retrieves the identification table for the ns namespace
		//
		internal IdentificationTable GetIdentificationTable (string ns)
		{
			return (IdentificationTable) namespaces [ns];
		}

		internal bool Contains (string ns, Symbol key)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				return false;

			return symbols.Contains (key);
		}

		internal object Get (string ns, Symbol key)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				return null;

			return symbols.Get (key);
		}

		internal void Enter (string ns, Symbol key, object value)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];
			
			if (symbols == null)
				throw new Exception (ns + " does not exist");

			symbols.Enter (key, value);
		}

		internal void Remove (string ns, Symbol key)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];
			
			if (symbols == null)
				throw new Exception (ns + " does not exist");

			symbols.Remove (key);
		}

		internal void BeginScope (string ns)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			symbols.BeginScope ();
		}

		internal void BeginScope (string ns, bool catchScope)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			symbols.BeginScope (catchScope);
		}

		internal void EndScope (string ns)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			symbols.EndScope ();
		}

		internal object [] CurrentLocals (string ns)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			return symbols.CurrentLocals;
		}

		internal int Depth (string ns)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			return symbols.depth;
		}

		internal bool InCurrentScope (string ns, Symbol id) 
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			return symbols.InCurrentScope (id);
		}

		internal bool CatchScope (string ns)
		{
			IdentificationTable symbols = (IdentificationTable) namespaces [ns];

			if (symbols == null)
				throw new Exception (ns + " does not exist");

			return symbols.CatchScope;
		}

		internal void BuildGlobalEnv ()
		{
			//
			// built in print function
			//
			if (SemanticAnalyser.print)
				Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("print"), new BuiltIn ("print", false, true));

			/* value properties of the Global Object */
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("NaN"), new BuiltIn ("NaN", false, false));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Infinity"), new BuiltIn ("Infinity", false, false));
 			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("undefined"), new BuiltIn ("undefined", false, false));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("null"), new BuiltIn ("null", false, false));
			
 			/* function properties of the Global Object */
			object [] custom_attrs;
			Type global_object = typeof (GlobalObject);
			MethodInfo [] methods = global_object.GetMethods (BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			foreach (MethodInfo mi in methods) {
				custom_attrs = mi.GetCustomAttributes (typeof (JSFunctionAttribute), false);
				foreach (JSFunctionAttribute attr in custom_attrs)
					if (attr.IsBuiltIn)
 						Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol (mi.Name), new BuiltIn (SemanticAnalyser.ImplementationName (attr.BuiltIn.ToString ()), false, true));
 			}

 			/* built in objects */
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Object"), new BuiltIn ("Object", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Function"), new BuiltIn ("Function", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Array"), new BuiltIn ("Array", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("String"), new BuiltIn ("String", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Boolean"), new BuiltIn ("Boolean", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Number"), new BuiltIn ("Number", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Math"), new BuiltIn ("Math", false, false));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Date"), new BuiltIn ("Date", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("RegExp"), new BuiltIn ("RegExp", true, true));

 			/* built in Error objects */
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("Error"), new BuiltIn ("Error", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("EvalError"), new BuiltIn ("EvalError", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("RangeError"), new BuiltIn ("RangeError", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("ReferenceError"), new BuiltIn ("ReferenceError", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("SyntaxError"), new BuiltIn ("SyntaxError", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("TypeError"), new BuiltIn ("TypeError", true, true));
			Enter (SemanticAnalyser.GlobalNamespace, Symbol.CreateSymbol ("URIError"), new BuiltIn ("URIError", true, true));
		}
	}
}
