//
// Use

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;




public class MyListViewForm : System.Windows.Forms.Form
{
	ColumnHeader	column1 = null;
	ColumnHeader	column2 = null;
	ColumnHeader	column3 = null;
	ColumnHeader	column4 = null;
	myListView listViewCtrl = null;
	ListView.SelectedListViewItemCollection sel = null;
	
	public static void Main(string[] args)
	{
		Application.Run(new MyListViewForm());
	}
	
	public void ClearButton()
	{		
		Console.WriteLine ("MyListViewForm.Clear");				
		listViewCtrl.Clear();		
		
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Columns.Count; i++)
			Console.WriteLine ("Column " +  listViewCtrl.Columns[i].Text + " idx: " + listViewCtrl.Columns[i].Index);		
		
		// Items
		for (int i=0; i < listViewCtrl.Items.Count; i++)
			Console.WriteLine ("Item->" +  listViewCtrl.Items[i].Text + " idx: " + listViewCtrl.Items[i].Index);
	
		// Selected Items
		for (int i=0; i < listViewCtrl.SelectedItems.Count; i++)
			Console.WriteLine ("Sel Item->" +  listViewCtrl.SelectedItems[i].Text + " idx: " + listViewCtrl.SelectedItems[i].Index);
	}
	
	
	public void DelColumnButton()
	{				
		listViewCtrl.Columns.RemoveAt(2);
		
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Columns.Count; i++)
			Console.WriteLine ("Column " +  listViewCtrl.Columns[i].Text + " idx: " + listViewCtrl.Columns[i].Index);		
		
	}
	
	public void DumpSelButton()
	{				
		// Show selected items
		if (sel==null)
		{
			Console.WriteLine ("Col init");
			sel = listViewCtrl.SelectedItems;
		}
		
		Console.WriteLine ("Selected---------------");
		
		for (int i=0; i < sel.Count; i++)
			Console.WriteLine ("Item->" +  sel[i].Text + " idx: " + sel[i].Index);
		
		
	}
	
	public void DelItemButton()
	{	
		Console.WriteLine ("Elements ");
			
		listViewCtrl.Items.RemoveAt(2);			
				
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Items.Count; i++)
			Console.WriteLine ("Items " +  listViewCtrl.Items[i].Text + " idx: " + listViewCtrl.Items[i].Index);
			
		
		
	}
	
	public void ShowClassDefaults()
	{
	
		Console.WriteLine ("Sorting " + listViewCtrl.Sorting);
		Console.WriteLine ("Label Edit " + listViewCtrl.LabelEdit);
		Console.WriteLine ("FullRowSelect " + listViewCtrl.FullRowSelect);
		Console.WriteLine ("GridLines " + listViewCtrl.GridLines);		
		Console.WriteLine ("AutoArrange " + listViewCtrl.AutoArrange);		
		Console.WriteLine ("LabelWrap " + listViewCtrl.LabelWrap);				
		Console.WriteLine ("MultiSelect " + listViewCtrl.MultiSelect);				
		Console.WriteLine ("ForeColor " + listViewCtrl.ForeColor);				
		Console.WriteLine ("BackColor " + listViewCtrl.BackColor);						
		Console.WriteLine ("ItemActivation " + listViewCtrl.Activation);				
		
				
	}
	
	public MyListViewForm()
	{
		InitializeComponent();
	}
	
	private void ColumnSample()
	{
		listViewCtrl = new myListView();
		
		ShowClassDefaults();
		
		// Set params
		listViewCtrl.View = View.Details;			
		//listViewCtrl.LabelEdit = true;			
		listViewCtrl.AllowColumnReorder=true;
		listViewCtrl.FullRowSelect = true;	
		listViewCtrl.GridLines = true;
		listViewCtrl.Activation = ItemActivation.OneClick;
			 
		
    	listViewCtrl.Bounds = new Rectangle(new Point(10,60), new Size(600, 550));
    	ListViewItem item1 = new ListViewItem("item1");
    	ListViewItem item2 = new ListViewItem("item2");
    	ListViewItem item3 = new ListViewItem("item3");
    	ListViewItem item4 = new ListViewItem("item4");
    	ListViewItem item5 = new ListViewItem("item5");
    	ListViewItem item6 = new ListViewItem("item6");
    	ListViewItem item7 = new ListViewItem("item7");
    	ListViewItem item8 = new ListViewItem("item8");
    	ListViewItem item9 = new ListViewItem("item9");
    	ListViewItem item10 = new ListViewItem("item10");
    	
   	    column1 = listViewCtrl.Columns.Add("Column 1", 100, HorizontalAlignment.Left);
   	   	column2 =  listViewCtrl.Columns.Add("Column 2", 75, HorizontalAlignment.Right);
   	   	column3 =  listViewCtrl.Columns.Add("Column 3", 50, HorizontalAlignment.Right);
   	   	column4 =  new ColumnHeader();
   	   	
   	   	column4.Text="Column 4";
   	   	column4.Width= 150;
   	   	
   	   	listViewCtrl.Columns.AddRange(new ColumnHeader[]{column4});
   	    
		listViewCtrl.Items.Add(item1);					
		listViewCtrl.Items.Add(item2);					
		
		listViewCtrl.Items.AddRange(new ListViewItem[]{item3,item4,item5,item6,item7,item8,item9,item10});
				
    	item1.SubItems.Add("sub item 1");        
    	item1.SubItems.Add("sub item 2");        
    	
    	listViewCtrl.Items.Add( new ListViewItem(new string[]{"boy 1", "boy 2", "boy 3"}));
    	
   		DelColumnButton button = new DelColumnButton(this);		
		button.Location = new System.Drawing.Point(5, 10);
		button.Name = "button1";
		button.Size = new System.Drawing.Size(100, 30);		
		button.Text = "Delete Column 2";
		button.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button); 
		
		DelItemButton button2 = new DelItemButton(this);		
		button2.Location = new System.Drawing.Point(630, 90);
		button2.Name = "button2";
		button2.Size = new System.Drawing.Size(100, 30);		
		button2.Text = "Delete Item 3";
		button2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button2); 
		
		DumpSelButton button3 = new DumpSelButton(this);		
		button3.Location = new System.Drawing.Point(630, 120);
		button3.Name = "button3";
		button3.Size = new System.Drawing.Size(100, 30);		
		button3.Text = "Show selection";
		button3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button3); 		
		
		ClearButton button4 = new ClearButton(this);		
		button4.Location = new System.Drawing.Point(630, 150);
		button4.Name = "button4";
		button4.Size = new System.Drawing.Size(100, 30);		
		button4.Text = "Clear";
		button4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button4);    	
		
		/*
		ClearButton button5 = new ClearButton(this);		
		button4.Location = new System.Drawing.Point(630, 150);
		button4.Name = "button4";
		button4.Size = new System.Drawing.Size(100, 30);		
		button4.Text = "Clear";
		button4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button4);    	*/
    	
    	Controls.Add(listViewCtrl);
	}
	
	private void InitializeComponent()
	{
		
		ClientSize = new System.Drawing.Size(750, 650);		
		
		ColumnSample();
		
		return;
		
	Console.WriteLine ("InitializeComponent()");

    // Create a new ListView control.
    ListView listView1 = new ListView();
    listView1.Bounds = new Rectangle(new Point(10,10), new Size(300,200));

    // Set the view to show details.
    //listView1.View = View.Details;
    // Allow the user to edit item text.
    //listView1.LabelEdit = true;
    // Allow the user to rearrange columns.
    //listView1.AllowColumnReorder = true;
    // Display check boxes.
    //listView1.CheckBoxes = true;
    // Select the item and subitems when selection is made.
    //listView1.FullRowSelect = true;
    // Display grid lines.
    //listView1.GridLines = true;
    // Sort the items in the list in ascending order.
    //((listView1.Sorting = SortOrder.Ascending;
    
                
    // Create three items and three sets of subitems for each item.
    //ListViewItem item1 = new ListViewItem("item1",0);
    ListViewItem item1 = new ListViewItem("item1");
    // Place a check mark next to the item.
    
    // Create columns for the items and subitems.
    listView1.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
    //listView1.Columns.Add("Column 2", -2, HorizontalAlignment.Left);


    //Add the items to the ListView.
    //listView1.Items.AddRange(new ListViewItem[]{item1});
    listView1.Items.Add(item1);
    
    //item1.Checked = true;
    item1.SubItems.Add("1");        
    //item1.SubItems.Add("2");
    //item1.SubItems.Add("3");    
       
    //Console.WriteLine ("fi InitializeComponent()" + item1.ListView);
    
    
    //Controls.Add(listView1);
    Controls.Add(listView1);
  	Console.WriteLine ("fi InitializeComponent()");
    
    return;
    
    ListViewItem item2 = new ListViewItem("item2",1);
    item2.SubItems.Add("4");
    item2.SubItems.Add("5");
    item2.SubItems.Add("6");
    ListViewItem item3 = new ListViewItem("item3",0);
    // Place a check mark next to the item.
    item3.Checked = true;
    item3.SubItems.Add("7");
    item3.SubItems.Add("8");
    item3.SubItems.Add("9");

    // Create columns for the items and subitems.
    listView1.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
    listView1.Columns.Add("Column 2", -2, HorizontalAlignment.Left);
    listView1.Columns.Add("Column 3", -2, HorizontalAlignment.Left);
    listView1.Columns.Add("Column 4", -2, HorizontalAlignment.Center);

    //Add the items to the ListView.
            listView1.Items.AddRange(new ListViewItem[]{item1,item2,item3});

    // Create two ImageList objects.
    ImageList imageListSmall = new ImageList();
    ImageList imageListLarge = new ImageList();

    // Initialize the ImageList objects with bitmaps.
    /*imageListSmall.Images.Add(Bitmap.FromFile("C:\\MySmallImage1.bmp"));
    imageListSmall.Images.Add(Bitmap.FromFile("C:\\MySmallImage2.bmp"));
    imageListLarge.Images.Add(Bitmap.FromFile("C:\\MyLargeImage1.bmp"));
    imageListLarge.Images.Add(Bitmap.FromFile("C:\\MyLargeImage2.bmp"));*/

    //Assign the ImageList objects to the ListView.
    listView1.LargeImageList = imageListLarge;
    listView1.SmallImageList = imageListSmall;

    // Add the ListView to the control collection.
    Controls.Add(listView1);
    
	}
}

