/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;
using System.Collections;

namespace FirebirdSql.Data.Common
{
	internal sealed class CharsetCollection : CollectionBase
	{
		#region Indexers

		public Charset this[int index]
		{
			get { return (Charset)this.List[index]; }
		}

		public Charset this[string name]
		{
			get { return (Charset)this[this.IndexOf(name)]; }
		}

		#endregion

		#region Methods

		public int IndexOf(int id)
		{
			int index = 0;

			foreach (Charset item in this)
			{
				if (item.ID == id)
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		public int IndexOf(string name)
		{
			int index = 0;

			foreach (Charset item in this)
			{
				if (GlobalizationHelper.CultureAwareCompare(item.Name, name))
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		internal Charset Add(
			int		id,
			string	charset,
			int		bytesPerCharacter,
			string	systemCharset)
		{
			Charset charSet = new Charset(
				id, charset, bytesPerCharacter, systemCharset);

			return this.Add(charSet);
		}

		internal Charset Add(Charset charset)
		{
			this.List.Add(charset);

			return charset;
		}

		#endregion
	}
}
