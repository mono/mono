'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoSingleA
	Sub Main()
			Dim a as Single
			Dim b as Double = -4.94065645841247e-324
			a = CSng(b)
			if a<>-0 Then
				Throw New System.Exception("Double to Single Conversion is not working properly.")		
		End if		
	End Sub
End Module
