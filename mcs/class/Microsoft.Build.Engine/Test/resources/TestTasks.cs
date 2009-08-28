//
// SimpleTask.cs: Used for UsingTask tests.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text;

namespace Foo
{
    public class NamespacedOutputTestTask : Task
    {
        public override bool Execute()
        {
            return true;
        }

        [Output]
        public string Property
        {
            get { return "some_text"; }
        }
    }
}

public class OutputTestTask : Task {
	public override bool Execute ()
	{
		return true;
	}

	[Output]
	public string Property {
		get { return "some_text"; }
	}
}

public class TestTask_TaskItems : Task
{
	string output;
	public override bool Execute () {
		output = items == null ? "null" : "count: " + items.Length.ToString ();
		return true;
	}

	ITaskItem[] items;
	public ITaskItem[] Property {
		set { items = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}

public class TestTask_TaskItem : Task
{
	string output;
	public override bool Execute () {
		output = item == null ? "null" : "not null: " + item.ItemSpec;
		return true;
	}

	ITaskItem item;
	public ITaskItem Property {
		set { item = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}


public class RequiredTestTask_TaskItems : Task {
	string output;
	public override bool Execute ()
	{
		output = items == null ? "null" : "count: " + items.Length.ToString ();
		return true;
	}

	ITaskItem [] items;
	[Required]
	public ITaskItem[] Property {
		set { items = value; }
	}

	[Output]
	public string Output
	{
		get { return output; }
	}
}

public class RequiredTestTask_TaskItem : Task
{
	string output;
	public override bool Execute ()
	{
		output = item == null ? "null" : "not null: " + item.ItemSpec;
		return true;
	}

	ITaskItem item;
	[Required]
	public ITaskItem Property
	{
		set { item = value; }
	}

	[Output]
	public string Output
	{
		get { return output; }
	}
}

public class RequiredTestTask_String : Task
{
	string output;
	public override bool Execute ()
	{
		output = property == null ? "null" : property.Length.ToString ();
		return true;
	}

	string property;
	[Required]
	public string Property
	{
		set { property = value; }
	}

	[Output]
	public string Output
	{
		get { return output; }
	}
}

public class RequiredTestTask_Strings : Task
{
	string output;
	public override bool Execute () {
		output = property == null ? "null" : property.Length.ToString ();
		return true;
	}

	string []property;
	[Required]
	public string[] Property {
		set { property = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}

public class RequiredTestTask_OtherObjectArray : Task
{
	string output;
	public override bool Execute () {
		output = property == null ? "null" : property.Length.ToString ();
		return true;
	}

	OtherClass[] property;
	[Required]
	public OtherClass[] Property {
		set { property = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}

public class RequiredTestTask_OtherObject : Task
{
	string output;
	public override bool Execute () {
		output = property == null ? "null" : "not null";
		return true;
	}

	OtherClass property;
	[Required]
	public OtherClass Property {
		set { property = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}

public class RequiredTestTask_MyTaskItemArray : Task
{
	string output;
	public override bool Execute () {
		output = property == null ? "null" : property.Length.ToString ();
		return true;
	}

	MyTaskItem[] property;
	[Required]
	public MyTaskItem[] Property {
		set { property = value; }
	}

	[Output]
	public string Output {
		get { return output; }
	}
}

public class OtherClass
{
}

public class MyTaskItem : ITaskItem
{
	#region ITaskItem Members

	public System.Collections.IDictionary CloneCustomMetadata () {
		throw new NotImplementedException ();
	}

	public void CopyMetadataTo (ITaskItem destinationItem) {
		throw new NotImplementedException ();
	}

	public string GetMetadata (string metadataName) {
		throw new NotImplementedException ();
	}

	public string ItemSpec {
		get {
			throw new NotImplementedException ();
		}
		set {
			throw new NotImplementedException ();
		}
	}

	public int MetadataCount {
		get { throw new NotImplementedException (); }
	}

	public System.Collections.ICollection MetadataNames {
		get { throw new NotImplementedException (); }
	}

	public void RemoveMetadata (string metadataName) {
		throw new NotImplementedException ();
	}

	public void SetMetadata (string metadataName, string metadataValue) {
		throw new NotImplementedException ();
	}

	#endregion
}

public class RequiredTestTask_IntArray: Task
{
	string output;
	public override bool Execute ()
	{
		output = items == null ? "null" : "count: " + items.Length.ToString ();
		return true;
	}

	int[] items;
	[Required]
	public int[] Property
	{
		set { items = value; }
	}

	[Output]
	public string Output
	{
		get { return output; }
	}
}


public class TrueTestTask : Task {
	public override bool Execute ()
	{
		return true;
	}
}

public class FalseTestTask : Task {
	public override bool Execute ()
	{
		return false;
	}
}

public class StringTestTask : Task {
	string output;

	public override bool Execute ()
	{
		StringBuilder sb = new StringBuilder ();
		sb.AppendFormat ("property: {0} ## ", property == null ? "null" : property);
		sb.AppendFormat ("array: {0}", array == null ? "null" : array.Length.ToString ());

		output = sb.ToString ();
		return true;
	}

	string property;
	string[] array;

	[Output]
	public string Property {
		get { return property; }
		set { property = value; }
	}

	[Output]
	public string[] Array {
		get { return array; }
		set { array = value; }
	}

	[Output]
	public string OutputString {
		get { return output; }
	}
}

public class PublishTestTask : Task {
	public override bool Execute ()
	{
		return true;
	}

	[Output]
	public bool True {
		get { return true; }
	}

	[Output]
	public bool False {
		get { return false; }
	}

	[Output]
	public char Char {
		get { return 'A'; }
	}

	[Output]
	public int Int {
		get { return -100; }
	}

	[Output]
	public uint Uint {
		get { return 100; }
	}

	[Output]
	public float Float {
		get { return 0.5f; }
	}

	[Output]
	public double Double {
		get { return 0.5; }
	}

	[Output]
	public DateTime DateTimeNow {
		get { return DateTime.Now; }
	}
}

public class BatchingTestTask : Task
{
	ITaskItem [] sources;
	string [] output;
	string[] strings;
	string single_string;

	public override bool Execute ()
	{
		return true;
	}

	public ITaskItem [] Sources
	{
		set {
			sources = value;
			output = new string [sources.Length];
			for (int i = 0; i < sources.Length; i++)
				output [i] = sources [i].ItemSpec; 
		}
	}

	public string [] Strings
	{
		set { strings = value; }
	}

	public ITaskItem SingleTaskItem
	{
		set { single_string = value.ItemSpec; }
	}

	public string SingleString
	{
		set { single_string = value; }
	}

	[Output]
	public string SingleStringOutput
	{
		get { return single_string; }
	}

	[Output]
	public string [] Output
	{
		get { return output; }
	}

	[Output]
	public string [] StringsOutput
	{
		get { return strings; }
	}

	[Output]
	public ITaskItem [] TaskItemsOutput
	{
		get { return sources; }
		set { sources = value; }
	}
}

namespace Another
{
	public class SameTask : Task
	{
		string output;
		public override bool Execute ()
		{
			output = "Another.SameTask";
			return true;
		}

		[Output]
		public string OutputString
		{
			get { return output; }
		}
	}
}

namespace Other
{
	public class SameTask : Task
	{
		string output;
		public override bool Execute ()
		{
			output = "Other.SameTask";
			return true;
		}

		[Output]
		public string OutputString
		{
			get { return output; }
		}
	}
}

