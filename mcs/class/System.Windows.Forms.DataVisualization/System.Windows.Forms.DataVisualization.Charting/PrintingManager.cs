// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
using System.Drawing;
using System.Drawing.Printing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class PrintingManager : IDisposable
	{
		public PrintDocument PrintDocument { get; set; }


		public void Dispose(){
			throw new NotImplementedException();
		}
		protected virtual void Dispose(bool disposing){
			throw new NotImplementedException();
		}
		public void PageSetup(){
			throw new NotImplementedException();
		}
		public void Print(bool showPrintDialog){
			throw new NotImplementedException();
		}
		public void PrintPaint(Graphics graphics,Rectangle position){
			throw new NotImplementedException ();
		}
		public void PrintPreview(){
			throw new NotImplementedException ();
		}
	}
}