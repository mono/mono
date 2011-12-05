/*
  Copyright (C) 2011 Jeroen Frijters

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
using System.Text;

namespace IKVM.Reflection.Emit
{
	public sealed class CustomModifiersBuilder
	{
		private readonly List<Item> list = new List<Item>();

		internal struct Item
		{
			internal Type type;
			internal bool required;
		}

		public void AddRequired(Type type)
		{
			Item item;
			item.type = type;
			item.required = true;
			list.Add(item);
		}

		public void AddOptional(Type type)
		{
			Item item;
			item.type = type;
			item.required = false;
			list.Add(item);
		}

		// this adds the custom modifiers in the same order as the normal SRE APIs
		// (the advantage over using the SRE APIs is that a CustomModifiers object is slightly more efficient,
		// because unlike the Type arrays it doesn't need to be copied)
		public void Add(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			foreach (CustomModifiers.Entry entry in CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers))
			{
				Item item;
				item.type = entry.Type;
				item.required = entry.IsRequired;
				list.Add(item);
			}
		}

		public CustomModifiers Create()
		{
			return new CustomModifiers(list);
		}
	}
}
