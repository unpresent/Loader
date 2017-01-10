using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Loader
{
    internal static class ParamsManager
    {
        // -----------------------------------------------------------
        #region Константы
        // -----------------------------------------------------------
        private const string CParamPath = "PATH";
        private const string CParamDest = "DEST";
        private const string CParamApp = "APP";
        private const string CParamsDelimeter = " ";

        private static readonly char[] CParamPrefixes = { '-', '/' };

        // Параметры, которые предназначены для нас. Их обрабатываем.
        // "Наши параметры"
        private static readonly string[] COurParams = { CParamPath, CParamApp, CParamDest };

        // Обазательные "наши параметры"
        private static readonly string[] CMandatoryParams = { CParamPath, CParamApp, CParamDest };
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Поля
        // -----------------------------------------------------------
        private static bool mParsed;
        private static bool mValidated;
        private static List<string> mOtherParams = new List<string>();
        private static readonly SortedList<string, string> mParams = new SortedList<string, string>();
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Свойства
        // -----------------------------------------------------------
        
        /// <summary>
        /// Путь к папке, которую надо скопировать
        /// </summary>
        public static string Path
        {
            get
            {
                CheckValidated();
                string vResult = mParams[CParamPath];
                if (vResult.PadRight(1) != "\\")
                {
                    vResult = vResult + "\\";
                }
                return vResult;
            }
        }

        /// <summary>
        /// Приложение, которое надо будет запустить после копирования
        /// </summary>
        public static string App
        {
            get
            {
                CheckValidated();
                return mParams[CParamApp];
            }
        }

        /// <summary>
        /// Папка, в которую надо скопировать
        /// </summary>
        public static string Dest
        {
            get
            {
                CheckValidated();
                string vResult = mParams[CParamDest];
                if (vResult.PadRight(1) != "\\")
                {
                    vResult = vResult + "\\";
                }
                return vResult;
            }
        }

        /// <summary>
        /// Формируем строку запуска приложения
        /// </summary>
        public static string OtherParamsString
        {
            get
            {
                CheckValidated();

                var vResult = string.Empty;
                foreach (var vItem in mOtherParams)
                {
                    vResult += CParamsDelimeter + vItem;
                }
                return vResult;
            }
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Реализация
        // -----------------------------------------------------------
        /// <summary>
        /// Разбор параметров
        /// </summary>
        /// <param name="aArgs">Параметры приложения</param>
        public static void Parse(string[] aArgs)
        {
            string vParamName = null;
            var vIsParamValue = false;

            foreach (var vArg in aArgs)
            {
                if (CParamPrefixes.Contains(vArg[0]))
                {
                    vParamName = vArg.Substring(1);
                    if (COurParams.Contains(vParamName))
                    {
                        vIsParamValue = true;
                        mParams.Add(vParamName, null);
                    }
                    else
                    {
                        mOtherParams.Add(vArg);
                    }
                }
                else if (vIsParamValue)
                {
                    mParams[vParamName] = vArg;
                    vParamName = null;
                    vIsParamValue = false;
                }
                else
                {
                    mOtherParams.Add(vArg);
                }
            }

            // Ставим флаг того, что параметры были разобраны
            mParsed = true;
        }

        /// <summary>
        /// Проверка параметров на наличие обязательных параметров
        /// </summary>
        public static bool Validate()
        {
            CheckParsed();

            // Если отсутсвует хотя бы один обязательный параметр
            if (CMandatoryParams.Any(aItem => !mParams.ContainsKey(aItem)))
            {
                MessageBox.Show("Неверные параметры запуска:\n-PATH - путь к ресурсам,\n-APP - исполняемый EXE-файл\n\nНапример: Loader.exe -PATH D:/dir -APP Application.exe\r\nЗапрещено указывать в качестве ресурса корень диска", "Loader");
                return false;
            }

            mValidated = true;
            return true;
        }

        private static void CheckParsed()
        {
            if (!mParsed)
            {
                throw new Exception("Не произведен преварительный разбор параметров!");
            }
        }

        private static void CheckValidated()
        {
            if (!mValidated)
            {
                throw new Exception("Не произведена преварительная пероверка параметров!");
            }
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------

    }
}
