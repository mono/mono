Imports System
Module APR_1_0_0
	Class C1
		Sub F(ByRef p As Integer)
			p += 1
		End Sub 
	End Class
	
	Sub Main()
		Dim obj As Object = new C1()
		Dim a As Integer = 1
		obj.F(a)
		if (a=1)
			Throw New System.Exception("#A1, Unexcepted Behaviour in Arguments_ByReferenceA.vb")
		end if
 	End Sub 
End Module

'============================================================================================
