//
// DependencyObject.cs
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

using System.Collections;

namespace System.Windows {
	public class DependencyObject {
		private static Hashtable propertyDeclarations = new Hashtable();
		private Hashtable properties = new Hashtable();
		
		private DependencyObjectType dependencyObjectType;
		public DependencyObjectType DependencyObjectType { 
			get { return dependencyObjectType; }
		}

		[MonoTODO()]		
		public void ClearValue(DependencyProperty dp)
		{
			throw new NotImplementedException("ClearValue(DependencyProperty dp)");
		}
		
		[MonoTODO()]		
		public void ClearValue(DependencyPropertyKey key)
		{
			throw new NotImplementedException("ClearValue(DependencyPropertyKey key)");
		}
		
		[MonoTODO()]		
		public LocalValueEnumerator GetLocalValueEnumerator()
		{
			throw new NotImplementedException("GetLocalValueEnumerator()");
		}
		
		public object GetValue(DependencyProperty dp)
		{
			return properties[dp];
		}
		
		[MonoTODO()]		
		public object GetValueBase(DependencyProperty dp)
		{
			throw new NotImplementedException("GetValueBase(DependencyProperty dp)");
		}
		
		[MonoTODO()]		
		protected virtual object GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)
		{
			throw new NotImplementedException("GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)");
		}
		
		[MonoTODO()]		
		public void InvalidateProperty(DependencyProperty dp)
		{
			throw new NotImplementedException("InvalidateProperty(DependencyProperty dp)");
		}
		
		[MonoTODO()]		
		protected virtual void OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata)
		{
			throw new NotImplementedException("OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata)");
		}
		
		[MonoTODO()]		
		public object ReadLocalValue(DependencyProperty dp)
		{
			throw new NotImplementedException("ReadLocalValue(DependencyProperty dp)");
		}
		
		public void SetValue(DependencyProperty dp, object value)
		{

			ValidateValueCallback validate = dp.ValidateValueCallback;
			if (validate != null && !validate(value))
				throw new Exception("Value does not validate");
			else
				properties[dp] = value;
		}
		
		[MonoTODO()]		
		public void SetValue(DependencyPropertyKey key, object value)
		{
			throw new NotImplementedException("SetValue(DependencyPropertyKey key, object value)");
		}
		
		[MonoTODO()]		
		public void SetValueBase(DependencyProperty dp, object value)
		{
			throw new NotImplementedException("SetValueBase(DependencyProperty dp, object value)");
		}
		
		[MonoTODO()]		
		public void SetValueBase(DependencyPropertyKey key, object value)
		{
			throw new NotImplementedException("SetValueBase(DependencyPropertyKey key, object value)");
		}

		internal static DependencyProperty lookup(Type t, string name)
		{
			return (DependencyProperty)((Hashtable)propertyDeclarations[t])[name];
		}

		internal static void register(Type t, DependencyProperty dp)
		{
			if (propertyDeclarations[t] == null)
				propertyDeclarations[t] = new Hashtable();
			Hashtable typeDeclarations = (Hashtable)propertyDeclarations[t];
			if (!typeDeclarations.ContainsKey(dp.Name))
				typeDeclarations[dp.Name] = dp;
			else
				throw new Exception("A property named " + dp.Name + " already exists on " + t.Name);
		}
	}
}
