//
// Mono.Data.TdsClient.Internal.TdsContext.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Text;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsContext 
	{
		#region Fields

		DataColumnCollection columns;
		Encoding encoding;

		#endregion // Fields

		#region Constructors

		public TdsContext (DataColumnCollection columns, Encoding encoding)
		{
			this.columns = columns;
			this.encoding = encoding;
		}

		#endregion // Constructors

		#region Properties

		public DataColumnCollection Columns {
			get { return Columns; }
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		#endregion // Properties

		#region Methods

		public void Clear ()
		{
			columns.Clear ();
		}
		
		#endregion // Methods
	}
}
