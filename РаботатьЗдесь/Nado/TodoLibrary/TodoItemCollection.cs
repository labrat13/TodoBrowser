using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TodoLibrary
{
    /* Коллекция тодо-задач.
     * Здесь предполагается, что на каждый файл создается своя коллекция тодо-задач.
     * Хотя логично собрать все задачи в общую коллекцию и рассортировать их по классам (тегам).
     *  И можно сэкономить время на создании объекта регекса.
     *  Для этого надо переделать код функции Extract так, чтобы она вызывала не-статическую функцию, 
     *   которая бы возвращала массив тодо-итемов для указанного файла, а их уже добавлять в текущую коллекцию. 
    
     * И код все так же сыроват, как и в прототипе.
     */
    
    /// <summary>
    /// Коллекция тодо-задач.
    /// </summary>
    public class TodoItemCollection
    {
        /// <summary>
        /// Символы-ограничители конца строки
        /// </summary>
        private static char[] endline = { '\n', '\r', '!', '?', '.' };
        
        /// <summary>
        /// Список извлеченных тодо-задач.
        /// </summary>
        private List<TodoItem> m_items;
        /// <summary>
        /// Тип содержимого коллекции
        /// </summary>
        private TodoEngineMode m_ContentType;


        /// <summary>
        /// Default constructor
        /// </summary>
        public TodoItemCollection()
        {
            m_items = new List<TodoItem>();
            m_ContentType = TodoEngineMode.Default;
        }
        #region Properties
        /// <summary>
        /// Список извлеченных тодо-задач.
        /// </summary>
        public List<TodoItem> Items
        {
            get { return m_items; }
        }

        /// <summary>
        /// NT-Получить число элементов коллекции.
        /// </summary>
        public int Count
        { 
            get { return this.m_items.Count; }
        }

        /// <summary>
        /// Тип содержимого коллекции
        /// </summary>
        public TodoEngineMode ContentType
        {
            get { return this.m_ContentType; }
            set { this.m_ContentType = value; }
        }
        #endregion
        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}: {1} items in collection", this.m_ContentType,  this.m_items.Count);
        }

        /// <summary>
        /// NT-Добавить элемент в коллекцию
        /// </summary>
        /// <param name="ti"></param>
        public void Add(TodoItem ti)
        {
            this.m_items.Add(ti);

            return;
        }
        /// <summary>
        /// NT-Добавить содержимое коллекции в текущую коллекцию
        /// </summary>
        /// <param name="coll">Добавляемая коллекция</param>
        public void Add(TodoItemCollection coll)
        {
            //1. установить тип содержимого коллекции
            // если содержимое коллекции неопределенного типа, то установить тип из добавляемой коллекции
            if (this.ContentType == TodoEngineMode.Default)
                this.ContentType = coll.ContentType;
            // иначе, если тип содержимого двух коллекций не совпадает, установить режим Смешанный.
            //Это значит, что в коллекции присутствуют элементы разных типов.
            else if (this.ContentType != coll.ContentType)
                this.ContentType = TodoEngineMode.Mixed;
            
            //2. тут добавить сами элементы.
            this.m_items.AddRange(coll.m_items);

            return;
        }

        /// <summary>
        /// NT-Добавить набор итемов в коллекцию.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TodoItem> items)
        {
            this.m_items.AddRange(items);

            return;
        }

        /// <summary>
        /// NT- Очистить коллекцию.
        /// </summary>
        public void Clear()
        {
            this.m_items.Clear();

            return;
        }
        #region extract Todo items
        /// <summary>
        /// NT-Создать коллекцию тодо-задач для текста из файла.
        /// </summary>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <param name="text">Текст исходного файла.</param>
        /// <param name="tags">Список тегов тодо-задач. Пример: todo done question.</param>
        /// <returns>Возвращает объект коллекции тодо-задач для исходного файла</returns>
        public static TodoItemCollection ExtractTodos(string src, string text, string[] tags)
        {
            TodoItemCollection result = new TodoItemCollection();
            result.ContentType = TodoEngineMode.TodoItems;//установить флаг, что содержимое коллекции - тодо-задачи
            List<TodoItem> loti = result.ExtractTodoItems(src, text, tags);
            result.AddRange(loti);

            return result;
        }

        /// <summary>
        /// NT-Создать список тодо-задач для текста из файла.
        /// </summary>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <param name="text">Текст исходного файла.</param>
        /// <param name="tags">Список тегов тодо-задач. Пример: todo done question.</param>
        /// <returns>Возвращает список тодо-задач для исходного файла.</returns>
        public List<TodoItem> ExtractTodoItems(string src, string text, string[] tags)
        {
            List<TodoItem> result = new List<TodoItem>();
            //parsing
            Regex regex = this.makeTodoRegex(tags);
            MatchCollection mc = regex.Matches(text);
            foreach (Match m in mc)
            {
                if (m.Success == false)
                    continue;

                TodoItem ti = makeTodoItem(m, text, src);
                if (ti != null)
                    result.Add(ti);
            }
            
            return result;
        }

        /// <summary>
        /// NFT-Собрать тодо-итем из найденного регексом совпадения
        /// </summary>
        /// <param name="m">совпадение</param>
        /// <param name="text">исходный текст</param>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <returns>Функция возвращает тодо-итем или null при неправильном распознавании.</returns>
        private static TodoItem makeTodoItem(Match m, string text, string src)
        {
            //ti.m_length = findEndOfTodo(ti.m_beginPos, ti.m_keyLength, text);
            int startSearch = m.Index + m.Length;
            //тут по startSearch должен быть разделитель тодо - двоеточие. Если оно есть, то выкинем его из выходной строки.
            //А если нет - это неправильный тодо-тег! 
            if (text[startSearch] != ':')
                return null;//это неправильный случай, например TODO.\n
            //собираем тодо-итем
            TodoItem ti = new TodoItem();
            ti.Source = src;
            ti.Key = m.Value;
            ti.StartPosition = m.Index;
            ti.KeyLength = m.Length;
            int endSearch = text.IndexOfAny(endline, startSearch);
            //если не найдено ничего, то текстовый файл кончился на тодо без перевода строки. Так бывает.
            if (endSearch == -1)
                endSearch = Math.Min(text.Length - 1, startSearch + 256);
            //вычислим длину текста тодо-задачи с разделительным :
            int length = endSearch - startSearch;
            //запишем длину всей задачи
            ti.Length = length + m.Length;
            //ti.m_content = getTodoContent(ti.m_beginPos, ti.m_keyLength, ti.m_length, text);
            //избавимся от : но возьмем последний символ, который не был включен в вырезку - это признак конца строки из массива endline.
            String content = text.Substring(startSearch + 1, length);
            ti.Content = content;//не триммим строку, чтобы длина совпадала. 

            return ti;
        }
        /// <summary>
        /// NT - создать регекс для разбора текста по набору todo-тегов
        /// </summary>
        /// <param name="tags">Массив строк тегов тодо-задач</param>
        /// <returns></returns>
        private Regex makeTodoRegex(string[] tags)
        {
            String r = String.Join("|", tags);
            String rx = @"\b(?:" + r + @")(?:\.[^:\s]+)?";

            return new Regex(rx, RegexOptions.CultureInvariant);
        }

