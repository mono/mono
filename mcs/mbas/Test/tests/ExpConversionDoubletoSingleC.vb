'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoSingleC
	Sub Main()
			Dim a as Single
			Dim b as Double = -4.94065645841247e300
			a = CSng(b)
			if a<>Single.NegativeInfinity Then
				Throw New System.Exception("Double to Single Conversion is not working properly. Expected -Infinity but got " &a)		
		End if		
	End Sub
End Module
