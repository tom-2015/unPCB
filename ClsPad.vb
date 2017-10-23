Imports System.Globalization
Imports System.Drawing
Imports System.ComponentModel

Public Class Pad
    Inherits SelectableLayerObject
    Implements System.ICloneable

    Public Const PadDefaultColor As Int32 = &HFFA54B4B

    Protected m_Pen As System.Drawing.Pen
    Protected m_Brush As System.Drawing.SolidBrush
    Protected m_DevicePin As DevicePin 'pin of device connected to
    Protected m_EagleDevicePadName As String
    Protected m_Routes As New List(Of Route) 'connected routes

    Public Sub New()
        MyBase.New()
        m_Color = System.Drawing.Color.FromArgb(PadDefaultColor)
        m_Pen = New System.Drawing.Pen(m_Color)
    End Sub

    Public Sub New(ByVal location As System.Drawing.PointF)
        MyBase.New(location)
        m_Color = System.Drawing.Color.FromArgb(PadDefaultColor)
        m_Pen = New System.Drawing.Pen(m_Color)
    End Sub

    Public Sub New(ByVal location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(location, Width, Height)
        m_Color = System.Drawing.Color.FromArgb(PadDefaultColor)
        m_Pen = New System.Drawing.Pen(m_Color)
        m_Name = "pad_" & m_id
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    'Not selected color
    '<CategoryAttribute("Color"), _
    ' DefaultValueAttribute(PadDefaultColor)> _
    Public Overrides Property Color() As System.Drawing.Color
        Get
            Return m_Pen.Color
        End Get
        Set(ByVal value As System.Drawing.Color)
            m_Color = value
            m_Pen.Color = value
        End Set
    End Property

    'Draws the pad on the needed layers
    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        If m_Selected Then
            m_Pen = New Pen(m_SelectedColor)
            m_Brush = New SolidBrush(m_SelectedColor)
        Else
            If m_Highlighted Then
                m_Pen = New Pen(m_HighlightColor)
                m_Brush = New SolidBrush(m_HighlightColor)
            Else
                m_Pen = New Pen(m_Color)
                m_Brush = New SolidBrush(m_Color)
            End If
        End If
    End Sub

    Public Property DevicePin() As DevicePin
        Get
            Return m_DevicePin
        End Get
        Set(ByVal value As DevicePin)
            m_DevicePin = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the name of the pad of the eagle device this pad is connected with
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property EaglePadName() As String
        Get
            Return m_EagleDevicePadName
        End Get
        Set(ByVal value As String)
            m_EagleDevicePadName = value
        End Set
    End Property

    Public Overrides Sub AddToPCB(ByVal PCB As PCB)
        MyBase.AddToPCB(PCB)
        PCB.ConnectionMatrix.AddPad(Me)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, m_Center)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, m_Rect.Location)
    End Sub

    ''' <summary>
    ''' Called by PCB class when deleting the object by the user, class has to create undo information and adds to project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoDeleteItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoDeletePad(PCB, Me)
    End Function

    ''' <summary>
    ''' Called by PCB class before placing the object to the PCB
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <returns>Returns a undo redo stack item to undo adding the object</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetUndoAddItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoAddPad(PCB, Me)
    End Function

    Public Overrides Function GetProperties() As LayerObjectProperties
        Return New PadProperties(m_PCB, Me)
    End Function

    ''' <summary>
    ''' Called by the PCB class when removing the object from the project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Remove()
        MyBase.Remove()
        'delete all the routes
        Dim TmpRoutes() As Route = m_Routes.ToArray()
        For Each Route As Route In TmpRoutes
            m_PCB.RemoveObject(Route)
        Next
        m_PCB.ConnectionMatrix.RemovePad(Me)
        If DevicePin IsNot Nothing Then
            'DevicePin.Pads.Remove(Me)
            DevicePin.RemovePad(Me)
        End If
        m_Routes = New List(Of Route)
        For Each Route As Route In TmpRoutes 'for making undo delete device work properly
            m_Routes.Add(Route)
        Next
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("eagle_device_pad_name")).Value = m_EagleDevicePadName
    End Sub

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        m_EagleDevicePadName = Root.Attributes("eagle_device_pad_name").Value
    End Sub

    Public Overrides Function Clone() As Object
        Dim Obj As Object = MyBase.Clone()
        CType(Obj, Pad).m_Name = "pad_" & CType(Obj, Pad).m_id
        CType(Obj, Pad).m_Routes = New List(Of Route)
        CType(Obj, Pad).m_DevicePin = Nothing
        Return Obj
    End Function

    Public ReadOnly Property Routes() As List(Of Route)
        Get
            Return m_Routes
        End Get
    End Property

    ''' <summary>
    ''' Returns all pads that are connected to this pad by the connected routes
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetConnectedPadsByRoutes() As List(Of Pad)
        Dim Pads As New List(Of Pad)
        Pads.Add(Me)
        AddConnectedPadsByRoutes(Pads)
        Return Pads
    End Function

    Protected Sub AddConnectedPadsByRoutes(ByVal Lst As List(Of Pad))
        For Each Route As Route In m_Routes
            If Not Lst.Contains(Route.StartPad) Then
                Lst.Add(Route.StartPad)
                Route.StartPad.AddConnectedPadsByRoutes(Lst)
            End If
            If Not Lst.Contains(Route.EndPad) Then
                Lst.Add(Route.EndPad)
                Route.EndPad.AddConnectedPadsByRoutes(Lst)
            End If
        Next
    End Sub

