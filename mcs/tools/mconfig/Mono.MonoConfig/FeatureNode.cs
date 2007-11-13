//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;

namespace Mono.MonoConfig
{
	public class FeatureNode
	{
		List <FeatureBlock> blocks;
		List <FeatureAction> actionsBefore;
		List <FeatureAction> actionsAfter;
		string description;
		
		public List <FeatureBlock> Blocks {
			get {
				if (blocks != null)
					return blocks;

				return new List <FeatureBlock> ();
			}
		}

		public string Description {
			get {
				if (description != null)
					return description;

				return String.Empty;
			}
		}

		public List <FeatureAction> ActionsBefore {
			get { return actionsBefore; }
		}

		public List <FeatureAction> ActionsAfter {
			get { return actionsAfter; }
		}
		
		public FeatureNode (List <FeatureBlock> blocks, string description,
				    List <FeatureAction> actionsBefore, List <FeatureAction> actionsAfter)
		{
			this.blocks = blocks;
			this.description = description;
			this.actionsBefore = actionsBefore;
			this.actionsAfter = actionsAfter;
		}
	}
}
