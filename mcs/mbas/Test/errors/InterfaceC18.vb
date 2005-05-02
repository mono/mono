'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30363
REM ErrorMessage: 'Sub New' cannot be declared in an interface.

Module InterfaceC
	Interface A
		Sub New() 
	End Interface	
	
	Sub Main()
	End Sub
End Module
 
