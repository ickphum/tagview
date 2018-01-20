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
    class CategoryAdapter : BaseAdapter<CategoryRec>
    {

        Context context;
        List<CategoryRec> items;

        public CategoryAdapter(Context context)
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
                view = inflater.Inflate(Resource.Layout.DataListItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ListItemText);
                holder.Title.Click += (sender, e) => {
                    ((CategoriesActivity)context).HandleCategoryClick(this[holder.position]);
                };

                holder.Active = view.FindViewById<Switch>(Resource.Id.enabled_swt);
                holder.Active.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs args) => {
                    CategoryRec category = this[holder.position];
                    category.active = args.IsChecked;
                    Update(holder.position, category);
                };

                holder.EditChild = view.FindViewById<ImageButton>(Resource.Id.edit_child_btn);
                holder.EditChild.Click += (sender, e) => {
                    ((CategoriesActivity)context).EditCategory(holder.position, this[holder.position]);
                };

                holder.DeleteChild = view.FindViewById<ImageButton>(Resource.Id.delete_child_btn);
                holder.DeleteChild.Click += (sender, e) => {
                    ((CategoriesActivity)context).DeleteCategory(holder.position, this[holder.position]);
                };

                view.Tag = holder;
            }

            holder.Title.Text = items[position].name;
            holder.Active.Checked = items[position].active;
            holder.position = position;

            return view;
        }

        public override int Count {
            get {
                return items.Count();
            }
        }

        public void Add(CategoryRec category)
        {
            try {
                DataStore.AddCategory(category);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Add failed : " + ex);
                return;
            }

            items.Add(category);
            NotifyDataSetChanged();
        }

        public void Add(String name)
        {
            CategoryRec newCategory = new CategoryRec(name);
            Add(newCategory);
        }

        public void Update(int position, CategoryRec category)
        {
            try {
                DataStore.UpdateCategory(category);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Update failed : " + ex);
                return;
            }

            items[position] = category;
            NotifyDataSetChanged();
        }

        public void Delete(int position, CategoryRec category)
        {
            try {
                DataStore.DeleteCategory(category);
            }
            catch (SQLiteException ex) {
                Log.Error(this.ToString(), "Delete failed : " + ex);
                return;
            }

            items.Remove(category);
            NotifyDataSetChanged();
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
        public Switch Active { get; set; }
        public ImageButton EditChild { get; set; }
        public ImageButton DeleteChild { get; set; }
        public int position { get; set; }
    }
}