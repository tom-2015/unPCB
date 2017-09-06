Module Functions

    Public Function Distance(ByVal Point1 As Point, ByVal Point2 As Point) As Integer
        Return Math.Sqrt((Point1.X - Point2.X) ^ 2 + (Point1.Y - Point2.Y) ^ 2)
    End Function

    Public Function Distance(ByVal Point1 As PointF, ByVal Point2 As Point) As Single
        Return Math.Sqrt((Point1.X - Point2.X) ^ 2 + (Point1.Y - Point2.Y) ^ 2)
    End Function

    Public Function Distance(ByVal Point1 As Point, ByVal Point2 As PointF) As Single
        Return Math.Sqrt((Point1.X - Point2.X) ^ 2 + (Point1.Y - Point2.Y) ^ 2)
    End Function

    Public Function Distance(ByVal Point1 As PointF, ByVal Point2 As PointF) As Single
        Return Math.Sqrt((Point1.X - Point2.X) ^ 2 + (Point1.Y - Point2.Y) ^ 2)
    End Function

    Public Function MidPoint(ByVal Point1 As PointF, ByVal Point2 As PointF) As PointF
        Return New PointF((Point1.X + Point2.X) / 2, (Point1.Y + Point2.Y) / 2)
    End Function

    ''' <summary>
    ''' Parses a string to single 
    ''' </summary>
    ''' <param name="Val"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function StringToSingle(ByVal Val As String) As Single
        Return Single.Parse(Val, System.Globalization.NumberFormatInfo.InvariantInfo)
    End Function

    ''' <summary>
    ''' Converts a single to string
    ''' </summary>
    ''' <param name="Val"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SingleToString(ByVal Val As Single) As String
        Return Val.ToString("G", System.Globalization.CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Casts a Selectablelayerobject to pad
    ''' </summary>
    ''' <param name="Obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CastPad(ByVal Obj As SelectableLayerObject) As Pad
        Return CType(Obj, Pad)
    End Function

    ''' <summary>
    ''' Returns if form type is already open
    ''' </summary>
    ''' <param name="Form"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsFormOpen(ByVal Form As Type) As Boolean
        For Each Frm As Form In Application.OpenForms
            If Frm.GetType.Name = Form.Name Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function GetForm(ByVal Form As Type) As Form
        For Each Frm As Form In Application.OpenForms
            If Frm.GetType.Name = Form.Name Then
                Return Frm
            End If
        Next
        Return Nothing
    End Function

    Public Class ListBoxItem(Of T)
        Dim m_Text As String
        Dim m_Object As T

        Public Sub New(ByVal Text As String, ByVal Value As T)
            m_Object = Value
            m_Text = Text
        End Sub

        Public Property Value() As T
            Get
                Return m_Object
            End Get
            Set(ByVal value As T)
                m_Object = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return m_Text
        End Function

    End Class

    ''' <summary>
    ''' Returns distance to a piece of line defined by point1 and Point2 to Topoint
    ''' </summary>
    ''' <param name="Point1"></param>
    ''' <param name="Point2"></param>
    ''' <param name="ToPoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DistanceToLineSegment(ByVal Point1 As PointF, ByVal Point2 As PointF, ByVal ToPoint As PointF) As Single

        If Point1.X = Point2.X And Point1.Y = Point2.Y Then 'this is no line, start = end
            Return Distance(Point1, ToPoint)
        End If

        If Point1.X = Point2.X Then
            Dim TopPoint As PointF = New Point(Point1.X, Math.Max(Point1.Y, Point2.Y))
            Dim BottomPoint As PointF = New Point(Point1.X, Math.Min(Point1.Y, Point2.Y))
            If ToPoint.Y <= BottomPoint.Y Then
                Return Distance(ToPoint, BottomPoint)
            ElseIf ToPoint.Y > TopPoint.Y Then
                Return Distance(ToPoint, TopPoint)
            Else
                Return Math.Abs(ToPoint.X - Point1.X)
            End If
        ElseIf Point1.Y = Point2.Y Then 'Y co same
            Dim leftPoint As PointF = New Point(Math.Max(Point1.X, Point2.X), Point1.Y)
            Dim rightPoint As PointF = New Point(Math.Max(Point1.X, Point2.X), Point1.Y)
            If ToPoint.X <= leftPoint.X Then
                Return Distance(ToPoint, leftPoint)
            ElseIf ToPoint.X >= rightPoint.X Then
                Return Distance(ToPoint, rightPoint)
            Else
                Return Math.Abs(ToPoint.Y - Point1.Y)
            End If
        Else
            Dim A As Double = (Point2.Y - Point1.Y)
            Dim B As Double = (Point2.X - Point1.X)
            Dim Rico As Double = A / B 'A/B
            Dim x As Double = (ToPoint.Y - Point1.Y + Rico * Point1.X + ToPoint.X / Rico) / (Rico + 1 / Rico) 'this is the x co of the intersect of the route with the perpendicular from the topoint to the route
            If Math.Min(Point1.X, Point2.X) < x AndAlso x < Math.Max(Point1.X, Point2.X) Then 'check if the x is between the 2 x co of the points
                Return Math.Abs(A * ToPoint.X - B * ToPoint.Y + Point2.X * Point1.Y - Point2.Y * Point1.X) / Math.Sqrt(A ^ 2 + B ^ 2) 'distance to line
            Else
                Return Math.Min(Distance(Point1, ToPoint), Distance(Point2, ToPoint)) 'no, return distance to the closest point
            End If
        End If
    End Function


End Module
