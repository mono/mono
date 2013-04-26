//
// ServiceControllerTest.cs -
//	NUnit Test Cases for ServiceController
//
// Author:
//	Gert Driesen  (drieseng@users.sourceforge.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
// TODO:
// - Status
// - Start

using System;
using System.ComponentModel;
using System.ServiceProcess;
using TimeoutException = System.ServiceProcess.TimeoutException;

using NUnit.Framework;

namespace MonoTests.System.ServiceProcess
{
	[TestFixture]
	public class ServiceControllerTest
	{
		[Test]
		public void Constructor1 ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();

			try {
				bool value = sc.CanPauseAndContinue;
				Assert.Fail ("#A1: " + value.ToString ());
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
			}

			try {
				bool value = sc.CanShutdown;
				Assert.Fail ("#B1: " + value.ToString ());
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
			}

			try {
				bool value = sc.CanStop;
				Assert.Fail ("#C1: " + value.ToString ());
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
				Assert.IsNull (ex.InnerException, "#C7");
			}

			// closing the ServiceController does not result in exception
			sc.Close ();

			try {
				sc.Continue ();
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#D5");
				Assert.IsNull (ex.ParamName, "#D6");
				Assert.IsNull (ex.InnerException, "#D7");
			}

			try {
				Assert.Fail ("#E1: " + sc.DependentServices.Length);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#E4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#E5");
				Assert.IsNull (ex.ParamName, "#E6");
				Assert.IsNull (ex.InnerException, "#E7");
			}

			Assert.IsNotNull (sc.DisplayName, "#F1");
			Assert.AreEqual (string.Empty, sc.DisplayName, "#F2");

			try {
				sc.ExecuteCommand (0);
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNotNull (ex.Message, "#G3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#G4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#G5");
				Assert.IsNull (ex.ParamName, "#G6");
				Assert.IsNull (ex.InnerException, "#G7");
			}

			Assert.IsNotNull (sc.MachineName, "#H1");
			Assert.AreEqual (".", sc.MachineName, "#H2");


			try {
				sc.Pause ();
				Assert.Fail ("#I1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#I2");
				Assert.IsNotNull (ex.Message, "#I3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#I4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#I5");
				Assert.IsNull (ex.ParamName, "#I6");
				Assert.IsNull (ex.InnerException, "#I7");
			}
		}

		[Test]
		public void Constructor2 ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("lanmanworkstation");

			Assert.IsTrue (sc.CanPauseAndContinue, "#A1");
			Assert.IsTrue (sc.CanShutdown, "#B1");
			Assert.IsTrue (sc.CanStop, "#C1");

			sc.Close ();
			sc.Continue ();

			ServiceController [] dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#D1");
			Assert.IsTrue (dependentServices.Length > 1, "#D2");

			Assert.IsNotNull (sc.DisplayName, "#E1");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#E2");

			Assert.IsNotNull (sc.MachineName, "#F1");
			Assert.AreEqual (".", sc.MachineName, "#F2");

			sc.Refresh ();

