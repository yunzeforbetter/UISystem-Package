
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace UISystem.Editor
{
    /// <summary>
    ///Code Generation - Class
    /// </summary>
    public class CCFile
    {
        /// <summary>
        ///Namespaces used
        /// </summary>
        private readonly List<string> mUsingNameSpaces = new List<string>();

        /// <summary>
        ///Namespaces
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        ///Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///Path (without file name)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///Author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///Class
        /// </summary>
        private readonly List<CCClass> mClasses = new List<CCClass>();

        /// <summary>
        ///IsHaveHeadNotes
        /// </summary>
        public bool IsHaveHeadNotes = true;

        /// <summary>
        ///Write Stream
        /// </summary>
        /// <param name="tw"></param>
        public void Write(TextWriter tw)
        {
            //File Header
            var header = CodeGenHelper.GetFileHeader(Author, Name,DateTime.Today.ToShortDateString(),IsHaveHeadNotes);
            for (int i = 0; i < header.Length; ++i)
            {
                tw.WriteLine($"//{header[i]}");
            }

            //Referenced namespace
            for (int i = 0, ci = mUsingNameSpaces.Count; i < ci;  ++i)
            {
                tw.WriteLine($"using {mUsingNameSpaces[i]};");
            }
            tw.WriteLine();

            //Namespaces
            tw.WriteLine($"namespace {NameSpace}");
            tw.WriteLine("{");

            //class
            for (int i = 0, ci = mClasses.Count; i < ci; ++i)
            {
                mClasses[i].Write(tw, 1);
            }

            //end
            tw.WriteLine("}");
        }

        /// <summary>
        ///Write File
        /// </summary>
        public void WriteToFile()
        {
            //Create directory
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            //Write file
            var path = $"{Path}/{Name}.cs";
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                Write(sw);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///Add reference namespace
        /// </summary>
        /// <param name="s"></param>
        public void AddUsingNameSpace(string s)
        {
            mUsingNameSpaces.Add(s);
        }

        /// <summary>
        ///Add Class
        /// </summary>
        /// <param name="cls"></param>
        public void AddClass(CCClass cls)
        {
            mClasses.Add(cls);
        }
    }
}