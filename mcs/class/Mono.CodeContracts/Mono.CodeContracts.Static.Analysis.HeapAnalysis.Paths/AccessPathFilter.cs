// 
// AccessPathFilter.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths {
	class AccessPathFilter<TMember> {
		public static AccessPathFilter<TMember> NoFilter = new AccessPathFilter<TMember> ();
		private readonly Flags flags;
		private readonly TMember member;
		private readonly MemberFilter member_filter;

		private AccessPathFilter ()
		{
			this.flags = Flags.AllowLocals;
			this.member_filter = MemberFilter.NoFilter;
		}

		private AccessPathFilter (TMember member, MemberFilter memberFilter)
		{
			this.member_filter = memberFilter;
			this.member = member;
		}

		public bool AllowLocal
		{
			get { return (this.flags & Flags.AllowLocals) == Flags.AllowLocals; }
		}

		public bool HasVisibilityMember
		{
			get { return this.member_filter != MemberFilter.NoFilter; }
		}

		public TMember VisibilityMember
		{
			get { return this.member; }
		}

		public static AccessPathFilter<TMember> FromPrecondition (TMember member)
		{
			return new AccessPathFilter<TMember> (member, MemberFilter.FromPrecondition);
		}

		public static AccessPathFilter<TMember> FromPostcondition (TMember member)
		{
			return new AccessPathFilter<TMember> (member, MemberFilter.FromPostcondition);
		}

		public static AccessPathFilter<TMember> IsVisibleFrom (TMember member)
		{
			return new AccessPathFilter<TMember> (member, MemberFilter.FromMethodBody);
		}

		public bool FilterOutPathElement<P> (P element)
			where P : IVisibilityCheck<TMember>
		{
			switch (this.member_filter) {
			case MemberFilter.FromPrecondition:
				return !element.IsAsVisibleAs (this.member);
			case MemberFilter.FromPostcondition:
				return !element.IfRootIsParameter || !element.IsVisibleFrom (this.member);
			case MemberFilter.FromMethodBody:
				return !element.IsVisibleFrom (this.member);
			default:
				return false;
			}
		}

		#region Nested type: Flags
		[Flags]
		private enum Flags {
			AllowLocals = 1,
			RequireParameter = 2
		}
		#endregion

		#region Nested type: MemberFilter
		private enum MemberFilter {
			NoFilter = 0,
			FromPrecondition,
			FromPostcondition,
			FromMethodBody
		}
		#endregion
	}
}
