using PragmaticSegmenterNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextHelper;

namespace LegalMindOCR
{
    public class MainActGenerator
    {
        private readonly string filePath;
        private readonly string SECTION_PATTERN = @"\s*\d{1,3}[A-z]{0,1}\.\s*";
        private static readonly int MAX_WRAP_WIDTH = 30;
        private static readonly int MIN_WRAP_WIDTH = 12;
        public MainActGenerator(string filePath)
        {
            this.filePath = filePath;
        }

        public string[] GetSections(string[] sections)
        {
            var k = sections.Length;
            var count = 1;
            for (int i = 0; i < k; i++)
            {
                var currentSection = Regex.Replace(sections[i], @"[A-Za-z]", "");
                var previousSection = i > 0 ? Regex.Replace(sections[i - 1], @"[A-Za-z]", "") : "0";
                var validSequence = (int.Parse(currentSection) == int.Parse(previousSection) + 1) || (previousSection == currentSection);
                if (!validSequence)
                {
                    //out of sequence                        
                    for (int j = i; j <= k - 1; j++)
                    {
                        sections[j] = string.Empty;
                    }
                    break;
                }
                if (!sections[i].Equals(count.ToString()))
                {
                    if (Regex.Match(sections[i], SECTION_PATTERN, RegexOptions.Multiline).Success)
                    {
                        while (true)
                        {
                            //Look ahead
                            if (i + 1 < k)
                            {
                                if (sections[i + 1].Contains(count.ToString()))
                                {
                                    count++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        i = count - 1;
                    }
                    else
                    {
                        //out of sequence                        
                        for (int j = i; j < k - 1; j++)
                        {
                            sections[j] = string.Empty;
                        }
                        break;
                    }
                }
                else
                {
                    count++;
                }
            }
            //Get Array length
            return sections;
        }
        private static List<Section> SplitSectionsAndIncludeSectionId(string input)
        {
            var pattern = @"\s*\d{1,3}[A-z]{0,1}\.\s*";
            var listOfSections = new List<Section>();
            int pos = 0;
            var previousSectionMatch = string.Empty;
            foreach (Match m in Regex.Matches(input, pattern, RegexOptions.Multiline))
            {
                if (pos == 0)
                {
                    //values.Add("0", input.Substring(pos, m.Index - pos));
 
                    listOfSections.Add(new Section("0", input.Substring(pos, m.Index - 3 - pos), Section.LocationOfSection.TOC) { SectionTitle="MAIN_HEADING"});
                    previousSectionMatch = m.Value;
                }
                else
                {
                    var formattedId = previousSectionMatch.Replace("\n", "").Replace("\r", "").Trim();
                    var loc = Section.LocationOfSection.TOC;
                    listOfSections.ForEach(x =>
                    {
                        if (x.Id.Contains(formattedId))
                        {
                            loc = Section.LocationOfSection.BODY;
                        }
                    });
                   
                    listOfSections.Add(new Section(formattedId, input.Substring(pos, m.Index - pos), loc));
                    previousSectionMatch = m.Value;

                    if (m.NextMatch().Length == 0)
                    {
                        var i = input.Length;
                        var j = m.Index;//last match
                                        //Last match not included
                        listOfSections.Add(new Section(previousSectionMatch.Replace("\n", "").Replace("\r", "").Trim(), input.Substring(m.Index + m.Length, i - (m.Index + m.Length)), loc));
                    }
                }


                pos = m.Index + m.Length;
            }
            return listOfSections;
        }

        public string PrimaryAct
        {
            get
            {
                return ReplaceHeadersFooters(ExtractMainAct(GetActSourceData));
            }
        }
        public List<Section> ProcessSections()
        {
            var mainAct = ReplaceHeadersFooters(ExtractMainAct(GetActSourceData));
            var sectionsInAct = SplitSectionsAndIncludeSectionId(mainAct);
            var updatedSections = sectionsInAct;
            var sectionContent = string.Empty;
            var updatedSectionContent = string.Empty;
            sectionsInAct.ForEach(s =>
            {
                while (true)
                {
                    var sectionBody = sectionsInAct.Find(x => (x.Id.Equals(s.Id) && Enum.Equals(x.SecLocation, Section.LocationOfSection.BODY)));
                    if ((sectionBody != null) && Enum.Equals(s.SecLocation, Section.LocationOfSection.TOC))
                    {
                        var title = s.Content;
                        s.SectionTitle = string.Empty;
                        updatedSectionContent = RemoveMarginReferencesFromSection(s.Content, sectionBody.Content);
                        updatedSections.ForEach(j =>
                        {
                            if (j.Id.Equals(s.Id) && Enum.Equals(j.SecLocation, Section.LocationOfSection.BODY))
                            {
                                j.SectionTitle = title;
                                j.Content = updatedSectionContent;
                            }
                        });
                    }
                    break;
                }
            });
            return updatedSections;
        }

        public string RemoveMarginReferencesFromSection(string sectionTitle, string sectionContent)
        {
            var processedSection = sectionContent.Replace("[","").Replace("]","");
            var regexSafeContent = sectionContent.Replace("[", "").Replace("]", "");
            int width = MAX_WRAP_WIDTH;
            var s = sectionTitle;
            var wrapped = s.WordWrap(width);
            while (width >= MIN_WRAP_WIDTH)
            {
                try
                {
                    wrapped = Regex.Escape(s.WordWrap(width));
                    var matched = Regex.Match(regexSafeContent, wrapped).Success;
                    if (matched)
                    {
                        processedSection = Regex.Replace(regexSafeContent, wrapped, "");
                        break;
                    }
                    width--;
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return processedSection;
        }

        public static List<Definition> ProcessInterpretation(string content)
        {
            var listOfDefinitions = new List<Definition>();
            if (!String.IsNullOrEmpty(content))
            {
                var definitions = content.Split(';');
                foreach (var definition in definitions)
                {
                    if (definition.Length > 0)
                    {
                        var a = definition.Contains('\u201C') ? definition.IndexOf('\u201C') : definition.IndexOf((char)34);
                        var z = definition.Contains('\u201D') ? definition.IndexOf('\u201D') : definition.LastIndexOf((char)34);
                        if (z > -1 && a > -1)
                        {
                            if (a!=z&&z>a)
                            {
                                var term = definition.Substring(a + 1, z - a - 1);
                                var meaning = definition.Substring(z + 1);
                                listOfDefinitions.Add(new Definition(term, meaning));
                            }
                            else
                            {
                                listOfDefinitions.Add(new Definition("ERROR_RESOLVING_INTERPRETATION", "ERROR_RESOLVING_INTERPRETATION"));
                            }
                        }
                    }
                }
            }
            return listOfDefinitions;
        }

        private string ExtractMainAct(string sourceContent)
        {
            Queue<string> schedules = new Queue<string>();   
            //Valid section => 2A. 2.
            //Rules - always split on SUBSIDIARY LEGISLATION FIRST, THEN SPLIT BY SCHEDULE
            var content = ReplaceHeadersFooters(sourceContent);
            var mainAct = string.Empty;
            if (!ContainsSubsidiaryLegislation(content))
            {
                //Check if contains schedules
                if (ContainsSchedule(content))
                {
                    //Split
                    var s = content.Split(new string[] { "SCHEDULE" }, StringSplitOptions.RemoveEmptyEntries);
                    //First array contains section information
                    //Check which array contains the main act
                    mainAct = s[0];// first element contains section information
                    if (s.Length > 1)
                    {                        
                        if (((s.Length - 1) % 2)==1)
                        {
                            Console.WriteLine($"{ sourceContent.Substring(0, 85)}");
                        }
                        for (int i = 1; i < s.Length; i++)
                        {
                            if (s[i].Contains("Short title") || s[i].Contains("Interpretation"))
                            {
                                //This is the main act
                                
                                mainAct += s[i];
                                break;
                            }
                        }
                    }

                }
                else
                {
                    //process as normal. Nothing to interfere with section generation
                    mainAct = content;
                }
            }
            else
            {
                //Split
                var s = Regex.Split(content, "SUBSIDIARY LEGISLATION");
                //We get the primary act and supporting legislation
                return ExtractMainAct(s[0]);
            }
            return mainAct;
        }
        private static String ReplaceHeadersFooters(string content)
        {
            StringBuilder sb = new StringBuilder(content);
            return sb.Replace("The Laws of Zambia", string.Empty).Replace("Copyright Ministry of Legal Affairs, Government of the Republic of Zambia", string.Empty).ToString();
        }
       
        private string GetActSourceData
        {
            get { return File.ReadAllText(filePath).Replace("\r\n"," "); }
        }

        private bool ContainsSchedule(string content)
        {
            return content.Contains("SCHEDULE");
        }

        private bool ContainsSubsidiaryLegislation(string actOfParliament)
        {
            return actOfParliament.Contains("SUBSIDIARY LEGISLATION");
        }
    }
}
