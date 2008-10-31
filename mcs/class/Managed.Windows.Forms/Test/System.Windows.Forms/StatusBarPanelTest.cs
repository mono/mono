//
// StatusBarPanelTest.cs: Test cases for StatusBar.
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class StatusBarPanelTest : TestHelper 
	{
		[Test]
		public void MinimumWidth1 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual (10, p.MinWidth, "1");
		}

		[Test]
		public void MinimumWidth2 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 100;
			Assert.AreEqual (100, p.Width, "1");
		}

		[Test]
		public void MinimumWidth3 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 200;
			p.MinWidth = 25;
			Assert.AreEqual (200, p.Width, "#1");
			
			p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 25;
			Assert.AreEqual (50, p.Width, "#2");
			
			p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 100;
			Assert.AreEqual (100, p.Width, "#3");
			
			p = new StatusBarPanel ();
			p.MinWidth = 200;
			Assert.AreEqual (200, p.Width, "#4");
		}
		
		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException(typeof(ArgumentException))]
#endif
		public void MinimumWidth4 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.MinWidth = -50;
		}

		[Test]
		public void MinWidth_AutoSize_None ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			Assert.AreEqual (10, p.MinWidth, "#1");
		}
		
		[Test]
		public void ToStringTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual ("StatusBarPanel: {}", p.ToString(), "1");

			p.Text = "Hello";
			Assert.AreEqual ("StatusBarPanel: {Hello}", p.ToString(), "2");

			p.Text = "}";
			Assert.AreEqual ("StatusBarPanel: {}}", p.ToString(), "3");
		}
		
		[Test]
		public void DefaultPropertiesTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual (100, p.Width, "#1");
			Assert.AreEqual (10, p.MinWidth, "#2");
			Assert.AreEqual (String.Empty, p.Text, "#3");
			Assert.AreEqual (HorizontalAlignment.Left, p.Alignment, "#4");
			Assert.AreEqual (StatusBarPanelAutoSize.None, p.AutoSize, "#5");
			Assert.AreEqual (StatusBarPanelBorderStyle.Sunken, p.BorderStyle, "#6");
			Assert.AreEqual (StatusBarPanelStyle.Text, p.Style, "#7");
			Assert.AreEqual (String.Empty, p.ToolTipText, "#8");
			
		}
		
		[Test] // bug 82487
		public void IconWidth ()
		{
			using (Form f = new Form ()) {
				StatusBar _statusBar;
				StatusBarPanel _myComputerPanel;
				
				_statusBar = new StatusBar ();
				_statusBar.ShowPanels = true;
				//Controls.Add (_statusBar);
				
				_myComputerPanel = new StatusBarPanel ();
				_myComputerPanel.AutoSize = StatusBarPanelAutoSize.Contents;
				_myComputerPanel.Text = "My Computer";
				_statusBar.Panels.Add (_myComputerPanel);

				int width = _myComputerPanel.Width;
				
				_myComputerPanel.Icon = f.Icon;
				
				Assert.AreEqual (width + 21, _myComputerPanel.Width, "#01");
			}
		}
#if NET_2_0
		[Test]
		public void TagTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			
			Assert.AreEqual (null, p.Tag, "#1");
			p.Tag = "a";
			Assert.AreEqual ("a", p.Tag, "#2");
			p.Tag = null;
			Assert.AreEqual (null, p.Tag, "#3");
		}
		
		[Test]
		public void NameTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual ("", p.Name, "#1");
			p.Name = "a";
			Assert.AreEqual ("a", p.Name, "#2");
			p.Name = null;
			Assert.AreEqual ("", p.Name, "#3");
		}
#endif 
		
	}
}
