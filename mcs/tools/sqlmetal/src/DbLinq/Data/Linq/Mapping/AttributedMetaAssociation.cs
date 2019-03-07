#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Stefan Klinger
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
using System.Linq;
using System.Reflection;
using DbLinq.Util;
using System.Collections.Generic;

namespace DbLinq.Data.Linq.Mapping
{
    internal class AttributedMetaAssociation : MetaAssociation
    {
		//Seperator used for key lists
		private static readonly char[] STRING_SEPERATOR =  new[] { ',' };

		private static string AttributeNameNullCheck (AssociationAttribute attribute)
		{
			if ( attribute == null )
				return null;
			else
				return attribute.Name;
		}

        public AttributedMetaAssociation(MemberInfo member, AssociationAttribute attribute, MetaDataMember metaDataMember)
        {
            _memberInfo = member;
            _associationAttribute = attribute;
            _thisMember = metaDataMember;
			// The bug described here:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=376669
			// says that under certain conditions, there's a bug where _otherMember == _thisMember
			// Not only is there no point in reproducing a MS bug, it's not as simple as simply setting _otherMember = metaDataMember
			Type otherType = _memberInfo.GetFirstInnerReturnType();
			string associationName = member.GetAttribute<AssociationAttribute>().Name;
			AttributedMetaType ownedMetaType = metaDataMember.DeclaringType.Model.GetMetaType(otherType) as AttributedMetaType;

			if ( ownedMetaType == null )
				throw new InvalidOperationException("Key in referenced table is of a different SQL MetaData provider");

			_otherMember = ownedMetaType.AssociationsLookup[otherType.GetMembers().Where(m => (AttributeNameNullCheck(m.GetAttribute<AssociationAttribute>()) == associationName) && (m != member)).Single()];
        }

		/// <summary>
		/// Returns a list of keys from the given meta type based on the key list string.
		/// </summary>
		/// <param name="keyListString">The key list string.</param>
		/// <param name="parentType">Type of the parent.</param>
		/// <returns></returns>
		private static ReadOnlyCollection<MetaDataMember> GetKeys(string keyListString, MetaType parentType)
		{
			if(keyListString != null)
			{
				var thisKeyList = new List<MetaDataMember>();

				string[] keyNames = keyListString.Split(STRING_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);

				foreach (string rawKeyName in keyNames)
				{
					string keyName = rawKeyName.Trim();

					//TODO: maybe speed the lookup up
					MetaDataMember key = (from dataMember in parentType.PersistentDataMembers
					             where dataMember.Name == keyName
					             select dataMember).SingleOrDefault();

					if(key == null)
					{
						string errorMessage = string.Format("Could not find key member '{0}' of key '{1}' on type '{2}'. The key may be wrong or the field or property on '{2}' has changed names.",
							keyName, keyListString, parentType.Type.Name);

						throw new InvalidOperationException(errorMessage);
					}

					thisKeyList.Add(key);
				}

				return new ReadOnlyCollection<MetaDataMember>(thisKeyList);
			}
			else //Key is the primary key of this table
			{
				return parentType.IdentityMembers;
			}
		}

        private AssociationAttribute _associationAttribute;
        private MemberInfo _memberInfo;

        public override bool DeleteOnNull
        {
            get { return _associationAttribute.DeleteOnNull; }
        }

        public override string DeleteRule
        {
            get { return _associationAttribute.DeleteRule; }
        }

        public override bool IsForeignKey
        {
            get { return _associationAttribute.IsForeignKey; }
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
            get { return _memberInfo.GetMemberType().CanBeNull(); }
        }

        public override bool IsUnique
        {
            get { return _associationAttribute.IsUnique; }
        }

        private ReadOnlyCollection<MetaDataMember> _otherKeys;
        public override ReadOnlyCollection<MetaDataMember> OtherKey
        {
            get {
                if (_otherKeys == null)
                {
                    //Get the association target type
                    var targetType = _memberInfo.GetFirstInnerReturnType();

                    var otherTable = ThisMember.DeclaringType.Model.GetTable(targetType);

                    //Setup other key
                    _otherKeys = GetKeys(_associationAttribute.OtherKey, otherTable.RowType);
                }
                return _otherKeys;
            }
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

        private MetaDataMember _otherMember;
        public override MetaDataMember OtherMember
        {
            get { return _otherMember; }
        }

        public override MetaType OtherType
        {
            get { return _otherMember.DeclaringType; }
        }

        private ReadOnlyCollection<MetaDataMember> _thisKey;
        public override ReadOnlyCollection<MetaDataMember> ThisKey
        {
            get {
                if (_thisKey == null)
                    _thisKey = GetKeys(_associationAttribute.ThisKey, ThisMember.DeclaringType);
                return _thisKey;
            }
        }

        public override bool ThisKeyIsPrimaryKey
        {
            get
            {
                foreach (var thisKey in _thisKey)
                {
                    if (!thisKey.IsPrimaryKey)
                        return false;
                }
                return true;
            }
        }

        private MetaDataMember _thisMember;
        public override MetaDataMember ThisMember
        {
            get { return _thisMember; }
        }
    }
}