using System;
using System.Collections.Generic;
using System.Text;

namespace TodoLibrary
{
    /// <summary>
    /// Аргумент события сообщения процесса
    /// </summary>
    public class ApplicationMessageEventArgs: EventArgs
    {
        private readonly String m_Message;
        private readonly Int32 m_Code;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="code">Message code </param>
        /// <param name="message">Message text</param>
        public ApplicationMessageEventArgs(int code, string message)
        {
            this.m_Code = code;
            this.m_Message = message;
        }
        /// <summary>
        /// Message code
        /// </summary>
        public int Code
        { 
            get { return m_Code; } 
        }
        /// <summary>
        /// Message text
        /// </summary>
        public string Message
        {
            get { return this.m_Message; }
        }

    }
    /// <summary>
    /// Аргумент события прогресса процесса
    /// </summary>
    public class ApplicationProgressEventArgs : EventArgs
    {
        private readonly String m_Message;
        private readonly Int32 m_Current;
        private readonly Int32 m_MaxValue;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="current">Current progress value</param>
        /// <param name="maxvalue">Maximum progress value</param>
        /// <param name="message">Message text</param>
        public ApplicationProgressEventArgs(int current, int maxvalue, string message)
        {
            this.m_Current = current;
            this.m_MaxValue = maxvalue;
            this.m_Message = message;
        }
        /// <summary>
        /// Current progress value
        /// </summary>
        public int CurrentValue
        { 
            get { return this.m_Current; } 
        }
        /// <summary>
        /// Maximum progress value
        /// </summary>
        public int MaxValue
        {
            get { return this.m_MaxValue; }
        }

        /// <summary>
        ///Message text
        /// </summary>
        public string Message
        {
            get { return this.m_Message; }
        }
    }
}
