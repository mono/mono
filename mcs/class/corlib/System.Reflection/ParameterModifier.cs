//
// System.Reflection/ParameterModifier.cs
//
// Author:
// Paolo Molaro (lupus@ximian.com)
//
// Copyright (c) 2001 Ximian, Inc

using System;

namespace System.Reflection {
	public struct ParameterModifier {
		private bool[] data;
		
		public ParameterModifier (int paramaterCount) {
			data = new bool [paramaterCount];
		}

		public bool this [int index] {
			get {return data [index];} 
			set {data [index] = value;}
		}

	}

}
