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

        public SequenceRec()
        {
            Name = "";
        }
        public SequenceRec(string NewName)
        {
            Name = NewName;
        }
    }

    static class DataStore
    {
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
            int general = db.Insert(new CategoryRec("General"));
            db.Insert(new TagRec(general, "Funny"));
            db.Insert(new TagRec(general, "Pretty"));
            db.Insert(new TagRec(general, "Cold"));
            db.Insert(new CategoryRec("Holiday"));
            db.Insert(new CategoryRec("Family"));
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

        public static List<CategoryRec> LoadCategories()
        {
            var table = db.Table<CategoryRec>();
            return table.ToList<CategoryRec>();
        }

        public static int AddCategory(CategoryRec newCategory)
        {
            return db.Insert(newCategory);
        }

        public static int UpdateCategory(CategoryRec category)
        {
            return db.Update(category);
        }

        public static int DeleteCategory(CategoryRec category)
        {
            db.Execute("delete from Tag where category_id = ?", category.Id);
            return db.Delete(category);
        }

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

    }
}