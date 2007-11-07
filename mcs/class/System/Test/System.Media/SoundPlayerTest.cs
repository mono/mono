//
// SoundPlayerTest.cs - NUnit Test Cases for System.Media.SoundPlayer
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2007 Gert Driesen
// 

#if NET_2_0

using System;
using System.IO;
using System.Media;

using NUnit.Framework;

namespace MonoTests.System.Media
{
	[TestFixture]
	public class SoundPlayerTest
	{
		private int stream_changed;

		[SetUp]
		public void SetUp ()
		{
			stream_changed = 0;
		}

		[Test] // ctor ()
		public void Constructor0 ()
		{
			SoundPlayer player = new SoundPlayer ();
			Assert.IsNull (player.Container, "#1");
			Assert.IsFalse (player.IsLoadCompleted, "#2");
			Assert.AreEqual (10000, player.LoadTimeout, "#3");
			Assert.IsNull (player.Site, "#4");
			Assert.IsNotNull (player.SoundLocation, "#5");
			Assert.AreEqual (string.Empty, player.SoundLocation, "#6");
			Assert.IsNull (player.Stream, "#7");
			Assert.IsNull (player.Tag, "#8");
		}

		[Test] // ctor (Stream)
		public void Constructor1 ()
		{
			MemoryStream ms = new MemoryStream ();

			SoundPlayer player = new SoundPlayer (ms);
			Assert.IsFalse (player.IsLoadCompleted, "#A1");
			Assert.IsNotNull (player.SoundLocation, "#A2");
			Assert.AreEqual (string.Empty, player.SoundLocation, "#A3");
			Assert.AreSame (ms, player.Stream, "#A4");

			player = new SoundPlayer ((Stream) null);
			Assert.IsFalse (player.IsLoadCompleted, "#B1");
			Assert.IsNotNull (player.SoundLocation, "#B2");
			Assert.AreEqual (string.Empty, player.SoundLocation, "#B3");
			Assert.IsNull (player.Stream, "#B4");
		}

		[Test] // ctor (string)
		public void Constructor2 ()
		{
			string location = "whatever";

			SoundPlayer player = new SoundPlayer (location);
			Assert.IsFalse (player.IsLoadCompleted, "#1");
			Assert.AreSame (location, player.SoundLocation, "#2");
			Assert.AreEqual ("whatever", player.SoundLocation, "#3");
			Assert.IsNull (player.Stream, "#4");
		}

		[Test]
		public void Stream ()
		{
			MemoryStream ms = new MemoryStream ();

			SoundPlayer player = new SoundPlayer ();
			player.StreamChanged += new EventHandler (StreamChangedCounter);
			player.Stream = ms;
			Assert.AreSame (ms, player.Stream, "#A1");
			Assert.IsFalse (player.IsLoadCompleted, "#A2");
			Assert.AreEqual (1, stream_changed, "#A3");
			player.Stream = ms;
			Assert.AreSame (ms, player.Stream, "#B1");
			Assert.IsFalse (player.IsLoadCompleted, "#B2");
			Assert.AreEqual (1, stream_changed, "#B3");
			player.Stream = null;
			Assert.IsNull (player.Stream, "#C1");
			Assert.IsFalse (player.IsLoadCompleted, "#C2");
			Assert.AreEqual (2, stream_changed, "#C3");
			player.Stream = null;
			Assert.IsNull (player.Stream, "#D1");
			Assert.IsFalse (player.IsLoadCompleted, "#D2");
			Assert.AreEqual (2, stream_changed, "#D3");
		}

		void StreamChangedCounter (object sender, EventArgs e)
		{
			stream_changed++;
		}
	}
}

#endif
