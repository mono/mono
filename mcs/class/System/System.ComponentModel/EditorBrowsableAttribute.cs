using System;

namespace System.ComponentModel
{

	/// <summary>
	/// Specifies that a property or method is viewable in an editor. This class cannot be inherited.
	/// </summary>
	[MonoTODO("Missing description for State. Only minimal testing.")]
	[AttributeUsage(
		AttributeTargets.Class|
		AttributeTargets.Constructor|
		AttributeTargets.Delegate|
		AttributeTargets.Enum|
		AttributeTargets.Event|
		AttributeTargets.Field|
		AttributeTargets.Interface|
		AttributeTargets.Method|
		AttributeTargets.Property|
		AttributeTargets.Struct)]
	public sealed class EditorBrowsableAttribute : Attribute
	{
		private System.ComponentModel.EditorBrowsableState state;

		/// <summary>
		/// FIXME: Summary description for State.
		/// </summary>
		public System.ComponentModel.EditorBrowsableState State
		{
			get 
			{
				return state;
			}
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.EditorBrowsableAttribute class with an System.ComponentModel.EditorBrowsableState.
		/// </summary>
		/// <param name="state">The System.ComponentModel.EditorBrowsableState to set System.ComponentModel.EditorBrowsableAttribute.State to.</param>
		public EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState state)
		{
			this.state = state;
		}

		/// <summary>
		/// Initializes a new instance of the System.ComponentModel.EditorBrowsableAttribute class with an System.ComponentModel.EditorBrowsableState == System.ComponentModel.EditorBrowsableState.Always.
		/// </summary>
		public EditorBrowsableAttribute()
		{
			this.state = System.ComponentModel.EditorBrowsableState.Always; 
		}
	}
}
