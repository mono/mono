/*
  Copyright (C) 2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;

namespace IKVM.Reflection.Emit
{
	public struct ExceptionHandler : IEquatable<ExceptionHandler>
	{
		private readonly int tryOffset;
		private readonly int tryLength;
		private readonly int filterOffset;
		private readonly int handlerOffset;
		private readonly int handlerLength;
		private readonly ExceptionHandlingClauseOptions kind;
		private readonly int exceptionTypeToken;

		public ExceptionHandler(int tryOffset, int tryLength, int filterOffset, int handlerOffset, int handlerLength, ExceptionHandlingClauseOptions kind, int exceptionTypeToken)
		{
			if (tryOffset < 0 || tryLength < 0 || filterOffset < 0 || handlerOffset < 0 || handlerLength < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.tryOffset = tryOffset;
			this.tryLength = tryLength;
			this.filterOffset = filterOffset;
			this.handlerOffset = handlerOffset;
			this.handlerLength = handlerLength;
			this.kind = kind;
			this.exceptionTypeToken = exceptionTypeToken;
		}

		public int TryOffset
		{
			get { return tryOffset; }
		}

		public int TryLength
		{
			get { return tryLength; }
		}

		public int FilterOffset
		{
			get { return filterOffset; }
		}

		public int HandlerOffset
		{
			get { return handlerOffset; }
		}

		public int HandlerLength
		{
			get { return handlerLength; }
		}

		public ExceptionHandlingClauseOptions Kind
		{
			get { return kind; }
		}

		public int ExceptionTypeToken
		{
			get { return exceptionTypeToken; }
		}

		public bool Equals(ExceptionHandler other)
		{
			return tryOffset == other.tryOffset
				&& tryLength == other.tryLength
				&& filterOffset == other.filterOffset
				&& handlerOffset == other.handlerOffset
				&& handlerLength == other.handlerLength
				&& kind == other.kind
				&& exceptionTypeToken == other.exceptionTypeToken;
		}

		public override bool Equals(object obj)
		{
			ExceptionHandler? other = obj as ExceptionHandler?;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return tryOffset ^ tryLength * 33 ^ filterOffset * 333 ^ handlerOffset * 3333 ^ handlerLength * 33333;
		}

		public static bool operator ==(ExceptionHandler left, ExceptionHandler right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ExceptionHandler left, ExceptionHandler right)
		{
			return !left.Equals(right);
		}
	}
}
