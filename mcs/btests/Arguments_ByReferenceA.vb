'==========================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Reference:
'APR-1.0.0: Argument Passing by Reference, which means the procedure can modify the variable
' 		itself.
'===========================================================================================

Imports System
Module APR_1_0_0
	Sub F(ByRef p As Integer)
      p += 1
	End Sub 
   
   Sub Main()
      Dim a As Integer = 1
      F(a)
	if (a=1)
		Throw New System.Exception("#A1, Unexcepted Behaviour in Arguments_ByReferenceA.vb")
	end if
   End Sub 
End Module

'============================================================================================