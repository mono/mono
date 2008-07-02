//
// System.Runtime.Remoting.Contexts.ContextAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Remoting.Activation;
using System.Collections;

namespace System.Runtime.Remoting.Contexts {

	[AttributeUsage (AttributeTargets.Class)]
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class ContextAttribute : Attribute, IContextAttribute, IContextProperty {
		protected string AttributeName;

		public ContextAttribute (string name)
		{
			AttributeName = name;
		}

		public virtual string Name {
			get {
				return AttributeName;
			}
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;

			if (!(o is ContextAttribute))
				return false;

			ContextAttribute ca = (ContextAttribute) o;
			
			if (ca.AttributeName != AttributeName)
				return false;

			return true;
		}

		public virtual void Freeze (Context newContext)
		{
		}

		public override int GetHashCode ()
		{
			if (AttributeName == null)
				return 0;
			
			return AttributeName.GetHashCode ();
		}

		/// <summary>
		///    Adds the current context property to the IConstructionCallMessage
		/// </summary>
		public virtual void GetPropertiesForNewContext (IConstructionCallMessage ctorMsg)
		{
			if (ctorMsg == null)
				throw new ArgumentNullException ("ctorMsg");

			IList list = ctorMsg.ContextProperties;

			list.Add (this);
		}

		// <summary>
		//   True whether the context arguments satisfies the requirements
		//   of the current context.
		// </summary>
		public virtual bool IsContextOK (Context ctx, IConstructionCallMessage ctorMsg)
		{
			if (ctorMsg == null)
				throw new ArgumentNullException ("ctorMsg");
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (!ctorMsg.ActivationType.IsContextful)
				return true;

			IContextProperty p = ctx.GetProperty (AttributeName);
			if (p == null)
				return false;

			if (this != p)
				return false;
				
			return true;
		}

		public virtual bool IsNewContextOK (Context newCtx)
		{
			return true;
		}
	}
}
