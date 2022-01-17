using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using MyCodeLibrary.TextProcessing;
using System.Diagnostics;

namespace MyCodeLibrary.FileOperations
{
    // TODO: ����� �����������������, ������ �� ��������� ����, ������� ��������, �� �� ����� ����������� �����.
    // TODO: �������� ����� ������: ����� � ������, ��� ��� ��� ������ � Uri
 
    /// <summary>
    /// ����� ������������ �������� ������ file:/
    /// </summary>
    public class FileLink
    {
        /// <summary>
        /// ������ ����������������� ��� Url ��������
        /// </summary>
        private static string UrlReservedChars = ";?:@$&=+,/{}|\\^~[]`\"%";
        
        /// <summary>
        /// ���� � �����
        /// </summary>
        protected string m_uncPathName;

        public FileLink()
        {
            this.m_uncPathName = String.Empty;
        }

        public FileLink(string uncPath)
        {
            this.m_uncPathName = String.Copy(uncPath);
        }

        #region Properties
        /// <summary>
        /// NT - UNC ������� ���� � �����
        /// </summary>
        public string UncPath
        {
            get { return m_uncPathName; }
            set { m_uncPathName = value; }
        }

        /// <summary>
        /// NR - ��������� ���� � �����
        /// </summary>
        public string LocalPath
        {
            get { return UNCpathToLocalPath(m_uncPathName); }
            set { m_uncPathName = LocalPathToUNCpath(value); }
        }
        /// <summary>
        /// NT- ���������� �� ����?
        /// </summary>
        public bool Exists
        {
            get { return File.Exists(m_uncPathName); }
        }

        #endregion

        /// <summary>
        /// NT-������� ��������� �������� ����� ShellExecute
        /// </summary>
        /// <returns>������� ���������� ������ ��������.</returns>
        public Process Run()
        {
            return Process.Start(this.m_uncPathName);
        }
        /// <summary>
        /// NT-������� ��������� �������� ����� ShellExecute
        /// </summary>
        /// <param name="info">��������� ������� ��������.</param>
        /// <returns>������� ���������� ������ ��������.</returns>
        public Process Run(ProcessStartInfo info)
        {
            return Process.Start(info);
        }

        #region ����������� ������� ������

        /// <summary>
        /// NR-�������������� ������� ���� � ��������� ����
        /// </summary>
        /// <param name="UNCpath">������� ���� � ���������</param>
        /// <returns>���������� ��������� �������� ���� � ���������</returns>
        public static string UNCpathToLocalPath(string UNCpath)
        {
            throw new NotImplementedException();//TODO: add code here
        }
        
        /// <summary>
        /// NT- ���������� ���� � ����� � (����������) ������� ������ �� ����, ��������� ��� ������������� � �������� �����. 
        /// </summary>
        /// <param name="localPath">���������� ���� � �����</param>
        /// <returns>
        /// ���������� ��������� �������������� ������ ����: 
        /// file://localhost/C:/Documents%20and%20Settings/1/%D0%A0%D0%B0%D0%B1%D0%BE%D1%87%D0%B8%D0%B9%20%D1%81%D1%82%D0%BE%D0%BB/12%D0%81%60~!@%23$%25%5E&()_-+=,.txt
        /// </returns>
        public static string LocalPathToUNCpath2(string localPath)
        {
            UriBuilder u = new UriBuilder();
            u.Scheme = Uri.UriSchemeFile;
            u.Path = localPath;
            return u.ToString();
        }
        
        /// <summary>
        /// NT-�������������� ��������� ���� � ������� ����.
        /// ���������� ������ ������ ��� ������.
        /// </summary>
        /// <param name="localPath">��������� �������� ���� � ���������</param>
        /// <returns>
        /// ���������� ������� ���� � �����.
        /// ���������� ������ ������ ��� ������.
        /// </returns>
        public static string LocalPathToUNCpath(string localPath)
        {
            //    Convert a DOS/Windows path name to a file url.
            //            C:\foo\bar\spam.foo
            //                    becomes
            //            file:///C:/foo/bar/spam.foo
            Char[] splitter1 = new char[] { '\\' };
            Char[] splitter2 = new char[] { ':' };
            String[] components = null;

            //    ���� ��� ':' � ����
            if (localPath.IndexOf(':') == -1)//  (!pathname.Contains(":"))
            {
                //��� ����� �����, ������ ������ ����� � ���������� �������
                components = localPath.Split(splitter1); // "\"
                return urlQuote(String.Join("/", components), "/"); //������������� ��������
            }
            //�����, ������ ���� ����� ����� - ����� ������ �� :
            String[] comp = localPath.Split(splitter2); // ":"
            //���������, ��� ���� ����� �����
            if ((comp.Length != 2) || (comp[0].Length > 1))
            {
                //TODO: ��� ������������ ��� ������?
                //String error = "Bad path: " + pathname;
                //throw new System.IO.IOException(error);
                return String.Empty;
            }
            //���������� ����� ����� (�����? ��� �� �����)
            String drive = urlQuote(comp[0].ToUpper(), "/");
            //����� ���� �� �����
            components = comp[1].Split(splitter1);
            //������ ����� ���������� ��������. � ����� ���� �� ����� ��� ������������, ���� / ������� �� ������������. 
            //TODO: ��� ������� ��� ���� ������������ � ����������� �� ���� ��������� �����, ������ � ������.
            String path = "file:///" + drive + ":";
            foreach (String s in components)
                if (s != String.Empty)
                    path = path + "/" + urlQuote(s, "/"); //���, ���� � ����� ����� ����� / �� �� �� ����� ����������� � �������� ����. �� ������-�� ��� �� ����������� �����.

            return path;
        }
        