// Delete column
public class DelColumnButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DelColumnButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DelColumnButton();
		}
}

// Delete item
public class DelItemButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DelItemButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DelItemButton();
		}
}

// Show selection
public class DumpSelButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DumpSelButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DumpSelButton();
		}
}

// ClearButton
public class ClearButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public ClearButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.ClearButton();
		}
}

public class myListView : System.Windows.Forms.ListView
{		

		protected override  void  OnColumnClick(ColumnClickEventArgs e) {				
			Console.WriteLine ("Column " +  Columns[e.Column].Text + " idx: " + Columns[e.Column].Index);			
			

		}
		
		protected override  void  OnBeforeLabelEdit(LabelEditEventArgs e){
			
			Console.WriteLine ("OnBeforeLabelEdit. CancelEdit->" +  e.CancelEdit + " Item-> "+e.Item + " Label->"+e.Label  );						
			
			//e.CancelEdit = true;
			
		}
		
		protected override  void  OnAfterLabelEdit(LabelEditEventArgs e){
			
			Console.WriteLine ("OnAfterLabelEdit. CancelEdit->" +  e.CancelEdit + " Item-> "+e.Item + " Label->"+e.Label  );						
			
			e.CancelEdit = true;
			
		}
		
		protected override  void  OnItemActivate(EventArgs ice){
			
			Console.WriteLine ("OnItemActivate");					
			
			
		}
			

}