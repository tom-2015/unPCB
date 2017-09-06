Imports System.ComponentModel
Imports System.Drawing


Public Class Layer

    Public LayerObjects As List(Of LayerObject) 'all objects on the layer

    Dim m_LayerType As PCB.LayerTypes
    Dim m_LayerName As String
    Dim m_Visible As Boolean = True
    Dim m_Hidden As Boolean = False

    Public Event LayerVisibilityChanged(ByVal layer As Layer, ByVal Visible As Boolean)

    Public Sub New(ByVal LayerType As PCB.LayerTypes)
        LayerObjects = New List(Of LayerObject)
        Me.LayerType = LayerType
    End Sub

    'construct from xml
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Sub Draw(ByVal Graphics As System.Drawing.Graphics)
        Dim LayerObject As LayerObject
        If m_Visible Then
            'draw all objects
            For Each LayerObject In LayerObjects
                Dim Container As System.Drawing.Drawing2D.GraphicsContainer = Graphics.BeginContainer()
                If LayerObject.Rotation <> 0 Then
                    Graphics.TranslateTransform(LayerObject.Location.X, LayerObject.Location.Y)
                    Graphics.RotateTransform(LayerObject.Rotation)
                    Graphics.TranslateTransform(-LayerObject.Location.X, -LayerObject.Location.Y)
                End If
                LayerObject.Draw(Graphics, Me)
                Graphics.EndContainer(Container)
            Next
        End If
    End Sub

    Public Overridable Property LayerType() As unPCB.PCB.LayerTypes
        Get
            Return m_LayerType
        End Get
        Set(ByVal value As unPCB.PCB.LayerTypes)
            m_LayerType = value
            Select Case value
                Case PCB.LayerTypes.LayerTypeBottomDevice
                    m_LayerName = "Bottom Devices"
                    m_Hidden = True
                Case PCB.LayerTypes.LayerTypeBottomDrawing
                    m_LayerName = "Bottom Drawing"
                    m_Hidden = True
                Case PCB.LayerTypes.LayerTypeBottomImage
                    m_LayerName = "Bottom PCB"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeBottomInfo
                    m_LayerName = "Bottom Info"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeBottomPads
                    m_LayerName = "Bottom Pads"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeBottomRoute
                    m_LayerName = "Bottom Route"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeBottomVias
                    m_LayerName = "Bottom Vias"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeTopDevice
                    m_LayerName = "Top Devices"
                    m_Hidden = True
                Case PCB.LayerTypes.LayerTypeTopDrawing
                    m_LayerName = "Top Drawing"
                    m_Hidden = True
                Case PCB.LayerTypes.LayerTypeTopImage
                    m_LayerName = "Top PCB"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeTopInfo
                    m_LayerName = "Top Info"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeTopPads
                    m_LayerName = "Top Pads"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeTopRoute
                    m_LayerName = "Top Route"
                    m_Hidden = False
                Case PCB.LayerTypes.LayerTypeTopVias
                    m_LayerName = "Top vias"
                    m_Hidden = False
            End Select

        End Set
    End Property

    ''' <summary>
    ''' Returns if visible turned on / off in the layer menu
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
            RaiseEvent LayerVisibilityChanged(Me, Visible)
        End Set
    End Property

    ''' <summary>
    ''' Returns user readable layer name
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Name() As String
        Get
            Return m_LayerName
        End Get
    End Property

    ''' <summary>
    ''' Returns if this is a hidden layer (user should not be able to turn it on/off)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Hidden() As Boolean
        Get
            Return m_Hidden
        End Get
    End Property

    Public Sub AddObject(ByVal LayerObject As LayerObject)
        LayerObjects.Add(LayerObject)
    End Sub

    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim LayerObject As LayerObject
        Dim ObjectNodes As Xml.XmlElement
        Dim ObjectNode As Xml.XmlElement

        Root.Attributes.Append(XMLDoc.CreateAttribute("type")).Value = m_LayerType
        Root.Attributes.Append(XMLDoc.CreateAttribute("name")).Value = m_LayerName
        Root.Attributes.Append(XMLDoc.CreateAttribute("visible")).Value = m_Visible
        ObjectNodes = Root.AppendChild(XMLDoc.CreateElement("objects"))
        For Each LayerObject In LayerObjects
            ObjectNode = ObjectNodes.AppendChild(XMLDoc.CreateElement("object"))
            ObjectNode.Attributes.Append(XMLDoc.CreateAttribute("id")).Value = LayerObject.id
        Next

    End Sub

    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim ObjectNodes As Xml.XmlNodeList

        Me.LayerType = Root.Attributes("type").Value
        'm_LayerName = Root.Attributes("name").Value
        m_Visible = Root.Attributes("visible").Value

        ObjectNodes = Root.SelectNodes("./objects/object")
        For Each ObjectNode As Xml.XmlNode In ObjectNodes
            Dim Id As Integer = ObjectNode.Attributes("id").Value
            LayerObjects.Add(PCB.GetLayerObject(Id))
        Next
    End Sub

End Class