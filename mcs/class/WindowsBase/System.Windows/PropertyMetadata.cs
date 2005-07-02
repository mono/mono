//
// PropertyMetadata.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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

namespace System.Windows {
	public class PropertyMetadata {
		private object defaultValue;
		private GetValueOverride getValueOverride;
		private bool isSealed;
		private PropertyInvalidatedCallback propertyInvalidatedCallback;
		private ReadLocalValueOverride readLocalValueOverride;
		private bool readOnly;
		private WriteLocalValueOverride writeLocalValueOverride;
		
		public object DefaultValue {
			get { return defaultValue; }
			set { defaultValue = value; }
		}
		public GetValueOverride GetValueOverride {
			get { return getValueOverride; }
			set { getValueOverride = value;}
		}
		protected bool IsSealed {
			get { return isSealed; }
		}
		public PropertyInvalidatedCallback PropertyInvalidatedCallback {
			get { return propertyInvalidatedCallback; }
			set { propertyInvalidatedCallback = value; }
		}
		public ReadLocalValueOverride ReadLocalValueOverride {
			get { return readLocalValueOverride; }
			set { readLocalValueOverride = value; }
		}
		public bool ReadOnly {
			get { return readOnly; }
		}
		public WriteLocalValueOverride WriteLocalValueOverride {
			get { return writeLocalValueOverride; }
			set { writeLocalValueOverride = value; }
		}

		[MonoTODO()]		
		protected void ClearCachedDefaultValue (DependencyObject owner)
		{
			throw new NotImplementedException("ClearCachedDefaultValue(DependencyObject owner)");
		}
		
		[MonoTODO()]		
		protected virtual object CreateDefaultValue (DependencyObject owner, DependencyProperty property)
		{
			throw new NotImplementedException("CreateDefaultValue(DependencyObject owner, DependencyProperty property)");
		}
		
		[MonoTODO()]		
		protected virtual void Merge (PropertyMetadata baseMetadata, DependencyProperty dp)
		{
			throw new NotImplementedException("Merge(PropertyMetadata baseMetadata, DependencyProperty dp)");
		}
		
		[MonoTODO()]		
		protected virtual void OnApply (DependencyProperty dp, Type targetType)
		{
			throw new NotImplementedException("OnApply(DependencyProperty dp, Type targetType)");
		}
	}
}
