Public Structure Vertex_Coloured
    Public Position As VEC3
    Public Colour As Color
    Public Normal As VEC3

    Public Sub New(ByVal _position As VEC3,
                   ByVal _colour As Color,
                   ByVal _normal As VEC3)
        Position = _position
        Colour = _colour
        Normal = _normal
    End Sub

    Public Sub New(ByVal X As Double, ByVal Y As Double, ByVal Z As Double,
                   ByVal R As Byte, ByVal G As Byte, ByVal B As Byte)
        Position = New VEC3(X, Y, Z)
        Colour = Color.FromArgb(R, G, B)
    End Sub

    Public Sub SetColour(ByVal C As Color)
        Colour = C
    End Sub

    Public Property X() As Double
        Get
            Return Position.X
        End Get
        Set(ByVal value As Double)
            Position.X = value
        End Set
    End Property

    Public Property Y() As Double
        Get
            Return Position.Y
        End Get
        Set(ByVal value As Double)
            Position.Y = value
        End Set
    End Property

    Public Property Z() As Double
        Get
            Return Position.Z
        End Get
        Set(ByVal value As Double)
            Position.Z = value
        End Set
    End Property

    Public ReadOnly Property R() As Double
        Get
            Return Colour.R
        End Get
    End Property

    Public ReadOnly Property G() As Double
        Get
            Return Colour.G
        End Get
    End Property

    Public ReadOnly Property B() As Double
        Get
            Return Colour.B
        End Get
    End Property

    Shared Function LERP(ByVal v1 As Vertex_Coloured,
                         ByVal v2 As Vertex_Coloured,
                         ByVal t As Double) As Vertex_Coloured
        Return New Vertex_Coloured((v1.Position + (v2.Position - v1.Position) * t), Add_Cr(v1.Colour, Mult_Cr((Sub_Cr(v2.Colour, v1.Colour)), t)), v1.Normal)
    End Function

    Shared Function Add_Cr(ByVal C1 As Color,
                           ByVal C2 As VEC3) As Color
        Return Color.FromArgb(C1.R + C2.X, C1.G + C2.Y, C1.B + C2.Z)
    End Function

    Shared Function Sub_Cr(ByVal C1 As Color,
                           ByVal C2 As Color) As VEC3
        Return New VEC3(CDbl(C1.R) - CDbl(C2.R), CDbl(C1.G) - CDbl(C2.G), CDbl(C1.B) - CDbl(C2.B))
    End Function

    Shared Function Mult_Cr(ByVal C1 As VEC3,
                            ByVal C2 As VEC3) As VEC3
        Return New VEC3(C1.X * C2.X, C1.Y * C2.Y, C1.Z * C2.Z)
    End Function

    Shared Function Mult_Cr(ByVal C1 As VEC3,
                            ByVal t As Double) As VEC3
        Return New VEC3(C1.X * t, C1.Y * t, C1.Z * t)
    End Function

End Structure

Public Structure Vertex_Textured
    Public Position As VEC3
    Public TextureC As PointF
    Public Normal As VEC3
    Public Light_Level As Double

    Public Sub Set_Light_Level(ByVal value As Double)
        Light_Level = Math.Max(0, Math.Min(1, value))
    End Sub

    Public Sub New(ByVal _position As VEC3, ByVal _textureC As PointF, ByVal _normal As VEC3, ByVal _Light_Level As Double)
        Position = _position
        TextureC = _textureC
        Normal = _normal
        Light_Level = _Light_Level
    End Sub

    Public Sub New(ByVal _position As VEC3, ByVal _textureC As PointF, ByVal _normal As VEC3)
        Position = _position
        TextureC = _textureC
        Normal = _normal
    End Sub

    Public Sub New(ByVal X As Double, ByVal Y As Double, ByVal Z As Double,
                   ByVal U As Double, ByVal V As Double)
        Position = New VEC3(X, Y, Z)
        TextureC = New PointF(U, V)
    End Sub

    Public Property X() As Double
        Get
            Return Position.X
        End Get
        Set(ByVal value As Double)
            Position.X = value
        End Set
    End Property

    Public Property Y() As Double
        Get
            Return Position.Y
        End Get
        Set(ByVal value As Double)
            Position.Y = value
        End Set
    End Property

    Public Property Z() As Double
        Get
            Return Position.Z
        End Get
        Set(ByVal value As Double)
            Position.Z = value
        End Set
    End Property

    Public ReadOnly Property U() As Single
        Get
            Return TextureC.X
        End Get
    End Property

    Public ReadOnly Property V() As Single
        Get
            Return TextureC.Y
        End Get
    End Property

    Shared Function LERP(ByVal v1 As Vertex_Textured,
                         ByVal v2 As Vertex_Textured,
                         ByVal t As Double) As Vertex_Textured
        Return New Vertex_Textured((v1.Position + (v2.Position - v1.Position) * t),
                                   Add_PF(v1.TextureC, Mult_PF((Sub_PF(v2.TextureC, v1.TextureC)), t)),
                                   v1.Normal,
                                   v1.Light_Level + (v2.Light_Level - v1.Light_Level) * t)
    End Function

    Shared Function Add_PF(ByVal PF1 As PointF,
                               ByVal PF2 As PointF) As PointF
        Return New PointF(PF1.X + PF2.X, PF1.Y + PF2.Y)
    End Function

    Shared Function Sub_PF(ByVal PF1 As PointF,
                               ByVal PF2 As PointF) As PointF
        Return New PointF(PF1.X - PF2.X, PF1.Y - PF2.Y)
    End Function

    Shared Function Mult_PF(ByVal PF1 As PointF,
                            ByVal t As Double) As PointF
        Return New PointF(PF1.X * t, PF1.Y * t)
    End Function

End Structure

Public Structure Triangle_Coloured
    Public V1 As Vertex_Coloured
    Public V2 As Vertex_Coloured
    Public V3 As Vertex_Coloured
    Public Normal As VEC3

    Public Sub New(ByVal _V1 As Vertex_Coloured,
                   ByVal _V2 As Vertex_Coloured,
                   ByVal _V3 As Vertex_Coloured)
        V1 = _V1
        V2 = _V2
        V3 = _V3
    End Sub

End Structure

Public Structure Triangle_Textured
    Public V1 As Vertex_Textured
    Public V2 As Vertex_Textured
    Public V3 As Vertex_Textured
    Public TX As Texture
    Public Normal As VEC3

    Public Sub New(ByVal _V1 As Vertex_Textured,
                   ByVal _V2 As Vertex_Textured,
                   ByVal _V3 As Vertex_Textured)
        V1 = _V1
        V2 = _V2
        V3 = _V3
    End Sub

End Structure

Public Structure Triangle
    Public V1 As Integer
    Public V2 As Integer
    Public V3 As Integer
    Public Type As Model.Triangle_Type

    Public Sub New(ByVal _V1 As Integer,
                   ByVal _V2 As Integer,
                   ByVal _V3 As Integer,
                   ByVal TType As Model.Triangle_Type)
        V1 = _V1
        V2 = _V2
        V3 = _V3
        Type = TType
    End Sub

End Structure