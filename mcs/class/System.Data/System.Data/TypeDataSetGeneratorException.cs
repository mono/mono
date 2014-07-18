//
// System.Data.TypedDataSetGeneratorException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

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

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data
{
#if !WINDOWS_PHONE && !NETFX_CORE
	[Serializable]
#endif
	public class TypedDataSetGeneratorException : DataException
	{
		readonly ArrayList errorList;

		#region Constructors

		public TypedDataSetGeneratorException ()
			: base (Locale.GetText ("System error."))
		{
		}

#if !WINDOWS_PHONE && !NETFX_CORE
		public 
#else
		internal
#endif
		TypedDataSetGeneratorException (ArrayList list)
			: base (Locale.GetText ("System error."))
		{
			errorList = list;
		}

#if !WINDOWS_PHONE && !NETFX_CORE
		protected TypedDataSetGeneratorException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			int count = info.GetInt32 ("KEY_ARRAYCOUNT");
			errorList = new ArrayList (count);

			for (int i=0; i < count; i++)
				errorList.Add (info.GetString("KEY_ARRAYVALUES" + i));
		}
#endif

#if NET_2_0
		public TypedDataSetGeneratorException (String message) : base (message)
		{
		}
		
		public TypedDataSetGeneratorException (String message, Exception innerException)
			: base (message, innerException)
		{
		}
#endif
		#endregion //Constructors

#if !WINDOWS_PHONE && !NETFX_CORE
		public 
#else
		internal
#endif		
		ArrayList ErrorList {
			get { return errorList; }
		}

		#region Methods

#if !WINDOWS_PHONE && !NETFX_CORE
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);

			int count = (errorList != null) ? ErrorList.Count : 0;
			info.AddValue ("KEY_ARRAYCOUNT", count);

			for (int i=0; i < count; i++)
				info.AddValue ("KEY_ARRAYVALUES" + i, ErrorList [i]);
		}
#endif

		#endregion // Methods
	}
}
