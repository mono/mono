Imports System

Module ArrayF

	Sub Main()
		Dim arr As Integer(,) = {{1, 2, 3}, {3, 4, 7}}
		ReDim arr(1, 1)
		Dim obj As Object = arr
		If obj(0, 0) = 1 Then
			Throw New Exception("#AF1 - ReDim Statement failed")
		End If
		
		obj(0, 0) = 1
		obj(0, 1) = 2
		If obj(0, 0) <> 1 Then
			Throw New Exception("#AF2 - ReDim Statement failed")
		End If
		
		Try
			Erase arr
			obj = arr
			Console.WriteLine(obj(0, 0))
		Catch e As Exception
			If e.GetType.ToString <> "System.NullReferenceException" Then
				Throw New Exception("#AF3 - Erase Statement failed")
			End If
		End Try
	
	End Sub

End Module
