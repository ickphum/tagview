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
using Android.Util;
using SQLite;

namespace Tagview
{
    [Activity(Label = "CategoryActivity")]
    public class CategoryActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.DataList);
            int Id = Intent.GetIntExtra("Id", -1);
            string Name = Intent.GetStringExtra("Name");

            FindViewById<TextView>(Resource.Id.title_tvw).Text = Name;

            ListView category_tags_lvw = FindViewById<ListView>(Resource.Id.children_lvw);
            var adapter = new TagAdapter(this);
            adapter.Fill(Id);
            category_tags_lvw.Adapter = adapter;

            FindViewById<ImageButton>(Resource.Id.add_child_btn).Click += (object sender, EventArgs args) => {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("New Tag");
                EditText et = new EditText(this);
                alert.SetView(et);
                alert.SetPositiveButton("Add", (senderAlert, args2) => {
                    try {
                        Toast.MakeText(this, "Category added: " + et.Text, ToastLength.Short).Show();
                        adapter.Add(Id,et.Text);
                    }
                    catch (SQLiteException ex) {
                        Log.Error(this.ToString(), "Add failed : " + ex);
                        Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                    }
                });
                alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
                Dialog dialog = alert.Create();
                dialog.Show();

            };
        }
    }
}