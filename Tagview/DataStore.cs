using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using System.IO;
using Android.Util;

namespace Tagview
{
    [Table(name: "Category")]
    class CategoryRec
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(15)]
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
        static List<CategoryRec> categories = new List<CategoryRec>();
        static string dbPath;
        static SQLiteConnection db;

        static DataStore()
        {
            dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "tagview.db3");
            if (File.Exists(dbPath))
            {
                Log.Info("DataStore", "Database exists at " + dbPath);
            }
            db = new SQLiteConnection(dbPath);
            // db.CreateTable<Category>();
            var table = db.Table<CategoryRec>();
            foreach (var c in table)
            {
                categories.Add(c);
            }

        }

        // send the category list to the adapter
        public static string[] GetCategories()
        {
            Log.Info("DataStore", "GetCategories");

            string[] names = new string[categories.Count];
            int i = 0;
            foreach (var c in categories)
            {
                names[i++] = c.Name;
            }
            return names;
        }

        public static void ClearDatabase()
        {
            Log.Info("DataStore", "ClearDatabase: ");
            db.DropTable<CategoryRec>();
            db.CreateTable<CategoryRec>();

            db.Insert(new CategoryRec("General"));
            db.Insert(new CategoryRec("Holiday"));
            db.Insert(new CategoryRec("Family"));

            categories = new List<CategoryRec>();
            var table = db.Table<CategoryRec>();
            foreach (var c in table)
            {
                categories.Add(c);
            }
        }

    }
}