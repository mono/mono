// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.ComponentModel.Design;
using ByteFX.Data.MySqlClient;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for DBParametersEditor.
	/// </summary>
	internal class DBParametersEditor : CollectionEditor
	{
		public DBParametersEditor(Type t) : base(t)
		{
		}

		protected override object CreateInstance(Type itemType)
		{
			object[] items = base.GetItems(null);

			int i = 1;
			while (true) 
			{
				bool found = false;
				foreach (object obj in items) 
				{
					MySqlParameter p = (MySqlParameter)obj;
					if (p.ParameterName.Equals( "parameter" + i )) 
					{
						found = true;
						break;
					}
				}
				if (! found) break;
				i ++;
			}

			MySqlParameter parm = new MySqlParameter("parameter"+i, MySqlDbType.VarChar);
			return parm;
		}

	}
}
