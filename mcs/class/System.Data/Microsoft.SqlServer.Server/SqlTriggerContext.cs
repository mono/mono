//
// Microsoft.SqlServer.Server.SqlTriggerContext.cs
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
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Server {

	public sealed class SqlTriggerContext
	{
		#region Fields

		private TriggerAction triggerAction;
		private bool[] columnsUpdated;
		private SqlXml eventData;
 
		#endregion // Fields

		#region Constructors

		internal SqlTriggerContext (TriggerAction triggerAction, bool[] columnsUpdated, 
					SqlXml eventData)
		{
			this.triggerAction = triggerAction;
			this.columnsUpdated = columnsUpdated;
			this.eventData = eventData;
		}

		#endregion // Constructors

		#region Properties

		public int ColumnCount {
			get {
				 return this.columnsUpdated == null ? 0 : columnsUpdated.Length; 
			}
		}

		public SqlXml EventData {
			get { return this.eventData; }
		}

		public TriggerAction TriggerAction {
			get { return this.triggerAction; }
		}


		#endregion // Properties

		#region Methods
		
		public bool IsUpdatedColumn (int columnOrdinal)
		{
			if (columnsUpdated == null)
				throw new IndexOutOfRangeException("The index specified does not exist");
		
			return this.columnsUpdated[columnOrdinal];
		}

		#endregion //Methods
	}
}

#endif
