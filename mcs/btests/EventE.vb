Imports System

Public Class C
   Public Event E() 

   Sub S()
      RaiseEvent E() 
   End Sub
End Class


Module M
Sub S1()
   Dim x As New C()
   AddHandler x.E, AddressOf EH
   x.S()   
End Sub

Sub EH()
   Console.WriteLine("Event fired")
End Sub

Sub Main()
	S1()
End Sub

End Module

