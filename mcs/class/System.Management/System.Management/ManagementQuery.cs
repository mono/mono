//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
//
using System;
using System.ComponentModel;
using System.Runtime;

namespace System.Management
{
	[TypeConverter(typeof(ManagementQueryConverter))]
	public abstract class ManagementQuery : ICloneable
	{
		internal readonly static string tokenSelect;

		private string queryLanguage;

		private string queryString;

		internal const string DEFAULTQUERYLANGUAGE = "WQL";

		public virtual string QueryLanguage
		{
			get
			{
				if (this.queryLanguage != null)
				{
					return this.queryLanguage;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				if (this.queryLanguage != value)
				{
					this.queryLanguage = value;
					this.FireIdentifierChanged();
				}
			}
		}

		public virtual string QueryString
		{
			get
			{
				if (this.queryString != null)
				{
					return this.queryString;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				if (this.queryString != value)
				{
					this.ParseQuery(value);
					this.queryString = value;
					this.FireIdentifierChanged();
				}
			}
		}

		static ManagementQuery()
		{
			ManagementQuery.tokenSelect = "select ";
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal ManagementQuery() : this("WQL", null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal ManagementQuery(string query) : this("WQL", query)
		{
		}

		internal ManagementQuery(string language, string query)
		{
			this.QueryLanguage = language;
			this.QueryString = query;
		}

		public abstract object Clone();

		internal void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		protected internal virtual void ParseQuery(string query)
		{
		}

		internal static void ParseToken(ref string q, string token, string op, ref bool bTokenFound, ref string tokenValue)
		{
			if (!bTokenFound)
			{
				bTokenFound = true;
				q = q.Remove(0, token.Length).TrimStart(null);
				if (op != null)
				{
					if (q.IndexOf(op, StringComparison.Ordinal) == 0)
					{
						q = q.Remove(0, op.Length).TrimStart(null);
					}
					else
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"));
					}
				}
				if (q.Length != 0)
				{
					int num = q.IndexOf(' ');
					int length = num;
					if (-1 == num)
					{
						length = q.Length;
					}
					tokenValue = q.Substring(0, length);
					q = q.Remove(0, tokenValue.Length).TrimStart(null);
					return;
				}
				else
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY_NULL_TOKEN"));
				}
			}
			else
			{
				throw new ArgumentException(RC.GetString("INVALID_QUERY_DUP_TOKEN"));
			}
		}

		internal static void ParseToken(ref string q, string token, ref bool bTokenFound)
		{
			if (!bTokenFound)
			{
				bTokenFound = true;
				q = q.Remove(0, token.Length).TrimStart(null);
				return;
			}
			else
			{
				throw new ArgumentException(RC.GetString("INVALID_QUERY_DUP_TOKEN"));
			}
		}

		internal void SetQueryString(string qString)
		{
			this.queryString = qString;
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}