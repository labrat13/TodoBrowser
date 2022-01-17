using System;
using System.IO;

using MyCodeLibrary.TextProcessing.UDE.Core;
using System.Text;

namespace MyCodeLibrary.TextProcessing.UDE
{
    /// <summary>
    /// Default implementation of charset detection interface. 
    /// The detector can be fed by a System.IO.Stream:
    /// <example>
    /// <code>
    /// using (FileStream fs = File.OpenRead(filename)) {
    ///    CharsetDetector cdet = new CharsetDetector();
    ///    cdet.Feed(fs);
    ///    cdet.DataEnd();
    ///    Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
    /// </code>
    /// </example>
    /// 
    ///  or by a byte a array:
    /// 
    /// <example>
    /// <code>
    /// byte[] buff = new byte[1024];
    /// int read;
    /// while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
    ///     Feed(buff, 0, read);
    /// cdet.DataEnd();
    /// Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
    /// </code>
    /// </example>
    /// 
    /// detect file encoding from static function:
    /// 
    /// <example>
    /// <code>
    /// Encoding e1 = CharsetDetector.DetectFileEncoding("C:\\Temp\\pitest.xml");
    /// Encoding e2 = CharsetDetector.DetectFileEncoding("C:\\Temp\\test.txt");
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// ��� ������������� ��������, � �� ��������� ������ �� �������� � 10% ������� ����������� ���������� ��������� ���� ����� �� ���������� ��.
    /// </remarks>
    public class CharsetDetector : UniversalDetector, ICharsetDetector
    {
        /// <summary>
        /// ��������� �������� ���������
        /// </summary>
        private string charset;
        /// <summary>
        /// ����������� ����������� ��� ����� � �������� 0..1
        /// </summary>
        private float confidence;
        /// <summary>
        /// ����������� �� ���������
        /// </summary>
        public CharsetDetector() : base()
        {
        }

        /// <summary>
        /// ��������� �������� ���������
        /// </summary>
        public string Charset
        {
            get { return charset; }
        }
        /// <summary>
        /// ����������� ����������� ��� ����� � �������� 0..1
        /// </summary>
        public float Confidence
        {
            get { return confidence; }
        }

        /// <summary>
        /// ��������� � �������� ����� ������ ��� ���������
        /// </summary>
        /// <param name="stream">�����, ���������� �������� �����</param>
        public void Feed(Stream stream)
        { 
            byte[] buff = new byte[1024];
            int read;
            while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
            {
                Feed(buff, 0, read);
            }
        }
        /// <summary>
        /// ������, ��������� �� ����������� ���������
        /// </summary>
        /// <returns></returns>
        public bool IsDone() 
        {
            return done;
        }
        /// <summary>
        /// �������� �������� � �������� ���������
        /// </summary>
        public override void Reset()
        {
            this.charset = null;
            this.confidence = 0.0f;
            base.Reset();
        }

        /// <summary>
        /// �������������� ��� ������� ��� ��������� ���������� ��������������
        /// </summary>
        /// <param name="charset">�������� ��������� ��� ���� ������ Charsets</param>
        /// <param name="confidence">����������� ��������� (0..1)</param>
        protected override void Report(string charset, float confidence)
        {
            this.charset = charset;
            this.confidence = confidence;
        }

        /// <summary>
        /// NT-���������� ��������� ���������� �����
        /// </summary>
        /// <param name="filepath">���� ���������� �����</param>
        /// <returns>������� ���������� �������� ���������, ���� null ���� ��������� ���������� �� �������.</returns>
        public static string DetectFileEncodingString(String filepath)
        {
            String encod = String.Empty;
            
            using (FileStream fs = File.OpenRead(filepath))
            {
                CharsetDetector cdet = new CharsetDetector();
                //�������� ���������� ����� �������� � ������� - �� ��������� �� ��������.
                cdet.DumpToConsole = false;
                //��������� ����� � �������������
                cdet.Feed(fs);
                cdet.DataEnd();
                encod = cdet.charset;
            }

            return encod;
        }

        /// <summary>
        /// NT-���������� ��������� ���������� �����
        /// </summary>
        /// <param name="filepath">���� ���������� �����</param>
        /// <returns>������� ���������� �������� ���������, ���� ����������� ���������� ApplicationException, ���� ��������� ���������� �� �������.</returns>
        /// <exception cref="ApplicationException">������� ����������� ����������, ���� ���� ��������� ���������� �� �������.</exception>
        public static Encoding DetectFileEncoding(String filepath)
        {
            String encod = String.Empty;
            
            using (FileStream fs = File.OpenRead(filepath))
            {
                CharsetDetector cdet = new CharsetDetector();
                //�������� ���������� ����� �������� � ������� - �� ��������� �� ��������.
                cdet.DumpToConsole = false;
                //��������� ����� � �������������
                cdet.Feed(fs);
                cdet.DataEnd();
                encod = cdet.charset;
            }
            if (String.IsNullOrEmpty(encod))
                throw new ApplicationException("Error: Encoding detection failed!");
            //else
            Encoding enc = Encoding.GetEncoding(encod);
            
            return enc;
        }

    }//end class
}

