//
// Mono.Data.TdsTypes.TdsTypeException.cs
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
	public class TdsTypeException : SystemException
	{
		#region Constructors

		public TdsTypeException (string message)
			: base (message)
		{
		}

		protected TdsTypeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion // Constructors
	}
}
