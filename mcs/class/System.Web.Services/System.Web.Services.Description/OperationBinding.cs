// 
// System.Web.Services.Description.OperationBinding.cs
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

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
	public sealed class OperationBinding :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{
		#region Fields

		Binding binding;
		ServiceDescriptionFormatExtensionCollection extensions;
		FaultBindingCollection faults;
		InputBinding input;
#if !NET_2_0
		string name;
#endif
		OutputBinding output;

		#endregion // Fields

		#region Constructors
		
		public OperationBinding ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			faults = new FaultBindingCollection (this);
			input = null;
#if !NET_2_0
			name = String.Empty;
#endif
			output = null;
		}
		
		#endregion // Constructors

		#region Properties
	
//		[XmlIgnore]
		public Binding Binding {
			get { return binding; }
		}

		[XmlIgnore]
		public 
#if NET_2_0
		override
#endif
		ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		[XmlElement ("fault")]
		public FaultBindingCollection Faults {
			get { return faults; }
		}

		[XmlElement ("input")]
		public InputBinding Input {
			get { return input; }
			set {
				input = value; 
				if (input != null)
					input.SetParent (this);
			}
		}

#if !NET_2_0
		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlElement ("output")]
		public OutputBinding Output {
			get { return output; }
			set {
				output = value; 
				if (output != null)
					output.SetParent (this);
			}
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Binding binding) 
		{
			this.binding = binding; 
		} 

		#endregion
	}
}
