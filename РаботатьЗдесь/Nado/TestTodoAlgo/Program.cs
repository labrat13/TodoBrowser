using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TodoLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            //source folder
            string srcFolder = "D:\\";
            //задаем список тегов - его обычно следует хранить в настройках приложения.
            string tags = "todo TODO done DONE question QUESTION надо НАДО тодо ТОДО вопрос ВОПРОС";
            //задаем расширения файлов
            string exts = ".txt xml cs cpp";
            TodoEngine engine = new TodoEngine();
            engine.Init(exts, tags, Encoding.GetEncoding(1251), 0, 0);
            //add events
            engine.AppMessageHandler += new ApplicationMessageEventHandler(engine_AppMessageHandler);
            engine.AppProgressHandler += new ApplicationProgressEventHandler(engine_AppProgressHandler);

            TodoItemCollection col = engine.getTodoItemsFromFolder(srcFolder);

            Console.WriteLine("Найденные задачи:");
            foreach (TodoItem it in col.Items)
                Console.WriteLine(it.ToString());

            return;
        }
        /// <summary>
        /// Print progress message to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void engine_AppProgressHandler(object sender, ApplicationProgressEventArgs e)
        {
            Console.WriteLine("Файл {0} из {1}: {2}", e.CurrentValue, e.MaxValue, e.Message);
        }
        /// <summary>
        /// Print error message to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void engine_AppMessageHandler(object sender, ApplicationMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }



    }
}
