//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker, Rafael Teixeira
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

namespace System.Messaging 
{
	public class MessageQueueCriteria 
	{
		public MessageQueueCriteria()
		{
			ClearAll();
		}
		
		private bool setCategory;
		private Guid category;
		public Guid Category 
		{
			get 
			{ 
				if (!setCategory) 
					throw new InvalidOperationException();
				return category;
			}
			set 
			{ 
				category = value; 
				setCategory = true; 
			}
		}
		
		private bool setCreatedAfter;
		private DateTime createdAfter;
		public DateTime CreatedAfter 
		{
			get 
			{ 
				if (!setCreatedAfter) 
					throw new InvalidOperationException();
				return createdAfter;
			}
			set 
			{ 
				createdAfter = value; 
				setCreatedAfter = true; 
			}
		}
		
		private bool setCreatedBefore;
		private DateTime createdBefore;
		public DateTime CreatedBefore 
		{
			get 
			{ 
				if (!setCreatedBefore) 
					throw new InvalidOperationException();
				return createdBefore;
			}
			set 
			{ 
				createdBefore = value; 
				setCreatedBefore = true; 
			}
		}
		
		private bool setLabel;
		private string label; 
		public string Label 
		{
			get 
			{ 
				if (!setLabel) 
					throw new InvalidOperationException();
				return label;
			}
			set 
			{ 
				label = value; 
				setLabel = true; 
			}
		}
		
		[MonoTODO]
		private bool invalidMachineName(string name)
		{
			return false;
		}
		
		private bool setMachineName;
		private string machineName; 
		public string MachineName 
		{
			get 
			{ 
				if (!setMachineName) 
					throw new InvalidOperationException();
				return machineName;
			}
			set 
			{ 
				if (invalidMachineName(value)) 
					throw new InvalidOperationException();
				machineName = value; 
				setMachineName = true; 
			}
		}
		
		private bool setModifiedAfter;
		private DateTime modifiedAfter; 
		public DateTime ModifiedAfter 
		{
			get 
			{ 
				if (!setModifiedAfter) 
					throw new InvalidOperationException();
				return modifiedAfter;
			}
			set 
			{ 
				modifiedAfter = value; 
				setModifiedAfter = true; 
			}
		}
		
		private bool setModifiedBefore;
		private DateTime modifiedBefore; 
		public DateTime ModifiedBefore 
		{
			get 
			{ 
				if (!setModifiedBefore) 
					throw new InvalidOperationException();
				return modifiedBefore;
			}
			set 
			{ 
				modifiedBefore = value; 
				setModifiedBefore = true; 
			}
		}
		
		public void ClearAll()
		{
			setCategory = false;
			setCreatedAfter = false;
			setCreatedBefore = false;
			setLabel = false;
			setMachineName = false;
			setModifiedAfter = false;
			setModifiedBefore = false;
		}
		
		// To be called by the MessageQueue.GetPublicQueues(MessageQueueCriteria criteria) method
		internal bool Match(
			Guid category,
			DateTime created,
			string label,
			string machineName,
			DateTime modified)
		{
			if (setCategory && this.category != category)
				return false;
			if (setCreatedAfter && created < createdAfter)
				return false;
			if (setCreatedBefore && created > createdBefore)
				return false;
			if (setLabel && this.label != label)
				return false;			
			if (setMachineName && this.machineName != machineName)
				return false;			
			if (setModifiedAfter && modified < modifiedAfter)
				return false;
			if (setModifiedBefore && modified > modifiedBefore)
				return false;
			return true;
		}	
		
	}
}
