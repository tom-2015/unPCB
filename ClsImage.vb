Public Class PCBImage
    Inherits LayerObject

    Public m_pic As System.Drawing.Image
    Public m_CachedPicture As Bitmap

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Picture As System.Drawing.Image)
        Me.New()
        m_pic = Picture 'Image.FromFile("bottom.jpg")
        m_Rect.Width = Picture.Width
        m_Rect.Height = Picture.Height
        UpdateCache()

    End Sub

    ''' <summary>
    ''' Updates the cached picture which is in the 32 Bits/pixel format for faster rendering
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UpdateCache()
        If m_pic IsNot Nothing Then
            m_CachedPicture = New Bitmap(m_pic.Width, m_pic.Height, Imaging.PixelFormat.Format32bppPArgb)
            Dim Graphics As Graphics = Graphics.FromImage(m_CachedPicture)
            Graphics.DrawImage(m_pic, 0, 0, m_pic.Width, m_pic.Height)
            Graphics.Dispose()
        End If
    End Sub

    Public Sub New(ByVal Location As System.Drawing.PointF, ByVal Picture As String)
        Me.New(Location, Image.FromFile(Picture))
    End Sub

    'construct from XML
    Public Sub New(ByVal PCB As PCB, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        fromXML(PCB, Root, BinData)
    End Sub

    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes)
        PlaceObject(PCB, WindowType, m_Rect.Location)
    End Sub

    'places the layerobject on the PCB/layer/window
    Public Overrides Sub PlaceObject(ByVal PCB As PCB, ByVal WindowType As PCB.WindowTypes, ByVal Location As System.Drawing.PointF)
        MyBase.PlaceObject(PCB, WindowType, Location)
        If WindowType = unPCB.PCB.WindowTypes.WindowTypeTop Then
            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeTopImage).LayerObjects.Add(Me)
        Else
            PCB.GetLayer(unPCB.PCB.LayerTypes.LayerTypeBottomImage).LayerObjects.Add(Me)
        End If
    End Sub

    Public Overrides Sub Draw(ByVal Graphics As System.Drawing.Graphics, ByVal Layer As Layer)
        MyBase.Draw(Graphics, Layer)

        If m_pic IsNot Nothing Then Graphics.DrawImage(m_CachedPicture, m_Rect) 'draw the cached picture image on the layer

    End Sub

    Public Property Image() As Image
        Get
            Return m_pic
        End Get
        Set(ByVal value As Image)
            m_pic = value
        End Set
    End Property

    Public Overrides Property Width() As Single
        Get
            Return m_pic.Width
        End Get
        Set(ByVal value As Single)

        End Set
    End Property

    Public Overrides Property Height() As Single
        Get
            Return m_pic.Height
        End Get
        Set(ByVal value As Single)

        End Set
    End Property

    Public Overrides Sub toXML(ByVal XMLDoc As System.Xml.XmlDocument, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.toXML(XMLDoc, Root, BinData)
        Dim ImgStream As New System.IO.MemoryStream

        m_pic.Save(ImgStream, System.Drawing.Imaging.ImageFormat.Png)
        ImgStream.Seek(0, IO.SeekOrigin.Begin)
        BinData.AddEntry("data/img_" & m_id & ".png", ImgStream)

    End Sub

    Public Overrides Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        MyBase.fromXML(PCB, Root, BinData)
        Dim ImgStream As New System.IO.MemoryStream
        Dim ZipEntry As Ionic.Zip.ZipEntry

        For Each ZipEntry In BinData
            If ZipEntry.FileName.ToLower() = "data/img_" & m_id & ".png" Then
                ZipEntry.Extract(ImgStream)
                ImgStream.Seek(0, IO.SeekOrigin.Begin)
                m_pic = Image.FromStream(ImgStream)
                m_Rect.Width = m_pic.Width
                m_Rect.Height = m_pic.Height
                UpdateCache()
            End If
        Next

    End Sub

End Class
