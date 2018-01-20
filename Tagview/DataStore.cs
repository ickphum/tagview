using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.IO;
using Android.Util;
using Android.OS.Storage;

namespace Tagview
{
    [Table(name: "Category")]
    public class CategoryRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [MaxLength(15), Unique]
        public string name { get; set; }
        public bool active { get; set; }
        public CategoryRec()
        {
            name = "";
        }
        public CategoryRec(string newName)
        {
            name = newName;
        }
    }

    [Table(name: "Tag")]
    public class TagRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [MaxLength(15), Unique]
        public string name { get; set; }
        [ForeignKey(typeof(CategoryRec))]
        public int categoryId { get; set; }

        public TagRec()
        {
            name = "";
        }
        public TagRec(int category, string newName)
        {
            categoryId = category;
            name = newName;
        }
    }

    [Table(name: "Sequence")]
    public class SequenceRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [MaxLength(15), Unique]
        public string name { get; set; }
        public int sortCode { get; set; }
        public int slideShowPeriodSecs { get; set; }

        public SequenceRec() { }

        public SequenceRec(string newName)
        {
            name = newName;
        }
    }

    [Table(name: "SequenceDir")]
    public class SequenceDirRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [ForeignKey(typeof(SequenceRec))]
        public int sequenceId { get; set; }
        [MaxLength(200), Unique]
        public string directory { get; set; }
        public bool includeChildren { get; set; }

        public SequenceDirRec() { }

        public SequenceDirRec(int sequence, string newDir, bool newIncludeChildren)
        {
            sequenceId = sequence;
            directory = newDir;
            includeChildren = newIncludeChildren;
        }
    }

    [Table(name: "Image")]
    public class ImageRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [MaxLength(100)]
        public string name { get; set; }
        [MaxLength(200)]
        public string directory { get; set; }

        public ImageRec() { }
        public ImageRec(string newName, string newDir)
        {
            name = newName;
            directory = newDir;
        }
    }

    static class DataStore
    {
        private static string TAG = "DataStore";
        static string dbPath;
        static SQLiteConnection db;

        static DataStore()
        {
            OpenDatabase();
        }

        private static void OpenDatabase()
        {
            dbPath = Path.Combine(
                //System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "/storage/emulated/0/Android/data/Tagview.Tagview",
                "tagview.db3");

            // remember this before opening the connection creates the file automatically
            bool database_exists = File.Exists(dbPath);

            db = new SQLiteConnection(dbPath);

            if (! database_exists) {
                Log.Info("DataStore", "No database exists at " + dbPath);
                InitialiseDatabase();
            }
        }

        private static void InitialiseDatabase()
        {
            db.CreateTable<CategoryRec>();
            db.CreateTable<TagRec>();
            db.CreateTable<SequenceRec>();
            db.CreateTable<SequenceDirRec>();
            db.CreateTable<ImageRec>();

            int general = AddCategory(new CategoryRec("General"));
            AddTag(new TagRec(general, "Funny"));
            AddTag(new TagRec(general, "Pretty"));
            AddTag(new TagRec(general, "Cold"));

            AddCategory(new CategoryRec("Holiday"));
            AddCategory(new CategoryRec("Family"));

            int sequence = db.Insert(new SequenceRec("Test"));
            db.Insert(new SequenceDirRec(sequence, "/a/b/c", true));
            db.Insert(new SequenceDirRec(sequence, "/a/b/c1", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/d", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/e", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/f", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/g", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/h", false));
            db.Insert(new SequenceDirRec(sequence, "/a/b/i", false));

            db.Insert(new ImageRec("match 1 of 3", "/a/b/c"));
            db.Insert(new ImageRec("match 2 of 3", "/a/b/c/d"));
            db.Insert(new ImageRec("match 3 of 3", "/a/b/c1"));
            db.Insert(new ImageRec("nomatch 1 of 2", "/a/b/c2"));
            db.Insert(new ImageRec("nomatch 2 of 2", "/a/b"));

            /*
            List<ImageRec> table = db.Query<ImageRec>("select i.* from Image i, Sequence s, SequenceDir sd"
                + " where s.name = ? and s.id = sd.sequenceId"
                + " and ((i.directory = sd.directory) or (sd.includeChildren = 1 and i.directory LIKE sd.directory || '/%'))", "test");

            Log.Info(TAG, "count = " + table.Count);

            foreach (var image in table) {
                Log.Info(TAG, image.name);
            }
            */

        }

        public static void ClearDatabase()
        {
            Log.Info("DataStore", "ClearDatabase: ");

            // easier and more definite to delete the database file and recreate it
            // rather than dropping all the objects
            db.Close();
            File.Delete(dbPath);
            db = new SQLiteConnection(dbPath);

            InitialiseDatabase();
        }

        /* Category methods */

        public static List<CategoryRec> LoadCategories()
        {
            var table = db.Table<CategoryRec>();
            return table.ToList<CategoryRec>();
        }

        public static int AddCategory(CategoryRec newCategory)
        {
            newCategory.active = true;
            return db.Insert(newCategory);
        }

        public static int UpdateCategory(CategoryRec category)
        {
            return db.Update(category);
        }

        public static int DeleteCategory(CategoryRec category)
        {
            // delete children first
            db.Execute("delete from Tag where categoryId = ?", category.id);
            return db.Delete(category);
        }

        public static void SetCategoryActive(int id, bool active)
        {
            Log.Info(TAG, "Id = " + id + ", Active = " + active);
            db.Execute("update Category set active = ? where id = ?", active, id);
        }

        /* Tag methods */

        public static List<TagRec> LoadTags(int categoryId)
        {
            var table = db.Query<TagRec>("select * from Tag where categoryId = ?", categoryId);
            return table;
        }
        
        public static int AddTag(TagRec newTag)
        {
            return db.Insert(newTag); 
        }

        public static int UpdateTag(TagRec tag)
        {
            return db.Update(tag);
        }

        public static int DeleteTag(TagRec tag)
        {
            return db.Delete(tag);
        }

        /* Sequence methods */

        public static List<SequenceRec> LoadSequences()
        {
            var table = db.Table<SequenceRec>();
            return table.ToList<SequenceRec>();
        }

        public static int AddSequence(SequenceRec newSequence)
        {
            return db.Insert(newSequence);
        }

        public static int UpdateSequence(SequenceRec sequence)
        {
            return db.Update(sequence);
        }

        public static int DeleteSequence(SequenceRec sequence)
        {
            // delete children first
            db.Execute("delete from SequenceDir where sequenceId = ?", sequence.id);
            return db.Delete(sequence);
        }

        /* Sequence Dir methods */

        public static List<SequenceDirRec> LoadSequenceDirs(int sequenceId)
        {
            var table = db.Query<SequenceDirRec>("select * from SequenceDir where sequenceId = ?", sequenceId);
            return table;
        }

        public static int AddSequenceDir(SequenceDirRec newSequenceDir)
        {
            return db.Insert(newSequenceDir);
        }

        public static int UpdateSequenceDir(SequenceDirRec tag)
        {
            return db.Update(tag);
        }

        public static int DeleteSequenceDir(SequenceDirRec tag)
        {
            return db.Delete(tag);
        }

    }
}