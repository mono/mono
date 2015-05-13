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
using System.Collections.Specialized;
using System.Runtime;

namespace System.Management
{
	public class SelectQuery : WqlObjectQuery
	{
		private bool isSchemaQuery;

		private string className;

		private string condition;

		private StringCollection selectedProperties;

		public string ClassName
		{
			get
			{
				if (this.className != null)
				{
					return this.className;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.className = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string Condition
		{
			get
			{
				if (this.condition != null)
				{
					return this.condition;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.condition = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public bool IsSchemaQuery
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.isSchemaQuery;
			}
			set
			{
				this.isSchemaQuery = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public override string QueryString
		{
			get
			{
				this.BuildQuery();
				return base.QueryString;
			}
			set
			{
				base.QueryString = value;
			}
		}

		public StringCollection SelectedProperties
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.selectedProperties;
			}
			set
			{
				if (value == null)
				{
					this.selectedProperties = new StringCollection();
				}
				else
				{
					StringCollection stringCollections = value;
					StringCollection stringCollections1 = new StringCollection();
					foreach (string str in stringCollections)
					{
						stringCollections1.Add(str);
					}
					this.selectedProperties = stringCollections1;
				}
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SelectQuery() : this(null)
		{
		}

		public SelectQuery(string queryOrClassName)
		{
			this.selectedProperties = new StringCollection();
			if (queryOrClassName == null)
			{
				return;
			}
			else
			{
				if (!queryOrClassName.TrimStart(new char[0]).StartsWith(ManagementQuery.tokenSelect, StringComparison.OrdinalIgnoreCase))
				{
					ManagementPath managementPath = new ManagementPath(queryOrClassName);
					if (!managementPath.IsClass || managementPath.NamespacePath.Length != 0)
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrClassName");
					}
					else
					{
						this.ClassName = queryOrClassName;
						return;
					}
				}
				else
				{
					this.QueryString = queryOrClassName;
					return;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SelectQuery(string className, string condition) : this(className, condition, null)
		{
		}

		public SelectQuery(string className, string condition, string[] selectedProperties)
		{
			this.isSchemaQuery = false;
			this.className = className;
			this.condition = condition;
			this.selectedProperties = new StringCollection();
			if (selectedProperties != null)
			{
				this.selectedProperties.AddRange(selectedProperties);
			}
			this.BuildQuery();
		}

		public SelectQuery(bool isSchemaQuery, string condition)
		{
			if (isSchemaQuery)
			{
				this.isSchemaQuery = true;
				this.className = null;
				this.condition = condition;
				this.selectedProperties = null;
				this.BuildQuery();
				return;
			}
			else
			{
				throw new ArgumentException(RC.GetString("INVALID_QUERY"), "isSchemaQuery");
			}
		}

		protected internal void BuildQuery()
		{
			string str;
			string str1;
			if (this.isSchemaQuery)
			{
				str = "select * from meta_class";
			}
			else
			{
				if (this.className == null)
				{
					base.SetQueryString(string.Empty);
				}
				if (this.className == null || this.className.Length == 0)
				{
					return;
				}
				else
				{
					str = ManagementQuery.tokenSelect;
					if (this.selectedProperties == null || 0 >= this.selectedProperties.Count)
					{
						str = string.Concat(str, "* ");
					}
					else
					{
						int count = this.selectedProperties.Count;
						for (int i = 0; i < count; i++)
						{
							string str2 = str;
							string item = this.selectedProperties[i];
							if (i == count - 1)
							{
								str1 = " ";
							}
							else
							{
								str1 = ",";
							}
							str = string.Concat(str2, item, str1);
						}
					}
					str = string.Concat(str, "from ", this.className);
				}
			}
			if (this.Condition != null && this.Condition.Length != 0)
			{
				str = string.Concat(str, " where ", this.condition);
			}
			base.SetQueryString(str);
		}

		public override object Clone()
		{
			string[] strArrays = null;
			if (this.selectedProperties != null)
			{
				int count = this.selectedProperties.Count;
				if (0 < count)
				{
					strArrays = new string[count];
					this.selectedProperties.CopyTo(strArrays, 0);
				}
			}
			if (this.isSchemaQuery)
			{
				return new SelectQuery(true, this.condition);
			}
			else
			{
				return new SelectQuery(this.className, this.condition, strArrays);
			}
		}

		protected internal override void ParseQuery(string query)
		{
			string str;
			int num;
			this.className = null;
			this.condition = null;
			if (this.selectedProperties != null)
			{
				this.selectedProperties.Clear();
			}
			string str1 = query.Trim();
			bool flag = false;
			if (this.isSchemaQuery)
			{
				string str2 = "select";
				if (str1.Length < str2.Length || string.Compare(str1, 0, str2, 0, str2.Length, StringComparison.OrdinalIgnoreCase) != 0)
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY"), "select");
				}
				else
				{
					str1 = str1.Remove(0, str2.Length).TrimStart(null);
					if (str1.IndexOf('*', 0) == 0)
					{
						str1 = str1.Remove(0, 1).TrimStart(null);
						str2 = "from";
						if (str1.Length < str2.Length || string.Compare(str1, 0, str2, 0, str2.Length, StringComparison.OrdinalIgnoreCase) != 0)
						{
							throw new ArgumentException(RC.GetString("INVALID_QUERY"), "from");
						}
						else
						{
							str1 = str1.Remove(0, str2.Length).TrimStart(null);
							str2 = "meta_class";
							if (str1.Length < str2.Length || string.Compare(str1, 0, str2, 0, str2.Length, StringComparison.OrdinalIgnoreCase) != 0)
							{
								throw new ArgumentException(RC.GetString("INVALID_QUERY"), "meta_class");
							}
							else
							{
								str1 = str1.Remove(0, str2.Length).TrimStart(null);
								if (0 >= str1.Length)
								{
									this.condition = string.Empty;
								}
								else
								{
									str2 = "where";
									if (str1.Length < str2.Length || string.Compare(str1, 0, str2, 0, str2.Length, StringComparison.OrdinalIgnoreCase) != 0)
									{
										throw new ArgumentException(RC.GetString("INVALID_QUERY"), "where");
									}
									else
									{
										str1 = str1.Remove(0, str2.Length);
										if (str1.Length == 0 || !char.IsWhiteSpace(str1[0]))
										{
											throw new ArgumentException(RC.GetString("INVALID_QUERY"));
										}
										else
										{
											str1 = str1.TrimStart(null);
											this.condition = str1;
										}
									}
								}
								this.className = null;
								this.selectedProperties = null;
							}
						}
					}
					else
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "*");
					}
				}
			}
			else
			{
				string str3 = ManagementQuery.tokenSelect;
				if (str1.Length < str3.Length || string.Compare(str1, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) != 0)
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY"));
				}
				else
				{
					ManagementQuery.ParseToken(ref str1, str3, ref flag);
					if (str1[0] == '*')
					{
						str1 = str1.Remove(0, 1).TrimStart(null);
					}
					else
					{
						if (this.selectedProperties == null)
						{
							this.selectedProperties = new StringCollection();
						}
						else
						{
							this.selectedProperties.Clear();
						}
						while (true)
						{
							int num1 = str1.IndexOf(',');
							num = num1;
							if (num1 <= 0)
							{
								break;
							}
							str = str1.Substring(0, num);
							str1 = str1.Remove(0, num + 1).TrimStart(null);
							str = str.Trim();
							if (str.Length > 0)
							{
								this.selectedProperties.Add(str);
							}
						}
						int num2 = str1.IndexOf(' ');
						num = num2;
						if (num2 <= 0)
						{
							throw new ArgumentException(RC.GetString("INVALID_QUERY"));
						}
						else
						{
							str = str1.Substring(0, num);
							str1 = str1.Remove(0, num).TrimStart(null);
							this.selectedProperties.Add(str);
						}
					}
					str3 = "from ";
					flag = false;
					if (str1.Length < str3.Length || string.Compare(str1, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) != 0)
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"));
					}
					else
					{
						ManagementQuery.ParseToken(ref str1, str3, null, ref flag, ref this.className);
						str3 = "where ";
						if (str1.Length >= str3.Length && string.Compare(str1, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
						{
							this.condition = str1.Substring(str3.Length).Trim();
							return;
						}
					}
				}
			}
		}
	}
}