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
    internal abstract class AttributedAbstractMetaDataMember : MetaDataMember
    {
		protected AttributedAbstractMetaDataMember(MemberInfo member, MetaType declaringType, DataAttribute attribute)
		{
			memberInfo = member;
			memberAccessor = LambdaMetaAccessor.Create(member, declaringType.Type);
			this.declaringType = declaringType;
			
			if(attribute.Storage != null)
			{
				storageMember = member.DeclaringType.GetSingleMember(attribute.Storage);
				if (storageMember != null)
					storageAccessor = LambdaMetaAccessor.Create(storageMember, declaringType.Type);
			}
		}

        protected MemberInfo memberInfo;
        protected MetaType declaringType;
		protected MetaAccessor memberAccessor;
		protected MetaAccessor storageAccessor;

        public override MetaType DeclaringType
        {
            get { return declaringType; }
        }

        public override MetaAccessor DeferredSourceAccessor
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaAccessor DeferredValueAccessor
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDeferred
        {
            get { return false; }
        }

        public override bool IsPersistent
        {
            get { return true; }
        }

        public override MethodInfo LoadMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override MemberInfo Member
        {
            get { return memberInfo; }
        }

        public override MetaAccessor MemberAccessor
        {
            get { return memberAccessor; }
        }

        public override string Name
        {
            get { return memberInfo.Name; }
        }

        public override int Ordinal
        {
            get { throw new NotImplementedException(); }
        }

        public override MetaAccessor StorageAccessor
        {
            get { return storageAccessor; }
        }

        public override Type Type
        {
            get { return memberInfo.GetMemberType(); }
        }

        public override UpdateCheck UpdateCheck
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDeclaredBy(MetaType type)
        {
            return type == declaringType;
        }

        protected MemberInfo storageMember;
        public override MemberInfo StorageMember
        {
            get { return storageMember; }
        }

    }
}