        /// <summary>
        /// ������������ ������� � ������ ������� %20, ��� �� ������������� � Url
        /// </summary>
        /// <param name="path">������ ��� �������������</param>
        /// <param name="safe">������ ��������, �� ���������� �������������</param>
        /// <returns></returns>
        private static string urlQuote(string path, string safe)
        {
            //    Modified version of urllib.quote supporting unicode.

            //    Each part of a URL, e.g. the path info, the query, etc., has a
            //    different set of reserved characters that must be quoted.

            //    RFC 2396 Uniform Resource Identifiers (URI): Generic Syntax lists
            //    the following reserved characters.

            //    reserved    = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" |
            //                  "$" | ","

            //    Each of these characters is reserved in some component of a URL,
            //    but not necessarily in all of them.

            //    The function is intended for quoting the path
            //    section of a URL.  Thus, it will not encode '/'.  This character
            //    is reserved, but in typical usage the quote function is being
            //    called on a path where the existing slash characters are used as
            //    reserved characters.

            //    The characters u"{", u"}", u"|", u"\", u"^", u"~", u"[", u"]", u"`"
            //    are considered unsafe and should be quoted as well.

            StringBuilder result = new StringBuilder();
            //    for c in s:
            for (int i = 0; i < path.Length; i++)
            {
                Char c = path[i];
                //��� ���� � ������ �������� ������ �������, �� �������� � ������ �����������, ��� ���������� � ���������� safe
                //� ��������� �������� �� ����������� ����� %20 (=������)
                //�������� ������� �������� ��� ������� ����� ����, � ��� �� ���� ������ ����.
                //if c not in safe and (ord(c) < 33 or c in URL_RESERVED):
                if ((safe.IndexOf(c) == -1) && ((Char.ConvertToUtf32(path, i) < 33) || (UrlReservedChars.IndexOf(c) != -1)))
                    result.AppendFormat("%{0:X2}", Char.ConvertToUtf32(path, i));
                else result.Append(c);
            }
            return result.ToString();
        }


        /// <summary>
        /// NT-������� ������� ���� � ����� �� �������������� ��� ����������� ���� � �����
        /// </summary>
        /// <param name="rootpath">��������� ����� ���� ��� �������������� ���� �����</param>
        /// <param name="text">������������� ��� ���������� ���� � �����, ��� ������ ���� file:///C:/data.dat ��� file:///data.dat </param>
        /// <returns>
        /// ���� ���� ������ ���������, ������������ ������� ������ �� ����, ��������� ��� ������������� � �������� �����.
        /// ���� ���� �� ����������, ��� ��������� ����� ������, ������������ String.Empty.
        /// </returns>
        public static String makeUriFromRelativeFilePath(string rootpath, string text)
        {
            String result = String.Empty;
            try
            {
                String absolutePath = text.Trim();

                //���������, ��� ���� ��� ��� ������� ������
                //� ������� �� ��� ���������� ����
                // � ������ ������������ �� �� ���������� - ��� ���������� ������������� ���� � ������� 
                if (Uri.IsWellFormedUriString(absolutePath, UriKind.Absolute))
                {
                    UriBuilder ub = new UriBuilder(absolutePath);
                    absolutePath = ub.Path;
                    ub = null;
                }
                //������� ������ ������� / \ �� ���� �����, ���� ��� ����.
                absolutePath = absolutePath.TrimStart(new Char[] { '/', '\\' });
                //��������� ��� ���� ������������� � ������������� � ����������
                String root = Path.GetPathRoot(absolutePath);
                if ((String.IsNullOrEmpty(root)))
                {
                    //��� ������������� ����
                    //��� ���� ���������� � ����������
                    absolutePath = Path.Combine(rootpath, absolutePath);
                }
                else if (root.Length < 3)
                {
                    //� ��� �� �� ����� ��� ������, ����� root != null � ������ 3 ��������
                    //��������� ���������� ������ �����:  C:\
                    //absolutePath = Path.Combine(rootpath, absolutePath); - absolute path already comes from ImportMenuManager, and we can ignore this case
                    absolutePath = String.Empty;
                }
                //���� ���� �� ������ ���� �� ����������, ���������� ������ ������
                //����� ���������� ������� ���� � �����
                if (File.Exists(absolutePath))
                    result = LocalPathToUNCpath2(absolutePath);//���������� ���� � URI
            }
            catch (Exception)
            {
                result = String.Empty;
            }

            return result;
        }
#endregion
    }//end class
}//end namespace