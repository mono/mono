/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.HtmlControls{
	public sealed class HtmlTableRowCollection : ICollection {
		
		private HtmlTable _owner;
		
		internal HtmlTableRowCollection(HtmlTable owner){
			_owner = owner;
		}
		
		public void Add(HtmlTableRow row){
			Insert(-1, row);
		}
		
		public void Clear(){
			if (_owner.HasControls()) _owner.Controls.Clear();
		}
		
		public void CopyTo(Array array, int index){
			//FIXME: foreach(IEnumerator i in GetEnumerator()){
			//	array.SetValue(i, index+1);
			//}
		}
		
		public IEnumerator GetEnumerator(){
			return _owner.Controls.GetEnumerator();
		}
		
		public void Insert(int index, HtmlTableRow row){
			_owner.Controls.AddAt(index,row);
		}
		
		public void Remove(HtmlTableRow row){
			_owner.Controls.Remove(row);
		}
		
		public void RemoveAt(int index){
			_owner.Controls.RemoveAt(index);
		}
		
		public int Count {
			get{
				if (_owner.HasControls()) return _owner.Controls.Count;
				return 0;
			}
		}
		
		public bool IsReadOnly {
			get{
				return false;
			}
		}
		
		public bool IsSynchronized {
			get{
				return false;
			}
		}
		
		public HtmlTableRow this[int index] {
			get{
				return (HtmlTableRow) _owner.Controls[index];
			}
		}
		
		public object SyncRoot {
			get{
				//LAMESPEC: what to return
				return null;
			}
	}
	}
	// end of System.Web.UI.HtmlControls.HtmlTableRowCollection
}
