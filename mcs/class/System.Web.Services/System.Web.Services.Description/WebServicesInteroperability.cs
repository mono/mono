// 
// System.Web.Services.Description.WebServicesInteroperability.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

#if NET_2_0

using System.Collections;

namespace System.Web.Services.Description 
{
	public sealed class WebServicesInteroperability
	{
		private WebServicesInteroperability ()
		{
		}
		
		[MonoTODO]
		public static bool CheckConformance (WsiClaims claims, ServiceDescription service, BasicProfileViolationCollection violations)
		{
			ServiceDescriptionCollection col = new ServiceDescriptionCollection ();
			col.Add (service);
			ConformanceCheckContext ctx = new ConformanceCheckContext (col, violations);
			return Check (claims, ctx, col);
		}

		[MonoTODO]
		public static bool CheckConformance (WsiClaims claims, ServiceDescriptionCollection services, BasicProfileViolationCollection violations)
		{
			ConformanceCheckContext ctx = new ConformanceCheckContext (services, violations);
			return Check (claims, ctx, services);
		}

		[MonoTODO]
		public static bool CheckConformance (WsiClaims claims, WebReference webReference, BasicProfileViolationCollection violations)
		{
			ConformanceCheckContext ctx = new ConformanceCheckContext (webReference, violations);
			return Check (claims, ctx, webReference.Documents);
		}
		
		static bool Check (WsiClaims claims, ConformanceCheckContext ctx, IEnumerable documents)
		{
			ConformanceChecker[] checkers = GetCheckers (claims);
			if (checkers == null) return true;
			
			foreach (object doc in documents) {
				if (!(doc is ServiceDescription)) continue;
				
				foreach (ConformanceChecker c in checkers)
					Check (ctx, c, (ServiceDescription)doc);
			}
				
			return ctx.Violations.Count == 0;
		}
		
		static ConformanceChecker[] GetCheckers (WsiClaims claims)
		{
			if ((claims & WsiClaims.BP10) != 0)
				return new ConformanceChecker[] { BasicProfileChecker.Instance };
			return null;
		}
		
		static void Check (ConformanceCheckContext ctx, ConformanceChecker checker, ServiceDescription sd)
		{
			ctx.ServiceDescription = sd;
			ctx.Checker = checker;
			
			checker.Check (ctx, sd);
			CheckExtensions (ctx, checker, sd.Extensions);
			
			foreach (Service s in sd.Services) {
				checker.Check (ctx, s);
				foreach (Port p in s.Ports) {
					checker.Check (ctx, p);
					CheckExtensions (ctx, checker, p.Extensions);
				}
			}
			
			foreach (Binding b in sd.Bindings)
			{
				checker.Check (ctx, b);
				CheckExtensions (ctx, checker, b.Extensions);
				
				foreach (OperationBinding oper in b.Operations) {
					CheckExtensions (ctx, checker, oper.Extensions);
					
					foreach (MessageBinding mb in oper.Faults) {
						checker.Check (ctx, mb);
						CheckExtensions (ctx, checker, mb.Extensions);
					}
					
					checker.Check (ctx, oper.Input);
					CheckExtensions (ctx, checker, oper.Input.Extensions);
					
					checker.Check (ctx, oper.Output);
					CheckExtensions (ctx, checker, oper.Output.Extensions);
				}
			}
			
			foreach (PortType pt in sd.PortTypes)
			{
				checker.Check (ctx, pt);
				
				foreach (Operation oper in pt.Operations) {
					checker.Check (ctx, oper);
					foreach (OperationMessage msg in oper.Messages)
						checker.Check (ctx, msg);
					
					foreach (OperationMessage msg in oper.Faults)
						checker.Check (ctx, msg);
				}
			}
			
			foreach (Message msg in sd.Messages)
			{
				checker.Check (ctx, msg);
				foreach (MessagePart part in msg.Parts)
					checker.Check (ctx, part);
			}
		}
		
		static void CheckExtensions (ConformanceCheckContext ctx, ConformanceChecker checker, ServiceDescriptionFormatExtensionCollection extensions)
		{
			foreach (ServiceDescriptionFormatExtension ext in extensions)
				checker.Check (ctx, ext);
		}
	}
}

#endif
