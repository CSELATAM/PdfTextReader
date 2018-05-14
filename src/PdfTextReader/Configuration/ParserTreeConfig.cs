﻿using PdfTextReader.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfTextReader.Configuration
{
    class ParserTreeConfig : IExecutionConfiguration
    {
        List<string> _titles;

        public void Init(string content)
        {
            _titles = content
                        .Split(new char[] { '\r', '\n' })
                        .Select(RemoveComment)
                        .Where(StringNotEmpty)
                        .ToList();
        }

        bool StringNotEmpty(string line) => !String.IsNullOrWhiteSpace(line);

        string RemoveComment(string line)
        {
            if( line.Contains("((") && line.Contains("))"))
            {
                string[] components = line.Split(new String[] { "(("  }, 2, StringSplitOptions.None);

                string text = components[0].Trim();

                return text;
            }

            return line.Trim();
        }
    }
}
