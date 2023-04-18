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
            string[] fileEntries = Directory.GetFiles(@"C:\Users\JoeNyirenda\Documents\Laws Zambia\", "*.html");
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
            // try
            {
                ProcessPrincipleAct();
                //ExtractAllText(@"C:\Sandbox\legalminds\pdfs\");
                //ExtractAllText(@"C:\Users\jnj\Documents\NetBeansProjects\LegalMindScraper\Judgements\B\");
                //ExtractAllText(@"C:\Users\jnj\Documents\NetBeansProjects\LegalMindScraper\Judgements\A\");

                //Parallel.Invoke(
                //   () => ExtractAllText(@"C:\Sandbox\legalminds\pdfs\")
                //    () => ExtractAllText(@"C:\Users\jnj\Documents\NetBeansProjects\LegalMindScraper\Judgements\B\"),
                //    () => ExtractAllText(@"C:\Users\jnj\Documents\NetBeansProjects\LegalMindScraper\Judgements\A\")
                //   );
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            Console.ReadKey();
        }


        private static void InsertPrincipalActDb(string fileName, string content)
        {

            string queryString = $"INSERT MainActs (FileName, Details) VALUES (@colParam1, @colParam2)";

            SqlCommand insertCommand = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = queryString
            };

            insertCommand.Parameters.AddWithValue("@colParam1", fileName);
            insertCommand.Parameters.AddWithValue("@colParam2", content);
            using (SqlConnection sqlConnection1 =
    new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=LegalMind;Data Source=.\sqlexpress"))
            {

                insertCommand.Connection = sqlConnection1;

                sqlConnection1.Open();
                insertCommand.ExecuteNonQuery();
                Console.WriteLine($"Saved {fileName}");
            }
        }


        private static void InsertJudgement(string fileName, string content)
        {

            string queryString = $"INSERT Judgements (FileName, Details) VALUES (@colParam1, @colParam2)";

            SqlCommand insertCommand = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = queryString
            };

            //        insertCommand.Parameters.AddWithValue("@colParam1", fileName);
            //        insertCommand.Parameters.AddWithValue("@colParam2", content);
            //        using (SqlConnection sqlConnection1 =
            //new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=LegalMind;Data Source=.\sqlexpress"))
            //        {

            //            insertCommand.Connection = sqlConnection1;

            //            sqlConnection1.Open();
            //            insertCommand.ExecuteNonQuery();
            Console.WriteLine($"Saved {content}");
            // }
        }


        private static void InsertAct(string fileName, string content)
        {

            string queryString = $"INSERT Act (ActName, HtmlContent) VALUES (@colParam1, @colParam2)";

            SqlCommand insertCommand = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = queryString
            };

            insertCommand.Parameters.AddWithValue("@colParam1", fileName);
            insertCommand.Parameters.AddWithValue("@colParam2", content);
            using (SqlConnection sqlConnection1 =
    new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=LegalMind;Data Source=.\sqlexpress"))
            {

                insertCommand.Connection = sqlConnection1;

                sqlConnection1.Open();
                insertCommand.ExecuteNonQuery();
                Console.WriteLine($"Saved {fileName}");
            }
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
            IronOcr.Installation.LicenseKey = "IRONOCR.JOSEPHNYIRENDA.11818-8A92956B58-A3YQV6OYCEAUP-BGODVHZDDFXL-Q5BQW4SBS5KN-7LZKNLVC3CZ2-D3FGY3AEI4PK-MJOQXG-TQ3RJ2UDWFKEEA-DEPLOYMENT.TRIAL-XZZJTS.TRIAL.EXPIRES.12.FEB.2022";
            var Ocr = new IronTesseract();
            foreach (var file in fileEntries)
            {
                using (var input = new OcrInput())
                {
                    try
                    {
                        input.AddPdf(file);
                        var Result = Ocr.Read(input);
                        InsertJudgement(file, Result.Text);
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
