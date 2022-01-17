using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TodoLibrary
{
    // Delegate declaration.
    /// <summary>
    /// Обработчик события сообщения приложения
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ApplicationMessageEventHandler(object sender, ApplicationMessageEventArgs e);
    /// <summary>
    /// Обработчик события обновления прогресс-бара
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ApplicationProgressEventHandler(object sender, ApplicationProgressEventArgs e);
    
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// <code>
    ///         static void Main(string[] args)
    /// {
    ///    //source folder
    ///    string srcFolder = "D:\\";
    ///    //задаем список тегов - его обычно следует хранить в настройках приложения.
    ///    string tags = "todo TODO done DONE question QUESTION надо НАДО тодо ТОДО вопрос ВОПРОС";
    ///    //задаем расширения файлов
    ///    string exts = ".txt xml cs cpp";
    ///    TodoEngine engine = new TodoEngine();
    ///    engine.Init(exts, tags, Encoding.GetEncoding(1251));
    ///    //add events
    ///    engine.AppMessageHandler += new ApplicationMessageEventHandler(engine_AppMessageHandler);
    ///    engine.AppProgressHandler += new ApplicationProgressEventHandler(engine_AppProgressHandler);
    ///
    ///    TodoItemCollection col = engine.getTodoItemsFromFolder(srcFolder);
    ///
    ///    Console.WriteLine("Найденные задачи:");
    ///    foreach (TodoItem it in col.Items)
    ///        Console.WriteLine(it.ToString());
    ///
    ///    return;
    /// }
    /// /// <summary>
    /// /// Print progress message to console
    /// /// </summary>
    /// /// <param name="sender"></param>
    /// /// <param name="e"></param>
    /// //static void engine_AppProgressHandler(object sender, ApplicationProgressEventArgs e)
    /// //{
    ///    Console.WriteLine("Файл {0} из {1}: {2}", e.CurrentValue, e.MaxValue, e.Message);
    /// }
    /// /// <summary>
    /// /// Print error message to console
    /// /// </summary>
    /// /// <param name="sender"></param>
    /// /// <param name="e"></param>
    /// static void engine_AppMessageHandler(object sender, ApplicationMessageEventArgs e)
    /// {
    ///    Console.WriteLine(e.Message);
    /// }
    /// </code>
    /// </example>
    public class TodoEngine
    {
        /// <summary>
        /// Массив строк допустимых расширений файлов
        /// </summary>
        private string[] m_extensions;
        /// <summary>
        /// Массив строк тегов тодо-задач или надо-задач
        /// </summary>
        private string[] m_tags;
        /// <summary>
        /// Кодировка по умолчанию для текстовых файлов.
        /// </summary>
        private Encoding m_defaultEncoding;
        /// <summary>
        /// Количество символов текста перед надо-тегом.
        /// </summary>
        private Int32 m_numCharsBeforeNado;
        /// <summary>
        /// Количество символов текста после надо-тега.
        /// </summary>
        private Int32 m_numCharsAfterNado;


        /// <summary>
        /// Конструктор
        /// </summary>
        public TodoEngine()
        {
            this.m_extensions = new string[0];
            this.m_tags = new string[0];
            this.m_defaultEncoding = Encoding.Default;
            this.m_numCharsAfterNado = 50;
            this.m_numCharsBeforeNado = 50;
        }

        #region Properties
        /// <summary>
        /// Кодировка по умолчанию для текстовых файлов.
        /// </summary>
        public Encoding DefaultEncoding
        { 
            get { return this.m_defaultEncoding; }
            set { this.m_defaultEncoding = value;}
        }

        /// <summary>
        /// Массив строк допустимых расширений файлов
        /// </summary>
        /// <example>
        /// .txt .cs .c .cpp .h .hpp
        /// </example>
        public string[] Extensions
        {
            get { return this.m_extensions; }
            set { this.m_extensions = value; }
        }
        /// <summary>
        /// Строка тегов тодо-задач
        /// </summary>
        /// <example>
        /// "todo TODO"
        /// </example>
        public string[] Tags
        {
            get { return this.m_tags; }
            set { this.m_tags = value; }
        }
        /// <summary>
        /// Количество символов текста перед надо-тегом.
        /// Игнорируется при поиске тодо-задач.
        /// </summary>
        public Int32 NumberOfCharsBeforeNado
        {
            get { return this.m_numCharsBeforeNado; }
            set { this.m_numCharsBeforeNado = value; }
        }
        /// <summary>
        /// Количество символов текста после надо-тега.
        /// Игнорируется при поиске тодо-задач.
        /// </summary>
        public Int32 NumberOfCharsAfterNado
        {
            get { return this.m_numCharsAfterNado; }
            set { this.m_numCharsAfterNado = value; }
        }
#endregion
        /// <summary>
        /// NT-Инициализировать движок
        /// </summary>
        /// <param name="exts">Строка расширений файлов с задачами</param>
        /// <param name="tags">Массив строк тегов тодо-задач или надо-задач</param>
        /// <param name="enc">Кодировка по умолчанию для текстовых файлов.</param>
        /// <param name="charsBeforeNado">Количество символов текста перед надо-тегом. Игнорируется при поиске тодо-задач.</param>
        /// <param name="charsAfterNado">Количество символов текста после надо-тега. Игнорируется при поиске тодо-задач.</param>
        /// <example>
        /// <code>
        /// Init(".txt .cs .c .cpp .h .hpp", "todo TODO done DONE QUESTION question", Encoding.Default);
        /// </code>
        /// </example>
        public void Init(string exts, string tags, Encoding enc, int charsBeforeNado, int charsAfterNado)
        {
            
            char[] splitter = new char[] { ' ' };
            //1. parse tags string
            this.m_tags = tags.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            //2. parse extensions string
            this.m_extensions = exts.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            //массив расширений сейчас не может содержать пустые строки
            //если расширение из исходной строки не содержит точки впереди, подставим ее сами
            for(int i = 0; i < m_extensions.Length; i++)
            {
                string s = m_extensions[i];
                if (s.StartsWith(".") == false)
                    m_extensions[i] = String.Concat(".", s);
            }
            //3. set default encoding as 
            this.m_defaultEncoding = enc;
            this.m_numCharsBeforeNado = charsBeforeNado;
            this.m_numCharsAfterNado = charsAfterNado;

            return;
        }

        #region Events for console and progress bar
        /// <summary>
        /// Обработчик события изменения прогресс-бара
        /// </summary>
        public event ApplicationProgressEventHandler AppProgressHandler;
        /// <summary>
        /// Обработчик события сообщения приложения
        /// </summary>
        public event ApplicationMessageEventHandler AppMessageHandler;

        /// <summary>
        /// Raise Progress event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnProgress(ApplicationProgressEventArgs e)
        {
            ApplicationProgressEventHandler handler = AppProgressHandler;
            if (handler != null)
            {
                // Invokes the delegates. 
                handler(this, e);
            }
        }
        /// <summary>
        /// Raise Message event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessage(ApplicationMessageEventArgs e)
        {
            ApplicationMessageEventHandler handler = AppMessageHandler;
            if (handler != null)
            {
                // Invokes the delegates. 
                handler(this, e);
            }
        }

        #endregion

        #region Вспомогательные функции движка
        /// <summary>
        /// NT-Получить файлы-источники задач, соответствующие допустимым расширениям файлов
        /// </summary>
        /// <param name="srcFolder">Путь к папке проекта</param>
        /// <returns></returns>
        private string[] getSrcFiles(string srcFolder)
        {
            //TODO: после отладки функции добавить этот код функции в MyCodeLibrary.FileOperations.CFileOperations 
            //public static string[] getFolderFilesByExts(String folder, string[] fileExtensions, SearchOption option)
            //как более оптимизированный способ выбрать все файлы с заданными расширениями.
            
            //Важно: выдает ошибку, если в каталоге встречается папка или файл с запрещенным доступом.
            //Например, папка D:\SystemVolumeInformation
            //После нее исполнение останавливается и прочие папки не просматриваются.
            //- Надо обходить каталоги рекурсивно, игнорируя эти исключения доступа.
            //- Замена паттерна * на *.* также выдает исключение доступа.
            //string[] files = Directory.GetFiles(srcFolder, "*", SearchOption.AllDirectories);
            
            //переделано на рекурсивный обход каталога и перехват исключений
            string[] files = getFiles(srcFolder, SearchOption.AllDirectories);
            
            List<string> result = new List<string>();
            foreach (String s in files)
            {
                if (isAllowedExtension(s))
                    result.Add(s);
            }

            return result.ToArray();
        }

        /// <summary>
        /// NT-Получить файлы-источники задач, соответствующие допустимым расширениям файлов
        /// </summary>
        /// <param name="srcFolder">Путь к папке проекта</param>
        /// <param name="option">SearchOption</param>
        /// <returns></returns>
        private string[] getFiles(string srcFolder, SearchOption option)
        {
            List<String> result = new List<string>();
            DirectoryInfo di = new DirectoryInfo(srcFolder);
            recursiveGetFiles(result, di, option);

            return result.ToArray();
        }

        private void recursiveGetFiles(List<string> result, DirectoryInfo di, SearchOption option)
        {
            try
            {
                //process files
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                {
                    result.Add(f.FullName);
                }
                //process directories
                if (option != SearchOption.AllDirectories)
                    return;

                DirectoryInfo[] dir = di.GetDirectories();
                foreach (DirectoryInfo d in dir)
                    recursiveGetFiles(result, d, option);
            }
            catch (Exception ex)
            {
                OnMessage(new ApplicationMessageEventArgs(0, String.Format("Ошибка {0} для {1}", ex.GetType().ToString(), di.FullName)));
            }
            return;
        }

        /// <summary>
        /// NT-Определить кодировку текстового файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Функция возвращает кодировку файла. Если определить кодировку не удалось, функция возвращает установленную в объекте кодировку по умолчанию.</returns>
        private Encoding detectEncoding(string file)
        {
            //определить кодировку этим глючным вероятностным определителем
            string enc = MyCodeLibrary.TextProcessing.UDE.CharsetDetector.DetectFileEncodingString(file);
            if (String.IsNullOrEmpty(enc))
            {
                //создать событие Ошибка и передать ему текст сообщения об ошибке "Не распознана кодировка для файла {0}"
                
                //Console.WriteLine("Не распознана кодировка для файла {0}. Использована кодировка по умолчанию: {1}.", file, this.m_defaultEncoding.WebName);
                String msg = String.Format("Ошибка: Не распознана кодировка для файла {0}. Использована кодировка по умолчанию: {1}.", file, this.m_defaultEncoding.WebName);
                ApplicationMessageEventArgs apma = new ApplicationMessageEventArgs(0, msg);
                this.OnMessage(apma);
                
                return this.m_defaultEncoding;
            }
            else return Encoding.GetEncoding(enc);

            //TODO: Этот детектор не работает толком! Нет гарантии, что он все текстовые файлы правильно прочитал!
            //- он не тестирован, текстовые файлы блокнота он определяет не всегда правильно!
            //- надо тестировать его на определение текстовых файлов разных типов: win-1251, unicode, UTF-8 UTF-16 итп.
            //  и с разным содержимым на русском языке.
        }

        /// <summary>
        /// NT-убедиться, что файл имеет допустимое расширение.
        /// </summary>
        /// <param name="s">Путь к файлу</param>
        /// <returns></returns>
        private bool isAllowedExtension(string s)
        {
            //TODO: если файл не имеет расширения, эта функция вернет false. Следовательно, файлы без расширения эта программа не читает.
            string ext = Path.GetExtension(s); //'.ext'
            foreach (string t in this.m_extensions)
                if (String.Equals(ext, t, StringComparison.OrdinalIgnoreCase) == true)
                    return true;

            return false;
        }

        #endregion

        #region Todo extraction functions


        /// <summary>
        /// NT-Извлечь тодо-задачи из указанной папки
        /// </summary>
        /// <param name="srcFolder">Папка файлов-источников задач</param>
        /// <returns>Функция возвращает коллекцию задач</returns>
        public TodoItemCollection getTodoItemsFromFolder(string srcFolder)
        {
            //1. выбрать из указанной папки все файлы с допустимыми расширениями
            string[] files = getSrcFiles(srcFolder);
            TodoItemCollection result = new TodoItemCollection();//создать просто пустую коллекцию
            result.ContentType = TodoEngineMode.TodoItems;//установить флаг что содержимое коллекции - тодо-задачи
            if (files.Length == 0) return result; //быстро выйти, если нет файлов для анализа
            
            //2. Для каждого исходного файла:
            for(int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                // тут вызвать событие и передать ему данные для отображения прогресса на форме
                // - путь к файлу
                // - общее число файлов
                // - число уже обработанных файлов
                ApplicationProgressEventArgs apea = new ApplicationProgressEventArgs(i, files.Length, file);
                this.OnProgress(apea);
                //тут можно было бы использовать функцию getTodoItemsFromFile(string file)
                //вместо кода ниже, поскольку он почти одинаковый, но вдруг потребуются различия...
                try
                {
                    //2.1 Определить кодировку текста файла
                    Encoding enc = detectEncoding(file);
                    //2.2 Собрать ссылку на файл документа - отменено.
                    //Поскольку оказалось, что формат ссылки зависит от типа документа,
                    // то вписывать надо путь к файлу документа, а ссылку формировать позже по месту использования.
                    //2.3 Извлечь тодо-задачи из файла
                    StreamReader sr = new StreamReader(file, enc);
                    string text = sr.ReadToEnd();
                    sr.Close();
                    //добавить задачи в общий список тодо-задач 
                    //add items to result collection
                    result.Add(TodoItemCollection.ExtractTodos(file, text, this.m_tags));
                }
                catch (Exception ex)
                {
                    //Создать событие Ошибка и передать ему текст сообщения об ошибке
                    //не выбрасывать исключение, так как файлы часто бывают недоступны
                    //не прерывать из-за них общий процесс поиска задач, иначе часть задач никогда не будет обнаружена этим алгоритмом.

                    String msg = String.Format("Ошибка: исключение {0} для файла: {1}", ex.GetType().ToString(), file);
                    ApplicationMessageEventArgs apma = new ApplicationMessageEventArgs(0, msg);
                    this.OnMessage(apma);
                }

            }
            //4 вернуть список тодо-задач
            return result;
        }

        /// <summary>
        /// NT-Извлечь тодо-задачи из указанного файла
        /// </summary>
        /// <param name="file">Файл-источник задач</param>
        /// <returns>Функция возвращает коллекцию задач</returns>
        public TodoItemCollection getTodoItemsFromFile(string file)
        {
            TodoItemCollection result = new TodoItemCollection();//создать просто пустую коллекцию

            try
            {
                //2.1 Определить кодировку текста файла
                Encoding enc = detectEncoding(file);
                //2.2 Собрать ссылку на файл документа - отменено.
                //Поскольку оказалось, что формат ссылки зависит от типа документа,
                // то вписывать надо путь к файлу документа, а ссылку формировать позже по месту использования.
                //2.3 Извлечь тодо-задачи из файла
                StreamReader sr = new StreamReader(file, enc);
                string text = sr.ReadToEnd();
                sr.Close();
                //добавить задачи в общий список тодо-задач 
                //add items to result collection
                result = TodoItemCollection.ExtractTodos(file, text, this.m_tags);
            }
            catch (Exception ex)
            {
                //Создать событие Ошибка и передать ему текст сообщения об ошибке
                //не выбрасывать исключение, так как файлы часто бывают недоступны
                //не прерывать из-за них общий процесс поиска задач, иначе часть задач никогда не будет обнаружена этим алгоритмом.

                String msg = String.Format("Ошибка: исключение {0} для файла: {1}", ex.GetType().ToString(), file);
                ApplicationMessageEventArgs apma = new ApplicationMessageEventArgs(0, msg);
                this.OnMessage(apma);
            }

            //4 вернуть список тодо-задач
            return result;
        }

        #endregion

        #region Nado Extraction functions
        /// <summary>
        /// NR-Извлечь надо-задачи из указанного файла
        /// </summary>
        /// <param name="file">Файл-источник задач</param>
        /// <returns>Функция возвращает коллекцию задач</returns>
        public TodoItemCollection getNadoItemsFromFile(string file)
        {
            throw new NotImplementedException();//TODO: add code here
        }
        /// <summary>
        /// NR-Извлечь надо-задачи из указанной папки
        /// </summary>
        /// <param name="srcFolder">Папка файлов-источников задач</param>
        /// <returns>Функция возвращает коллекцию задач</returns>
        public TodoItemCollection getNadoItemsFromFolder(string srcFolder)
        {
            //1. выбрать из указанной папки все файлы с допустимыми расширениями
            string[] files = getSrcFiles(srcFolder);
            TodoItemCollection result = new TodoItemCollection();//создать просто пустую коллекцию
            result.ContentType = TodoEngineMode.NadoItems;//установить флаг что содержимое коллекции - надо-задачи
            if (files.Length == 0) return result; //быстро выйти, если нет файлов для анализа

            //2. Для каждого исходного файла:
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                // тут вызвать событие и передать ему данные для отображения прогресса на форме
                // - путь к файлу
                // - общее число файлов
                // - число уже обработанных файлов
                ApplicationProgressEventArgs apea = new ApplicationProgressEventArgs(i, files.Length, file);
                this.OnProgress(apea);
                //тут можно было бы использовать функцию getNadoItemsFromFile(string file)
                //вместо кода ниже, поскольку он почти одинаковый, но вдруг потребуются различия...
                try
                {
                    //2.1 Определить кодировку текста файла
                    Encoding enc = detectEncoding(file);
                    //2.2 Собрать ссылку на файл документа - отменено.
                    //Поскольку оказалось, что формат ссылки зависит от типа документа,
                    // то вписывать надо путь к файлу документа, а ссылку формировать позже по месту использования.
                    //2.3 Извлечь тодо-задачи из файла
                    StreamReader sr = new StreamReader(file, enc);
                    string text = sr.ReadToEnd();
                    sr.Close();
                    //добавить задачи в общий список тодо-задач 
                    //add items to result collection
                    result.Add(TodoItemCollection.ExtractNados(file, text, this.m_tags, this.m_numCharsBeforeNado, this.m_numCharsAfterNado));
                }
                catch (Exception ex)
                {
                    //Создать событие Ошибка и передать ему текст сообщения об ошибке
                    //не выбрасывать исключение, так как файлы часто бывают недоступны
                    //не прерывать из-за них общий процесс поиска задач, иначе часть задач никогда не будет обнаружена этим алгоритмом.

                    String msg = String.Format("Ошибка: исключение {0} для файла: {1}", ex.GetType().ToString(), file);
                    ApplicationMessageEventArgs apma = new ApplicationMessageEventArgs(0, msg);
                    this.OnMessage(apma);
                }

            }
            //4 вернуть список тодо-задач
            return result;
        }

        #endregion
    }
}
