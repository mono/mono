Module ExpConversionofBoolToDouble
        Sub Main()
                Dim a as Boolean = True
                Dim b as Double = Cdbl(a)
                if b <> -1 then
                        Throw New System.Exception("Explicit Conversion of Bool(True) to Double has Failed. Expected -1, but got " & b)
                End if
        End Sub
End Module
