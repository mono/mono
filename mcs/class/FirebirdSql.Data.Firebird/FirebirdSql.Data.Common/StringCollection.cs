/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(NETCF)

using System;
using System.Collections;

namespace System.Collections.Specialized
{
	public class StringCollection : CollectionBase
	{
		#region Indexers

		public string this[int index]
		{
			get { return ((string)base.List[index]); }
			set { base.List[index] = value; }
		}

		#endregion

		#region Methods

		public int Add(string value)
		{
			return (base.List.Add(value));
		}

		public void CopyTo(string[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array is null.");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index is less than zero.");
			}
			if (array.Rank > 1)
			{
				throw new ArgumentException("array is multidimensional.");
			}
			if (index >= array.Length)
			{
				throw new ArgumentException("index is equal to or greater than the length of array.");
			}
			if ((array.Length - index) < this.List.Count)
			{
				throw new ArgumentException("The number of elements in the source StringCollection is greater than the available space from index to the end of the destination array.");
			}

			foreach (string value in this.List)
			{
				if (index < array.Length)
				{
					array[index++] = value;
				}
			}
		}

		public int IndexOf(string value)
		{
			return (base.List.IndexOf(value));
		}

		public void Insert(int index, string value)
		{
			base.List.Insert(index, value);
		}

		public void Remove(string value)
		{
			base.List.Remove(value);
		}

		public bool Contains(string value)
		{
			// If value	is not of type String, this	will return	false.
			return (base.List.Contains(value));
		}

		#endregion

		#region Protected MEthods

		protected override void OnValidate(Object value)
		{
			if (value.GetType() != Type.GetType("System.String"))
			{
				throw new ArgumentException("value must be of type String.", "value");
			}
		}

		#endregion
	}
}

#endif