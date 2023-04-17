using HtmlAgilityPack;
using LegalMindPersistence.db;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LegalMindPersistence
{
    public class Program
    {
        private static IList<BsonDocument> documents = new List<BsonDocument>();
        private static readonly string BASE_DIR = Environment.GetEnvironmentVariable("docPath") ;
        private static void ProcessAct()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(BASE_DIR);
            foreach (var dir in subdirectoryEntries)
            {
                string[] fileEntries = Directory.GetFiles(dir);
                foreach (var file in fileEntries)
                {
                    if (file.Contains("_BODY_C.html"))
                    {
                        var path = file;
                        var pathToTitleFile = path.Replace("_BODY_C.html", "_BODY_T.html");
                        BuildAndSaveSectionHtml(path, pathToTitleFile);
                    }
                    BuildAndSaveActHtml(file);

                }
            }
            DbFacade.SaveMany(documents);
        }

        private static void BuildAndSaveActHtml(string file)
        {
            if (file.Contains("_TIDY.html"))
            {
                var  fp  = file.Split(new char[] { '\\'});
                var actName  = fp[fp.Length - 1].ToString().Replace("_TIDY.html",string.Empty);                

                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode.SelectSingleNode("//body");
                var content = node.InnerHtml;
                var act  = new JsonAct(actName, content);
                SerializeContent(act);
            }
        }

        private static void SerializeContent<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            documents.Add(document);        
        }

        static void Main(string[] args)
        {

            ProcessAct();
            Console.ReadKey();
        }

        private static void BuildAndSaveSectionHtml(string path, string pathToTitleFile)
        {
            var pathTokens = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
            var actName = pathTokens[pathTokens.Length - 2];
            var fileNameToken = pathTokens[pathTokens.Length - 1].Split(new char[] { '_' });
            var actParts = new { sectionNo = fileNameToken[0], location = fileNameToken[1], contentType = fileNameToken[2] };

            var titleDoc = new HtmlDocument();
            titleDoc.Load(pathToTitleFile);
            var titleNode = titleDoc.DocumentNode.SelectSingleNode("//body");
            var sectionTitle = titleNode.GetDirectInnerText();

            var bodyDoc = new HtmlDocument();
            bodyDoc.Load(path);
            var div = HtmlNode.CreateNode("<div></div>");
            var a = HtmlNode.CreateNode("<a></a>");
            a.SetAttributeValue("href", $"#{actParts.sectionNo}");
            div.SetAttributeValue("class", "section");
            var node = bodyDoc.DocumentNode.SelectSingleNode("//body");
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType.Equals(HtmlNodeType.Element))
                {
                    child.SetAttributeValue("class", "section-p");
                }
            }

            node.PrependChild(a);
            if (!String.IsNullOrEmpty(sectionTitle)&&sectionTitle.Trim().Length>0)
            {
                div.InnerHtml = node.InnerHtml.Trim().Replace(sectionTitle.Trim(), "");
            }


            //SaveSection(actName, div.OuterHtml, sectionTitle, actParts.sectionNo);
            var s = new JsonSection(actParts.sectionNo, actName, sectionTitle, div.OuterHtml);
            SerializeContent(s);
        }            
    }
}