			Assert.IsNotNull (sc.ServiceName, "#G1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#G2");

			ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#H1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#H2");

			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#I1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#J1");
		}

		[Test]
		public void Constructor2_Name_Empty ()
		{
			try {
				new ServiceController (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value  for parameter name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("name") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Constructor2_Name_DisplayName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("workstation");

			Assert.IsTrue (sc.CanPauseAndContinue, "#A1");
			Assert.IsTrue (sc.CanShutdown, "#B1");
			Assert.IsTrue (sc.CanStop, "#C1");
		}

		[Test]
		public void Constructor2_Name_Null ()
		{
			try {
				new ServiceController (null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value  for parameter name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("name") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Constructor2_Name_ServiceName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("lanmanworkstation");

			Assert.IsTrue (sc.CanPauseAndContinue, "#A1");
			Assert.IsTrue (sc.CanShutdown, "#B1");
			Assert.IsTrue (sc.CanStop, "#C1");
		}

		[Test]
		public void Constructor3 ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("lanmanworkstation",
				Environment.MachineName);

			Assert.IsTrue (sc.CanPauseAndContinue, "#A1");
			Assert.IsTrue (sc.CanShutdown, "#B1");
			Assert.IsTrue (sc.CanStop, "#C1");

			sc.Close ();
			sc.Continue ();

			ServiceController [] dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#D1");
			Assert.IsTrue (dependentServices.Length > 1, "#D2");

			Assert.IsNotNull (sc.DisplayName, "#E1");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#E2");

			Assert.IsNotNull (sc.MachineName, "#F1");
			Assert.AreEqual (Environment.MachineName, sc.MachineName, "#F2");

			sc.Refresh ();

			Assert.IsNotNull (sc.ServiceName, "#G1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#G2");

			ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#H1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#H2");

			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#I1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#J1");
		}

		[Test]
		public void Constructor3_MachineName_Empty ()
		{
			try {
				new ServiceController ("alerter", string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Constructor3_MachineName_Null ()
		{
			try {
				new ServiceController ("alerter", null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Constructor3_Name_Empty ()
		{
			try {
				new ServiceController (string.Empty, ".");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value  for parameter name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("name") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Constructor3_Name_Null ()
		{
			try {
				new ServiceController (null, ".");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value  for parameter name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("name") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void CanPauseAndContinue ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.IsTrue (sc.CanPauseAndContinue, "#1");
			sc.ServiceName = "SamSs";
			Assert.IsFalse (sc.CanPauseAndContinue, "#2");
			sc.DisplayName = "Workstation";
			Assert.IsTrue (sc.CanPauseAndContinue, "#3");
			sc.MachineName = "doesnotexist";
			try {
				bool value = sc.CanPauseAndContinue;
				Assert.Fail ("#4: " + value.ToString ());
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#5");
				Assert.IsNotNull (ex.Message, "#6");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#7");
				Assert.IsNotNull (ex.InnerException, "#8");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#9");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#10");
				Assert.IsNotNull (win32Error.Message, "#11");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#12");
				Assert.IsNull (win32Error.InnerException, "#13");
			}
		}

		[Test]
		public void CanPauseAndContinue_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				bool canPauseAndContinue = sc.CanPauseAndContinue;
				Assert.Fail ("#1: " + canPauseAndContinue);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void CanPauseAndContinue_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			Assert.IsFalse (sc1.CanPauseAndContinue);
		}

		[Test]
		public void CanPauseAndContinue_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				bool canPauseAndContinue = sc.CanPauseAndContinue;
				Assert.Fail ("#1: " + canPauseAndContinue);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void CanPauseAndContinue_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			Assert.IsFalse (sc1.CanPauseAndContinue);
		}

		[Test]
		public void CanPauseAndContinue_Service_Running ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#1");
			Assert.IsTrue (sc.CanPauseAndContinue, "#2");
		}

		[Test]
		public void CanPauseAndContinue_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanPauseAndContinue, "#B1");
			Assert.IsTrue (sc2.CanPauseAndContinue, "#B2");

			sc1.Pause ();

			try {
				Assert.IsTrue (sc1.CanPauseAndContinue, "#C1");
				Assert.IsTrue (sc2.CanPauseAndContinue, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.IsTrue (sc1.CanPauseAndContinue, "#D1");
				Assert.IsTrue (sc2.CanPauseAndContinue, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanPauseAndContinue, "#E1");
			Assert.IsTrue (sc2.CanPauseAndContinue, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanPauseAndContinue_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanPauseAndContinue, "#B1");
			Assert.IsTrue (sc2.CanPauseAndContinue, "#B2");

			sc1.Stop ();

			try {
				Assert.IsTrue (sc1.CanPauseAndContinue, "#C1");
				Assert.IsTrue (sc2.CanPauseAndContinue, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.IsFalse (sc1.CanPauseAndContinue, "#D1");
				Assert.IsTrue (sc2.CanPauseAndContinue, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanPauseAndContinue, "#E1");
			Assert.IsTrue (sc2.CanPauseAndContinue, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanPauseAndContinue_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				bool canPauseAndContinue = sc.CanPauseAndContinue;
				Assert.Fail ("#1: " + canPauseAndContinue);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void CanShutdown ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.IsTrue (sc.CanShutdown, "#1");
			sc.ServiceName = "SamSs";
			Assert.IsFalse (sc.CanShutdown, "#2");
			sc.DisplayName = "Workstation";
			Assert.IsTrue (sc.CanShutdown, "#3");
			sc.MachineName = "doesnotexist";
			try {
				bool value = sc.CanShutdown;
				Assert.Fail ("#4: " + value.ToString ());
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#5");
				Assert.IsNotNull (ex.Message, "#6");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#7");
				Assert.IsNotNull (ex.InnerException, "#8");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#9");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#10");
				Assert.IsNotNull (win32Error.Message, "#11");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#12");
				Assert.IsNull (win32Error.InnerException, "#13");
			}
		}

		[Test]
		public void CanShutdown_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				bool canShutdown = sc.CanShutdown;
				Assert.Fail ("#1: " + canShutdown);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void CanShutdown_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			Assert.IsFalse (sc1.CanShutdown);
		}

		[Test]
		public void CanShutdown_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				bool value = sc.CanShutdown;
				Assert.Fail ("#1: " + value.ToString ());
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void CanShutdown_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			Assert.IsFalse (sc1.CanShutdown);
		}

		[Test]
		public void CanShutdown_Service_Running ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#1");
			Assert.IsTrue (sc.CanShutdown, "#2");
		}

		[Test]
		public void CanShutdown_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanShutdown, "#B1");
			Assert.IsTrue (sc2.CanShutdown, "#B2");

			sc1.Pause ();

			try {
				Assert.IsTrue (sc1.CanShutdown, "#C1");
				Assert.IsTrue (sc2.CanShutdown, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.IsTrue (sc1.CanShutdown, "#D1");
				Assert.IsTrue (sc2.CanShutdown, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanShutdown, "#E1");
			Assert.IsTrue (sc2.CanShutdown, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanShutdown_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanShutdown, "#B1");
			Assert.IsTrue (sc2.CanShutdown, "#B2");

			sc1.Stop ();

			try {
				Assert.IsTrue (sc1.CanShutdown, "#C1");
				Assert.IsTrue (sc2.CanShutdown, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.IsFalse (sc1.CanShutdown, "#D1");
				Assert.IsTrue (sc2.CanShutdown, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanShutdown, "#E1");
			Assert.IsTrue (sc2.CanShutdown, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanShutdown_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				bool canShutdown = sc.CanShutdown;
				Assert.Fail ("#1: " + canShutdown);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void CanStop ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.IsTrue (sc.CanStop, "#1");
			sc.ServiceName = "SamSs";
			Assert.IsFalse (sc.CanStop, "#2");
			sc.DisplayName = "Workstation";
			Assert.IsTrue (sc.CanStop, "#3");
			sc.MachineName = "doesnotexist";
			try {
				bool value = sc.CanStop;
				Assert.Fail ("#4: " + value.ToString ());
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#5");
				Assert.IsNotNull (ex.Message, "#6");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#7");
				Assert.IsNotNull (ex.InnerException, "#8");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#9");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#10");
				Assert.IsNotNull (win32Error.Message, "#11");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#12");
				Assert.IsNull (win32Error.InnerException, "#13");
			}
		}

		[Test]
		public void CanStop_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				bool canStop = sc.CanStop;
				Assert.Fail ("#1: " + canStop);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void CanStop_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			Assert.IsFalse (sc1.CanStop);
		}

		[Test]
		public void CanStop_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				bool canStop = sc.CanStop;
				Assert.Fail ("#1: " + canStop);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void CanStop_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			Assert.IsFalse (sc1.CanStop);
		}

		[Test]
		public void CanStop_Service_Running ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#1");
			Assert.IsTrue (sc.CanStop, "#2");
		}

		[Test]
		public void CanStop_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanStop, "#B1");
			Assert.IsTrue (sc2.CanStop, "#B2");

			sc1.Pause ();

			try {
				Assert.IsTrue (sc1.CanStop, "#C1");
				Assert.IsTrue (sc2.CanStop, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.IsTrue (sc1.CanStop, "#D1");
				Assert.IsTrue (sc2.CanStop, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanStop, "#E1");
			Assert.IsTrue (sc2.CanStop, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanStop_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			Assert.IsTrue (sc1.CanStop, "#B1");
			Assert.IsTrue (sc2.CanStop, "#B2");

			sc1.Stop ();

			try {
				Assert.IsTrue (sc1.CanStop, "#C1");
				Assert.IsTrue (sc2.CanStop, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.IsFalse (sc1.CanStop, "#D1");
				Assert.IsTrue (sc2.CanStop, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.IsTrue (sc1.CanShutdown, "#E1");
			Assert.IsTrue (sc2.CanShutdown, "#E2");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void CanStop_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				bool canStop = sc.CanStop;
				Assert.Fail ("#1: " + canStop);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Continue ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Pause ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Continue ();

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc1.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}
		}

		[Test]
		public void Continue_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				sc.Continue ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void Continue_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			ServiceController sc2 = new ServiceController ("NetDDE", ".");

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#A2");

			try {
				sc1.Continue ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot resume NetDDE service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("NetDDE") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#C2");
		}

		[Test]
		public void Continue_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				sc.Continue ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void Continue_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			ServiceController sc2 = new ServiceController ("SamSs", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			try {
				sc1.Continue ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot resume SamSs service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("SamSs") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The requested control is not valid for this service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1052, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
		}

		[Test]
		public void Continue_Service_Running ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Continue ();

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");
		}

		[Test]
		public void Continue_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Continue ();
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Cannot resume Schedule service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
				Assert.IsNotNull (ex.InnerException, "#D6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
				Assert.IsNotNull (win32Error.Message, "#D9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#D10");
				Assert.IsNull (win32Error.InnerException, "#D11");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
		}

		[Test]
		public void Continue_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				sc.Continue ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void DependentServices ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = null;
			ServiceController [] dependentServices = null;

			// single dependent service
			sc = new ServiceController ("dmserver", ".");
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#A1");
			Assert.AreEqual (1, dependentServices.Length, "#A2");
			Assert.AreEqual ("dmadmin", dependentServices [0].ServiceName, "#A3");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "alerter";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#B1");
			Assert.AreEqual (1, dependentServices.Length, "#B2");
			Assert.AreEqual ("dmadmin", dependentServices [0].ServiceName, "#B3");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "Spooler";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#C1");
			Assert.AreEqual (1, dependentServices.Length, "#C2");
			Assert.AreEqual ("dmadmin", dependentServices [0].ServiceName, "#C3");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = "doesnotexist";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#D1");
			Assert.AreEqual (1, dependentServices.Length, "#D2");
			Assert.AreEqual ("dmadmin", dependentServices [0].ServiceName, "#D3");

			// no dependent services
			sc = new ServiceController ("alerter", ".");
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#E1");
			Assert.AreEqual (0, dependentServices.Length, "#E2");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "dmserver";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#F1");
			Assert.AreEqual (0, dependentServices.Length, "#F2");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "Workstation";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#G1");
			Assert.AreEqual (0, dependentServices.Length, "#G2");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = Environment.MachineName;
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#H1");
			Assert.AreEqual (0, dependentServices.Length, "#H2");

			// multiple dependent services
			sc = new ServiceController ("TapiSrv", ".");
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#I1");
			Assert.AreEqual (2, dependentServices.Length, "#I2");
			Assert.AreEqual ("RasAuto", dependentServices [0].ServiceName, "#I3");
			Assert.AreEqual ("RasMan", dependentServices [1].ServiceName, "#I4");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "spooler";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#J1");
			Assert.AreEqual (2, dependentServices.Length, "#J3");
			Assert.AreEqual ("RasAuto", dependentServices [0].ServiceName, "#J4");
			Assert.AreEqual ("RasMan", dependentServices [1].ServiceName, "#J5");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "Alerter";
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#K1");
			Assert.AreEqual (2, dependentServices.Length, "#K2");
			Assert.AreEqual ("RasAuto", dependentServices [0].ServiceName, "#K3");
			Assert.AreEqual ("RasMan", dependentServices [1].ServiceName, "#K4");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = Environment.MachineName;
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#L1");
			Assert.AreEqual (2, dependentServices.Length, "#L2");
			Assert.AreEqual ("RasAuto", dependentServices [0].ServiceName, "#L3");
			Assert.AreEqual ("RasMan", dependentServices [1].ServiceName, "#L4");
		}

		[Test]
		public void DependentServices_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("dmserver",
				"doesnotexist");
			try {
				ServiceController [] dependenServices = sc.DependentServices;
				Assert.Fail ("#1: " + dependenServices.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void DependentServices_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("NetDDE", ".");
			ServiceController [] dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#1");
			Assert.AreEqual (1, dependentServices.Length, "#2");
			Assert.AreEqual ("ClipSrv", dependentServices [0].ServiceName, "#3");
		}

		[Test]
		public void DependentServices_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				ServiceController [] dependenServices = sc.DependentServices;
				Assert.Fail ("#1: " + dependenServices.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void DependentServices_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				ServiceController [] dependenServices = sc.DependentServices;
				Assert.Fail ("#1: " + dependenServices.Length);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void DisplayName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.DisplayName = "workstation";
			Assert.AreEqual ("workstation", sc.DisplayName, "#A1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#A2");

			sc.DisplayName = "alerter";
			Assert.AreEqual ("alerter", sc.DisplayName, "#B1");
			Assert.AreEqual ("Alerter", sc.ServiceName, "#B2");

			sc = new ServiceController ("workstation");
			sc.DisplayName = "alerter";
			Assert.AreEqual ("alerter", sc.DisplayName, "#C1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#C2");
			Assert.AreEqual ("workstation", sc.DisplayName, "#C3");

			sc.DisplayName = "alerter";
			Assert.AreEqual ("alerter", sc.DisplayName, "#D1");
			Assert.AreEqual ("Alerter", sc.ServiceName, "#D2");

			sc.DisplayName = "workstation";
			Assert.AreEqual ("workstation", sc.DisplayName, "#E1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#E2");

			sc = new ServiceController ("workstation");
			Assert.AreEqual ("workstation", sc.DisplayName, "#F1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#F2");

			sc.DisplayName = "Workstation";
			Assert.AreEqual ("Workstation", sc.DisplayName, "#G1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#G2");
		}

		[Test]
		public void DisplayName_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("dmserver",
				"doesnotexist");
			try {
				string displayName = sc.DisplayName;
				Assert.Fail ("#1: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void DisplayName_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("NetDDE", ".");
			Assert.AreEqual ("Network DDE", sc.DisplayName);
		}

		[Test]
		public void DisplayName_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				string displayName = sc.DisplayName;
				Assert.Fail ("#1: " + displayName);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void DisplayName_ServiceName_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.DisplayName = "workstation";
			Assert.AreEqual ("workstation", sc.DisplayName, "#A1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#A2");
		}

		[Test]
		public void DisplayName_Value_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.DisplayName = "doesnotexist";
			Assert.AreEqual ("doesnotexist", sc.DisplayName, "#1");
			try {
				string serviceName = sc.ServiceName;
				Assert.Fail ("#2: " + serviceName);
			} catch (InvalidOperationException ex) {
				// Service doesnotexist was not found on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#6");
				Assert.IsNotNull (ex.InnerException, "#7");

				// The specified service does not exist as an installed service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#8");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#9");
				Assert.IsNotNull (win32Error.Message, "#10");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#11");
				Assert.IsNull (win32Error.InnerException, "#12");
			}
			Assert.AreEqual ("doesnotexist", sc.DisplayName, "#13");
		}

		[Test]
		public void DisplayName_Value_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			Assert.AreEqual (string.Empty, sc.DisplayName, "#A1");
			Assert.AreEqual (string.Empty, sc.ServiceName, "#A2");

			sc.DisplayName = "WorkStation";

			Assert.AreEqual ("WorkStation", sc.DisplayName, "#B1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#B2");

			sc.DisplayName = string.Empty;

			Assert.AreEqual (string.Empty, sc.DisplayName, "#C1");
			Assert.AreEqual (string.Empty, sc.ServiceName, "#C2");
		}

		[Test]
		public void DisplayName_Value_Null ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.DisplayName = "Alerter";
			try {
				sc.DisplayName = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
			Assert.AreEqual ("Alerter", sc.DisplayName, "#7");
		}

		[Test]
		public void DisplayName_Value_ServiceName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.DisplayName = "lanmanworkstation";
			Assert.AreEqual ("lanmanworkstation", sc.DisplayName, "#A1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#A2");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#A3");
		}

		[Test]
		public void ExecuteCommand_Device_ControlCodes ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Disk", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			try {
				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_CONTINUE);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNotNull (ex.InnerException, "#B6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
					Assert.IsNotNull (win32Error.Message, "#B9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#B10");
					Assert.IsNull (win32Error.InnerException, "#B11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_DEVICEEVENT);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_HARDWAREPROFILECHANGE);
					Assert.Fail ("#D1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
					Assert.IsNotNull (ex.InnerException, "#D6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
					Assert.IsNotNull (win32Error.Message, "#D9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#D10");
					Assert.IsNull (win32Error.InnerException, "#D11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_INTERROGATE);
					Assert.Fail ("#E1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
					Assert.IsNotNull (ex.Message, "#E3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#E4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#E5");
					Assert.IsNotNull (ex.InnerException, "#E6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#E7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#E8");
					Assert.IsNotNull (win32Error.Message, "#E9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#E10");
					Assert.IsNull (win32Error.InnerException, "#E11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDADD);
					Assert.Fail ("#F1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
					Assert.IsNotNull (ex.Message, "#F3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#F4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#F5");
					Assert.IsNotNull (ex.InnerException, "#F6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#F7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#F8");
					Assert.IsNotNull (win32Error.Message, "#F9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#F10");
					Assert.IsNull (win32Error.InnerException, "#F11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDDISABLE);
					Assert.Fail ("#G1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
					Assert.IsNotNull (ex.Message, "#G3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#G4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#G5");
					Assert.IsNotNull (ex.InnerException, "#G6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#G7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#G8");
					Assert.IsNotNull (win32Error.Message, "#G9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#G10");
					Assert.IsNull (win32Error.InnerException, "#G11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDENABLE);
					Assert.Fail ("#H1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#H2");
					Assert.IsNotNull (ex.Message, "#H3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#H4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#H5");
					Assert.IsNotNull (ex.InnerException, "#H6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#H7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#H8");
					Assert.IsNotNull (win32Error.Message, "#H9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#H10");
					Assert.IsNull (win32Error.InnerException, "#H11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDREMOVE);
					Assert.Fail ("#I1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#I2");
					Assert.IsNotNull (ex.Message, "#I3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#I4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#I5");
					Assert.IsNotNull (ex.InnerException, "#I6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#I7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#I8");
					Assert.IsNotNull (win32Error.Message, "#I9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#I10");
					Assert.IsNull (win32Error.InnerException, "#I11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PARAMCHANGE);
					Assert.Fail ("#J1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#J2");
					Assert.IsNotNull (ex.Message, "#J3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#J4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#J5");
					Assert.IsNotNull (ex.InnerException, "#J6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#J7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#J8");
					Assert.IsNotNull (win32Error.Message, "#J9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#J10");
					Assert.IsNull (win32Error.InnerException, "#J11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PAUSE);
					Assert.Fail ("#K1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#K2");
					Assert.IsNotNull (ex.Message, "#K3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#K4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#K5");
					Assert.IsNotNull (ex.InnerException, "#K6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#K7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#K8");
					Assert.IsNotNull (win32Error.Message, "#K9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#K10");
					Assert.IsNull (win32Error.InnerException, "#K11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_POWEREVENT);
					Assert.Fail ("#L1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#L2");
					Assert.IsNotNull (ex.Message, "#L3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#L4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#L5");
					Assert.IsNotNull (ex.InnerException, "#L6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#L7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#L8");
					Assert.IsNotNull (win32Error.Message, "#L9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#L10");
					Assert.IsNull (win32Error.InnerException, "#L11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_SESSIONCHANGE);
					Assert.Fail ("#M1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M2");
					Assert.IsNotNull (ex.Message, "#M3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#M4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#M5");
					Assert.IsNotNull (ex.InnerException, "#M6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#M7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#M8");
					Assert.IsNotNull (win32Error.Message, "#M9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#M10");
					Assert.IsNull (win32Error.InnerException, "#M11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_SHUTDOWN);
					Assert.Fail ("#N1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
					Assert.IsNotNull (ex.Message, "#N3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#N4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#N5");
					Assert.IsNotNull (ex.InnerException, "#N6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#N7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#N8");
					Assert.IsNotNull (win32Error.Message, "#N9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#N10");
					Assert.IsNull (win32Error.InnerException, "#N11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_STOP);
					Assert.Fail ("#O1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#O2");
					Assert.IsNotNull (ex.Message, "#O3");
					Assert.IsTrue (ex.Message.IndexOf ("Disk") != -1, "#O4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#O5");
					Assert.IsNotNull (ex.InnerException, "#O6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#O7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#O8");
					Assert.IsNotNull (win32Error.Message, "#O9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#O10");
					Assert.IsNull (win32Error.InnerException, "#O11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#P");
		}

		[Test]
		public void ExecuteCommand_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PAUSE);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void ExecuteCommand_Parameter_Incorrect ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			try {
				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNotNull (ex.InnerException, "#B6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
					Assert.IsNotNull (win32Error.Message, "#B9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#B10");
					Assert.IsNull (win32Error.InnerException, "#B11");
				}

				sc.ExecuteCommand (128);
				sc.ExecuteCommand (255);

				try {
					sc.ExecuteCommand (256);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#D");
		}

		[Test]
		public void ExecuteCommand_Service_ContinuePending ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Pause ();

			try {
				sc.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));
				Assert.AreEqual (ServiceControllerStatus.Paused, sc.Status, "#B");

				sc.Continue ();

				sc.ExecuteCommand (128);

				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#E");
		}

		[Test]
		public void ExecuteCommand_Service_ControlCodes ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			try {
				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_CONTINUE);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNotNull (ex.InnerException, "#B6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
					Assert.IsNotNull (win32Error.Message, "#B9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#B10");
					Assert.IsNull (win32Error.InnerException, "#B11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_DEVICEEVENT);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_HARDWAREPROFILECHANGE);
					Assert.Fail ("#D1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
					Assert.IsNotNull (ex.InnerException, "#D6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
					Assert.IsNotNull (win32Error.Message, "#D9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#D10");
					Assert.IsNull (win32Error.InnerException, "#D11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_INTERROGATE);
					Assert.Fail ("#E1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
					Assert.IsNotNull (ex.Message, "#E3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#E4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#E5");
					Assert.IsNotNull (ex.InnerException, "#E6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#E7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#E8");
					Assert.IsNotNull (win32Error.Message, "#E9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#E10");
					Assert.IsNull (win32Error.InnerException, "#E11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDADD);
					Assert.Fail ("#F1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
					Assert.IsNotNull (ex.Message, "#F3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#F4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#F5");
					Assert.IsNotNull (ex.InnerException, "#F6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#F7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#F8");
					Assert.IsNotNull (win32Error.Message, "#F9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#F10");
					Assert.IsNull (win32Error.InnerException, "#F11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDDISABLE);
					Assert.Fail ("#G1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
					Assert.IsNotNull (ex.Message, "#G3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#G4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#G5");
					Assert.IsNotNull (ex.InnerException, "#G6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#G7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#G8");
					Assert.IsNotNull (win32Error.Message, "#G9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#G10");
					Assert.IsNull (win32Error.InnerException, "#G11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDENABLE);
					Assert.Fail ("#H1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#H2");
					Assert.IsNotNull (ex.Message, "#H3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#H4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#H5");
					Assert.IsNotNull (ex.InnerException, "#H6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#H7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#H8");
					Assert.IsNotNull (win32Error.Message, "#H9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#H10");
					Assert.IsNull (win32Error.InnerException, "#H11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_NETBINDREMOVE);
					Assert.Fail ("#I1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#I2");
					Assert.IsNotNull (ex.Message, "#I3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#I4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#I5");
					Assert.IsNotNull (ex.InnerException, "#I6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#I7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#I8");
					Assert.IsNotNull (win32Error.Message, "#I9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#I10");
					Assert.IsNull (win32Error.InnerException, "#I11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PARAMCHANGE);
					Assert.Fail ("#J1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#J2");
					Assert.IsNotNull (ex.Message, "#J3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#J4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#J5");
					Assert.IsNotNull (ex.InnerException, "#J6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#J7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#J8");
					Assert.IsNotNull (win32Error.Message, "#J9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#J10");
					Assert.IsNull (win32Error.InnerException, "#J11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PAUSE);
					Assert.Fail ("#K1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#K2");
					Assert.IsNotNull (ex.Message, "#K3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#K4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#K5");
					Assert.IsNotNull (ex.InnerException, "#K6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#K7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#K8");
					Assert.IsNotNull (win32Error.Message, "#K9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#K10");
					Assert.IsNull (win32Error.InnerException, "#K11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_POWEREVENT);
					Assert.Fail ("#L1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#L2");
					Assert.IsNotNull (ex.Message, "#L3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#L4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#L5");
					Assert.IsNotNull (ex.InnerException, "#L6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#L7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#L8");
					Assert.IsNotNull (win32Error.Message, "#L9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#L10");
					Assert.IsNull (win32Error.InnerException, "#L11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_SESSIONCHANGE);
					Assert.Fail ("#M1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#M2");
					Assert.IsNotNull (ex.Message, "#M3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#M4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#M5");
					Assert.IsNotNull (ex.InnerException, "#M6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#M7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#M8");
					Assert.IsNotNull (win32Error.Message, "#M9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#M10");
					Assert.IsNull (win32Error.InnerException, "#M11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_SHUTDOWN);
					Assert.Fail ("#N1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#N2");
					Assert.IsNotNull (ex.Message, "#N3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#N4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#N5");
					Assert.IsNotNull (ex.InnerException, "#N6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#N7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#N8");
					Assert.IsNotNull (win32Error.Message, "#N9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#N10");
					Assert.IsNull (win32Error.InnerException, "#N11");
				}

				try {
					sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_STOP);
					Assert.Fail ("#O1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#O2");
					Assert.IsNotNull (ex.Message, "#O3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#O4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#O5");
					Assert.IsNotNull (ex.InnerException, "#O6");

					// Access is denied
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#O7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#O8");
					Assert.IsNotNull (win32Error.Message, "#O9");
					Assert.AreEqual (5, win32Error.NativeErrorCode, "#O10");
					Assert.IsNull (win32Error.InnerException, "#O11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#P");
		}

		[Test]
		public void ExecuteCommand_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_INTERROGATE);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void ExecuteCommand_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Pause ();

			try {
				sc.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));
				Assert.AreEqual (ServiceControllerStatus.Paused, sc.Status, "#B");
				sc.ExecuteCommand (154);
				sc.Refresh ();
				Assert.AreEqual (ServiceControllerStatus.Paused, sc.Status, "#C");
				//sc.ExecuteCommand (127);
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#D");
		}

		[Test]
		public void ExecuteCommand_Service_PausePending ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Pause ();

			try {
				sc.ExecuteCommand (128);

				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNotNull (ex.InnerException, "#B6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
					Assert.IsNotNull (win32Error.Message, "#B9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#B10");
					Assert.IsNull (win32Error.InnerException, "#B11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#C");
		}

		[Test]
		public void ExecuteCommand_Service_StartPending ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Stop ();

			try {
				sc.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc.Status, "#B");

				sc.Start ();

				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}

				try {
					sc.ExecuteCommand (128);
					Assert.Fail ("#D1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
					Assert.IsNotNull (ex.InnerException, "#D6");

					// The service cannot accept control messages at this time
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
					Assert.IsNotNull (win32Error.Message, "#D9");
					Assert.AreEqual (1061, win32Error.NativeErrorCode, "#D10");
					Assert.IsNull (win32Error.InnerException, "#D11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#D");
		}

		[Test]
		public void ExecuteCommand_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Stop ();

			try {
				sc.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc.Status, "#B");

				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}

				try {
					sc.ExecuteCommand (128);
					Assert.Fail ("#D1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
					Assert.IsNotNull (ex.Message, "#D3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
					Assert.IsNotNull (ex.InnerException, "#D6");

					// The service has not been started
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
					Assert.IsNotNull (win32Error.Message, "#D9");
					Assert.AreEqual (1062, win32Error.NativeErrorCode, "#D10");
					Assert.IsNull (win32Error.InnerException, "#D11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#E");
		}

		[Test]
		public void ExecuteCommand_Service_StopPending ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule", ".");
			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A");

			sc.Stop ();

			try {
				try {
					sc.ExecuteCommand (127);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNotNull (ex.Message, "#B3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
					Assert.IsNotNull (ex.InnerException, "#B6");

					// The parameter is incorrect
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
					Assert.IsNotNull (win32Error.Message, "#B9");
					Assert.AreEqual (87, win32Error.NativeErrorCode, "#B10");
					Assert.IsNull (win32Error.InnerException, "#B11");
				}

				try {
					sc.ExecuteCommand (128);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Cannot control Schedule service on computer '.'
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNotNull (ex.Message, "#C3");
					Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#C5");
					Assert.IsNotNull (ex.InnerException, "#C6");

					// The service cannot accept control messages at this time
					Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#C7");
					Win32Exception win32Error = (Win32Exception) ex.InnerException;
					//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#C8");
					Assert.IsNotNull (win32Error.Message, "#C9");
					Assert.AreEqual (1061, win32Error.NativeErrorCode, "#C10");
					Assert.IsNull (win32Error.InnerException, "#C11");
				}
			} finally {
				EnsureServiceIsRunning (sc);
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#D");
		}

		[Test]
		public void ExecuteCommand_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				sc.ExecuteCommand ((int) SERVICE_CONTROL_TYPE.SERVICE_CONTROL_INTERROGATE);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void GetDevices ()
		{
			if (RunningOnUnix)
				return;

			ServiceController [] devices = null;

			devices = ServiceController.GetDevices ();
			Assert.IsNotNull (devices, "#A1");

			bool foundDisk = false;
			bool foundAlerter = false;

			foreach (ServiceController sc in devices) {
				switch (sc.ServiceName) {
				case "Disk":
					Assert.AreEqual ("Disk Driver", sc.DisplayName, "#A2");
					Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#A3");
					foundDisk = true;
					break;
				case "Alerter":
					foundAlerter = true;
					break;
				}
			}

			Assert.IsTrue (foundDisk, "#A4");
			Assert.IsFalse (foundAlerter, "#A5");

			devices = ServiceController.GetDevices (Environment.MachineName);
			Assert.IsNotNull (devices, "#B1");

			foundDisk = false;
			foundAlerter = false;

			foreach (ServiceController sc in devices) {
				switch (sc.ServiceName) {
				case "Disk":
					Assert.AreEqual ("Disk Driver", sc.DisplayName, "#B2");
					Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#B3");
					foundDisk = true;
					break;
				case "Alerter":
					foundAlerter = true;
					break;
				}
			}

			Assert.IsTrue (foundDisk, "#B4");
			Assert.IsFalse (foundAlerter, "#B5");
		}

		[Test]
		public void GetDevices_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			try {
				ServiceController [] devices = ServiceController.GetDevices ("doesnotexist");
				Assert.Fail ("#1: " + devices.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void GetDevices_MachineName_Empty ()
		{
			try {
				ServiceController.GetDevices (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void GetDevices_MachineName_Null ()
		{
			try {
				ServiceController.GetDevices (null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void GetServices ()
		{
			if (RunningOnUnix)
				return;

			ServiceController [] services = null;

			services = ServiceController.GetServices ();
			Assert.IsNotNull (services, "#A1");

			bool foundDisk = false;
			bool foundWorkstation = false;

			foreach (ServiceController sc in services) {
				switch (sc.ServiceName) {
				case "Disk":
					foundDisk = true;
					break;
				case "lanmanworkstation":
					foundWorkstation = true;
					break;
				}
			}

			Assert.IsFalse (foundDisk, "#A4");
			Assert.IsTrue (foundWorkstation, "#A5");

			services = ServiceController.GetServices (Environment.MachineName);
			Assert.IsNotNull (services, "#B1");

			foundDisk = false;
			foundWorkstation = false;

			foreach (ServiceController sc in services) {
				switch (sc.ServiceName) {
				case "Disk":
					foundDisk = true;
					break;
				case "lanmanworkstation":
					Assert.AreEqual ("Workstation", sc.DisplayName, "#B2");
					Assert.AreEqual (ServiceControllerStatus.Running, sc.Status, "#B3");
					foundWorkstation = true;
					break;
				}
			}

			Assert.IsFalse (foundDisk, "#B4");
			Assert.IsTrue (foundWorkstation, "#B5");
		}

		[Test]
		public void GetServices_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			try {
				ServiceController [] services = ServiceController.GetServices ("doesnotexist");
				Assert.Fail ("#1: " + services.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void GetServices_MachineName_Empty ()
		{
			try {
				ServiceController.GetServices (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void GetServices_MachineName_Null ()
		{
			try {
				ServiceController.GetServices (null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void MachineName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "alerter";
			Assert.AreEqual ("Alerter", sc.DisplayName, "#A1");
			Assert.AreEqual (".", sc.MachineName, "#A2");
			Assert.AreEqual ("alerter", sc.ServiceName, "#A3");

			sc.MachineName = Environment.MachineName;
			Assert.AreEqual ("Alerter", sc.DisplayName, "#B1");
			Assert.AreEqual (Environment.MachineName, sc.MachineName, "#B2");
			Assert.AreEqual ("alerter", sc.ServiceName, "#B3");

			sc.MachineName = "doesnotexist";
			Assert.AreEqual ("Alerter", sc.DisplayName, "#C1");
			Assert.AreEqual ("doesnotexist", sc.MachineName, "#C2");
			Assert.AreEqual ("alerter", sc.ServiceName, "#C3");

			sc.MachineName = "DoesNotExist";
			Assert.AreEqual ("Alerter", sc.DisplayName, "#D1");
			Assert.AreEqual ("DoesNotExist", sc.MachineName, "#D2");
			Assert.AreEqual ("alerter", sc.ServiceName, "#D3");
		}

		[Test]
		public void MachineName_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.MachineName = Environment.MachineName;
			try {
				sc.MachineName = string.Empty;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
			Assert.AreEqual (Environment.MachineName, sc.MachineName, "#8");
		}

		[Test]
		public void MachineName_Null ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.MachineName = Environment.MachineName;
			try {
				sc.MachineName = null;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// MachineName value  is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("MachineName") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
			Assert.AreEqual (Environment.MachineName, sc.MachineName, "#8");
		}

		[Test]
		public void Pause ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Pause ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#D1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");
		}

		[Test]
		public void Pause_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				sc.Pause ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void Pause_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			ServiceController sc2 = new ServiceController ("NetDDE", ".");

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#A2");

			try {
				sc1.Pause ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot pause NetDDE service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("NetDDE") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#C2");
		}

		[Test]
		public void Pause_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				sc.Pause ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void Pause_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			ServiceController sc2 = new ServiceController ("SamSs", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			try {
				sc1.Pause ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot pause SamSs service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("SamSs") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The requested control is not valid for this service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1052, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
		}

		[Test]
		public void Pause_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Pause ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Pause ();

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
		}

		[Test]
		public void Pause_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Pause ();
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Cannot pause Schedule service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
				Assert.IsNotNull (ex.InnerException, "#D6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
				Assert.IsNotNull (win32Error.Message, "#D9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#D10");
				Assert.IsNull (win32Error.InnerException, "#D11");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
		}

		[Test]
		public void Pause_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				sc.Pause ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void Refresh ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = null;
			ServiceController [] dependentServices = null;
			ServiceController [] servicesDependedOn = null;

			sc = new ServiceController ("NetDDE", ".");
			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#A1");
			Assert.AreEqual (1, dependentServices.Length, "#A2");
			Assert.AreEqual ("ClipSrv", dependentServices [0].ServiceName, "#A3");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#A4");
			Assert.AreEqual (1, servicesDependedOn.Length, "#A5");
			Assert.AreEqual ("NetDDEDSDM", servicesDependedOn [0].ServiceName, "#A6");

			sc.ServiceName = "rasman";
			sc.Refresh ();

			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#B1");
			Assert.AreEqual (1, dependentServices.Length, "#B2");
			Assert.AreEqual ("RasAuto", dependentServices [0].ServiceName, "#B3");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#B4");
			Assert.AreEqual (1, servicesDependedOn.Length, "#B5");
			Assert.AreEqual ("Tapisrv", servicesDependedOn [0].ServiceName, "#B6");

			sc.DisplayName = "NetDDE";
			sc.Refresh ();

			dependentServices = sc.DependentServices;
			Assert.IsNotNull (dependentServices, "#C1");
			Assert.AreEqual (1, dependentServices.Length, "#C2");
			Assert.AreEqual ("ClipSrv", dependentServices [0].ServiceName, "#C3");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#C4");
			Assert.AreEqual (1, servicesDependedOn.Length, "#C5");
			Assert.AreEqual ("NetDDEDSDM", servicesDependedOn [0].ServiceName, "#C6");
		}

		[Test]
		public void Refresh_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			sc.Refresh ();
		}

		[Test]
		public void Refresh_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			sc.Refresh ();
		}

		[Test]
		public void Refresh_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Pause ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.Refresh ();

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc2.Refresh ();

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Paused, sc2.Status, "#E2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void Refresh_Service_Running ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));
				sc2.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#C2");

				sc1.Start ();

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#D2");

				sc1.Refresh ();

				Assert.AreEqual (ServiceControllerStatus.StartPending, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#E2");

				sc1.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#F2");

				sc2.Refresh ();

				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#G1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#G2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#H1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#H2");
		}

		[Test]
		public void Refresh_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc2.Refresh ();

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#E2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void Refresh_ServiceName_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.Refresh ();
		}

		[Test]
		public void ServiceName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "lanmanworkstation";
			Assert.AreEqual ("Workstation", sc.DisplayName, "#A1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#A2");

			sc.ServiceName = "alerter";
			Assert.AreEqual ("Alerter", sc.DisplayName, "#B1");
			Assert.AreEqual ("alerter", sc.ServiceName, "#B2");

			sc = new ServiceController ("lanmanworkstation");
			sc.ServiceName = "alerter";
			Assert.AreEqual ("alerter", sc.ServiceName, "#C1");
			Assert.AreEqual ("Alerter", sc.DisplayName, "#C2");
			Assert.AreEqual ("Alerter", sc.DisplayName, "#C3");

			sc.ServiceName = "alerter";
			Assert.AreEqual ("alerter", sc.ServiceName, "#D1");
			Assert.AreEqual ("Alerter", sc.DisplayName, "#D2");

			sc.ServiceName = "lanmanworkstation";
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#E1");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#E2");

			sc = new ServiceController ("lanmanWorkstation");
			Assert.AreEqual ("lanmanWorkstation", sc.ServiceName, "#F1");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#F2");

			sc.ServiceName = "LanManWorkstation";
			Assert.AreEqual ("LanManWorkstation", sc.ServiceName, "#G1");
			Assert.AreEqual ("Workstation", sc.DisplayName, "#G2");
		}

		[Test]
		public void ServiceName_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("dmserver",
				"doesnotexist");
			try {
				string serviceName = sc.ServiceName;
				Assert.Fail ("#1: " + serviceName);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void ServiceName_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("NetDDE", ".");
			Assert.AreEqual ("NetDDE", sc.ServiceName);
		}

		[Test]
		public void ServiceName_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				string serviceName = sc.ServiceName;
				Assert.Fail ("#1: " + serviceName);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void ServiceName_DisplayName_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "lanmanworkstation";
			Assert.AreEqual ("Workstation", sc.DisplayName, "#1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#2");
		}

		[Test]
		public void ServiceName_Value_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "doesnotexist";
			try {
				string displayName = sc.DisplayName;
				Assert.Fail ("#1: " + displayName);
			} catch (InvalidOperationException ex) {
				// Service doesnotexist was not found on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The specified service does not exist as an installed service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
			Assert.AreEqual ("doesnotexist", sc.ServiceName, "#12");
		}

		[Test]
		public void ServiceName_Value_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			Assert.AreEqual (string.Empty, sc.DisplayName, "#A1");
			Assert.AreEqual (string.Empty, sc.ServiceName, "#A2");

			sc.ServiceName = "lanmanworkstation";

			Assert.AreEqual ("Workstation", sc.DisplayName, "#B1");
			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#B2");

			try {
				sc.ServiceName = string.Empty;
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
				Assert.IsNull (ex.InnerException, "#A7");
			}
		}

		[Test]
		public void ServiceName_Value_Null ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "lanmanworkstation";
			try {
				sc.ServiceName = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}

			Assert.AreEqual ("lanmanworkstation", sc.ServiceName, "#7");
		}

		[Test]
		public void ServiceName_Value_DisplayName ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ();
			sc.ServiceName = "workstation";
			try {
				string displayName = sc.DisplayName;
				Assert.Fail ("#1: " + displayName);
			} catch (InvalidOperationException ex) {
				// Display name could not be retrieved for service workstation
				// on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("workstation") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The specified service does not exist as an installed service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
			Assert.AreEqual ("workstation", sc.ServiceName, "#12");
		}

		[Test]
		public void ServicesDependedOn ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = null;
			ServiceController [] servicesDependedOn = null;

			// single depended service
			sc = new ServiceController ("spooler", ".");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#A1");
			Assert.AreEqual (1, servicesDependedOn.Length, "#A2");
			Assert.AreEqual ("RPCSS", servicesDependedOn [0].ServiceName, "#A3");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "lanmanworkstation";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#B1");
			Assert.AreEqual (1, servicesDependedOn.Length, "#B2");
			Assert.AreEqual ("RPCSS", servicesDependedOn [0].ServiceName, "#B3");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "alerter";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#C1");
			Assert.AreEqual (1, servicesDependedOn.Length, "#C2");
			Assert.AreEqual ("RPCSS", servicesDependedOn [0].ServiceName, "#C3");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = "doesnotexist";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#D1");
			Assert.AreEqual (1, servicesDependedOn.Length, "#D2");
			Assert.AreEqual ("RPCSS", servicesDependedOn [0].ServiceName, "#D3");

			// no depended services
			sc = new ServiceController ("lanmanworkstation", ".");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#E1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#E2");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "spooler";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#F1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#F2");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "Alerter";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#G1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#G2");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = Environment.MachineName;
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#H1");
			Assert.AreEqual (0, servicesDependedOn.Length, "#H2");

			// multiple depended services
			sc = new ServiceController ("dmadmin", ".");
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#I1");
			Assert.AreEqual (3, servicesDependedOn.Length, "#I2");
			// do not rely on the order of the services
			Assert.IsTrue (ContainsService (servicesDependedOn, "RpcSs"), "#I3");
			Assert.IsTrue (ContainsService (servicesDependedOn, "PlugPlay"), "#I4");
			Assert.IsTrue (ContainsService (servicesDependedOn, "DmServer"), "#I5");

			// modifying ServiceName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.ServiceName = "spooler";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#J1");
			Assert.AreEqual (3, servicesDependedOn.Length, "#J2");
			// do not rely on the order of the services
			Assert.IsTrue (ContainsService (servicesDependedOn, "RpcSs"), "#J3");
			Assert.IsTrue (ContainsService (servicesDependedOn, "PlugPlay"), "#J4");
			Assert.IsTrue (ContainsService (servicesDependedOn, "DmServer"), "#J5");

			// modifying DisplayName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.DisplayName = "Alerter";
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#K1");
			Assert.AreEqual (3, servicesDependedOn.Length, "#K2");
			// do not rely on the order of the services
			Assert.IsTrue (ContainsService (servicesDependedOn, "RpcSs"), "#K3");
			Assert.IsTrue (ContainsService (servicesDependedOn, "PlugPlay"), "#K4");
			Assert.IsTrue (ContainsService (servicesDependedOn, "DmServer"), "#K5");

			// modifying MachineName does not cause cache to be cleared:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762
			sc.MachineName = Environment.MachineName;
			servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#L1");
			Assert.AreEqual (3, servicesDependedOn.Length, "#L2");
			// do not rely on the order of the services
			Assert.IsTrue (ContainsService (servicesDependedOn, "RpcSs"), "#L3");
			Assert.IsTrue (ContainsService (servicesDependedOn, "PlugPlay"), "#L4");
			Assert.IsTrue (ContainsService (servicesDependedOn, "DmServer"), "#L5");
		}

		[Test]
		public void ServicesDependedOn_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("dmserver",
				"doesnotexist");
			try {
				ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
				Assert.Fail ("#1: " + servicesDependedOn.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void ServicesDependedOn_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("ClipSrv", ".");
			ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
			Assert.IsNotNull (servicesDependedOn, "#1");
			Assert.AreEqual (1, servicesDependedOn.Length, "#2");
			Assert.AreEqual ("NetDDE", servicesDependedOn [0].ServiceName, "#3");
		}

		[Test]
		public void ServicesDependedOn_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
				Assert.Fail ("#1: " + servicesDependedOn.Length);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void ServicesDependedOn_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				ServiceController [] servicesDependedOn = sc.ServicesDependedOn;
				Assert.Fail ("#1: " + servicesDependedOn.Length);
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void ServiceTypeTest ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = null;
			
			sc = new ServiceController ("dmserver", ".");
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#A1");
			sc.ServiceName = "Disk";
			Assert.AreEqual (ServiceType.KernelDriver, sc.ServiceType, "#A2");
			sc.DisplayName = "Workstation";
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#A3");
			sc.MachineName = "doesnotexist";
			try {
				ServiceType serviceType = sc.ServiceType;
				Assert.Fail ("#A4: " + serviceType);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A5");
				Assert.IsNotNull (ex.Message, "#A6");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#A7");
				Assert.IsNotNull (ex.InnerException, "#A8");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#A9");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#A10");
				Assert.IsNotNull (win32Error.Message, "#A11");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#A12");
				Assert.IsNull (win32Error.InnerException, "#A13");
			}

			sc = new ServiceController ("Disk", ".");
			Assert.AreEqual (ServiceType.KernelDriver, sc.ServiceType, "#B1");
			sc.DisplayName = "Alerter";
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#B2");
			sc.MachineName = Environment.MachineName;
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#B3");
			sc.ServiceName = "Disk";
			Assert.AreEqual (ServiceType.KernelDriver, sc.ServiceType, "#B4");
		}

		[Test]
		public void ServiceType_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("dmserver",
				"doesnotexist");
			try {
				ServiceType serviceType = sc.ServiceType;
				Assert.Fail ("#1: " + serviceType);
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void ServiceType_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("NetDDE", ".");
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType);
		}

		[Test]
		public void ServiceType_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				ServiceType serviceType = sc.ServiceType;
				Assert.Fail ("#1: " + serviceType);
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void ServiceType_ServiceName_Empty ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = null;

			sc = new ServiceController ();
			sc.DisplayName = "workstation";
			Assert.AreEqual (ServiceType.Win32ShareProcess, sc.ServiceType, "#1");

			sc = new ServiceController ();
			sc.DisplayName = "disk driver";
			Assert.AreEqual (ServiceType.KernelDriver, sc.ServiceType, "#2");
		}

		[Test]
		public void Start ()
		{
			// not sure if we need additional tests for this, as it's actually
			// tested as part of the other unit tests
		}

		[Test]
		public void Status ()
		{
			// not sure if we need additional tests for this, as it's actually
			// tested as part of the other unit tests
		}

		[Test]
		public void Stop ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#D1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");
		}

		[Test]
		public void Stop_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				sc.Stop ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void Stop_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			ServiceController sc2 = new ServiceController ("NetDDE", ".");

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#A2");

			try {
				sc1.Stop ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot stop NetDDE service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("NetDDE") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#C2");
		}

		[Test]
		public void Stop_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				sc.Stop ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void Stop_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			ServiceController sc2 = new ServiceController ("SamSs", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			try {
				sc1.Stop ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Cannot stop SamSs service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("SamSs") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#B5");
				Assert.IsNotNull (ex.InnerException, "#B6");

				// The requested control is not valid for this service
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#B7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#B8");
				Assert.IsNotNull (win32Error.Message, "#B9");
				Assert.AreEqual (1052, win32Error.NativeErrorCode, "#B10");
				Assert.IsNull (win32Error.InnerException, "#B11");
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
		}

		[Test]
		public void Stop_Service_Paused ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Pause ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Stop ();

				Assert.AreEqual (ServiceControllerStatus.Paused, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void Stop_Service_Stopped ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.Stop ();
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Cannot stop Schedule service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("Schedule") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#D5");
				Assert.IsNotNull (ex.InnerException, "#D6");

				// The service has not been started
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#D7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#D8");
				Assert.IsNotNull (win32Error.Message, "#D9");
				Assert.AreEqual (1062, win32Error.NativeErrorCode, "#D10");
				Assert.IsNull (win32Error.InnerException, "#D11");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
		}

		[Test]
		public void Stop_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				sc.Stop ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void WaitForStatus ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.Stop ();

			try {
				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#B1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#B2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");

				sc1.WaitForStatus (ServiceControllerStatus.Stopped);

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc1.Start ();

				Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#D1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#D2");

				sc1.WaitForStatus (ServiceControllerStatus.Running);

				Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#E1");
				Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#E2");
			} finally {
				EnsureServiceIsRunning (sc1);
				sc2.Refresh ();
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#F1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#F2");
		}

		[Test]
		public void WaitForStatus_Machine_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("Schedule",
				"doesnotexist");
			try {
				sc.WaitForStatus (ServiceControllerStatus.Stopped,
					new TimeSpan (0, 0, 1));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open Service Control Manager on computer 'doesnotexist'.
				// This operation might require other priviliges
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("'doesnotexist'") != -1, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

				// The RPC server is unavailable
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#6");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#7");
				Assert.IsNotNull (win32Error.Message, "#8");
				Assert.AreEqual (1722, win32Error.NativeErrorCode, "#9");
				Assert.IsNull (win32Error.InnerException, "#10");
			}
		}

		[Test]
		public void WaitForStatus_Service_Disabled ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("NetDDE", ".");
			ServiceController sc2 = new ServiceController ("NetDDE", ".");

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#A2");

			sc1.WaitForStatus (ServiceControllerStatus.Stopped,
				new TimeSpan (0, 0, 1));

			try {
				sc1.WaitForStatus (ServiceControllerStatus.Running,
					new TimeSpan (0, 0, 1));
				Assert.Fail ("#B1");
			} catch (TimeoutException ex) {
				// Time out has expired and the operation has not been completed
				Assert.AreEqual (typeof (TimeoutException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Data, "#B3");
				Assert.AreEqual (0, ex.Data.Count, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			Assert.AreEqual (ServiceControllerStatus.Stopped, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Stopped, sc2.Status, "#C2");
		}

		[Test]
		public void WaitForStatus_Service_DoesNotExist ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc = new ServiceController ("doesnotexist", ".");
			try {
				sc.WaitForStatus (ServiceControllerStatus.Stopped);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Cannot open doesnotexist service on computer '.'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#5");
				Assert.IsNotNull (ex.InnerException, "#6");

				// The filename, directory name, or volume label is incorrect
				Assert.AreEqual (typeof (Win32Exception), ex.InnerException.GetType (), "#7");
				Win32Exception win32Error = (Win32Exception) ex.InnerException;
				//Assert.AreEqual (-2147467259, win32Error.ErrorCode, "#8");
				Assert.IsNotNull (win32Error.Message, "#9");
				Assert.AreEqual (1060, win32Error.NativeErrorCode, "#10");
				Assert.IsNull (win32Error.InnerException, "#11");
			}
		}

		[Test]
		public void WaitForStatus_Service_OperationNotValid ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("SamSs", ".");
			ServiceController sc2 = new ServiceController ("SamSs", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			sc1.WaitForStatus (ServiceControllerStatus.Running,
				new TimeSpan (0, 0, 1));

			try {
				sc1.WaitForStatus (ServiceControllerStatus.Stopped,
					new TimeSpan (0, 0, 1));
				Assert.Fail ("#B1");
			} catch (TimeoutException ex) {
				// Time out has expired and the operation has not been completed
				Assert.AreEqual (typeof (TimeoutException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Data, "#B3");
				Assert.AreEqual (0, ex.Data.Count, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
		}

		[Test]
		public void WaitForStatus_ServiceName_Empty ()
		{
			ServiceController sc = new ServiceController ();
			try {
				sc.WaitForStatus (ServiceControllerStatus.Stopped,
					new TimeSpan (0, 0, 1));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Service name  contains invalid characters, is empty or is
				// too long (max length = 80)
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsTrue (ex.Message.IndexOf ("  ") != -1, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("80") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void WaitForStatus_Timeout ()
		{
			if (RunningOnUnix)
				return;

			ServiceController sc1 = new ServiceController ("Schedule", ".");
			ServiceController sc2 = new ServiceController ("Schedule", ".");

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#A1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#A2");

			try {
				sc1.WaitForStatus (ServiceControllerStatus.Stopped,
					new TimeSpan (0, 0, 1));
				Assert.Fail ("#B1");
			} catch (TimeoutException ex) {
				// Time out has expired and the operation has not been completed
				Assert.AreEqual (typeof (TimeoutException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Data, "#B3");
				Assert.AreEqual (0, ex.Data.Count, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			Assert.AreEqual (ServiceControllerStatus.Running, sc1.Status, "#C1");
			Assert.AreEqual (ServiceControllerStatus.Running, sc2.Status, "#C2");
		}

		private static void EnsureServiceIsRunning (ServiceController sc)
		{
			sc.Refresh ();
			switch (sc.Status) {
			case ServiceControllerStatus.ContinuePending:
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			case ServiceControllerStatus.Paused:
				sc.Continue ();
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			case ServiceControllerStatus.PausePending:
				sc.WaitForStatus (ServiceControllerStatus.Paused, new TimeSpan (0, 0, 5));
				sc.Continue ();
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			case ServiceControllerStatus.StartPending:
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			case ServiceControllerStatus.Stopped:
				sc.Start ();
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			case ServiceControllerStatus.StopPending:
				sc.WaitForStatus (ServiceControllerStatus.Stopped, new TimeSpan (0, 0, 5));
				sc.Start ();
				sc.WaitForStatus (ServiceControllerStatus.Running, new TimeSpan (0, 0, 5));
				break;
			}
		}

		private static bool ContainsService (ServiceController [] services, string serviceName)
		{
			for (int i = 0; i < services.Length; i++) {
				if (services [i].ServiceName == serviceName)
					return true;
			}
			return false;
		}

		private bool RunningOnUnix
		{
			get {
				int p = (int) Environment.OSVersion.Platform;
				return ((p == 4) || (p == 128) || (p == 6));
			}
		}

		private enum SERVICE_CONTROL_TYPE
		{
			SERVICE_CONTROL_STOP = 0x1,
			SERVICE_CONTROL_PAUSE = 0x2,
			SERVICE_CONTROL_CONTINUE = 0x3,
			SERVICE_CONTROL_INTERROGATE = 0x4,
			SERVICE_CONTROL_SHUTDOWN = 0x5,
			SERVICE_CONTROL_PARAMCHANGE = 0x6,
			SERVICE_CONTROL_NETBINDADD = 0x7,
			SERVICE_CONTROL_NETBINDREMOVE = 0x8,
			SERVICE_CONTROL_NETBINDENABLE = 0x9,
			SERVICE_CONTROL_NETBINDDISABLE = 0xA,
			SERVICE_CONTROL_DEVICEEVENT = 0xB,
			SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0xC,
			SERVICE_CONTROL_POWEREVENT = 0xD,
			SERVICE_CONTROL_SESSIONCHANGE = 0xE
		}
	}
}
