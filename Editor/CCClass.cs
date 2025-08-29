using System.Collections.Generic;
using System.IO;

namespace UISystem.Editor
{
    /// <summary>
    ///Code Generation - Class
    /// </summary>
    public class CCClass
    {
        /// <summary>
        ///Class name
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///Class Generic Parameters 
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        ///Attribute declaration ({0} is the class name)
        /// </summary>
        public string DeclarationFormat { get; set; }

        /// <summary>
        ///Description (Note)
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///Attribute List
        /// </summary>
        private readonly List<CCClassProperty> mProperties = new List<CCClassProperty>();

        /// <summary>
        ///Method List
        /// </summary>
        private readonly List<CCClassMethod> mMethods = new List<CCClassMethod>();

        /// <summary>
        ///Add Attribute
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(CCClassProperty property)
        {
            mProperties.Add(property);
        }

        /// <summary>
        ///Add Method
        /// </summary>
        /// <param name="method"></param>
        public void AddMethod(CCClassMethod method)
        {
            mMethods.Add(method);
        }

        /// <summary>
        ///Write Stream
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="tabCount"></param>
        public void Write(TextWriter tw, int tabCount)
        {
            //notes
            var annotation = string.IsNullOrEmpty(Desc) ? ClassName : Desc;
            tw.WriteAnnotation(tabCount, annotation);

            //head
            tw.WriteLineWithTab(tabCount, DeclarationFormat, ClassName,Parameters);
            tw.WriteLineWithTab(tabCount, "{");

            //attribute
            var memberTabCount = tabCount + 1;
            for (int i = 0, ci = mProperties.Count; i < ci; ++i)
            {
                mProperties[i].Write(tw, memberTabCount);
            }

            //method
            for (int i = 0, ci = mMethods.Count; i < ci; ++i)
            {
                tw.WriteLine();
                mMethods[i].Write(tw, memberTabCount);
            }

            //tail
            tw.WriteLineWithTab(tabCount, "}");
        }
    }
}