//
// IndentingTypeDisplayer.cs: Displays type information as a tree
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
	public class IndentingTypeDisplayer : TypeDisplayer {

		private IndentingTextWriter _writer;

		protected IndentingTypeDisplayer (TextWriter writer)
		{
			SetWriter (writer);
		}

		public IndentingTextWriter Writer {
			get {return _writer;}
		}

		public void SetWriter (TextWriter writer)
		{
			_writer = new IndentingTextWriter (writer);
			_writer.IndentChar = ' ';
			_writer.IndentSize = 2;
		}

		protected Indenter GetIndenter ()
		{
			return new Indenter (Writer);
		}

		protected void Write (object o)
		{
			if (Writer.IndentLevel <= MaxDepth)
				Writer.Write (o);
		}

		protected void Write (string format, params object[] args)
		{
			if (Writer.IndentLevel <= MaxDepth)
				Writer.Write (format, args);
		}

		protected void WriteLine (object o)
		{
			if (Writer.IndentLevel <= MaxDepth)
				Writer.WriteLine (o);
		}

		protected void WriteLine (string format, params object[] args)
		{
			if (Writer.IndentLevel <= MaxDepth)
				Writer.WriteLine (format, args);
		}

		protected void WriteLine ()
		{
			Writer.WriteLine ();
		}

		protected override void OnType (TypeEventArgs e)
		{
			OnIndentedType (e);
		}

		protected virtual void OnIndentedType (TypeEventArgs e)
		{
		}

		protected override void OnBaseType (BaseTypeEventArgs e)
		{
			using (Indenter n = GetIndenter ()) {
				OnIndentedBaseType (e);
			}
		}

		protected virtual void OnIndentedBaseType (BaseTypeEventArgs e)
		{
		}

		protected override void OnInterfaces (InterfacesEventArgs e)
		{ 
			using (Indenter n = GetIndenter()) {
				OnIndentedInterfaces (e);
			}
		}

		protected virtual void OnIndentedInterfaces (InterfacesEventArgs e)
		{
		}

		protected override void OnFields (FieldsEventArgs e)
		{
			using (Indenter n1 = GetIndenter()) {
				OnIndentedFields (e);
			}
		}

		protected virtual void OnIndentedFields (FieldsEventArgs e)
		{
		}

		protected override void OnProperties (PropertiesEventArgs e)
		{
			using (Indenter n1 = GetIndenter()) {
				OnIndentedProperties (e);
			}
		}

		protected virtual void OnIndentedProperties (PropertiesEventArgs e)
		{
		}

		protected override void OnEvents (EventsEventArgs e)
		{
			using (Indenter n1 = GetIndenter()) {
				OnIndentedEvents (e);
			}
		}

		protected virtual void OnIndentedEvents (EventsEventArgs e)
		{
		}

		protected override void OnConstructors (ConstructorsEventArgs e)
		{
			using (Indenter n1 = GetIndenter()) {
				OnIndentedConstructors (e);
			}
		}

		protected virtual void OnIndentedConstructors (ConstructorsEventArgs e)
		{
		}

		protected override void OnMethods (MethodsEventArgs e)
		{
			using (Indenter n1 = GetIndenter()) {
				OnIndentedMethods (e);
			}
		}

		protected virtual void OnIndentedMethods (MethodsEventArgs e)
		{
		}
	}
}

