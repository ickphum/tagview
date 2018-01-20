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
    class SequenceDirAdapter : BaseAdapter<SequenceDirRec>
    {
        private static string TAG = "SequenceDirAdapter";
        Context context;
        List<SequenceDirRec> items;

        public SequenceDirAdapter(Context context)
        {
            this.context = context;
            items = new List<SequenceDirRec>();
        }

        public override SequenceDirRec this[int position] {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void Fill(int sequence_id)
        {
            items = DataStore.LoadSequenceDirs(sequence_id);
            foreach (var item in items) {
                Log.Info(TAG, "loaded item " + item.directory);
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            SequenceDirAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as SequenceDirAdapterViewHolder;

            if (holder == null) {
                Log.Info(TAG, "create holder for position " + position);
                holder = new SequenceDirAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.DataListItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ListItemText);

                view.FindViewById<Switch>(Resource.Id.enabled_swt).Visibility = ViewStates.Gone;
                view.FindViewById<ImageButton>(Resource.Id.edit_child_btn).Visibility = ViewStates.Gone;

                holder.DeleteChild = view.FindViewById<ImageButton>(Resource.Id.delete_child_btn);
                holder.DeleteChild.Click += (sender, e) => {
                    ((SequenceActivity)context).DeleteSequenceDir(holder.position, this[holder.position]);
                };

                view.Tag = holder;
            }

            holder.Title.Text = items[position].directory + (items[position].includeChildren ? "+" : "");
            holder.position = position;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Add(SequenceDirRec tag)
        {
            items.Add(tag);
            NotifyDataSetChanged();
            DataStore.AddSequenceDir(tag);
        }

        public void Add(int sequence_id, String directory, bool includeChildren)
        {
            SequenceDirRec newSequenceDir = new SequenceDirRec(sequence_id, directory, includeChildren);
            Add(newSequenceDir);
        }

        public void Update(int position, SequenceDirRec sequenceDir)
        {
            try {
                DataStore.UpdateSequenceDir(sequenceDir);
            }
            catch (SQLiteException ex) {
                Log.Error(TAG, "Update failed : " + ex);
                return;
            }

            items[position] = sequenceDir;
            NotifyDataSetChanged();
        }

        public void Delete(int position, SequenceDirRec sequenceDir)
        {
            try {
                DataStore.DeleteSequenceDir(sequenceDir);
            }
            catch (SQLiteException ex) {
                Log.Error(TAG, "Delete failed : " + ex);
                return;
            }

            items.Remove(sequenceDir);
            NotifyDataSetChanged();
        }


        public void Clear()
        {
            items.Clear();
        }

    }

    class SequenceDirAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public ImageButton DeleteChild { get; set; }
        public int position { get; set; }
    }

}