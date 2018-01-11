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
using SQLite;
using Android.Util;

namespace Tagview
{
    class SequenceAdapter : BaseAdapter<SequenceRec>
    {

        Context context;
        List<SequenceRec> items;

        public SequenceAdapter(Context context)
        {
            this.context = context;
            items = new List<SequenceRec>();
        }

        public override SequenceRec this[int position] {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void Fill()
        {
            items = DataStore.LoadSequences();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            SequenceAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as SequenceAdapterViewHolder;

            if (holder == null) {
                holder = new SequenceAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.DataListItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ListItemText);
                holder.Title.Click += (sender, e) => {
                    ((SequencesActivity)context).HandleSequenceClick(this[position]);
                };

                view.FindViewById<Switch>(Resource.Id.enabled_swt).Visibility = ViewStates.Gone;

                holder.EditChild = view.FindViewById<ImageButton>(Resource.Id.edit_child_btn);
                holder.EditChild.Click += (sender, e) => {
                    ((SequencesActivity)context).EditSequence(position, this[position]);
                };

                holder.DeleteChild = view.FindViewById<ImageButton>(Resource.Id.delete_child_btn);
                holder.DeleteChild.Click += (sender, e) => {
                    ((SequencesActivity)context).DeleteSequence(position, this[position]);
                };

                view.Tag = holder;
            }

            holder.Title.Text = items[position].Name;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Add(SequenceRec sequence)
        {
            try {
                DataStore.AddSequence(sequence);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Add failed : " + ex);
                return;
            }

            items.Add(sequence);
            NotifyDataSetChanged();
        }

        public void Add(String name)
        {
            SequenceRec newSequence = new SequenceRec(name);
            Add(newSequence);
        }

        public void Update(int position, SequenceRec sequence)
        {
            try {
                DataStore.UpdateSequence(sequence);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Update failed : " + ex);
                return;
            }

            items[position] = sequence;
            NotifyDataSetChanged();
        }

        public void Delete(int position, SequenceRec sequence)
        {
            try {
                DataStore.DeleteSequence(sequence);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Delete failed : " + ex);
                return;
            }

            items.Remove(sequence);
            NotifyDataSetChanged();
        }

        public void Clear()
        {
            items.Clear();
        }

    }

    class SequenceAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public ImageButton EditChild { get; set; }
        public ImageButton DeleteChild { get; set; }
    }
}