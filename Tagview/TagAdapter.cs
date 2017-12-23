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

        public void Clear()
        {
            items.Clear();
        }

    }

    class TagAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}