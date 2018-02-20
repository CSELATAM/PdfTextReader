using System;
using System.IO;
using System.Diagnostics;

namespace PdfTextReader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("PDF Text Reader");
            var watch = Stopwatch.StartNew();

            Program3.CreateLayout("D14/D142-p41");
            //Program3.ProcessStats2("D14/D142",41);

            //ExamplesPipeline.ProcessPipeline("bin/DO1_2017_11_14");

            //            IVirtualFS localFiles = new Base.VirtualFS();
            //ExamplesAzure.RunParserPDF(localFiles, "DO1_2010_01_04", "bin", "bin/test");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Elapsed time was: {elapsedMs}");

            Console.ReadKey();
        }
    }
}
