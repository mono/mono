'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 18
REM ExpectedError: BC30179
REM ErrorMessage: class 'C' and class 'C' conflict in namespace 'B'.

Namespace A
	Namespace B
		Class C
		End Class
	End Namespace
End Namespace

Namespace A.B
	Class C
	End Class
End Namespace

Module NamespaceA	
	Sub Main()
	End Sub
End Module

