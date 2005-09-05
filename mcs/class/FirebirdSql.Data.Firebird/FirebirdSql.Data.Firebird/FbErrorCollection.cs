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

using System;
using System.Collections;
using System.ComponentModel;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/overview/*'/>
#if	(!NETCF)
	[Serializable, ListBindable(false)]
#endif
	public sealed class FbErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		private ArrayList errors;

		#endregion

		#region Indexers

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/indexer[@name="Item(System.Int32)"]/*'/>
		public FbError this[int index]
		{
			get { return (FbError)this.errors[index]; }
		}

		#endregion

		#region Constructors

		internal FbErrorCollection()
		{
			this.errors = ArrayList.Synchronized(new ArrayList());
		}

		#endregion

		#region ICollection	Properties

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/property[@name="Count"]/*'/>
		public int Count
		{
			get { return this.errors.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return this.errors.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return this.errors.SyncRoot; }
		}

		#endregion

		#region ICollection	Methods

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/method[@name="CopyTo(System.Array,System.Int32)"]/*'/>	
		public void CopyTo(Array array, int index)
		{
			this.errors.CopyTo(array, index);
		}

		#endregion

		#region IEnumerable	Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.errors.GetEnumerator();
		}

		#endregion

		#region Internal Methods

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/method[@name="IndexOf(System.String)"]/*'/>		
		internal int IndexOf(string errorMessage)
		{
			int index = 0;
			foreach (FbError item in this)
			{
				if (GlobalizationHelper.CultureAwareCompare(item.Message, errorMessage))
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/method[@name="Add(FbError)"]/*'/>
		internal FbError Add(FbError error)
		{
			this.errors.Add(error);

			return error;
		}

		/// <include file='Doc/en_EN/FbErrorCollection.xml'	path='doc/class[@name="FbErrorCollection"]/method[@name="Add(System.String,System.Int32)"]/*'/>
		internal FbError Add(string errorMessage, int number)
		{
			FbError error = new FbError(errorMessage, number);

			return this.Add(error);
		}

		#endregion
	}
}
