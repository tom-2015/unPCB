Imports System.ComponentModel

''' <summary>
''' General class for an object that can be placed on a layer
''' </summary>
''' <remarks></remarks>
Public Class LayerObject
    Implements ICloneable

    Protected Shared m_next_id As Integer = 1
    Protected Shared ObjectIds As Dictionary(Of Integer, LayerObject)

    Protected m_id As Integer
    Protected m_Rect As System.Drawing.RectangleF 'rectangle the object fits in 
    Protected m_Center As System.Drawing.PointF   'center of gravity location of the object
    Protected m_Visible As Boolean
    Protected m_Name As String 'name of the pad
    Protected WithEvents m_PCB As PCB 'once the object is placed, this will refer to the pcb project, needed to check for name existance when changing the name property
    Protected m_WindowType As PCB.WindowTypes
    Protected m_Rotation As Single 'rotation of the object

    Public Event NameChanged(ByVal Sender As LayerObject, ByVal OldName As String, ByRef NewName As String)
    Public Event Move(ByVal Sender As LayerObject, ByVal Center As PointF) 'if object is moved

    Public Sub New()
        Me.New(New Point(0, 0))
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        Me.New(Location, 0, 0)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Integer, ByVal Height As Integer)
        m_Center = New PointF(Location.X + Width / 2, Location.Y + Height / 2)
        m_Rect = New RectangleF(Location, New Size(Width, Height))
        m_Visible = True
        m_id = m_next_id
        m_next_id += 1
        m_Name = "obj_" & m_id
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    'renders the object to the layer (or all layers from the PCB project if needed) 
    Public Overridable Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)

    End Sub

    ''' <summary>
    ''' Called when the object is added to a PCB project
    ''' </summary>
    ''' <param name="PCB">the project the object is added to</param>
    ''' <remarks></remarks>
    Public Overridable Sub AddToPCB(ByVal PCB As PCB)
        m_PCB = PCB
    End Sub

    ''' <summary>
    ''' Called when the object is placed at it's current internal location
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="WindowType"></param>
    ''' <remarks></remarks>
    Public Overridable Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, m_Rect.Location)
    End Sub

    ''' <summary>
    ''' Called when the user places the object on the PCB at a certain position
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="WindowType"></param>
    ''' <param name="Location"></param>
    ''' <remarks></remarks>
    Public Overridable Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        Me.Location = Location
        m_WindowType = WindowType
    End Sub

    ''' <summary>
    ''' Called by PCB class when deleting the object by the user, class has to create undo information and adds to project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Function GetUndoDeleteItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoDeleteObject(PCB, Me)
    End Function

    ''' <summary>
    ''' Called by PCB class before the user places the object on the PCB
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Returns a undo redo stack item to undo adding the object</remarks>
    Public Overridable Function GetUndoAddItem(ByVal PCB As PCB) As UndoRedoItem
        Return New UndoRedoAddObject(PCB, Me)
    End Function

    ''' <summary>
    ''' Called when the user repositions the object, returns the undoredo position object item
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetUndoPositionItem(ByVal PCB As PCB) As UndoRedoItem
        Return Nothing
    End Function

    ''' <summary>
    ''' Called by the PCB class when removing the object from the project
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Remove()
        For Each Layer As KeyValuePair(Of PCB.LayerTypes, Layer) In m_PCB.Layers
            Layer.Value.LayerObjects.Remove(Me)
        Next
    End Sub

    ''' <summary>
    ''' Automatic scaling of the object
    ''' </summary>
    ''' <param name="Window">The window the object scaled on</param>
    ''' <param name="StartPoint">Point clicked by left mouse button at start of drawing (x,y on screen)</param>
    ''' <param name="CurrentPoint">Current point the mouse is down at (x,y on screen)</param>
    ''' <remarks></remarks>
    Public Overridable Sub AutoScale(ByVal Window As FrmLayer, ByVal StartPoint As PointF, ByVal CurrentPoint As PointF)
        m_Rect.Width = Math.Abs(StartPoint.X - CurrentPoint.X)
        m_Rect.Height = Math.Abs(StartPoint.Y - CurrentPoint.Y)

        If CurrentPoint.X < StartPoint.X Then
            m_Rect.X = CurrentPoint.X
        Else
            m_Rect.X = StartPoint.X
        End If

        If CurrentPoint.Y < StartPoint.Y Then
            m_Rect.Y = CurrentPoint.Y
        Else
            m_Rect.Y = StartPoint.Y
        End If
        m_Center.X = m_Rect.X + m_Rect.Width / 2
        m_Center.Y = m_Rect.Y + m_Rect.Height / 2

        RaiseEvent Move(Me, m_Center)

    End Sub

    ''' <summary>
    ''' Returns the unique ID of the layer object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property id() As Integer
        Get
            Return m_id
        End Get
    End Property

    ''' <summary>
    ''' returns the drawing location (X,Y start of the rect)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Location() As System.Drawing.PointF
        Get
            Return m_Rect.Location
        End Get
        Set(ByVal value As System.Drawing.PointF)
            m_Rect.Location = value
            m_Center.X = value.X + m_Rect.Width / 2
            m_Center.Y = value.Y + m_Rect.Height / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property


    ''' <summary>
    ''' Returns the rectangle the object fits in
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <BrowsableAttribute(False)> _
    Public Overridable ReadOnly Property Rect() As System.Drawing.RectangleF
        Get
            Return m_Rect
        End Get
    End Property

    ''' <summary>
    ''' Gets / sets the center point of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <BrowsableAttribute(False)> _
    Public Overridable Property Center() As System.Drawing.PointF
        Get
            Return m_Center
        End Get
        Set(ByVal value As System.Drawing.PointF)
            m_Center = value
            m_Rect.X = m_Center.X - m_Rect.Width / 2
            m_Rect.Y = m_Center.Y - m_Rect.Height / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets  the center X location of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property CenterX() As Single
        Get
            Return m_Center.X
        End Get
        Set(ByVal value As Single)
            m_Center.X = value
            m_Rect.X = m_Center.X - m_Rect.Width / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the center Y of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property CenterY() As Single
        Get
            Return m_Center.Y
        End Get
        Set(ByVal value As Single)
            m_Center.Y = value
            m_Rect.Y = m_Center.Y - m_Rect.Height / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the height of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Height() As Single
        Get
            Return m_Rect.Height
        End Get
        Set(ByVal value As Single)
            If value <= 0 Then Return
            m_Rect.Height = value
            m_Center.Y = m_Rect.Y + m_Rect.Height / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the width of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Width() As Single
        Get
            Return m_Rect.Width
        End Get
        Set(ByVal value As Single)
            If value <= 0 Then Return
            m_Rect.Width = value
            m_Center.X = m_Rect.X + m_Rect.Width / 2
            RaiseEvent Move(Me, m_Center)
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets if the object is visible
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Visible() As Boolean
        Get
            Return m_Visible
        End Get
        Set(ByVal value As Boolean)
            m_Visible = value
        End Set
    End Property

    ''' <summary>
    ''' Gets / Sets the name of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            RaiseEvent NameChanged(Me, m_Name, value)
            m_Name = value
        End Set
    End Property


    Public Overridable ReadOnly Property WindowType() As PCB.WindowTypes
        Get
            Return m_WindowType
        End Get
    End Property

    ''' <summary>
    ''' Returns a layerobjectproperties class which is used at the property grid window to edit some properties of this object
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetProperties() As LayerObjectProperties
        Return New LayerObjectProperties(m_PCB, Me)
    End Function

    ''' <summary>
    ''' Returns the rotation of the object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Rotation() As Single
        Get
            Return m_Rotation
        End Get
        Set(ByVal value As Single)
            m_Rotation = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the distance of the object to a point
    ''' </summary>
    ''' <param name="ToPoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetDistance(ByVal ToPoint As PointF) As Single
        Return Distance(m_Center, ToPoint)
    End Function


    ''' <summary>
    ''' Makes a clone of the object, adjusts the name to a unique value
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function Clone() As Object Implements System.ICloneable.Clone
        Dim Obj As Object = Me.MemberwiseClone()
        CType(Obj, LayerObject).m_id = m_next_id
        CType(Obj, LayerObject).m_Name = "obj_" & m_next_id
        m_next_id = m_next_id + 1
        Return Obj
    End Function

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Root.Attributes.Append(XMLDoc.CreateAttribute("id")).Value = m_id
        Root.Attributes.Append(XMLDoc.CreateAttribute("type")).Value = TypeName(Me)
        Root.Attributes.Append(XMLDoc.CreateAttribute("x")).Value = SingleToString(m_Rect.Left)
        Root.Attributes.Append(XMLDoc.CreateAttribute("y")).Value = SingleToString(m_Rect.Top)
        Root.Attributes.Append(XMLDoc.CreateAttribute("width")).Value = SingleToString(m_Rect.Width)
        Root.Attributes.Append(XMLDoc.CreateAttribute("height")).Value = SingleToString(m_Rect.Height)
        Root.Attributes.Append(XMLDoc.CreateAttribute("visible")).Value = m_Visible
        Root.Attributes.Append(XMLDoc.CreateAttribute("name")).Value = m_Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("center_x")).Value = SingleToString(m_Center.X)
        Root.Attributes.Append(XMLDoc.CreateAttribute("center_y")).Value = SingleToString(m_Center.Y)
        Root.Attributes.Append(XMLDoc.CreateAttribute("windowtype")).Value = [Enum].GetName(GetType(PCB.WindowTypes), m_WindowType)
        Root.Attributes.Append(XMLDoc.CreateAttribute("rotation")).Value = SingleToString(m_Rotation)
    End Sub

    ''' <summary>
    ''' Converts from xml
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overridable Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        m_id = Root.Attributes("id").Value
        m_Rect.X = StringToSingle(Root.Attributes("x").Value)
        m_Rect.Y = StringToSingle(Root.Attributes("y").Value)
        m_Rect.Width = StringToSingle(Root.Attributes("width").Value)
        m_Rect.Height = StringToSingle(Root.Attributes("height").Value)
        m_Visible = Root.Attributes("visible").Value
        m_WindowType = [Enum].Parse(GetType(PCB.WindowTypes), Root.Attributes("windowtype").Value)
        m_Name = Root.Attributes("name").Value
        m_Center.X = m_Rect.X + m_Rect.Width / 2
        m_Center.Y = m_Rect.Y + m_Rect.Height / 2
        m_next_id = Math.Max(m_id, m_next_id) + 1
        m_Rotation = StringToSingle(Root.Attributes("rotation").Value)
        m_PCB = PCB
    End Sub

    Public Overrides Function GetHashCode() As Integer
        Return m_id
        'MyBase.GetHashCode()
    End Function


End Class

''' <summary>
''' Represents layer objects that can be selected
''' </summary>
''' <remarks></remarks>
Public Class SelectableLayerObject
    Inherits LayerObject

    Protected m_Highlighted As Boolean 'if we are not selected, do we draw as highlighted?
    Protected m_Selected As Boolean 'do we need to draw as selected (use selected color)?
    Protected m_SelectedColor As System.Drawing.Color 'use this color if we are selected
    Protected m_Color As System.Drawing.Color 'default unselected color
    Protected m_HighlightColor As System.Drawing.Color
    Protected m_StartCenter As PointF 'start center for moving operations

    Public Const DefaultSelectedColor As Int32 = &HFFFF0000
    Public Const DefaultHighlightColor As Int32 = &HFFFFFF00

    Public Sub New()
        MyBase.New()
        m_SelectedColor = System.Drawing.Color.FromArgb(DefaultSelectedColor)
        m_HighlightColor = System.Drawing.Color.FromArgb(DefaultHighlightColor)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        MyBase.New(Location)
        m_SelectedColor = System.Drawing.Color.FromArgb(DefaultSelectedColor)
        m_HighlightColor = System.Drawing.Color.FromArgb(DefaultHighlightColor)
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Integer, ByVal Height As Integer)
        MyBase.New(Location, Width, Height)
        m_SelectedColor = System.Drawing.Color.FromArgb(DefaultSelectedColor)
        m_HighlightColor = System.Drawing.Color.FromArgb(DefaultHighlightColor)
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        MyBase.Draw(Graphics, Layer)
    End Sub

    'Public Overridable Function GetDistance(ByVal ToPoint As PointF) As Single
    '    Return Distance(m_Center, ToPoint)
    'End Function

    'selected or not?
    <BrowsableAttribute(False)> _
    Public Property Selected() As Boolean
        Get
            Selected = m_Selected
        End Get
        Set(ByVal value As Boolean)
            m_Selected = value
        End Set
    End Property

    'The color to use when selected
    <BrowsableAttribute(False)> _
    Public Overridable Property SelectedColor() As System.Drawing.Color
        Get
            Return m_SelectedColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            m_SelectedColor = value
        End Set
    End Property

    <BrowsableAttribute(False)> _
    Public Overridable Property HighlightColor() As System.Drawing.Color
        Get
            Return m_HighlightColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            m_HighlightColor = value
        End Set
    End Property

    <BrowsableAttribute(False)> _
    Public Overridable Property Highlighted() As Boolean
        Get
            Return m_Highlighted
        End Get
        Set(ByVal value As Boolean)
            m_Highlighted = value
        End Set
    End Property

    Public Overridable Property Color() As Color
        Get
            Return m_Color
        End Get
        Set(ByVal value As Color)
            m_Color = value
        End Set
    End Property

    <BrowsableAttribute(False)> _
    Public Property StartCenter() As PointF
        Get
            Return m_StartCenter
        End Get
        Set(ByVal value As PointF)
            m_StartCenter = value
        End Set
    End Property

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overrides Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("color")).Value = m_Color.ToArgb()
        Root.Attributes.Append(XMLDoc.CreateAttribute("selectedcolor")).Value = m_SelectedColor.ToArgb()
        'Root.Attributes.Append(XMLDoc.CreateAttribute("selected")).Value = m_Selected
        Root.Attributes.Append(XMLDoc.CreateAttribute("highlightcolor")).Value = m_HighlightColor.ToArgb()
    End Sub

    ''' <summary>
    ''' Converts from xml
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        m_Color = Color.FromArgb(Root.Attributes("color").Value)
        m_SelectedColor = Color.FromArgb(Root.Attributes("selectedcolor").Value)
        m_HighlightColor = Color.FromArgb(Root.Attributes("highlightcolor").Value)
    End Sub
End Class

Public Class Line
    Inherits SelectableLayerObject

    Dim m_Points(0 To 1) As PointF
    Dim m_Pen As Pen
    Dim m_SelectedPen As Pen
    Dim m_HighlightedPen As Pen

    Public Sub New()
        MyBase.New()
        Init()
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        MyBase.New(Location)
        Init()
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Integer, ByVal Height As Integer)
        MyBase.New(Location, Width, Height)
        Init()
    End Sub

    Protected Sub Init()
        m_Color = Drawing.Color.Purple
        m_SelectedColor = Drawing.Color.PaleVioletRed
        m_HighlightColor = Drawing.Color.Blue
        m_Pen = New System.Drawing.Pen(m_Color, 1)
        m_SelectedPen = New Pen(m_SelectedColor, 1)
        m_HighlightedPen = New Pen(m_HighlightColor, 1)
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        Dim Pen As Pen = m_Pen
        If m_Selected Then
            Pen = m_SelectedPen
        End If
        If m_Highlighted Then
            Pen = m_HighlightedPen
        End If
        Graphics.DrawLine(Pen, m_Points(0), m_Points(1))
    End Sub

    Public Property Points(ByVal Index As Integer) As PointF
        Get
            Return m_Points(Index)
        End Get
        Set(ByVal Point As PointF)
            m_Points(Index) = Point
        End Set
    End Property

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overrides Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("x1")).Value = SingleToString(m_Points(0).X)
        Root.Attributes.Append(XMLDoc.CreateAttribute("y1")).Value = SingleToString(m_Points(0).Y)
        Root.Attributes.Append(XMLDoc.CreateAttribute("x2")).Value = SingleToString(m_Points(1).X)
        Root.Attributes.Append(XMLDoc.CreateAttribute("y2")).Value = SingleToString(m_Points(1).Y)
    End Sub

    ''' <summary>
    ''' Converts from xml
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        m_Points(0).X = StringToSingle(Root.Attributes("x1").Value)
        m_Points(0).Y = StringToSingle(Root.Attributes("y1").Value)
        m_Points(1).X = StringToSingle(Root.Attributes("x2").Value)
        m_Points(1).Y = StringToSingle(Root.Attributes("y2").Value)
    End Sub


End Class

''' <summary>
''' This class provides an interface between the layerobject and the property grid
''' </summary>
''' <remarks></remarks>
Public Class LayerObjectProperties

    Protected m_Object As LayerObject
    Protected m_PCB As PCB

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        m_Object = Obj
        m_PCB = PCB
    End Sub

    Public Overridable ReadOnly Property Location() As PointF
        Get
            Return m_Object.Location
        End Get
    End Property

    Public Overridable ReadOnly Property Size() As SizeF
        Get
            Return m_Object.Rect.Size
        End Get
    End Property

    Public Overridable Property Name() As String
        Get
            Return m_Object.Name
        End Get
        Set(ByVal value As String)
            If value <> m_Object.Name Then
                If Not m_PCB.LayerObjectNameExists(value) Then
                    m_PCB.AddUndoItem(New UndoRedoNameChange(m_PCB, m_Object, m_Object.Name, value))
                    m_Object.Name = value
                Else
                    MsgBox("This name already exists.", MsgBoxStyle.Critical)
                End If
            End If
        End Set
    End Property

    Public Overridable Property Rotation() As Single
        Get
            Return m_Object.rotation
        End Get
        Set(ByVal value As Single)
            m_PCB.AddUndoItem(New UndoRedoChangeRotation(m_PCB, m_Object))
            m_Object.Rotation = value
        End Set
    End Property

End Class