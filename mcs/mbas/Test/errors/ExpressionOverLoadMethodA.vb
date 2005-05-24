'Author: Satya Sudha K <ksathyasudha@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
REM LineNo: 18
REM ExpectedError: BC30519 
REM ErrorMessage: Overload resolution failed because no accessible 'F' can be called without a narrowing conversion

Imports System
Module M
	Function F (a As Long, b As Integer) As Integer
		return 1
	End Function
	Function F (a As String, b As Short) As Integer
		return 2
	End Function
	Sub Main ()
		Dim obj As Object = "ABC"
		Dim b As Long = 345
		F (obj, b)
	End Sub
End Module
