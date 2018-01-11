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
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(15), Unique]
        public string Name { get; set; }
        public bool Active { get; set; }
        public CategoryRec()
        {
            Name = "";
        }
        public CategoryRec(string NewName)
        {
            Name = NewName;
        }
    }

    [Table(name: "Tag")]
    public class TagRec
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(15), Unique]
        public string Name { get; set; }
        [ForeignKey(typeof(CategoryRec))]
        public int category_id { get; set; }

        public TagRec()
        {
            Name = "";
        }
        public TagRec(int Category, string NewName)
        {
            category_id = Category;
            Name = NewName;
        }
    }

    [Table(name: "Sequence")]
    public class SequenceRec
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(15), Unique]
        public string Name { get; set; }
        public int SortCode { get; set; }
        public int SlideShowPeriodSecs { get; set; }

        public SequenceRec() { }

        public SequenceRec(string NewName)
        {
            Name = NewName;
        }
    }

    [Table(name: "SequenceDir")]
    public class SequenceDirRec
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [ForeignKey(typeof(SequenceRec))]
        public int SequenceId { get; set; }
        [MaxLength(200), Unique]
        public string Directory { get; set; }
        public bool IncludeChildren { get; set; }

        public SequenceDirRec(int Sequence, string NewDir, bool NewIncludeChildren)
        {
            SequenceId = Sequence;
            Directory = NewDir;
            IncludeChildren = NewIncludeChildren;
        }
    }

    [Table(name: "Image")]
    public class ImageRec
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Directory { get; set; }

        public ImageRec() { }
        public ImageRec(string NewName, string NewDir)
        {
            Name = NewName;
            Directory = NewDir;
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

            db.Insert(new ImageRec("match 1 of 3", "/a/b/c"));
            db.Insert(new ImageRec("match 2 of 3", "/a/b/c/d"));
            db.Insert(new ImageRec("match 3 of 3", "/a/b/c1"));
            db.Insert(new ImageRec("nomatch 1 of 2", "/a/b/c2"));
            db.Insert(new ImageRec("nomatch 2 of 2", "/a/b"));

            /*
            List<ImageRec> table = db.Query<ImageRec>("select i.* from Image i, Sequence s, SequenceDir sd"
                + " where s.Name = ? and s._id = sd.SequenceId"
                + " and ((i.Directory = sd.Directory) or (sd.IncludeChildren = 1 and i.Directory LIKE sd.Directory || '/%'))", "test");

            Log.Info(TAG, "count = " + table.Count);

            foreach (var image in table) {
                Log.Info(TAG, image.Name);
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
            newCategory.Active = true;
            return db.Insert(newCategory);
        }

        public static int UpdateCategory(CategoryRec category)
        {
            return db.Update(category);
        }

        public static int DeleteCategory(CategoryRec category)
        {
            // delete children first
            db.Execute("delete from Tag where category_id = ?", category.Id);
            return db.Delete(category);
        }

        public static void SetCategoryActive(int Id, bool Active)
        {
            Log.Info(TAG, "Id = " + Id + ", Active = " + Active);
            db.Execute("update Category set Active = ? where _id = ?", Active, Id);
        }

        /* Tag methods */

        public static List<TagRec> LoadTags(int category_id)
        {
            var table = db.Query<TagRec>("select * from Tag where category_id = ?", category_id);
            return table;
            //return table.ToList<TagRec>();
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
            db.Execute("delete from SequenceDir where SequenceId = ?", sequence.Id);
            return db.Delete(sequence);
        }

    }
}