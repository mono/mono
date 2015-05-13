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
using System.Runtime;

namespace System.Management
{
	public class RelationshipQuery : WqlObjectQuery
	{
		private readonly static string tokenReferences;

		private readonly static string tokenOf;

		private readonly static string tokenWhere;

		private readonly static string tokenResultClass;

		private readonly static string tokenRole;

		private readonly static string tokenRequiredQualifier;

		private readonly static string tokenClassDefsOnly;

		private readonly static string tokenSchemaOnly;

		private string sourceObject;

		private string relationshipClass;

		private string relationshipQualifier;

		private string thisRole;

		private bool classDefinitionsOnly;

		private bool isSchemaQuery;

		public bool ClassDefinitionsOnly
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.classDefinitionsOnly;
			}
			set
			{
				this.classDefinitionsOnly = value;
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

		public string RelationshipClass
		{
			get
			{
				if (this.relationshipClass != null)
				{
					return this.relationshipClass;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.relationshipClass = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string RelationshipQualifier
		{
			get
			{
				if (this.relationshipQualifier != null)
				{
					return this.relationshipQualifier;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.relationshipQualifier = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string SourceObject
		{
			get
			{
				if (this.sourceObject != null)
				{
					return this.sourceObject;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.sourceObject = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string ThisRole
		{
			get
			{
				if (this.thisRole != null)
				{
					return this.thisRole;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.thisRole = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		static RelationshipQuery()
		{
			RelationshipQuery.tokenReferences = "references";
			RelationshipQuery.tokenOf = "of";
			RelationshipQuery.tokenWhere = "where";
			RelationshipQuery.tokenResultClass = "resultclass";
			RelationshipQuery.tokenRole = "role";
			RelationshipQuery.tokenRequiredQualifier = "requiredqualifier";
			RelationshipQuery.tokenClassDefsOnly = "classdefsonly";
			RelationshipQuery.tokenSchemaOnly = "schemaonly";
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public RelationshipQuery() : this(null)
		{
		}

		public RelationshipQuery(string queryOrSourceObject)
		{
			if (queryOrSourceObject == null)
			{
				return;
			}
			else
			{
				if (!queryOrSourceObject.TrimStart(new char[0]).StartsWith(RelationshipQuery.tokenReferences, StringComparison.OrdinalIgnoreCase))
				{
					ManagementPath managementPath = new ManagementPath(queryOrSourceObject);
					if ((managementPath.IsClass || managementPath.IsInstance) && managementPath.NamespacePath.Length == 0)
					{
						this.SourceObject = queryOrSourceObject;
						this.isSchemaQuery = false;
						return;
					}
					else
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrSourceObject");
					}
				}
				else
				{
					this.QueryString = queryOrSourceObject;
					return;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public RelationshipQuery(string sourceObject, string relationshipClass) : this(sourceObject, relationshipClass, null, null, false)
		{
		}

		public RelationshipQuery(string sourceObject, string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly)
		{
			this.isSchemaQuery = false;
			this.sourceObject = sourceObject;
			this.relationshipClass = relationshipClass;
			this.relationshipQualifier = relationshipQualifier;
			this.thisRole = thisRole;
			this.classDefinitionsOnly = classDefinitionsOnly;
			this.BuildQuery();
		}

		public RelationshipQuery(bool isSchemaQuery, string sourceObject, string relationshipClass, string relationshipQualifier, string thisRole)
		{
			if (isSchemaQuery)
			{
				this.isSchemaQuery = true;
				this.sourceObject = sourceObject;
				this.relationshipClass = relationshipClass;
				this.relationshipQualifier = relationshipQualifier;
				this.thisRole = thisRole;
				this.classDefinitionsOnly = false;
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
			if (this.sourceObject == null)
			{
				base.SetQueryString(string.Empty);
			}
			if (this.sourceObject == null || this.sourceObject.Length == 0)
			{
				return;
			}
			else
			{
				string[] strArrays = new string[6];
				strArrays[0] = RelationshipQuery.tokenReferences;
				strArrays[1] = " ";
				strArrays[2] = RelationshipQuery.tokenOf;
				strArrays[3] = " {";
				strArrays[4] = this.sourceObject;
				strArrays[5] = "}";
				string str = string.Concat(strArrays);
				if (this.RelationshipClass.Length != 0 || this.RelationshipQualifier.Length != 0 || this.ThisRole.Length != 0 || this.classDefinitionsOnly || this.isSchemaQuery)
				{
					str = string.Concat(str, " ", RelationshipQuery.tokenWhere);
					if (this.RelationshipClass.Length != 0)
					{
						string[] strArrays1 = new string[5];
						strArrays1[0] = str;
						strArrays1[1] = " ";
						strArrays1[2] = RelationshipQuery.tokenResultClass;
						strArrays1[3] = " = ";
						strArrays1[4] = this.relationshipClass;
						str = string.Concat(strArrays1);
					}
					if (this.ThisRole.Length != 0)
					{
						string[] strArrays2 = new string[5];
						strArrays2[0] = str;
						strArrays2[1] = " ";
						strArrays2[2] = RelationshipQuery.tokenRole;
						strArrays2[3] = " = ";
						strArrays2[4] = this.thisRole;
						str = string.Concat(strArrays2);
					}
					if (this.RelationshipQualifier.Length != 0)
					{
						string[] strArrays3 = new string[5];
						strArrays3[0] = str;
						strArrays3[1] = " ";
						strArrays3[2] = RelationshipQuery.tokenRequiredQualifier;
						strArrays3[3] = " = ";
						strArrays3[4] = this.relationshipQualifier;
						str = string.Concat(strArrays3);
					}
					if (this.isSchemaQuery)
					{
						str = string.Concat(str, " ", RelationshipQuery.tokenSchemaOnly);
					}
					else
					{
						if (this.classDefinitionsOnly)
						{
							str = string.Concat(str, " ", RelationshipQuery.tokenClassDefsOnly);
						}
					}
				}
				base.SetQueryString(str);
				return;
			}
		}

		public override object Clone()
		{
			if (this.isSchemaQuery)
			{
				return new RelationshipQuery(true, this.sourceObject, this.relationshipClass, this.relationshipQualifier, this.thisRole);
			}
			else
			{
				return new RelationshipQuery(this.sourceObject, this.relationshipClass, this.relationshipQualifier, this.thisRole, this.classDefinitionsOnly);
			}
		}

		protected internal override void ParseQuery(string query)
		{
			string str = null;
			string str1 = null;
			string str2 = null;
			bool flag = false;
			bool flag1 = false;
			string str3 = query.Trim();
			if (string.Compare(str3, 0, RelationshipQuery.tokenReferences, 0, RelationshipQuery.tokenReferences.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				str3 = str3.Remove(0, RelationshipQuery.tokenReferences.Length);
				if (str3.Length == 0 || !char.IsWhiteSpace(str3[0]))
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY"));
				}
				else
				{
					str3 = str3.TrimStart(null);
					if (string.Compare(str3, 0, RelationshipQuery.tokenOf, 0, RelationshipQuery.tokenOf.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						str3 = str3.Remove(0, RelationshipQuery.tokenOf.Length).TrimStart(null);
						if (str3.IndexOf('{') == 0)
						{
							str3 = str3.Remove(0, 1).TrimStart(null);
							int num = str3.IndexOf('}');
							int num1 = num;
							if (-1 != num)
							{
								string str4 = str3.Substring(0, num1).TrimEnd(null);
								str3 = str3.Remove(0, num1 + 1).TrimStart(null);
								if (0 < str3.Length)
								{
									if (string.Compare(str3, 0, RelationshipQuery.tokenWhere, 0, RelationshipQuery.tokenWhere.Length, StringComparison.OrdinalIgnoreCase) == 0)
									{
										str3 = str3.Remove(0, RelationshipQuery.tokenWhere.Length);
										if (str3.Length == 0 || !char.IsWhiteSpace(str3[0]))
										{
											throw new ArgumentException(RC.GetString("INVALID_QUERY"));
										}
										else
										{
											str3 = str3.TrimStart(null);
											bool flag2 = false;
											bool flag3 = false;
											bool flag4 = false;
											bool flag5 = false;
											bool flag6 = false;
											while (true)
											{
												if (str3.Length < RelationshipQuery.tokenResultClass.Length || string.Compare(str3, 0, RelationshipQuery.tokenResultClass, 0, RelationshipQuery.tokenResultClass.Length, StringComparison.OrdinalIgnoreCase) != 0)
												{
													if (str3.Length < RelationshipQuery.tokenRole.Length || string.Compare(str3, 0, RelationshipQuery.tokenRole, 0, RelationshipQuery.tokenRole.Length, StringComparison.OrdinalIgnoreCase) != 0)
													{
														if (str3.Length < RelationshipQuery.tokenRequiredQualifier.Length || string.Compare(str3, 0, RelationshipQuery.tokenRequiredQualifier, 0, RelationshipQuery.tokenRequiredQualifier.Length, StringComparison.OrdinalIgnoreCase) != 0)
														{
															if (str3.Length < RelationshipQuery.tokenClassDefsOnly.Length || string.Compare(str3, 0, RelationshipQuery.tokenClassDefsOnly, 0, RelationshipQuery.tokenClassDefsOnly.Length, StringComparison.OrdinalIgnoreCase) != 0)
															{
																if (str3.Length < RelationshipQuery.tokenSchemaOnly.Length || string.Compare(str3, 0, RelationshipQuery.tokenSchemaOnly, 0, RelationshipQuery.tokenSchemaOnly.Length, StringComparison.OrdinalIgnoreCase) != 0)
																{
																	break;
																}
																ManagementQuery.ParseToken(ref str3, RelationshipQuery.tokenSchemaOnly, ref flag6);
																flag1 = true;
															}
															else
															{
																ManagementQuery.ParseToken(ref str3, RelationshipQuery.tokenClassDefsOnly, ref flag5);
																flag = true;
															}
														}
														else
														{
															ManagementQuery.ParseToken(ref str3, RelationshipQuery.tokenRequiredQualifier, "=", ref flag4, ref str2);
														}
													}
													else
													{
														ManagementQuery.ParseToken(ref str3, RelationshipQuery.tokenRole, "=", ref flag3, ref str1);
													}
												}
												else
												{
													ManagementQuery.ParseToken(ref str3, RelationshipQuery.tokenResultClass, "=", ref flag2, ref str);
												}
											}
											if (str3.Length == 0)
											{
												if (flag && flag1)
												{
													throw new ArgumentException(RC.GetString("INVALID_QUERY"));
												}
											}
											else
											{
												throw new ArgumentException(RC.GetString("INVALID_QUERY"));
											}
										}
									}
									else
									{
										throw new ArgumentException(RC.GetString("INVALID_QUERY"), "where");
									}
								}
								this.sourceObject = str4;
								this.relationshipClass = str;
								this.thisRole = str1;
								this.relationshipQualifier = str2;
								this.classDefinitionsOnly = flag;
								this.isSchemaQuery = flag1;
								return;
							}
							else
							{
								throw new ArgumentException(RC.GetString("INVALID_QUERY"));
							}
						}
						else
						{
							throw new ArgumentException(RC.GetString("INVALID_QUERY"));
						}
					}
					else
					{
						throw new ArgumentException(RC.GetString("INVALID_QUERY"), "of");
					}
				}
			}
			else
			{
				throw new ArgumentException(RC.GetString("INVALID_QUERY"), "references");
			}
		}
	}
}