/*
  Copyright (C) 2009 Jeroen Frijters

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
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	[Flags]
	public enum ExceptionHandlingClauseOptions
	{
		Clause  = 0x0000,
		Filter  = 0x0001,
		Finally = 0x0002,
		Fault   = 0x0004,
	}

	public sealed class ExceptionHandlingClause
	{
		private readonly int flags;
		private readonly int tryOffset;
		private readonly int tryLength;
		private readonly int handlerOffset;
		private readonly int handlerLength;
		private readonly Type catchType;
		private readonly int filterOffset;

		internal ExceptionHandlingClause(ModuleReader module, int flags, int tryOffset, int tryLength, int handlerOffset, int handlerLength, int classTokenOrfilterOffset, IGenericContext context)
		{
			this.flags = flags;
			this.tryOffset = tryOffset;
			this.tryLength = tryLength;
			this.handlerOffset = handlerOffset;
			this.handlerLength = handlerLength;
			this.catchType = flags == (int)ExceptionHandlingClauseOptions.Clause && classTokenOrfilterOffset != 0 ? module.ResolveType(classTokenOrfilterOffset, context) : null;
			this.filterOffset = flags == (int)ExceptionHandlingClauseOptions.Filter ? classTokenOrfilterOffset : 0;
		}

		public Type CatchType
		{
			get { return catchType; }
		}

		public int FilterOffset
		{
			get { return filterOffset; }
		}

		public ExceptionHandlingClauseOptions Flags
		{
			get { return (ExceptionHandlingClauseOptions)flags; }
		}

		public int HandlerLength
		{
			get { return handlerLength; }
		}

		public int HandlerOffset
		{
			get { return handlerOffset; }
		}

		public int TryLength
		{
			get { return tryLength; }
		}

		public int TryOffset
		{
			get { return tryOffset; }
		}
	}
}
