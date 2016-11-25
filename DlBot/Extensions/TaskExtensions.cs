using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DlBot.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Used to drop the task. Removes compiler warning about not awaiting task.
        /// Use this for a fire and forget async call.
        /// </summary>
        /// <param name="task"></param>
        public static void Forget(this Task task)
        {
        }
    }

}
