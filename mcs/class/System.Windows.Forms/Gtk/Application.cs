//
// System.Windows.Forms.Application
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Windows.Forms
{

	public sealed class Application
	{
		static private ApplicationContext applicationContext = null;
		static private bool messageLoopStarted = false;
		static private bool messageLoopStopRequest = false;
		static private  ArrayList messageFilters = new ArrayList ();
		static private string safeTopLevelCaptionFormat;
		static private bool showingException = false;
		
		//For signiture compatablity. Prevents the auto creation of public constructor
		private Application (){		
		}

		// --- (public) Properties ---
		public static bool AllowQuit{
			// according to docs return false if embbedded in a
			// browser, not (yet?) embedded in a browser
			get{return true;}
		}
		
		
		[MonoTODO]
		public static string CommonAppDataPath{
			get{
				//FIXME:
				return "";
			}
		}

		[MonoTODO]
		public static RegistryKey CommonAppDataRegistry {
			get{throw new NotImplementedException ();}
		}
	
		[MonoTODO]
		public static string CompanyName{
			get{
				AssemblyCompanyAttribute[] attrs =(AssemblyCompanyAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute),true);
				if (attrs != null && attrs[0] != null)
					return attrs[0].Company;
				return "";
			}
		}
	
		[MonoTODO]
		public static CultureInfo CurrentCulture 
		{
			get{return CultureInfo.CurrentCulture;}
			set{Thread.CurrentThread.CurrentCulture = value;}
		}
	
		//[MonoTODO]
		//public static InputLanguage CurrentInputLanguage 
		//{
		//	get { throw new NotImplementedException (); }
		//	set {return;}
		//}
	
		[MonoTODO]
		public static string ExecutablePath{
			get {return Assembly.GetExecutingAssembly().Location;}
		}
	
		[MonoTODO]
		public static string LocalUserAppDataPath {
			get{
				//FIXME:
				return "";
			}
		}
	
		public static bool MessageLoop{
			get {return messageLoopStarted;}
		}
		

			
		[MonoTODO]
		public static string ProductName{
			get{
				AssemblyProductAttribute[] attrs =(AssemblyProductAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute),true);
				if (attrs != null && attrs[0] != null)
					return attrs[0].Product;
				return "";
			}
		}
	
		[MonoTODO]
		public static string ProductVersion{
			get	{
				AssemblyVersionAttribute[] attrs =(AssemblyVersionAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyVersionAttribute),true);
				if (attrs != null && attrs[0] != null)
					return attrs[0].Version;
				return "";
			}
		}
	
		[MonoTODO]
		public static string SafeTopLevelCaptionFormat{
			get{return safeTopLevelCaptionFormat;}
			set{safeTopLevelCaptionFormat = value;}
		}
	
		[MonoTODO]
		public static string StartupPath {
			get{
				//FIXME:
				return "";
			}
		}
	
		[MonoTODO]
		public static string UserAppDataPath{
			get 
			{
				//FIXME:
				return "";
			}
		}
	
		[MonoTODO]
		// Registry key not yet defined
		public static RegistryKey UserAppDataRegistry {
			get { throw new NotImplementedException (); }
		}
		// --- Methods ---
		public static void AddMessageFilter (IMessageFilter value) {
			messageFilters.Add (value);
		}

		//Compact Framework	
		public static void DoEvents (){
			while (Gtk.Application.EventsPending())
					Gtk.Application.RunIteration(); 

		}
		[MonoTODO]
		//.NET version 1.1
		public static void EnableVisualStyles (){
			return;
		}

		//Compact Framework	
		[MonoTODO]
		public static void Exit () {
			DoEvents();
			Gtk.Application.Quit();
			System.Environment.Exit(0);
		}
	
		public static void ExitThread (){
			messageLoopStopRequest = true;
		}
	
		[MonoTODO]
		public static ApartmentState OleRequired (){
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void OnThreadException (Exception t){			
			
			if(Application.ThreadException != null) 
				Application.ThreadException(null, new ThreadExceptionEventArgs(t));
			else{				
				if (!showingException)	{
					
					/*showingException = true;
				
					Form	excepForm = new Form();
					excepForm.ClientSize = new System.Drawing.Size(400, 250);				
					
					TextBox txtLabel = new TextBox();		
					txtLabel.Location = new System.Drawing.Point(30, 30);					
					txtLabel.ReadOnly = true;					
					txtLabel.Multiline = true;
					txtLabel.Size = new System.Drawing.Size(310, 50);		 
					txtLabel.Text = "The application has produced an exception. Press 'Continue' if you want the application to try to continue its execution";										
					excepForm.Controls.Add(txtLabel); 					
					
					TextBox txtError = new TextBox();		
					txtError.Location = new System.Drawing.Point(30, 110);					
					txtError.ReadOnly = true;					
					txtLabel.Multiline = true;
					txtError.Size = new System.Drawing.Size(310, 50);		
					txtError.Text = t.Message;										
					excepForm.Controls.Add(txtError);
					
					StackButton stackbtn = new StackButton(t);		
					stackbtn.Location = new System.Drawing.Point(30, 200);					
					stackbtn.Size = new System.Drawing.Size(100, 30);		
					stackbtn.Text = "Stack Trace";										
					excepForm.Controls.Add(stackbtn); 
					
					ContinueButton continuebtn = new ContinueButton(excepForm);		
					continuebtn.Location = new System.Drawing.Point(160, 200);					
					continuebtn.Size = new System.Drawing.Size(100, 30);		
					continuebtn.Text = "Continue";										
					excepForm.Controls.Add(continuebtn);    		    	    												
					
					QuitButton quitbtn = new QuitButton();		
					quitbtn.Location = new System.Drawing.Point(290, 200);					
					quitbtn.Size = new System.Drawing.Size(100, 30);		
					quitbtn.Text = "Quit";										
					excepForm.Controls.Add(quitbtn);    		    	    												
					
					excepForm.ShowDialog();							
					showingException = false;*/
				}							
				
			}
			
		}
		
		
		public static void RemoveMessageFilter (IMessageFilter value){
			messageFilters.Remove (value);
		}
		public static void Run (ApplicationContext context){
			applicationContext = context;
			applicationContext.MainForm.Show ();
			applicationContext.ThreadExit += new EventHandler( ApplicationFormClosed );
			Run();
		}

		//[TypeAttributes.BeforeFieldInit]
		[MonoTODO]
		public static void Run (Form mainForm){
			// Documents say this parameter name should be mainform, 
			// but the verifier says context.
			//mainForm.CreateControl ();
			ApplicationContext context = new ApplicationContext (mainForm);
			Run (context);
		}
		
		public static void Run(){
			messageLoopStarted = true;
			Gtk.Application.Run();
		}
		

		[MonoTODO]
		static private void ApplicationFormClosed (object o, EventArgs args){
			Exit();
		}

		// --- Events ---
		public static event EventHandler ApplicationExit;
		public static event EventHandler Idle;
		public static event ThreadExceptionEventHandler ThreadException;
		public static event EventHandler ThreadExit; 
		
	}
}
