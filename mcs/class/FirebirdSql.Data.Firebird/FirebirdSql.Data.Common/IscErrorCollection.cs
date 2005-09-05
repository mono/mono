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

namespace FirebirdSql.Data.Common
{
	internal sealed class IscErrorCollection : CollectionBase
	{
		#region Indexers

		public IscError this[int index]
		{
			get { return (IscError)this.List[index]; }
		}

		#endregion

		#region Methods

		public IscError Add(int type, string strParam)
		{
			return this.Add(new IscError(type, strParam));
		}

		public IscError Add(int type, int errorCode)
		{
			return this.Add(new IscError(type, errorCode));
		}

		public IscError Add(IscError error)
		{
			this.List.Add(error);

			return error;
		}

		#endregion
	}
}
