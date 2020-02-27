Public Structure VEC3
    Public X, Y, Z As Double

    Public ReadOnly Property Length() As Double
        Get
            Return Math.Sqrt(X * X + Y * Y + Z * Z)
        End Get
    End Property

    Public ReadOnly Property LengthSquared() As Double
        Get
            Return (X * X + Y * Y + Z * Z)
        End Get
    End Property

    Public Shared ReadOnly Property XA() As VEC3
        Get
            Return New VEC3(1.0, 0.0, 0.0)
        End Get
    End Property

    Public Shared ReadOnly Property YA() As VEC3
        Get
            Return New VEC3(0.0, 1.0, 0.0)
        End Get
    End Property

    Public Shared ReadOnly Property ZA() As VEC3
        Get
            Return New VEC3(0.0, 0.0, 1.0)
        End Get
    End Property

    Public Shadows ReadOnly Property ToString() As String
        Get
            Return CStr(X) & "," & CStr(Y) & "," & CStr(Z)
        End Get
    End Property

    Public Shadows ReadOnly Property ToString(ByVal accuracy As Integer) As String
        Get
            Return "{" &
                   CStr(Math.Round(X, accuracy)) & "," &
                   CStr(Math.Round(Y, accuracy)) & "," &
                   CStr(Math.Round(Z, accuracy) & "}")
        End Get
    End Property

    Sub New(ByVal _x As Double, ByVal _y As Double, ByVal _z As Double)
        X = _x
        Y = _y
        Z = _z
    End Sub

    Shared Function DotProduct(ByVal v1 As VEC3, ByVal v2 As VEC3) As Double
        Return (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z)
    End Function

    Shared Function CrossProduct(ByVal v1 As VEC3, ByVal v2 As VEC3) As VEC3
        Return New VEC3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X)
    End Function

    Shared Function AngleBetween(ByVal v1 As VEC3, ByVal v2 As VEC3) As Double
        Return Math.Acos(DotProduct(v1, v2) / (v1.Length * v2.Length))
    End Function

    Shared Operator +(ByVal v1 As VEC3, ByVal v2 As VEC3) As VEC3
        Return New VEC3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z)
    End Operator

    Shared Operator -(ByVal v1 As VEC3, ByVal v2 As VEC3) As VEC3
        Return New VEC3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z)
    End Operator

    Shared Operator *(ByVal v As VEC3, ByVal c As Double) As VEC3
        Return New VEC3(v.X * c, v.Y * c, v.Z * c)
    End Operator

    Shared Operator *(ByVal c As Double, ByVal v As VEC3) As VEC3
        Return New VEC3(v.X * c, v.Y * c, v.Z * c)
    End Operator

    Shared Operator /(ByVal v As VEC3, ByVal c As Double) As VEC3
        c = 1.0R / c
        Return New VEC3(v.X * c, v.Y * c, v.Z * c)
    End Operator

    Shared Operator =(ByVal v1 As VEC3, ByVal v2 As VEC3) As Boolean
        Return ((v1.X = v2.X) AndAlso (v1.Y = v2.Y) AndAlso (v1.Z = v2.Z))
    End Operator

    Shared Operator <>(ByVal v1 As VEC3, ByVal v2 As VEC3) As Boolean
        Return ((v1.X <> v2.X) OrElse (v1.Y <> v2.Y) OrElse (v1.Z <> v2.Z))
    End Operator

    Shared Function UnitVector(ByVal v As VEC3, Optional ByVal len As Integer = 1) As VEC3
        Dim a As Double = v.Length
        If a > 0 Then v = v / a * len
        Return v
    End Function

    Public Shared Function Parse(ByVal str As String) As VEC3
        Dim v As VEC3, dv As String()
        If str.Contains(",") Then
            dv = str.Split(","c)
        ElseIf str.Contains(" ") Then
            dv = str.Split(" "c)
        End If
        If dv.Count = 3 Then
            v = New VEC3(CDbl(dv(0)), CDbl(dv(1)), CDbl(dv(2)))
        End If
        Return v
    End Function

    Shared Function LERP(ByVal v1 As VEC3, ByVal v2 As VEC3, ByVal t As Double) As VEC3
        Return (v1 + (v2 - v1) * t)
    End Function

End Structure