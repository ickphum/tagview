using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database.Sqlite;
using Android.Util;

namespace Tagview
{
    class DirectoryItem
    {
        public String name { get; }
        public String basename { get; }
        public Boolean directoryFlag { get; }

        public DirectoryItem() : this ("", false, false) { }
        public DirectoryItem(String name) : this(name, false, false) { }
        public DirectoryItem(String name, Boolean directoryFlag) : this(name, directoryFlag, false) { }
        public DirectoryItem(String name, Boolean directoryFlag, Boolean noSplit)
        {
            this.name = name;
            this.directoryFlag = directoryFlag;

            // noSplit is used when we pass in the rootDirs 
            if (noSplit) {
                this.basename = name;
            }
            else {
                int lastSeparator = name.LastIndexOf('/');
                this.basename = name.Substring(lastSeparator+1);
            }
        }
    }

    class FileDialogAdapter : BaseAdapter<DirectoryItem>
    {
        private static string TAG = "FileDialogAdapter";
        Context context;
        public String directory;
        static String rootDirectory = "/";
        List<DirectoryItem> items = new List<DirectoryItem>();
        Boolean directoriesOnly;
        List<String> driveRoots = new List<string>();

        public FileDialogAdapter(Context context, Boolean directoriesOnly)
        {
            Log.Info(TAG, "create FileDialogAdapter");

            this.context = context;
            this.directoriesOnly = directoriesOnly;

            // initial directory is the previous one displayed
            directory = rootDirectory;

            // the list of top level user-accessible dirs includes the internal storage root
            // (referred to as ExternalStorageDirectory for some reason) and any actual external
            // drives, which should be found by parsing /proc/mounts.
            // This is the list that's passed back when the pseudo-root directory is selected;
            // after this point we fill properly, ie by looking up the file lists.
            driveRoots.Add(Android.OS.Environment.ExternalStorageDirectory.Path);
            driveRoots.Add("/storage/25AD-18F8");

            // load the initial list
            Fill();
        }

        public FileDialogAdapter(Context context) : this(context, false) { }

        public override DirectoryItem this[int position] 
        {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            FileDialogAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as FileDialogAdapterViewHolder;

            if (holder == null) {
                holder = new FileDialogAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.FileListItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ListItemText);
                holder.Title.Click += (sender, e) => {
                    ((SelectFileActivity)context).HandleItemClick(this[holder.position]);
                };

                holder.Image = view.FindViewById<Android.Widget.ImageView>(Resource.Id.ListItemImage);

                view.Tag = holder;
            }

            holder.Title.Text = items[position].basename;
            holder.Image.SetImageResource(items[position].directoryFlag
                ? Resource.Drawable.ic_folder_white_24dp
                : Resource.Drawable.ic_photo_white_24dp);

            holder.position = position;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Fill()
        {
            items.Clear();
            if (directory.Equals(rootDirectory)) {
                foreach (var dir in driveRoots) {
                    items.Add(new DirectoryItem(dir, true, true));
                }
            }
            else {
                string[] dirs = System.IO.Directory.GetDirectories(directory);
                foreach (var dir in dirs) {
                    items.Add(new DirectoryItem(dir, true));
                }
                if (!directoriesOnly) {
                    string[] files = System.IO.Directory.GetFiles(directory);
                    foreach (var file in files) {
                        items.Add(new DirectoryItem(file, false));
                    }
                }
            }
        }

        public String[] getDirectoryChunks()
        {

            if (directory.Equals(rootDirectory)) {
                return new String[] { "/" };
            }

            // we go back up the string getting chunks until we get one of the root dirs, 
            // then we add that root dir and the pseudo-root dir
            List<String> chunks = new List<String>();
            String workingDir = directory;
            Boolean foundRoot = false;
            while (!foundRoot) {

                // check if we're back to one of the roots
                foreach (var drive in driveRoots) {
                    if (workingDir.Equals(drive)) {
                        chunks.Add(drive.Substring(1));
                        chunks.Add(rootDirectory);
                        foundRoot = true;
                    }
                }

                if (!foundRoot) {

                    // not at root, so get the last chunk in the string 
                    int lastSlashPos = workingDir.LastIndexOf('/');
                    chunks.Add(workingDir.Substring(lastSlashPos + 1));
                    workingDir = workingDir.Substring(0, lastSlashPos);
                }
            }

            chunks.Reverse();
            return chunks.ToArray();
        }

        internal void DirectorySelected(DirectoryItem directoryItem)
        {
            directory = directoryItem.name;
            Fill();
            NotifyDataSetChanged();
        }

    }

    class FileDialogAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public Android.Widget.ImageView Image { get; set; }
        public int position { get; set; }
    }

}