End Class

'http://msdn.microsoft.com/en-us/library/aa302326.aspx
'<DefaultPropertyAttribute("SaveOnClose")> _
Public Class SMDPad
    Inherits Pad

    Public Const SMDPadDefaultWidth As Integer = 10
    Public Const SMDPadDefaultHeight As Integer = 8

    Public Sub New()
        MyBase.New(New PointF(0, 0), SMDPadDefaultWidth, SMDPadDefaultHeight)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        Me.New(Location, 0, 0)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(Location, Width, Height)
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        If Not m_Visible Then Return
        MyBase.Draw(Graphics, Layer)
        Graphics.FillRectangle(m_Brush, m_Rect)
    End Sub

    ''' <summary>
    ''' Places the object on the current location
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, m_Rect.Location)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location)
        If WindowType = PCB.WindowTypes.WindowTypeBottom Then
            PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomPads).LayerObjects.Add(Me)
        Else
            PCB.GetLayer(PCB.LayerTypes.LayerTypeTopPads).LayerObjects.Add(Me)
        End If
    End Sub

    Public Overloads Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Layer As PCB.LayerTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location)
        PCB.GetLayer(Layer).LayerObjects.Add(Me)
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
    End Sub

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
    End Sub

End Class

Public Class ThroughHolePad
    Inherits Pad

    Public Const ThroughHolePadDefaultRadius As Single = 12

    Public Sub New()
        MyBase.New(New Point(0, 0), ThroughHolePadDefaultRadius, ThroughHolePadDefaultRadius)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        Me.New(Location, 0, 0)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(Location, Width, Height)
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        If Not m_Visible Then Return
        MyBase.Draw(Graphics, Layer)
        Graphics.FillEllipse(m_Brush, m_Rect)
    End Sub

    ''' <summary>
    ''' Places the object on the current location
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, m_Rect.Location)
    End Sub

    ''' <summary>
    ''' Places the object on a center location, keeping the current width and height
    ''' </summary>
    ''' <param name="PCB">PCB project</param>
    ''' <param name="Location">Center of where to place the object</param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        PlaceObject(PCB, WindowType, Location, PCB.LayerTypes.LayerTypeTopPads, PCB.LayerTypes.LayerTypeBottomPads)
    End Sub

    Protected Overloads Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF, ByVal TopLayer As PCB.LayerTypes, ByVal BottomLayer As PCB.LayerTypes)
        MyBase.PlaceObject(PCB, WindowType, Location)
        PCB.GetLayer(TopLayer).LayerObjects.Add(Me)
        PCB.GetLayer(BottomLayer).LayerObjects.Add(Me)
    End Sub

    ''' <summary>
    ''' Automatic scaling of the object
    ''' </summary>
    ''' <param name="Window">The window the object scaled on</param>
    ''' <param name="StartPoint">Point clicked by left mouse button at start of drawing (x,y on screen)</param>
    ''' <param name="CurrentPoint">Current point the mouse is down at (x,y on screen)</param>
    ''' <remarks></remarks>
    Public Overrides Sub AutoScale(ByVal Window As FrmLayer, ByVal StartPoint As PointF, ByVal CurrentPoint As PointF)
        Dim Rad As Integer = Distance(CurrentPoint, StartPoint)
        If Rad <= 0 Then Rad = 1

        m_Rect.Width = Rad * 2
        m_Rect.Height = Rad * 2
        Me.Center = New Point(StartPoint.X, StartPoint.Y)
    End Sub

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
    End Sub

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
    End Sub

