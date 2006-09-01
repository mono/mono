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

using System.Collections;

namespace System.Workflow.ComponentModel
{
	public class PropertyMetadata
	{
		private object default_value;
		private DependencyPropertyOptions options = DependencyPropertyOptions.Default;
		private bool _sealed = false;
		private SetValueOverride set_value;
		private GetValueOverride get_value;
		private Attribute[] attributes;

		// Constructors
		public PropertyMetadata ()
		{

		}

		public PropertyMetadata (object defaultValue)
		{
			default_value = defaultValue;
		}

		public PropertyMetadata (DependencyPropertyOptions options)
		{
			this.options = options;
		}

		public PropertyMetadata (params Attribute[] attributes)
		{
			this.attributes = attributes;
		}

		public PropertyMetadata (object defaultValue, params Attribute[] attributes)
		{
			default_value = defaultValue;
			this.attributes = attributes;
		}

		public PropertyMetadata (object defaultValue, DependencyPropertyOptions options)
		{
			default_value = defaultValue;
			this.options = options;
		}

		public PropertyMetadata (DependencyPropertyOptions options, params Attribute[] attributes)
		{
			this.options = options;
			this.attributes = attributes;
		}

		public PropertyMetadata (object defaultValue, DependencyPropertyOptions options, params Attribute[] attributes)
		{
			this.options = options;
			default_value = defaultValue;
			this.attributes = attributes;
		}

		public PropertyMetadata (object defaultValue, DependencyPropertyOptions options, GetValueOverride getValueOverride, SetValueOverride setValueOverride)
		{
			this.options = options;
			default_value = defaultValue;
			set_value = setValueOverride;
			get_value = getValueOverride;
		}

		public PropertyMetadata (object defaultValue, DependencyPropertyOptions options, GetValueOverride getValueOverride, SetValueOverride setValueOverride, params Attribute[] attributes)
		{
			this.options = options;
			default_value = defaultValue;
			set_value = setValueOverride;
			get_value = getValueOverride;
			this.attributes = attributes;
		}

      		// Properties
		public object DefaultValue {
			get { return default_value; }
			set { default_value = value; }
		}

		public GetValueOverride GetValueOverride {
			get { return get_value; }
			set { get_value = value; }
		}

		public bool IsMetaProperty {
			get { return (options & DependencyPropertyOptions.Metadata) == DependencyPropertyOptions.Metadata; }
		}

		public bool IsNonSerialized {
			get { return (options & DependencyPropertyOptions.NonSerialized) == DependencyPropertyOptions.NonSerialized; }
		}

		public bool IsReadOnly {
			get { return (options & DependencyPropertyOptions.Readonly) == DependencyPropertyOptions.Readonly; }
		}

		protected bool IsSealed {
			get { return _sealed;}
		}

		public DependencyPropertyOptions Options {
			get { return options; }
			set { options = value; }
		}

		public SetValueOverride SetValueOverride {
			get { return set_value; }
			set { set_value = value; }
		}


		// Methods
      		public Attribute[] GetAttributes ()
      		{
 			return attributes;
      		}

      		public Attribute[] GetAttributes (Type attributeType)
      		{
      			ArrayList array = new ArrayList (attributes.Length);

			foreach (Attribute attribute in attributes) {
				if (attribute != null && attribute.GetType () == attributeType) {
					array.Add (attribute);
				}
			}

			return (Attribute[]) array.ToArray (typeof (Attribute));
      		}

      		protected virtual void OnApply (DependencyProperty dependencyProperty, Type targetType)
		{

		}
	}
}

