'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 24
REM ExpectedError: BC30456
REM ErrorMessage: 'C' is not a member of 'A'.

'To check if Importing is done properly... without ambiguity...

Imports A

Namespace A
	Namespace A 
		Public Module C
			Public D as Integer=10
		End Module
	End Namespace
End Namespace

Module NamespaceA	
	Sub Main()
		if A.C.D<>10 Then
			Throw New System.Exception("Namespace Not working")
		End If
	End Sub
End Module

