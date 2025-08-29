using System.IO;
using System.Text;

namespace UISystem.Editor
{
    /// <summary>
    ///Code generation auxiliary class
    /// </summary>
    public static class CodeGenHelper
    {
        /// <summary>
        ///Temporary StringBuilder
        /// </summary>
        private static readonly StringBuilder sTempStringBuilder = new StringBuilder();

        /// <summary>
        ///Get file header
        /// </summary>
        /// <param name="author"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] GetFileHeader(string author, string name, string date, bool isShow = true)
        {
            if (isShow)
            {
                return new string[]
                {
                    "---------------------------------------------------------------------------------------",
                    //" Copyright (c)  2025-2035",
                   $" Author: {author}",
                    $" Date: {date}",
                   $" Name: {name}",
                    " Desc: Auto Generated, DON'T EDIT IT!",
                    "---------------------------------------------------------------------------------------"
                };
            }
            else
            {
                return new string[]
                {
                    "---------------------------------------------------------------------------------------",
                    //" Copyright (c)  2025-2035",                   
                    $" Date: {date}",                     
                    "---------------------------------------------------------------------------------------"
                };

            }

        }

        /// <summary>
        ///Write a row for the specified tab quantity
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="tabCount"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteLineWithTab(this TextWriter sw, int tabCount, string format, params object[] arg)
        {
            sTempStringBuilder.Clear();
            for (int i = 0; i < tabCount; ++i)
            {
                sTempStringBuilder.Append("\t");
            }
            if (null == arg || arg.Length == 0)
            {
                sTempStringBuilder.Append(format);
            }
            else
            {
                sTempStringBuilder.AppendFormat(format, arg);
            }
            sw.WriteLine(sTempStringBuilder.ToString());
        }

        /// <summary>
        ///Write comments
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="tabCount"></param>
        /// <param name="annotation"></param>
        public static void WriteAnnotation(this TextWriter sw, int tabCount, string annotation)
        {
            WriteLineWithTab(sw, tabCount, "/// <summary>");
            WriteLineWithTab(sw, tabCount, $"/// {annotation}");
            WriteLineWithTab(sw, tabCount, "/// </summary>");
        }
    }
}