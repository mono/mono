//
// Mono.Data.TdsTypes.TdsTruncateException.cs
//
// Authors: 
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

using Mono.Data.TdsClient;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Mono.Data.TdsTypes {
	[Serializable]
	public class TdsTruncateException : TdsTypeException
	{
		#region Constructors

		public TdsTruncateException ()
			: base (Locale.GetText ("This value is being truncated"))
		{
		}

		public TdsTruncateException (string message)
			: base (message)
		{
		}

		#endregion // Constructors
	}
}
