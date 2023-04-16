using LegalMindOCR;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RemoveInvalidSections()
        {
            var data = new string[] { "1", "2", "3A", "3B", "5", "6A","6B" ,"4","5" };
            var expectedSections = new string[] { "1", "2", "3A", "3B", "5", "6A", "6B","","" };
            //var actualSections = MainActGenerator.GetSections(data);

            //Assert.AreEqual(expectedSections, actualSections);
        }

        

        [Test]
        public void RemoveMarginSectionReferences()
        {
            var sectionTitle = "Prohibition against sale of poisonous, unwholesome or adultered food\n\r";
            var marginReference = "Prohibition against\nsale of poisonous,\nunwholesome or\nadultered food";
            var unprocessedAct = sectionTitle + "\nThis is my act " + marginReference;
            //var processedAct = MainActGenerator.RemoveMarginReferences(sectionTitle, unprocessedAct);
            //var expectedAct = sectionTitle + "\nThis is my act ";

            //Assert.True(expectedAct.Equals(processedAct));
        }
    }
}