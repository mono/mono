'==========================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Reference:
'APR-1.0.0: Argument Passing by Reference, which means the procedure can modify the variable
' 		itself.
'===========================================================================================

Imports System
Module APR_1_0_0
	Function F(ByRef p As Integer) as Integer
      p += 1
	return p
	End Function
   
   Sub Main()
      Dim a As Integer = 1
	Dim b as Integer = 0
      b=F(a)
	if (b<>a)
		Throw New System.Exception("#A1, Unexcepted Behaviour in Arguments_ByReferenceA.vb")
	end if
   End Sub 
End Module

'============================================================================================