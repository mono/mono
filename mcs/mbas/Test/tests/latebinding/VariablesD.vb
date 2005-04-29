Imports System

Class AA
	Inherits System.MarshalByRefObject
	Public Function fun()
	End Function
End Class


Class AAA
	Public Function fun(a As AA)		
	End Function
End Class

Module Test
    Public Sub Main()
		dim b as Object = new AA()
		dim a as Object  = new AAA()
		a.fun(b)
    End Sub
End Module

