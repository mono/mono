//
// ast.cs: Data structures used for building the EcmaScript 's Abstract Syntax
//         Tree.
//
// Author: Cesar Octavio Lopez Nataren 
//
// (C) 2003 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//


namespace Microsoft.JScript
{
	using System;
	using System.Collections;
	using System.Text;

	using System.Reflection;
	using System.Reflection.Emit;


        public class Program
	{
		public SourceElements se;

		public SourceElements SourceElements {
			get { return se; }

			set { se = value; }
		}


		public Program ()
		{
			se = new SourceElements ();
		}

		public override string ToString ()
		{
			return se.ToString ();
		}
	}


	
        public class SourceElements : IEnumerable, IEnumerator
	{
		int pos = -1;

		public ArrayList elems;

		// IEnumerable implementation
		public IEnumerator GetEnumerator ()
		{
			return (IEnumerator) this;
		}

		
		// IEnumerator methods implementations
		public bool MoveNext ()
		{
			if (pos < Size) {
				pos++;
				return true;
			} else return false;
		}

		public void Reset () { pos = 0; }

		public object Current
		{
			get { return elems [pos]; }
		}			
				
		public SourceElements ()
		{
			elems = new ArrayList ();
		}



		public int Size {
			get {
				return elems.Count;
			}
		}


		public void Add (SourceElement se)
		{
			elems.Add (se);
		}

		public void Add (object o)
		{		
			elems.Add (o);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (SourceElement se in elems) {
				sb.Append (se.ToString ());
				sb.Append (" ");
			}
			
			return sb.ToString ();
		}
	}


        public class SourceElement 
	{
		public SourceElement ()
		{}
	}


        public class Statement : SourceElement
	{
		public virtual bool Resolve (Context context)
		{
			throw new NotImplementedException ();
		}

		public virtual void Emit (Context context)
		{}
	}


        public class PrintStatement: Statement
	{
		private string message;

		public PrintStatement ()
		{}

		
		public PrintStatement (string str)
		{
			message = str;
		}


		public string Message {
			get { return message; }
			set {  message = value; }
		}


		public override void Emit (Context context)
		{
				context.ig.Emit (OpCodes.Ldstr, message);

				context.ig.Emit (OpCodes.Call, 
						 typeof (Microsoft.JScript.ScriptStream).GetMethod ("WriteLine", new Type [] {typeof (string)}));				
		}

		

		public override string ToString ()
		{
			return message;
		}
	}


	public class FunctionDeclaration : SourceElement
	{
		public SourceElements elems;

		public FunctionDeclaration ()
		{
			elems = new SourceElements ();
		}


		public virtual void Emit (Context context)
		{}
	}
}
