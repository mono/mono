'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 23
REM ExpectedError: BC31035
REM ErrorMessage: Interface 'InterfaceA.A' is not implemented by  this class.

Module InterfaceA
	Interface A
		Sub fun()		
	End Interface
	
	Class AB
		Implements A
		Sub fun() Implements A.fun
		End sub		
	End Class

	Class AB1
		Inherits AB
		Sub fun1() Implements A.fun
		End sub		
	End Class

	Sub Main()		
	End Sub
End Module
 
