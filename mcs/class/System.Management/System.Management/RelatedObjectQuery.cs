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
	public class RelatedObjectQuery : WqlObjectQuery
	{
		private readonly static string tokenAssociators;

		private readonly static string tokenOf;

		private readonly static string tokenWhere;

		private readonly static string tokenResultClass;

		private readonly static string tokenAssocClass;

		private readonly static string tokenResultRole;

		private readonly static string tokenRole;

		private readonly static string tokenRequiredQualifier;

		private readonly static string tokenRequiredAssocQualifier;

		private readonly static string tokenClassDefsOnly;

		private readonly static string tokenSchemaOnly;

		private bool isSchemaQuery;

		private string sourceObject;

		private string relatedClass;

		private string relationshipClass;

		private string relatedQualifier;

		private string relationshipQualifier;

		private string relatedRole;

		private string thisRole;

		private bool classDefinitionsOnly;

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

		public string RelatedClass
		{
			get
			{
				if (this.relatedClass != null)
				{
					return this.relatedClass;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.relatedClass = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string RelatedQualifier
		{
			get
			{
				if (this.relatedQualifier != null)
				{
					return this.relatedQualifier;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.relatedQualifier = value;
				this.BuildQuery();
				base.FireIdentifierChanged();
			}
		}

		public string RelatedRole
		{
			get
			{
				if (this.relatedRole != null)
				{
					return this.relatedRole;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				this.relatedRole = value;
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

		static RelatedObjectQuery()
		{
			RelatedObjectQuery.tokenAssociators = "associators";
			RelatedObjectQuery.tokenOf = "of";
			RelatedObjectQuery.tokenWhere = "where";
			RelatedObjectQuery.tokenResultClass = "resultclass";
			RelatedObjectQuery.tokenAssocClass = "assocclass";
			RelatedObjectQuery.tokenResultRole = "resultrole";
			RelatedObjectQuery.tokenRole = "role";
			RelatedObjectQuery.tokenRequiredQualifier = "requiredqualifier";
			RelatedObjectQuery.tokenRequiredAssocQualifier = "requiredassocqualifier";
			RelatedObjectQuery.tokenClassDefsOnly = "classdefsonly";
			RelatedObjectQuery.tokenSchemaOnly = "schemaonly";
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public RelatedObjectQuery() : this(null)
		{
		}

		public RelatedObjectQuery(string queryOrSourceObject)
		{
			if (queryOrSourceObject == null)
			{
				return;
			}
			else
			{
				if (!queryOrSourceObject.TrimStart(new char[0]).StartsWith(RelatedObjectQuery.tokenAssociators, StringComparison.OrdinalIgnoreCase))
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
		public RelatedObjectQuery(string sourceObject, string relatedClass) : this(sourceObject, relatedClass, null, null, null, null, null, false)
		{
		}

		public RelatedObjectQuery(string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly)
		{
			this.isSchemaQuery = false;
			this.sourceObject = sourceObject;
			this.relatedClass = relatedClass;
			this.relationshipClass = relationshipClass;
			this.relatedQualifier = relatedQualifier;
			this.relationshipQualifier = relationshipQualifier;
			this.relatedRole = relatedRole;
			this.thisRole = thisRole;
			this.classDefinitionsOnly = classDefinitionsOnly;
			this.BuildQuery();
		}

		public RelatedObjectQuery(bool isSchemaQuery, string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole)
		{
			if (isSchemaQuery)
			{
				this.isSchemaQuery = true;
				this.sourceObject = sourceObject;
				this.relatedClass = relatedClass;
				this.relationshipClass = relationshipClass;
				this.relatedQualifier = relatedQualifier;
				this.relationshipQualifier = relationshipQualifier;
				this.relatedRole = relatedRole;
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
				strArrays[0] = RelatedObjectQuery.tokenAssociators;
				strArrays[1] = " ";
				strArrays[2] = RelatedObjectQuery.tokenOf;
				strArrays[3] = " {";
				strArrays[4] = this.sourceObject;
				strArrays[5] = "}";
				string str = string.Concat(strArrays);
				if (this.RelatedClass.Length != 0 || this.RelationshipClass.Length != 0 || this.RelatedQualifier.Length != 0 || this.RelationshipQualifier.Length != 0 || this.RelatedRole.Length != 0 || this.ThisRole.Length != 0 || this.classDefinitionsOnly || this.isSchemaQuery)
				{
					str = string.Concat(str, " ", RelatedObjectQuery.tokenWhere);
					if (this.RelatedClass.Length != 0)
					{
						string[] strArrays1 = new string[5];
						strArrays1[0] = str;
						strArrays1[1] = " ";
						strArrays1[2] = RelatedObjectQuery.tokenResultClass;
						strArrays1[3] = " = ";
						strArrays1[4] = this.relatedClass;
						str = string.Concat(strArrays1);
					}
					if (this.RelationshipClass.Length != 0)
					{
						string[] strArrays2 = new string[5];
						strArrays2[0] = str;
						strArrays2[1] = " ";
						strArrays2[2] = RelatedObjectQuery.tokenAssocClass;
						strArrays2[3] = " = ";
						strArrays2[4] = this.relationshipClass;
						str = string.Concat(strArrays2);
					}
					if (this.RelatedRole.Length != 0)
					{
						string[] strArrays3 = new string[5];
						strArrays3[0] = str;
						strArrays3[1] = " ";
						strArrays3[2] = RelatedObjectQuery.tokenResultRole;
						strArrays3[3] = " = ";
						strArrays3[4] = this.relatedRole;
						str = string.Concat(strArrays3);
					}
					if (this.ThisRole.Length != 0)
					{
						string[] strArrays4 = new string[5];
						strArrays4[0] = str;
						strArrays4[1] = " ";
						strArrays4[2] = RelatedObjectQuery.tokenRole;
						strArrays4[3] = " = ";
						strArrays4[4] = this.thisRole;
						str = string.Concat(strArrays4);
					}
					if (this.RelatedQualifier.Length != 0)
					{
						string[] strArrays5 = new string[5];
						strArrays5[0] = str;
						strArrays5[1] = " ";
						strArrays5[2] = RelatedObjectQuery.tokenRequiredQualifier;
						strArrays5[3] = " = ";
						strArrays5[4] = this.relatedQualifier;
						str = string.Concat(strArrays5);
					}
					if (this.RelationshipQualifier.Length != 0)
					{
						string[] strArrays6 = new string[5];
						strArrays6[0] = str;
						strArrays6[1] = " ";
						strArrays6[2] = RelatedObjectQuery.tokenRequiredAssocQualifier;
						strArrays6[3] = " = ";
						strArrays6[4] = this.relationshipQualifier;
						str = string.Concat(strArrays6);
					}
					if (this.isSchemaQuery)
					{
						str = string.Concat(str, " ", RelatedObjectQuery.tokenSchemaOnly);
					}
					else
					{
						if (this.classDefinitionsOnly)
						{
							str = string.Concat(str, " ", RelatedObjectQuery.tokenClassDefsOnly);
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
				return new RelatedObjectQuery(true, this.sourceObject, this.relatedClass, this.relationshipClass, this.relatedQualifier, this.relationshipQualifier, this.relatedRole, this.thisRole);
			}
			else
			{
				return new RelatedObjectQuery(this.sourceObject, this.relatedClass, this.relationshipClass, this.relatedQualifier, this.relationshipQualifier, this.relatedRole, this.thisRole, this.classDefinitionsOnly);
			}
		}

		protected internal override void ParseQuery(string query)
		{
			string str = null;
			string str1 = null;
			string str2 = null;
			string str3 = null;
			string str4 = null;
			string str5 = null;
			bool flag = false;
			bool flag1 = false;
			string str6 = query.Trim();
			if (string.Compare(str6, 0, RelatedObjectQuery.tokenAssociators, 0, RelatedObjectQuery.tokenAssociators.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				str6 = str6.Remove(0, RelatedObjectQuery.tokenAssociators.Length);
				if (str6.Length == 0 || !char.IsWhiteSpace(str6[0]))
				{
					throw new ArgumentException(RC.GetString("INVALID_QUERY"));
				}
				else
				{
					str6 = str6.TrimStart(null);
					if (string.Compare(str6, 0, RelatedObjectQuery.tokenOf, 0, RelatedObjectQuery.tokenOf.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						str6 = str6.Remove(0, RelatedObjectQuery.tokenOf.Length).TrimStart(null);
						if (str6.IndexOf('{') == 0)
						{
							str6 = str6.Remove(0, 1).TrimStart(null);
							int num = str6.IndexOf('}');
							int num1 = num;
							if (-1 != num)
							{
								string str7 = str6.Substring(0, num1).TrimEnd(null);
								str6 = str6.Remove(0, num1 + 1).TrimStart(null);
								if (0 < str6.Length)
								{
									if (string.Compare(str6, 0, RelatedObjectQuery.tokenWhere, 0, RelatedObjectQuery.tokenWhere.Length, StringComparison.OrdinalIgnoreCase) == 0)
									{
										str6 = str6.Remove(0, RelatedObjectQuery.tokenWhere.Length);
										if (str6.Length == 0 || !char.IsWhiteSpace(str6[0]))
										{
											throw new ArgumentException(RC.GetString("INVALID_QUERY"));
										}
										else
										{
											str6 = str6.TrimStart(null);
											bool flag2 = false;
											bool flag3 = false;
											bool flag4 = false;
											bool flag5 = false;
											bool flag6 = false;
											bool flag7 = false;
											bool flag8 = false;
											bool flag9 = false;
											while (true)
											{
												if (str6.Length < RelatedObjectQuery.tokenResultClass.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenResultClass, 0, RelatedObjectQuery.tokenResultClass.Length, StringComparison.OrdinalIgnoreCase) != 0)
												{
													if (str6.Length < RelatedObjectQuery.tokenAssocClass.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenAssocClass, 0, RelatedObjectQuery.tokenAssocClass.Length, StringComparison.OrdinalIgnoreCase) != 0)
													{
														if (str6.Length < RelatedObjectQuery.tokenResultRole.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenResultRole, 0, RelatedObjectQuery.tokenResultRole.Length, StringComparison.OrdinalIgnoreCase) != 0)
														{
															if (str6.Length < RelatedObjectQuery.tokenRole.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenRole, 0, RelatedObjectQuery.tokenRole.Length, StringComparison.OrdinalIgnoreCase) != 0)
															{
																if (str6.Length < RelatedObjectQuery.tokenRequiredQualifier.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenRequiredQualifier, 0, RelatedObjectQuery.tokenRequiredQualifier.Length, StringComparison.OrdinalIgnoreCase) != 0)
																{
																	if (str6.Length < RelatedObjectQuery.tokenRequiredAssocQualifier.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenRequiredAssocQualifier, 0, RelatedObjectQuery.tokenRequiredAssocQualifier.Length, StringComparison.OrdinalIgnoreCase) != 0)
																	{
																		if (str6.Length < RelatedObjectQuery.tokenSchemaOnly.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenSchemaOnly, 0, RelatedObjectQuery.tokenSchemaOnly.Length, StringComparison.OrdinalIgnoreCase) != 0)
																		{
																			if (str6.Length < RelatedObjectQuery.tokenClassDefsOnly.Length || string.Compare(str6, 0, RelatedObjectQuery.tokenClassDefsOnly, 0, RelatedObjectQuery.tokenClassDefsOnly.Length, StringComparison.OrdinalIgnoreCase) != 0)
																			{
																				break;
																			}
																			ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenClassDefsOnly, ref flag8);
																			flag = true;
																		}
																		else
																		{
																			ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenSchemaOnly, ref flag9);
																			flag1 = true;
																		}
																	}
																	else
																	{
																		ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenRequiredAssocQualifier, "=", ref flag7, ref str5);
																	}
																}
																else
																{
																	ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenRequiredQualifier, "=", ref flag6, ref str4);
																}
															}
															else
															{
																ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenRole, "=", ref flag5, ref str3);
															}
														}
														else
														{
															ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenResultRole, "=", ref flag4, ref str2);
														}
													}
													else
													{
														ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenAssocClass, "=", ref flag3, ref str1);
													}
												}
												else
												{
													ManagementQuery.ParseToken(ref str6, RelatedObjectQuery.tokenResultClass, "=", ref flag2, ref str);
												}
											}
											if (str6.Length == 0)
											{
												if (flag9 && flag8)
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
								this.sourceObject = str7;
								this.relatedClass = str;
								this.relationshipClass = str1;
								this.relatedRole = str2;
								this.thisRole = str3;
								this.relatedQualifier = str4;
								this.relationshipQualifier = str5;
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
				throw new ArgumentException(RC.GetString("INVALID_QUERY"), "associators");
			}
		}
	}
}