'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Reference:
'APR-1.4.0: If procedure is define by passing argument by reference and while calling the
'		 procedure it is passes by giving parentheses around the variable then it protect
'		 it from change 
'=============================================================================================

Imports System
Module APR_1_4_0
	Sub F(ByRef p As Integer)
      p += 1
   End Sub 
   
   Sub Main()
      Dim a As Integer = 1
      F((a))
		if(a<>1)
		Throw new System.Exception ("#A1, Unexpected behavior in Arguments_ByReferenceB.vb")
		end if
   End Sub 
End Module

'==============================================================================================