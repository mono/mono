'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 11
REM ExpectedError: BC30397
REM ErrorMessage: : 'Shared' is not valid on an Interface declaration.

Module InterfaceC
	Shared Interface A
	End Interface
	Sub Main()		
	End Sub
End Module
 
