// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

using System;
using System.IO;
using System.Collections;
using NUnit.Framework;

namespace GHTUtils.Base
{
	public class GHTBase
	{
		#region Constructors	
		/// <summary>Constructor 
		/// <param name="Logger">Custom TextWriter to log to</param>
		/// <param name="LogOnSuccess">False to log only failed TestCases, True to log all</param>
		/// </summary>
		protected GHTBase(TextWriter Logger, bool LogOnSuccess)
		{
			this._logger = Logger;
			this._logOnSuccess = LogOnSuccess;
			this._testName = this.GetType().Name;
		}

		/// <summary>Constructor, log to Console
		/// <param name="LogOnSuccess">False to log only failed TestCases, True to log all</param>
		/// </summary>
		protected GHTBase(bool LogOnSuccess):this(Console.Out, LogOnSuccess){}

		/// <summary>Constructor, log to Console only when Failed 
		/// </summary>
		protected GHTBase():this(Console.Out, false){}
		#endregion

		#region protected methods

		public void GHTSetLogger(TextWriter Logger)
		{
			this._logger = Logger;
		}
		/// <summary>Begin Test which containes TestCases
		/// <param name="testName">Test name, used on logs</param>
		/// </summary>
		public virtual void BeginTest(string testName)
		{
			//set test name
			this._testName = testName;
			//reset the Failure Counter and the TestCase Number
			UniqueId.ResetCounters();

			if(this._logOnSuccess == true)
				Log(string.Format("*** Starting Test: [{0}] ***", this._testName));
		}

		/// <summary>Begin TestCase
		/// <param name="Description">TestCase Description, used on logs</param>
		/// </summary>
		public void BeginCase(string Description)
		{
			//init the new TestCase with Unique TestCase Number and Description
			_testCase = new UniqueId(Description);

			if(this._logOnSuccess == true)
				Log(string.Format("Starting Case: [{0}]", _testCase.ToString()));
		}


