//
// Microsoft.SqlServer.Server.TriggerAction
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Tim Coleman, 2003
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
   
#if NET_2_0

namespace Microsoft.SqlServer.Server {
	public enum TriggerAction
	{
		AlterAppRole = 138,
		AlterAssembly = 102,
		AlterBinding = 175,
		AlterFunction = 62,
		AlterIndex = 25,
		AlterLogin = 145,
		AlterPartitionFunction = 192,
		AlterPartitionScheme = 195,
		AlterProcedure = 52,
		AlterQueue = 158,
		AlterRole = 135,
		AlterRoute = 165,
		AlterSchema = 142,
		AlterService = 162,
		AlterTable = 22,
		AlterTrigger = 72,
		AlterUser = 132,
		AlterView = 42,
		CreateAppRole = 137,
		CreateAssembly = 101,
		CreateBinding = 174,
		CreateContract = 154,
		CreateEventNotification = 74,
		CreateFunction = 61,
		CreateIndex = 24,
		CreateLogin = 144,
		CreateMsgType = 151,
		CreatePartitionFunction = 191,
		CreatePartitionScheme = 194,
		CreateProcedure = 51,
		CreateQueue = 157,
		CreateRole = 134,
		CreateRoute = 164,
		CreateSchema = 141,
		CreateSecurityExpression = 31,
		CreateService = 161,
		CreateSynonym = 34,
		CreateTable = 21,
		CreateTrigger = 71,
		CreateType = 91,
		CreateUser = 131,
		CreateView = 41,
		Delete = 3,
		DenyObject = 171,
		DropAppRole = 139,
		DropAssembly = 103,
		DropBinding = 176,
		DropContract = 156,
		DropEventNotification = 76,
		DropFunction = 63,
		DropIndex = 26,
		DropLogin = 146,
		DropMsgType = 153,
		DropPartitionFunction = 193,
		DropPartitionScheme = 196,
		DropProcedure = 53,
		DropQueue = 159,
		DropRole = 136,
		DropRoute = 166,
		DropSchema = 143,
		DropSecurityExpression = 33,
		DropService = 163,
		DropSynonym = 36,
		DropTable = 23,
		DropTrigger = 73,
		DropType = 93,
		DropUser = 133,
		DropView = 43,
		GrantObject = 170,
		GrantStatement = 167,
		Insert = 1,
		Invalid = 0,
		RevokeObject = 172,
		RevokeStatement = 169,
		Update = 2
	}
}

#endif
