'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Check the working of the Non-Inheritable Class.
'Does Not apply for the Sub classes

Class A
	NotInheritable Class B
      	  Sub G()  	
		  End Sub      
	End Class
	Function F()
	End Function
End Class

Class C
	Inherits A
	Public Dim c as A.B=New A.B()
End Class

Module InheritanceN
        Sub Main()
		Dim a as C=New C()
		Dim b as A.B=New A.B()
		a.c.G()
		b.G()
	  End Sub
End Module

