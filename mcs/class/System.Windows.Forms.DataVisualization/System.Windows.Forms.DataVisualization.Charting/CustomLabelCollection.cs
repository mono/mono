// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CustomLabelCollection : ChartElementCollection<CustomLabel>
	{
		public void Add(
			double labelsStep,
			DateTimeIntervalType intervalType
			){
			throw new NotImplementedException ();
		}
		public CustomLabel Add(
			double fromPosition,
			double toPosition,
			string text
			){
			throw new NotImplementedException ();
		}
		public void Add(
			double labelsStep,
			DateTimeIntervalType intervalType,
			string format
			){
			throw new NotImplementedException ();
		}

		public CustomLabel Add(
			double fromPosition,
			double toPosition,
			string text,
			int rowIndex,
			LabelMarkStyle markStyle
			){
			throw new NotImplementedException ();
		}
		public void Add(
			double labelsStep,
			DateTimeIntervalType intervalType,
			string format,
			int rowIndex,
			LabelMarkStyle markStyle
			){
			throw new NotImplementedException ();
		}
		public CustomLabel Add(
			double fromPosition,
			double toPosition,
			string text,
			int rowIndex,
			LabelMarkStyle markStyle,
			GridTickTypes gridTick
			){
			throw new NotImplementedException ();
		}
		public void Add(
			double labelsStep,
			DateTimeIntervalType intervalType,
			double min,
			double max,
			string format,
			int rowIndex,
			LabelMarkStyle markStyle
			){
			throw new NotImplementedException ();
		}
	}
}