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
    class TableAdapter : BaseAdapter<CategoryRec>
    {

        Context context;
        List<CategoryRec> items;

        public TableAdapter(Context context)
        {
            this.context = context;
            items = new List<CategoryRec>();
        }

        public override CategoryRec this[int position] {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void Fill()
        {
            items = DataStore.LoadCategories();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            TableAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as TableAdapterViewHolder;

            if (holder == null) {
                holder = new TableAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.TableListLayout, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.TableItemText);
                view.Tag = holder;
            }


            //fill in your items
            holder.Title.Text = items[position].Name;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Add(CategoryRec category)
        {
            items.Add(category);
            NotifyDataSetChanged();
            DataStore.AddCategory(category);
        }

        public void Add(String name)
        {
            CategoryRec newCategory = new CategoryRec(name);
            Add(newCategory);
        }

        public void Clear()
        {
            items.Clear();
        }

    }

    class TableAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}