End Class

Public Class Via
    Inherits ThroughHolePad

    Public Sub New()
        MyBase.New(New Point(0, 0), ThroughHolePadDefaultRadius, ThroughHolePadDefaultRadius)
        m_Color = Drawing.Color.Purple
        m_Name = "via_" & m_id
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        Me.New(Location, 0, 0)
        m_Color = Drawing.Color.Purple
        m_Name = "via_" & m_id
    End Sub

    ''' <summary>
    ''' Places the object on a center location, keeping the current width and height
    ''' </summary>
    ''' <param name="PCB">PCB project</param>
    ''' <param name="Location">Center of where to place the object</param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location, unPCB.PCB.LayerTypes.LayerTypeTopVias, unPCB.PCB.LayerTypes.LayerTypeBottomVias)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(Location, Width, Height)
        m_Color = Drawing.Color.Purple
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Function Clone() As Object
        Dim Obj As Via = CType(MyBase.Clone(), Via)
        Obj.m_Name = "via_" & Obj.m_id
        Return Obj
    End Function

End Class

Public Class RouteJunction
    Inherits SMDPad

    Public Const RouteJunctionDefaultSize As Integer = 5

    Public Sub New()
        MyBase.New(New Point(0, 0), RouteJunctionDefaultSize, RouteJunctionDefaultSize)
        m_Color = Drawing.Color.Purple
        m_Name = "junction_" & m_id
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        Me.New(Location, RouteJunctionDefaultSize, RouteJunctionDefaultSize)
        m_Color = Drawing.Color.Purple
        m_Name = "junction_" & m_id
    End Sub

    ''' <summary>
    ''' Places the object on a pcb, keeping the current width and height
    ''' </summary>
    ''' <param name="PCB">PCB project</param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, WindowType, m_Rect.Location)
    End Sub

    ''' <summary>
    ''' Places the object on a center location, keeping the current width and height
    ''' </summary>
    ''' <param name="PCB">PCB project</param>
    ''' <param name="Location">Center of where to place the object</param>
    ''' <remarks></remarks>
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, IIf(WindowType = unPCB.PCB.WindowTypes.WindowTypeTop, PCB.LayerTypes.LayerTypeTopRoute, PCB.LayerTypes.LayerTypeBottomRoute), Location)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Single, ByVal Height As Single)
        MyBase.New(Location, Width, Height)
        m_Color = Drawing.Color.Purple
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Function Clone() As Object
        Dim Obj As RouteJunction = CType(MyBase.Clone(), RouteJunction)
        Obj.m_Name = "junction_" & Obj.m_id
        Return Obj
    End Function

End Class

Public Class PadProperties
    Inherits LayerObjectProperties

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB, Obj)
    End Sub

    Public Overridable ReadOnly Property EaglePadName() As String
        Get
            Return CType(m_Object, Pad).EaglePadName
        End Get
    End Property

End Class