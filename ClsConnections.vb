Imports System.IO

Public Class ConnectionMatrix
    Implements ICloneable

    Public Enum ConnectionTypes As Byte
        ConnectionTypeNotConnected = 0
        ConnectionTypeConnected = 1
        ConnectionTypeUnknown = 2
        ConnectionTypeUnused = 3
    End Enum

    Protected Class PadMap
        Public Pad As Pad
        Public Index As Integer

        Public Sub New(ByVal Pad As Pad, ByVal Index As Integer)
            Me.Pad = Pad
            Me.Index = Index
        End Sub
    End Class

    Protected m_Connections(,) As ConnectionTypes
    Protected m_ConnectionsSize As Integer
    Protected m_ConnectionsCount As Integer
    Protected m_Pads As New Dictionary(Of Pad, PadMap)   'pads in the connection matrix Pad => PadMap(array index)
    Protected m_Index As New Dictionary(Of Integer, Pad) 'maps index in the connection matrix to Pad
    Protected m_UnusedIndex As New List(Of Integer)      'holds unused index in the array (when pad was removed) it will be filled by first pad added
    Protected m_PCB As PCB

    'Public Connections As New Dictionary(Of Pad, Dictionary(Of Pad, ConnectionTypes))

    Public Event ConnectionsChanged(ByVal Sender As ConnectionMatrix)

    Public Sub New(ByVal PCB As PCB)
        GrowArray(32)
        m_PCB = PCB
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        GrowArray(32)
        m_PCB = PCB
        fromXML(PCB, Root, BinData)
    End Sub

    ''' <summary>
    ''' Grows the array to fit Size elements in both X and Y
    ''' Does not increase actual connections_count
    ''' </summary>
    ''' <param name="Size"></param>
    ''' <remarks></remarks>
    Private Sub GrowArray(ByVal Size As Integer)
        If Size > m_ConnectionsSize Then
            'Dim Backup() As ConnectionState
            'ReDim Backup(0 To m_Connections.Length - 1)
            'Array.Copy(m_Connections, Backup, m_Connections.Length)

            If (Size Mod 8) > 0 Then 'align size to 64 bit
                Size = Size + 8 - (Size Mod 8)
            End If

            Dim NewConnections(,) As ConnectionTypes
            ReDim NewConnections(0 To Size - 1, 0 To Size - 1)

            For i As Integer = 0 To m_ConnectionsCount - 1
                For j As Integer = 0 To m_ConnectionsCount - 1
                    NewConnections(i, j) = m_Connections(i, j)
                Next
            Next
            m_ConnectionsSize = Size
            m_Connections = NewConnections
            'ReDim Preserve m_Connections(0 To m_ConnectionsSize * m_ConnectionsSize - 1)
            'Array.Copy(Backup, m_Connections, Backup.Length)
        End If
    End Sub

    ''' <summary>
    ''' Returns a Dictionary that maps Pad ids with pads that are currently added to the connection matrix
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getPadIDMap() As Dictionary(Of Integer, Pad)
        Dim Map As New Dictionary(Of Integer, Pad)
        For Each Pad As KeyValuePair(Of Pad, PadMap) In m_Pads
            Map.Add(Pad.Key.id, Pad.Key)
        Next
        'For Each ConnectionRow As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    Map.Add(ConnectionRow.Key.id, ConnectionRow.Key)
        'Next
        Return Map
    End Function

    ''' <summary>
    ''' Returns a matrix of connectiontypes for each pad id
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BackUp() As ConnectionMatrix
        'Dim Cloned As New Dictionary(Of Integer, Dictionary(Of Integer, ConnectionTypes))
        'For Each ConnectionRow As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    Dim ClonedRow As New Dictionary(Of Integer, ConnectionTypes)
        '    For Each Connection As KeyValuePair(Of Pad, ConnectionTypes) In ConnectionRow.Value
        '        ClonedRow.Add(Connection.Key.id, Connection.Value)
        '    Next
        '    Cloned.Add(ConnectionRow.Key.id, ClonedRow)
        'Next

        Return Me.Clone()
    End Function

    ''' <summary>
    ''' Restores connections to a previous backup, leaving connections that are new as they are now. notice: that pads must exist
    ''' </summary>
    ''' <param name="Backup"></param>
    ''' <remarks></remarks>
    Public Sub Restore(ByVal Backup As ConnectionMatrix)
        m_ConnectionsCount = Backup.m_ConnectionsCount
        m_ConnectionsSize = Backup.m_ConnectionsSize
        Array.Copy(Backup.m_Connections, m_Connections, Backup.m_Connections.Length)
        m_Pads.Clear()
        m_Index.Clear()
        For Each Pad As KeyValuePair(Of Pad, PadMap) In Backup.m_Pads
            m_Pads.Add(Pad.Key, Pad.Value)
        Next
        For Each Index As KeyValuePair(Of Integer, Pad) In Backup.m_Index
            m_Index.Add(Index.Key, Index.Value)
        Next
        m_UnusedIndex = New List(Of Integer)(Backup.m_UnusedIndex)

        'Dim PadIDMap As Dictionary(Of Integer, Pad) = getPadIDMap()
        'Dim RowPad As Pad, ColPad As Pad
        'For Each ConnectionRow As KeyValuePair(Of Integer, Dictionary(Of Integer, ConnectionTypes)) In StoredConnections
        '    For Each Connection As KeyValuePair(Of Integer, ConnectionTypes) In ConnectionRow.Value
        '        RowPad = PadIDMap(ConnectionRow.Key)
        '        ColPad = PadIDMap(Connection.Key)
        '        Connections(RowPad)(ColPad) = Connection.Value
        '    Next
        'Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Removes a pad from the matrix and all it's connection states
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub RemovePad(ByVal Pad As Pad)
        Dim Index As Integer = m_Pads(Pad).Index
        For i As Integer = 0 To m_ConnectionsCount - 1
            m_Connection(i, Index) = ConnectionTypes.ConnectionTypeUnused
            m_Connection(Index, i) = ConnectionTypes.ConnectionTypeUnused
        Next
        m_Connection(Index, Index) = ConnectionTypes.ConnectionTypeUnused
        m_Index.Remove(Index)
        m_Pads.Remove(Pad)
        m_UnusedIndex.Add(Index)

        While CheckLastRowIsUnused()

        End While
        'For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    Row.Value.Remove(Pad) 'remove pad from each column
        'Next
        'Connections.Remove(Pad) 'remove pad from the rows
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' This checks if the last item in the table is unused and removes it from the unused index list
    ''' </summary>
    ''' <returns>false if there is no more unused index found as the last item in the table</returns>
    ''' <remarks></remarks>
    Private Function CheckLastRowIsUnused() As Boolean
        For Each UnusedIndex As Integer In m_UnusedIndex
            If UnusedIndex = m_ConnectionsCount - 1 Then
                m_UnusedIndex.Remove(UnusedIndex)
                m_ConnectionsCount -= 1
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Adds a new Pad to the connection matrix
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub AddPad(ByVal Pad As Pad)
        Dim Index As Integer = m_ConnectionsCount

        If m_UnusedIndex.Count > 0 Then
            Index = m_UnusedIndex.Item(0)
            m_UnusedIndex.Remove(Index)
        Else
            m_ConnectionsCount = m_ConnectionsCount + 1
            If m_ConnectionsCount = m_ConnectionsSize Then
                GrowArray(IIf(m_ConnectionsSize < 20000, m_ConnectionsSize * 2, m_ConnectionsSize + m_ConnectionsSize / 2))
            End If
        End If

        For i As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(i, i) <> ConnectionTypes.ConnectionTypeUnused Then
                m_Connection(Index, i) = ConnectionTypes.ConnectionTypeUnknown
                m_Connection(i, Index) = ConnectionTypes.ConnectionTypeUnknown
            End If
        Next

        m_Connection(Index, Index) = ConnectionTypes.ConnectionTypeConnected
        m_Index.Add(Index, Pad)
        m_Pads.Add(Pad, New PadMap(Pad, Index))
        'Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes))
        'Dim NewRow As New Dictionary(Of Pad, ConnectionTypes)

        'For Each Row In Connections
        '    Row.Value.Add(Pad, ConnectionTypes.ConnectionTypeUnknown)
        '    NewRow.Add(Row.Key, ConnectionTypes.ConnectionTypeUnknown)
        'Next

        'Connections.Add(Pad, NewRow)
        'Connections(Pad)(Pad) = ConnectionTypes.ConnectionTypeConnected 'connect to myself
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Gets all pads that are connected to the given pad by ConnectionType (default is normal connected), includes the pad itself
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <param name="ConnectionType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNet(ByVal Pad As Pad, Optional ByVal ConnectionType As ConnectionTypes = ConnectionTypes.ConnectionTypeConnected) As Net
        Dim Net As New Net
        Dim Index As Integer = m_Pads(Pad).Index

        'Dim Row As Dictionary(Of Pad, ConnectionTypes)
        'Dim Col As KeyValuePair(Of Pad, ConnectionTypes)
        For i As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(Index, i) = ConnectionType Then
                Net.Pads.Add(m_Index(i))
            End If
        Next
        'Row = Connections(Pad)
        'For Each Col In Row
        '    If Col.Value = ConnectionType Then
        '        Net.Pads.Add(Col.Key)
        '    End If
        'Next

        Return Net
    End Function

    ''' <summary>
    ''' Returns all the nets (net = collection of connected pads/pins), slow function!
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNets() As List(Of Net)
        Dim PadsChecked As New List(Of Pad) 'contains all pads that we know are in some net
        Dim Nets As New List(Of Net)

        For Each Pad As KeyValuePair(Of Pad, PadMap) In m_Pads
            If Not PadsChecked.Contains(Pad.Key) Then
                Dim CurrentNet As Net = GetNet(Pad.Key, ConnectionTypes.ConnectionTypeConnected)
                'add the pads in this net to the padschecked list, we will not get a net for them anymore
                For Each Pd As Pad In CurrentNet.Pads
                    PadsChecked.Add(Pd)
                Next
                Nets.Add(CurrentNet)
            End If
        Next

        'get the net for all pads
        'For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    If Not PadsChecked.Contains(Row.Key) Then
        '        Dim CurrentNet As Net = GetNet(Row.Key, ConnectionTypes.ConnectionTypeConnected)
        '        'add the pads in this net to the padschecked list, we will not get a net for them anymore
        '        For Each Pad As Pad In CurrentNet.Pads
        '            PadsChecked.Add(Pad)
        '        Next
        '        Nets.Add(CurrentNet)
        '    End If
        'Next
        Return Nets
    End Function

    Public Sub ConnectPads(ByVal Pad1 As Pad, ByVal Pad2 As Pad)
        Dim NetConnected As Net
        Dim NetNotConnected As Net
        Dim RowPad As Pad
        Dim ColPad As Pad
        Dim Pad1Index As Integer = m_Pads(Pad1).Index
        Dim Pad2Index As Integer = m_Pads(Pad2).Index

        m_Connection(Pad1Index, Pad1Index) = ConnectionTypes.ConnectionTypeConnected
        m_Connection(Pad2Index, Pad2Index) = ConnectionTypes.ConnectionTypeConnected

        'first select all pads pad1 and pad2 are connected with
        NetConnected = GetNet(Pad1, ConnectionTypes.ConnectionTypeConnected) + GetNet(Pad2, ConnectionTypes.ConnectionTypeConnected)

        For Each RowPad In NetConnected.Pads 'iterate through all the connected pads and connect them with each other
            Dim Idx1 As Integer = m_Pads(RowPad).Index
            For Each ColPad In NetConnected.Pads
                m_Connection(Idx1, m_Pads(ColPad).Index) = ConnectionTypes.ConnectionTypeConnected
            Next
        Next

        'select all pads that are not connected to pad1 and pad2
        NetNotConnected = GetNet(Pad1, ConnectionTypes.ConnectionTypeNotConnected) + GetNet(Pad2, ConnectionTypes.ConnectionTypeNotConnected)

        For Each RowPad In NetNotConnected.Pads 'iterate through all the NOT connected pads and NOT connect them with the pads we are connected with, what's left over will be UNKNOWN connection
            Dim Idx1 As Integer = m_Pads(RowPad).Index
            For Each ColPad In NetConnected.Pads
                Dim Idx2 As Integer = m_Pads(ColPad).Index
                m_Connection(Idx1, Idx2) = ConnectionTypes.ConnectionTypeNotConnected
                m_Connection(Idx2, Idx1) = ConnectionTypes.ConnectionTypeNotConnected
            Next
        Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    Public Sub ConnectPads(ByVal Pads() As Pad)
        Dim Pad As Pad
        Dim FirstPad As Pad = Nothing
        For Each Pad In Pads
            If FirstPad Is Nothing Then
                FirstPad = Pad
            Else
                ConnectPads(FirstPad, Pad)
            End If
        Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Says that Pad1 and Pad2 and the pads they are connected with are NOT connected to each other, this eliminates further needed testing for connection between the 2 nets (for ex. all pads connected to GND and VCC will never be connected to each other)
    ''' </summary>
    ''' <param name="Pad1"></param>
    ''' <param name="Pad2"></param>
    ''' <remarks></remarks>
    Public Sub NotConnectPads(ByVal Pad1 As Pad, ByVal Pad2 As Pad)
        Dim NetConnected1 As Net
        Dim NetConnected2 As Net
        'Dim Pad1 As Pad, Pad2 As Pad

        If Not Pad1.Equals(Pad2) Then
            'get all pads that are connected
            NetConnected1 = GetNet(Pad1, ConnectionTypes.ConnectionTypeConnected)
            NetConnected2 = GetNet(Pad2, ConnectionTypes.ConnectionTypeConnected)

            For Each PadConnected1 As Pad In NetConnected1.Pads
                Dim Idx1 As Integer = m_Pads(PadConnected1).Index
                For Each padConnected2 As Pad In NetConnected2.Pads
                    Dim Idx2 As Integer = m_Pads(padConnected2).Index
                    m_Connection(Idx1, Idx2) = ConnectionTypes.ConnectionTypeNotConnected
                    m_Connection(Idx2, Idx1) = ConnectionTypes.ConnectionTypeNotConnected
                Next
            Next

            m_Connection(m_Pads(Pad1).Index, m_Pads(Pad1).Index) = ConnectionTypes.ConnectionTypeConnected
            m_Connection(m_Pads(Pad2).Index, m_Pads(Pad2).Index) = ConnectionTypes.ConnectionTypeConnected

            RaiseEvent ConnectionsChanged(Me)
        End If
    End Sub

    ''' <summary>
    ''' Says that the pads in the array and the pads they are connected with, are NOT connected to each other, this eliminates further needed testing for connection between the 2 nets (for ex. all pads connected to GND and +3.3V, +5V will never be connected to each other)
    ''' </summary>
    ''' <param name="Pads"></param>
    ''' <remarks></remarks>
    Public Sub NotConnectPads(ByVal Pads() As Pad)
        For Each Pad1 As Pad In Pads
            For Each pad2 As Pad In Pads
                NotConnectPads(Pad1, pad2)
            Next
        Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' The pad will be NOT connected to all other pads which are not checked
    ''' </summary>
    ''' <param name="Pad1">The pad to NOT connect to all other pads which have connectiontype: unknown</param>
    ''' <remarks></remarks>
    Public Sub NotConnectToAllPads(ByVal Pad1 As Pad)
        Dim Index As Integer = m_Pads(Pad1).Index
        For i As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(Index, i) = ConnectionTypes.ConnectionTypeUnknown Then
                NotConnectPads(Pad1, m_Index(i))
            End If
        Next


        'Dim AllPads As Dictionary(Of Pad, ConnectionTypes)
        'AllPads = New Dictionary(Of Pad, ConnectionTypes)(Connections.Item(Pad1))
        'For Each Pad As KeyValuePair(Of Pad, ConnectionTypes) In AllPads
        '    If Pad.Value = ConnectionTypes.ConnectionTypeUnknown Then
        '        NotConnectPads(Pad1, Pad.Key)
        '    End If
        'Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Disconnects the pad from all pads it's connected with and NOT connected with (resets all connection states for the pad)
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <remarks></remarks>
    Public Sub DisconnectPad(ByVal Pad As Pad)
        Dim Index As Integer = m_Pads(Pad).Index

        For i As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(i, i) <> ConnectionTypes.ConnectionTypeUnused Then
                m_Connection(Index, i) = ConnectionTypes.ConnectionTypeUnknown
                m_Connection(i, Index) = ConnectionTypes.ConnectionTypeUnknown
            End If
        Next
        m_Connection(Index, Index) = ConnectionTypes.ConnectionTypeConnected

        ''Dim Row As Dictionary(Of Pad, ConnectionTypes)
        'Dim Col As KeyValuePair(Of Pad, ConnectionTypes)
        'Dim Pads As New List(Of Pad)
        ''Row = 
        'For Each Col In Connections(Pad)
        '    Pads.Add(Col.Key)
        'Next
        'For Each Pad1 As Pad In Pads
        '    Connections(Pad)(Pad1) = ConnectionTypes.ConnectionTypeUnknown
        '    Connections(Pad1)(Pad) = ConnectionTypes.ConnectionTypeUnknown
        'Next
        'Connections(Pad)(Pad) = ConnectionTypes.ConnectionTypeConnected
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Disconnects pads, changing the connection state to unknown
    ''' </summary>
    ''' <param name="Pads"></param>
    ''' <remarks></remarks>
    Public Sub DisconnectPads(ByVal Pads() As Pad)
        For Each pad As Pad In Pads
            DisconnectPad(pad)
        Next
        RaiseEvent ConnectionsChanged(Me)
    End Sub

    ''' <summary>
    ''' Returns next pad to check connection with based on the pad we want to test against, returns nothing if all are checked
    ''' </summary>
    ''' <param name="CheckWithPad"></param>
    ''' <param name="OrderByDistance">If set to true the nearest unchecked pad will be returned first, if set false the first found will be returned</param>
    ''' <param name="ExcludePads">Optional list containing pads that must be excluded from search</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNextToCheckPad(ByVal CheckWithPad As Pad, Optional ByVal OrderByDistance As Boolean = True, Optional ByVal ExcludePads As List(Of Pad) = Nothing) As Pad
        'Dim Row As Dictionary(Of Pad, ConnectionMatrix.ConnectionTypes)
        Dim ClosestPad As Pad = Nothing
        Dim ClosestDistance As UInt32 = UInt32.MaxValue
        Dim CheckWithPadIndex As Integer = m_Pads(CheckWithPad).Index

        For i As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(CheckWithPadIndex, i) = ConnectionTypes.ConnectionTypeUnknown Then
                Dim PadToCheck As Pad = m_Index(i)
                If ExcludePads Is Nothing OrElse ExcludePads.Contains(PadToCheck) = False Then
                    If OrderByDistance Then 'check if we need to order by distance
                        Dim Distance As UInt32 = Functions.Distance(PadToCheck.Center, CheckWithPad.Center)
                        If Distance < ClosestDistance Then
                            ClosestDistance = Distance
                            ClosestPad = PadToCheck
                        End If
                    Else
                        Return PadToCheck
                    End If
                End If
            End If
        Next

        'Row = Connections.Item(CheckWithPad)
        'For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row
        '    If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
        '        If ExcludePads Is Nothing OrElse ExcludePads.Contains(Col.Key) = False Then
        '            If OrderByDistance Then 'check if we need to order by distance
        '                Dim Distance As UInt32 = Functions.Distance(Col.Key.Center, CheckWithPad.Center)
        '                If Distance < ClosestDistance Then
        '                    ClosestDistance = Distance
        '                    ClosestPad = Col.Key
        '                End If
        '            Else
        '                Return Col.Key
        '            End If
        '        End If
        '    End If
        'Next
        Return ClosestPad
    End Function

    ''' <summary>
    ''' Returns the first found pad which has unknown connection states to another pad
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetFirstUnconnectedPad() As Pad
        For i As Integer = 0 To m_ConnectionsCount - 1
            For j As Integer = 0 To m_ConnectionsCount - 1
                If m_Connection(i, j) = ConnectionTypes.ConnectionTypeUnknown Then
                    Return m_Index(i)
                End If
            Next
        Next
        'For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row.Value
        '        If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
        '            Return Row.Key
        '        End If
        '    Next
        'Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Performs a count of the number of connections that still have to be checked
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUnknownConnections() As Integer
        Dim Count As Integer = 0
        For i As Integer = 0 To m_ConnectionsCount - 1
            For j As Integer = 0 To m_ConnectionsCount - 1
                If m_Connection(i, j) = ConnectionTypes.ConnectionTypeUnknown Then
                    Count += 1
                End If
            Next
        Next
        'For Each Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes)) In Connections
        '    For Each Col As KeyValuePair(Of Pad, ConnectionTypes) In Row.Value
        '        If Col.Value = ConnectionTypes.ConnectionTypeUnknown Then
        '            Count += 1
        '        End If
        '    Next
        'Next
        Return Count
    End Function

    ''' <summary>
    ''' Returns total number of cells in the matrix
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTotalConnections() As Integer
        Return m_ConnectionsCount * m_ConnectionsCount
    End Function


    ''' <summary>
    ''' Returns number of pads (rows in matrix)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ConnectionCount() As Integer
        Get
            Return m_ConnectionsCount
        End Get
    End Property

    ''' <summary>
    ''' Gets / sets the connection state in the one dimensional array
    ''' </summary>
    ''' <param name="Row"></param>
    ''' <param name="Col"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Property m_Connection(ByVal Row As Integer, ByVal Col As Integer) As ConnectionTypes
        Get
            Return m_Connections(Row, Col)
        End Get
        Set(ByVal value As ConnectionTypes)
            m_Connections(Row, Col) = value
        End Set
    End Property

    ''' <summary>
    ''' Returns Connection state for row and col index
    ''' </summary>
    ''' <param name="Row"></param>
    ''' <param name="Col"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Connection(ByVal Row As Integer, ByVal Col As Integer) As ConnectionTypes
        Get
            Return m_Connections(Row, Col)
        End Get
    End Property

    ''' <summary>
    ''' Returns the index of a pad in the matrix
    ''' </summary>
    ''' <param name="Pad"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetPadIndex(ByVal Pad As Pad) As Integer
        Return m_Pads(Pad).Index
    End Function

    ''' <summary>
    ''' Return The pad at index in the matrix
    ''' </summary>
    ''' <param name="Index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetPad(ByVal Index As Integer) As Pad
        Return m_Index(Index)
    End Function

    ''' <summary>
    ''' Returns if index is used
    ''' </summary>
    ''' <param name="Index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetIndexUsed(ByVal Index As Integer) As Boolean
        Return m_Connection(Index, Index) <> ConnectionTypes.ConnectionTypeUnused
    End Function


    ''' <summary>
    ''' Converts to XML document
    ''' </summary>
    ''' <param name="XMLDoc"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData">Zip file containing binary data</param>
    ''' <remarks></remarks>
    Public Sub toXML(ByVal XMLDoc As Xml.XmlDocument, ByVal Root As Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        'Dim Row As KeyValuePair(Of Pad, Dictionary(Of Pad, ConnectionTypes))
        'Dim Col As KeyValuePair(Of Pad, ConnectionTypes)
        'Dim ConnectionNode As Xml.XmlElement = Root.AppendChild(XMLDoc.CreateElement("connections"))
        Root.Attributes.Append(XMLDoc.CreateAttribute("size")).Value = m_ConnectionsSize
        Root.Attributes.Append(XMLDoc.CreateAttribute("count")).Value = m_ConnectionsCount
        Root.Attributes.Append(XMLDoc.CreateAttribute("file")).Value = "data/connections.csv"

        Dim Data As New System.IO.MemoryStream
        Dim Writer As New StreamWriter(Data)
        Writer.NewLine = vbCrLf

        For Row As Integer = 0 To m_ConnectionsCount - 1
            If m_Connection(Row, Row) <> ConnectionTypes.ConnectionTypeUnused Then
                Writer.Write(m_Index(Row).id & ";")
                For Col As Integer = 0 To m_ConnectionsCount - 1
                    Writer.Write(m_Connection(Row, Col) & ";")
                Next
                Writer.WriteLine("")
            End If
        Next
        Writer.Flush()
        Data.Seek(0, SeekOrigin.Begin)
        BinData.AddEntry("data/connections.csv", Data)



        'For i As Integer = 0 To m_ConnectionsCount - 1
        '    If m_Index.ContainsKey(i) Then
        '        Dim RowConnection As Xml.XmlElement = Root.AppendChild(XMLDoc.CreateElement("row"))
        '        RowConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = m_Index(i).id
        '        For j As Integer = 0 To m_ConnectionsCount - 1
        '            Dim ColConnection As Xml.XmlElement = RowConnection.AppendChild(XMLDoc.CreateElement("col"))
        '            ColConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = m_Index(j).id
        '            ColConnection.Attributes.Append(XMLDoc.CreateAttribute("con_type")).Value = m_Connection(i, j)
        '        Next
        '    End If
        'Next

        'For Each Row In Connections
        '    Dim RowConnection As Xml.XmlElement = Root.AppendChild(XMLDoc.CreateElement("row"))
        '    RowConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = Row.Key.id
        '    For Each Col In Row.Value
        '        Dim ColConnection As Xml.XmlElement = RowConnection.AppendChild(XMLDoc.CreateElement("col"))
        '        ColConnection.Attributes.Append(XMLDoc.CreateAttribute("pad_id")).Value = Col.Key.id
        '        ColConnection.Attributes.Append(XMLDoc.CreateAttribute("con_type")).Value = Col.Value
        '    Next
        'Next
    End Sub

    ''' <summary>
    ''' Converts from XML document
    ''' </summary>
    ''' <param name="PCB"></param>
    ''' <param name="Root"></param>
    ''' <param name="BinData"></param>
    ''' <remarks></remarks>
    Public Sub fromXML(ByVal PCB As PCB, ByVal Root As System.Xml.XmlNode, ByVal BinData As Ionic.Zip.ZipFile)
        Dim Size As Integer

        m_Index = New Dictionary(Of Integer, Pad)
        m_Pads = New Dictionary(Of Pad, PadMap)
        m_UnusedIndex = New List(Of Integer)

        If Root.Attributes("size") IsNot Nothing Then
            Size = Int16.Parse(Root.Attributes("size").Value)
            If Size > 0 Then
                GrowArray(Size)
            End If
        End If

        'faster to load connections from another file than the document xml
        'this file consists of lines where each line is one row in the connection table
        'first element in the line is the pad ID for that row, the rest of the elements is 
        'PADID;1;2;1;2;0;1;2;0; ->each line is terminated with ;\r\n
        If Root.Attributes("file") IsNot Nothing Then
            Dim Filename As String = Root.Attributes("file").Value

            For Each ZipEntry As Ionic.Zip.ZipEntry In BinData
                If ZipEntry.FileName.ToLower() = Filename Then
                    Dim DataStream As New MemoryStream()
                    Dim Reader As New StreamReader(DataStream)
                    ZipEntry.Extract(DataStream)
                    DataStream.Seek(0, IO.SeekOrigin.Begin)

                    Dim Line As String = Reader.ReadLine()
                    Dim Row As Integer = 0

                    While Line <> ""
                        Dim Values() As String = Line.Split(";")
                        Dim PadID As Integer = Integer.Parse(Values(0))
                        Dim Pad As Pad = PCB.GetLayerObject(PadID)
                        m_Pads.Add(Pad, New PadMap(Pad, Row)) 'add pad to the map, this pad has index Row number
                        m_Index.Add(Row, Pad) 'at this index there is pad as object

                        If Row >= m_ConnectionsCount Then GrowArray(Row)
                        For Col As Integer = 1 To Values.Length - 2
                            m_Connection(Row, Col - 1) = Byte.Parse(Values(Col))
                        Next
                        Row += 1
                        m_ConnectionsCount += 1
                        Line = Reader.ReadLine()
                    End While

                    DataStream.Close()

                End If
            Next
        Else
            'old versions load directly from xml, this was too slow so it was updated to load from CSV file in the project zip
            Dim Rows As Xml.XmlNodeList
            Dim Row As Xml.XmlNode
            Dim Cols As Xml.XmlNodeList
            Dim Col As Xml.XmlNode
            Dim Pad As Pad
            Dim PadId As Integer
            Dim ConnectionType As ConnectionTypes

            Rows = Root.SelectNodes("row")
            m_ConnectionsCount = Rows.Count
            GrowArray(Rows.Count)

            'Dim RowIndex As Integer

            If Rows.Count > 0 Then
                Cols = Rows(0).SelectNodes("col")
                Dim Index As Integer = 0
                For Each Col In Cols
                    PadId = Col.Attributes("pad_id").Value
                    Pad = CType(PCB.GetLayerObject(PadId), Pad)
                    m_Pads.Add(Pad, New PadMap(Pad, Index))
                    m_Index.Add(Index, Pad)
                    Index += 1
                Next
            End If

            For Each Row In Rows
                PadId = Integer.Parse(Row.Attributes("pad_id").Value)
                Pad = CType(PCB.GetLayerObject(PadId), Pad)
                Cols = Row.SelectNodes("col")
                Dim RowIndex As Integer = m_Pads(Pad).Index
                'm_Pads.Add(Pad, New PadMap(Pad, RowIndex))
                'm_Index.Add(RowIndex, Pad)
                ' m_Connection(RowIndex, RowIndex) = ConnectionTypes.ConnectionTypeConnected

                For Each Col In Cols
                    Dim ColPadId As Integer = Col.Attributes("pad_id").Value
                    Dim ColPad As Pad = CType(PCB.GetLayerObject(ColPadId), Pad)
                    Dim ColIndex As Integer = m_Pads(ColPad).Index
                    ConnectionType = Col.Attributes("con_type").Value

                    m_Connection(RowIndex, ColIndex) = ConnectionType

                    'ColIndex += 1
                Next
                'RowIndex += 1
            Next
        End If

        RaiseEvent ConnectionsChanged(Me)
    End Sub

    Public Function Clone() As Object Implements System.ICloneable.Clone
        Dim Cloned As New ConnectionMatrix(m_PCB)
        Cloned.m_ConnectionsSize = m_ConnectionsSize
        Cloned.m_ConnectionsCount = m_ConnectionsCount
        ReDim Cloned.m_Connections(0 To m_ConnectionsSize - 1, 0 To m_ConnectionsSize - 1)
        Array.Copy(m_Connections, Cloned.m_Connections, m_Connections.Length)
        For Each Pad As KeyValuePair(Of Pad, PadMap) In m_Pads
            Cloned.m_Pads.Add(Pad.Key, Pad.Value)
        Next
        For Each Index As KeyValuePair(Of Integer, Pad) In m_Index
            Cloned.m_Index.Add(Index.Key, Index.Value)
        Next
        Cloned.m_UnusedIndex = New List(Of Integer)(m_UnusedIndex)
        Return Cloned
    End Function
End Class