#endregion
        #region extract Nado items
        /// <summary>
        /// NT-Создать коллекцию Надо-задач для текста из файла.
        /// </summary>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <param name="text">Текст исходного файла.</param>
        /// <param name="tags">Список тегов Надо-задач. Пример: надо</param>
        /// <param name="charsBefore">Количество выводимых символов текста перед Надо-тегом.</param>
        /// <param name="charsAfter">Количество выводимых символов текста после Надо-тега.</param>
        /// <returns>Возвращает объект коллекции тодо-задач для исходного файла</returns>
        public static TodoItemCollection ExtractNados(string src, string text, string[] tags, int charsBefore, int charsAfter)
        {
            TodoItemCollection result = new TodoItemCollection();
            result.ContentType = TodoEngineMode.NadoItems;//установить флаг что содержимое коллекции - тодо-задачи
            List<TodoItem> loti = result.ExtractNadoItems(src, text, tags, charsBefore, charsAfter);
            result.AddRange(loti);

            return result;
        }
        /// <summary>
        /// NT-Создать регекс для разбора текста по набору Надо-тегов 
        /// </summary>
        /// <param name="tags">Массив строк тегов Надо-задач</param>
        /// <returns></returns>
        private Regex makeNadoRegex(string[] tags)
        {
            String r = String.Join("|", tags);
            String rx = @"(\b?)(" + r + @")(\b)";//(\b?)(надо|тоженадо)(\b)

            return new Regex(rx, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
        }
        /// <summary>
        /// NT-Создать список Надо-задач для текста из файла.
        /// </summary>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <param name="text">Текст исходного файла.</param>
        /// <param name="tags">Список тегов Надо-задач. Пример: надо</param>
        /// <param name="charsBefore">Количество выводимых символов текста перед Надо-тегом.</param>
        /// <param name="charsAfter">Количество выводимых символов текста после Надо-тега.</param>
        /// <returns>Возвращает список надо-задач для исходного файла.</returns>
        private List<TodoItem> ExtractNadoItems(string src, string text, string[] tags, int charsBefore, int charsAfter)
        {
            List<TodoItem> result = new List<TodoItem>();
            //parsing
            Regex regex = makeNadoRegex(tags);
            MatchCollection mc = regex.Matches(text);
            foreach (Match m in mc)
            {
                if (m.Success == false)
                    continue;

                TodoItem ti = makeNadoItem(m, text, src, charsBefore, charsAfter);
                if (ti != null)
                    result.Add(ti);             
            }

            return result;
        }
        /// <summary>
        /// NT-Собрать тодо-итем из найденного регексом совпадения
        /// </summary>
        /// <param name="m">совпадение</param>
        /// <param name="text">исходный текст</param>
        /// <param name="src">Путь к исходному файлу для указания в качестве источника.</param>
        /// <param name="charsBefore">Количество выводимых символов текста перед Надо-тегом.</param>
        /// <param name="charsAfter">Количество выводимых символов текста после Надо-тега.</param>
        /// <returns>Функция возвращает тодо-итем или null при неправильном распознавании.</returns>
        private TodoItem makeNadoItem(Match m, string text, string src, int charsBefore, int charsAfter)
        {          
            //1. вычисляем начало куска текста
            int startText = m.Index - charsBefore;
            //проверить, что не вышли за начало текста
            if (startText < 0) startText = 0;
            //2. вычисляем конец куска текста
            int endText = m.Index + m.Length + charsAfter;
            //проверяем, что не вышли за конец текста
            int textLength = text.Length;
            if (endText >= textLength) endText = textLength - 1;
            //3. получаем текст куска
            int len = endText - startText;
            string kusok = text.Substring(startText, len);
            //Не Trim-мим текст куска, чтобы его длина соответствовала координатам.
            //Триммить будем в приложении при выводе на экран.

            //собираем тодо-итем
            TodoItem ti = new TodoItem();
            ti.Source = src;
            ti.Key = "Nado";
            ti.KeyLength = 4;
            ti.Content = kusok;
            ti.Length = len;
            ti.StartPosition = startText;

            return ti;
        }


        #endregion
    }
}
