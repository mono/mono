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
using System.Data.Linq.Mapping;
using System.Reflection;
using DbLinq.Util;

namespace DbLinq.Data.Linq.Mapping
{
    internal class AttributedAssociationMetaDataMember : AttributedAbstractMetaDataMember
    {
        public AttributedAssociationMetaDataMember(MemberInfo member, AssociationAttribute attribute, MetaType declaringType)
            : base(member, declaringType, attribute)
        {
            associationAttribute = attribute;
        }

        public void SetAssociation(MetaAssociation association)
        {
            metaAssociation = association;
        }

        private AssociationAttribute associationAttribute;
        private MetaAssociation metaAssociation;

        public override MetaAssociation Association
        {
            get { return metaAssociation; }
        }

        public override AutoSync AutoSync
        {
            // TODO: check this is the right value
            get { return AutoSync.Never; }
        }

        public override bool CanBeNull
        {
            get { return false; }
        }

        public override string DbType
        {
            get { return string.Empty; }
        }

        public override string Expression
        {
            get { return string.Empty; }
        }

        public override bool IsAssociation
        {
            get { return true; }
        }

        public override bool IsDbGenerated
        {
            get { return false; }
        }

        public override bool IsDiscriminator
        {
            get { return false; }
        }

        public override bool IsPrimaryKey
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsVersion
        {
            get { throw new NotImplementedException(); }
        }

        public override string MappedName
        {
            get { return associationAttribute.Name ?? Member.Name; }
        }
    }
}