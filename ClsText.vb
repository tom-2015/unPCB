Imports System.Globalization
Imports System.Drawing
Imports System.ComponentModel

Public Class TextBox
    Inherits SelectableLayerObject

    Dim m_Text As String
    Dim m_Font As Font

    Public Sub New()
        MyBase.New()
        m_Color = Color.White
        m_Font = SystemFonts.DefaultFont
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF)
        MyBase.New(Location)
        m_Color = Color.White
        m_Font = SystemFonts.DefaultFont
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Width As Integer, ByVal Height As Integer)
        MyBase.New(Location, Width, Height)
        m_Color = Color.White
        m_Font = SystemFonts.DefaultFont
    End Sub

    Public Sub New(ByVal Text As String)
        MyBase.New()
        m_Color = Color.White
        m_Font = SystemFonts.DefaultFont
        m_Text = Text
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Property Font() As Font
        Get
            Return m_Font
        End Get
        Set(ByVal value As Font)
            m_Font = value
        End Set
    End Property

    Public Property Text() As String
        Get
            Return m_Text
        End Get
        Set(ByVal value As String)
            m_Text = value
        End Set
    End Property

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        MyBase.Draw(Graphics, Layer)
        Graphics.DrawString(m_Text, m_Font, New SolidBrush(IIf(m_Selected, m_SelectedColor, m_Color)), m_Rect.X, m_Rect.Y)
        Dim Size As SizeF = Graphics.MeasureString(m_Text, m_Font, m_Rect.Size, New StringFormat(StringFormatFlags.NoWrap Or StringFormatFlags.NoClip))
        m_Rect.Width = Size.Width
        m_Rect.Height = Size.Height
    End Sub

    Public Overrides Function GetProperties() As LayerObjectProperties
        Return New textboxproperties(m_PCB, Me)
    End Function

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
            PCB.GetLayer(PCB.LayerTypes.LayerTypeBottomInfo).LayerObjects.Add(Me)
        Else
            PCB.GetLayer(PCB.LayerTypes.LayerTypeTopInfo).LayerObjects.Add(Me)
        End If
    End Sub

    ''' <summary>
    ''' Converts to xml
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Overrides Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Root.Attributes.Append(XMLDoc.CreateAttribute("font")).Value = m_Font.Name
        Root.Attributes.Append(XMLDoc.CreateAttribute("font_size")).Value = SingleToString(m_Font.Size)
        Root.Attributes.Append(XMLDoc.CreateAttribute("font_style")).Value = m_Font.Style
        Root.Attributes.Append(XMLDoc.CreateAttribute("font_unit")).Value = m_Font.Unit
        Root.Attributes.Append(XMLDoc.CreateAttribute("font_charset")).Value = m_Font.GdiCharSet
        Root.AppendChild(XMLDoc.CreateTextNode(m_Text))
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
        Dim FontName As String = Root.Attributes("font").Value
        Dim FontSize As Single = StringToSingle(Root.Attributes("font_size").Value)
        Dim FontStyle As FontStyle = Root.Attributes("font_style").Value
        Dim FontUnit As GraphicsUnit = Root.Attributes("font_unit").Value
        Dim FontCharSet As Byte = Root.Attributes("font_charset").Value
        m_Font = New Font(FontName, FontSize, FontStyle, FontUnit, FontCharSet)
        m_Text = Root.InnerText
    End Sub

End Class


Public Class TextBoxProperties
    Inherits LayerObjectProperties

    Public Sub New(ByVal PCB As PCB, ByVal Obj As LayerObject)
        MyBase.New(PCB, Obj)
    End Sub

    <CategoryAttribute("Font")> _
    Public Overridable Property Font() As Font
        Get
            Return CType(m_Object, TextBox).Font
        End Get
        Set(ByVal value As Font)
            m_PCB.AddUndoItem(New UndoRedoTextChange(m_PCB, m_Object))
            CType(m_Object, TextBox).Font = value
        End Set
    End Property

    <CategoryAttribute("Color"), _
     DefaultValueAttribute(GetType(System.Drawing.Color), "0")> _
    Public Overridable Property Color() As Color
        Get
            Return CType(m_Object, TextBox).Color
        End Get
        Set(ByVal value As Color)
            m_PCB.AddUndoItem(New UndoRedoChangeColor(m_PCB, m_Object))
            CType(m_Object, TextBox).Color = value
        End Set
    End Property

    Public Overridable Property Text() As String
        Get
            Return CType(m_Object, TextBox).Text
        End Get
        Set(ByVal value As String)
            m_PCB.AddUndoItem(New UndoRedoTextChange(m_PCB, m_Object))
            CType(m_Object, TextBox).Text = value
        End Set
    End Property

End Class