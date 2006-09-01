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
using System.ComponentModel;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities
{

	public class StateMachineWorkflowActivity : StateActivity
	{
		public static readonly DependencyProperty CompletedStateNameProperty;
		public static readonly DependencyProperty InitialStateNameProperty;
		public const string SetStateQueueName = "SetStateQueue";
		private string current_state;
		private string previous_state;

		static StateMachineWorkflowActivity ()
		{
			InitialStateNameProperty = DependencyProperty.Register ("InitialStateName",
				typeof (string), typeof (StateMachineWorkflowActivity));

      			CompletedStateNameProperty = DependencyProperty.Register ("CompletedStateName",
      				typeof (string), typeof (StateMachineWorkflowActivity));
		}

		public StateMachineWorkflowActivity ()
		{

		}

		public StateMachineWorkflowActivity (string name) : base (name)
		{

		}

		// Properties
		public string CompletedStateName {
			get {
				return (string) GetValue (CompletedStateNameProperty);
			}
			set {
				SetValue (CompletedStateNameProperty, value);
			}
		}

		public string CurrentStateName {
			get {
				return current_state;
			}
		}

		[MonoTODO]
		public ActivityCondition DynamicUpdateCondition {
			get { return null; }
			set {}
		}

		public string InitialStateName {
			get {
				return (string) GetValue (InitialStateNameProperty);
			}
			set {
				SetValue (InitialStateNameProperty, value);
			}
		}

		public string PreviousStateName {
			get { return previous_state; }
		}

		// Private
		// TODO: This breaks API compatibility
		public void SetCurrentStateName (string state)
		{
			previous_state = current_state;
			current_state =  state;
		}
	}
}

