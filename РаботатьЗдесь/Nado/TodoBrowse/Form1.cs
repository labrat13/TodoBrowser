using System;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using TodoLibrary;
using System.IO;

namespace TodoBrowse
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// App title string
        /// </summary>
        private const string m_ApplicationTitle = "TodoBrowse";
        /// <summary>
        /// Название раздела, служащее одновременно ключом-разделителем заголовка 
        /// </summary>
        private const string m_FoundedTagsHeader = "Найденные элементы: ";
        /// <summary>
        /// Поле для хранения извлеченных тодо-задач или надо-задач, которые сейчас отображаются в окне приложения.
        /// И которые надо выводить в файл, если пользователь пожелает. 
        /// </summary>
        private TodoItemCollection m_extractedItems;

        public Form1()
        {
            InitializeComponent();
            
        }


        /// <summary>
        /// NT-
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            setTitleBarText(null);
            showStatusText("Для просмотра задач надо просканировать папку.");
        }

        /// <summary>
        /// NT-
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save settings before exit
            Properties.Settings.Default.Save();

            return;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
        /// <summary>
        /// NT-Обработчик клика по ссылке в RichText контроле
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            String linktext = e.LinkText;
            
            try
            {
                linktext = makeSpecialLink(linktext);
                Process.Start(linktext);
            }
            catch (Exception ex)
            {
                string msg = String.Format("При открытии ссылки {0} возникло исключение {1} : {2}", linktext, ex.GetType().ToString(), ex.Message);
                MessageBox.Show(msg, "Ошибка при открытии файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //TODO: как бы запускать документ из ссылки - не в дефолтовом приложении, а в указанном для данного расширения?
            //нужен еще диалог для сопоставления расширения и приложения.
            //и хранить это сопоставление и командную строку - в настройках приложения.
            //а пока - только через ShellExecute, поскольку иначе из ссылки надо извлекать путь к файлу документа.

            return;
        }
        
        #region Обработчики главного меню приложения
        
        /// <summary>
        /// NT-File-FindNado  menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void искатьнадоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindNado();

            return;
        }

        /// <summary>
        /// NT-File-FindTodos  menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void искатьТегиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: выбрать папку и запустить поиск тодо
            FindTodos();

            return;
        }
        /// <summary>
        /// NR - File-Save as menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void сохранитьКакюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //если коллекция пустая, еще не было сканирования, предложить пользователю сначала выполнить сканирование.
            if (this.m_extractedItems == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine("Нечего выводить в файл!");
                sb.AppendLine("Сначала выполните поиск тодо-задач либо Надо-задач.");
                sb.AppendLine("");
                //вывести на экран
                this.richTextBox1.AppendText(sb.ToString());

                return;
            }

            //TODO: сохранить содержимое ричтекста согласно расширению: txt rtf html
            //расширение выбирает пользователь в окне диалога сохранения файла
            //это имеет смысл только для форматов, в которых работают ссылки вида file:///
            //- данные брать из this.m_extractedItems, флаг ContentType покажет, тодо-задачи или Надо-задачи хранятся в коллекции
            
            //Specify saving file path and select file type
            //FilterIndex: RichTextFormat = 1;HTML files = 2;LittleTasks files = 3;Unicode text files = 4;ANSI text files = 5;All = 6;
            String fileFormats = "RichTextFormat files(*.rtf)|*.rtf|HTML files(*.html)|*.html|LittleTasks files(*.tasks)|*.tasks|Unicode text files(*.txt)|*.txt|ANSI text files (*.txt)|*.txt|All files (*.*)|*.*";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.AutoUpgradeEnabled = true;
            sfd.CheckPathExists = true;
            sfd.DefaultExt = "";
            sfd.Filter = fileFormats;
            sfd.FilterIndex = 1;//first item = 1;
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.OverwritePrompt = true;
            sfd.RestoreDirectory = true;
            sfd.ShowHelp = false;
            sfd.SupportMultiDottedExtensions = true;
            sfd.Title = "Сохранить как";
            sfd.ValidateNames = true;
            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            //store text to file
            saveFileWithType(sfd.FilterIndex, sfd.FileName, this.richTextBox1);

            return;
        }

        /// <summary>
        /// NT-File-Exit  menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: закрыть приложение
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /// <summary>
        /// NR Help-Help menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void справкаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //TODO: показать chm-справку
        }
        /// <summary>
        /// NT-Help-About menu handler 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 f = new AboutBox1();
            f.ShowDialog(this);

            return;
        }
