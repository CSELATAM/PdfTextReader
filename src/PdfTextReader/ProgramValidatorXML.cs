﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PdfTextReader
{
    class ProgramValidatorXML
    {
        bool bodyConditions = false;
        bool titleConditions = false;
        bool hierarchyConditions = false;

        public void ValidateArticle(string inputfolder)
        {
            var directory = new DirectoryInfo(inputfolder);
            foreach (var file in directory.EnumerateFiles("*.xml"))
            {
                Validate(file);
            }
        }

        void Validate(FileInfo file)
        {
            XDocument doc = XDocument.Load(file.FullName);
            //Elementos Metadado e Conteudo
            foreach (XElement el in doc.Root.Elements())
            {
                foreach (XAttribute item in el.Attributes())
                {
                    if (item.Name == "Hierarquia")
                        CheckHierarchy(item.Value);
                }

                foreach (XElement item in el.Elements())
                {
                    if (item.Name == "Titulo")
                        CheckTitle(item.Value);

                    if (item.Name == "Corpo")
                        CheckBody(item.Value);
                }
            }



            if (bodyConditions || titleConditions || hierarchyConditions)
                file.CopyTo($"bin/{file.Name.Replace(".xml", "")}-ISSUE.xml");
        }

        void CheckHierarchy(string text)
        {
            if (text != null)
            {
                if (text.Replace("o", "O").ToUpper() == text.Replace("o", "O"))
                    hierarchyConditions = true;
            }
        }

        void CheckTitle(string text)
        {
            if (text != null)
            {
                if (text.Replace("o", "O").ToUpper() == text.Replace("o", "O"))
                    titleConditions = true;
            }
        }

        void CheckBody(string text)
        {
            if (text != null)
                bodyConditions = true;
        }
    }
}
