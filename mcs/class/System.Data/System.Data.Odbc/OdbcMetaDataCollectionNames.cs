//
// System.Data.Odbc.OdbcMetaDataCollectionNames
//
// Author:
//   Amit Biswas (amit@amitbiswas.com)
//
// Copyright (C) Novell Inc, 2007
//

//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.Odbc
{
	/// <summary>
	/// Provides a list of constants which can be used with the GetSchema method to retrieve metadata collections.
	/// </summary>
	/// 

#if NET_2_0
	public static class OdbcMetaDataCollectionNames
	{
	#region Fields

		#endregion // Fields


		#region Methods

		public virtual bool Equals (Object obj)
		{
			return (this == obj);
		}

		public static bool Equals (Object o1, Object o2)
		{
			if (o1 == o2 || (o1 != null && o1.Equals (o2) == true))
				return true;
			else
				return false;
		}

		#endregion // Methods
	}
#endif // NET_2_0

}
