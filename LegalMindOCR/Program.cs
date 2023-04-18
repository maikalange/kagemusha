using com.sun.tools.javac.util;
using IronOcr;
using IronPdf;
using LegalMindPersistence.db;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;

namespace LegalMindOCR
{
    class Program
    {
        private static string GetFileName(string filePath)
        {
            var file = filePath.Split(new string[] { @"\" }, StringSplitOptions.None);
            var x = file.Length - 1;
            return file[x].Replace("pdf", "txt");
        }

        private static void ProcessPrincipleAct()
        {
            IList<BsonDocument> docs = new List<BsonDocument>();
            string[] fileEntries = Directory.GetFiles(Environment.GetEnvironmentVariable(""), "*.html");
            foreach (var f in fileEntries)
            {
                MainActGenerator actGenerator = new MainActGenerator(f);
                //InsertPrincipalActDb(f, sg.ExtractMainAct(sg.ActOfParliament));
                //Console.WriteLine(sg.ExtractMainAct(sg.ActOfParliament));
                //get section 2 body interpretation

                var actContent = actGenerator.PrimaryAct;
                var listOfSections = actGenerator.ProcessSections();
                var definitionSection = listOfSections.Find(
               x => x.Id.Equals("2.") && Enum.Equals(Section.LocationOfSection.BODY, x.SecLocation)
               );
                var d = MainActGenerator.ProcessInterpretation(definitionSection?.Content);
                var act = new Act()
                {
                   Pages = new Page[] { new Page() { PageNo = 0,
                   Content = actGenerator.PrimaryAct } },
                    Sections = listOfSections, Definitions = d,
                };

                var json = JsonConvert.SerializeObject(act);
                // Console.WriteLine(json);
                var document = BsonSerializer.Deserialize<BsonDocument>(json);
                docs.Add(document);          
            }
            DbFacade.SaveMany(docs);
        }

        static void Main()
        {
            ProcessPrincipleAct();            
            Console.ReadKey();
        }

        private static void ExtractAllText(string pdfSourcePath)
        {
            try
            {
                string[] fileEntries = Directory.GetFiles(pdfSourcePath);
                foreach (var fileName in fileEntries)
                {
                    PdfDocument PDF = PdfDocument.FromFile(fileName);
                    
                    string AllText = PDF.ExtractAllText();
                    //Console.WriteLine(AllText);
                    using (var sw = File.CreateText(fileName + ".txt"))
                    {
                        sw.Write(AllText);
                    }
                    //InsertAct(fileName, AllText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" Error processing file" + e.Message);
            }
        }

        private static void ProcessFiles(string pdfSourcePath)
        {
            string[] fileEntries = Directory.GetFiles(pdfSourcePath);
            IronOcr.Installation.LicenseKey = "vvvxxxxzzzz";
            var Ocr = new IronTesseract();
            foreach (var file in fileEntries)
            {
                using (var input = new OcrInput())
                {
                    try
                    {
                        input.AddPdf(file);
                        var Result = Ocr.Read(input);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"There was an error processing {file}");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.InnerException);
                    }
                }
            }
        }
    }
}
