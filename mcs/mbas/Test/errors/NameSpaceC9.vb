'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 12
REM ExpectedError: BC30183
REM ErrorMessage: Keyword is not valid as an identifier.

'VB.Net Accepts recognizes even keywords as qualifiers as long as they follow a period, but Alias cannot be a keyword 

Imports Default = A.B

Namespace A
	Namespace B
		Public Module C
			Public D as Integer=10
		End Module
	End Namespace
End Namespace

Module NamespaceA	
	Sub Main()
	End Sub
End Module

