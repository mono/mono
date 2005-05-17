'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class Base
End Class

Class Derived
    Inherits Base
End Class

Class Derived1
    Inherits Derived
End Class

Module Test
    Public i as integer	
    Sub F(ByVal b As Base)
        i = 10
    End Sub

    Sub F(ByVal d As Object)
        i = 20
    End Sub

    Sub Main()
        Dim b As Base = New Derived1()
        Dim o As Object = b

        F(b)
	  if i<>10
		throw new System.Exception("#A1 Latebinding Not working")
	  end if
        F(o)
	  if i<>20
		throw new System.Exception("#A2 Latebinding Not working")
	  end if
    End Sub
End Module
