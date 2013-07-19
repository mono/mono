//
// DefaultObjectContext.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
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

using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Compiler = Mono.CSharp;
using PSCompiler = Mono.PlayScript;

namespace PlayScript.Runtime
{
	public class DefaultObjectContext
	{
		readonly Stack<Compiler.Expression> stack;
		
		private DefaultObjectContext ()
		{
			stack = new Stack<Compiler.Expression> ();
		}

		internal Stack<Compiler.Expression> Context {
			get {
				return stack;
			}
		}
		
		public static DefaultObjectContext Create ()
		{
			return new DefaultObjectContext ();
		}
		
		// TODO: Will likely need extra argument for something similar to CSharpArgumentInfoFlags to correctly
		// recreate source expression
		public void Register (object instance)
		{
			if (instance == null)
				throw new _root.TypeError (_root.Error.getErrorMessage (1009), 1009);

			var ctx = DynamicContext.Create ();
			var expr = new PSCompiler.ObjectContextRuntimeExpression (ctx.ImportType (instance.GetType ()));

			stack.Push (expr);
		}
		
		public void Unregister ()
		{
			stack.Pop ();
		}
	}
}