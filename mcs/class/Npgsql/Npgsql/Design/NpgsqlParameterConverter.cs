using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Reflection;

namespace Npgsql.Design
{
	/// <summary>
	/// Zusammenfassung fr NpgsqlParameterConverter.
	/// </summary>
	internal class NpgsqlParameterConverter : ExpandableObjectConverter
	{
		public NpgsqlParameterConverter()
		{
			//
			// TODO: Fgen Sie hier die Konstruktorlogik hinzu
			//
		}
	
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}
	
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if(destinationType == null){
				throw new ArgumentNullException("destinationType");
			}
			if(destinationType == typeof(InstanceDescriptor) && value as NpgsqlParameter != null){
				NpgsqlParameter param = (NpgsqlParameter)value;
				bool DbTypeChanged = false;
				bool OtherChanged = false;
				bool SizeChanged = false;
				bool SourceColumnChanged = false;
				bool ValueChanged = false;
				if(param.DbType != DbType.String){
					DbTypeChanged= true;
				}
				if(param.Direction != ParameterDirection.Input || param.Precision != 0 || param.Scale != 0 || param.SourceVersion != DataRowVersion.Current || param.IsNullable == true){
					OtherChanged = true;
				}
				if(param.Size != 0){
					SizeChanged = true;
				}
				if(param.SourceColumn == null || param.SourceColumn.Trim() != String.Empty){
					SourceColumnChanged = true;
				}
				if(param.Value != null){
					ValueChanged = true;
				}

				if(!(OtherChanged || SizeChanged || SourceColumnChanged || ValueChanged)){
					ConstructorInfo ci = typeof(NpgsqlParameter).GetConstructor(new Type[]{typeof(String), typeof(DbType)});
					if (ci != null){
						return new InstanceDescriptor(ci, new Object[]{param.ParameterName, param.DbType});
					}
				}else if(!(OtherChanged || SourceColumnChanged || ValueChanged)){
					ConstructorInfo ci = typeof(NpgsqlParameter).GetConstructor(new Type[]{typeof(String), typeof(DbType), typeof(Int32)});
					if (ci != null){
						return new InstanceDescriptor(ci, new Object[]{param.ParameterName, param.DbType, param.Size});
					}
				}else if(!(OtherChanged || ValueChanged)){
					ConstructorInfo ci = typeof(NpgsqlParameter).GetConstructor(new Type[]{typeof(String), typeof(DbType), typeof(Int32), typeof(String)});
					if (ci != null){
						return new InstanceDescriptor(ci, new Object[]{param.ParameterName, param.DbType, param.Size, param.SourceColumn});
					}
				}else if(ValueChanged && !DbTypeChanged){
					ConstructorInfo ci = typeof(NpgsqlParameter).GetConstructor(new Type[]{typeof(String), typeof(Object)});
					if (ci != null){
						return new InstanceDescriptor(ci, new Object[]{param.ParameterName, param.Value});
					}
				}else{
					ConstructorInfo ci = typeof(NpgsqlParameter).GetConstructor(new Type[]{typeof(String), typeof(DbType), typeof(Int32), typeof(String), typeof(ParameterDirection), typeof(Boolean), typeof(Byte), typeof(Byte), typeof(DataRowVersion), typeof(Object)});
					if (ci != null){
						return new InstanceDescriptor(ci, new Object[]{param.ParameterName, param.DbType, param.Size, param.SourceColumn, param.Direction, param.IsNullable, param.Precision, param.Scale, param.SourceVersion, param.Value});
					}
				}
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
