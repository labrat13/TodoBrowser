using System;
using System.Collections.Generic;

namespace MyCodeLibrary
{   
    //��� ������ ��� ��������� ��������� ��������� ������
    
    /// <summary>
    /// ��� ����� ����, �������� Dictionary, ��:
    /// - �� �������� KeyValuePair<> �����������
    /// - �� �������� ������� �������� ��������
    /// - ������������� ������ ���� �������� ��� �������� ��������� � �� ����� ���� ������� �������.
    /// - ����� ����� �������� ��������� ��������� ����� ������.
    /// - ���� ���������� ������ ��������� ������ = 0, ���������� �������������� ������ ���������, ������ ������� Dictionary.
    /// </summary>
    /// <typeparam name="KeyType">��� ������ �����</typeparam>
    /// <typeparam name="ValueType">��� ������ ��������</typeparam>
    public class Cache<KeyType, ValueType>
    {
        /// <summary>
        /// ������������ ����� ���������, ����������  � ����
        /// </summary>
        private int _capacity;
        /// <summary>
        /// ������� ������
        /// </summary>
        private Queue<KeyType> _keyQ;
        /// <summary>
        /// �������� ��������� ������
        /// </summary>
        private Dictionary<KeyType, ValueType> _contents;
        
        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="initialCapacity">��������� ������� ���������</param>
        /// <param name="capacity">���������� ������� ���������. ���� ������� 0, ������� ��������� �� ����������.</param>
        public Cache(int initialCapacity, int capacity)
        {
            _capacity = capacity;
            _contents = new Dictionary<KeyType, ValueType>(initialCapacity);

            if (capacity > 0)
                _keyQ = new Queue<KeyType>(initialCapacity);
        }
        /// <summary>
        /// ����������[key]
        /// </summary>
        /// <param name="key">�������� �����</param>
        /// <returns></returns>
        public ValueType this[KeyType key]
        {
            get
            {
                ValueType val;
                if (_contents.TryGetValue(key, out val))
                    return val;
                else
                    return default(ValueType);//This will be null for reference types and zero for value types.
            }
            set { InternalAdd(key, value); }
        }
        /// <summary>
        /// �������� ���� � �������� � ���������
        /// </summary>
        /// <param name="key">����</param>
        /// <param name="value">��������</param>
        public void Add(KeyType key, ValueType value)
        {
            InternalAdd(key, value);
        }
        /// <summary>
        /// �������� ���� � �������� � ���������
        /// </summary>
        /// <param name="key">����</param>
        /// <param name="value">��������</param>
        private void InternalAdd(KeyType key, ValueType value)
        {
            if (!_contents.ContainsKey(key))
            {

                if (_capacity > 0)
                {
                    _keyQ.Enqueue(key);//�������� ������ � ����� �������

                    if (_keyQ.Count > _capacity)
                        _contents.Remove(_keyQ.Dequeue());//������� �� ������� ������ � ��� ����, ����������� � ������ �������.
                }
            }

            _contents[key] = value;
        }
    }
}
