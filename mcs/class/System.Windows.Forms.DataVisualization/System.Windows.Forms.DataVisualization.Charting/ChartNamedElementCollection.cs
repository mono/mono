//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
// Francis Fisher (frankie@terrorise.me.uk)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ChartNamedElementCollection<T> : ChartElementCollection<T> where T : ChartNamedElement
	{
		public T this[string name] { //FIXME this should probably be indexed
			get{ 
				foreach (T el in this) {
					if (el.Name == name) {
						return el;
					}
				}
				throw new KeyNotFoundException (); //FIXME check what actual behaviour is in MS implementation
			}

			set{
				for(int i = 0; i<this.Count; i++) 
				{
					T el = this[i];
					if (el.Name == name) {
						this.SetItem (i, value);
						return;
					}
				}
				throw new KeyNotFoundException (); //FIXME check what actual behaviour is in MS implementation
			}
		}

		protected virtual string NamePrefix { get; private set;}

		
		[MonoTODO]
		public virtual T FindByName (string name)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int IndexOf (string name)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void InsertItem (int index,T item)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual bool IsUniqueName (string name)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual string NextUniqueName ()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void RemoveItem (int index)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void SetItem (int index,T item)
		{
			throw new NotImplementedException();
		}
	}
}
