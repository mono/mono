/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Utilclass.ResultCodeMessages.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> This class contains strings corresponding to Ldap Result Codes.
	/// The resources are accessed by the String representation of the result code.
	/// </summary>
	
	public class ResultCodeMessages:System.Resources.ResourceManager
	{
		public System.Object[][] getContents()
		{
			return contents;
		}
		internal static readonly System.Object[][] contents = {new System.Object[]{"0", "Success"}, new System.Object[]{"1", "Operations Error"}, new System.Object[]{"2", "Protocol Error"}, new System.Object[]{"3", "Timelimit Exceeded"}, new System.Object[]{"4", "Sizelimit Exceeded"}, new System.Object[]{"5", "Compare False"}, new System.Object[]{"6", "Compare True"}, new System.Object[]{"7", "Authentication Method Not Supported"}, new System.Object[]{"8", "Strong Authentication Required"}, new System.Object[]{"9", "Partial Results"}, new System.Object[]{"10", "Referral"}, new System.Object[]{"11", "Administrative Limit Exceeded"}, new System.Object[]{"12", "Unavailable Critical Extension"}, new System.Object[]{"13", "Confidentiality Required"}, new System.Object[]{"14", "SASL Bind In Progress"}, new System.Object[]{"16", "No Such Attribute"}, new System.Object[]{"17", "Undefined Attribute Type"}, new System.Object[]{"18", "Inappropriate Matching"}, new System.Object[]{"19", "Constraint Violation"}, new System.Object[]{"20", "Attribute Or Value Exists"}, new System.Object[]{"21", "Invalid Attribute Syntax"}, new System.Object[]{"32", "No Such Object"}, new System.Object[]{"33", "Alias Problem"}, new System.Object[]{"34", "Invalid DN Syntax"}, new System.Object[]{"35", "Is Leaf"}, new System.Object[]{"36", "Alias Dereferencing Problem"}, new System.Object[]{"48", "Inappropriate Authentication"}, new System.Object[]{"49", "Invalid Credentials"}, new System.Object[]{"50", "Insufficient Access Rights"}, new System.Object[]{"51", "Busy"}, new System.Object[]{"52", "Unavailable"}, new System.Object[]{"53", "Unwilling To Perform"}, new System.Object[]{"54", "Loop Detect"}, new System.Object[]{"64", "Naming Violation"}, new System.Object[]{"65", "Object Class Violation"}, new System.Object[]{"66", "Not Allowed On Non-leaf"}, new System.Object[]{"67", "Not Allowed On RDN"}, new System.Object[]{"68", "Entry Already Exists"}, new System.Object[]{"69", "Object Class Modifications Prohibited"}, new System.Object[]{"71", 
			"Affects Multiple DSAs"}, new System.Object[]{"80", "Other"}, new System.Object[]{"81", "Server Down"}, new System.Object[]{"82", "Local Error"}, new System.Object[]{"83", "Encoding Error"}, new System.Object[]{"84", "Decoding Error"}, new System.Object[]{"85", "Ldap Timeout"}, new System.Object[]{"86", "Authentication Unknown"}, new System.Object[]{"87", "Filter Error"}, new System.Object[]{"88", "User Cancelled"}, new System.Object[]{"89", "Parameter Error"}, new System.Object[]{"90", "No Memory"}, new System.Object[]{"91", "Connect Error"}, new System.Object[]{"92", "Ldap Not Supported"}, new System.Object[]{"93", "Control Not Found"}, new System.Object[]{"94", "No Results Returned"}, new System.Object[]{"95", "More Results To Return"}, new System.Object[]{"96", "Client Loop"}, new System.Object[]{"97", "Referral Limit Exceeded"}, new System.Object[]{"112", "TLS not supported"}};
	} //End ResultCodeMessages
}
