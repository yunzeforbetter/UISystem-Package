using System.IO;

namespace UISystem.Editor
{
    /// <summary>
    ///Code Generation - Class Properties
    /// </summary>
    public class CCClassProperty
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
        ///Write Stream
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="tabCount"></param>
        virtual public void Write(TextWriter tw, int tabCount)
        {
            //notes
            if (!string.IsNullOrEmpty(Desc))
            {
                tw.WriteAnnotation(tabCount, Desc);
            }

            //statement
            tw.WriteLineWithTab(tabCount, Declaration);
        }
    }
}