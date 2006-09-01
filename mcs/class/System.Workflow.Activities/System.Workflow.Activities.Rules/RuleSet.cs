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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;

namespace System.Workflow.Activities.Rules
{
	[Serializable]
	public class RuleSet
	{
		private string name;
		private string description;

		public RuleSet ()
		{

		}

		public RuleSet (string name)
		{
			this.name = name;
		}

		public RuleSet (string name, string description) : this (name)
		{
			this.description = description;
		}

		//public RuleSet Clone ()

		// Properties
		//public RuleChainingBehavior ChainingBehavior { get; set; }
		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		//public ICollection <Rule> Rules { get; }

		// Methods
		public override bool Equals (object obj)
		{
			RuleSet target = (obj as RuleSet);

			if (target  == null) {
				return false;
			}

			if (Name == target.Name && Description == target.Description) {
				return true;
			}

			return false;
		}

		public void Execute (RuleExecution ruleExecution)
		{

		}

		public override int GetHashCode ()
		{
			return name.GetHashCode () ^ description.GetHashCode ();
		}

		/*public bool Validate (RuleValidation validation)
		{

		}*/
	}

}

