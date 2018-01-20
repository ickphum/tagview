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
    class TagAdapter : BaseAdapter<TagRec>
    {

        Context context;
        List<TagRec> items;

        public TagAdapter(Context context)
        {
            this.context = context;
            items = new List<TagRec>();
        }

        public override TagRec this[int position] {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void Fill(int category_id)
        {
            items = DataStore.LoadTags(category_id);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            TagAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as TagAdapterViewHolder;

            if (holder == null) {
                holder = new TagAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.DataListItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ListItemText);

                view.FindViewById<Switch>(Resource.Id.enabled_swt).Visibility = ViewStates.Gone;

                holder.EditChild = view.FindViewById<ImageButton>(Resource.Id.edit_child_btn);
                holder.EditChild.Click += (sender, e) => {
                    ((CategoryActivity)context).EditTag(holder.position, this[holder.position]);
                };

                holder.DeleteChild = view.FindViewById<ImageButton>(Resource.Id.delete_child_btn);
                holder.DeleteChild.Click += (sender, e) => {
                    ((CategoryActivity)context).DeleteTag(holder.position, this[holder.position]);
                };

                view.Tag = holder;
            }

            holder.Title.Text = items[position].name;
            holder.position = position;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Add(TagRec tag)
        {
            items.Add(tag);
            NotifyDataSetChanged();
            DataStore.AddTag(tag);
        }

        public void Add(int category_id, String name)
        {
            TagRec newTag = new TagRec(category_id, name);
            Add(newTag);
        }

        public void Update(int position, TagRec tag)
        {
            try {
                DataStore.UpdateTag(tag);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Update failed : " + ex);
                return;
            }

            items[position] = tag;
            NotifyDataSetChanged();
        }

        public void Delete(int position, TagRec tag)
        {
            try {
                DataStore.DeleteTag(tag);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Delete failed : " + ex);
                return;
            }

            items.Remove(tag);
            NotifyDataSetChanged();
        }


        public void Clear()
        {
            items.Clear();
        }

    }

    class TagAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public Switch Active { get; set; }
        public ImageButton EditChild { get; set; }
        public ImageButton DeleteChild { get; set; }
        public int position { get; set; }
    }
}