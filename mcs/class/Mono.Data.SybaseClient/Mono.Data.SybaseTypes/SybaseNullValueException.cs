//
// Mono.Data.SybaseTypes.SybaseNullValueException.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

using Mono.Data.SybaseClient;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Mono.Data.SybaseTypes {
	[Serializable]
	public class SybaseNullValueException : SybaseTypeException
	{
		#region Constructors

		public SybaseNullValueException ()
			: base (Locale.GetText ("Data is Null. This method or property cannot be called on Null values."))
		{
		}

		public SybaseNullValueException (string message)
			: base (message)
		{
		}

		#endregion // Constructors
	}
}
