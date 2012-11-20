/*
  Copyright (C) 2009-2011 Jeroen Frijters

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

namespace IKVM.Reflection
{
	public class LocalVariableInfo
	{
		private readonly int index;
		private readonly Type type;
		private readonly bool pinned;
		private readonly CustomModifiers customModifiers;

		internal LocalVariableInfo(int index, Type type, bool pinned)
		{
			this.index = index;
			this.type = type;
			this.pinned = pinned;
		}

		internal LocalVariableInfo(int index, Type type, bool pinned, CustomModifiers customModifiers)
			: this(index, type, pinned)
		{
			this.customModifiers = customModifiers;
		}

		public bool IsPinned
		{
			get { return pinned; }
		}

		public int LocalIndex
		{
			get { return index; }
		}

		public Type LocalType
		{
			get { return type; }
		}

		public CustomModifiers __GetCustomModifiers()
		{
			return customModifiers;
		}

		public override string ToString()
		{
			return String.Format(pinned ? "{0} ({1}) (pinned)" : "{0} ({1})", type, index);
		}
	}
}
