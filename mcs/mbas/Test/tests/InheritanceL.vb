'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Checking for NotInheritable class....
'This insists that NonInheritable belongs only to the specific class.. not branches to its sub-classes

NotInheritable Class A
	Class B
	     	  Sub G()  	
		  End Sub      
	End Class
End Class

Class C
	Inherits A.B
End Class

Module InheritanceN
        Sub Main()
		Dim a as C=New C()
		a.G()
	  End Sub
End Module

