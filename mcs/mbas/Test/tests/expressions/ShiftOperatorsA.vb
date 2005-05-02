
Imports System

Module ShiftOperatorsA

    Sub Main()
        Dim a1 As Double = 200.93
        a1 = a1 >> 109.95
     	  if a1<>0 
        	throw new System.Exception("#A1 Shift operator not working")
 	  end if
    End Sub

End Module