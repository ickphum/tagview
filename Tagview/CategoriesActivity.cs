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
using System.IO;
using Android.Util;

namespace Tagview
{
    [Activity(Label = "CategoriesActivity")]
    public class CategoriesActivity : Activity
    {

        CategoryAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.DataList);

            FindViewById<TextView>(Resource.Id.title_tvw).Text = "Categories";

            ListView category_lvw = FindViewById<ListView>(Resource.Id.children_lvw);
            adapter = new CategoryAdapter(this);
            adapter.Fill();
            category_lvw.Adapter = adapter;

            FindViewById<ImageButton>(Resource.Id.add_child_btn).Click += (object sender, EventArgs args) => {
                Log.Info("CategoriesActivity", "Add category");
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("New Category");
                EditText et = new EditText(this);
                alert.SetView(et);
                alert.SetPositiveButton("Add", (senderAlert, args2) => {
                    adapter.Add(et.Text);
                });
                alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
                
            };
        }

        public void HandleCategoryClick(CategoryRec category)
        {
            Android.Widget.Toast.MakeText(this, "Selected " + category.name, Android.Widget.ToastLength.Short).Show();
            Log.Info("CategoriesActivity", "ItemClick");
            var category_activity = new Intent(this, typeof(CategoryActivity));
            category_activity.PutExtra("Id", category.id);
            category_activity.PutExtra("Name", category.name);
            StartActivity(category_activity);
        }

        public void EditCategory(int position, CategoryRec category)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Edit Category");
            EditText et = new EditText(this);
            String name = category.name;
            if (category.single)
                name += "; single";
            et.Text = name;

            alert.SetView(et);
            alert.SetPositiveButton("Update", (senderAlert, args2) => {
                category.name = et.Text;
                adapter.Update(position, category);
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public void DeleteCategory(int position, CategoryRec category)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete Category '" + category.name + "'");

            alert.SetPositiveButton("Delete", (senderAlert, args2) => {
                adapter.Delete(position, category);
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}
