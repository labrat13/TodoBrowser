using System;

namespace TodoLibrary
{
    public class TodoItem
    {
        /// <summary>
        /// строка-ключ тодо-итема.
        /// Пример: todo.важно.сложно
        /// </summary>
        private string m_key;
        /// <summary>
        /// строка текста тодо-задачи.
        /// </summary>
        private string m_content;
        /// <summary>
        /// путь к файлу-источнику тодо-задачи.
        /// </summary>
        private string m_sourcePath;
        /// <summary>
        /// Позиция заголовка тодо-задачи как начала тодо-задачи в исходном тексте.
        /// </summary>
        private int m_startPos;
        /// <summary>
        /// Длина заголовка тодо-задачи, символов.
        /// </summary>
        private int m_keyLength;
        /// <summary>
        /// Длина всей тодо-задачи, символов.
        /// </summary>
        private int m_length;

        /// <summary>
        /// Разделитель подключей в составном тодо.важно
        /// </summary>
        private static char[] keySeparator = { '.' };
        /// <summary>
        /// default constructor
        /// </summary>
        public TodoItem()
        {
            this.m_content = string.Empty;
            this.m_key = string.Empty;
            this.m_keyLength = 0;
            this.m_length = 0;
            this.m_sourcePath = String.Empty;
            this.m_startPos = 0;

            return;
        }
        /// <summary>
        /// путь к файлу-источнику тодо-задачи.
        /// </summary>
        public String Source
        {
            get { return this.m_sourcePath; }
            set { this.m_sourcePath = value; }
        }
        /// <summary>
        /// строка-ключ тодо-итема.
        /// Пример: todo.важно.сложно
        /// </summary>
        public String Key
        {
            get { return this.m_key; }
            set { this.m_key = value; }
        }
        /// <summary>
        /// строка текста тодо-задачи.
        /// </summary>
        public String Content
        {
            get { return this.m_content; }
            set { this.m_content = value; }
        }
        /// <summary>
        /// Позиция заголовка тодо-задачи как начала тодо-задачи в исходном тексте.
        /// </summary>
        public Int32 StartPosition
        {
            get { return this.m_startPos; }
            internal set { this.m_startPos = value; }
        }
        /// <summary>
        /// Длина заголовка тодо-задачи, символов.
        /// </summary>        
        public Int32 KeyLength
        {
            get { return this.m_keyLength; }
            internal set { this.m_keyLength = value; }
        }
        /// <summary>
        /// Длина всей тодо-задачи, символов.
        /// </summary>
        public Int32 Length
        {
            get { return this.m_length; }
            internal set { this.m_length = value; }
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", this.m_key, this.m_content);
        }

        /// <summary>
        /// NT-получить ключи составного тодо как массив строк.
        /// </summary>
        /// <returns></returns>
        public string[] getKeys()
        {
            return this.m_key.Split(keySeparator, StringSplitOptions.RemoveEmptyEntries);
        }




    }
}
