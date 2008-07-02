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
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	[AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple=true)]
#if NET_2_0
	[ComVisible (true)]
	public sealed class DebuggerVisualizerAttribute : Attribute
#else
	internal sealed class DebuggerVisualizerAttribute : Attribute
#endif
	{
		private string description;
		private string visualizerSourceName;
		private string visualizerName;
		private string targetTypeName;
		private Type target;

		public DebuggerVisualizerAttribute (string visualizerTypeName)
		{
			this.visualizerName = visualizerTypeName;
		}

		public DebuggerVisualizerAttribute (Type visualizer)
		{
			if (visualizer == null)
				throw new ArgumentNullException ("visualizer");

			this.visualizerName = visualizer.AssemblyQualifiedName;
		}

		public DebuggerVisualizerAttribute (string visualizerTypeName, string visualizerObjectSourceTypeName)
		{
			this.visualizerName = visualizerTypeName;
			this.visualizerSourceName = visualizerObjectSourceTypeName;
		}

		public DebuggerVisualizerAttribute (string visualizerTypeName, Type visualizerObjectSource)
		{
			if (visualizerObjectSource == null)
				throw new ArgumentNullException ("visualizerObjectSource");

			this.visualizerName = visualizerTypeName;
			this.visualizerSourceName = visualizerObjectSource.AssemblyQualifiedName;
		}

		public DebuggerVisualizerAttribute (Type visualizer, string visualizerObjectSourceTypeName)
		{
			if (visualizer == null)
				throw new ArgumentNullException ("visualizer");

			this.visualizerName = visualizer.AssemblyQualifiedName;
			this.visualizerSourceName = visualizerObjectSourceTypeName;
		}

		public DebuggerVisualizerAttribute (Type visualizer, Type visualizerObjectSource)
		{
			if (visualizer == null)
				throw new ArgumentNullException ("visualizer");
			if (visualizerObjectSource == null)
				throw new ArgumentNullException ("visualizerObjectSource");

			this.visualizerName = visualizer.AssemblyQualifiedName;
			this.visualizerSourceName = visualizerObjectSource.AssemblyQualifiedName;
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
	}
}
