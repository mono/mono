'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 22
REM ExpectedError: BC30471
REM ErrorMessage: Expression is not an array or a method, and cannot have an argument list.

'Checking if the type of an unqualified name holds good

Imports A

Namespace A
	Public Module B
		Public C as Integer=10
	End Module
End Namespace

Module NamespaceA	
	Sub Main()
		If B.C()<>10 Then
			Throw New System.Exception("Not Working")
		End If
	End Sub
End Module

