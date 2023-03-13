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

        #region base

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
        public void PersistenceTest()
        {
            // Create a database + files
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(new TestModel { TestPropOne = "TestItemOne" });

            // Destroy context and trigger garbage collection
            context = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Create a new context which should import the existing collections
            var newContext = new DatabaseContext();
            var savedItems = newContext.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemOne", savedItems.First().TestPropOne, "Property value has changed");
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
            context.Add(new TestModel { TestPropOne = "TestItemOne" });

            context.CreateTable<TestModel>(_defaultTableName);

            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemOne", savedItems.First().TestPropOne, "Property value has changed");
        }

        #endregion
        #region add

        [Test]
        public void AddMethod_ShouldContainItem_AfterItWasAdded()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(new TestModel { TestPropOne = "TestItemOne" });
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemOne", savedItems.First().TestPropOne, "Property value has changed");
        }

        [Test]
        public void AddMethod_ShouldContainList_AfterMultipleItemsAdded()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(new TestModel { TestPropOne = "TestItemOne" });
            context.Add(new TestModel { TestPropOne = "TestItemTwo" });
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(2, savedItems.Count(), "Number of items does not match");
        }

        [Test]
        public void AddMethod_ShouldThrow_IfTableWasNotCreated()
        {
            var context = new DatabaseContext();
            var ex = Assert.Throws<TableNotFoundException>(delegate
            {
                context.Add(new TestModel());
            });

            Assert.That(ex!.Message, Contains.Substring(typeof(TestModel).ToString()));
        }

        [Test]
        public void AddMethod_ShouldThrow_IfItemIsAlreadyInCollection()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            var entity = new TestModel { TestPropOne = "TestItemOne" };
            context.Add(entity);

            Assert.Throws<DublicateException>(delegate
            {
                context.Add(entity);
            });
        }

        [Test]
        public void AddMethod_ShouldThrow_IfItemIsNull()
        {
            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);

            var ex = Assert.Throws<ArgumentNullException>(delegate
            {
                context.Add<TestModel>(null!);
            });

            Assert.That(ex!.ParamName, Is.EqualTo("objectToAdd"));
        }

        #endregion
        #region remove

        [Test]
        public void RemoveMethod_ItemShouldBeGone_AfterRemoval()
        {
            var entity = new TestModel { TestPropOne = "TestItemOne" };

            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(entity);
            context.Remove(entity);
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(0, savedItems.Count(), "Number of items does not match");
        }

        [Test]
        public void RemoveMethod_OtherItemsShouldStillExists_AfterRemoval()
        {
            var entityOne = new TestModel { TestPropOne = "TestItemOne" };
            var entityTwo = new TestModel { TestPropOne = "TestItemTwo" };

            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(entityOne);
            context.Add(entityTwo);
            context.Remove(entityOne);
            var savedItems = context.GetAll<TestModel>();

            Assert.AreEqual(1, savedItems.Count(), "Number of items does not match");
            Assert.AreEqual("TestItemTwo", savedItems.First().TestPropOne, "Wrong item was removed");
        }

        [Test]
        public void RemoveMethod_ShouldThrow_IfItemNotExistsInCollection()
        {
            var entityOne = new TestModel { TestPropOne = "TestItemOne" };
            var entityTwo = new TestModel { TestPropOne = "TestItemTwo" };

            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(entityOne);

            Assert.Throws<ItemNotFoundException>(delegate
            {
                context.Remove(entityTwo);
            });
        }

        [Test]
        public void RemoveMethod_ShouldThrow_IfItemIsNull()
        {
            var entityOne = new TestModel { TestPropOne = "TestItemOne" };

            var context = new DatabaseContext();
            context.CreateTable<TestModel>(_defaultTableName);
            context.Add(entityOne);

            var ex = Assert.Throws<ArgumentNullException>(delegate
            {
                context.Remove<TestModel>(null!);
            });

            Assert.That(ex!.ParamName, Is.EqualTo("objectToRemove"));
        }

        #endregion 

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