		/// <summary>Compare two objects (using Object.Equals)
		/// </summary>
		protected bool Compare(object a, object b)
		{
			//signal that the Compare method has been called
			if (_testCase == null) {
				_testCase = new UniqueId(_testName);
			}
			this._testCase.CompareInvoked = true;
			//a string that holds the description of the objects for log
			string ObjectData;

			//check if one of the objects is null
			if  (a == null && b != null)
			{
				ObjectData = "Object a = null" + ", Object b.ToString() = '" + b.ToString() + "'(" + b.GetType().FullName + ")";
				this._testCase.Success = false; //objects are different, TestCase Failed
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//check if the other object is null
			if  (a != null && b == null)
			{
				ObjectData = "Object a.ToString() = '" + a.ToString() + "'(" + a.GetType().FullName + "), Object b = null";
				this._testCase.Success = false; //objects are different, TestCase Failed
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//check if both objects are null
			if ( (a == null && b == null) )
			{
				ObjectData = "Object a = null, Object b = null";
				this._testCase.Success = true; //both objects are null, TestCase Succeed
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			ObjectData = "Object a.ToString() = '" + a.ToString() + "'(" + a.GetType().FullName + "), Object b.ToString = '" + b.ToString() + "'(" + b.GetType().FullName + ")";
			//use Object.Equals to compare the objects
			this._testCase.Success = (a.Equals(b));
			LogCompareResult(ObjectData);
			return this._testCase.Success;
		}

		/// <summary>Compare two Object Arrays. 
		/// <param name="a">First array.</param>
		/// <param name="b">Second array.</param>
		/// <param name="Sorted">Used to indicate if both arrays are sorted.</param>
		/// </summary>
		protected bool Compare(Array a, Array b)
		{
			//signal that the Compare method has been called
			this._testCase.CompareInvoked=true;
			//a string that holds the description of the objects for log
			string ObjectData;

			//check if both objects are null
			if ( (a == null && b == null) )
			{
				ObjectData = "Array a = null, Array b = null";
				this._testCase.Success = true; //both objects are null, TestCase Succeed
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//Check if one of the objects is null.
			//(If both were null, we wouldn't have reached here).
			if (a == null || b == null)
			{
				string aData = (a==null) ? "null" : "'" + a.ToString() + "' (" + a.GetType().FullName + ")";
				string bData = (b==null) ? "null" : "'" +b.ToString() + "' (" + b.GetType().FullName + ")";
				ObjectData = "Array a = "  + aData + ", Array b = " + bData;
				this._testCase.Success = false; //objects are different, testCase Failed.
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//check if both arrays are of the same rank.
			if (a.Rank != b.Rank)
			{
				this._testCase.Success = false;
				ObjectData = string.Format("Array a.Rank = {0}, Array b.Rank = {1}", a.Rank, b.Rank);
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//Do not handle multi dimentional arrays.
			if (a.Rank != 1)
			{
				this._testCase.Success = false;
				ObjectData = "Multi-dimension array comparison is not supported";
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			//Check if both arrays are of the same length.
			if (a.Length != b.Length)
			{
				this._testCase.Success = false;
				ObjectData = string.Format("Array a.Length = {0}, Array b.Length = {1}", a.Length, b.Length);
				LogCompareResult(ObjectData);
				return this._testCase.Success;
			}

			ObjectData = "Array a.ToString() = '" + a.ToString() + "'(" + a.GetType().FullName + ") Array b.ToString = '" + b.ToString() + "'(" + b.GetType().FullName + ")";

			//Compare elements of the Array.
			int iLength = a.Length;
			for (int i=0; i<iLength; i++)
			{
				object aValue = a.GetValue(i);
				object bValue = b.GetValue(i);

				if (aValue == null && bValue == null)
				{
					continue;
				}

				if (aValue == null || bValue == null  ||  !aValue.Equals(bValue) )
				{
					string aData = (aValue==null) ? "null" : "'" + aValue.ToString() + "' (" + aValue.GetType().FullName + ")";
					string bData = (bValue==null) ? "null" : "'" + bValue.ToString() + "' (" + bValue.GetType().FullName + ")";
					ObjectData = string.Format("Array a[{0}] = {1}, Array b[{0}] = {2}", i, aData, bData);
					this._testCase.Success = false; //objects are different, testCase Failed.
					LogCompareResult(ObjectData);
					return this._testCase.Success;
				}
			}


			this._testCase.Success = true;
			LogCompareResult(ObjectData);
			return this._testCase.Success;
		}
	

		/// <summary>
		/// Intentionally fail a testcase, without calling the compare method.
		/// </summary>
		/// <param name="message">The reason for the failure.</param>
		protected void Fail(string message)
		{
			this._testCase.CompareInvoked = true;
			this._testCase.Success = false;
			string msg = string.Format("TestCase \"{0}\" Failed: [{1}]", _testCase.ToString(), message);
			if (_failAtTestEnd == null)
				Assert.Fail(msg);
			Log(msg);
		}

		/// <summary>
		/// Intentionally cause a testcase to pass, without calling the compare message.
		/// </summary>
		/// <param name="message">The reason for passing the test.</param>
		protected void Pass(string message)
		{
			this._testCase.CompareInvoked = true;
			this._testCase.Success = true;
			if (this._logOnSuccess)
			{
				Log(string.Format("TestCase \"{0}\" Passed: [{1}]", _testCase.ToString(), message));
			}
		}

		/// <summary>
		/// Marks this testcase as success, but logs the reason for skipping regardless of _logOnSuccess value.
		/// </summary>
		/// <param name="message">The reason for skipping the test.</param>
		protected void Skip(string message)
		{
			this._testCase.CompareInvoked = true;
			this._testCase.Success = true;
			Log(string.Format("TestCase \"{0}\" Skipped: [{1}]", _testCase.ToString(), message));
		}

		/// <summary>
		/// Intentionally fail a testcase when an expected exception is not thrown.
		/// </summary>
		/// <param name="exceptionName">The name of the expected exception type.</param>
		protected void ExpectedExceptionNotCaught(string exceptionName)
		{
			this.Fail(string.Format("Expected {0} was not caught.", exceptionName));
		}

		/// <summary>
		/// Intentionally cause a testcase to pass, when an expected exception is thrown.
		/// </summary>
		/// <param name="ex"></param>
		protected void ExpectedExceptionCaught(Exception ex)
		{
			this.Pass(string.Format("Expected {0} was caught.", ex.GetType().FullName));
		}

		/// <summary>End TestCase
		/// <param name="ex">Exception object if exception occured during the TestCase, null if not</param>
		/// </summary>
		protected void EndCase(Exception ex)
		{
			//check if BeginCase was called. cannot end an unopen TestCase
			if(_testCase == null)
			{
				throw new Exception("BeginCase was not called");
			}
			else
			{
				// if Exception occured during the test - log the error and faile the TestCase.
				if(ex != null)
				{
					_testCase.Success=false;
					if (_failAtTestEnd == null)
						throw ex;
					Log(string.Format("TestCase: \"{0}\" Error: [Failed With Unexpected {1}: \n\t{2}]", _testCase.ToString(), ex.GetType().FullName, ex.Message + "\n" + ex.StackTrace ));
				}
				else
				{                    
					//check if Compare was called
					if (_testCase.CompareInvoked == true)
					{
						if(this._logOnSuccess == true) Log(string.Format("Finished Case: [{0}] ", _testCase.ToString()));
					}
					else
					{
						//if compare was not called, log error message
						//Log(string.Format("TestCase \"{0}\" Warning: [TestCase didn't invoke the Compare mehtod] ", _testCase.ToString()));
					}
				}
				//Terminate TestCase (set TestCase to null)
				_testCase = null;
			}
		}


		/// <summary>End Test
		/// <param name="ex">Exception object if exception occured during the Test, null if not</param>
		/// </summary>
		public void EndTest(Exception ex)
		{
			if (ex != null) 
				throw ex;
			else if (UniqueId.FailureCounter != 0)
				Assert.Fail(String.Format("Test {0} failed in {1} scenarios.", this._testName, UniqueId.FailureCounter));

			if(this._logOnSuccess)
			{
				Log(string.Format("*** Finished Test: [{0}] ***", this._testName));
			}
		}
	

		public int GHTGetExitCode()
		{
			return UniqueId.FailureCounter;
		}

		/// <summary>logger 
		/// <param name="text">string message to log</param>
		/// </summary>
		protected void Log(string text)
		{
			_loggerBuffer = _loggerBuffer + "\n" + "GHTBase:Logger - " + text;
			_logger.WriteLine("GHTBase:Logger - " + text);
		}

		//used to log the results from the compare methods
		private void LogCompareResult(string ObjectData)
		{
			if(this._testCase.Success == false)
			{
				string msg = string.Format("TeseCase \"{0}\" Error: [Failed while comparing(" + ObjectData + ")] ", _testCase.ToString() );
				if (_failAtTestEnd == null)
					Assert.Fail(msg);
				Log("Test: " + _testName + " " + msg);
			}
			else if(this._logOnSuccess == true)
				Log(string.Format("TestCase \"{0}\" Passed ", _testCase.ToString()));
			
		}

		protected int TestCaseNumber
		{
			get
			{
				return _testCase.CaseNumber;
			}
		}

		#endregion

		#region private fields
		
		private TextWriter _logger;
		public string _loggerBuffer; // a public clone string of the _logger (used in web tests)

		private string _testName;
		private UniqueId _testCase;
		private bool _logOnSuccess;
		private string _failAtTestEnd = Environment.GetEnvironmentVariable("MONOTEST_FailAtTestEnd");
		#endregion
		
	}

	//holds all the info on a TestCase
	internal class UniqueId
	{
		//holds the unique name of the test case
		//this name must be recieved from the test case itself
		//when calling BeginCase.
		//example: BeginCase("MyName")
		private string _caseName;

		//maintains the number generated for this test case
		private static int _caseNumber;

		//maintains the number of failed test case 
		private static int _FailureCounter;
		internal static int FailureCounter
		{
			get
			{
				return _FailureCounter;
			}
		}
		
		//indicate if the Compare method has been invoked AND containes compare objects message (ToString)
		private bool _CompareInvoked;
		internal bool CompareInvoked
		{
			get
			{
				return _CompareInvoked;
			}
			set
			{
				_CompareInvoked = value;
			}
		}


		//reset the static counters when a new Test (not TestCase !!) begin
		internal static void ResetCounters()
		{
			_FailureCounter = 0;
			_caseNumber = 0;
		}

		//signal if a TestCase failed, if failed - increment the _FailureCounter
		private bool _success;
		internal bool Success
		{
			get
			{
				return this._success;
			}
			set
			{
				this._success = value;

				if (value == false) 
				{
					_FailureCounter++;
				}
			}
		}


		//Ctor, Recieve the name for the test case
		//generate a unique number and apply it to the test case
		internal UniqueId(string Name)
		{
			this._caseName = Name;
			//this._caseNumber = ++UniqueId._counter;
			_caseNumber++;
		}

		internal int CaseNumber
		{
			get
			{
				return _caseNumber;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} #{1}", this._caseName, _caseNumber);
		}
	}
}
