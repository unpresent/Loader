using System;
using System.Windows.Forms;

namespace Loader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] aArgs)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Разбираем параметры
            ParamsManager.Parse(aArgs);

            // Проверяем параметры
            if (!ParamsManager.Validate())
            {
                return;
            }

            ProgressForm vForm = new ProgressForm();

            // Готовим испольнителя к работе
            Executor vExecutor = new Executor(ParamsManager.Path, ParamsManager.Dest, ParamsManager.App, ParamsManager.OtherParamsString, vForm);

            // Поднимаем форму и выполняем копирование файлов
            // Application.Run() внутри
            vExecutor.Run();
        }
    }
}
