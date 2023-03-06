using DatabaseChamp.Exceptions;
using DatabaseChamp.Test.Models;
using NUnit.Framework;

namespace DatabaseChamp.Test
{
    [TestFixture]
    public class DatabaseContextTest
    {
        private static readonly string _defaultTableName = "testfile";
        private static readonly string _defaultFolderPath = Path.Combine(Environment.CurrentDirectory, "databaseFiles");

        [TestCase()]
        [TestCase("C:\\Users\\robin\\databaseFiles")]
        [Test]
        public void DatabaseFolder_Exists_AfterInitialization(string folderPath = "")
        {
            new DatabaseContext(folderPath);
            var result = Directory.Exists(string.IsNullOrEmpty(folderPath) ? _defaultFolderPath : folderPath);

            Assert.IsTrue(result, "DatabaseFolder should have been created during initialization.");
        }

        [Test]
        public void DatabaseFile_Exists_AfterCreation()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            var result = File.Exists(Path.Combine(_defaultFolderPath, $"{_defaultTableName}.json"));

            Assert.IsTrue(result, "DatabaseFile should have been created.");
        }

        [Test]
        public void DatabaseFile_ContentUnchanged_AfterRecreation()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add<TestModel>(new TestModel { TestPropOne = "TestItemOne" });

            context.CreateTable<TestModel>(_defaultTableName);

            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemOne", savedItems.First().TestPropOne, "Property value has changed");
        }

        [Test]
        public void AddMethod_ShouldContainItem_AfterItWasAdded()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add<TestModel>(new TestModel { TestPropOne = "TestItemOne" });
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemOne", savedItems.First().TestPropOne, "Property value has changed");
        }

        [Test]
        public void AddMethod_ShouldContainList_AfterMultipleItemsAdded()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add<TestModel>(new TestModel { TestPropOne = "TestItemOne" });
            context.Add<TestModel>(new TestModel { TestPropOne = "TestItemTwo" });
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(2, savedItems.Count(), "Number of items does not match");
        }

            [Test]
        public void AddMethod_ShouldThrow_IfTableWasNotCreated()
        {
            var context = new DatabaseContext();
            var ex = Assert.Throws<TableNotFoundException>(delegate
            {
                context.Add<TestModel>(new TestModel());
            });

            Assert.That(ex!.Message, Contains.Substring(typeof(TestModel).ToString()));
        }

        [Test]
        public void AddMethod_ShouldThrow_IfDatatypesDoesNotMatch()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            var ex = Assert.Throws<WrongDatatypeException>(delegate
            {
                context.Add<TestModel>(new TestModelTwo());
            });

            Assert.That(ex!.Message, Contains.Substring("TestModel"));
            Assert.That(ex!.Message, Contains.Substring("TestModelTwo"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_defaultFolderPath))
            {
                Directory.Delete(_defaultFolderPath, true);
            }

            if (Directory.Exists("C:\\Users\\robin\\databaseFiles"))
            {
                Directory.Delete("C:\\Users\\robin\\databaseFiles", true);
            }
        }
    }
}