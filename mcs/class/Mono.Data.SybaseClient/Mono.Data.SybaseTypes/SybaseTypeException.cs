//
// Mono.Data.SybaseTypes.SybaseTypeException.cs
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
	public class SybaseTypeException : SystemException
	{
		#region Constructors

		public SybaseTypeException (string message)
			: base (message)
		{
		}

		protected SybaseTypeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		#endregion // Constructors
	}
}
