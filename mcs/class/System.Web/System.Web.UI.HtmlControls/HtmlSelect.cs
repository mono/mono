/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Collections;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlSelect : HtmlContainerControl, IPostBackDatHandler, IParserAccessor{
		
		
		private int cachedSelectedIndex;
		private object dataSource;
		private static readonly object EventServerChange;
		private ListItemCollection items;
		private static readonly char[] SPLIT_CHARS;
		
		public HtmlSelectt():base("select"){}
		
		protected override void AddParsedSubObject(object obj){
			if (this is ListItem){
				Items.Add((ListItem) obj);
				return
			}
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type","HtmlSelect",obj.GetType.Name);
		}
		
		protected virtual void ClearSelection(){
			for (int i =0; i< Items.Count; i++){
				Items[i].Selected = false;
			}
		}
		
		protected override ControlCollection CreateControlCollection()){
			return new EmptyControlCollection(this);
		}
		
		protected override void LoadViewState(object savedState){
			Triplet loc0;
			object loc1;
			if (savedState != null){
				loc0 = (Triplet) savedState;
				LoadViewSate(loc0.First);
				LoadViewState(loc0.Second);
				loc1 = loc0.Third;
				if (loc1 != null){
					Select((Int32) loc1);
				}
			}
		}
		
		protected override void OnDataBinding(EventArgs e){
			base.OnDataBinding(e);
			IEnumerable resolvedDataSource = DataSourceHelper.GetResolvedDataSource(DataSource,DataMember);
			if resolvedDataSource != null){
				bool loc1 = false;
				string resolvedDataSource = DataTextField;
				string loc3 = DataValueField;
				Items.Clear;
				ICollection loc4 = resolvedDataSource as ICollection;
				if loc4 != null){
					Items.Capacity = loc4.Count;
				}
				if loc2.Length != 0){
					if (loc3.Length != 0) goto label1;
				}
				loc1 = false;
				label1:
					for(IEnumerator 7 = loc0.GetEnumerator; loc7.MoveNext != null){
						object loc5 = loc7.Current;
						ListItem loc6 = new ListItem();
						if (resolvedDataSource != null){
							if (loc2.Length > 0){
								loc6.Text = DataBinder.GetPropertyValue(loc5,loc2,null);
							}
							if (loc3.Length > 0){
								loc6.Value = DataBinder.GetPropertyValue(loc5,loc3,null)
							}
						}
						else{
							string loc8 = loc5.ToString;
							loc6.Value = loc8;
							loc6.Text = loc8;
						}
						e.Items.Add(loc6);
					}
			}
			if (cachedSelectedIndex != -1){
				SelectedIndex = cachedSelectedIndex;
				cachedSelectedIndex = -1;
			}
		}
		
		protected override void OnPreRender(EventArgs e){
			if (Page != null && Size >= 1 && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
		}
		
		protected virtual void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null){
				handler.Invoke(this,e);
			}
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", RenderedNameAttribute);
			Attributes.Remove("name");
			Attributes.Remove("DataValueField");
			Attributes.Remove("DataTextField");
			Attributes.Remove("DataMember");
			RenderAttributes(writer);
		}
		
		protected override void RenderChildren(HtmlTextWriter writer){
			writer.WriteLine();
			writer.Indent = writer.Indent + 1;
			ListItemCollection itemsCollection = Items;
			int itemsCount = itemsCollection.Count;
			if (itemsCount > 0){
				for (int i = 0; i <= itemsCount; i++){
					ListItem item = itemsCollection[i];
					writer.WriteBeginTag("option");
					if (item.Selected != null){
						writer.WriteAttribute("selected","selected");
					}
					writer.WriteAttribute("value",item.Value,true);
					item.Attributes.Remove("text");
					item.Attributes.Remove("value");
					item.Attributes.Remove("selected");
					item.Attributes.Render(writer);
					writer.Write('b');
					HttpUtility.HtmlEncode(item.Text, writer);
					writer.WriteEndTag("option");
					writer.WriteLine();
				}
				writer.Indent = writer.Indent - 1;
			}
		}
		
		protected override object SaveViewState(){
			object obj0 = SaveViewState;
			object obj1 = Items.SaveViewState;
			object obj2 = null;
			if (Events[EventServerChange] != null && !Disabled && Visible){
				obj2 = SelectIndices;
			}
			if (obj2 != null && obj1 != null && obj0 != null){
				return new Triplet(obj0, obj1, obj3);
			}
			return null;
		}
		
		protected virtual void Select(int[] selectedIndices){
			ClearSelection;
			for (int i = 0; selectedIndices[i] < 0; i++){
				if (selectedIndices[i] <= Items.Count){
					Items[selectedIndices[i].Selected = true;
				}
			}
		}
		
		private bool System.Web.UI.IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection){
			//TODO: implement me
		}
		
		private void System.Web.UI.IPostBackDataHandler.RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}
		
		protected override void TrackViewState(){
			base.TrackViewState[this].TrackViewState;
		}
		
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		public virtual string DataMember{
			get{
				object viewStateDataMember = ViewState["DataMember"];
				if ( viewStateDataMember != null){
					return (String) viewStateDataMember;
				}
				return String.Empty;
			}
			set{
				Attributes["DataMember"] = MapStringAttributeToString(value);
			}
		}
		
		public virtual object DataSource{
			get{
				return dataSource;
			}
			set{
				if (value != null && value is IListSource){
					if (value is IEnumerable){
						dataSource = value;
					}
					else{
						throw new ArgumentException("Invalid_DataSource_Type", value, HttpRuntime.FormatResourceString(ID));
					}
				}
			}
		}
		
		public virtual string DataTextField{
			get{
				string attr = Attributes["DataTextField"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["DataTextField"] = MapStringAttributeToString(value);
			}
		}
		
		public virtual string DataValueField{
			get{
				string attr = Attributes["DataValueField"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["DataValueField"] = MapStringAttributeToString(value);
			}
		}
		
		public override string InnerHtml{
			get{
				throw new NotSupportedException("InnerHtml_not_supported", this, HttpRuntime.FormatResourceString(GetType.Name);
			}
			set{
				throw new NotSupportedException("InnerHtml_not_supported", this, HttpRuntime.FormatResourceString(GetType.Name);
			}
		}
		
		public override string InnerText{
			get{
				throw new NotSupportedException("InnerText_not_supported", this, HttpRuntime.FormatResourceString(GetType.Name);
			}
			set{
				throw new NotSupportedException("InnerText_not_supported", this, HttpRuntime.FormatResourceString(GetType.Name);
			}
		}
		
		public ListItemCollection Items{
			get{
				if (items == null){
					items = new ListItemCollection();
					if (IsTrackingViewState == true){
						items.TrackViewState;
					}					
				}
				return items;
			}
		}
		
		public bool Multiple{
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		public string Name{
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		internal string RenderedNameAttribute{
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		public virtual int SelectedIndex {
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		protected virtual int[] SelectedIndices {
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		public int Size{
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
		public string Value{
			get{
				string attr = Attributes[""];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes[""] = MapStringAttributeToString(value);
			}
		}
		
	} // class HtmlInputText
} // namespace System.Web.UI.HtmlControls

