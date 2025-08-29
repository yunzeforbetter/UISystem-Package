using System.Collections.Generic;
using System.IO;

namespace UISystem.Editor
{
    /// <summary>
    ///Code Generation - Class Method
    /// </summary>
    public class CCClassMethod
    {
        /// <summary>
        ///Attribute declaration
        /// </summary>
        public string Declaration { get; set; }

        /// <summary>
        ///Description (Note)
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///Method body code
        /// </summary>
        private List<string> mBodyLines = new List<string>();

        /// <summary>
        ///Add a line of code
        /// </summary>
        /// <param name="s"></param>
        public void AppendBodyLine(string s)
        {
            mBodyLines.Add(s);
        }

        /// <summary>
        ///Write Stream
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="tabCount"></param>
        public void Write(TextWriter tw, int tabCount)
        {
            //notes
            if (!string.IsNullOrEmpty(Desc))
            {
                tw.WriteAnnotation(tabCount, Desc);
            }

            //head
            tw.WriteLineWithTab(tabCount, Declaration);
            tw.WriteLineWithTab(tabCount, "{");

            //Method body
            var bodyTabCount = tabCount + 1;
            for (int i = 0, ci = mBodyLines.Count; i < ci; ++i)
            {
                tw.WriteLineWithTab(bodyTabCount, mBodyLines[i]);
            }

            //tail
            tw.WriteLineWithTab(tabCount, "}");
        }
    }
}