'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Param Array:
'APR-1.1.1:If ParamArray modifier is precied by ByRef modifier the it produces compiler error 
'=============================================================================================
Imports System
Module PA_1_1_1
   Function F(ParamArray ByRef args() As Integer) As Integer()
      Dim i As Integer
      For Each i In args
         Console.Write(" " & i)
      Next i
      Console.WriteLine()
   End Function
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }

      F(a)
      F(10, 20, 30, 40)
      F()
   End Sub
End Module
'================================================================================