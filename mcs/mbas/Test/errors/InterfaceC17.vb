'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC31042
REM ErrorMessage: 'Sub New' cannot implement interface members.

Module InterfaceC
	Interface A
		Sub C() 
	End Interface	
	
	Class C
		Implements A
		Public Sub New() Implements A.C
		End Sub
		Public Sub C() Implements A.C
		End Sub
	End Class			
	
	Sub Main()
		Dim i as integer
	End Sub
End Module
 
