/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.AttributeQualifier.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> Encapsulates a qualifier in a Schema definition.  Definitions that are not
	/// in rfc2252.  Begins with 'X-'
	/// </summary>
	public class AttributeQualifier
	{
		virtual public System.String Name
		{
			/*
			public void addValue( String value )
			{
			values.add( value );
			return;
			}
			*/
			
			get
			{
				return name;
			}
			
		}
		virtual public System.String[] Values
		{
			get
			{
				System.String[] strValues = null;
				if (values.Count > 0)
				{
					strValues = new System.String[values.Count];
					for (int i = 0; i < values.Count; i++)
					{
						strValues[i] = ((System.String) values[i]);
					}
				}
				return strValues;
			}
			
		}
		internal System.String name;
		internal System.Collections.ArrayList values;
		
		public AttributeQualifier(System.String name, System.String[] value_Renamed)
		{
			if ((System.Object) name == null || value_Renamed == null)
			{
				throw new System.ArgumentException("A null name or value " + "was passed in for a schema definition qualifier");
			}
			this.name = name;
			values = new System.Collections.ArrayList(5);
			for (int i = 0; i < value_Renamed.Length; i++)
			{
				values.Add(value_Renamed[i]);
			}
			return ;
		}
	}
}
