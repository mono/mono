#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Reflection;
using DbLinq.Util;
using System.Collections.Generic;

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    internal class AttributedMetaAssociation : MetaAssociation
    {
        public AttributedMetaAssociation(MemberInfo member, AssociationAttribute attribute, MetaDataMember metaDataMember)
        {
            memberInfo = member;
            associationAttribute = attribute;
            thisMember = metaDataMember;
            Load();
        }

        public virtual void SetOtherKey(string literalOtherKey, MetaTable thisTable, MetaTable otherTable, MetaDataMember otherAssociationMember)
        {
            var comma = new[] { ',' };
            var otherKeysList = new List<MetaDataMember>();
            if (literalOtherKey != null)
            {
                foreach (var otherKeyRaw in literalOtherKey.Split(comma, StringSplitOptions.RemoveEmptyEntries))
                {
                    var otherKey = otherKeyRaw.Trim();
                    //we need to revisit this code - it has caused problems on both MySql and Pgsql
                    MemberInfo[] otherKeyFields = otherTable.RowType.Type.GetMember(otherKey);
                    if (otherKeyFields.Length == 0)
                    {
                        string msg = "ERROR L57 Database contains broken join information."
                            + " thisTable=" + thisTable.TableName
                            + " otherTable=" + otherTable.TableName
                            + " orphanKey=" + literalOtherKey;
                        throw new InvalidOperationException(msg);
                    }
                    var keyMember = otherKeyFields[0];
                    otherKeysList.Add(new AttributedColumnMetaDataMember(keyMember, GetColumnAttribute(keyMember),
                                                                         thisTable.RowType));
                }
            }
            otherKeys = new ReadOnlyCollection<MetaDataMember>(otherKeysList);
            otherMember = otherAssociationMember;
        }

        protected virtual void Load()
        {
            LoadThisKey();
        }

        protected virtual void LoadThisKey()
        {
            var comma = new[] { ',' };
            var thisKeyList = new List<MetaDataMember>();
            if (associationAttribute.ThisKey != null)
            {
                foreach (
                    var thisKeyRaw in associationAttribute.ThisKey.Split(comma, StringSplitOptions.RemoveEmptyEntries))
                {
                    var thisKey = thisKeyRaw.Trim();
                    var keyMember = memberInfo.DeclaringType.GetSingleMember(thisKey);
                    thisKeyList.Add(new AttributedColumnMetaDataMember(keyMember, GetColumnAttribute(keyMember),
                                                                       ThisMember.DeclaringType));
                }
            }
            theseKeys = new ReadOnlyCollection<MetaDataMember>(thisKeyList);
        }

        protected virtual ColumnAttribute GetColumnAttribute(MemberInfo memberInfo)
        {
            return memberInfo.GetAttribute<ColumnAttribute>();
        }

        private AssociationAttribute associationAttribute;
        private MemberInfo memberInfo;

        public override bool DeleteOnNull
        {
            get { return associationAttribute.DeleteOnNull; }
        }

        public override string DeleteRule
        {
            get { return associationAttribute.DeleteRule; }
        }

        public override bool IsForeignKey
        {
            get { return associationAttribute.IsForeignKey; }
        }

        public override bool IsMany
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool IsNullable
        {
            get { return memberInfo.GetMemberType().CanBeNull(); }
        }

        public override bool IsUnique
        {
            get { return associationAttribute.IsUnique; }
        }

        private ReadOnlyCollection<MetaDataMember> otherKeys;
        public override ReadOnlyCollection<MetaDataMember> OtherKey
        {
            get { return otherKeys; }
        }

        public override bool OtherKeyIsPrimaryKey
        {
            get
            {
                foreach (var otherKey in OtherKey)
                {
                    if (!otherKey.IsPrimaryKey)
                        return false;
                }
                return true;
            }
        }

        private MetaDataMember otherMember;
        public override MetaDataMember OtherMember
        {
            get { return otherMember; }
        }

        public override MetaType OtherType
        {
            get { return thisMember.DeclaringType; }
        }

        private ReadOnlyCollection<MetaDataMember> theseKeys;
        public override ReadOnlyCollection<MetaDataMember> ThisKey
        {
            get { return theseKeys; }
        }

        public override bool ThisKeyIsPrimaryKey
        {
            get
            {
                foreach (var thisKey in theseKeys)
                {
                    if (!thisKey.IsPrimaryKey)
                        return false;
                }
                return true;
            }
        }

        private MetaDataMember thisMember;
        public override MetaDataMember ThisMember
        {
            get { return thisMember; }
        }
    }
}