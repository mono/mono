// SortedStringValues.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The original concept for this class was presented in an MSDN online article
// by Eric Gunnerson.

using System;
using System.Collections;

namespace Mono.Doc.Core
{
	public class SortedStringValues : IEnumerable
	{
		private IEnumerable enumerable;

		public SortedStringValues(IEnumerable enumerable)
		{
			this.enumerable = enumerable;
		}

		internal class SortedStringValuesEnumerator : IEnumerator
		{
			private ArrayList   items = new ArrayList();
			private int         current;

			internal SortedStringValuesEnumerator(IEnumerator enumerator)
			{
				while (enumerator.MoveNext())
				{
					items.Add(enumerator.Current.ToString());
				}

				IDisposable disposable = enumerator as IDisposable;

				if (disposable != null)
				{
					disposable.Dispose();
				}

				items.Sort();
				current = -1;
			}

			public void Reset()
			{
				current = -1;
			}

			public bool MoveNext()
			{
				if (++current == items.Count) return false;

				return true;
			}

			public object Current
			{
				get { return items[current]; }
			}
		}

		public IEnumerator GetEnumerator()
		{
			return new SortedStringValuesEnumerator(enumerable.GetEnumerator());
		}
	}
}