#endregion


        #region Status bar functions
        //не нужна - ее функцию выполняет showStatusText()
        ///// <summary>
        ///// NT-Скрыть прогресс-бар в строке состояния
        ///// </summary>
        //private void hideProgressBar()
        //{
        //    this.toolStripProgressBar1.Visible = false;
        //}

        /// <summary>
        /// NT-Показать прогресс-бар в строке состояния
        /// </summary>
        /// <param name="current">Значение прогресса</param>
        /// <param name="max">Максимальное значение прогресса</param>
        /// <param name="text">Текст сообщения.</param>
        private void showProgressBar(int current, int max, string text)
        {
            //show status progress bar if need
            if (this.toolStripProgressBar1.Visible == false)
            {
                this.toolStripProgressBar1.Visible = true;
            }
            //calculate new position for status bar
            if (max <= 0) max = 1;
            if (max <= current) max = current;
            float maxpos = (float)Math.Abs(max);
            float cur = (float)Math.Abs(current);
            float r1 = (cur / maxpos) * 100.0f;
            int r2 = (Int32)r1;
            //update progress bar if need
            if (this.toolStripProgressBar1.Value != r2)
                this.toolStripProgressBar1.Value = r2;
            //set new status text
            this.toolStripStatusLabel1.Text = text;

            return;
        }
        
        /// <summary>
        /// Обновить текст в строке состояния
        /// </summary>
        /// <param name="text">Текст сообщения.</param>
        private void showStatusText(string text)
        {
            this.toolStripProgressBar1.Visible = false;
            this.toolStripStatusLabel1.Text = text;
        }

        #endregion

        #region TitleBar functions
        private void setTitleBarText(string text)
        {
            if (String.IsNullOrEmpty(text))
                this.Text = m_ApplicationTitle;
            else
                this.Text = m_ApplicationTitle + " - " + text;
        }

        #endregion

        #region Find Todo functions
        /// <summary>
        /// RT-искать тодо
        /// </summary>
        private void FindTodos()
        {
            //заблокировать форму от действий пользователя на время выполнения операции
            this.UseWaitCursor = true;
            //очистить ричтекстбокс
            this.richTextBox1.Clear();
            //Запросить у пользователя папку для поиска
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Укажите каталог для поиска тодо-задач:";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowNewFolderButton = false;
            //установить предыдущую выбранную папку.
            fbd.SelectedPath = Properties.Settings.Default.LastFolder;
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                //выходим по Отмена
                //разблокировать форму от действий пользователя после завершения операции
                this.UseWaitCursor = false;
                return;
            }
            String folder = fbd.SelectedPath;
            fbd.Dispose();
            fbd = null;
            //запомнить предыдущую выбранную папку.
            Properties.Settings.Default.LastFolder = folder;
            //изменить заголовок окна приложения
            this.setTitleBarText(folder);

            //вывести шапку для файла отчета, если пользователь надумает его сделать
            richTextBox1.AppendText(String.Format("Поиск тодо-задач в каталоге {0} {1}", folder, Environment.NewLine));
            richTextBox1.AppendText(String.Format("Создан {0} {1}", DateTime.Now.ToString(), Environment.NewLine));
            richTextBox1.AppendText(String.Format("Используйте текстовый редактор с распознаванием гиперссылок, не Блокнот.{0}", Environment.NewLine));
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(String.Format("Начат просмотр каталога {0}..{1}", folder, Environment.NewLine));
            Application.DoEvents();

            //do work with todo
            string tags = Properties.Settings.Default.TodoTags;
            string exts = Properties.Settings.Default.ReadFileExts;
            string defaultEncoding = Properties.Settings.Default.DefaultEncoding;
            TodoEngine engine = new TodoEngine();
            engine.Init(exts, tags, Encoding.GetEncoding(defaultEncoding), 0, 0);
            //add events
            engine.AppMessageHandler += new ApplicationMessageEventHandler(engine_AppMessageHandler);
            engine.AppProgressHandler += new ApplicationProgressEventHandler(engine_AppProgressHandler);
            //scan todos
            TodoItemCollection col = engine.getTodoItemsFromFolder(folder);
            //print result
            showStatusText("Поиск тодо-задач завершен");
            richTextBox1.AppendText(String.Format("Просмотр каталога {0} закончен {1}", folder, Environment.NewLine));
            richTextBox1.AppendText(Environment.NewLine);
            Application.DoEvents();
            //print todo-items
            printTodoItems(col);
            //установить новую коллекцию как текущую
            if (this.m_extractedItems != null)
                this.m_extractedItems.Clear();
            this.m_extractedItems = col;
            //разблокировать форму от действий пользователя после завершения операции
            this.UseWaitCursor = false;

            return;
        }

        /// <summary>
        /// NT-Print progress message to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void engine_AppProgressHandler(object sender, ApplicationProgressEventArgs e)
        {
            showProgressBar(e.CurrentValue, e.MaxValue, e.Message);
            Application.DoEvents();
            return;
        }
        /// <summary>
        /// NT-Print error message to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void engine_AppMessageHandler(object sender, ApplicationMessageEventArgs e)
        {
            //добавим сообщение об ошибке в текстовый контрол в качестве лога процесса.
            this.richTextBox1.AppendText("* " + e.Message + Environment.NewLine);
            showStatusText(e.Message);
            Application.DoEvents();
            return;
        }
        /// <summary>
        /// NT-Вывести найденные элементы в RichTextBox
        /// </summary>
        /// <param name="col"></param>
        private void printTodoItems(TodoItemCollection col)
        {
            string newline = Environment.NewLine;
            //сбросить кеш создания ссылок на файлы
            m_filepath_Cache = null;
            //работать далее
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("{0}{1} шт.{2}", m_FoundedTagsHeader, col.Count, newline);
            sb.AppendLine();
            foreach (TodoItem it in col.Items)
            {
                string link = makeFileLink(it.Source);
                string todo = it.Key;
                string text = it.Content;
                int position = it.StartPosition;
                //clear string builder
                sb.AppendFormat("* {0}:{1}{2}", todo, text, newline);
                sb.AppendFormat("      {0}  [поз. ", link);
                sb.AppendFormat("{0}]{1}", position.ToString(), newline);
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("Вывод тодо-задач завершен.");
            //add text to richtextbox
            this.richTextBox1.AppendText(sb.ToString());
            sb.Length = 0;//clear string builder

            return;
        }
        #endregion

        #region Link generation
        //TODO: перенести всю генерацию в класс-движок, а форму оставить только как пульт к движку.
        /// <summary>
        /// Кеш строки пути файла для оптимизации генерации ссылок
        /// </summary>
        private string m_filepath_Cache;
        /// <summary>
        /// Кеш строки ссылки для оптимизации генерации ссылок
        /// </summary>
        private string m_filepath_Cache_Result;
        /// <summary>
        /// NT-Создать ссылку на файл из пути файла
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string makeFileLink(string filepath)
        {
            //если ссылка уже есть в кеше, просто вернем результат из кеша.
            if (!String.Equals(filepath, m_filepath_Cache, StringComparison.Ordinal))
            {
                //добавляем ссылку в кеш, чтобы повторно использовать.
                this.m_filepath_Cache = filepath;
                this.m_filepath_Cache_Result = MyCodeLibrary.FileOperations.FileLink.LocalPathToUNCpath(filepath);

            }
            return m_filepath_Cache_Result;
        }
        /// <summary>
        /// NT- Переделать ссылку в специальную ссылку для вызова приложений вроде википада
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string makeSpecialLink(string link)
        {
            //собрать ссылку согласно расширению файла
            string ext = Path.GetExtension(link);
            String result = null;
            //специальная форма ссылки для файлов википада
            if (String.Equals(ext, ".wiki", StringComparison.OrdinalIgnoreCase))
            {
                //file path = C:/Users/Администратор/ЛокальныеПроекты/ПоискНадо/РаботатьЗдесь/NadoWiki/data/ЛогПроекта.wiki
                String s0 = Path.GetFileNameWithoutExtension(link);//s10 = ЛогПроекта
                String s1 = Path.GetDirectoryName(link);//s1 = C:/Users/Администратор/ЛокальныеПроекты/ПоискНадо/РаботатьЗдесь/NadoWiki/data
                String s2 = Path.GetDirectoryName(s1);//s2 = C:/Users/Администратор/ЛокальныеПроекты/ПоискНадо/РаботатьЗдесь/NadoWiki
                String s3 = Path.GetFileName(s1);//s3 = data 
                //Если папка = data, это файл википада
                if (String.Equals(s3, "data", StringComparison.OrdinalIgnoreCase) == true)
                {
                    String s4 = Path.GetFileName(s2);//s4 = NadoWiki
                    String s5 = Path.Combine(s2, s4 + ".wiki");
                    //String s6 = MyCodeLibrary.FileOperations.FileLink.makeFileUrlFromAbsoluteNTPath(s5);
                    String s7 = s5 + "?page=" + s0;
                    String s8 = s7.Substring(4);
                    result = "wiki" + s8;
                }
                //else
                //а если нет - формируем обычную ссылку.
            }
            //тут можно добавить еще обработчик для какой-либо специальной формы ссылки     
            //else if(String.Equals(ext, ".wiki", StringComparison.OrdinalIgnoreCase))
            
            //если новой ссылки нет, вернем старую
            if (result == null)
                return link;
            else return result;
        }

        #endregion

        #region Find Nado functions
        /// <summary>
        /// NR - 
        /// </summary>
        private void FindNado()
        {
            //заблокировать форму от действий пользователя на время выполнения операции
            this.UseWaitCursor = true;
            //очистить ричтекстбокс
            this.richTextBox1.Clear();
            //Запросить у пользователя папку для поиска
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Укажите каталог для поиска надо-задач:";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowNewFolderButton = false;
            //установить предыдущую выбранную папку.
            fbd.SelectedPath = Properties.Settings.Default.LastFolder;
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                //выходим по Отмена
                //разблокировать форму от действий пользователя после завершения операции
                this.UseWaitCursor = false;
                return;
            }
            String folder = fbd.SelectedPath;
            fbd.Dispose();
            fbd = null;
            //запомнить предыдущую выбранную папку.
            Properties.Settings.Default.LastFolder = folder;
            //изменить заголовок окна приложения
            this.setTitleBarText(folder);

            //вывести шапку для файла отчета, если пользователь надумает его сделать
            richTextBox1.AppendText(String.Format("Поиск Надо-задач в каталоге {0} {1}", folder, Environment.NewLine));
            richTextBox1.AppendText(String.Format("Создан {0} {1}", DateTime.Now.ToString(), Environment.NewLine));
            richTextBox1.AppendText(String.Format("Используйте текстовый редактор с распознаванием гиперссылок, не Блокнот.{0}", Environment.NewLine));
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(String.Format("Начат просмотр каталога {0}..{1}", folder, Environment.NewLine));
            Application.DoEvents();

            //do work with Надо
            string tags = Properties.Settings.Default.NadoTags;
            string exts = Properties.Settings.Default.ReadFileExts;
            int n1 = Properties.Settings.Default.NumOfCharsBeforeNado;
            int n2 = Properties.Settings.Default.NumOfCharsAfterNado;
            string defaultEncoding = Properties.Settings.Default.DefaultEncoding;
            TodoEngine engine = new TodoEngine();
            engine.Init(exts, tags, Encoding.GetEncoding(defaultEncoding), n1, n2);
            //add events
            engine.AppMessageHandler += new ApplicationMessageEventHandler(engine_AppMessageHandler);
            engine.AppProgressHandler += new ApplicationProgressEventHandler(engine_AppProgressHandler);
            //scan Nado's
            TodoItemCollection col = engine.getNadoItemsFromFolder(folder);
            //print result
            showStatusText("Поиск Надо-задач завершен");
            richTextBox1.AppendText(String.Format("Просмотр каталога {0} закончен {1}", folder, Environment.NewLine));
            richTextBox1.AppendText(Environment.NewLine);
            Application.DoEvents();
            //print todo-items
            printNadoItems(col);
            //установить новую коллекцию как текущую
            if(this.m_extractedItems != null)
                this.m_extractedItems.Clear();
            this.m_extractedItems = col;
            //разблокировать форму от действий пользователя после завершения операции
            this.UseWaitCursor = false;

            return;
        }
        /// <summary>
        /// NT-Вывести в RichText контрол содержимое коллекции как Надо-задачи
        /// </summary>
        /// <param name="col"></param>
        private void printNadoItems(TodoItemCollection col)
        {
            string newline = Environment.NewLine;
            //сбросить кеш создания ссылок на файлы
            m_filepath_Cache = null;
            //работать далее
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("{0}{1} шт.{2}", m_FoundedTagsHeader, col.Count, newline);
            sb.AppendLine();
            foreach (TodoItem it in col.Items)
            {
                string link = makeFileLink(it.Source);
                string text = it.Content.Trim();
                int position = it.StartPosition;
                //clear string builder
                sb.AppendLine();
                sb.AppendLine("= = = = = = = = = =");
                sb.AppendFormat("* {0}{1}", text, newline);
                sb.AppendLine("= = = = = = = = = =");
                sb.AppendFormat("      * {0}  [поз. ", link);
                sb.AppendFormat("{0}]{1}", position.ToString(), newline);

                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("Вывод Надо-задач завершен.");
            //add text to richtextbox
            this.richTextBox1.AppendText(sb.ToString());
            sb.Length = 0;//clear string builder

            return;
        }

        #endregion
        #region Save as functions
        /// <summary>
        /// NR-Сохранить содержимое ричтекстбокса в файле
        /// </summary>
        /// <param name="fileType">Тип файла: RTF, HTML, LittleTasks, Unicode, ANSI, All</param>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        private void saveFileWithType(int fileType, string filePath, RichTextBox rtb)
        {
            //set status text
            this.showStatusText(String.Format("Сохранение: {0}", filePath));
            //заблокировать форму от действий пользователя на время выполнения операции
            this.UseWaitCursor = true;
            //
            try
            {
                switch (fileType)
                {
                    case 1: //RTF
                        saveAsRtf(filePath, rtb);
                        break;
                    case 2: //HTML
                        saveAsHtml(filePath, rtb, this.m_extractedItems);
                        break;
                    case 3: //LittleTasks
                        saveAsLittleTasks(filePath, rtb, this.m_extractedItems);
                        break;
                    case 4: //Unicode
                        saveAsUnicode(filePath, rtb);
                        break;
                    default://ANSI, All
                        saveAsTxt(filePath, rtb, Encoding.Default);
                        break;
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("Ошибка: исключение {0}:\n{1}", ex.GetType().ToString(), ex.Message);
                MessageBox.Show(msg, m_ApplicationTitle + " - ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //set status text
            this.showStatusText(String.Format("Файл сохранен: {0}", filePath));
            //разблокировать форму от действий пользователя после завершения операции
            this.UseWaitCursor = false;

            return;
        }

        /// <summary>
        /// NT-Сохранить в файл
        /// </summary>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        private void saveAsRtf(string filePath, RichTextBox rtb)
        {
            //ссылки не работают в Ворде. Только в Вордпаде работают.
            rtb.SaveFile(filePath, RichTextBoxStreamType.RichNoOleObjs);

            return;
        }
        /// <summary>
        /// NR-Сохранить в файл
        /// </summary>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        private void saveAsUnicode(string filePath, RichTextBox rtb)
        {
            //TODO: файл не читается нигде
            rtb.SaveFile(filePath, RichTextBoxStreamType.UnicodePlainText);
        }
        /// <summary>
        /// NT-Сохранить в файл
        /// </summary>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        /// <param name="encoding">Кодировка для текстового файла.</param>
        private void saveAsTxt(string filePath, RichTextBox rtb, Encoding encoding)
        {
            //ссылки не работают нигде, кроме студии.
            rtb.SaveFile(filePath, RichTextBoxStreamType.PlainText);
        }
        /// <summary>
        /// NR-Сохранить в файл
        /// </summary>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        /// <param name="tic">Коллекция сохраняемых элементов.</param>
        private void saveAsLittleTasks(string filePath, RichTextBox rtb, TodoItemCollection tic)
        {
            //1 извлечь из контрола текст с информацией о процессе и ошибках, расположенный до вывода тодо-задач.
            string headText = splitHeadText(rtb.Text);
            if(String.IsNullOrEmpty(headText))
            {
                //заголовок не обнаружен, выводить тут ничего нельзя
                MessageBox.Show("Нельзя сохранить данные, поскольку заголовок отчета отсутствует или испорчен.", m_ApplicationTitle + " - сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //2 сформировать текст для вывода этого текста как первой задачи в списке задач. 
            StringBuilder sb = new StringBuilder();
            throw new NotImplementedException();

            //затем сформировать вывод остальных задач для списка задач.
        }
        /// <summary>
        /// NT-Сохранить в файл
        /// </summary>
        /// <param name="filePath">Путь создаваемого файла.</param>
        /// <param name="rtb">Rich Text Control с сохраняемым текстом.</param>
        /// <param name="tic">Коллекция сохраняемых элементов.</param>
        private void saveAsHtml(string filePath, RichTextBox rtb, TodoItemCollection tic)
        {
            //1 извлечь из контрола текст с информацией о процессе и ошибках, расположенный до вывода тодо-задач.
            string headText = MyCodeLibrary.HTMLoperations.HTMLprocessor.getSafeHtmlText2(splitHeadText(rtb.Text));
            if (String.IsNullOrEmpty(headText))
            {
                //заголовок не обнаружен, выводить тут ничего нельзя
                MessageBox.Show("Нельзя сохранить данные, поскольку заголовок отчета отсутствует или испорчен.", m_ApplicationTitle + " - сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //2 сформировать текст для вывода в html-формате UTF-8
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html lang=\"ru-RU\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<title>Отчет о найденных элементах</title>");
            sb.AppendLine("<pre>");
            sb.AppendLine(headText);
            sb.AppendLine("</pre>");
            sb.AppendFormat("<H2>{0}{1} шт.</H2>{2}", m_FoundedTagsHeader, tic.Count, Environment.NewLine);
            //print elements
            TodoEngineMode mode = tic.ContentType;
            foreach (TodoItem it in tic.Items)
            {
                if (mode == TodoEngineMode.NadoItems)
                {
                    string link = makeSpecialLink(makeFileLink(it.Source));
                    string text = MyCodeLibrary.HTMLoperations.HTMLprocessor.getSafeHtmlText2(it.Content.Trim());
                    int position = it.StartPosition;
                    //print as nado-item
                    sb.AppendLine("<p><pre>");
                    sb.AppendLine("= = = = = = = = = =");
                    sb.AppendFormat("* {0}{1}", text, Environment.NewLine);
                    sb.AppendLine("= = = = = = = = = =");
                    sb.AppendFormat("      * <a href =\"{0}\">{0}</a>  [поз. ", link);
                    sb.AppendFormat("{0}]{1}", position.ToString(), Environment.NewLine);
                    sb.AppendLine();
                    sb.AppendLine("</pre></p>");
                }
                else
                {
                    //print as todo-item
                    string link = makeFileLink(it.Source);
                    string todo = it.Key;
                    string text = MyCodeLibrary.HTMLoperations.HTMLprocessor.getSafeHtmlText2(it.Content);
                    int position = it.StartPosition;
                    //clear string builder
                    sb.AppendLine("<p><pre>");
                    sb.AppendFormat("* {0}:{1}{2}", todo, text, Environment.NewLine);
                    sb.AppendFormat("      * <a href =\"{0}\">{0}</a>  [поз. ", link);
                    sb.AppendFormat("{0}]{1}", position.ToString(), Environment.NewLine);
                    sb.AppendLine();
                    sb.AppendLine("</pre></p>");
                }
            }

            //footer document
            sb.AppendLine("<p>Вывод элементов завершен. </p>");
            sb.AppendLine("</body></html>");

            //save to file
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(sb.ToString());
            sw.Close();
            sb.Length = 0;
            sb = null;

            return;
        }
        /// <summary>
        /// NT - извлечь из контрола текст с информацией о процессе и ошибках, расположенный до вывода тодо-задач.
        /// </summary>
        /// <param name="p">Текст из контрола.</param>
        /// <returns>Функция возвращает текст заголовка при успехе, или null при неудаче.</returns>
        private string splitHeadText(string p)
        {
            int pos = p.IndexOf(m_FoundedTagsHeader);
            //если строки заголовка нет, значит, в ричтекстбоксе неизвестно что, сохранять его не будем. 
            if (pos < 0)
                return null;
            //get header text
            string header = p.Remove(pos);

            return header;
        }

        #endregion



    }
}
