using System;
using System.Collections.Generic;
using System.Text;

namespace TodoLibrary
{
    /// <summary>
    /// Режим работы движка, содержимое коллекции элементов
    /// </summary>
    public enum TodoEngineMode
    {
        /// <summary>
        /// Неопределенный режим движка, ошибка, (=0)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Режим работы с тодо-задачами
        /// </summary>
        TodoItems,
        /// <summary>
        /// Режим работы с Надо-задачами
        /// </summary>
        NadoItems,
        /// <summary>
        /// Работа с разными типами элементов
        /// </summary>
        Mixed,

    }
}
