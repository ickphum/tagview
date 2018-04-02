using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Android.Preferences;

namespace Tagview
{
    public class BitmapCache 
    {
        private static String TAG = "BitmapCache";

        private int maxCacheEntries;
        public static int displayWidth;
        public static int displayHeight;

        List<String> sequenceFiles;
        List<int> sequenceImageIds;

        // current list situation
        String currentDirectory;
        String currentFile;
        int currentIndex;
        int currentSequenceId;
        HashSet<int> imageTagSet;

        // setting labels for preferences file
        public static string maxCacheEntriesSetting = "maxCacheEntries";
        public static string currentDirectorySetting = "currentDirectory";
        public static string currentFileSetting = "currentFile";
        public static string currentSequenceIdSetting = "currentSequenceId";
        public static string currentIndexSetting = "currentIndex";

        Dictionary<String, CacheEntry> bitmapCache;

        /*
         * The cache holds a certain number of records
         * 
         * 
         */

        public BitmapCache(int displayWidth, int displayHeight) {
            
            BitmapCache.displayWidth = displayWidth;
            BitmapCache.displayHeight = displayHeight;

            LoadPreferences();

            this.bitmapCache = new Dictionary<string, CacheEntry>();

            if (currentDirectory != null)
                LoadDirectory(currentDirectory);
            else
                Log.Info(TAG, "Not loading directory, no current directory");
        }

        public BitmapCache(int displayWidth, int displayHeight, string directory)
        {

            BitmapCache.displayWidth = displayWidth;
            BitmapCache.displayHeight = displayHeight;

            LoadPreferences();

            this.bitmapCache = new Dictionary<string, CacheEntry>();

            LoadDirectory(directory);
        }

        public void LoadPreferences()
        {
            // initialise the view settings from preferences
            Log.Info(TAG, "LoadPreferences");

            // load a preferences object so we don't do it with each call
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            maxCacheEntries = MainActivity.GetIntPref(Resource.String.maxCacheEntries, 5, prefs);
            currentDirectory = MainActivity.GetStringPref(Resource.String.currentDirectory, null, prefs);
            currentFile = MainActivity.GetStringPref(Resource.String.currentFile, null, prefs);
            currentIndex = MainActivity.GetIntPref(Resource.String.currentIndex, -1, prefs);
            currentSequenceId = MainActivity.GetIntPref(Resource.String.currentSequenceId, -1, prefs);

            return;
        }

        internal void LoadDirectory(string directory)
        {
            // /mnt/media_rw/25AD-18F8 /storage/25AD-18F8
            Log.Info(TAG, "LoadDirectory " + directory);

            //var internalStorageRoot = Android.OS.Environment.ExternalStorageDirectory.Path;
            //var cameraDir = System.IO.Path.Combine(internalStorageRoot, "DCIM", "Camera");
            // var mounts = System.IO.File.ReadAllText("/proc/mounts");
            // Log.Info(TAG, "mounts = " + mounts);
            // /storage/25AD-18F8 is the external SD root as found from /proc/mounts
            // sequenceFiles = new List<String>(System.IO.Directory.GetFiles(directory));
            (sequenceFiles, sequenceImageIds) = DataStore.OpenDirectory(directory, false);
            Log.Info(TAG, "LoadDirectory loaded this many files: " + sequenceFiles.Count);
            if (sequenceFiles.Count >= 0) {

                // if this is the previous current directory, try to reposition on the previous file
                if (directory.Equals(currentDirectory)) {

                    // see if we've got an exact match at the old position (if we had one)
                    if (currentIndex >= 0 && currentIndex < sequenceFiles.Count) {

                        // if currentFile matches the file at currentIndex, we're all good
                        // to restart at that point. Otherwise we need to search around that point
                        // for the same name.
                        if (!currentFile.Equals(sequenceFiles[currentIndex])) {

                            // files have been added or removed and the old file is somewhere
                            // else, if it exists. See if it does; this returns -1 if not found
                            // so it fits with ???
                            currentIndex = sequenceFiles.IndexOf(currentFile);

                        }
                    }
                }
                else {
                    currentIndex = 0;
                }
            }
            else {
                currentIndex = -1;
            }

            currentDirectory = directory;
            MainActivity.SetSinglePref(Resource.String.currentDirectory, currentDirectory);

            // load the current bitmap right now; the display of the initial image is waiting for us
            MoveToBitmap(0);

        }

        private void DumpCache(String tag)
        {
            Log.Info(TAG, "DumpCache " + tag);
            foreach (CacheEntry b in bitmapCache.Values) {
                Log.Info(TAG, "entry: {0} - {1}, bitmap is null: {2}", b.listIndex, b.filepath, b.bitmap == null ? "yes" : "no");

            }
        }

