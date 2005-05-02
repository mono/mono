'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'System.NullReferenceException

'This is done mainly to check if a variable is automatically assigned as Object if its type is not specified.

Imports System
Imports Nunit.Framework

<TestFixture>_
Public Class Variables123
	_<Test, ExpectedException (GetType (System.NullReferenceException))>
        Public Sub TestForException()	
			Dim a
			Console.WriteLine(a.GetTypeCode())
        End Sub
End Class 