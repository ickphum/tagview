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
        string[] categories;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Categories);

            ListView category_lvw = FindViewById<ListView>(Resource.Id.category_lvw);
            category_lvw.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItemMultipleChoice, DataStore.GetCategories());

        }
    }
}