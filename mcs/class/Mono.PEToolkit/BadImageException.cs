/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit {

	public class BadImageException : Exception {

		public BadImageException() : base()
		{
		}

		public BadImageException(string msg) : base(msg)
		{
		}
	}

}
