//
// System.Windows.Forms.FolderBrowserDialog.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
//
// (C) 2002-3 Ximian, Inc
//

using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
	using System.Runtime.Remoting;
	using System.ComponentModel;

	// Beta specs do not specify what class to defrive from.
	// Using CommonDialog because 
	public class FolderBrowserDialog : CommonDialog  {

		string description;
		string selectedPath;
		Environment.SpecialFolder folder = Environment.SpecialFolder.Desktop;		
		bool bShowNewFolderButton;		
		
		private IntPtr SpecialFolderConv(Environment.SpecialFolder fldr) {
			
			IntPtr nRslt = IntPtr.Zero;
			
			switch (fldr) 
			{
				case Environment.SpecialFolder.ApplicationData:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_APPDATA;
					break;
				
				case Environment.SpecialFolder.CommonApplicationData:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_COMMON_APPDATA;
					break;
					
				case Environment.SpecialFolder.CommonProgramFiles:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_COMMON_PROGRAMS;
					break;
				
				case Environment.SpecialFolder.Cookies:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_COOKIES;
					break;
				
				case Environment.SpecialFolder.Desktop:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_DESKTOP;
					break;
				
				case Environment.SpecialFolder.DesktopDirectory:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_DESKTOPDIRECTORY;
					break;
					
				case Environment.SpecialFolder.Favorites:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_FAVORITES;
					break;
				
				case Environment.SpecialFolder.History:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_HISTORY;
					break;
					
				case Environment.SpecialFolder.InternetCache:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_INTERNET_CACHE;
					break;
				
				case Environment.SpecialFolder.LocalApplicationData:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_LOCAL_APPDATA;
					break;
					
				//case Environment.SpecialFolder.MyComputer: //TODO: Which value?
					//nRslt = (IntPtr) ShellSpecialFolder.;
					//break;
				
				case Environment.SpecialFolder.MyMusic:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_MYMUSIC;
					break;
					
				case Environment.SpecialFolder.MyPictures:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_MYPICTURES;
					break;
				
				case Environment.SpecialFolder.Personal:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_PERSONAL;
					break;
					
				case Environment.SpecialFolder.ProgramFiles:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_PROGRAM_FILES;
					break;
				
				case Environment.SpecialFolder.Programs:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_PROGRAMS;
					break;
					
				case Environment.SpecialFolder.Recent:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_RECENT;
					break;
				
				case Environment.SpecialFolder.SendTo:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_SENDTO;
					break;
					
				case Environment.SpecialFolder.StartMenu:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_STARTMENU;
					break;
					
				case Environment.SpecialFolder.Startup:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_STARTUP;
					break;
					
				case Environment.SpecialFolder.System:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_SYSTEM;
					break;
					
				case Environment.SpecialFolder.Templates:
					nRslt = (IntPtr) ShellSpecialFolder.CSIDL_TEMPLATES;
					break;													
					
				default:
					throw new  InvalidEnumArgumentException();	
			}	
			
			return nRslt;
		}
		//
		//  --- Constructor
		//
		
		public FolderBrowserDialog() {		
				
				Reset();		
		}

		
		public override void Reset(){
			
			description = "";
			selectedPath  = "";
			folder = Environment.SpecialFolder.Desktop;		
			bShowNewFolderButton = true;		
		}

		
		protected override bool RunDialog(IntPtr hWndOwner){
			
			BROWSEINFO		bi = new BROWSEINFO();
			IntPtr			pidl = IntPtr.Zero;			
			IntPtr olePath = Marshal.AllocHGlobal(2048);						
			int nRet = 0;			
			
			bi.hwndOwner = hWndOwner;
			bi.pidlRoot = (IntPtr) SpecialFolderConv(RootFolder);			
			bi.lpszTitle      = Description;
			bi.ulFlags        = (uint) (BrowseDirFlags.BIF_RETURNONLYFSDIRS | BrowseDirFlags.BIF_STATUSTEXT);
			bi.lpfn           = IntPtr.Zero;
			bi.lParam         = IntPtr.Zero;
			bi.iImage  = 0;					
			bi.pszDisplayName = olePath;
			pidl = Win32.SHBrowseForFolder(ref bi);
			
		    Marshal.FreeHGlobal(olePath);
		    
			if (pidl==IntPtr.Zero) return false;
			
			StringBuilder sBuilder = new StringBuilder();
			
			nRet = Win32.SHGetPathFromIDList(pidl, sBuilder);
				
			// TODO: Dealocate the pidl returned by Win32.SHBrowseForFolder
			//	Win32.SHFreeMalloc(pidl);						
						    
            if (nRet==0) return false;   
			
			selectedPath = sBuilder.ToString();
			return true;
		}

		//
		//  --- Public Properties
		//

		public string Description {
			get {return description;}
			set {description = value;}
		}

		//beta docs do not have accessor.
		//protected bool DesignMode {
		//}

		//protected EventHandlerList Events {
		//}

		public Environment.SpecialFolder RootFolder {
			get {return folder;}
			set {folder = value;}
		}

		public string SelectedPath {
			get {return selectedPath;}
			set {selectedPath = value;}
		}

		public bool ShowNewFolderButton {
			get {return bShowNewFolderButton;}
			set {bShowNewFolderButton = value;}
		}

		//public virtual System.ComponentModel.IContainer Container {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		// FIXME: beta 1.1 says the following should be public virtual ISite Site {
		// but the compiler gives warning that it must be new.
		// Probably system.component needs to change to be beta 1.1 compliant
		// looks fixed on 9/28/2003
		public virtual ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		//Specs seem to say they need to be here, but adding them conflicts with commondialog : component.disposed/helprequest
		//public event EventHandler Disposed;
		//public event EventHandler HelpRequest;

	}
}







