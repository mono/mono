/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       RegexEditorDialog
 *
 * Author:      Gaurav Vaish
 * Maintainer:  mastergaurav AT users DOT sf DOT net
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls
{
	public class RegexEditorDialog : Form
	{
		private ISite     site;
		private Container components;
		private bool      isActivated;
		private bool      sVal;
		private string regularExpression = String.Empty;

		private Button   helpBtn;
		private Button   testValBtn;
		private Button   okBtn;
		private Button   cancelBtn;

		private Label    inputLabel;
		private Label    testResultsLabel;
		private Label    stdExprLabel;
		private Label    exprLabel;

		private ListBox  stdExprsList;

		private TextBox  exprText;
		private TextBox  sampleText;

		private GroupBox exprGrp;

		public RegexEditorDialog(ISite site) : base()
		{
			this.site        = site;
			this.isActivated = false;
			this.sVal        = false;

			InitializeComponents();
		}

		[MonoTODO]
		private void InitializeComponents()
		{
			components = new Container();

			helpBtn    = new Button();
			testValBtn = new Button();
			okBtn      = new Button();
			cancelBtn  = new Button();

			inputLabel       = new Label();
			testResultsLabel = new Label();
			stdExprLabel     = new Label();
			exprLabel        = new Label();

			stdExprsList = new ListBox();

			exprText   = new TextBox();
			sampleText = new TextBox();

			exprGrp = new GroupBox();

			System.Drawing.Font cFont = System.Windows.Forms.Control.DefaultFont;
			IUIService service = (IUIService)site.GetService(typeof(IUIService));
			if(service != null)
			{
				cFont = (Font)(service.Styles["DialogFont"]);
			}
			throw new NotImplementedException();
		}

		public string RegularExpression
		{
			get
			{
				return regularExpression;
			}
			set
			{
				regularExpression = value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		[MonoTODO]
		private object[] CannedExpressions
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		private class CannedExpression
		{
			public string Description;
			public string Expression;

			public CannedExpression(string description, string expression)
			{
				Description = description;
				Expression  = expression;
			}

			public override string ToString()
			{
				return Description;
			}
		}
	}
}
