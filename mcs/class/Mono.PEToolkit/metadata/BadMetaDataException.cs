/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	public class BadMetaDataException : Exception {

		public BadMetaDataException() : base()
		{
		}

		public BadMetaDataException(string msg) : base(msg)
		{
		}
	}

}
