// Compiler options: -doc:xml-030.xml -warn:4 -warnaserror
using System;

class Test
{
	static void Main () {}

	/// <summary>
	/// some summary
	/// </summary>
	/// <value>
	/// <see cref="T:Test[]"/>
	/// <see cref="T:System.Text.RegularExpressions.Regex"/>
	/// <see cref="System.Text.RegularExpressions.Regex"/>
	/// <see cref="System.Text.RegularExpressions"/>
	/// <see cref="T:System.Text.RegularExpressions.Regex[]"/>
	/// </value>
	//
	// <see cref="T:System.Text.RegularExpressions"/> .. csc incorrectly allows it
	// <see cref="System.Text.RegularExpressions.Regex[]"/> ... csc does not allow it.
	//
	public void foo2() {
	}

	/// <summary>
	/// <see cref="String.Format(string, object[])" />.
	/// <see cref="string.Format(string, object[])" />.
	/// <see cref="String.Format(string, object [ ])" />.
	/// <see cref="string.Format(string, object [ ])" />.
	/// </summary>
	/// <param name="line">The formatting string.</param>
	/// <param name="args">The object array to write into format string.</param>
	public void foo3(string line, params object[] args) {
	}
}
