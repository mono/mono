// 
// System.Web.Services.Description.OperationMessageCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class OperationMessageCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationMessageCollection (Operation operation)
			: base (operation)
		{
		}

		#endregion // Constructors

		#region Properties

		public OperationFlow Flow {
			get { 
				switch (Count) {
				case 1: 
					if (this[0] is OperationInput)
						return OperationFlow.OneWay;
					else
						return OperationFlow.Notification;
				case 2:
					if (this[0] is OperationInput)
						return OperationFlow.RequestResponse;
					else
						return OperationFlow.SolicitResponse;
				}
				return OperationFlow.None;
			}
		}

		public OperationInput Input {
			get { 
				foreach (object message in List)
					if (message is OperationInput)
						return (OperationInput) message;
				return null;
			}
		}
	
		public OperationMessage this [int index] {
			get { return (OperationMessage) List[index]; }
			set { List[index] = value; }
		}

		public OperationOutput Output {
			get { 
				foreach (object message in List)
					if (message is OperationOutput)
						return (OperationOutput) message;
				return null;
			}
		}

		internal OperationFault Fault {
			get { 
				foreach (object message in List)
					if (message is OperationFault)
						return (OperationFault) message;
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationMessage operationMessage) 
		{
			Insert (Count, operationMessage);
			return (Count - 1);
		}

		public bool Contains (OperationMessage operationMessage)
		{
			return List.Contains (operationMessage);
		}

		public void CopyTo (OperationMessage[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		internal OperationMessage Find (string name)
		{
			foreach (OperationMessage m in List)
				if (m.Name == name)
					return m;
			return null;
		}

		public int IndexOf (OperationMessage operationMessage)
		{
			return List.IndexOf (operationMessage);
		}

		public void Insert (int index, OperationMessage operationMessage)
		{
			List.Insert (index, operationMessage);
		}

		protected override void OnInsert (int index, object value)
		{
			if (Count == 0)
				return;
			
			if (Count == 1 && value.GetType() != this[0].GetType())
				return;

				throw new InvalidOperationException ("The operation object can only contain one input and one output message.");
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			if (oldValue.GetType () != newValue.GetType ())
				throw new InvalidOperationException ("The message types of the old and new value are not the same.");
			base.OnSet (index, oldValue, newValue);
		}

		protected override void OnValidate (object value)
		{
			if (value == null)
				throw new ArgumentException("The message object is a null reference.");
			if (!(value is OperationInput || value is OperationOutput))
				throw new ArgumentException ("The message object is not an input or an output message.");
		}
	
		public void Remove (OperationMessage operationMessage)
		{
			List.Remove (operationMessage);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationMessage) value).SetParent ((Operation) parent);
		}
			
		#endregion // Methods
	}
}
