//
// Mono.Data.TdsTypes.TdsNullValueException.cs
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
	public class TdsNullValueException : TdsTypeException
	{
		#region Constructors

		public TdsNullValueException ()
			: base (Locale.GetText ("The value property is null"))
		{
		}

		public TdsNullValueException (string message)
			: base (message)
		{
		}

		#endregion // Constructors
	}
}
