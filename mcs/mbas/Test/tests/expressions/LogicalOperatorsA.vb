
Imports System

Module LogicalOperatorsA

    Sub Main()
        Dim a1, a2 As Integer
        a1 = f1() AndAlso f2()
        a2 = a1 OrElse f1()
	  if a1<>0
		throw new System.Exception("#A1 Logical Operator not working")
	  End if
	  if a2<>-1 
		throw new System.Exception("#A2 Logical Operator not working")
	  End if
    End Sub

    Function f1() As Integer
        Return 1
    End Function

    Function f2() As Boolean
        Return False
    End Function

End Module