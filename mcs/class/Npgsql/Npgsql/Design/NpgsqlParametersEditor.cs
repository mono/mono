using System;
using System.ComponentModel.Design;

namespace Npgsql.Design
{
	/// <summary>
	/// Zusammenfassung fr NpgsqlParametersEditor.
	/// </summary>
	internal class NpgsqlParametersEditor : CollectionEditor
	{
		NpgsqlParameterCollection parameters;
		public NpgsqlParametersEditor(Type type) : base(type){
			this.parameters = null;
		}
	
		protected override bool CanSelectMultipleInstances() {
			return false;
		}
	
		protected override object CreateInstance(Type itemType) {
			NpgsqlParameter param = base.CreateInstance(itemType) as NpgsqlParameter;
			if (param != null){
				param.ParameterName = this.GetUniqueParameterName(this.parameters, ":Parameter", 1);
			}
			return param;
		}
	
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value) {
			this.parameters = value as NpgsqlParameterCollection;
			return base.EditValue (context, provider, value);
		}
	
		protected override string HelpTopic {
			get {
					return "vs.data.collectioneditor.parameter";
			}
		}
		private string GetUniqueParameterName(NpgsqlParameterCollection parameters, string Prefix, int InitialPostfix){
			int Postfix = InitialPostfix;
			string ReturnValue = String.Empty;
			bool IsInside = true;
			while(IsInside){
				ReturnValue = String.Concat(Prefix, Postfix.ToString());
				if(parameters == null)
					break;
				IsInside = parameters.Contains(ReturnValue);
				Postfix++;
			}
			return ReturnValue;
		}
	}
}
