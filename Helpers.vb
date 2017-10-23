Imports System.Runtime.CompilerServices

Public Class TreeNodeComparer
    Implements IComparer(Of TreeNode)

    Public Function Compare(ByVal x As System.Windows.Forms.TreeNode, ByVal y As System.Windows.Forms.TreeNode) As Integer Implements System.Collections.Generic.IComparer(Of System.Windows.Forms.TreeNode).Compare


        Return StrComp(x.Text, y.Text, CompareMethod.Text)
    End Function
End Class

Public Class SortableTreeNode
    Inherits TreeNode

    Sub New()
        MyBase.New()
    End Sub

    Sub New(ByVal text As String)
        MyBase.New(text)
    End Sub

    Public Sub Sort()
        Me.Sort(0, Me.Nodes.Count, Nothing)
    End Sub

    Public Sub Sort(ByVal index As Integer, ByVal count As Integer, ByVal comparer As IComparer(Of SortableTreeNode))
        Dim nodes(Me.Nodes.Count - 1) As TreeNode
        Me.Nodes.CopyTo(nodes, 0)
        Array.Sort(Of TreeNode)(nodes, index, count, New TreeNodeComparer)
        Me.Nodes.Clear()
        Me.Nodes.AddRange(nodes)
    End Sub
End Class


Public Class NameExistsException
    Inherits Exception

    Public Sub New(ByVal Message As String)
        MyBase.New(Message)
    End Sub

End Class

Public Class ExtentedDictionary(Of key, value)
    Inherits Dictionary(Of Key, Value)
    Protected m_ReadonlyDictionary As ReadOnlyDictionary(Of key, value)

    Public Overridable Function GetReadonlyDictionary() As ReadOnlyDictionary(Of key, value)
        If m_ReadonlyDictionary Is Nothing Then m_ReadonlyDictionary = New ReadOnlyDictionary(Of key, value)(Me)
        Return m_ReadonlyDictionary
    End Function

End Class

Public Class ReadOnlyDictionary(Of Key, Value)
    Implements IDictionary(Of Key, Value)

    Protected m_Dictionary As Dictionary(Of Key, Value)

    Public Sub New(ByVal Source As Dictionary(Of Key, Value))
        m_Dictionary = Source
    End Sub

    Private Sub Add(ByVal item As System.Collections.Generic.KeyValuePair(Of Key, Value)) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).Add
        Throw New ReadOnlyException("The collection is read only.")
    End Sub

    Private Sub Clear() Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).Clear
        Throw New ReadOnlyException("The collection is read only.")
    End Sub

    Public Function Contains(ByVal item As System.Collections.Generic.KeyValuePair(Of Key, Value)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).Contains
        Return CType(m_Dictionary, ICollection(Of KeyValuePair(Of Key, Value))).Contains(item)
    End Function

    Public Sub CopyTo(ByVal array() As System.Collections.Generic.KeyValuePair(Of Key, Value), ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).CopyTo
        CType(m_Dictionary, ICollection(Of KeyValuePair(Of Key, Value))).CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).Count
        Get
            Return m_Dictionary.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).IsReadOnly
        Get
            Return True
        End Get
    End Property

    Private Function Remove(ByVal item As System.Collections.Generic.KeyValuePair(Of Key, Value)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).Remove
        Throw New ReadOnlyException("The collection is read only.")
    End Function

    Private Sub Add1(ByVal key As Key, ByVal value As Value) Implements System.Collections.Generic.IDictionary(Of Key, Value).Add
        Throw New ReadOnlyException("The collection is read only.")
    End Sub

    Public Function ContainsKey(ByVal key As Key) As Boolean Implements System.Collections.Generic.IDictionary(Of Key, Value).ContainsKey
        Return m_Dictionary.ContainsKey(key)
    End Function

    Default Public Property Item(ByVal key As Key) As Value Implements System.Collections.Generic.IDictionary(Of Key, Value).Item
        Get
            Return m_Dictionary(key)
        End Get
        Set(ByVal value As Value)
            Throw New ReadOnlyException("The collection is read only.")
        End Set
    End Property

    Public ReadOnly Property Keys() As System.Collections.Generic.ICollection(Of Key) Implements System.Collections.Generic.IDictionary(Of Key, Value).Keys
        Get
            Return m_Dictionary.Keys()
        End Get
    End Property

    Private Function Remove1(ByVal key As Key) As Boolean Implements System.Collections.Generic.IDictionary(Of Key, Value).Remove
        Throw New ReadOnlyException("The collection is read only.")
    End Function

    Public Function TryGetValue(ByVal key As Key, ByRef value As Value) As Boolean Implements System.Collections.Generic.IDictionary(Of Key, Value).TryGetValue
        Return m_Dictionary.TryGetValue(key, value)
    End Function

    Public ReadOnly Property Values() As System.Collections.Generic.ICollection(Of Value) Implements System.Collections.Generic.IDictionary(Of Key, Value).Values
        Get
            Return m_Dictionary.Values()
        End Get
    End Property

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of Key, Value)) Implements System.Collections.Generic.IEnumerable(Of System.Collections.Generic.KeyValuePair(Of Key, Value)).GetEnumerator
        Return m_Dictionary.GetEnumerator()
    End Function

    Private Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return m_Dictionary.GetEnumerator()
    End Function
End Class
