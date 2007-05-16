//
// System.Diagnostics.DebuggerVisualizerAttribute.cs
//
// Author:
//   Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Diagnostics {

	[AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple=true)]
#if NET_2_0
	[ComVisible (true)]
	public sealed class DebuggerVisualizerAttribute : Attribute
#else
	internal sealed class DebuggerVisualizerAttribute : Attribute
#endif
	{
		public DebuggerVisualizerAttribute(string visualizerSourceName) {
			this.visualizerSourceName = visualizerSourceName;
		}

		public DebuggerVisualizerAttribute(Type visualizerSource) {
			this.visualizerSource = visualizerSource;
			this.visualizerSourceName = visualizerSource.AssemblyQualifiedName;
		}

		public DebuggerVisualizerAttribute(string visualizerName, string visualizerSourceName) {
			this.visualizerName = visualizerName;
			this.visualizerSourceName = visualizerSourceName;
		}

#if NET_2_0
		public DebuggerVisualizerAttribute (string visualizerTypeName,
						    Type visualizerObjectSource)
		{
			this.visualizerName = visualizerTypeName;
			this.visualizerSource = visualizerObjectSource;
			this.visualizerSourceName = visualizerObjectSource.AssemblyQualifiedName;
		}
#endif

		public DebuggerVisualizerAttribute(Type visualizer, string visualizerSourceName) {
			this.visualizerSourceName = visualizerSourceName;
			this.visualizer = visualizer;
			this.visualizerName = visualizer.AssemblyQualifiedName;
		}

		public DebuggerVisualizerAttribute(Type visualizer, Type visualizerSource) {
			this.visualizer = visualizer;
			this.visualizerName = visualizer.AssemblyQualifiedName;
			this.visualizerSource = visualizerSource;
			this.visualizerSourceName = visualizerSource.AssemblyQualifiedName;
		}

		public string Description {
			get {
				return description;
			}
			set {
				description = value;
			}
		}

		public Type Target {
			get {
				return target;
			}
			set {
				target = value;
				targetTypeName = target.AssemblyQualifiedName;
			}
		}

		public string TargetTypeName {
			get {
				return targetTypeName;
			}
			set {
				targetTypeName = value;
			}
		}

		// Debugged program-side type
		public string VisualizerObjectSourceTypeName {
			get {
				return visualizerSourceName;
			}
		}

		// Debugger-side type
		public string VisualizerTypeName {
			get {
				return visualizerName;
			}
		}

		string description;
		string visualizerSourceName;
		Type visualizerSource;
		string visualizerName;
		Type visualizer;

		string targetTypeName;
		Type target;
	}

}
