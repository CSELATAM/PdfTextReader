﻿using PdfTextReader.Base;
using PdfTextReader.Configuration;
using PdfTextReader.Execution;
using PdfTextReader.Parser;
using PdfTextReader.PDFCore;
using PdfTextReader.PDFText;
using PdfTextReader.TextStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PdfTextReader
{
    public class ExampleStages
    {
        public static Dictionary<string, string> RunParserPDF(IVirtualFS virtualFS, string basename, string inputfolder, string outputfolder)
        {
            VirtualFS.ConfigureFileSystem(virtualFS);

            PdfReaderException.ContinueOnException();

            using (var context = new ParserStages.StageContext(basename, inputfolder, outputfolder))
            {
                //var dbg0 = new ParserStages.StageDbgFlow(context);
                //dbg0.Process();

                var extract = new ParserStages.StageExtractHeaderDOU(context);
                extract.Process();

                var stage0 = new ParserStages.StagePdfInput(context);
                stage0.Process();

                var stage1 = new ParserStages.StagePageMargins(context);
                stage1.Process();

                var stage2 = new ParserStages.StageBlocksets(context);
                stage2.Process();

                var stage3 = new ParserStages.StageRetrieveBlocks(context);
                stage3.Process();

                var stageText1 = new ParserStages.StageConvertText(context);
                stageText1.Process();

                var stageText2 = new ParserStages.StageConvertStructure(context);
                stageText2.Process();
                //stageText2.ProcessWithConfiguration($"{outputfolder}/{basename}/{basename}-tree.config");

                var stageText3 = new ParserStages.StageConvertStructText(context);
                stageText3.Process();

                var stageTextTree = new ParserStages.StageConvertTree(context);
                stageTextTree.Process();

                var stageContent = new ParserStages.StageConvertContent(context);
                stageContent.Process();

                var stageArtigos = new ParserStages.StageConvertArtigoGN(context);
                stageArtigos.Process();

                string logStage3 = context.GetOutput("stage3");
                string logTree = context.GetOutput("tree");

                return context.FileListOutput;
            }
        }

        public static string ConvertGN(string pdf, string article, string content)
        {
            var gn = new Converter2GN();
            return gn.Convert(pdf, article, content);
        }

        public static string ExtractHeader(string basename)
        {
            var virtualFS = new VirtualFS();
            VirtualFS.ConfigureFileSystem(virtualFS);

            PdfReaderException.ContinueOnException();

            using (var context = new ParserStages.StageContext(basename, "input", "output"))
            {
                var extract = new ParserStages.StageExtractHeaderDOU(context);
                extract.Process();

                return context.FileListOutput.ToString();
            }
        }
    }
}
