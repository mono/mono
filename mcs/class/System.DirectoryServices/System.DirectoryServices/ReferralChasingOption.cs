/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.ReferralChasingOption.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

namespace System.DirectoryServices
{
	
	/// <summary>
	/// Specifies if and how referral chasing is pursued.
	/// </summary>
	/// <remarks>
	/// When a server determines that other servers hold relevant information, 
	/// in part or as a whole, it may refer the client to another server to 
	/// obtain the result. Referral chasing is the action taken by a client 
	/// to contact the referenced server to continue the directory search.	
	/// 
	/// Use the constants of this enumeration to set up search preferences for 
	/// referral chasing. The action amounts to assigning the appropriate 
	/// fields of DirectorySearcher to elements of the ReferralChasingOption 
	/// enumeration.
	/// 
	/// The  Lightweight Directory Access Protocol (Ldap) provider supports 
	/// external referrals for paged searches, but does not support 
	/// subordinate referrals during paging.
	/// </remarks>
	[Serializable]
	public enum ReferralChasingOption
	{
		All = 96,
		External = 64,
		None = 0,
		Subordinate = 32
	}
}

