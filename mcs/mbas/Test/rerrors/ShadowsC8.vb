'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 24
REM ExpectedError: BC30149
REM ErrorMessage: 'C' must implement 'Sub F()' for interface 'A'.

'Check the working of the Inheritable interfaces
'Note that though Shadowing is done... error occurs because there is no implementation for A.F

Interface A
	Sub F()	
End Interface

Interface B
	Inherits A
     	  Shadows Sub F() 
	  Sub G()  	
End Interface

Class C
	Implements B
	Sub CF() Implements B.F
	End Sub
	Sub CG() Implements B.G
	End Sub
End Class

Module InheritanceQ
        Sub Main()
	  End Sub
End Module
