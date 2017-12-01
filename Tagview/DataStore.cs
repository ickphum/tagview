using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using System.IO;
using Android.Util;
using Android.OS.Storage;

namespace Tagview
{
    [Table(name: "Category")]
    class CategoryRec
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


    static class DataStore
    {
        static List<CategoryRec> categories;
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

            LoadDataModel();
        }

        private static void InitialiseDatabase()
        {
            db.CreateTable<CategoryRec>();
            db.Insert(new CategoryRec("General"));
            db.Insert(new CategoryRec("Holiday"));
            db.Insert(new CategoryRec("Family"));
        }

        private static void LoadDataModel()
        {
            //
            var table = db.Table<CategoryRec>();
            categories = new List<CategoryRec>();
            foreach (var c in table) {
                categories.Add(c);
            }

        }

        public static List<CategoryRec> LoadCategories()
        {
            var table = db.Table<CategoryRec>();
            return table.ToList<CategoryRec>();
        }

        // send the category list to the adapter
        public static string[] GetCategories()
        {
            Log.Info("DataStore", "GetCategories");

            string[] names = new string[categories.Count];
            int i = 0;
            foreach (var c in categories) {
                names[i++] = c.Name;
            }
            return names;
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
            LoadDataModel();
        }

        public static void AddCategory(CategoryRec newCategory)
        {
            db.Insert(newCategory);
            categories.Add(newCategory);
        }

    }
}