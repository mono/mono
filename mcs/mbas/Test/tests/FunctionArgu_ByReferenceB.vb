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
	Function F(ByRef p As Integer) as Integer
      p += 1
	return p
   End Function 
   
   Sub Main()
      Dim a As Integer = 1
	Dim b As Integer = 0
      b=F((a))
		if(b=a)
		Throw new System.Exception ("#A1, Unexpected behavior in Arguments_ByReferenceB.vb")
		end if
   End Sub 
End Module
'=============================================================================================