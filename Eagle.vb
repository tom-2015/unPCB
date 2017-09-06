Imports System.Xml
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Math

Namespace Eagle

    Public Enum GridUnit
        GU_mic
        GU_mm
        GU_mil
        GU_inch
    End Enum

    Public Enum GridStyle
        GS_lines
        GS_dots
    End Enum

    Public Enum WireStyle
        WS_continuous
        WS_longdash
        WS_shortdash
        WS_dashdot
    End Enum

    Public Enum WireCap
        WC_flat
        WC_round
    End Enum

    Public Enum PadShape
        PS_square
        PS_round
        PS_octagon
        PS_long
        PS_offset
    End Enum

    Public Enum ViaShape
        VS_square
        VS_round
        VS_octagon
    End Enum

    Public Enum TextFont
        TF_vector
        TF_proportional
        TF_fixed
    End Enum

    Public Enum AttributeDisplay
        AD_off
        AD_value
        AD_name
        AD_both
    End Enum

    Public Enum PolygonPour
        PP_solid
        PP_hatch
        PP_cutout
    End Enum

    Public Enum PinVisible
        PV_off
        PV_pad
        PV_pin
        PV_both
    End Enum

    Public Enum PinDirection
        PD_nc
        PD_in
        PD_out
        PD_io
        PD_oc
        PD_pwr
        PD_pas
        PD_hiz
        PD_sup
    End Enum

    Public Enum PinFunction
        PF_none
        PF_dot
        PF_clk
        PF_dotclk
    End Enum

    Public Enum PinLength
        PL_point
        PL_short
        PL_middle
        PL_long
    End Enum

    Public Enum GateAddLevel
        GAL_must
        GAL_can
        GAL_addnext
        GAL_request
        GAL_always
    End Enum

    Public Enum ContactRoute
        CR_all
        CR_any
    End Enum

    Public Enum DimensionType
        DT_parallel
        DT_horizontal
        DT_vertical
        DT_radius
        DT_diameter
        DT_leader
    End Enum

    Public Enum Severity
        S_info
        S_warning
        S_error
    End Enum

    Public Enum Align
        A_bottom_left
        A_bottom_center
        A_bottom_right
        A_center_left
        A_center
        A_center_right
        A_top_left
        A_top_center
        A_top_right
    End Enum

    Public Enum VerticalText
        VT_up
        VT_down
    End Enum

    Public Enum DrawingType
        schematic
        board
        library
    End Enum

    Public Enum PadTypes
        PadTypeThroughHole
        PadTypeSMD
    End Enum

    Public Enum GraphicOwnerType
        GO_Package
        GO_Symbol
        GO_Plain
        GO_Library
        GO_Sheet
        GO_Segment
    End Enum

    Public Class Rotation
        Implements ICloneable

        Dim m_Rotation As Single
        Dim m_Value As String

        Public Sub New()
            Rotation = 0
        End Sub

        Public Sub New(ByVal Value As String)
            m_Value = Value
            m_Rotation = toSingle(Value.Replace("M", "").Replace("S", "").Replace("R", ""))
        End Sub

        Public Overrides Function ToString() As String
            Return m_Value
        End Function

        Public Property Rotation() As Single
            Get
                Return m_Rotation
            End Get
            Set(ByVal value As Single)
                m_Rotation = value
                m_Value = "R" & value
            End Set
        End Property

        Public Function Clone() As Object Implements System.ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function

        Public Function RotationRadians() As Double
            Return m_Rotation / 180 * PI
        End Function
    End Class

    Public Interface EaglePad
        Property Name() As String
        ReadOnly Property PadType() As PadTypes
    End Interface

    Public Interface EagleSaveable
        ReadOnly Property NodeName() As String
        Sub WriteXml(ByVal Doc As XmlDocument, ByVal Node As XmlNode) 'saves the xml of the object to doc appending to node
    End Interface

    Public MustInherit Class Graphic
        Implements ICloneable

        Protected m_Owner As GraphicOwner 'the owner, schematic, package, plain, symbol,...
        Protected m_UpdateGraphics As Boolean 'set to true if some of the graphics classes need to be updated before rendering
        Protected m_Selected As Boolean

        Public Sub New(ByVal Owner As GraphicOwner)
            m_Owner = Owner
            m_UpdateGraphics = True
        End Sub

        ''' <summary>
        ''' Gets / Sets the owner of this graphic object, may have to be adjusted after a clone() was called
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Owner() As GraphicOwner
            Get
                Return m_Owner
            End Get
            Set(ByVal value As GraphicOwner)
                m_Owner = value
            End Set
        End Property

        ''' <summary>
        ''' This sub executes code that generates graphics objects for faster rendering
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overridable Sub RebuildGraphics()
            m_UpdateGraphics = False
        End Sub

        ''' <summary>
        ''' Renders the Graphic (line, wire, rectangle,...) to the graphics
        ''' </summary>
        ''' <param name="Graphics"></param>
        ''' <remarks></remarks>
        Public Overridable Sub Render(ByVal Graphics As Graphics)
            If m_UpdateGraphics Then RebuildGraphics()
        End Sub

        ''' <summary>
        ''' Returns the region that fits all points after rendering to Graphics
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GetRegion() As Region
            If m_UpdateGraphics Then RebuildGraphics()
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets / Sets if the graphic must be drawn as selected
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Selected() As Boolean
            Get
                Return m_Selected
            End Get
            Set(ByVal value As Boolean)
                If value <> m_Selected Then
                    m_Selected = value
                    m_UpdateGraphics = True
                End If
            End Set
        End Property

        Protected Function GetLayerColor(ByVal Layer As Integer) As System.Drawing.Color
            If m_Selected Then
                Return m_Owner.Drawing.Layer(Layer).SelectedColor
            Else
                Return m_Owner.Drawing.Layer(Layer).Color
            End If
        End Function

        ''' <summary>
        ''' Returns a clone of the graphic, having the same owner
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Clone() As Object Implements System.ICloneable.Clone
            Dim Cloned As Graphic = DirectCast(Me.MemberwiseClone, Graphic)
            Cloned.m_UpdateGraphics = True
            Return Cloned
        End Function
    End Class

    ''' <summary>
    ''' This class represents an owner of a graphic element, this can be package, symbol or plain (for schematic)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GraphicOwner
        Public Package As Package
        Public Symbol As Symbol
        Public Plain As Plain
        Public Sheet As Sheet
        Public Library As Library
        Public Segment As Segment
        Public Owner As Object
        Public Drawing As Drawing

        Dim m_Type As GraphicOwnerType

        'Public Sub New(ByVal Package As Package)
        '    Me.Package = Package
        '    Owner = Package
        '    m_Type = GraphicOwnerType.GO_Package
        '    Drawing = Package.Drawing
        'End Sub

        Public Sub New(ByVal SymbolOrPackage As GraphicSymbol)
            Owner = SymbolOrPackage
            If TypeOf SymbolOrPackage Is Package Then
                m_Type = GraphicOwnerType.GO_Package
                Me.Package = CType(SymbolOrPackage, Package)
            Else
                m_Type = GraphicOwnerType.GO_Symbol
                Me.Symbol = CType(SymbolOrPackage, Symbol)
            End If
            Drawing = SymbolOrPackage.Drawing
        End Sub

        Public Sub New(ByVal Plain As Plain)
            Me.Plain = Plain
            Owner = Plain
            m_Type = GraphicOwnerType.GO_Plain
            Drawing = Plain.Sheet.Schematic.Drawing
        End Sub

        Public Sub New(ByVal Library As Library)
            Me.Library = Library
            Owner = Library
            m_Type = GraphicOwnerType.GO_Library
            Drawing = Library.Drawing
        End Sub

        Public Sub New(ByVal Sheet As Sheet)
            Me.Sheet = Sheet
            Owner = Sheet
            m_Type = GraphicOwnerType.GO_Sheet
            Drawing = Sheet.Schematic.Drawing
        End Sub

        Public Sub New(ByVal Segment As Segment)
            Me.Segment = Segment
            Owner = Segment
            m_Type = GraphicOwnerType.GO_Segment
            Drawing = Segment.Sheet.Schematic.Drawing
        End Sub

        Public Overridable ReadOnly Property Type() As GraphicOwnerType
            Get
                Return m_Type
            End Get
        End Property

    End Class

    Public Class Vertex
        Implements EagleSaveable
        Implements ICloneable

        Dim m_Location As PointF
        Dim m_Curve As Single

        Public Sub New(ByVal Polygon As Polygon, ByVal Xml As XmlNode)
            m_Location = New PointF(toSingle(Xml.Attributes("x").Value), toSingle(Xml.Attributes("y").Value))
            If Xml.Attributes("curve") IsNot Nothing Then m_Curve = toSingle(Xml.Attributes("curve").Value)
        End Sub

        Public ReadOnly Property Location() As PointF
            Get
                Return m_Location
            End Get
        End Property

        Public ReadOnly Property X() As Single
            Get
                Return m_Location.X
            End Get
        End Property

        Public ReadOnly Property Y() As Single
            Get
                Return m_Location.Y
            End Get
        End Property

        Public ReadOnly Property Curve() As Single
            Get
                Return m_Curve
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("curve")).Value = ToEagleSingle(m_Curve)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "vertex"
            End Get
        End Property

        Public Overridable Function Clone() As Object Implements System.ICloneable.Clone
            Return Me.MemberwiseClone
        End Function
    End Class

    Public Class Polygon
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Vertices As New List(Of Vertex)
        Dim m_Width As Single
        Dim m_Layer As Integer
        Dim m_Spacing As Single
        Dim m_Pour As PolygonPour = PolygonPour.PP_solid
        Dim m_Isolate As Single
        Dim m_Orphans As Boolean
        Dim m_Thermals As Boolean = True
        Dim m_Rank As Integer
        Dim m_Pen As Pen
        Dim m_Region As Region
        Dim m_Path As GraphicsPath

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Width = toSingle(Xml.Attributes("width").Value)
            m_Layer = Xml.Attributes("layer").Value
            If Xml.Attributes("pour") IsNot Nothing Then m_Pour = [Enum].Parse(GetType(PolygonPour), "PP_" & Xml.Attributes("pour").Value)
            If Xml.Attributes("spacing") IsNot Nothing Then m_Spacing = toSingle(Xml.Attributes("spacing").Value)
            If Xml.Attributes("isolate") IsNot Nothing Then m_Isolate = toSingle(Xml.Attributes("isolate").Value)
            If Xml.Attributes("orphans") IsNot Nothing Then m_Orphans = Functions.ParseBool(Xml.Attributes("orphans").Value)
            If Xml.Attributes("thermals") IsNot Nothing Then m_Thermals = Functions.ParseBool(Xml.Attributes("thermals").Value)
            If Xml.Attributes("rank") IsNot Nothing Then m_Isolate = Xml.Attributes("rank").Value
            Dim VertexNodes As XmlNodeList = Xml.SelectNodes("./vertex")
            For Each VertexNode As XmlNode In VertexNodes
                m_Vertices.Add(New Vertex(Me, VertexNode))
            Next
        End Sub


        Private Sub AddCurve(ByVal Point0 As PointF, ByVal Point1 As PointF, ByVal Curve As Single, ByVal Path As GraphicsPath)
            'http://mymathforum.com/algebra/21368-find-equation-circle-given-two-points-arc-angle.html
            Dim d As Single = DistanceF(Point0, Point1) 'distance between 2 points
            Dim r2 As Single = d ^ 2 / (2 * (1 - Cos(Curve / 180 * PI))) '=r² of circle
            Dim r As Single = Sqrt(r2)
            Dim MidPoint As New PointF((Point0.X + Point1.X) / 2, (Point0.Y + Point1.Y) / 2) 'midpoint on line from point 0 to point 1
            Dim m As Single = (Point0.X - Point1.X) / (Point1.Y - Point0.Y) ' slope of the perpendicular P0-P1
            Dim a As Single = Sqrt(r2 - (d / 2) ^ 2) 'pythagoras distance to center

            Dim C As PointF 'center of circle

            If (r2 - (d / 2) ^ 2) <= 0 Then
                a = 0
            End If

            If (Point1.Y = Point0.Y) Then
                m = 0
            End If

            Dim Vector As New PointF(Point1.X - Point0.X, Point1.Y - Point0.Y) 'this vector gives the direction of the center of the circle depending on the location of point 1 and 2
            If Vector.X <> 0 Then Vector.X = Vector.X / Abs(Vector.X)
            If Vector.Y <> 0 Then Vector.Y = Vector.Y / Abs(Vector.Y)

            Dim tmp As Single = Vector.X
            Vector.X = -Vector.Y
            Vector.Y = Vector.X

            If Abs(Curve) > 180 Then 'for the center coo we can do +/-sqrt(), if the curve is larger than 180 we need * -1
                Vector.X = Vector.X * -1
                Vector.Y = Vector.Y * -1
            End If

            C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1) * Curve / Abs(Curve) * Vector.X
            C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1) * Curve / Abs(Curve) * Vector.Y
            Dim CurveStart As Single = GetAngle(C, r, Point0) 'get the start angle on the circle always begin at point 0
            Dim Rect As New RectangleF
            Rect.X = C.X - r 'rectangle the circle fits in
            Rect.Y = C.Y - r
            Rect.Width = r * 2
            Rect.Height = r * 2

            Path.AddArc(Rect, CurveStart, Curve)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            Dim PVertex As Vertex
            MyBase.RebuildGraphics()

            m_Path = New GraphicsPath()
            m_Pen = New Pen(GetLayerColor(m_Layer), m_Width)
            m_Path.StartFigure()
            If m_Vertices.Count > 0 Then
                PVertex = m_Vertices(0)
                For i As Integer = 1 To m_Vertices.Count - 1
                    If PVertex.Curve = 0 Then 'straight line
                        m_Path.AddLine(PVertex.Location, m_Vertices(i).Location)
                    Else 'curved line (=circle segment)
                        AddCurve(PVertex.Location, m_Vertices(i).Location, PVertex.Curve, m_Path)
                    End If
                    PVertex = m_Vertices(i)
                Next
                If PVertex.Curve = 0 Then 'straight line
                    m_Path.AddLine(PVertex.Location, m_Vertices(0).Location)
                Else 'curved line (=circle segment)
                    AddCurve(PVertex.Location, m_Vertices(0).Location, PVertex.Curve, m_Path)
                End If
            End If
            m_Path.CloseFigure()
            m_Region = New Region(m_Path)
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Graphics.DrawPath(m_Pen, m_Path)
            Select Case (m_Pour)
                Case PolygonPour.PP_cutout
                Case PolygonPour.PP_hatch
                    Graphics.FillPath(New System.Drawing.Drawing2D.HatchBrush(Drawing2D.HatchStyle.Cross, m_Pen.Color, System.Drawing.Color.Transparent), m_Path)
                    'Graphics.FillPolygon(New System.Drawing.Drawing2D.HatchBrush(Drawing2D.HatchStyle.Cross, m_Pen.Color, System.Drawing.Color.Transparent), m_Points)
                Case PolygonPour.PP_solid
                    Graphics.FillPath(New SolidBrush(m_Pen.Color), m_Path)
            End Select
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Return m_Region
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("width")).Value = ToEagleSingle(m_Width)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("pour")).Value = Mid([Enum].GetName(GetType(PolygonPour), m_Pour), 4)
                .Append(Doc.CreateAttribute("spacing")).Value = ToEagleSingle(m_Spacing)
                .Append(Doc.CreateAttribute("isolate")).Value = ToEagleSingle(m_Isolate)
                .Append(Doc.CreateAttribute("orphans")).Value = ToEagleBool(m_Orphans)
                .Append(Doc.CreateAttribute("thermals")).Value = ToEagleBool(m_Thermals)
                .Append(Doc.CreateAttribute("rank")).Value = m_Isolate
            End With
            For Each Vertex As Vertex In m_Vertices
                Vertex.WriteXml(Doc, Node.AppendChild(Doc.CreateAttribute("vertex")))
            Next
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "polygon"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As Polygon = DirectCast(MyBase.Clone(), Polygon)
            Cloned.m_Vertices = New List(Of Vertex)
            For Each Vertex As Vertex In m_Vertices
                Cloned.m_Vertices.Add(Vertex.Clone())
            Next
            Return Cloned
        End Function

    End Class

    Public Class Pad
        Inherits Graphic
        Implements EaglePad
        Implements EagleSaveable

        Dim m_Name As String
        Dim m_Location As PointF
        Dim m_Drill As Single
        Dim m_Diameter As Single = 0
        Dim m_Shape As PadShape = PadShape.PS_round
        Dim m_Rotation As New Rotation
        Dim m_Stop As Boolean = True
        Dim m_Thermals As Boolean = True
        Dim m_First As Boolean = False
        Dim m_Rectangle As New RectangleF
        Dim m_Hole As New RectangleF
        Dim m_Brush As SolidBrush
        Dim m_HoleRegion As Region
        Dim m_OctagonPoints() As PointF
        Dim m_Region As Region

        Public Const DefaultPadColorNumber As Integer = 2

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Name = Xml.Attributes("name").Value
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            m_Drill = toSingle(Xml.Attributes("drill").Value)
            If Xml.Attributes("diameter") IsNot Nothing Then m_Diameter = toSingle(Xml.Attributes("diameter").Value)
            If Xml.Attributes("shape") IsNot Nothing Then m_Shape = [Enum].Parse(GetType(PadShape), "PS_" & Xml.Attributes("shape").Value)
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
            If Xml.Attributes("stop") IsNot Nothing Then m_Stop = Functions.ParseBool(Xml.Attributes("stop").Value)
            If Xml.Attributes("thermals") IsNot Nothing Then m_Thermals = Functions.ParseBool(Xml.Attributes("thermals").Value)
            If Xml.Attributes("first") IsNot Nothing Then m_First = Functions.ParseBool(Xml.Attributes("first").Value)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            Dim Path As New GraphicsPath()
            Dim Transform As New Matrix()
            MyBase.RebuildGraphics()
            Select Case m_Shape
                Case PadShape.PS_octagon
                    ReDim m_OctagonPoints(0 To 8)
                    For i As Integer = 0 To 8
                        m_OctagonPoints(i) = New PointF(m_Diameter / 2 * Math.Sin(Math.PI / 4 * i + Math.PI / 8), m_Diameter / 2 * Math.Cos(Math.PI / 4 * i + Math.PI / 8))
                    Next
                    Path.AddPolygon(m_OctagonPoints)
                    Transform.Rotate(-45)
                Case PadShape.PS_offset
                    m_Rectangle.X = 0
                    m_Rectangle.Y = -m_Diameter / 2
                    m_Rectangle.Height = m_Diameter
                    m_Rectangle.Width = m_Diameter
                    Path.AddRectangle(m_Rectangle)
                    Path.AddRectangle(New RectangleF(-m_Diameter / 2, -m_Diameter / 2, m_Diameter, m_Diameter))
                    Path.AddRectangle(New RectangleF(m_Diameter / 2, -m_Diameter / 2, m_Diameter, m_Diameter))
                Case PadShape.PS_round, PadShape.PS_square, PadShape.PS_long
                    m_Rectangle.X = -m_Diameter / 2
                    m_Rectangle.Y = -m_Diameter / 2
                    m_Rectangle.Width = m_Diameter
                    m_Rectangle.Height = m_Diameter
                    Path.AddRectangle(m_Rectangle)
                    If m_Shape = PadShape.PS_long Then
                        Path.AddRectangle(New RectangleF(-m_Diameter, -m_Diameter / 2, m_Diameter / 2, m_Diameter / 2))
                        Path.AddRectangle(New RectangleF(0, -m_Diameter / 2, m_Diameter / 2, m_Diameter / 2))
                    End If
            End Select

            m_Hole.X = -m_Diameter / 2
            m_Hole.Y = -m_Diameter / 2
            m_Hole.Width = m_Drill
            m_Hole.Height = m_Drill
            m_Brush = New SolidBrush(m_Owner.Drawing.Project().ConvertColor(DefaultPadColorNumber))
            m_HoleRegion = New Region(m_Hole)

            Transform.Rotate(-m_Rotation.Rotation)
            Transform.Translate(m_Location.X, m_Location.Y)
            Path.Transform(Transform)
            m_Region = New Region(Path)
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()
            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            Graphics.RotateTransform(-m_Rotation.Rotation)

            Select Case m_Shape
                Case PadShape.PS_long
                    Graphics.ExcludeClip(m_HoleRegion)
                    Graphics.FillRectangle(m_Brush, m_Rectangle)
                    Graphics.FillPie(m_Brush, -m_Diameter, -m_Diameter / 2, m_Diameter, m_Diameter, 90, 180)
                    Graphics.FillPie(m_Brush, 0, -m_Diameter / 2, m_Diameter, m_Diameter, -90, 180)
                Case PadShape.PS_octagon
                    Graphics.ExcludeClip(m_HoleRegion)
                    Graphics.FillPolygon(m_Brush, m_OctagonPoints)
                Case PadShape.PS_offset
                    Graphics.ExcludeClip(m_HoleRegion)
                    Graphics.FillRectangle(m_Brush, m_Rectangle)
                    Graphics.FillPie(m_Brush, -m_Diameter / 2, -m_Diameter / 2, m_Diameter, m_Diameter, 90, 180)
                    Graphics.FillPie(m_Brush, m_Diameter / 2, -m_Diameter / 2, m_Diameter, m_Diameter, -90, 180)
                Case PadShape.PS_round
                    Graphics.ExcludeClip(m_HoleRegion)
                    Graphics.FillEllipse(m_Brush, m_Rectangle)
                    Graphics.ResetClip()
                Case PadShape.PS_square
                    Graphics.ExcludeClip(m_HoleRegion)
                    Graphics.FillRectangle(m_Brush, m_Rectangle)
                    Graphics.ResetClip()
            End Select
            Graphics.EndContainer(Container)
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Return m_Region
        End Function

        Public Property Name() As String Implements EaglePad.Name
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public ReadOnly Property PadType() As PadTypes Implements EaglePad.PadType
            Get
                Return PadTypes.PadTypeThroughHole
            End Get
        End Property

        Public Property Location() As PointF
            Get
                Return m_Location
            End Get
            Set(ByVal value As PointF)
                m_Location = value
            End Set
        End Property

        Public Property Drill() As Single
            Get
                Return m_Drill
            End Get
            Set(ByVal value As Single)
                m_Drill = value
                m_UpdateGraphics = True 'remember that we need to update the graphics objects before rendering
            End Set
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("drill")).Value = ToEagleSingle(m_Drill)
                .Append(Doc.CreateAttribute("diameter")).Value = ToEagleSingle(m_Diameter)
                .Append(Doc.CreateAttribute("shape")).Value = Mid([Enum].GetName(GetType(PadShape), m_Shape), 4)
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("stop")).Value = ToEagleBool(m_Stop)
                .Append(Doc.CreateAttribute("thermals")).Value = ToEagleBool(m_Thermals)
                .Append(Doc.CreateAttribute("first")).Value = ToEagleBool(m_First)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "pad"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As Pad = DirectCast(MyBase.Clone(), Pad)
            Cloned.m_Rotation = m_Rotation.Clone()
            Return Cloned
        End Function

    End Class

    Public Class SMD
        Inherits Graphic
        Implements EaglePad
        Implements EagleSaveable

        Dim m_Location As New PointF
        Dim m_Name As String
        Dim m_Width As Single
        Dim m_Height As Single
        Dim m_Layer As Integer
        Dim m_RoundNess As Integer
        Dim m_Rotation As New Rotation
        Dim m_Stop As Boolean
        Dim m_Thermals As Boolean
        Dim m_Cream As Boolean
        Dim m_Rect As New RectangleF
        Dim m_GraphicsPath As GraphicsPath
        Dim m_Brush As SolidBrush
        Dim m_Region As Region

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Name = Xml.Attributes("name").Value
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            m_Width = toSingle(Xml.Attributes("dx").Value)
            m_Height = toSingle(Xml.Attributes("dy").Value)
            m_Layer = Xml.Attributes("layer").Value
            If Xml.Attributes("roundness") IsNot Nothing Then m_RoundNess = Xml.Attributes("roundness").Value
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
            If Xml.Attributes("stop") IsNot Nothing Then m_Stop = Functions.ParseBool(Xml.Attributes("stop").Value)
            If Xml.Attributes("thermals") IsNot Nothing Then m_Thermals = Functions.ParseBool(Xml.Attributes("thermals").Value)
            If Xml.Attributes("cream") IsNot Nothing Then m_Cream = Functions.ParseBool(Xml.Attributes("cream").Value)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Rect.X = -m_Width
            m_Rect.Y = -m_Height
            m_Rect.Width = m_Width
            m_Rect.Height = m_Height
            If m_RoundNess > 0 Then
                m_GraphicsPath = Functions.RoundedRectangle(m_Rect, m_RoundNess * m_Height)
                m_Region = New Region(m_GraphicsPath)
            Else
                m_Region = New Region(m_Rect)
            End If
            Dim Mtrx As New Matrix()
            Mtrx.Rotate(-m_Rotation.Rotation)
            Mtrx.Translate(m_Location.X, m_Location.Y)
            m_Region.Transform(Mtrx)
            m_Brush = New SolidBrush(GetLayerColor(m_Layer))
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()

            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            Graphics.RotateTransform(-m_Rotation.Rotation)

            If m_RoundNess > 0 Then
                Graphics.FillPath(m_Brush, m_GraphicsPath)
            Else
                Graphics.FillRectangle(m_Brush, m_Rect)
            End If
            Graphics.EndContainer(Container)
            'return new RectangleF (m_location.X , m_location.Y 
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Return m_Region
        End Function

        Public Property Name() As String Implements EaglePad.Name
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
                m_UpdateGraphics = True
            End Set
        End Property

        Public ReadOnly Property PadType() As PadTypes Implements EaglePad.PadType
            Get
                Return PadTypes.PadTypeSMD
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("dx")).Value = ToEagleSingle(m_Width)
                .Append(Doc.CreateAttribute("dy")).Value = ToEagleSingle(m_Height)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("roundness")).Value = m_RoundNess
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("stop")).Value = ToEagleBool(m_Stop)
                .Append(Doc.CreateAttribute("thermals")).Value = ToEagleBool(m_Thermals)
                .Append(Doc.CreateAttribute("cream")).Value = ToEagleBool(m_Cream)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "smd"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As SMD = DirectCast(MyBase.Clone(), SMD)
            Cloned.m_Rotation = m_Rotation.Clone()
            Return Cloned
        End Function

    End Class

    Public Class Wire
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Points(0 To 1) As PointF
        Dim m_Width As Double
        Dim m_Layer As Integer
        Dim m_Extent As String
        Dim m_Style As WireStyle = WireStyle.WS_continuous
        Dim m_Cap As WireCap = WireCap.WC_round
        Dim m_Curve As Single
        Dim m_Pen As Pen
        Dim m_Rect As New RectangleF
        Dim m_CurveStart As Single
        Dim m_CurveEnd As Single
        Dim m_CurveSweep As Single

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Point1 As PointF, ByVal Point2 As PointF, ByVal Width As Single, ByVal Layer As Integer)
            MyBase.New(Owner)
            m_Points(0) = Point1
            m_Points(1) = Point2
            m_Width = Width
            m_Layer = Layer
        End Sub

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Points(0).X = toSingle(Xml.Attributes("x1").Value)
            m_Points(0).Y = toSingle(Xml.Attributes("y1").Value)
            m_Points(1).X = toSingle(Xml.Attributes("x2").Value)
            m_Points(1).Y = toSingle(Xml.Attributes("y2").Value)
            m_Width = toSingle(Xml.Attributes("width").Value)
            m_Layer = Xml.Attributes("layer").Value
            If Xml.Attributes("extent") IsNot Nothing Then m_Extent = Xml.Attributes("extent").Value
            If Xml.Attributes("style") IsNot Nothing Then m_Style = [Enum].Parse(GetType(WireStyle), "WS_" & Xml.Attributes("style").Value)
            If Xml.Attributes("curve") IsNot Nothing Then m_Curve = toSingle(Xml.Attributes("curve").Value)
            If Xml.Attributes("cap") IsNot Nothing Then m_Cap = [Enum].Parse(GetType(WireCap), "WC_" & Xml.Attributes("cap").Value)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Pen = New Pen(GetLayerColor(m_Layer), m_Width)
            Select Case m_Style
                Case WireStyle.WS_continuous
                    m_Pen.DashStyle = DashStyle.Solid
                Case WireStyle.WS_dashdot
                    m_Pen.DashStyle = DashStyle.DashDot
                Case WireStyle.WS_longdash
                    m_Pen.DashStyle = DashStyle.Dash
                Case WireStyle.WS_shortdash
                    m_Pen.DashStyle = DashStyle.Dot
            End Select
            Select Case m_Cap
                Case WireCap.WC_flat
                    m_Pen.StartCap = LineCap.Flat
                    m_Pen.EndCap = LineCap.Flat
                Case WireCap.WC_round
                    m_Pen.StartCap = LineCap.Round
                    m_Pen.EndCap = LineCap.Round
            End Select

            If m_Curve = 0 Then 'normal line, easy
                m_Rect.X = Min(m_Points(0).X - m_Width / 2, m_Points(1).X - m_Width / 2)
                m_Rect.Y = Min(m_Points(0).Y - m_Width / 2, m_Points(1).Y - m_Width / 2)
                m_Rect.Width = Abs(m_Points(0).X - m_Points(1).X) + m_Width * 2
                m_Rect.Height = Abs(m_Points(0).Y - m_Points(1).Y) + m_Width * 2
            Else
                'http://mymathforum.com/algebra/21368-find-equation-circle-given-two-points-arc-angle.html
                Dim d As Single = DistanceF(m_Points(0), m_Points(1)) 'distance between 2 points
                Dim r2 As Single = d ^ 2 / (2 * (1 - Cos(m_Curve / 180 * PI))) '=r² of circle
                Dim r As Single = Sqrt(r2)
                Dim MidPoint As New PointF((m_Points(0).X + m_Points(1).X) / 2, (m_Points(0).Y + m_Points(1).Y) / 2) 'midpoint on line from point 0 to point 1
                Dim m As Single = (m_Points(0).X - m_Points(1).X) / (m_Points(1).Y - m_Points(0).Y) ' slope of the perpendicular P0-P1
                Dim a As Single = Sqrt(r2 - (d / 2) ^ 2) 'pythagoras distance to center

                Dim C As PointF 'center of circle

                If (r2 - (d / 2) ^ 2) <= 0 Then
                    a = 0
                End If

                If (m_Points(1).Y = m_Points(0).Y) Then
                    m = 0
                End If

                'If m_Points(1).X > m_Points(0).X Then
                '    If m_Points(1).Y > m_Points(0).Y Then
                '        If m_Curve > 0 Then
                '            C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1) 'ok
                '            C.Y = MidPoint.Y - (m * a) / Sqrt(m ^ 2 + 1)
                '            'C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = m_CurveStart - m_CurveEnd
                '        Else
                '            C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1) 'ok
                '            C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1) '-?
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = -(m_CurveStart - m_CurveEnd)
                '        End If
                '    Else
                '        If m_Curve > 0 Then
                '            C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1)
                '            C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = m_CurveStart - m_CurveEnd
                '        Else
                '            C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1)
                '            C.Y = MidPoint.Y - (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = -(m_CurveStart - m_CurveEnd)
                '        End If
                '    End If
                'Else
                '    If m_Points(1).Y > m_Points(0).Y Then
                '        If m_Curve > 0 Then
                '            C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1) '+ ???
                '            C.Y = MidPoint.Y - (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = -(m_CurveStart - m_CurveEnd)
                '        Else
                '            C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1)
                '            'C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1)
                '            C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = m_CurveStart - m_CurveEnd
                '        End If
                '    Else
                '        If m_Curve > 0 Then
                '            C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1)
                '            C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = -(m_CurveEnd - m_CurveStart)
                '        Else
                '            C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1)
                '            C.Y = MidPoint.Y - (m * a) / Sqrt(m ^ 2 + 1)
                '            m_CurveStart = GetAngle(C, r, m_Points(1))
                '            m_CurveEnd = GetAngle(C, r, m_Points(0))
                '            m_CurveSweep = m_CurveEnd - m_CurveStart
                '        End If
                '    End If
                'End If


                Dim Vector As New PointF(m_Points(1).X - m_Points(0).X, m_Points(1).Y - m_Points(0).Y) 'this vector gives the direction of the center of the circle depending on the location of point 1 and 2
                If Vector.X <> 0 Then Vector.X = Vector.X / Abs(Vector.X)
                If Vector.Y <> 0 Then Vector.Y = Vector.Y / Abs(Vector.Y)

                Dim tmp As Single = Vector.X
                Vector.X = -Vector.Y
                Vector.Y = Vector.X

                If Abs(m_Curve) > 180 Then 'for the center coo we can do +/-sqrt(), if the curve is larger than 180 we need * -1
                    Vector.X = Vector.X * -1
                    Vector.Y = Vector.Y * -1
                End If

                C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1) * m_Curve / Abs(m_Curve) * Vector.X
                C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1) * m_Curve / Abs(m_Curve) * Vector.Y
                m_CurveStart = GetAngle(C, r, m_Points(0)) 'get the start angle on the circle always begin at point 0

                'If m_Curve > 0 Then
                '    C.X = MidPoint.X + a / Sqrt(m ^ 2 + 1) * Vector.X
                '    C.Y = MidPoint.Y + (m * a) / Sqrt(m ^ 2 + 1) * Vector.Y
                '    m_CurveStart = GetAngle(C, r, m_Points(0))
                '    m_CurveEnd = GetAngle(C, r, m_Points(0))
                '    m_CurveSweep = -(m_CurveEnd - m_CurveStart)
                'Else
                '    C.X = MidPoint.X - a / Sqrt(m ^ 2 + 1) * Vector.X
                '    C.Y = MidPoint.Y - (m * a) / Sqrt(m ^ 2 + 1) * Vector.Y
                '    m_CurveStart = GetAngle(C, r, m_Points(0))
                '    m_CurveEnd = GetAngle(C, r, m_Points(0))
                '    m_CurveSweep = m_CurveEnd - m_CurveStart
                'End If

                m_CurveSweep = m_Curve 'we sweep the angle given
                'm_CurveSweep = m_CurveStart - m_CurveEnd
                'm_CurveStart = Min(m_CurveStart, m_CurveEnd)
                'If m_Curve < 0 Then
                '    m_CurveSweep = -m_CurveSweep
                'End If

                m_Rect.X = C.X - r 'rectangle the circle fits in
                m_Rect.Y = C.Y - r
                m_Rect.Width = r * 2
                m_Rect.Height = r * 2

            End If
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            If m_Curve = 0 Then
                Graphics.DrawLine(m_Pen, m_Points(0).X, m_Points(0).Y, m_Points(1).X, m_Points(1).Y)
            Else
                Graphics.DrawArc(m_Pen, m_Rect.X, m_Rect.Y, m_Rect.Width, m_Rect.Height, m_CurveStart, m_CurveSweep)
            End If
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Dim Path As New GraphicsPath()
            If m_Curve = 0 Then
                Path.AddRectangle(m_Rect)
            Else
                Path.AddArc(m_Rect.X, m_Rect.Y, m_Rect.Width, m_Rect.Height, m_CurveStart, m_CurveSweep)
            End If
            Return New Region(Path)
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x1")).Value = ToEagleSingle(m_Points(0).X)
                .Append(Doc.CreateAttribute("y1")).Value = ToEagleSingle(m_Points(0).Y)
                .Append(Doc.CreateAttribute("x2")).Value = ToEagleSingle(m_Points(1).X)
                .Append(Doc.CreateAttribute("y2")).Value = ToEagleSingle(m_Points(1).Y)
                .Append(Doc.CreateAttribute("width")).Value = ToEagleSingle(m_Width)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                If m_Extent <> "" Then .Append(Doc.CreateAttribute("extent")).Value = m_Extent
                .Append(Doc.CreateAttribute("style")).Value = Mid([Enum].GetName(GetType(WireStyle), m_Style), 4)
                .Append(Doc.CreateAttribute("curve")).Value = ToEagleSingle(m_Curve)
                .Append(Doc.CreateAttribute("cap")).Value = Mid([Enum].GetName(GetType(WireCap), m_Cap), 4)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "wire"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Represents a graphical Pin on a Symbol of a gate of a deviceset
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Pin
        Inherits Graphic
        Implements EagleSaveable

        Private Class LineF
            Public Point1 As PointF
            Public Point2 As PointF
            Public Sub New(ByVal Point1 As PointF, ByVal Point2 As PointF)
                Me.Point1 = Point1
                Me.Point2 = Point2
            End Sub
            Public Function GetRect(ByVal LineWidth As Single) As RectangleF
                Dim x As Single = Min(Point1.X, Point2.X)
                Dim y As Single = Min(Point1.Y, Point2.Y)
                Return New RectangleF(x, y, Abs(Point1.X - Point2.X) + LineWidth, Abs(Point1.Y) - Abs(Point2.Y) + LineWidth)
            End Function
        End Class

        Dim m_Name As String
        Dim m_Location As PointF
        Dim m_Length As PinLength = PinLength.PL_long
        Dim m_Rotation As New Rotation
        Dim m_Visible As PinVisible = PinVisible.PV_both
        Dim m_Direction As PinDirection = PinDirection.PD_io
        Dim m_Function As PinFunction = PinFunction.PF_none
        Dim m_SwapLevel As Integer = 0
        Dim m_Pen As Pen
        Dim m_TextPen As Pen
        Dim m_Width As Single
        'Dim m_Path As GraphicsPath
        Dim m_TextPath As GraphicsPath
        Dim m_Region As Region
        Dim m_PadName As String = ""
        Dim m_TextRotation As Integer

        Dim m_Lines As New List(Of LineF) 'contains all lines that have to be drawn
        Dim m_Circles As New List(Of RectangleF) 'contains all circles that have to be drawn

        Public Const PinDefaultLayer As Integer = Layer.LayerSymbols
        Public Const PinDefaultWireWidth As Single = 0.254

        Private Const m_DotDiameter As Single = 2.2

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Name = Xml.Attributes("name").Value
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            If Xml.Attributes("visible") IsNot Nothing Then m_Visible = [Enum].Parse(GetType(PinVisible), "PV_" & Xml.Attributes("visible").Value)
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
            If Xml.Attributes("direction") IsNot Nothing Then m_Direction = [Enum].Parse(GetType(PinDirection), "PD_" & Xml.Attributes("direction").Value)
            If Xml.Attributes("length") IsNot Nothing Then m_Length = [Enum].Parse(GetType(PinLength), "PL_" & Xml.Attributes("length").Value)
            If Xml.Attributes("function") IsNot Nothing Then m_Function = [Enum].Parse(GetType(PinFunction), "PF_" & Xml.Attributes("function").Value)
            If Xml.Attributes("swaplevel") IsNot Nothing Then m_SwapLevel = Xml.Attributes("swaplevel").Value
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            Dim ExtraTextOffset As Single = 0
            Dim LineWidth As Single

            m_Lines.Clear()
            m_TextPath = New GraphicsPath()
            Select Case m_Length
                Case PinLength.PL_long
                    m_Width = 7.75
                Case PinLength.PL_middle
                    m_Width = 5 'mm
                Case PinLength.PL_point
                    m_Width = 0
                Case PinLength.PL_short
                    m_Width = 2.5
            End Select
            LineWidth = m_Width

            'ExtraTextOffset = m_DotDiameter / 2
            If m_Function = PinFunction.PF_clk OrElse m_Function = PinFunction.PF_dotclk Then
                m_Lines.Add(New LineF(New PointF(m_Width, -m_DotDiameter / 2), New PointF(m_Width + m_DotDiameter / 2, 0)))
                m_Lines.Add(New LineF(New PointF(m_Width + m_DotDiameter / 2, 0), New PointF(m_Width, m_DotDiameter / 2)))
            End If

            If m_Function = PinFunction.PF_dot OrElse m_Function = PinFunction.PF_dotclk Then
                LineWidth = LineWidth - m_DotDiameter
                m_Circles.Add(New RectangleF(m_Width - m_DotDiameter, -m_DotDiameter / 2, m_DotDiameter, m_DotDiameter))
            End If

            If m_Width > 0 Then
                m_Lines.Add(New LineF(New PointF(0, 0), New PointF(LineWidth, 0)))
            End If

            'Select Case m_Function
            '    Case PinFunction.PF_clk
            '        m_Path.AddLine(New Point(m_Width, -m_DotDiameter / 2), New Point(m_Width + m_DotDiameter, 0))
            '        m_Path.AddLine(New Point(m_Width, m_DotDiameter / 2), New Point(m_Width + m_DotDiameter, 0))
            '        ExtraTextOffset = m_DotDiameter
            '    Case PinFunction.PF_dot
            '        m_Width = m_Width - m_DotDiameter
            '        m_Path.AddEllipse(New RectangleF(m_Width, -m_DotDiameter / 2, m_DotDiameter, m_DotDiameter))
            '        ExtraTextOffset = m_DotDiameter
            '    Case PinFunction.PF_dotclk
            '        m_Path.AddEllipse(New RectangleF(m_Width, -m_DotDiameter / 2, m_DotDiameter, m_DotDiameter))
            '        m_Path.AddLine(New Point(m_Width + m_DotDiameter, -m_DotDiameter / 2), New Point(m_Width + m_DotDiameter * 2, 0))
            '        m_Path.AddLine(New Point(m_Width + m_DotDiameter, m_DotDiameter / 2), New Point(m_Width + m_DotDiameter * 2, 0))
            '        ExtraTextOffset = m_DotDiameter * 2
            '    Case PinFunction.PF_none
            'End Select
            'If m_Width > 0 Then
            '    m_Path.AddLine(0, 0, m_Width, 0)
            '    LineRect.Width = m_Width 'rectangle that fits the single line
            '    LineRect.Height = 1
            'End If
            Dim m_NameOrigin As PointF
            Dim m_PadNameOrigin As PointF
            Dim PinTextFormat As StringFormatFlags
            Dim PadTextFormat As StringFormatFlags

            Select Case m_Rotation.Rotation
                Case 0
                    m_NameOrigin = New PointF(m_Width + m_DotDiameter / 2 + ExtraTextOffset, -m_DotDiameter / 2)
                    m_PadNameOrigin = New PointF(m_Width / 2, -m_DotDiameter * 1.1)
                    m_TextRotation = 0
                    PadTextFormat = StringFormatFlags.DirectionRightToLeft
                Case 90
                    'm_NameOrigin = New PointF(-m_Width - m_DotDiameter / 2 - ExtraTextOffset, -m_DotDiameter / 2) '-(m_DotDiameter / 2 + ExtraTextOffset), -m_DotDiameter / 2) '-(m_DotDiameter / 2 + ExtraTextOffset)
                    'm_PadNameOrigin = New PointF(-m_Width / 2, -m_DotDiameter * 1.1)
                    m_NameOrigin = New PointF(m_Width + (m_DotDiameter / 2 + ExtraTextOffset), -m_DotDiameter / 2)
                    m_PadNameOrigin = New PointF(m_Width / 2, -m_DotDiameter * 1.1)
                    m_TextRotation = 90
                    PadTextFormat = StringFormatFlags.DirectionRightToLeft
                    'PinTextFormat = StringFormatFlags.DirectionRightToLeft
                Case 180
                    m_NameOrigin = New PointF(-m_Width - (m_DotDiameter / 2 + ExtraTextOffset), -m_DotDiameter / 2)
                    m_PadNameOrigin = New PointF(-m_Width / 2, -m_DotDiameter * 1.1)
                    m_TextRotation = 0
                    PinTextFormat = StringFormatFlags.DirectionRightToLeft
                Case 270
                    m_NameOrigin = New PointF(-m_Width - (m_DotDiameter / 2 + ExtraTextOffset), -m_DotDiameter / 2)
                    m_PadNameOrigin = New PointF(-m_Width / 2, -m_DotDiameter * 1.1)
                    'm_NameOrigin = New PointF(m_Width + m_DotDiameter / 2 + ExtraTextOffset, -m_DotDiameter / 2)
                    'm_PadNameOrigin = New PointF(m_Width / 2, -m_DotDiameter * 1.1)
                    PinTextFormat = StringFormatFlags.DirectionRightToLeft
                    m_TextRotation = 90
            End Select

            Select Case m_Visible
                Case PinVisible.PV_both
                    Dim Rotate As New Matrix
                    Rotate.Rotate(-m_Rotation.Rotation)
                    m_TextPath.AddString(m_Name, m_Owner.Drawing.Project.Font, System.Drawing.FontStyle.Regular, m_DotDiameter, m_NameOrigin, New System.Drawing.StringFormat(PinTextFormat))
                    m_TextPath.AddString(m_PadName, m_Owner.Drawing.Project.Font, System.Drawing.FontStyle.Regular, m_DotDiameter, m_PadNameOrigin, New StringFormat(PadTextFormat))
                Case PinVisible.PV_pad
                    m_TextPath.AddString(m_PadName, m_Owner.Drawing.Project.Font, System.Drawing.FontStyle.Regular, m_DotDiameter, m_PadNameOrigin, New StringFormat(PadTextFormat))
                Case PinVisible.PV_pin
                    m_TextPath.AddString(m_Name, m_Owner.Drawing.Project.Font, System.Drawing.FontStyle.Regular, m_DotDiameter, m_NameOrigin, New System.Drawing.StringFormat(PinTextFormat))
            End Select

            m_Pen = New Pen(GetLayerColor(PinDefaultLayer), PinDefaultWireWidth)
            m_TextPen = New Pen(GetLayerColor(Layer.LayerNames), PinDefaultWireWidth / 2)


        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()

            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            Graphics.RotateTransform(m_Rotation.Rotation)
            For Each Line As LineF In m_Lines
                Graphics.DrawLine(m_Pen, Line.Point1, Line.Point2)
            Next
            For Each Circle As RectangleF In m_Circles
                Graphics.DrawEllipse(m_Pen, Circle)
            Next
            Graphics.EndContainer(Container)

            Container = Graphics.BeginContainer()
            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            'If m_TextRotation <> 0 Then
            'Graphics.ScaleTransform(1, -1)
            'End If
            Graphics.RotateTransform(m_TextRotation)
            'If m_TextRotation = 0 Then
            Graphics.ScaleTransform(1, -1) 'mirror
            'Else
            'Graphics.ScaleTransform(-1, 1)
            'End If
            Graphics.FillPath(New SolidBrush(m_TextPen.Color), m_TextPath)
            Graphics.EndContainer(Container)
        End Sub

        Public Overrides Function GetRegion() As Region
            Dim TransForm As New Matrix
            MyBase.GetRegion()

            TransForm.Translate(m_Location.X, m_Location.Y)
            TransForm.Rotate(m_Rotation.Rotation)

            m_Region = New Region()
            m_Region.MakeEmpty()

            For Each Line As LineF In m_Lines
                m_Region.Union(Line.GetRect(m_Pen.Width))
            Next
            For Each Circle As RectangleF In m_Circles
                m_Region.Union(Circle)
            Next
            m_Region.Transform(TransForm)

            'm_Region.Union(m_TextPath)

            Dim m_TextRegion As Region

            m_TextRegion = New Region(m_TextPath)

            Dim TextTransform As New Matrix
            TextTransform.Translate(m_Location.X, m_Location.Y)
            TextTransform.Rotate(m_TextRotation)
            'If m_TextRotation = 0 Then
            TextTransform.Scale(1, -1) 'mirror
            'Else
            ' TextTransform.Scale(-1, 1)
            'End If
            m_TextRegion.Transform(TextTransform)

            m_Region.Union(m_TextRegion)
            Return m_Region
        End Function

        Public Property PadName() As String
            Get
                Return m_PadName
            End Get
            Set(ByVal value As String)
                m_PadName = value
                m_UpdateGraphics = True
            End Set
        End Property

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
                m_UpdateGraphics = True
            End Set
        End Property

        ''' <summary>
        ''' Returns the location of the point within the symbol
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Location() As PointF
            Get
                Return m_Location
            End Get
            Set(ByVal value As PointF)
                m_Location = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the location of where to connect the wire
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetWireLocation() As PointF
            Return m_Location
            'Select Case m_Rotation.Rotation
            '    Case 0
            '        Return New PointF(m_Location.X, m_Location.Y)
            '    Case 90
            '        Return New PointF(m_Location.X, m_Location.Y)
            '    Case 180
            '        Return New PointF(m_Location.X + m_Width, m_Location.Y)
            '    Case 270
            '        Return New PointF(m_Location.X, m_Location.Y + m_Width)
            'End Select
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With (Node.Attributes)
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("visible")).Value = Mid([Enum].GetName(GetType(PinVisible), m_Visible), 4)
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("direction")).Value = Mid([Enum].GetName(GetType(PinDirection), m_Direction), 4)
                .Append(Doc.CreateAttribute("length")).Value = Mid([Enum].GetName(GetType(PinLength), m_Length), 4)
                .Append(Doc.CreateAttribute("function")).Value = Mid([Enum].GetName(GetType(PinFunction), m_Function), 4)
                .Append(Doc.CreateAttribute("swaplevel")).Value = m_SwapLevel
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "pin"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As Pin = DirectCast(MyBase.Clone(), Pin)
            Cloned.m_Rotation = m_Rotation.Clone()
            Return Cloned
        End Function
    End Class

    Public Enum ParameterType As Integer
        PT_Text  'normal text
        PT_Name  '>NAME (replaced by the name of the device)
        PT_Value '>VALUE (replace by the value of the device)
    End Enum

    Public Class Text
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Location As New PointF
        Dim m_Size As Single
        Dim m_Layer As Integer
        Dim m_Font As TextFont = TextFont.TF_proportional
        Dim m_Ratio As Integer = 8
        Dim m_Rotation As New Rotation
        Dim m_Align As Align = Align.A_bottom_left
        Dim m_Text As String = ""
        Dim m_Path As GraphicsPath
        Dim m_Transform As Matrix
        Dim m_TextHeight As Single
        Dim m_RenderFont As Font

        Dim m_ParameterType As ParameterType 'holds if this is the >NAME or >VALUE or just regular text

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Text = Xml.ChildNodes(0).Value
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            m_Layer = Xml.Attributes("layer").Value
            m_Size = toSingle(Xml.Attributes("size").Value)
            If Xml.Attributes("font") IsNot Nothing Then m_Font = [Enum].Parse(GetType(TextFont), "TF_" & Xml.Attributes("font").Value)
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
            If Xml.Attributes("ratio") IsNot Nothing Then m_Ratio = Xml.Attributes("ratio").Value
            If Xml.Attributes("align") IsNot Nothing Then m_Align = [Enum].Parse(GetType(Align), "A_" & Xml.Attributes("align").Value)

            Select Case UCase(m_Text)
                Case ">NAME", ">PART"
                    m_ParameterType = ParameterType.PT_Name
                Case ">VALUE"
                    m_ParameterType = ParameterType.PT_Value
            End Select
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Transform = New Matrix
            m_Transform.Translate(m_Location.X, m_Location.Y)
            m_Transform.Rotate(m_Rotation.Rotation)
            m_Transform.Scale(1, -1)

            m_RenderFont = New Font(m_Owner.Drawing.Project.Font, m_Size, FontStyle.Regular, GraphicsUnit.World)
            m_Path = New GraphicsPath
            'needs improvement for alignment etc...
            m_Path.AddString(m_Text, m_Owner.Drawing.Project.Font, FontStyle.Regular, m_Size, New Point(0, 0), StringFormat.GenericDefault)
            m_Path.Transform(m_Transform)

        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            MyBase.GetRegion()
            Return New Region(m_Path)
        End Function

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)

            MyBase.Render(Graphics)
            'Font = New Font(m_Owner.Drawing.Project.Font, m_Size, FontStyle.Regular, GraphicsUnit.World)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()
            If m_TextHeight = 0 Then
                m_TextHeight = Graphics.MeasureString(m_Text, m_RenderFont, New PointF(0, 0), New System.Drawing.StringFormat()).Height
            End If
            Graphics.TranslateTransform(0, m_TextHeight) 'move for compensation of textheight
            Dim cnt2 As GraphicsContainer = Graphics.BeginContainer()
            Graphics.Transform = m_Transform
            Graphics.DrawString(m_Text, m_RenderFont, New SolidBrush(GetLayerColor(m_Layer)), 0, 0)
            Graphics.EndContainer(cnt2)
            Graphics.EndContainer(Container)
        End Sub

        ''' <summary>
        ''' Sets / gets the text displayed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Text() As String
            Get
                Return m_Text
            End Get
            Set(ByVal value As String)
                m_Text = value
                m_UpdateGraphics = True
                m_TextHeight = 0
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Type() As ParameterType
            Get
                Return m_ParameterType
            End Get
            Set(ByVal value As ParameterType)
                m_ParameterType = value
                m_UpdateGraphics = True
            End Set
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.AppendChild(Doc.CreateTextNode(m_Text))
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("size")).Value = ToEagleSingle(m_Size)
                .Append(Doc.CreateAttribute("font")).Value = Mid([Enum].GetName(GetType(TextFont), m_Font), 4)
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("ratio")).Value = m_Ratio
                .Append(Doc.CreateAttribute("align")).Value = Mid([Enum].GetName(GetType(Align), m_Align), 4)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "text"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As text = DirectCast(MyBase.Clone(), text)
            Cloned.m_Rotation = m_Rotation.Clone()
            Return Cloned
        End Function
    End Class

    Public Class Circle
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Location As New PointF
        Dim m_Rect As New RectangleF
        Dim m_Width As Single
        Dim m_Layer As Integer
        Dim m_Radius As Single
        Dim m_Pen As Pen

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            m_Layer = Xml.Attributes("layer").Value
            m_Width = toSingle(Xml.Attributes("width").Value)
            m_Radius = toSingle(Xml.Attributes("radius").Value)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Rect.X = -m_Radius
            m_Rect.Y = -m_Radius
            m_Rect.Width = m_Radius * 2
            m_Rect.Height = m_Radius * 2
            m_Pen = New Pen(GetLayerColor(m_Layer), m_Width)
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            MyBase.GetRegion()
            Dim Region As New Region(m_Rect)
            Region.Translate(m_Location.X, m_Location.Y)
            Return Region
        End Function

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()
            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            Graphics.DrawEllipse(m_Pen, m_Rect)
            Graphics.EndContainer(Container)
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("width")).Value = m_Width
                .Append(Doc.CreateAttribute("radius")).Value = m_Radius
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "circle"
            End Get
        End Property
    End Class

    Public Class Rectangle
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Points(0 To 1) As PointF
        Dim m_Layer As Integer
        Dim m_Rotation As New Rotation
        Dim m_Rect As New RectangleF
        Dim m_Brush As Brush

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.new(Owner)
            m_Points(0).X = toSingle(Xml.Attributes("x1").Value)
            m_Points(0).Y = toSingle(Xml.Attributes("y1").Value)
            m_Points(1).X = toSingle(Xml.Attributes("x2").Value)
            m_Points(1).Y = toSingle(Xml.Attributes("y2").Value)
            m_Layer = Xml.Attributes("layer").Value
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Rect.X = Min(m_Points(0).X, m_Points(1).X)
            m_Rect.Y = Min(m_Points(0).Y, m_Points(1).Y)
            m_Rect.Width = Abs(m_Points(0).X - m_Points(1).X)
            m_Rect.Height = Abs(m_Points(0).Y - m_Points(1).Y)
            m_Brush = New SolidBrush(GetLayerColor(m_Layer))
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            MyBase.GetRegion()
            Return New Region(m_Rect)
        End Function

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Graphics.FillRectangle(m_Brush, m_Rect)
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x1")).Value = ToEagleSingle(m_Points(0).X)
                .Append(Doc.CreateAttribute("y1")).Value = ToEagleSingle(m_Points(0).Y)
                .Append(Doc.CreateAttribute("x2")).Value = ToEagleSingle(m_Points(1).X)
                .Append(Doc.CreateAttribute("y2")).Value = ToEagleSingle(m_Points(1).Y)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "rectangle"
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As Rectangle = DirectCast(MyBase.Clone(), Rectangle)
            Cloned.m_Rotation = m_Rotation.Clone()
            Return Cloned
        End Function
    End Class

    ''' <summary>
    ''' Frame is not yet implemented
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Frame
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Points(0 To 1) As PointF
        Dim m_Columns As Integer
        Dim m_Rows As Integer
        Dim m_Layer As Integer
        Dim m_BorderLeft As Boolean
        Dim m_BorderTop As Boolean
        Dim m_BorderRight As Boolean
        Dim m_BorderBottom As Boolean

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Points(0).X = toSingle(Xml.Attributes("x1").Value)
            m_Points(0).Y = toSingle(Xml.Attributes("y1").Value)
            m_Points(1).X = toSingle(Xml.Attributes("x2").Value)
            m_Points(1).Y = toSingle(Xml.Attributes("y2").Value)
            m_Layer = Xml.Attributes("layer").Value
            m_Columns = Xml.Attributes("columns").Value
            m_Rows = Xml.Attributes("rows").Value
            m_Layer = Xml.Attributes("layer").Value
            m_BorderLeft = ParseBool(Xml.Attributes("border-left").Value)
            m_BorderTop = ParseBool(Xml.Attributes("border-top").Value)
            m_BorderRight = ParseBool(Xml.Attributes("border-right").Value)
            m_BorderBottom = ParseBool(Xml.Attributes("border-bottom").Value)
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            Dim Region As New Region
            MyBase.GetRegion()
            Region.MakeEmpty()
            Return Region
        End Function

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x1")).Value = ToEagleSingle(m_Points(0).X)
                .Append(Doc.CreateAttribute("y1")).Value = ToEagleSingle(m_Points(0).Y)
                .Append(Doc.CreateAttribute("x2")).Value = ToEagleSingle(m_Points(1).X)
                .Append(Doc.CreateAttribute("y2")).Value = ToEagleSingle(m_Points(1).Y)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("border-left")).Value = ToEagleBool(m_BorderLeft)
                .Append(Doc.CreateAttribute("border-top")).Value = ToEagleBool(m_BorderTop)
                .Append(Doc.CreateAttribute("border-right")).Value = ToEagleBool(m_BorderRight)
                .Append(Doc.CreateAttribute("border-bottom")).Value = ToEagleBool(m_BorderBottom)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "frame"
            End Get
        End Property
    End Class

    Public Class Hole
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Location As New PointF
        Dim m_Drill As Single
        ' Dim m_Plain As Plain
        Dim m_Rect As New RectangleF
        Dim m_Pen As Pen
        Dim m_Width As Single = 1

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
            m_Drill = toSingle(Xml.Attributes("drill").Value)
            RebuildGraphics()
        End Sub

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
            m_Rect = New RectangleF(-m_Drill / 2, -m_Drill / 2, m_Drill, m_Drill)
            m_Pen = New Pen(Color.Gray, m_Width) 'to be defined
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            MyBase.GetRegion()
            Dim Region As New Region(m_Rect)
            Region.Translate(m_Location.X, m_Location.Y)
            Return Region
        End Function

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            Dim Container As GraphicsContainer = Graphics.BeginContainer()
            Graphics.TranslateTransform(m_Location.X, m_Location.Y)
            Graphics.DrawEllipse(m_Pen, m_Rect)
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("drill")).Value = ToEagleSingle(m_Drill)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "hole"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Base Class for symbol or package
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GraphicSymbol
        Inherits Graphic
        Implements EagleSaveable

        Protected m_Name As String
        Protected m_Description As String
        Protected m_GraphicElements As New List(Of Graphic)
        Protected m_Pins As New List(Of Pin)

        Protected m_ValueParameters As New List(Of text) '>VALUE
        Protected m_NameParameters As New List(Of text) '>NAME
        Protected m_Pads As New List(Of EaglePad) 'SMD or throughole pads

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Name = Xml.Attributes("name").Value

            For Each XmlNode As XmlNode In Xml.ChildNodes()
                Select Case XmlNode.Name
                    Case "polygon"
                        m_GraphicElements.Add(New Polygon(New GraphicOwner(Me), XmlNode))
                    Case "wire"
                        m_GraphicElements.Add(New Wire(New GraphicOwner(Me), XmlNode))
                    Case "text"
                        Dim Text As text = New text(New GraphicOwner(Me), XmlNode)
                        m_GraphicElements.Add(Text)
                        Select Case Text.Type '<> ParameterType.PT_Text Then
                            Case ParameterType.PT_Name
                                m_NameParameters.Add(Text)
                            Case ParameterType.PT_Value
                                m_ValueParameters.Add(Text)
                        End Select
                    Case "pin"
                        Dim Pin As New Pin(New GraphicOwner(Me), XmlNode)
                        m_GraphicElements.Add(Pin)
                        m_Pins.Add(Pin)
                    Case "circle"
                        m_GraphicElements.Add(New Circle(New GraphicOwner(Me), XmlNode))
                    Case "rectangle"
                        m_GraphicElements.Add(New Eagle.Rectangle(New GraphicOwner(Me), XmlNode))
                    Case "frame"
                        m_GraphicElements.Add(New Frame(New GraphicOwner(Me), XmlNode))
                    Case "hole"
                        m_GraphicElements.Add(New Hole(New GraphicOwner(Me), XmlNode))
                    Case "pad"
                        Dim Pad As New Pad(New GraphicOwner(Me), XmlNode)
                        m_GraphicElements.Add(Pad)
                        m_Pads.Add(Pad)
                    Case "smd"
                        Dim SMD As New SMD(New GraphicOwner(Me), XmlNode)
                        m_GraphicElements.Add(SMD)
                        m_Pads.Add(SMD)
                End Select
            Next
            Dim DescriptionNode As XmlNode = Xml.SelectSingleNode("./description")
            If DescriptionNode IsNot Nothing Then m_Description = DescriptionNode.InnerText
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            For Each Graphic As Graphic In m_GraphicElements
                Graphic.Render(Graphics) 'draw all items in this symbol and return the bounding area rectangle
            Next
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Dim Region As New Region
            Region.MakeEmpty()
            For Each Graphic As Graphic In m_GraphicElements
                Region.Union(Graphic.GetRegion) 'draw all items in this symbol and return the bounding area rectangle
            Next
            Return Region
        End Function

        Public ReadOnly Property Name() As String
            Get
                Return m_Name
            End Get
        End Property

        Public ReadOnly Property GraphicElements() As List(Of Graphic)
            Get
                Return m_GraphicElements
            End Get
        End Property

        Public ReadOnly Property Library() As Library
            Get
                Return m_Owner.Library
            End Get
        End Property

        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Owner.Drawing
            End Get
        End Property

        ''' <summary>
        ''' Sets the >NAME or >VALUE parameters of this symbol, according to what has to be rendered
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="Value"></param>
        ''' <remarks></remarks>
        Public Sub SetParameter(ByVal type As ParameterType, ByVal Value As String)
            Dim Collection As List(Of text)
            Select Case type
                Case ParameterType.PT_Name
                    Collection = m_NameParameters
                Case ParameterType.PT_Value
                    Collection = m_ValueParameters
                Case Else
                    Return
            End Select
            For Each Text As text In Collection
                Text.Text = Value
            Next
        End Sub

        ''' <summary>
        ''' Returns number of >NAME text elements in this symbol
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property NameCount() As Integer
            Get
                Return m_NameParameters.Count
            End Get
        End Property

        ''' <summary>
        ''' Returns number of >VALUE text elements
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property ValueCount() As Integer
            Get
                Return m_ValueParameters.Count
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
            Node.AppendChild(Doc.CreateElement("description")).AppendChild(Doc.CreateTextNode(m_Description))
            For Each GraphicElement As Graphic In m_GraphicElements
                Dim SaveableGraphicElement As EagleSaveable = Nothing
                Try
                    SaveableGraphicElement = CType(GraphicElement, EagleSaveable)
                Catch CastExp As InvalidCastException

                End Try
                If SaveableGraphicElement IsNot Nothing Then
                    SaveableGraphicElement.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(SaveableGraphicElement.NodeName)))
                End If
            Next
        End Sub

        Public Overrides Property Selected() As Boolean
            Get
                Return MyBase.Selected
            End Get
            Set(ByVal value As Boolean)
                If value <> m_Selected Then
                    For Each GraphicElement As Graphic In m_GraphicElements
                        GraphicElement.Selected = value
                    Next
                    MyBase.Selected = value
                End If
            End Set
        End Property

        Public Overridable ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return ""
            End Get
        End Property

        Public Overrides Function Clone() As Object
            Dim Cloned As GraphicSymbol = DirectCast(MyBase.Clone(), GraphicSymbol)
            Cloned.m_GraphicElements = New List(Of Graphic)
            Cloned.m_NameParameters = New List(Of text)
            Cloned.m_ValueParameters = New List(Of text)
            Cloned.m_Pins = New List(Of Pin)
            Cloned.m_Pads = New List(Of EaglePad)
            For Each GraphicElement As Graphic In m_GraphicElements
                Dim ClonedGraphicElement As Graphic = GraphicElement.Clone()
                Cloned.m_GraphicElements.Add(ClonedGraphicElement)
                ClonedGraphicElement.Owner = New GraphicOwner(Cloned)
                Select Case ClonedGraphicElement.GetType.Name
                    Case GetType(text).Name ' "text"
                        Select Case DirectCast(ClonedGraphicElement, text).Type
                            Case ParameterType.PT_Name
                                Cloned.m_NameParameters.Add(ClonedGraphicElement)
                            Case ParameterType.PT_Value
                                Cloned.m_ValueParameters.Add(ClonedGraphicElement)
                        End Select
                    Case GetType(Pin).Name ' "pin"
                        Cloned.m_Pins.Add(ClonedGraphicElement)
                    Case GetType(Pad).Name  '"pad"
                        Cloned.m_Pads.Add(ClonedGraphicElement)
                    Case GetType(SMD).Name ' "smd"
                        Cloned.m_Pads.Add(ClonedGraphicElement)
                End Select
            Next
            Return Cloned
        End Function

    End Class

    ''' <summary>
    ''' Symbol
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Symbol
        Inherits GraphicSymbol

        Public Sub New(ByVal Library As Library, ByVal Xml As XmlNode)
            MyBase.New(New GraphicOwner(Library), Xml)
        End Sub

        Public ReadOnly Property Pins() As List(Of Pin)
            Get
                Return m_Pins
            End Get
        End Property

        Public Function getPin(ByVal Name As String) As Pin
            For Each Pin As Pin In m_Pins
                If Pin.Name = Name Then
                    Return Pin
                End If
            Next
            Return Nothing
        End Function

        Public Overrides ReadOnly Property NodeName() As String
            Get
                Return "symbol"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Package of device (is a renderable graphicsymbol)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Package
        Inherits GraphicSymbol

        Public Sub New(ByVal Library As Library, ByVal Xml As XmlNode)
            MyBase.New(New GraphicOwner(Library), Xml)
        End Sub

        Public ReadOnly Property GetPads() As List(Of EaglePad)
            Get
                Return m_Pads
            End Get
        End Property

        Public ReadOnly Property GetPad(ByVal Name As String) As EaglePad
            Get
                For Each Pad As EaglePad In m_Pads
                    If Pad.Name = Name Then
                        Return Pad
                    End If
                Next
                Return Nothing
            End Get
        End Property

        Public Overrides ReadOnly Property NodeName() As String
            Get
                Return "package"
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Tells how a gate / symbol pins are connected to the pads on the PCB
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PinConnection
        Implements EagleSaveable

        Dim m_GateName As String
        Dim m_PinName As String
        Dim m_PadName As String
        Dim m_Device As Device
        Dim m_PadNames() As String

        Public Sub New(ByVal Device As Device, ByVal Xml As XmlNode)
            m_Device = Device
            m_PinName = Xml.Attributes("pin").Value
            m_PadName = Xml.Attributes("pad").Value
            m_GateName = Xml.Attributes("gate").Value
            m_PadNames = Split(m_PadName, " ")
        End Sub

        ''' <summary>
        ''' Returns the name of the Gate
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GateName() As String
            Get
                Return m_GateName
            End Get
        End Property

        ''' <summary>
        ''' Returns the name of the pin on the Gate that must be connected
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PinName() As String
            Get
                Return m_PinName
            End Get
        End Property

        ''' <summary>
        ''' Returns the pad name(s) the Pin of the Gate is connected to
        ''' In case multiple pads are connected to this pin they will be seperated by a space
        ''' Use PadNames() to get an array of all pads connected to this pin
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PadName() As String
            Get
                Return m_PadName
            End Get
        End Property

        ''' <summary>
        ''' Returns all pad names of the package that are connected to this pin/gate combination
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PadNames() As String()
            Get
                Return m_PadNames
            End Get
        End Property

        Public ReadOnly Property PadCount() As Integer
            Get
                Return m_PadNames.Length
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Device() As Device
            Get
                Return m_Device
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "connect"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("pin")).Value = m_PinName
                .Append(Doc.CreateAttribute("pad")).Value = m_PadName
                .Append(Doc.CreateAttribute("gate")).Value = m_GateName
            End With
        End Sub
    End Class

    Public Class Technology
        Implements EagleSaveable
        Dim m_Name As String

        Public Sub New(ByVal Device As Device, ByVal Xml As XmlNode)
            m_Name = Xml.Attributes("name").Value
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "technology"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
        End Sub
    End Class

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Device
        Implements EagleSaveable

        Dim m_DeviceSet As DeviceSet
        Dim m_Connects As New List(Of PinConnection)
        Dim m_Technologies As New List(Of Technology)
        Dim m_PadNames As New Dictionary(Of String, Dictionary(Of String, String)) 'padnames(GateName)(PinName) = padname for that gate and pin
        Dim m_Name As String
        Dim m_PackageName As String
        Dim m_SchematicName As String 'device name on schematic
        Dim m_SchematicValue As String 'device value on schematic

        Public Sub New(ByVal DeviceSet As DeviceSet, ByVal Xml As XmlNode)
            m_DeviceSet = DeviceSet
            If Xml.Attributes("name") IsNot Nothing Then m_Name = Xml.Attributes("name").Value
            If Xml.Attributes("package") IsNot Nothing Then m_PackageName = Xml.Attributes("package").Value
            Dim ConnectionNodes As XmlNodeList = Xml.SelectNodes("./connects/connect")
            For Each ConnectionNode As XmlNode In ConnectionNodes
                Dim PinConnection As New PinConnection(Me, ConnectionNode)
                m_Connects.Add(PinConnection)
                If Not m_PadNames.ContainsKey(PinConnection.GateName) Then
                    m_PadNames.Add(PinConnection.GateName, New Dictionary(Of String, String))
                End If
                If Not m_PadNames(PinConnection.GateName).ContainsKey(PinConnection.PinName) Then
                    m_PadNames(PinConnection.GateName).Add(PinConnection.PinName, PinConnection.PadName)
                Else
                    m_PadNames(PinConnection.GateName)(PinConnection.PinName) &= ", " & PinConnection.PadName
                End If
            Next

            Dim Technologies As XmlNodeList = Xml.SelectNodes("./technologies/technology")
            For Each TechnologyNode As XmlNode In Technologies
                m_Technologies.Add(New Technology(Me, TechnologyNode))
            Next

        End Sub

        ''' <summary>
        ''' Returns all the connections for the device
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PinConnections() As List(Of PinConnection)
            Get
                Return m_Connects
            End Get
        End Property

        ''' <summary>
        ''' Returns the Pinconnection for a gate and pinname or nothing
        ''' </summary>
        ''' <param name="GateName"></param>
        ''' <param name="PinName"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetPinConnection(ByVal GateName As String, ByVal PinName As String) As PinConnection
            Get
                For Each PinConnection As PinConnection In m_Connects
                    If PinConnection.GateName = GateName AndAlso PinConnection.PinName = PinName Then
                        Return PinConnection
                    End If
                Next
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name() As String
            Get
                Return m_Name
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PackageName() As String
            Get
                Return m_PackageName
            End Get
        End Property

        ''' <summary>
        ''' Returns the name of the pad that is connected to the PinName (on symbol) of the given gate in this device
        ''' </summary>
        ''' <param name="PinName"></param>
        ''' <param name="GateName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetPadName(ByVal GateName As String, ByVal PinName As String) As String
            If m_PadNames.ContainsKey(GateName) Then
                If m_PadNames(GateName).ContainsKey(PinName) Then
                    Return m_PadNames(GateName)(PinName)
                End If
            End If
            'For Each PinConnection As PinConnection In m_Connects
            '    If PinConnection.PinName = PinName Then
            '        Return PinConnection.PadName
            '    End If
            'Next
            Return ""
        End Function

        Public ReadOnly Property DeviceSet() As DeviceSet
            Get
                Return m_DeviceSet
            End Get
        End Property

        ''' <summary>
        ''' Gets / sets the name of the device when rendered in the schematic
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property SchematicName() As String
            Get
                Return m_SchematicName
            End Get
            Set(ByVal value As String)
                m_SchematicName = value
            End Set
        End Property

        ''' <summary>
        ''' Gets / sets the value of the device when rendered in the schematic
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property SchematicValue() As String
            Get
                Return m_SchematicValue
            End Get
            Set(ByVal value As String)
                m_SchematicValue = value
            End Set
        End Property

        Public Sub RenderSymbol(ByVal Graphics As Graphics)
            Dim GateCount As Integer = m_DeviceSet.Gates.Count
            For Each Gate As Gate In m_DeviceSet.Gates
                Dim Container As GraphicsContainer = Graphics.BeginContainer
                Dim Symbol As Symbol = m_DeviceSet.Library.Symbols(Gate.SymbolName)
                If m_SchematicName = "" Then
                    Symbol.SetParameter(ParameterType.PT_Name, Gate.Name)
                Else
                    If GateCount = 1 Then
                        Symbol.SetParameter(ParameterType.PT_Name, m_SchematicName)
                    Else
                        Symbol.SetParameter(ParameterType.PT_Name, m_SchematicName & Gate.Name)
                    End If
                End If
                If m_SchematicValue <> "" Then Symbol.SetParameter(ParameterType.PT_Value, m_SchematicValue)
                Graphics.TranslateTransform(Gate.Location.X, Gate.Location.Y)
                For Each Pin As Pin In Symbol.Pins
                    Pin.PadName = m_PadNames(Gate.Name)(Pin.Name)
                Next
                Symbol.Render(Graphics)
                Graphics.EndContainer(Container)
            Next

        End Sub

        Public Function GetSymbolRegion() As Region
            Dim Region As New Region()
            Region.MakeEmpty()
            For Each gate As Gate In m_DeviceSet.Gates
                Dim SymbolRegion As Region = m_DeviceSet.Library.Symbols(gate.SymbolName).GetRegion().Clone()
                SymbolRegion.Translate(gate.Location.X, gate.Location.Y)
                Region.Union(SymbolRegion)
            Next
            Return Region
        End Function

        Public ReadOnly Property Technologies() As List(Of Technology)
            Get
                Return m_Technologies
            End Get
        End Property

        Public Function GetTechnology(ByVal Name As String) As Technology
            For Each Technology As Technology In m_Technologies
                If Technology.Name = Name Then
                    Return Technology
                End If
            Next
            Return Nothing
        End Function

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "device"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("package")).Value = m_PackageName
            End With
            Dim ConnectNodes As XmlNode = Node.AppendChild(Doc.CreateElement("connects"))
            For Each PinConnection As PinConnection In m_Connects
                PinConnection.WriteXml(Doc, ConnectNodes.AppendChild(Doc.CreateElement(PinConnection.NodeName())))
            Next
            Dim TechnologyNodes As XmlNode = Node.AppendChild(Doc.CreateElement("technologies"))
            For Each Technology As Technology In m_Technologies
                Technology.WriteXml(Doc, TechnologyNodes.AppendChild(Doc.CreateElement(Technology.NodeName())))
            Next
        End Sub
    End Class

    ''' <summary>
    ''' Represents a symbol in a device and how that is named (not the actual symbol graphics)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Gate
        Implements EagleSaveable

        Dim m_DeviceSet As DeviceSet
        Dim m_Name As String
        Dim m_SymbolName As String
        Dim m_Location As New PointF
        Dim m_Symbol As Symbol

        Public Sub New(ByVal DeviceSet As DeviceSet, ByVal Xml As XmlNode)
            m_DeviceSet = DeviceSet
            m_Name = Xml.Attributes("name").Value
            m_SymbolName = Xml.Attributes("symbol").Value
            m_Location.X = toSingle(Xml.Attributes("x").Value)
            m_Location.Y = toSingle(Xml.Attributes("y").Value)
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property SymbolName() As String
            Get
                Return m_SymbolName
            End Get
            Set(ByVal value As String)
                m_SymbolName = value
            End Set
        End Property

        Public Function GetSymbol() As Symbol
            If m_Symbol Is Nothing Then
                m_Symbol = m_DeviceSet.Library.Symbols(m_SymbolName)
            End If
            Return m_Symbol
        End Function

        Public ReadOnly Property Location() As PointF
            Get
                Return m_Location
            End Get
        End Property

        Public ReadOnly Property DeviceSet() As DeviceSet
            Get
                Return m_DeviceSet
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "gate"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("symbol")).Value = m_SymbolName
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
            End With
        End Sub
    End Class

    ''' <summary>
    ''' DeviceSet has a list of gates which represent the Symbols on the schematic
    ''' and a list of devices which represent a different package for each deviceset and how this is connected with each pin/gate
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DeviceSet
        Implements EagleSaveable

        Dim m_Devices As New List(Of Device)
        Dim m_Gates As New List(Of Gate)
        Dim m_Name As String
        Dim m_Library As Library
        Dim m_Prefix As String
        Dim m_Description As String
        Dim m_UserValue As Boolean

        Public Sub New(ByVal Library As Library, ByVal Xml As XmlNode)
            Dim DeviceNodes As XmlNodeList = Xml.SelectNodes("./devices/device")
            m_Library = Library
            For Each DeviceNode As XmlNode In DeviceNodes
                m_Devices.Add(New Device(Me, DeviceNode))
            Next
            Dim GateNodes As XmlNodeList = Xml.SelectNodes("./gates/gate")
            For Each GateNode As XmlNode In GateNodes
                m_Gates.Add(New Gate(Me, GateNode))
            Next
            If Xml.Attributes("prefix") IsNot Nothing Then m_Prefix = Xml.Attributes("prefix").Value
            If Xml.SelectSingleNode("description") IsNot Nothing Then
                m_Description = Xml.SelectSingleNode("description").InnerText
            End If
            If Xml.Attributes("uservalue") IsNot Nothing Then m_UserValue = Functions.ParseBool(Xml.Attributes("uservalue").Value)
            m_Name = Xml.Attributes("name").Value
        End Sub

        Public ReadOnly Property Devices() As List(Of Device)
            Get
                Return m_Devices
            End Get
        End Property

        Public ReadOnly Property Gates() As List(Of Gate)
            Get
                Return m_Gates
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                Return m_Name
            End Get
        End Property

        Public ReadOnly Property Library() As Library
            Get
                Return m_Library
            End Get
        End Property

        Public Property Prefix() As String
            Get
                Return m_Prefix
            End Get
            Set(ByVal value As String)
                m_Prefix = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the gate of this device set with Name
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetGate(ByVal Name As String) As Gate
            For Each Gate As Gate In m_Gates
                If Gate.Name = Name Then
                    Return Gate
                End If
            Next
            Return Nothing
        End Function

        Public Function GetDevice(ByVal Name As String) As Device
            For Each Device As Device In m_Devices
                If Device.Name = Name Then
                    Return Device
                End If
            Next
            Return Nothing
        End Function

        Public Property Description() As String
            Get
                Return m_Description
            End Get
            Set(ByVal value As String)
                m_Description = value
            End Set
        End Property

        ''' <summary>
        ''' Gets / Sets if the user can modify the >VALUE text by default
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property UserValue() As Boolean
            Get
                Return m_UserValue
            End Get
            Set(ByVal value As Boolean)
                m_UserValue = value
            End Set
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "deviceset"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            If m_Prefix <> "" Then Node.Attributes.Append(Doc.CreateAttribute("prefix")).Value = m_Prefix
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
            Dim DeviceNodes As XmlNode = Node.AppendChild(Doc.CreateElement("devices"))
            For Each Device As Device In m_Devices
                Device.WriteXml(Doc, DeviceNodes.AppendChild(Doc.CreateElement(Device.NodeName())))
            Next
            Dim GateNodes As XmlNode = Node.AppendChild(Doc.CreateElement("gates"))
            For Each Gate As Gate In m_Gates
                Gate.WriteXml(Doc, GateNodes.AppendChild(Doc.CreateElement(Gate.NodeName())))
            Next
            Node.AppendChild(Doc.CreateElement("description")).AppendChild(Doc.CreateTextNode(m_Description))
            If m_UserValue Then
                Node.Attributes.Append(Doc.CreateAttribute("uservalue")).Value = Functions.ToEagleBool(m_UserValue)
            End If
        End Sub
    End Class

    Public Class Library
        Implements EagleSaveable

        Dim m_DeviceSets As New List(Of DeviceSet)
        Dim m_Name As String
        Dim m_NameAttribute As String
        Dim m_Description As String
        Dim m_Symbols As New Dictionary(Of String, Symbol)
        Dim m_Packages As New Dictionary(Of String, Package)
        Dim m_Drawing As Drawing
        Dim m_Node As XmlNode

        Public Sub New(ByVal Drawing As Drawing, ByVal Xml As XmlNode)
            m_Drawing = Drawing
            m_Node = Xml
            If Xml.Attributes("name") IsNot Nothing Then
                m_Name = Xml.Attributes("name").Value
                m_NameAttribute = m_Name
            Else
                m_Name = System.IO.Path.GetFileNameWithoutExtension(Drawing.Project.ShortFileName)
            End If

            Dim DeviceSetNodes As XmlNodeList = Xml.SelectNodes("./devicesets/deviceset")
            For Each DeviceSetNode As XmlNode In DeviceSetNodes
                m_DeviceSets.Add(New DeviceSet(Me, DeviceSetNode))
            Next
            Dim SymbolNodes As XmlNodeList = Xml.SelectNodes("./symbols/symbol")
            For Each SymbolNode As XmlNode In SymbolNodes
                Dim Symbol = New Symbol(Me, SymbolNode)
                m_Symbols.Add(Symbol.name(), Symbol)
            Next
            Dim PackageNodes As XmlNodeList = Xml.SelectNodes("./packages/package")
            For Each PackageNode As XmlNode In PackageNodes
                Dim Package = New Package(Me, PackageNode)
                m_Packages.Add(Package.name(), Package)
            Next
            If Xml.SelectSingleNode("./description") IsNot Nothing Then m_Description = Xml.SelectSingleNode("./description").Value
        End Sub

        Public Function DevicesSets() As List(Of DeviceSet)
            Return m_DeviceSets
        End Function

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
                m_NameAttribute = value
            End Set
        End Property

        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Drawing
            End Get
        End Property

        Public ReadOnly Property Symbols() As Dictionary(Of String, Symbol)
            Get
                Return m_Symbols
            End Get
        End Property

        Public ReadOnly Property Packages() As Dictionary(Of String, Package)
            Get
                Return m_Packages
            End Get
        End Property

        Public Function GetSymbol(ByVal Name As String) As Symbol
            Return m_Symbols(Name)
        End Function

        Public Function GetPackage(ByVal Name As String) As Package
            Return m_Packages(Name)
        End Function

        Public Function GetDeviceSet(ByVal Name As String) As DeviceSet
            For Each DeviceSet As DeviceSet In m_DeviceSets
                If DeviceSet.Name = Name Then
                    Return DeviceSet
                End If
            Next
            Return Nothing
        End Function

        Public Function GetDevice(ByVal DeviceSetName As String, ByVal Name As String) As Device
            Dim DeviceSet As DeviceSet = GetDeviceSet(DeviceSetName)
            If DeviceSet IsNot Nothing Then
                Return DeviceSet.GetDevice(Name)
            End If
            Return Nothing
        End Function

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "library"
            End Get
        End Property

        Public ReadOnly Property Node() As XmlNode
            Get
                Return m_Node
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            If m_NameAttribute <> "" Then Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_NameAttribute
            Node.AppendChild(Doc.CreateElement("description")).AppendChild(Doc.CreateTextNode(m_Description))
            Dim DeviceSetNodes As XmlNode = Node.AppendChild(Doc.CreateElement("devicesets"))
            For Each DeviceSet As DeviceSet In m_DeviceSets
                DeviceSet.WriteXml(Doc, DeviceSetNodes.AppendChild(Doc.CreateElement(DeviceSet.NodeName())))
            Next
            Dim SymbolNodes As XmlNode = Node.AppendChild(Doc.CreateElement("symbols"))
            For Each Symbol As KeyValuePair(Of String, Symbol) In m_Symbols
                Symbol.Value.WriteXml(Doc, SymbolNodes.AppendChild(Doc.CreateElement(Symbol.Value.NodeName())))
            Next
            Dim PackageNodes As XmlNode = Node.AppendChild(Doc.CreateElement("packages"))
            For Each package As KeyValuePair(Of String, Package) In m_Packages
                package.Value.WriteXml(Doc, PackageNodes.AppendChild(Doc.CreateElement(package.Value.NodeName())))
            Next
        End Sub
    End Class

    Public Class Clearance
        Implements EagleSaveable

        Dim m_Class As Integer
        Dim m_Value As Single
        Dim m_NetClass As NetClass

        Public Sub New(ByVal NetClass As NetClass, ByVal Xml As XmlNode)
            m_Class = Xml.Attributes("class").Value
            If Xml.Attributes("value") IsNot Nothing Then m_Value = toSingle(Xml.Attributes("value").Value)
            m_NetClass = NetClass
        End Sub

        Public Property ClassNr() As Integer
            Get
                Return m_Class
            End Get
            Set(ByVal value As Integer)
                m_Class = value
            End Set
        End Property

        Public ReadOnly Property NetClass() As NetClass
            Get
                Return m_NetClass
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "clearance"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("class")).Value = m_Class
                .Append(Doc.CreateAttribute("value")).Value = ToEagleSingle(m_Value)
            End With
        End Sub
    End Class

    Public Class NetClass
        Implements EagleSaveable

        Dim m_Number As Integer
        Dim m_Clearance As New List(Of Clearance)
        Dim m_Name As String
        Dim m_Width As Single
        Dim m_Drill As Single
        Dim m_Schematic As Schematic

        Public Sub New(ByVal Schematic As Schematic)
            Me.New(Schematic, "default", 0, 0, 0)
        End Sub

        Public Sub New(ByVal Schematic As Schematic, ByVal Name As String, ByVal Number As Integer, ByVal Width As Single, ByVal Drill As Single)
            m_Schematic = Schematic
            m_Name = Name
            m_Width = Width
            m_Number = Number
            m_Drill = Drill
        End Sub

        Public Sub New(ByVal Schematic As Schematic, ByVal Xml As XmlNode)
            m_Number = Xml.Attributes("number").Value
            m_Name = Xml.Attributes("name").Value

            If Xml.Attributes("width") IsNot Nothing Then m_Width = toSingle(Xml.Attributes("width").Value)
            If Xml.Attributes("drill") IsNot Nothing Then m_Drill = toSingle(Xml.Attributes("drill").Value)
            Dim ClearanceNodes As XmlNodeList = Xml.SelectNodes("./clearance")

            For Each ClearanceNode As XmlNode In ClearanceNodes
                m_Clearance.Add(New Clearance(Me, ClearanceNode))
            Next
            m_Schematic = Schematic
        End Sub

        Public Property Number() As Integer
            Get
                Return m_Number
            End Get
            Set(ByVal value As Integer)
                m_Number = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Width() As Single
            Get
                Return m_Width
            End Get
            Set(ByVal value As Single)
                m_Width = value
            End Set
        End Property

        Public Property Drill() As Single
            Get
                Return Drill
            End Get
            Set(ByVal value As Single)
                m_Drill = value
            End Set
        End Property

        Public ReadOnly Property Clearances() As List(Of Clearance)
            Get
                Return m_Clearance
            End Get
        End Property

        Public ReadOnly Property Schematic() As Schematic
            Get
                Return m_Schematic
            End Get
        End Property


        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "class"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("number")).Value = m_Number
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("width")).Value = ToEagleSingle(m_Width)
                .Append(Doc.CreateAttribute("drill")).Value = ToEagleSingle(m_Drill)
            End With
            For Each Clearance As Clearance In m_Clearance
                Clearance.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Clearance.NodeName())))
            Next
        End Sub
    End Class

    Public Class Attribute
        Implements EagleSaveable

        Dim m_Name As String
        Dim m_Value As String
        Dim m_Location As PointF
        Dim m_Size As Single
        Dim m_Layer As Integer
        Dim m_Font As String
        Dim m_Ratio As Integer
        Dim m_Rotation As New Rotation
        Dim m_Display As AttributeDisplay
        Dim m_Constant As Boolean
        Dim m_Owner As Object

        Public Sub New(ByVal Owner As Object, ByVal Xml As XmlNode)
            m_Name = Xml.Attributes("name").Value
            m_Value = Xml.Attributes("value").Value
            m_Location = New PointF(toSingle(Xml.Attributes("x").Value), toSingle(Xml.Attributes("y").Value))
            m_Size = toSingle(Xml.Attributes("size").Value)
            m_Layer = Xml.Attributes("layer").Value
            m_Font = Xml.Attributes("font").Value
            m_Ratio = Xml.Attributes("ratio").Value
            m_Rotation = New Rotation(Xml.Attributes("rot").Value) '.Replace("M", "").Replace("S", "").Replace("R", "")
            m_Display = [Enum].Parse(GetType(AttributeDisplay), "AD_" & Xml.Attributes("display").Value)
            m_Constant = ParseBool(Xml.Attributes("constant").Value)
            m_Owner = Owner
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Value() As String
            Get
                Return m_Value
            End Get
            Set(ByVal value As String)
                m_Value = value
            End Set
        End Property

        Public Property Location() As PointF
            Get
                Return m_Location
            End Get
            Set(ByVal value As PointF)
                m_Location = value
            End Set
        End Property

        Public Property Size() As Single
            Get
                Return m_Size
            End Get
            Set(ByVal value As Single)
                m_Size = value
            End Set
        End Property

        Public Property Layer() As Integer
            Get
                Return m_Layer
            End Get
            Set(ByVal value As Integer)
                m_Layer = value
            End Set
        End Property

        Public Property Font() As String
            Get
                Return m_Font
            End Get
            Set(ByVal value As String)
                m_Font = value
            End Set
        End Property

        Public Property Rotation() As Single
            Get
                Return m_Rotation.Rotation
            End Get
            Set(ByVal value As Single)
                m_Rotation.Rotation = value
            End Set
        End Property

        Public Property Display() As AttributeDisplay
            Get
                Return m_Display
            End Get
            Set(ByVal value As AttributeDisplay)
                m_Display = value
            End Set
        End Property

        Public Property Constant() As Boolean
            Get
                Return m_Constant
            End Get
            Set(ByVal value As Boolean)
                m_Constant = value
            End Set
        End Property

        Public ReadOnly Property Owner() As Object
            Get
                Return m_Owner
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("value")).Value = m_Value
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("size")).Value = ToEagleSingle(m_Size)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("font")).Value = m_Font
                .Append(Doc.CreateAttribute("ratio")).Value = m_Ratio
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("display")).Value = Mid$([Enum].GetName(GetType(AttributeDisplay), m_Display), 4)
                .Append(Doc.CreateAttribute("constant")).Value = ToEagleBool(m_Constant)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "attribute"
            End Get
        End Property
    End Class


    Public Class VariantDef
        Implements EagleSaveable

        Dim m_Name As String
        Dim m_Current As Boolean
        Dim m_Schematic As Schematic

        Public Sub New(ByVal Schematic As Schematic, ByVal Xml As XmlNode)
            m_Name = Xml.Attributes("name").Value
            If Xml.Attributes("current") IsNot Nothing Then m_Current = ParseBool(Xml.Attributes("current").Value)
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Current() As Boolean
            Get
                Return Current
            End Get
            Set(ByVal value As Boolean)
                m_Current = value
            End Set
        End Property

        Public ReadOnly Property Schematic() As Schematic
            Get
                Return m_Schematic
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
            Node.Attributes.Append(Doc.CreateAttribute("current")).Value = ToEagleBool(m_Current)
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "variantdef"
            End Get
        End Property
    End Class

    Public Class EagleVariant
        Implements EagleSaveable

        Dim m_Name As String
        Dim m_Populate As Boolean
        Dim m_Value As String
        Dim m_Technology As String

        Public Sub New(ByVal Part As Part, ByVal Xml As XmlNode)
            m_Name = Xml.Attributes("name").Value
            If Xml.Attributes("populate") IsNot Nothing Then m_Populate = Xml.Attributes("populate").Value
            m_Value = Xml.Attributes("value").Value
            m_Technology = Xml.Attributes("technology").Value
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Populate() As Boolean
            Get
                Return m_Populate
            End Get
            Set(ByVal value As Boolean)
                m_Populate = value
            End Set
        End Property

        Public Property Value() As String
            Get
                Return m_Value
            End Get
            Set(ByVal value As String)
                m_Value = value
            End Set
        End Property

        Public Property Technology() As String
            Get
                Return m_Technology
            End Get
            Set(ByVal value As String)
                m_Technology = value
            End Set
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
            Node.Attributes.Append(Doc.CreateAttribute("populate")).Value = ToEagleBool(m_Populate)
            Node.Attributes.Append(Doc.CreateAttribute("value")).Value = m_Value
            Node.Attributes.Append(Doc.CreateAttribute("technology")).Value = m_Technology
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "variant"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Plain class can render schematic
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Plain
        Inherits Graphic
        Implements EagleSaveable

        Dim m_GraphicElements As List(Of Graphic)
        Dim m_Sheet As Sheet

        Public Sub New(ByVal Sheet As Sheet, ByVal Xml As XmlNode)
            MyBase.New(New GraphicOwner(Sheet))
            m_Sheet = Sheet
        End Sub

        Private Sub Init(ByVal Xml As XmlNode)
            For Each XmlNode As XmlNode In Xml.ChildNodes()
                Select Case XmlNode.Name
                    Case "polygon"
                        m_GraphicElements.Add(New Polygon(New GraphicOwner(Me), XmlNode))
                    Case "wire"
                        m_GraphicElements.Add(New Wire(New GraphicOwner(Me), XmlNode))
                    Case "text"
                        Dim Text As text = New text(New GraphicOwner(Me), XmlNode)
                        m_GraphicElements.Add(Text)
                    Case "circle"
                        m_GraphicElements.Add(New Circle(New GraphicOwner(Me), XmlNode))
                    Case "rectangle"
                        m_GraphicElements.Add(New Eagle.Rectangle(New GraphicOwner(Me), XmlNode))
                    Case "frame"
                        m_GraphicElements.Add(New Frame(New GraphicOwner(Me), XmlNode))
                    Case "hole"
                        m_GraphicElements.Add(New Hole(New GraphicOwner(Me), XmlNode))
                    Case "dimension" 'to be defined

                End Select
            Next
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
            For Each Graphic As Graphic In m_GraphicElements
                Graphic.Render(Graphics) 'draw all items in this symbol and return the bounding area rectangle
            Next
        End Sub

        Public Overrides Function GetRegion() As Region
            MyBase.GetRegion()
            Dim Region As New Region
            Region.MakeEmpty()
            For Each Graphic As Graphic In m_GraphicElements
                Region.Union(Graphic.GetRegion) 'draw all items in this symbol and return the bounding area rectangle
            Next
            Return Region
        End Function

        Public ReadOnly Property Sheet() As Sheet
            Get
                Return m_Sheet
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            For Each GraphicElement As Graphic In m_GraphicElements
                Dim SaveableGraphicElement As EagleSaveable = Nothing
                Try
                    SaveableGraphicElement = CType(GraphicElement, EagleSaveable)
                Catch ex As InvalidCastException

                End Try
                If SaveableGraphicElement IsNot Nothing Then
                    SaveableGraphicElement.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(SaveableGraphicElement.NodeName())))
                End If
            Next
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "plain"
            End Get
        End Property
    End Class

    Public Class Instance
        Implements EagleSaveable

        Dim m_Attributes As New List(Of Attribute)
        Dim m_Part As String
        Dim m_Gate As String
        Dim m_Location As PointF
        Dim m_Smashed As Boolean
        Dim m_Rotation As Rotation
        Dim m_Sheet As Sheet

        Dim m_Symbol As Symbol

        Public Sub New(ByVal Sheet As Sheet, ByVal Part As String, ByVal Gate As String, ByVal Location As PointF)
            m_Sheet = Sheet
            m_Gate = Gate
            m_Part = Part
            m_Location = Location
            m_Rotation = New Rotation
        End Sub

        Public Sub New(ByVal Sheet As Sheet, ByVal Xml As XmlNode)
            m_Sheet = Sheet
            m_Part = Xml.Attributes("part").Value
            m_Gate = Xml.Attributes("gate").Value
            m_Location = New PointF(toSingle(Xml.Attributes("x").Value), toSingle(Xml.Attributes("y").Value))
            If Xml.Attributes("smashed") IsNot Nothing Then m_Smashed = ParseBool(Xml.Attributes("smashed").Value)
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value) '.Replace("M", "").Replace("S", "").Replace("R", "")

            Dim AttributeNodes As XmlNodeList = Xml.SelectNodes("./attribute")
            For Each AttributeNode As XmlNode In AttributeNodes
                m_Attributes.Add(New Attribute(Me, Xml))
            Next
        End Sub

        Public Property Part() As String
            Get
                Return m_Part
            End Get
            Set(ByVal value As String)
                m_Part = value
            End Set
        End Property

        Public Property Gate() As String
            Get
                Return m_Gate
            End Get
            Set(ByVal value As String)
                m_Gate = value
            End Set
        End Property

        Public ReadOnly Property Location() As PointF
            Get
                Return m_Location
            End Get
        End Property

        Public Property Smashed() As Boolean
            Get
                Return m_Smashed
            End Get
            Set(ByVal value As Boolean)
                m_Smashed = value
            End Set
        End Property

        Public Property Rotation() As Single
            Get
                Return m_Rotation.Rotation
            End Get
            Set(ByVal value As Single)
                m_Rotation.Rotation = value
            End Set
        End Property

        Public ReadOnly Property Sheet() As Sheet
            Get
                Return m_Sheet
            End Get
        End Property

        Public Function GetSymbol() As Symbol
            If m_Symbol Is Nothing Then
                m_Symbol = m_Sheet.Schematic.GetPart(m_Part).GetSymbol(m_Gate)
            End If
            Return m_Symbol
        End Function

        ''' <summary>
        ''' Returns the location of a pin based on the instance location
        ''' Returns the location where the wire should be connected
        ''' </summary>
        ''' <param name="PinName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetPinLocation(ByVal PinName As String) As PointF
            Dim Symbol As Symbol = GetSymbol()
            Dim Location As PointF
            If m_Rotation.Rotation <> 0 Then
                Location = RotateLocation(Symbol.getPin(PinName).GetWireLocation(), m_Rotation.RotationRadians)
                Location.X = Round(Location.X, 5)
                Location.Y = Round(Location.Y, 5)
            Else
                Location = Symbol.getPin(PinName).GetWireLocation()
            End If
            Return New PointF(m_Location.X + Location.X, m_Location.Y + Location.Y)
        End Function


        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("part")).Value = m_Part
                .Append(Doc.CreateAttribute("gate")).Value = m_Gate
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("smashed")).Value = ToEagleBool(m_Smashed)
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
            End With
            For Each Attribute As Attribute In m_Attributes
                Dim AttributeNode = Node.AppendChild(Doc.CreateElement("attribute"))
                Attribute.WriteXml(Doc, AttributeNode)
            Next
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "instance"
            End Get
        End Property
    End Class

    Public Class PinRef
        Implements EagleSaveable

        Dim m_Part As String
        Dim m_Gate As String
        Dim m_Pin As String
        Dim m_Segment As Segment

        Public Sub New(ByVal Segment As Segment, ByVal Part As String, ByVal Gate As String, ByVal Pin As String)
            m_Segment = Segment
            m_Gate = Gate
            m_Part = Part
            m_Pin = Pin
        End Sub

        Public Sub New(ByVal Segment As Segment, ByVal Xml As XmlNode)
            m_Segment = Segment
            m_Part = Xml.Attributes("part").Value
            m_Gate = Xml.Attributes("gate").Value
            m_Pin = Xml.Attributes("pin").Value
        End Sub

        Public Property Part() As String
            Get
                Return m_Part
            End Get
            Set(ByVal value As String)
                m_Part = value
            End Set
        End Property

        Public Property Gate() As String
            Get
                Return m_Gate
            End Get
            Set(ByVal value As String)
                m_Gate = value
            End Set
        End Property

        Public Property Pin() As String
            Get
                Return m_Pin
            End Get
            Set(ByVal value As String)
                m_Pin = value
            End Set
        End Property

        Public ReadOnly Property Segment() As Segment
            Get
                Return m_Segment
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "pinref"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("part")).Value = m_Part
                .Append(Doc.CreateAttribute("gate")).Value = m_Gate
                .Append(Doc.CreateAttribute("pin")).Value = m_Pin
            End With
        End Sub
    End Class

    Public Class Junction
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Location As PointF

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Location As PointF)
            MyBase.New(Owner)
            m_Location = Location
        End Sub

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Location = New PointF(toSingle(Xml.Attributes("x").Value), toSingle(Xml.Attributes("y").Value))
        End Sub

        Public ReadOnly Property Location() As PointF
            Get
                Return m_Location
            End Get
        End Property

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            Return MyBase.GetRegion()
        End Function

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "junction"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
            End With
        End Sub
    End Class

    Public Class Label
        Inherits Graphic
        Implements EagleSaveable

        Dim m_Location As PointF
        Dim m_Size As Single
        Dim m_Layer As Integer
        Dim m_Font As TextFont
        Dim m_Ratio As Integer
        Dim m_Rotation As New Rotation
        Dim m_Xref As Boolean
        Dim m_Segment As Segment

        Public Sub New(ByVal Owner As GraphicOwner, ByVal Xml As XmlNode)
            MyBase.New(Owner)
            m_Location = New PointF(toSingle(Xml.Attributes("x").Value), toSingle(Xml.Attributes("y").Value))
            m_Size = toSingle(Xml.Attributes("size").Value)
            m_Layer = Xml.Attributes("layer").Value
            If Xml.Attributes("font") IsNot Nothing Then m_Font = [Enum].Parse(GetType(TextFont), "TF_" & Xml.Attributes("font").Value)
            If Xml.Attributes("ratio") IsNot Nothing Then m_Ratio = Xml.Attributes("ratio").Value
            If Xml.Attributes("rot") IsNot Nothing Then m_Rotation = New Rotation(Xml.Attributes("rot").Value)
            If Xml.Attributes("xref") IsNot Nothing Then m_Xref = ParseBool(Xml.Attributes("xref").Value)
        End Sub

        Public ReadOnly Property Location() As PointF
            Get
                Return m_Location
            End Get
        End Property

        Public Property Size() As String
            Get
                Return m_Size
            End Get
            Set(ByVal value As String)
                m_Size = value
            End Set
        End Property

        Public Property Layer() As Integer
            Get
                Return m_Layer
            End Get
            Set(ByVal value As Integer)
                m_Layer = value
            End Set
        End Property

        Public Property Font() As String
            Get
                Return m_Font
            End Get
            Set(ByVal value As String)
                m_Font = value
            End Set
        End Property

        Public Property Ratio() As Integer
            Get
                Return m_Ratio
            End Get
            Set(ByVal value As Integer)
                m_Ratio = value
            End Set
        End Property

        Public Property Rotation() As Single
            Get
                Return m_Rotation.Rotation
            End Get
            Set(ByVal value As Single)
                m_Rotation.Rotation = value
            End Set
        End Property

        Public Property Xref() As Boolean
            Get
                Return m_Xref
            End Get
            Set(ByVal value As Boolean)
                m_Xref = value
            End Set
        End Property

        Protected Overrides Sub RebuildGraphics()
            MyBase.RebuildGraphics()
        End Sub

        Public Overrides Sub Render(ByVal Graphics As System.Drawing.Graphics)
            MyBase.Render(Graphics)
        End Sub

        Public Overrides Function GetRegion() As System.Drawing.Region
            Return MyBase.GetRegion()
        End Function

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "label"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("x")).Value = ToEagleSingle(m_Location.X)
                .Append(Doc.CreateAttribute("y")).Value = ToEagleSingle(m_Location.Y)
                .Append(Doc.CreateAttribute("size")).Value = ToEagleSingle(m_Size)
                .Append(Doc.CreateAttribute("layer")).Value = m_Layer
                .Append(Doc.CreateAttribute("font")).Value = Mid$([Enum].GetName(GetType(TextFont), m_Font), 4)
                .Append(Doc.CreateAttribute("ratio")).Value = m_Ratio
                .Append(Doc.CreateAttribute("rot")).Value = m_Rotation.ToString()
                .Append(Doc.CreateAttribute("xref")).Value = ToEagleBool(m_Xref)
            End With
        End Sub
    End Class

    Public Class Segment
        Implements EagleSaveable

        Dim m_PinRefs As New List(Of PinRef)
        Dim m_Wires As New List(Of Wire)
        Dim m_Junctions As New List(Of Junction)
        Dim m_Label As New List(Of Label)
        Dim m_Bus As Bus
        Dim m_Net As Net

        Public Sub New(ByVal Net As Net)
            m_Net = Net
        End Sub

        Public Sub New(ByVal Bus As Bus)
            m_Bus = Bus
        End Sub

        Public Sub New(ByVal Bus As Bus, ByVal Xml As XmlNode)
            m_Bus = Bus
            Init(Xml)
        End Sub

        Public Sub New(ByVal Net As Net, ByVal Xml As XmlNode)
            m_Net = Net
            Init(Xml)
        End Sub

        Private Sub Init(ByVal Xml As XmlNode)
            Dim PinRefNodes As XmlNodeList = Xml.SelectNodes("./pinref")
            For Each PinRefNode As XmlNode In PinRefNodes
                m_PinRefs.Add(New PinRef(Me, PinRefNode))
            Next
            Dim WireNodes As XmlNodeList = Xml.SelectNodes("./wire")
            For Each WireNode As XmlNode In WireNodes
                m_Wires.Add(New Wire(New GraphicOwner(Me), WireNode))
            Next
            Dim JunctionNodes As XmlNodeList = Xml.SelectNodes("./junction")
            For Each JunctionNode As XmlNode In JunctionNodes
                m_Junctions.Add(New Junction(New GraphicOwner(Me), JunctionNode))
            Next
        End Sub

        Public ReadOnly Property Bus() As Bus
            Get
                Return m_Bus
            End Get
        End Property

        Public ReadOnly Property Net() As Net
            Get
                Return m_Net
            End Get
        End Property

        Public ReadOnly Property Sheet() As Sheet
            Get
                If m_Net IsNot Nothing Then
                    Return m_Net.Sheet
                Else
                    Return m_Bus.Sheet
                End If
                'Return IIf(m_Net IsNot Nothing, m_Net.Sheet, m_Bus.Sheet)
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "segment"
            End Get
        End Property

        Public Function AddPinRef(ByVal Part As String, ByVal Gate As String, ByVal Pin As String) As PinRef
            Dim PinRef As New PinRef(Me, Part, Gate, Pin)
            m_PinRefs.Add(PinRef)
            Return PinRef
        End Function

        Public Function AddWire(ByVal Point1 As PointF, ByVal Point2 As PointF, ByVal Width As Single, ByVal Layer As Integer) As Wire
            Dim Wire As New Wire(New GraphicOwner(Me), Point1, Point2, Width, Layer)
            m_Wires.Add(Wire)
            Return Wire
        End Function

        Public Function AddJunction(ByVal Location As PointF) As Junction
            Dim Junction As New Junction(New GraphicOwner(Me), Location)
            m_Junctions.Add(Junction)
            Return Junction
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            For Each PinRef As PinRef In m_PinRefs
                PinRef.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(PinRef.NodeName())))
            Next
            For Each Wire As Wire In m_Wires
                Wire.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Wire.NodeName())))
            Next
            For Each Junction As Junction In m_Junctions
                Junction.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Junction.NodeName())))
            Next
        End Sub
    End Class

    Public Class Bus
        Implements EagleSaveable

        Dim m_Segments As New List(Of Segment)
        Dim m_Sheet As Sheet
        Dim m_Name As String

        Public Sub New(ByVal Sheet As Sheet, ByVal XmlNode As XmlNode)
            m_Sheet = Sheet
            m_Name = XmlNode.Attributes("name").Value
            Dim SegmentNodes As XmlNodeList = XmlNode.SelectNodes("./segment")
            For Each SegmentNode As XmlNode In SegmentNodes
                m_Segments.Add(New Segment(Me, SegmentNode))
            Next
        End Sub

        Public ReadOnly Property Segments() As List(Of Segment)
            Get
                Return m_Segments
            End Get
        End Property

        Public ReadOnly Property Sheet() As Sheet
            Get
                Return m_Sheet
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "bus"
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("name")).Value = m_Name
            For Each Segment As Segment In m_Segments
                Segment.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Segment.NodeName())))
            Next
        End Sub
    End Class

    Public Class Net
        Implements EagleSaveable

        Dim m_Segments As New List(Of Segment)
        Dim m_Sheet As Sheet
        Dim m_Name As String
        Dim m_Class As Integer

        Public Sub New(ByVal Sheet As Sheet, ByVal Name As String, Optional ByVal NetClass As Integer = 0)
            m_Sheet = Sheet
            m_Name = Name
            m_Class = NetClass
        End Sub

        Public Sub New(ByVal Sheet As Sheet, ByVal XmlNode As XmlNode)
            m_Sheet = Sheet
            m_Name = XmlNode.Attributes("name").Value
            If XmlNode.Attributes("class") IsNot Nothing Then m_Class = XmlNode.Attributes("class").Value
            Dim SegmentNodes As XmlNodeList = XmlNode.SelectNodes("./segment")
            For Each SegmentNode As XmlNode In SegmentNodes
                m_Segments.Add(New Segment(Me, SegmentNode))
            Next
        End Sub

        Public ReadOnly Property Segments() As List(Of Segment)
            Get
                Return m_Segments
            End Get
        End Property

        Public ReadOnly Property Sheet() As Sheet
            Get
                Return m_Sheet
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "net"
            End Get
        End Property

        Public Function AddSegment() As Segment
            Dim Segment As New Segment(Me)
            m_Segments.Add(Segment)
            Return Segment
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("class")).Value = m_Class
            End With
            For Each Segment As Segment In m_Segments
                Segment.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Segment.NodeName())))
            Next
        End Sub
    End Class

    Public Class Sheet
        Implements EagleSaveable

        Dim m_Description As String
        Dim m_Plain As Plain
        Dim m_Instances As New List(Of Instance)
        Dim m_Busses As New List(Of Bus)
        Dim m_Nets As New List(Of Net)
        Dim m_Schematic As Schematic

        Public Sub New(ByVal Schematic As Schematic, ByVal Xml As XmlNode)
            m_Schematic = Schematic
            Dim DescriptionNode As XmlNode = Xml.SelectSingleNode("description")
            If DescriptionNode IsNot Nothing Then m_Description = DescriptionNode.FirstChild.Value
            Dim PlainNode As XmlNode = Xml.SelectSingleNode("plain")
            If PlainNode IsNot Nothing Then m_Plain = New Plain(Me, PlainNode)
            Dim InstanceNodes As XmlNodeList = Xml.SelectNodes("./instances/instance")
            For Each InstanceNode As XmlNode In InstanceNodes
                m_Instances.Add(New Instance(Me, InstanceNode))
            Next
            Dim BusNodes As XmlNodeList = Xml.SelectNodes("./busses/bus")
            For Each BusNode As XmlNode In BusNodes
                m_Busses.Add(New Bus(Me, BusNode))
            Next
            Dim NetNodes As XmlNodeList = Xml.SelectNodes("./nets/net")
            For Each NetNode As XmlNode In NetNodes
                m_Nets.Add(New Net(Me, NetNode))
            Next
        End Sub

        Public Sub New(ByVal Schematic As Schematic)
            m_Schematic = Schematic
        End Sub

        Public Property Description() As String
            Get
                Return m_Description
            End Get
            Set(ByVal value As String)
                m_Description = value
            End Set
        End Property

        Public ReadOnly Property Plain() As Plain
            Get
                Return m_Plain
            End Get
        End Property

        Public ReadOnly Property Instances() As List(Of Instance)
            Get
                Return m_Instances
            End Get
        End Property

        Public ReadOnly Property Busses() As List(Of Bus)
            Get
                Return m_Busses
            End Get
        End Property

        Public ReadOnly Property Nets() As List(Of Net)
            Get
                Return m_Nets
            End Get
        End Property

        Public ReadOnly Property Schematic() As Schematic
            Get
                Return m_Schematic
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "sheet"
            End Get
        End Property

        ''' <summary>
        ''' Returns an instance of a gate given the device name and gate name
        ''' For example gate A of IC1
        ''' </summary>
        ''' <param name="PartName"></param>
        ''' <param name="GateName"></param>
        ''' <returns>Instance</returns>
        ''' <remarks></remarks>
        Public Function GetInstance(ByVal PartName As String, ByVal GateName As String) As Instance
            For Each Instance As Instance In m_Instances
                If Instance.Part = PartName AndAlso Instance.Gate = GateName Then
                    Return Instance
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Adds a new instance of a gate for a device (part) name
        ''' for example: gate A of IC1
        ''' </summary>
        ''' <param name="Part"></param>
        ''' <param name="Gate"></param>
        ''' <param name="Location"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddInstance(ByVal Part As Part, ByVal Gate As Gate, ByVal Location As PointF, Optional ByVal Rotation As Single = 0) As Instance
            Dim Instance As New Instance(Me, Part.Name, Gate.Name, Location)
            If Rotation <> 0 Then Instance.Rotation = Rotation
            m_Instances.Add(Instance)
            Return Instance
        End Function

        Public Function AddNet(ByVal Name As String, Optional ByVal NetClass As Integer = 0) As Net
            Dim Net As New Net(Me, Name, NetClass)
            m_Nets.Add(Net)
            Return Net
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.AppendChild(Doc.CreateElement("description")).AppendChild(Doc.CreateTextNode(m_Description))
            If m_Plain IsNot Nothing Then m_Plain.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(m_Plain.NodeName())))
            Dim InstanceNodes As XmlNode = Node.AppendChild(Doc.CreateElement("instances"))
            For Each Instance As Instance In m_Instances
                Instance.WriteXml(Doc, InstanceNodes.AppendChild(Doc.CreateElement(Instance.NodeName())))
            Next
            Dim BusNodes As XmlNode = Node.AppendChild(Doc.CreateElement("busses"))
            For Each Bus As Bus In m_Busses
                Bus.WriteXml(Doc, BusNodes.AppendChild(Doc.CreateElement(Bus.NodeName())))
            Next
            Dim NetNodes As XmlNode = Node.AppendChild(Doc.CreateElement("nets"))
            For Each Net As Net In m_Nets
                Net.WriteXml(Doc, NetNodes.AppendChild(Doc.CreateElement(Net.NodeName())))
            Next
        End Sub
    End Class

    Public Class Part
        Implements EagleSaveable

        Dim m_Name As String
        Dim m_Library As String
        Dim m_DeviceSet As String
        Dim m_Device As String
        Dim m_Technology As String
        Dim m_Value As String

        Dim m_Attributes As New List(Of Attribute)
        Dim m_Variants As New List(Of EagleVariant)
        Dim m_Schematic As Schematic

        Dim m_oDevice As Device

        Public Sub New(ByVal Schematic As Schematic, ByVal Name As String, ByVal Library As String, ByVal DeviceSet As String, ByVal Value As String)
            Me.New(Schematic, Name, Library, DeviceSet, "", "", "")
        End Sub

        Public Sub New(ByVal Schematic As Schematic, ByVal Name As String, ByVal Library As String, ByVal DeviceSet As String, ByVal Device As String, ByVal Technology As String, ByVal Value As String)
            m_Schematic = Schematic
            m_Name = Name
            m_Library = Library
            m_DeviceSet = DeviceSet
            m_Device = Device
            m_Technology = Technology
            m_Value = Value
        End Sub

        Public Sub New(ByVal Schematic As Schematic, ByVal Xml As XmlNode)
            m_Name = Xml.Attributes("name").Value
            m_Library = Xml.Attributes("library").Value
            m_DeviceSet = Xml.Attributes("deviceset").Value
            If Xml.Attributes("technology") IsNot Nothing Then m_Technology = Xml.Attributes("technology").Value
            If Xml.Attributes("value") IsNot Nothing Then m_Value = Xml.Attributes("value").Value

            Dim AttributeNodes As XmlNodeList = Xml.SelectNodes("./attribute")
            For Each AttributeNode As XmlNode In AttributeNodes
                m_Attributes.Add(New Attribute(Me, AttributeNode))
            Next

            Dim VariantNodes As XmlNodeList = Xml.SelectNodes("./variant")
            For Each VariantNode As XmlNode In VariantNodes
                m_Variants.Add(New EagleVariant(Me, VariantNode))
            Next
            m_Schematic = Schematic
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Library() As String
            Get
                Return m_Library
            End Get
            Set(ByVal value As String)
                m_Library = value
            End Set
        End Property

        Public Property DeviceSet() As String
            Get
                Return m_DeviceSet
            End Get
            Set(ByVal value As String)
                m_DeviceSet = value
            End Set
        End Property

        Public Property Device() As String
            Get
                Return m_Device
            End Get
            Set(ByVal value As String)
                m_Device = value
            End Set
        End Property

        Public Property Technology() As String
            Get
                Return m_Technology
            End Get
            Set(ByVal value As String)
                m_Technology = value
            End Set
        End Property

        Public Property Value() As String
            Get
                Return m_Value
            End Get
            Set(ByVal value As String)
                m_Value = value
            End Set
        End Property

        Public ReadOnly Property Attributes() As List(Of Attribute)
            Get
                Return m_Attributes
            End Get
        End Property

        Public ReadOnly Property Variants() As List(Of EagleVariant)
            Get
                Return m_Variants
            End Get
        End Property


        Public ReadOnly Property Schematic() As Schematic
            Get
                Return m_Schematic
            End Get
        End Property

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "part"
            End Get
        End Property

        ''' <summary>
        ''' returns the symbol of a gate of this part
        ''' </summary>
        ''' <param name="GateName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSymbol(ByVal GateName As String) As Symbol
            If m_oDevice Is Nothing Then
                m_oDevice = m_Schematic.GetLibrary(m_Library).GetDevice(m_DeviceSet, m_Device)
            End If
            Return m_oDevice.DeviceSet.GetGate(GateName).GetSymbol()
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("library")).Value = m_Library
                .Append(Doc.CreateAttribute("deviceset")).Value = m_DeviceSet
                .Append(Doc.CreateAttribute("device")).Value = m_Device
                If m_Technology <> "" Then .Append(Doc.CreateAttribute("technology")).Value = m_Technology
                If m_Value <> "" Then .Append(Doc.CreateAttribute("value")).Value = m_Value
            End With
            For Each Attribute As Attribute In m_Attributes
                Attribute.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Attribute.NodeName())))
            Next
            For Each EagleVariant As EagleVariant In m_Variants
                EagleVariant.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(EagleVariant.NodeName())))
            Next
        End Sub
    End Class

    Public Class Schematic
        Implements EagleSaveable

        Dim m_Drawing As Drawing
        Dim m_Description As String
        Dim m_XRefLabel As String
        Dim m_XRefPart As String
        Dim m_LibraryNodes As New Dictionary(Of String, XmlNode) 'libraries can also be added as complete xml nodes, for saving the document faster
        Dim m_Libraries As New List(Of Library)
        Dim m_VariantDefs As New List(Of VariantDef)
        Dim m_NetClasses As New List(Of NetClass)
        Dim m_Parts As New List(Of Part)
        Dim m_Attributes As New List(Of Attribute)
        Dim m_Sheets As New List(Of Sheet)
        Dim m_SaveLibraries As Boolean 'if set to true the libraries list will be saved, else only the librarynodes will be saved
        Dim m_DefaultWireWidth As Single


        Public Sub New(ByVal Drawing As Drawing)
            Me.new(Drawing, "%F%N/%S.%C%R", "/%S.%C%R")

        End Sub

        Public Sub New(ByVal Drawing As Drawing, ByVal XrefLabel As String, ByVal XrefPart As String)
            m_XRefLabel = XrefLabel
            m_XRefPart = XrefPart
            m_NetClasses.Add(New NetClass(Me))
            m_DefaultWireWidth = 0.1524
            m_Drawing = Drawing
        End Sub

        Public Sub New(ByVal Drawing As Drawing, ByVal Xml As XmlNode)
            m_Drawing = Drawing
            If Xml.SelectSingleNode("description") IsNot Nothing Then
                m_Description = Xml.SelectSingleNode("description").ChildNodes(0).Value
            End If
            Dim AttributeNodes As XmlNodeList = Xml.SelectNodes("./attributes/attribute")
            For Each AttributeNode As XmlNode In AttributeNodes
                m_Attributes.Add(New Attribute(Me, AttributeNode))
            Next
            Dim VariantDefNodes As XmlNodeList = Xml.SelectNodes("./variantdefs/variantdef")
            For Each VariantDefNode As XmlNode In VariantDefNodes
                m_VariantDefs.Add(New VariantDef(Me, VariantDefNode))
            Next
            Dim NetClassNodes As XmlNodeList = Xml.SelectNodes("./classes/class")
            For Each NetClassNode As XmlNode In NetClassNodes
                m_NetClasses.Add(New NetClass(Me, NetClassNode))
            Next
            Dim SheetNodes As XmlNodeList = Xml.SelectNodes("./sheets/sheet")
            For Each SheetNode As XmlNode In SheetNodes
                m_Sheets.Add(New Sheet(Me, SheetNode))
            Next
            Dim PartNodes As XmlNodeList = Xml.SelectNodes("./parts/part")
            For Each PartNode As XmlNode In PartNodes
                m_Parts.Add(New Part(Me, PartNode))
            Next
            m_DefaultWireWidth = 0.1524
        End Sub

        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Drawing
            End Get
        End Property

        Public Property Description() As String
            Get
                Return m_Description
            End Get
            Set(ByVal value As String)
                m_Description = value
            End Set
        End Property

        Public Property XrefLabel() As String
            Get
                Return m_XRefLabel
            End Get
            Set(ByVal value As String)
                m_XRefLabel = value
            End Set
        End Property

        Public Property XrefPart() As String
            Get
                Return m_XRefPart
            End Get
            Set(ByVal value As String)
                m_XRefPart = value
            End Set
        End Property

        Public Function GetLibrary(ByVal Name As String) As Library
            For Each Library As Library In m_Libraries
                If Library.Name = Name Then
                    Return Library
                End If
            Next
            Return Nothing
        End Function

        Public ReadOnly Property Libraries() As List(Of Library)
            Get
                Return m_Libraries
            End Get
        End Property

        Public ReadOnly Property VariantDefs() As List(Of VariantDef)
            Get
                Return m_VariantDefs
            End Get
        End Property

        Public ReadOnly Property NetClasses() As List(Of NetClass)
            Get
                Return m_NetClasses
            End Get
        End Property

        Public ReadOnly Property Attributes() As List(Of Attribute)
            Get
                Return m_Attributes
            End Get
        End Property

        Public ReadOnly Property Parts() As List(Of Part)
            Get
                Return m_Parts
            End Get
        End Property

        Public ReadOnly Property Sheets() As List(Of Sheet)
            Get
                Return m_Sheets
            End Get
        End Property

        Public ReadOnly Property LibraryNodes() As Dictionary(Of String, XmlNode)
            Get
                Return m_LibraryNodes
            End Get
        End Property

        Public Property DefaultWireWidth() As Single
            Get
                Return m_DefaultWireWidth
            End Get
            Set(ByVal value As Single)
                m_DefaultWireWidth = value
            End Set
        End Property

        Public ReadOnly Property DefaultWireLayer() As Integer
            Get
                Return 91
            End Get
        End Property

        Public ReadOnly Property DefaultBusLayer() As Integer
            Get
                Return 92
            End Get
        End Property

        ''' <summary>
        ''' This property determines if library objects are saved or only added library xml nodes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SaveLibraryObjects() As Boolean
            Get
                Return m_SaveLibraries
            End Get
            Set(ByVal value As Boolean)
                m_SaveLibraries = value
            End Set
        End Property

        Public Sub AddXMLLibraryNode(ByVal LibraryName As String, ByVal Node As XmlNode)
            m_LibraryNodes.Add(LibraryName, Node)
        End Sub

        Public Function AddLibrary(ByVal Library As Library) As Library
            m_Libraries.Add(Library)
            Return Library
        End Function

        Public Function AddSheet() As Sheet
            Dim Sheet As Sheet = New Sheet(Me)
            m_Sheets.Add(Sheet)
            Return Sheet
        End Function

        Public Function GetPart(ByVal Name As String) As Part
            For Each Part As Part In m_Parts
                If Part.Name = Name Then
                    Return Part
                End If
            Next
            Return Nothing
        End Function

        Public Function AddPart(ByVal Name As String, ByVal LibraryName As String, ByVal DeviceSet As String, ByVal Device As String, Optional ByVal Technology As String = "", Optional ByVal Value As String = "") As Part
            Dim Part As New Part(Me, Name, LibraryName, DeviceSet, Device, Technology, Value)
            m_Parts.Add(Part)
            Return Part
        End Function

        Public Function AddPart(ByVal Device As Device, ByVal Technology As Technology, ByVal DeviceName As String, Optional ByVal Value As String = "") As Part
            With Device
                Dim TechnologyName As String = ""
                If Technology IsNot Nothing Then TechnologyName = Technology.Name
                Return Me.AddPart(DeviceName, .DeviceSet.Library.Name, .DeviceSet.Name, .Name, TechnologyName, Value)
            End With
        End Function

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("xreflabel")).Value = m_XRefLabel
            Node.Attributes.Append(Doc.CreateAttribute("xrefpart")).Value = m_XRefPart
            Node.AppendChild(Doc.CreateElement("description")).AppendChild(Doc.CreateTextNode(m_Description))
            Dim LibraryNodes As XmlNode = Node.AppendChild(Doc.CreateElement("libraries"))
            For Each Library As KeyValuePair(Of String, XmlNode) In m_LibraryNodes 'save all the imported library xml nodes
                Dim LibNode As XmlNode = Doc.ImportNode(Library.Value, True)
                LibNode.Attributes.Append(Doc.CreateAttribute("name")).Value = Library.Key
                LibraryNodes.AppendChild(LibNode)
            Next
            If m_SaveLibraries Then
                For Each Library As Library In m_Libraries
                    Dim LibNode As XmlNode = LibraryNodes.AppendChild(Doc.CreateElement(Library.NodeName()))
                    LibNode.Attributes.Append(Doc.CreateAttribute("name")).Value = Library.Drawing.Project.ShortFileName
                    Library.WriteXml(Doc, LibNode)
                Next
            End If
            Dim VariantDefNodes As XmlNode = Node.AppendChild(Doc.CreateElement("variantdefs"))
            For Each VariantDef As VariantDef In m_VariantDefs
                Dim VariantDefNode As XmlNode = VariantDefNodes.AppendChild(Doc.CreateElement(VariantDef.NodeName()))
                VariantDef.WriteXml(Doc, VariantDefNode)
            Next
            Dim NetClassNodes As XmlNode = Node.AppendChild(Doc.CreateElement("classes"))
            For Each NetClass As NetClass In m_NetClasses
                NetClass.WriteXml(Doc, NetClassNodes.AppendChild(Doc.CreateElement(NetClass.NodeName())))
            Next
            Dim PartNodes As XmlNode = Node.AppendChild(Doc.CreateElement("parts"))
            For Each Part As Part In m_Parts
                Part.WriteXml(Doc, PartNodes.AppendChild(Doc.CreateElement(Part.NodeName())))
            Next

            Dim SheetNodes As XmlNode = Node.AppendChild(Doc.CreateElement("sheets"))
            For Each sheet As Sheet In m_Sheets
                sheet.WriteXml(Doc, SheetNodes.AppendChild(Doc.CreateElement(sheet.NodeName())))
            Next
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "schematic"
            End Get
        End Property
    End Class


    Public Class Layer
        Implements EagleSaveable

        Dim m_Number As Integer
        Dim m_Name As String
        Dim m_Color As Integer
        Dim m_Fill As Integer
        Dim m_Visible As Boolean = True
        Dim m_Active As Boolean = True
        Dim m_Drawing As Drawing

        Public Const LayerSymbols As Integer = 94
        Public Const LayerNames As Integer = 95

        Public Sub New(ByVal Drawing As Drawing)
            m_Drawing = Drawing
        End Sub

        Public Sub New(ByVal Drawing As Drawing, ByVal Number As Integer, ByVal Name As String, ByVal Color As Integer, ByVal Fill As Integer)
            Me.New(Drawing, Number, Name, Color, Fill, True, True)
        End Sub

        Public Sub New(ByVal Drawing As Drawing, ByVal Number As Integer, ByVal Name As String, ByVal Color As Integer, ByVal Fill As Integer, ByVal Visible As Boolean, ByVal Active As Boolean)
            m_Drawing = Drawing
            m_Number = Number
            m_Name = Name
            m_Color = Color
            m_Fill = Fill
            m_Visible = Visible
            m_Active = Active
        End Sub

        Public Sub New(ByVal Drawing As Drawing, ByVal Xml As XmlNode)
            m_Drawing = Drawing
            m_Number = Xml.Attributes("number").Value
            m_Name = Xml.Attributes("name").Value
            m_Color = Xml.Attributes("color").Value
            m_Fill = Xml.Attributes("fill").Value
            If Xml.Attributes("visible") IsNot Nothing Then m_Visible = Functions.ParseBool(Xml.Attributes("visible").Value)
            If Xml.Attributes("active") IsNot Nothing Then m_Active = Functions.ParseBool(Xml.Attributes("active").Value)
        End Sub

        Public Property Number() As Integer
            Get
                Return m_Number
            End Get
            Set(ByVal value As Integer)
                m_Number = value
            End Set
        End Property

        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Drawing
            End Get
        End Property

        Public Property ColorNumber() As Integer
            Get
                Return m_Color
            End Get
            Set(ByVal value As Integer)
                m_Color = value
            End Set
        End Property

        Public ReadOnly Property Color() As System.Drawing.Color
            Get
                Return m_Drawing.Project.ConvertColor(m_Color)
            End Get
        End Property

        Public ReadOnly Property SelectedColor() As System.Drawing.Color
            Get
                Return m_Drawing.Project.ConvertSelectedColor(m_Color)
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("name")).Value = m_Name
                .Append(Doc.CreateAttribute("color")).Value = m_Color
                .Append(Doc.CreateAttribute("fill")).Value = m_Fill
                .Append(Doc.CreateAttribute("visible")).Value = ToEagleBool(m_Visible)
                .Append(Doc.CreateAttribute("active")).Value = ToEagleBool(m_Active)
                .Append(Doc.CreateAttribute("number")).Value = m_Number
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "layer"
            End Get
        End Property
    End Class

    Public Class Grid
        Implements EagleSaveable

        Dim m_Drawing As Drawing
        Dim m_Distance As Single
        Dim m_UnitDist As GridUnit
        Dim m_Unit As GridUnit
        Dim m_Style As GridStyle
        Dim m_Multiple As Integer
        Dim m_Display As Boolean
        Dim m_AltDistance As Single
        Dim m_AltUnitDist As GridUnit
        Dim m_AltUnit As GridUnit

        Public Sub New()
            m_Distance = 0.1
            m_UnitDist = GridUnit.GU_inch
            m_Unit = GridUnit.GU_inch
            m_Style = GridStyle.GS_lines
            m_Multiple = 1
            m_Display = False
            m_AltDistance = 0.01
            m_AltUnitDist = GridUnit.GU_inch
            m_AltUnit = GridUnit.GU_inch
        End Sub

        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Drawing
            End Get
        End Property

        Public Property Distance() As Single
            Get
                Return m_Distance
            End Get
            Set(ByVal value As Single)
                m_Distance = value
            End Set
        End Property

        Public Property UnitDist() As GridUnit
            Get
                Return m_UnitDist
            End Get
            Set(ByVal value As GridUnit)
                m_UnitDist = value
            End Set
        End Property

        Public Property Unit() As GridUnit
            Get
                Return m_Unit
            End Get
            Set(ByVal value As GridUnit)
                m_Unit = value
            End Set
        End Property

        Public Property Style() As GridStyle
            Get
                Return m_Style
            End Get
            Set(ByVal value As GridStyle)
                m_Style = value
            End Set
        End Property

        Public Property Multiple() As Integer
            Get
                Return m_Multiple
            End Get
            Set(ByVal value As Integer)
                m_Multiple = value
            End Set
        End Property

        Public Property Display() As Boolean
            Get
                Return m_Display
            End Get
            Set(ByVal value As Boolean)
                m_Display = value
            End Set
        End Property

        Public Property AltDistance() As Single
            Get
                Return m_AltDistance
            End Get
            Set(ByVal value As Single)
                m_AltDistance = value
            End Set
        End Property

        Public Property AltUnitDist() As GridUnit
            Get
                Return m_AltUnitDist
            End Get
            Set(ByVal value As GridUnit)
                m_AltUnitDist = value
            End Set
        End Property

        Public Property AltUnit() As GridUnit
            Get
                Return m_AltUnit
            End Get
            Set(ByVal value As GridUnit)
                m_AltUnit = value
            End Set
        End Property

        ''' <summary>
        ''' Rounds a location to fit the grid
        ''' </summary>
        ''' <param name="Location"></param>
        ''' <param name="isAltCo" >Sets if we need to use the Alt grid</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RoundLocation(ByVal Location As PointF, Optional ByVal isAltCo As Boolean = False) As PointF
            Dim Dist As Single = m_Distance
            Dim Unit As GridUnit = m_UnitDist
            If isAltCo Then
                Dist = m_AltDistance
                Unit = m_AltUnitDist
            End If

            Select Case Unit
                Case GridUnit.GU_inch
                    Return New PointF(RoundToGrid(Location.X, (Dist * 25.4)), RoundToGrid(Location.Y, (Dist * 25.4)))
                    'Return New PointF(Round(Location.X - Location.X Mod (Dist * 25.4), 5), Round(Location.Y - Location.Y Mod (Dist * 25.4), 5))
                Case GridUnit.GU_mic
                    Return New PointF(RoundToGrid(Location.X, (Dist / 1000)), RoundToGrid(Location.Y, (Dist / 1000)))
                    'Return New PointF(Round(Location.X - Location.X Mod (Dist / 1000), 5), Round(Location.Y - Location.Y Mod (Dist / 1000), 5))
                Case GridUnit.GU_mil
                    Return New PointF(RoundToGrid(Location.X, (Dist / 1000 * 25.4)), RoundToGrid(Location.Y, (Dist / 1000 * 25.4)))
                    'Return New PointF(Round(Location.X - Location.X Mod (Dist / 1000 * 25.4), 5), Round(Location.Y - Location.Y Mod (Dist / 1000 * 25.4), 5))
                Case GridUnit.GU_mm
                    Return New PointF(RoundToGrid(Location.X, Dist), RoundToGrid(Location.Y, Dist))
                    'Return New PointF(Round(Location.X - Location.X Mod Dist), Round(Location.Y - Location.Y Mod Dist, 5))
            End Select
        End Function

        Private Function RoundToGrid(ByVal Number As Single, ByVal GridSize As Single) As Single
            Return Int(Number / GridSize + 0.5F) * GridSize
        End Function

        Public Sub New(ByVal Drawing As Drawing, ByVal xml As XmlNode)
            m_Drawing = Drawing
            If xml.Attributes("distance") IsNot Nothing Then m_Distance = toSingle(xml.Attributes("distance").Value)
            If xml.Attributes("unitdist") IsNot Nothing Then m_UnitDist = [Enum].Parse(GetType(GridUnit), "GU_" & xml.Attributes("unitdist").Value)
            If xml.Attributes("unit") IsNot Nothing Then m_Unit = [Enum].Parse(GetType(GridUnit), "GU_" & xml.Attributes("unit").Value)
            If xml.Attributes("style") IsNot Nothing Then m_Style = [Enum].Parse(GetType(GridStyle), "GS_" & xml.Attributes("style").Value)
            If xml.Attributes("multiple") IsNot Nothing Then m_Multiple = xml.Attributes("multiple").Value
            If xml.Attributes("display") IsNot Nothing Then m_Display = ParseBool(xml.Attributes("display").Value)
            If xml.Attributes("altdistance") IsNot Nothing Then m_AltDistance = toSingle(xml.Attributes("altdistance").Value)
            If xml.Attributes("altunitdist") IsNot Nothing Then m_AltUnitDist = [Enum].Parse(GetType(GridUnit), "GU_" & xml.Attributes("altunitdist").Value)
            If xml.Attributes("altunit") IsNot Nothing Then m_AltUnit = [Enum].Parse(GetType(GridUnit), "GU_" & xml.Attributes("altunit").Value)
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            With Node.Attributes
                .Append(Doc.CreateAttribute("distance")).Value = ToEagleSingle(m_Distance)
                .Append(Doc.CreateAttribute("unitdist")).Value = Mid([Enum].GetName(GetType(GridUnit), m_UnitDist), 4)
                .Append(Doc.CreateAttribute("unit")).Value = Mid([Enum].GetName(GetType(GridUnit), m_Unit), 4)
                .Append(Doc.CreateAttribute("style")).Value = Mid([Enum].GetName(GetType(GridStyle), m_Style), 4)
                .Append(Doc.CreateAttribute("multiple")).Value = m_Multiple
                .Append(Doc.CreateAttribute("display")).Value = ToEagleBool(m_Display)
                .Append(Doc.CreateAttribute("altdistance")).Value = ToEagleSingle(m_AltDistance)
                .Append(Doc.CreateAttribute("altunitdist")).Value = Mid([Enum].GetName(GetType(GridUnit), m_AltUnitDist), 4)
                .Append(Doc.CreateAttribute("altunit")).Value = Mid([Enum].GetName(GetType(GridUnit), m_AltUnit), 4)
            End With
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "grid"
            End Get
        End Property
    End Class

    Public Class Drawing
        Implements EagleSaveable

        Dim m_Project As Project
        Dim m_Layers As New Dictionary(Of Integer, Layer)
        Dim m_Settings As New Dictionary(Of String, String)
        Dim m_Grid As Grid
        Dim m_Schematic As Schematic
        Dim m_Library As Library
        Dim m_DrawingType As DrawingType

        Public Sub New(ByVal Project As Project)
            Me.New(Project, New Grid())
        End Sub

        Public Sub New(ByVal Project As Project, ByVal Grid As Grid)
            m_Project = Project
            m_Grid = Grid
            m_Settings.Add("alwaysvectorfont", "no")
            m_Settings.Add("verticaltext", "up")
        End Sub

        Public Sub New(ByVal Project As Project, ByVal Xml As XmlNode)
            Dim LayerNodes As XmlNodeList = Xml.SelectNodes("./layers/layer")
            Dim SettingNodes As XmlNodeList = Xml.SelectNodes("./settings/setting")
            Dim GridNode As XmlNode = Xml.SelectSingleNode("./grid")
            Dim Schematic As XmlNode = Xml.SelectSingleNode("./schematic")
            Dim Library As XmlNode = Xml.SelectSingleNode("./library")
            Dim Board As XmlNode = Xml.SelectSingleNode("./board")
            Dim Layer As Layer

            m_Project = Project
            For Each LayerNode As XmlNode In LayerNodes
                Layer = New Layer(Me, LayerNode)
                m_Layers.Add(Layer.Number, Layer)
            Next

            If SettingNodes IsNot Nothing Then
                For Each SettingNode As XmlNode In SettingNodes
                    m_Settings.Add(SettingNode.Attributes(0).Name, SettingNode.Attributes(0).Value)
                Next
            End If

            If GridNode IsNot Nothing Then
                m_Grid = New Grid(Me, GridNode)
            End If

            If Schematic IsNot Nothing Then
                m_Schematic = New Schematic(Me, Schematic)
                m_DrawingType = DrawingType.schematic
            ElseIf Library IsNot Nothing Then
                m_Library = New Library(Me, Library)
                m_DrawingType = DrawingType.library
            ElseIf Board IsNot Nothing Then
                m_DrawingType = DrawingType.board
            End If

        End Sub

        Public ReadOnly Property Layers() As Dictionary(Of Integer, Layer)
            Get
                Return m_Layers
            End Get
        End Property

        Public ReadOnly Property Layer(ByVal Number As Integer) As Layer
            Get
                Return m_Layers(Number)
            End Get
        End Property

        Public ReadOnly Property Schematic() As Schematic
            Get
                Return m_Schematic
            End Get
        End Property

        Public ReadOnly Property Library() As Library
            Get
                Return m_Library
            End Get
        End Property

        Public Property Setting(ByVal Name As String) As String
            Get
                Return m_Settings(Name)
            End Get
            Set(ByVal value As String)
                If Not m_Settings.ContainsKey(Name) Then
                    m_Settings.Add(Name, value)
                Else
                    m_Settings(Name) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Returns the type of drawing (schematic, board, library)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DrawingType() As DrawingType
            Get
                Return m_DrawingType
            End Get
            Set(ByVal value As DrawingType)
                m_DrawingType = value
            End Set
        End Property

        Public ReadOnly Property Grid() As Grid
            Get
                Return m_Grid
            End Get
        End Property

        ''' <summary>
        ''' Returns the project
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Project() As Project
            Get
                Return m_Project
            End Get
        End Property

        ''' <summary>
        ''' Creates a schematic project, loads default layers if no layers are loaded
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateSchematic() As Schematic
            Dim Schematic As Schematic = New Schematic(Me)
            If m_Layers.Count = 0 Then LoadDefaultSchematicLayers()
            m_Schematic = Schematic
            m_DrawingType = Eagle.DrawingType.schematic
            Return m_Schematic
        End Function

        ''' <summary>
        ''' Creates anew library, to be defined
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateLibrary() As Library
            m_DrawingType = Eagle.DrawingType.library
            Return Nothing
        End Function

        ''' <summary>
        ''' Adds a layer to the layers collection
        ''' </summary>
        ''' <param name="Layer"></param>
        ''' <remarks></remarks>
        Public Function AddLayer(Optional ByVal Layer As Layer = Nothing) As Layer
            If Layer Is Nothing Then Layer = New Layer(Me)
            m_Layers.Add(Layer.Number, Layer)
            Return Layer
        End Function

        ''' <summary>
        ''' Loads the default layers for a schematic
        ''' Removes all previous added layers
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub LoadDefaultSchematicLayers()
            m_Layers.Clear()
            AddLayer(New Layer(Me, 1, "Top", 4, 1, False, False))
            AddLayer(New Layer(Me, 16, "Bottom", 1, 1, False, False))
            AddLayer(New Layer(Me, 17, "Pads", 2, 1, False, False))
            AddLayer(New Layer(Me, 18, "Vias", 2, 1, False, False))
            AddLayer(New Layer(Me, 19, "Unrouted", 6, 1, False, False))
            AddLayer(New Layer(Me, 20, "Dimension", 15, 1, False, False))
            AddLayer(New Layer(Me, 21, "tPlace", 7, 1, False, False))
            AddLayer(New Layer(Me, 22, "bPlace", 7, 1, False, False))
            AddLayer(New Layer(Me, 23, "tOrigins", 15, 1, False, False))
            AddLayer(New Layer(Me, 24, "bOrigins", 15, 1, False, False))
            AddLayer(New Layer(Me, 25, "tNames", 7, 1, False, False))
            AddLayer(New Layer(Me, 26, "bNames", 7, 1, False, False))
            AddLayer(New Layer(Me, 27, "tValues", 7, 1, False, False))
            AddLayer(New Layer(Me, 28, "bValues", 7, 1, False, False))
            AddLayer(New Layer(Me, 29, "tStop", 7, 3, False, False))
            AddLayer(New Layer(Me, 30, "bStop", 7, 6, False, False))
            AddLayer(New Layer(Me, 31, "tCream", 7, 4, False, False))
            AddLayer(New Layer(Me, 32, "bCream", 7, 5, False, False))
            AddLayer(New Layer(Me, 33, "tFinish", 6, 3, False, False))
            AddLayer(New Layer(Me, 34, "bFinish", 6, 6, False, False))
            AddLayer(New Layer(Me, 35, "tGlue", 7, 4, False, False))
            AddLayer(New Layer(Me, 36, "bGlue", 7, 5, False, False))
            AddLayer(New Layer(Me, 37, "tTest", 7, 1, False, False))
            AddLayer(New Layer(Me, 38, "bTest", 7, 1, False, False))
            AddLayer(New Layer(Me, 39, "tKeepout", 4, 11, False, False))
            AddLayer(New Layer(Me, 40, "bKeepout", 4, 11, False, False))
            AddLayer(New Layer(Me, 41, "tRestrict", 4, 10, False, False))
            AddLayer(New Layer(Me, 42, "bRestrict", 1, 10, False, False))
            AddLayer(New Layer(Me, 43, "vRestrict", 2, 10, False, False))
            AddLayer(New Layer(Me, 44, "Drills", 7, 1, False, False))
            AddLayer(New Layer(Me, 45, "Holes", 7, 1, False, False))
            AddLayer(New Layer(Me, 46, "Milling", 3, 1, False, False))
            AddLayer(New Layer(Me, 47, "Measures", 7, 1, False, False))
            AddLayer(New Layer(Me, 48, "Document", 7, 1, False, False))
            AddLayer(New Layer(Me, 49, "Reference", 7, 1, False, False))
            AddLayer(New Layer(Me, 51, "tDocu", 7, 9, False, False))
            AddLayer(New Layer(Me, 52, "bDocu", 7, 1, False, False))
            AddLayer(New Layer(Me, 91, "Nets", 2, 1, True, True))
            AddLayer(New Layer(Me, 92, "Busses", 1, 1, True, True))
            AddLayer(New Layer(Me, 93, "Pins", 2, 1, False, True))
            AddLayer(New Layer(Me, 94, "Symbols", 4, 1, True, True))
            AddLayer(New Layer(Me, 95, "Names", 7, 1, True, True))
            AddLayer(New Layer(Me, 96, "Values", 7, 1, True, True))
            AddLayer(New Layer(Me, 97, "Info", 7, 1, True, True))
            AddLayer(New Layer(Me, 98, "Guide", 6, 1, True, True))
            AddLayer(New Layer(Me, 118, "Rect_Pads", 4, 1, True, True))
        End Sub

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Dim SettingsNode As XmlNode = Node.AppendChild(Doc.CreateElement("settings"))
            For Each Setting As KeyValuePair(Of String, String) In m_Settings
                SettingsNode.AppendChild(Doc.CreateElement("setting")).Attributes.Append(Doc.CreateAttribute(Setting.Key)).Value = Setting.Value
            Next

            m_Grid.WriteXml(Doc, Node.AppendChild(Doc.CreateElement("grid")))

            Dim LayerNodes As XmlNode = Node.AppendChild(Doc.CreateElement("layers"))
            For Each Layer As KeyValuePair(Of Integer, Layer) In m_Layers
                Layer.Value.WriteXml(Doc, LayerNodes.AppendChild(Doc.CreateElement(Layer.Value.NodeName())))
            Next

            If m_Schematic IsNot Nothing Then 'save the schematic
                m_Schematic.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(m_Schematic.NodeName())))
            End If
            If m_Library IsNot Nothing Then
                m_Library.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(m_Library.NodeName())))
            End If
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "drawing"
            End Get
        End Property
    End Class

    Public Enum ColorPalleteSettings As Integer
        White = 0
        Black = 1
        Colored = 2
    End Enum

    ''' <summary>
    ''' Represents the Eagle project (Eagle node)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Project
        Implements EagleSaveable

        Public Const EagleVersion As String = "6.4"

        Dim m_Xml As XmlDocument
        Dim m_Drawing As Drawing
        Dim m_Version As String
        Dim m_FileName As String
        Dim m_ShortName As String
        Dim m_Pallete As ColorPalleteSettings = ColorPalleteSettings.White
        Dim m_Font As System.Drawing.FontFamily
        Dim m_Colors(,) As System.Drawing.Color = New System.Drawing.Color(,) {{Color.FromArgb(&H0), _
                                                                                Color.FromArgb(&HB4000080), _
                                                                                Color.FromArgb(&HB4008000), _
                                                                                Color.FromArgb(&HB4008080), _
                                                                                Color.FromArgb(&HB4800000), _
                                                                                Color.FromArgb(&HB4800080), _
                                                                                Color.FromArgb(&HB4808000), _
                                                                                Color.FromArgb(&HB4808080), _
                                                                                Color.FromArgb(&HB4C0C0C0), _
                                                                                Color.FromArgb(&HB40000FF), _
                                                                                Color.FromArgb(&HB400FF00), _
                                                                                Color.FromArgb(&HB400FFFF), _
                                                                                Color.FromArgb(&HB4FF0000), _
                                                                                Color.FromArgb(&HB4FF00FF), _
                                                                                Color.FromArgb(&HB4FFFF00), _
                                                                                Color.FromArgb(&HB4000000)}, _
                                                                               {Color.FromArgb(&HFF000000), _
                                                                                Color.FromArgb(&HB43232C8), _
                                                                                Color.FromArgb(&HB432C832), _
                                                                                Color.FromArgb(&HB432C8C8), _
                                                                                Color.FromArgb(&HB4C83232), _
                                                                                Color.FromArgb(&HB4C832C8), _
                                                                                Color.FromArgb(&HB4C8C832), _
                                                                                Color.FromArgb(&HB4C8C8C8), _
                                                                                Color.FromArgb(&HB4646464), _
                                                                                Color.FromArgb(&HB40000FF), _
                                                                                Color.FromArgb(&HB400FF00), _
                                                                                Color.FromArgb(&HB400FFFF), _
                                                                                Color.FromArgb(&HB4FF0000), _
                                                                                Color.FromArgb(&HB4FF00FF), _
                                                                                Color.FromArgb(&HB4FFFF00), _
                                                                                Color.FromArgb(&HB4FFFFFF)}, _
                                                                               {Color.FromArgb(&HFFEEEECE), _
                                                                                Color.FromArgb(&HB4000080), _
                                                                                Color.FromArgb(&HB4008000), _
                                                                                Color.FromArgb(&HB4008080), _
                                                                                Color.FromArgb(&HB4800000), _
                                                                                Color.FromArgb(&HB4800080), _
                                                                                Color.FromArgb(&HB4808000), _
                                                                                Color.FromArgb(&HB4808080), _
                                                                                Color.FromArgb(&HB4C0C0C0), _
                                                                                Color.FromArgb(&HB40000FF), _
                                                                                Color.FromArgb(&HB400FF00), _
                                                                                Color.FromArgb(&HB400FFFF), _
                                                                                Color.FromArgb(&HB4FF0000), _
                                                                                Color.FromArgb(&HB4FF00FF), _
                                                                                Color.FromArgb(&HB4FFFF00), _
                                                                                Color.FromArgb(&HB4000000)}}


        Public Sub New()

        End Sub

        ''' <summary>
        ''' Loads  .lbr library from file
        ''' </summary>
        ''' <param name="FileName"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal FileName As String)
            Load(FileName)
        End Sub

        Public Sub New(ByVal Xml As Xml.XmlDocument, ByVal ShortFileName As String)
            Load(Xml, ShortFileName)
        End Sub

        Public Sub New(ByVal XmlText As String, ByVal ShortFileName As String)
            LoadXml(XmlText, ShortFileName)
        End Sub

        ''' <summary>
        ''' Loads an eagle project, returns true on success
        ''' </summary>
        ''' <param name="FileName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load(ByVal FileName As String) As Boolean
            Dim Xml As New Xml.XmlDocument()
            Xml.XmlResolver = Nothing
            m_FileName = FileName
            Try
                Xml.Load(FileName)
            Catch e As Exception
                Return False
            End Try
            Return Load(Xml, System.IO.Path.GetFileName(FileName))
        End Function

        Public Function Load(ByVal Xml As Xml.XmlDocument, ByVal ShortFileName As String) As Boolean
            If Xml.ChildNodes.Count > 0 Then
                m_Xml = Xml
                m_ShortName = ShortFileName
                m_Drawing = New Drawing(Me, Xml.SelectSingleNode("/eagle/drawing"))
                Return True
            End If
            Return False
        End Function

        Public Function LoadXml(ByVal XmlText As String, ByVal ShortFileName As String) As Boolean
            Dim Xml As New Xml.XmlDocument
            Xml.XmlResolver = Nothing
            Try
                Xml.LoadXml(XmlText)
            Catch e As Exception
                Return False
            End Try
            Return Load(Xml, ShortFileName)
        End Function

        ''' <summary>
        ''' Saves only the previous loaded xml document
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SaveXml() As String
            Dim XMlStream As New System.IO.MemoryStream
            Dim Reader As New System.IO.StreamReader(XMlStream)
            m_Xml.Save(XMlStream)
            XMlStream.Seek(0, IO.SeekOrigin.Begin)
            Return Reader.ReadToEnd()
        End Function


        ''' <summary>
        ''' Only possible for schematic projects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SaveSchematic() As String
            Dim Xml As XmlDocument = SaveSchematicXmlDoc()
            Dim Stream As New System.IO.MemoryStream
            Dim StreamReader As New System.IO.StreamReader(Stream)
            Xml.Save(Stream)
            Stream.Seek(0, IO.SeekOrigin.Begin)
            Return StreamReader.ReadToEnd()
        End Function

        Public Function SaveSchematic(ByVal ToFile As String) As Boolean
            Dim FileStream As New System.IO.FileStream(ToFile, IO.FileMode.Create)
            Dim Xml As XmlDocument = SaveSchematicXmlDoc()
            Xml.Save(FileStream)
            FileStream.Close()
            Return True
        End Function

        Public Function SaveSchematicXmlDoc() As XmlDocument
            Dim Xml As New XmlDocument
            Xml.AppendChild(Xml.CreateXmlDeclaration("1.0", "utf-8", ""))
            Xml.XmlResolver = Nothing
            Xml.AppendChild(Xml.CreateDocumentType("eagle", "SYSTEM", "eagle.dtd", ""))
            Me.WriteXml(Xml, Xml.AppendChild(Xml.CreateElement(Me.NodeName())))
            Return Xml
        End Function

        ''' <summary>
        ''' Returns a color for a color number from the current used pallete
        ''' </summary>
        ''' <param name="ColorNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConvertColor(ByVal ColorNumber As Integer) As System.Drawing.Color
            Return m_Colors(m_Pallete, ColorNumber)
        End Function

        ''' <summary>
        ''' Returns the selected color for a non selected color number
        ''' </summary>
        ''' <param name="NormalColorNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConvertSelectedColor(ByVal NormalColorNumber As Integer) As System.Drawing.Color
            Return m_Colors(m_Pallete, NormalColorNumber + 8)
        End Function

        Public Property Font() As FontFamily
            Get
                Return New FontFamily("Arial")
            End Get
            Set(ByVal value As FontFamily)

            End Set
        End Property

        ''' <summary>
        ''' Creates and add a new drawing object to the project
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDrawing() As Drawing
            m_Drawing = New Drawing(Me)
            Return m_Drawing
        End Function

        ''' <summary>
        ''' Creates and add a new drawing object to the project using a defined grid
        ''' </summary>
        ''' <param name="Grid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDrawing(ByVal Grid As Grid) As Drawing
            m_Drawing = New Drawing(Me, Grid)
            Return m_Drawing
        End Function

        ''' <summary>
        ''' Returns the drawing object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Drawing() As Drawing
            Get
                Return m_Drawing
            End Get
        End Property

        ''' <summary>
        ''' Returns short filename (without full path)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ShortFileName() As String
            Get
                Return m_ShortName
            End Get
        End Property

        ''' <summary>
        ''' Returns full path of the project file
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FileName() As String
            Get
                Return m_FileName
            End Get
        End Property

        Public Sub WriteXml(ByVal Doc As System.Xml.XmlDocument, ByVal Node As System.Xml.XmlNode) Implements EagleSaveable.WriteXml
            Node.Attributes.Append(Doc.CreateAttribute("version")).Value = EagleVersion
            Drawing.WriteXml(Doc, Node.AppendChild(Doc.CreateElement(Drawing.NodeName())))
        End Sub

        Public ReadOnly Property NodeName() As String Implements EagleSaveable.NodeName
            Get
                Return "eagle"
            End Get
        End Property
    End Class

    Module Functions

        ''' <summary>
        ''' Returns the angle in degrees between point1 and the center. Uses Atan to determine the exact angle
        ''' </summary>
        ''' <param name="Center">Center point of the circle</param>
        ''' <param name="Radius">Radius of the circle</param>
        ''' <param name="Point1">Point on the circle to get the angle to horizontal X axes</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAngle(ByVal Center As PointF, ByVal Radius As Single, ByVal Point1 As PointF) As Single
            'Dim Angle As Single = 
            'If Angle < 0 Then Angle += 360
            Return (Atan2(Point1.Y - Center.Y, Point1.X - Center.X) * 180 / PI + 360) Mod 360
        End Function

        Public Function DistanceF(ByVal Point1 As PointF, ByVal Point2 As PointF) As Single
            Return Math.Sqrt((Point1.X - Point2.X) ^ 2 + (Point1.Y - Point2.Y) ^ 2)
        End Function

        Public Function toSingle(ByVal Value As String) As Single
            Return Convert.ToSingle(Value, System.Globalization.NumberFormatInfo.InvariantInfo)
        End Function

        Public Function ParseBool(ByVal bool As String) As Boolean
            Return bool.ToLower() = "yes" OrElse bool.ToLower() = "true"
        End Function

        Public Function ToEagleBool(ByVal bool As Boolean) As String
            If bool Then Return "yes"
            Return "no"
        End Function

        Public Function ToEagleSingle(ByVal Val As Single) As String
            Return Val.ToString("G", System.Globalization.CultureInfo.InvariantCulture)
            'Return Replace(Val.ToString(), ",", ".")
        End Function

        Public Sub FillRoundedRectangle(ByVal Graphics As Graphics, ByVal Rectangle As RectangleF, ByVal brush As Brush, ByVal radius As Single)
            Graphics.FillPath(brush, RoundedRectangle(Rectangle, radius))
        End Sub

        Public Function RotateLocation(ByVal Point As PointF, ByVal Angle As Single) As PointF
            Return New PointF(Point.X * Cos(Angle) - Point.Y * Sin(Angle), Point.X * Sin(Angle) + Point.Y * Cos(Angle))
        End Function

        Public Function RoundedRectangle(ByVal r As RectangleF, ByVal radius As Single) As System.Drawing.Drawing2D.GraphicsPath
            Dim path As New System.Drawing.Drawing2D.GraphicsPath()
            Dim d As Single = radius * 2
            path.AddLine(r.Left + d, r.Top, r.Right - d, r.Top)
            path.AddArc(System.Drawing.RectangleF.FromLTRB(r.Right - d, r.Top, r.Right, r.Top + d), -90, 90)
            path.AddLine(r.Right, r.Top + d, r.Right, r.Bottom - d)
            path.AddArc(System.Drawing.RectangleF.FromLTRB(r.Right - d, r.Bottom - d, r.Right, r.Bottom), 0, 90)
            path.AddLine(r.Right - d, r.Bottom, r.Left + d, r.Bottom)
            path.AddArc(System.Drawing.RectangleF.FromLTRB(r.Left, r.Bottom - d, r.Left + d, r.Bottom), 90, 90)
            path.AddLine(r.Left, r.Bottom - d, r.Left, r.Top + d)
            path.AddArc(System.Drawing.RectangleF.FromLTRB(r.Left, r.Top, r.Left + d, r.Top + d), 180, 90)
            path.CloseFigure()
            Return path
        End Function
    End Module
End Namespace