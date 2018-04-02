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
        public bool single { get; set; }
        public CategoryRec()
        {
            name = "";
        }
        public CategoryRec(string name, bool single)
        {
            this.name = name;
            this.single = single;
            active = true;
        }
        public CategoryRec(string name) : this(name, false) { }
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

    enum SortType { Name, Date, Size, Orientation };

    [Table(name: "Sequence")]
    public class SequenceRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [MaxLength(15), Unique]
        public string name { get; set; }
        public int sortCode { get; set; }
        public int reverseSort { get; set; }
        public float slideShowPeriodSecs { get; set; }

        public SequenceRec() { }

        public SequenceRec(string newName)
        {
            name = newName;
            sortCode = (int)SortType.Name;
            reverseSort = 0;
            slideShowPeriodSecs = 10;
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

    [Table(name: "ImageTag")]
    public class ImageTagRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [ForeignKey(typeof(ImageRec))]
        public int imageId { get; set; }
        [ForeignKey(typeof(TagRec))]
        public int tagId { get; set; }

        public ImageTagRec() { }
        public ImageTagRec(int imageId, int tagId)
        {
            this.imageId = imageId;
            this.tagId = tagId;
        }
    }

    [Table(name: "SequenceImage")]
    public class SequenceImageRec
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [ForeignKey(typeof(ImageRec))]
        public int imageId { get; set; }

        public SequenceImageRec() { }
        public SequenceImageRec(int imageId)
        {
            this.imageId = imageId;
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
            db.CreateTable<ImageTagRec>();
            db.CreateTable<SequenceImageRec>();

            int category_id = AddCategory(new CategoryRec("General"));
            AddTag(new TagRec(category_id, "Funny"));
            AddTag(new TagRec(category_id, "Sad"));
            AddTag(new TagRec(category_id, "Cold"));
            AddTag(new TagRec(category_id, "Hot"));
            AddTag(new TagRec(category_id, "Scenic"));
            AddTag(new TagRec(category_id, "Beach"));
            AddTag(new TagRec(category_id, "Trees"));

            category_id = AddCategory(new CategoryRec("Holiday"));
            AddTag(new TagRec(category_id, "Hong Kong"));
            AddTag(new TagRec(category_id, "Tokyo"));

            category_id = AddCategory(new CategoryRec("Family"));
            AddTag(new TagRec(category_id, "Ian"));
            AddTag(new TagRec(category_id, "Charmaine"));
            AddTag(new TagRec(category_id, "Hamish"));

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
            db.Insert(newCategory);
            return newCategory.id;
        }

        public static int UpdateCategory(CategoryRec category)
        {
            category.single = false;

            string[] tokens = category.name.Split(new Char [] { ';', ' '});
            List<String> realTokens = new List<String>();
            foreach (string token in tokens) {
                if (token.Trim() != "")
                    realTokens.Add(token);
            }
            if (realTokens.Count > 1) {
                category.name = realTokens[0];
                realTokens.RemoveAt(0);
                foreach (string token in realTokens) {
                    if (token.Equals("single"))
                        category.single = true;
                }
            }
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
            db.Insert(newTag);
            return newTag.id;
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
            db.Insert(newSequence);
            return newSequence.id;
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

        internal static SequenceRec FindSequence(int sequenceId)
        {
            return db.Find<SequenceRec>(sequenceId);                
        }

        /* Sequence Dir methods */

        public static List<SequenceDirRec> LoadSequenceDirs(int sequenceId)
        {
            var table = db.Query<SequenceDirRec>("select * from SequenceDir where sequenceId = ?", sequenceId);
            return table;
        }

        public static int AddSequenceDir(SequenceDirRec newSequenceDir)
        {
            db.Insert(newSequenceDir);
            return newSequenceDir.id;
        }

        public static int UpdateSequenceDir(SequenceDirRec sequenceDir)
        {
            return db.Update(sequenceDir);
        }

        public static int DeleteSequenceDir(SequenceDirRec sequenceDir)
        {
            return db.Delete(sequenceDir);
        }

        // image and directory load methods; we load and process (ie create image records)
        // for a directory when we open it or when we activate a sequence using that 
        // directory.

        public static int AddImage(ImageRec newImage)
        {
            db.Insert(newImage);
            return newImage.id;
        }

        public static int AddImage(String filename, String directory)
        {
            return AddImage(new ImageRec(filename, directory));
        }

        public static int DeleteImage(ImageRec image)
        {
            return db.Delete(image);
        }

        public static int DeleteImage(String filename, String directory)
        {
            return db.Execute("delete from Image where name = ? and directory = ?", filename, directory);
        }

        public static (List<String>, List<int>) OpenDirectory(String directory, Boolean recursive)
        {
            List<String> dirFiles = new List<String>(System.IO.Directory.GetFiles(directory));
            List<int> imageIds = new List<int>();
            List<ImageRec> dbFiles = LoadDirectoryImages(directory);

            if (dbFiles.Count == 0) {

                // no need to check, just add all these files
                Log.Info(TAG, String.Format("No files for dir {0} in db", directory));
                foreach (var pathname in dirFiles) {
                    var filename = pathname.Substring(directory.Length + 1);
                    // Log.Info(TAG, String.Format("Add file {0} in new dir to db", filename));
                    imageIds.Add(AddImage(filename, directory));
                }
            }
            else {

                // create a hash by name and remove everything we find; the remainder are 
                // the new files in the dir
                // HashSet<String> dbFile = dbFiles.ConvertAll(x => x.name).ToHashSet<String>();

                // create a lookup hash for database image id; these are used for updating
                // image tags. We know these files are in the current directory so we don't need
                // to qualify the name by dir
                Dictionary<String, int> imageId = dbFiles.ToDictionary(x => x.name, x => x.id);

                foreach (var pathname in dirFiles) {
                    var filename = pathname.Substring(directory.Length + 1);
                    int id;
                    if (imageId.TryGetValue(filename, out id)) {

                        // this file is already in the db;
                        // Log.Info(TAG, String.Format("{0} already in db", filename));
                        imageId.Remove(filename);
                        imageIds.Add(id);                        
                    }
                    else {

                        // this file needs to be added to the db
                        Log.Info(TAG, String.Format("new file {0}, add to db", filename));
                        imageIds.Add(AddImage(filename, directory));
                        //imageIds.Add(-1);
                    }
                }

                foreach (var filename in imageId.Keys) {

                    // these files have been deleted since they didn't appear in the 
                    // directory list and thus get removed from the dictionary
                    Log.Info(TAG, String.Format("deleted file {0}, remove from db", filename));
                    DeleteImage(filename, directory);
                }
            }

            /*
            foreach (var file in dirFiles) {
                Log.Info(TAG, "dirFiles: " + file);
            }
            foreach (var id in imageIds) {
                Log.Info(TAG, "imageIds: " + id);
            }
            */            

            return (dirFiles, imageIds);
        }

        public static List<ImageRec> LoadDirectoryImages(String directory)
        {
            var table = db.Query<ImageRec>("select * from Image where directory = ?", directory);
            return table;
        }

        // image tag methods; no updating
        public static int AddImageTag(ImageTagRec newImageTag)
        {
            db.Insert(newImageTag);
            return newImageTag.id;
        }

        public static int AddImageTag(int imageId, int tagId)
        {
            return AddImageTag(new ImageTagRec(imageId, tagId));
        }

        public static int DeleteImageTag(ImageTagRec image)
        {
            return db.Delete(image);
        }

        public static int DeleteImageTag(int imageId, int tagId)
        {
            return db.Execute("delete from ImageTag where imageId = ? and tagId = ?", imageId, tagId);
        }

        public static List<ImageTagRec> LoadImageTags(int imageId)
        {
            var table = db.Query<ImageTagRec>("select * from ImageTag where imageId = ?", imageId);
            return table;
        }

    }
}