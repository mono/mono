//
// Test application for the FontDialog class implementation
//
// Author:
//   Jordi Mas i Hernàndez, jmas@softcatala.org
//

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

//
namespace FontDialogTest
{
	public class myButton : System.Windows.Forms.Button
	{
		public FontDialog fontDialog = null;				
		public TestForm testForm = null;
				
		public FontDialog fontdlg {	get { return fontDialog;  }	}
		
	
		public myButton(TestForm testFrm) : base()
		{
			fontDialog = new FontDialog();				
			testForm = testFrm;
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{		
			
			// Show default values			
			Console.WriteLine("Default values---");							
		  	Console.WriteLine("AllowScriptChange " 	+ fontDialog.AllowScriptChange);							
		  	Console.WriteLine("Color " +  fontDialog.Color);							
		  	Console.WriteLine("FixedPitchOnly " +  fontDialog.FixedPitchOnly);							
		  	Console.WriteLine("Font " +  fontDialog.Font);							
		  	Console.WriteLine("FontMustExist " +  fontDialog.FontMustExist);							
		  	Console.WriteLine("MaxSize " +  fontDialog.MaxSize);							
		  	Console.WriteLine("MinSize " +  fontDialog.MinSize);									  	
		  	Console.WriteLine("ScriptsOnly " +  fontDialog.ScriptsOnly);							
		  	Console.WriteLine("ShowApply " +  fontDialog.ShowApply);							
		  	Console.WriteLine("ShowColor " +  fontDialog.ShowColor);							
		  	Console.WriteLine("ShowEffects " +  fontDialog.ShowEffects);							
		  	Console.WriteLine("ShowHelp " +  fontDialog.ShowHelp);							
		  	
		  	testForm.Update();	
		  	
		    fontDialog.ShowColor = true;		    		    
		    fontDialog.Color = Color.Red;
		    fontDialog.MinSize = 10;
		    fontDialog.MaxSize = 12;
		  
		    if(fontDialog.ShowDialog(this) != DialogResult.Cancel )
		    {
		    	Console.WriteLine("Seleted Font " +  fontDialog.Font.FontFamily.Name);							
		    	Console.WriteLine("Seleted Size " + fontDialog.Font.Size);								    			      
		    	Console.WriteLine("Seleted Color " + fontDialog.Color);								    			      
		    	testForm.Update();	
		    }			    
		    else
		    {		    	
		    	fontDialog.Reset();
		    	testForm.Update();	
		    }
		    		
		}
	}


public class TestForm : System.Windows.Forms.Form
{
	
	TextBox	fontName = null;
	TextBox	fontSize = null;
	TextBox	fontColor = null;
	myButton button = null;		
	
	public static void Main(string[] args)
	{
		Application.Run(new TestForm());
	}
	
	public TestForm()
	{
		InitializeComponent();
	}
	
	private void InitializeComponent()
	{			
		Text = "Test application for the FontDialog class implementation";	
		
		FontDialog fontDialog = new FontDialog();				
		ClientSize = new System.Drawing.Size(300, 300);		
		
		button = new myButton(this);		
		button.Location = new System.Drawing.Point(5, 10);
		button.Name = "button20";
		button.Size = new System.Drawing.Size(100, 30);		
		button.Text = "Press me baby";
		button.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button); 
		
		fontName = new TextBox();
		fontName.Location = new System.Drawing.Point(5, 60);
		fontName.Name = "FontName";
		fontName.Size = new System.Drawing.Size(200, 30);				
		fontName.ReadOnly = true;
		Controls.Add(fontName); 		   
		
		fontSize = new TextBox();
		fontSize.Location = new System.Drawing.Point(5, 100);
		fontSize.Name = "FontSize";
		fontSize.Size = new System.Drawing.Size(200, 30);				
		fontSize.ReadOnly = true;
		Controls.Add(fontSize); 		   
		
		fontColor = new TextBox();
		fontColor.Location = new System.Drawing.Point(5, 140);
		fontColor.Name = "FontSize";
		fontColor.Size = new System.Drawing.Size(200, 30);				
		fontColor.ReadOnly = true;
		Controls.Add(fontColor); 		   
		
    	return;     
	}
	
	public new void Update()
	{
		//fontName.Text = "Font: " + button.fontDialog.MaxSize + " "+ button.fontDialog.MinSize;
		fontName.Text = "Font: " + button.fontDialog.Font.FontFamily.Name;
		fontSize.Text = "Size: " + button.fontDialog.Font.Size;
		fontColor.Text = "Color: " + button.fontDialog.Color;
	}
}

}


