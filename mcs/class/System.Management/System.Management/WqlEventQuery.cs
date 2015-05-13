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
using System.Globalization;
using System.Runtime;

namespace System.Management
{
	public class WqlEventQuery : EventQuery
	{
		private readonly static string tokenSelectAll;

		private string eventClassName;

		private TimeSpan withinInterval;

		private string condition;

		private TimeSpan groupWithinInterval;

		private StringCollection groupByPropertyList;

		private string havingCondition;

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
			}
		}

		public string EventClassName
		{
			get
			{
				if (this.eventClassName != null)
				{
					return this.eventClassName;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.eventClassName = value;
				this.BuildQuery();
			}
		}

		public StringCollection GroupByPropertyList
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.groupByPropertyList;
			}
			set
			{
				StringCollection stringCollections = value;
				StringCollection stringCollections1 = new StringCollection();
				foreach (string str in stringCollections)
				{
					stringCollections1.Add(str);
				}
				this.groupByPropertyList = stringCollections1;
				this.BuildQuery();
			}
		}

		public TimeSpan GroupWithinInterval
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.groupWithinInterval;
			}
			set
			{
				this.groupWithinInterval = value;
				this.BuildQuery();
			}
		}

		public string HavingCondition
		{
			get
			{
				if (this.havingCondition != null)
				{
					return this.havingCondition;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.havingCondition = value;
				this.BuildQuery();
			}
		}

		public override string QueryLanguage
		{
			get
			{
				return base.QueryLanguage;
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

		public TimeSpan WithinInterval
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.withinInterval;
			}
			set
			{
				this.withinInterval = value;
				this.BuildQuery();
			}
		}

		static WqlEventQuery()
		{
			WqlEventQuery.tokenSelectAll = "select * ";
		}

		public WqlEventQuery() : this(null, TimeSpan.Zero, null, TimeSpan.Zero, null, null)
		{
		}

		public WqlEventQuery(string queryOrEventClassName)
		{
			this.groupByPropertyList = new StringCollection();
			if (queryOrEventClassName == null)
			{
				return;
			}
			else
			{
				if (!queryOrEventClassName.TrimStart(new char[0]).StartsWith(WqlEventQuery.tokenSelectAll, StringComparison.OrdinalIgnoreCase))
				{
					ManagementPath managementPath = new ManagementPath(queryOrEventClassName);
					if (!managementPath.IsClass || managementPath.NamespacePath.Length != 0)
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrEventClassName");
					}
					else
					{
						this.EventClassName = queryOrEventClassName;
						return;
					}
				}
				else
				{
					this.QueryString = queryOrEventClassName;
					return;
				}
			}
		}

		public WqlEventQuery(string eventClassName, string condition) : this(eventClassName, TimeSpan.Zero, condition, TimeSpan.Zero, null, null)
		{
		}

		public WqlEventQuery(string eventClassName, TimeSpan withinInterval) : this(eventClassName, withinInterval, null, TimeSpan.Zero, null, null)
		{
		}

		public WqlEventQuery(string eventClassName, TimeSpan withinInterval, string condition) : this(eventClassName, withinInterval, condition, TimeSpan.Zero, null, null)
		{
		}

		public WqlEventQuery(string eventClassName, string condition, TimeSpan groupWithinInterval) : this(eventClassName, TimeSpan.Zero, condition, groupWithinInterval, null, null)
		{
		}

		public WqlEventQuery(string eventClassName, string condition, TimeSpan groupWithinInterval, string[] groupByPropertyList) : this(eventClassName, TimeSpan.Zero, condition, groupWithinInterval, groupByPropertyList, null)
		{
		}

		public WqlEventQuery(string eventClassName, TimeSpan withinInterval, string condition, TimeSpan groupWithinInterval, string[] groupByPropertyList, string havingCondition)
		{
			this.eventClassName = eventClassName;
			this.withinInterval = withinInterval;
			this.condition = condition;
			this.groupWithinInterval = groupWithinInterval;
			this.groupByPropertyList = new StringCollection();
			if (groupByPropertyList != null)
			{
				this.groupByPropertyList.AddRange(groupByPropertyList);
			}
			this.havingCondition = havingCondition;
			this.BuildQuery();
		}

		protected internal void BuildQuery()
		{
			string str;
			if (this.eventClassName == null || this.eventClassName.Length == 0)
			{
				base.SetQueryString(string.Empty);
				return;
			}
			else
			{
				string str1 = WqlEventQuery.tokenSelectAll;
				str1 = string.Concat(str1, "from ", this.eventClassName);
				if (this.withinInterval != TimeSpan.Zero)
				{
					double totalSeconds = this.withinInterval.TotalSeconds;
					str1 = string.Concat(str1, " within ", totalSeconds.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(double))));
				}
				if (this.Condition.Length != 0)
				{
					str1 = string.Concat(str1, " where ", this.condition);
				}
				if (this.groupWithinInterval != TimeSpan.Zero)
				{
					double num = this.groupWithinInterval.TotalSeconds;
					str1 = string.Concat(str1, " group within ", num.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(double))));
					if (this.groupByPropertyList != null && 0 < this.groupByPropertyList.Count)
					{
						int count = this.groupByPropertyList.Count;
						str1 = string.Concat(str1, " by ");
						for (int i = 0; i < count; i++)
						{
							string str2 = str1;
							string item = this.groupByPropertyList[i];
							if (i == count - 1)
							{
								str = "";
							}
							else
							{
								str = ",";
							}
							str1 = string.Concat(str2, item, str);
						}
					}
					if (this.HavingCondition.Length != 0)
					{
						str1 = string.Concat(str1, " having ", this.havingCondition);
					}
				}
				base.SetQueryString(str1);
				return;
			}
		}

		public override object Clone()
		{
			string[] strArrays = null;
			if (this.groupByPropertyList != null)
			{
				int count = this.groupByPropertyList.Count;
				if (0 < count)
				{
					strArrays = new string[count];
					this.groupByPropertyList.CopyTo(strArrays, 0);
				}
			}
			return new WqlEventQuery(this.eventClassName, this.withinInterval, this.condition, this.groupWithinInterval, strArrays, this.havingCondition);
		}

		protected internal override void ParseQuery(string query)
		{
			string str;
			string str1;
			this.eventClassName = null;
			this.withinInterval = TimeSpan.Zero;
			this.condition = null;
			this.groupWithinInterval = TimeSpan.Zero;
			if (this.groupByPropertyList != null)
			{
				this.groupByPropertyList.Clear();
			}
			this.havingCondition = null;
			string str2 = query.Trim();
			bool flag = false;
			string str3 = ManagementQuery.tokenSelect;
			if (str2.Length < str3.Length || string.Compare(str2, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new ArgumentException(RC.GetString("INVALID_QUERY"));
			}
			else
			{
				str2 = str2.Remove(0, str3.Length).TrimStart(null);
				if (str2.StartsWith("*", StringComparison.Ordinal))
				{
					str2 = str2.Remove(0, 1).TrimStart(null);
					str3 = "from ";
					if (str2.Length < str3.Length || string.Compare(str2, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) != 0)
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "from");
					}
					else
					{
						ManagementQuery.ParseToken(ref str2, str3, null, ref flag, ref this.eventClassName);
						str3 = "within ";
						if (str2.Length >= str3.Length && string.Compare(str2, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
						{
							string str4 = null;
							flag = false;
							ManagementQuery.ParseToken(ref str2, str3, null, ref flag, ref str4);
							this.withinInterval = TimeSpan.FromSeconds(((IConvertible)str4).ToDouble(null));
						}
						str3 = "group within ";
						if (str2.Length >= str3.Length)
						{
							int num = str2.ToLower(CultureInfo.InvariantCulture).IndexOf(str3, StringComparison.Ordinal);
							int num1 = num;
							if (num == -1)
							{
								str = str2.Trim();
								str3 = "where ";
								if (str.Length >= str3.Length && string.Compare(str, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
								{
									this.condition = str.Substring(str3.Length);
								}
								return;
							}
							str = str2.Substring(0, num1).Trim();
							str2 = str2.Remove(0, num1);
							string str5 = null;
							flag = false;
							ManagementQuery.ParseToken(ref str2, str3, null, ref flag, ref str5);
							this.groupWithinInterval = TimeSpan.FromSeconds(((IConvertible)str5).ToDouble(null));
							str3 = "by ";
							if (str2.Length >= str3.Length && string.Compare(str2, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
							{
								str2 = str2.Remove(0, str3.Length);
								if (this.groupByPropertyList == null)
								{
									this.groupByPropertyList = new StringCollection();
								}
								else
								{
									this.groupByPropertyList.Clear();
								}
								while (true)
								{
									int num2 = str2.IndexOf(',');
									num1 = num2;
									if (num2 <= 0)
									{
										break;
									}
									str1 = str2.Substring(0, num1);
									str2 = str2.Remove(0, num1 + 1).TrimStart(null);
									str1 = str1.Trim();
									if (str1.Length > 0)
									{
										this.groupByPropertyList.Add(str1);
									}
								}
								int num3 = str2.IndexOf(' ');
								num1 = num3;
								if (num3 <= 0)
								{
									this.groupByPropertyList.Add(str2);
									return;
								}
								else
								{
									str1 = str2.Substring(0, num1);
									str2 = str2.Remove(0, num1).TrimStart(null);
									this.groupByPropertyList.Add(str1);
								}
							}
							str3 = "having ";
							if (str2.Length >= str3.Length && string.Compare(str2, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
							{
								str2 = str2.Remove(0, str3.Length);
								if (str2.Length != 0)
								{
									this.havingCondition = str2;
									str3 = "where ";
									if (str.Length >= str3.Length && string.Compare(str, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
									{
										this.condition = str.Substring(str3.Length);
									}
									return;
								}
								else
								{
									throw new ArgumentException(RC.GetString("INVALID_QUERY"), "having");
								}
							}
							else
							{
								str3 = "where ";
								if (str.Length >= str3.Length && string.Compare(str, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
								{
									this.condition = str.Substring(str3.Length);
								}
								return;
							}
						}
						str = str2.Trim();
						str3 = "where ";
						if (str.Length >= str3.Length && string.Compare(str, 0, str3, 0, str3.Length, StringComparison.OrdinalIgnoreCase) == 0)
						{
							this.condition = str.Substring(str3.Length);
						}
						return;
					}
				}
				else
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY"), "*");
				}
			}
		}
	}
}