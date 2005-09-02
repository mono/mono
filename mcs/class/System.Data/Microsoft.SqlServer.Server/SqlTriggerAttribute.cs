//
// Microsoft.SqlServer.Server.SqlTriggerAttribute
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
//  Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;

namespace Microsoft.SqlServer.Server {
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[Serializable]
	public sealed class SqlTriggerAttribute : Attribute
	{
		#region Fields

		private string triggerEvent;
		private string name;
		private string target;
 
		#endregion // Fields

		#region Constructors

		public SqlTriggerAttribute ()
			: base ()
		{
			this.triggerEvent = null;
			this.name = null;
			this.target = null;
		}

		#endregion // Constructors

		#region Properties

		public string Event {
			get { return this.triggerEvent; }
			set { this.triggerEvent = value; }
		}

		public string Name {
			get { return this.name; }
			set { this.name = value; }
		}

		public string Target {
			get { return this.target; }
			set { this.target = value; }
		}


		#endregion // Properties
	}
}

#endif
