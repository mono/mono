//
// ImageCodecInfo class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;

namespace MonoTests.System.Drawing
{

	[TestFixture]	
	public class TestImageCodecInfo 
	{
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		internal void isPresent (ImageCodecInfo[] codecs, string clsid, string formatID)
		{			
			for (int i = 0; i < codecs.Length; i++) {
				if (codecs[i].FormatID.ToString() == formatID) {
					Assert.AreEqual (codecs[i].Clsid.ToString(), clsid);
					return;
				}
			}

			Assert.IsTrue (false);
		}
		
		/*
			This test makes sure that we deliver at least the BMP, GIF, JPEG
			and PNG encoders
		*/
		[Test]
		public void Encoders()
		{
		
			ImageCodecInfo[] decoders =  ImageCodecInfo.GetImageDecoders();			
			ImageCodecInfo[] encoders =  ImageCodecInfo.GetImageEncoders();	


			/* BMP */
			isPresent (encoders, "557cf400-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cab-0728-11d3-9d7b-0000f81ef32e");

			/* GIF */
			isPresent (encoders, "557cf402-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cb0-0728-11d3-9d7b-0000f81ef32e");

			/* JPEG */
			isPresent (encoders, "557cf401-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cae-0728-11d3-9d7b-0000f81ef32e");

			/* PNG */
			isPresent (encoders, "557cf406-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3caf-0728-11d3-9d7b-0000f81ef32e");

		}

		/*
			This test makes sure that we deliver at least the BMP, GIF, JPEG
			and PNG decoders
		*/
		[Test]
		public void Decoders()
		{		
			ImageCodecInfo[] decoders =  ImageCodecInfo.GetImageDecoders();			

			/* BMP */
			isPresent (decoders, "557cf400-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cab-0728-11d3-9d7b-0000f81ef32e");

			/* GIF */
			isPresent (decoders, "557cf402-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cb0-0728-11d3-9d7b-0000f81ef32e");

			/* JPEG */
			isPresent (decoders, "557cf401-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3cae-0728-11d3-9d7b-0000f81ef32e");

			/* PNG */
			isPresent (decoders, "557cf406-1a04-11d3-9a73-0000f81ef32e", 
				"b96b3caf-0728-11d3-9d7b-0000f81ef32e");
		}

	
		
	}
}
