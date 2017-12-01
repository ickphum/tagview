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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Categories);

            ListView category_lvw = FindViewById<ListView>(Resource.Id.category_lvw);
            //var adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItemMultipleChoice, DataStore.GetCategories());
            var adapter = new TableAdapter(this);
            adapter.Fill();
            category_lvw.Adapter = adapter;

            FindViewById<ImageButton>(Resource.Id.add_category_btn).Click += (object sender, EventArgs args) => {
                Log.Info("CategoriesActivity", "Add category");
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("New Category");
                EditText et = new EditText(this);
                alert.SetView(et);
                alert.SetPositiveButton("Add", (senderAlert, args2) => {
                    try {
                        //DataStore.AddCategory(et.Text);
                        Toast.MakeText(this, "Category added: " + et.Text, ToastLength.Short).Show();
                        adapter.Add(et.Text);
                        //System.Collections.ICollection items = DataStore.GetCategories();
                        //adapter.AddAll(items);
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