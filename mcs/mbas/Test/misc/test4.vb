Delegate Sub SubDelegate()

Module Test4

Sub MySub()
	System.Console.WriteLine ("In MySub")
End Sub

Sub Main()
	Dim dsub as SubDelegate
	
	dsub = New SubDelegate (AddressOf MySub)
	dsub()
End Sub
	
End Module
