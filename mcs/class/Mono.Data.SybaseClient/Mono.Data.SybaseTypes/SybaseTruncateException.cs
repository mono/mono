//
// Mono.Data.SybaseTypes.SybaseTruncateException.cs
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
	public class SybaseTruncateException : SybaseTypeException
	{
		#region Constructors

		public SybaseTruncateException ()
			: base (Locale.GetText ("This value is being truncated"))
		{
		}

		public SybaseTruncateException (string message)
			: base (message)
		{
		}

		#endregion // Constructors
	}
}