        public Bitmap MoveToBitmap(int indexIncrement)
        {
            Log.Info(TAG, "MoveToBitmap " + indexIncrement);

            if (currentDirectory == null) {
                Log.Info(TAG, "no current directory, bail on MoveToBitmap");
                return null;
            }

            if (currentIndex + indexIncrement >= 0 && currentIndex + indexIncrement < sequenceFiles.Count) {

                Log.Info(TAG, "load bitmap");

                CacheEntry entry;

                currentIndex += indexIncrement;
                currentFile = sequenceFiles[currentIndex];

                MainActivity.SetSinglePref(Resource.String.currentIndex, currentIndex);
                MainActivity.SetSinglePref(Resource.String.currentFile, currentFile);

                if (! bitmapCache.TryGetValue(currentFile, out entry)) {

                    // no match in cache; load from file and insert into cache
                    entry = new CacheEntry(currentFile, currentIndex, true);
                    bitmapCache.Add(currentFile, entry);
                }

                // load the list of tags for this image
                List<ImageTagRec> imageTags = DataStore.LoadImageTags(sequenceImageIds[currentIndex]);

                // the draw routine needs a quick way of telling what tag ids are current for an image.
                // These aren't cached, we create a hashSet whenever we change image
                imageTagSet = imageTags.ConvertAll(x => x.tagId).ToHashSet<int>();

                //DumpCache("1");

                // adjust the cache; add another entry in the direction we're heading
                // and if necessary, remove the entry furthest away in the other direction.
                // we've loaded the requested image or hopefully retrieved it from the cache.
                // Now load the next N images in the same direction into the cache, in the background.
                // Usually, after the first image loads (and we load the N images into cache), each navigation 
                // event will only load one more additional image, since the N-1 images will already be loaded.
                List<int> cacheIndexes = new List<int>();
                int cacheCount = maxCacheEntries >> 1;
                for (int i = 1; i <= cacheCount; i++) {

                    // count upward if going that way or starting out
                    if (indexIncrement >= 0) {
                        if (currentIndex + i < sequenceFiles.Count)
                            cacheIndexes.Add(currentIndex + i);
                    }

                    // ditto for downward 
                    if (indexIncrement <= 0) {
                        if (currentIndex - i >= 0)
                            cacheIndexes.Add(currentIndex - i);
                    }
                }

                foreach (var index in cacheIndexes) {

                    if (!bitmapCache.TryGetValue(sequenceFiles[index], out entry)) {

                        // before we add an entry, check to see if we need to remove an entry first
                        if (bitmapCache.Count == maxCacheEntries) {
                            Log.Info(TAG, "Cache full, remove entry");

                            // we remove the entry that's furthest in the opposite direction
                            // from where we moved this time, ie if indexIncrement is positive,
                            // we want the minimum index and v.v.
                            CacheEntry winner = new CacheEntry(null, -1);
                            foreach (CacheEntry b in bitmapCache.Values) {
                                if (winner.listIndex == -1 || (
                                        (indexIncrement > 0 && b.listIndex < winner.listIndex)
                                        || (indexIncrement < 0 && b.listIndex > winner.listIndex))) 
                                {
                                    Log.Info(TAG, "new winner: {0} - {1}", b.listIndex, b.filepath);
                                    if (b.bitmap == null) {
                                        Log.Warn(TAG, "new winner's bitmap is null");
                                    }
                                    winner = b;
                                }
                            }
                            Log.Info(TAG, "remove entry {0} - {1}", winner.listIndex, winner.filepath);

                            if (winner.bitmap == null) {
                                Log.Warn(TAG, "winner's bitmap is null");
                            }
                            winner.bitmap.Recycle();
                            winner.bitmap.Dispose();
                            bitmapCache.Remove(winner.filepath);

                        }

                        // no match in cache; insert placeholder into cache
                        CacheEntry newEntry = new CacheEntry(sequenceFiles[index], index);
                        bitmapCache.Add(sequenceFiles[index], newEntry);
                        
                        // queue the actual load in the background
                        ThreadPool.QueueUserWorkItem(o => {
                            newEntry.LoadBitmap();
                        });
                    }
                }

                //DumpCache("2");

                // this might be null, if the entry was queued for loading via
                // the cache but hasn't finished loading yet.
                bitmapCache.TryGetValue(currentFile, out entry);
                return entry.bitmap;                
            }

            // tried to move outside the range
            Log.Info(TAG, "Tried to move outside range; currentIndex {0}, indexIncrement {1}, file count {2}",
                currentIndex, indexIncrement, sequenceFiles.Count);
            return null;
        }

        internal bool CurrentImageHasTag(int tagId)
        {
            return imageTagSet.Contains(tagId);
        }

        internal void ToggleImageTag(int tagId)
        {
            int imageId = this.sequenceImageIds[currentIndex];
            Log.Info(TAG, "Toggle tag {0} for image id {1}", tagId, imageId);
            if (CurrentImageHasTag(tagId)) {
                DataStore.DeleteImageTag(imageId, tagId);
                imageTagSet.Remove(tagId);
            }
            else {
                DataStore.AddImageTag(imageId, tagId);
                imageTagSet.Add(tagId);
            }
        }
    }
}