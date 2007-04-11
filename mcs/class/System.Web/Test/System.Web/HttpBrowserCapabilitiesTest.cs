//
// System.Web.HttpBrowserCapabilitiesTest.cs - Unit tests for System.Web.HttpBrowserCapabilities
//
// Author:
//	Adar Wesley <adarw@mainsoft.com>
//
// Copyright (C) 2007 Mainsoft, Inc (http://www.mainsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0
using System;
using System.Web;
using System.Web.UI;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Text;
using System.Web.Configuration;

namespace MonoTests.System.Web
{
	[TestFixture]
	public class HttpBrowserCapabilitiesTest
	{
		[Test]
		[Category("NunitWeb")]
		public void DefaultCapabilities () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (Page_OnLoad));
			t.Run ();
		}

		public static void Page_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;
			Assert.IsNotNull (caps, "Loaded Capabilities");
		}

		[Test]
		[Category ("NunitWeb")]
		public void CapabilitiesUsedBySystemWebIE7 () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CapabilitiesUsedBySystemWebIE7_OnLoad));
			// Set UserAgent string to IE7
			t.Request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
			t.Run ();
		}

		public static void CapabilitiesUsedBySystemWebIE7_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.IsFalse (String.IsNullOrEmpty (caps.Browser), "Browser"); // Used in System.Web code
			Assert.IsTrue (new Version (1, 2) <= caps.EcmaScriptVersion, "EcmaScriptVersion"); // Used in System.Web code
			Assert.IsTrue (0 != caps.MajorVersion, "MajorVersion"); // Used in System.Web code
			Assert.IsFalse (String.IsNullOrEmpty (caps.Type), "Type"); // Used in System.Web code
			Assert.AreEqual (new Version (1, 0), caps.W3CDomVersion, "W3CDomVersion"); // Used in System.Web code
		}

		[Test]
		[Category ("NunitWeb")]
		public void CapabilitiesUsedBySystemWebIE6 () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CapabilitiesUsedBySystemWebIE6_OnLoad));
			// Set UserAgent string to IE6
			t.Request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; InfoPath.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.30)";
			t.Run ();
		}

		public static void CapabilitiesUsedBySystemWebIE6_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.IsFalse (String.IsNullOrEmpty (caps.Browser), "Browser"); // Used in System.Web code
			Assert.IsTrue (new Version (1, 2) <= caps.EcmaScriptVersion, "EcmaScriptVersion"); // Used in System.Web code
			Assert.IsTrue (0 != caps.MajorVersion, "MajorVersion"); // Used in System.Web code
			Assert.IsFalse (String.IsNullOrEmpty (caps.Type), "Type"); // Used in System.Web code
			Assert.AreEqual (new Version (1, 0), caps.W3CDomVersion, "W3CDomVersion"); // Used in System.Web code
		}

		[Test]
		[Category ("NunitWeb")]
		public void CapabilitiesUsedBySystemWebFirefox () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CapabilitiesUsedBySystemWebFirefox_OnLoad));
			// Set UserAgent string to IE7
			t.Request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.1) Gecko/20061204 Firefox/2.0.0.1";
			t.Run ();
		}

		public static void CapabilitiesUsedBySystemWebFirefox_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.IsFalse (String.IsNullOrEmpty(caps.Browser), "Browser"); // Used in System.Web code
			Assert.IsTrue (new Version (1, 2) <= caps.EcmaScriptVersion, "EcmaScriptVersion"); // Used in System.Web code
			Assert.IsTrue (0 != caps.MajorVersion, "MajorVersion"); // Used in System.Web code
			Assert.IsFalse (String.IsNullOrEmpty(caps.Type), "Type"); // Used in System.Web code
			Assert.AreEqual (new Version (1, 0), caps.W3CDomVersion, "W3CDomVersion"); // Used in System.Web code
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void CapabilitiesValues () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (CapabilitiesValues_OnLoad));
			t.Run ();
		}

		public static void CapabilitiesValues_OnLoad (Page p) 
		{
			HttpRequest request = p.Request;
			HttpCapabilitiesBase caps = request.Browser;

			Assert.IsTrue (caps.ActiveXControls, "ActiveXControls");
			Assert.AreEqual (0, caps.Adapters.Count, "Adapters");
			Assert.IsFalse (caps.AOL, "AOL");
			Assert.IsTrue (caps.BackgroundSounds, "BackgroundSounds");
			Assert.IsFalse (caps.Beta, "Beta");
			Assert.AreEqual ("IE", caps.Browser, "Browser"); // Used in System.Web code
			Assert.IsTrue (caps.Browsers.Count > 0, "Browsers.Count > 0");
			Assert.IsTrue (caps.CanCombineFormsInDeck, "CanCombineFormsInDeck");
			Assert.IsFalse (caps.CanInitiateVoiceCall, "CanInitiateVoiceCall");
			Assert.IsTrue (caps.CanRenderAfterInputOrSelectElement, "CanRenderAfterInputOrSelectElement");
			Assert.IsTrue (caps.CanRenderEmptySelects, "CanRenderEmptySelects");
			Assert.IsTrue (caps.CanRenderInputAndSelectElementsTogether, "CanRenderInputAndSelectElementsTogether");
			Assert.IsTrue (caps.CanRenderMixedSelects, "CanRenderMixedSelects");
			Assert.IsTrue (caps.CanRenderOneventAndPrevElementsTogether, "CanRenderOneventAndPrevElementsTogether");
			Assert.IsTrue (caps.CanRenderPostBackCards, "CanRenderPostBackCards");
			Assert.IsTrue (caps.CanRenderSetvarZeroWithMultiSelectionList, "CanRenderSetvarZeroWithMultiSelectionList");
			Assert.IsTrue (caps.CanSendMail, "CanSendMail");
			Assert.IsFalse (caps.CDF, "CDF");
			Assert.IsTrue (caps.Cookies, "Cookies");
			Assert.IsFalse (caps.Crawler, "Crawler");
			Assert.AreEqual (1, caps.DefaultSubmitButtonLimit, "DefaultSubmitButtonLimit");
			Assert.AreEqual (new Version(1, 2), caps.EcmaScriptVersion, "EcmaScriptVersion"); // Used in System.Web code
			Assert.IsTrue (caps.Frames, "frames");
			Assert.AreEqual (0, caps.GatewayMajorVersion, "GatewayMajorVersion");
			Assert.AreEqual (0.0, caps.GatewayMinorVersion, "GatewayMinorVersion");
			Assert.AreEqual ("None", caps.GatewayVersion, "GatewayVersion");
			Assert.IsTrue (caps.HasBackButton, "HasBackButton");
			Assert.IsFalse (caps.HidesRightAlignedMultiselectScrollbars, "HidesRightAlignedMultiselectScrollbars");
			Assert.IsNull (caps.HtmlTextWriter, "HtmlTextWriter");
			Assert.AreEqual ("ie6to9", caps.Id, "Id");
			Assert.AreEqual ("keyboard", caps.InputType, "InputType");
			Assert.IsTrue (caps.IsColor, "IsColor");
			Assert.IsFalse (caps.IsMobileDevice, "IsMobileDevice");
			Assert.IsTrue (caps.JavaApplets, "JavaApplets");
			Assert.IsTrue (caps.JavaScript, "JavaScript");
			Assert.AreEqual (new Version(5, 6), caps.JScriptVersion, "JScriptVersion");
			Assert.AreEqual (6, caps.MajorVersion, "MajorVersion"); // Used in System.Web code
			Assert.AreEqual (10000, caps.MaximumHrefLength, "MaximumHrefLength");
			Assert.AreEqual (300000, caps.MaximumRenderedPageSize, "MaximumRenderedPageSize");
			Assert.AreEqual (5, caps.MaximumSoftkeyLabelLength, "MaximumSoftkeyLabelLength");
			Assert.AreEqual (0.0, caps.MinorVersion, "MinorVersion");
			Assert.AreEqual (".0", caps.MinorVersionString, "MinorVersionString");
			Assert.AreEqual ("Unknown", caps.MobileDeviceManufacturer, "MobileDeviceManufacturer");
			Assert.AreEqual ("Unknown", caps.MobileDeviceModel, "MobileDeviceModel");
			Assert.AreEqual (new Version(6, 0), caps.MSDomVersion, "MSDomVersion");
			Assert.AreEqual (0, caps.NumberOfSoftkeys, "NumberOfSoftkeys");
			Assert.AreEqual ("WinXP", caps.Platform, "Platform");
			Assert.AreEqual ("image/gif", caps.PreferredImageMime, "PreferredImageMime");
			Assert.AreEqual ("text/html", caps.PreferredRenderingMime, "PreferredRenderingMime");
			Assert.AreEqual ("html32", caps.PreferredRenderingType, "PreferredRenderingType");
			Assert.IsNull (caps.PreferredRequestEncoding, "PreferredRequestEncoding");
			Assert.IsNull (caps.PreferredResponseEncoding, "PreferredResponseEncoding");
			Assert.IsFalse (caps.RendersBreakBeforeWmlSelectAndInput, "RendersBreakBeforeWmlSelectAndInput");
			Assert.IsTrue (caps.RendersBreaksAfterHtmlLists, "RendersBreaksAfterHtmlLists");
			Assert.IsFalse (caps.RendersBreaksAfterWmlAnchor, "RendersBreaksAfterWmlAnchor");
			Assert.IsFalse (caps.RendersBreaksAfterWmlInput, "RendersBreaksAfterWmlInput");
			Assert.IsTrue (caps.RendersWmlDoAcceptsInline, "RendersWmlDoAcceptsInline");
			Assert.IsFalse (caps.RendersWmlSelectsAsMenuCards, "RendersWmlSelectsAsMenuCards");
			Assert.IsNull (caps.RequiredMetaTagNameValue, "RequiredMetaTagNameValue");
			Assert.IsFalse (caps.RequiresAttributeColonSubstitution, "RequiresAttributeColonSubstitution");
			Assert.IsFalse (caps.RequiresContentTypeMetaTag, "RequiresContentTypeMetaTag");
			Assert.IsFalse (caps.RequiresControlStateInSession, "RequiresControlStateInSession");
			Assert.IsFalse (caps.RequiresDBCSCharacter, "RequiresDBCSCharacter");
			Assert.IsFalse (caps.RequiresHtmlAdaptiveErrorReporting, "RequiresHtmlAdaptiveErrorReporting");
			Assert.IsFalse (caps.RequiresLeadingPageBreak, "RequiresLeadingPageBreak");
			Assert.IsFalse (caps.RequiresNoBreakInFormatting, "RequiresNoBreakInFormatting");
			Assert.IsFalse (caps.RequiresOutputOptimization, "RequiresOutputOptimization");
			Assert.IsFalse (caps.RequiresPhoneNumbersAsPlainText, "RequiresPhoneNumbersAsPlainText");
			Assert.IsFalse (caps.RequiresSpecialViewStateEncoding, "RequiresSpecialViewStateEncoding");
			Assert.IsFalse (caps.RequiresUniqueFilePathSuffix, "RequiresUniqueFilePathSuffix");
			Assert.IsFalse (caps.RequiresUniqueHtmlCheckboxNames, "RequiresUniqueHtmlCheckboxNames");
			Assert.IsFalse (caps.RequiresUniqueHtmlInputNames, "RequiresUniqueHtmlInputNames");
			Assert.IsFalse (caps.RequiresUrlEncodedPostfieldValues, "RequiresUrlEncodedPostfieldValues");
			Assert.AreEqual (8, caps.ScreenBitDepth, "ScreenBitDepth");
			Assert.AreEqual (40, caps.ScreenCharactersHeight, "ScreenCharactersHeight");
			Assert.AreEqual (80, caps.ScreenCharactersWidth, "ScreenCharactersWidth");
			Assert.AreEqual (480, caps.ScreenPixelsHeight, "ScreenPixelsHeight");
			Assert.AreEqual (640, caps.ScreenPixelsWidth, "ScreenPixelsWidth");
			Assert.IsFalse (caps.SupportsAccesskeyAttribute, "SupportsAccesskeyAttribute");
			Assert.IsTrue (caps.SupportsBodyColor, "SupportsBodyColor");
			Assert.IsTrue (caps.SupportsBold, "SupportsBold");
			Assert.IsTrue (caps.SupportsCacheControlMetaTag, "SupportsCacheControlMetaTag");
			Assert.IsTrue (caps.SupportsCallback, "SupportsCallback");
			Assert.IsTrue (caps.SupportsCss, "SupportsCss");
			Assert.IsTrue (caps.SupportsDivAlign, "SupportsDivAlign");
			Assert.IsTrue (caps.SupportsDivNoWrap, "SupportsDivNoWrap");
			Assert.IsTrue (caps.SupportsEmptyStringInCookieValue, "SupportsEmptyStringInCookieValue");
			Assert.IsTrue (caps.SupportsFontColor, "SupportsFontColor");
			Assert.IsTrue (caps.SupportsFontName, "SupportsFontName");
			Assert.IsTrue (caps.SupportsFontSize, "SupportsFontSize");
			Assert.IsTrue (caps.SupportsImageSubmit, "SupportsImageSubmit");
			Assert.IsFalse (caps.SupportsIModeSymbols, "SupportsIModeSymbols");
			Assert.IsFalse (caps.SupportsInputIStyle, "SupportsInputIStyle");
			Assert.IsFalse (caps.SupportsInputMode, "SupportsInputMode");
			Assert.IsTrue (caps.SupportsItalic, "SupportsItalic");
			Assert.IsFalse (caps.SupportsJPhoneMultiMediaAttributes, "SupportsJPhoneMultiMediaAttributes");
			Assert.IsFalse (caps.SupportsJPhoneSymbols, "SupportsJPhoneSymbols");
			Assert.IsTrue (caps.SupportsQueryStringInFormAction, "SupportsQueryStringInFormAction");
			Assert.IsTrue (caps.SupportsRedirectWithCookie, "SupportsRedirectWithCookie");
			Assert.IsTrue (caps.SupportsSelectMultiple, "SupportsSelectMultiple");
			Assert.IsTrue (caps.SupportsUncheck, "SupportsUncheck");
			Assert.IsTrue (caps.SupportsXmlHttp, "SupportsXmlHttp");
			Assert.IsTrue (caps.Tables, "Tables");
			Assert.AreEqual ("IE6", caps.Type, "Type"); // Used in System.Web code
			Assert.IsTrue (caps.UseOptimizedCacheKey, "UseOptimizedCacheKey");
			Assert.IsTrue (caps.VBScript, "VBScript");
			Assert.AreEqual ("6.0", caps.Version, "Version");
			Assert.AreEqual (new Version (1, 0), caps.W3CDomVersion, "W3CDomVersion"); // Used in System.Web code
			Assert.IsFalse (caps.Win16, "Win16");
			Assert.IsTrue (caps.Win32, "Win32");
		}
	}
}
#endif