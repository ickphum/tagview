using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Util;
using Android.Content;
using System.IO;
using SQLite;


namespace Tagview
{
    [Activity(Label = "Tagview", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity
    {
        public ImageView imageView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            this.imageView = FindViewById<ImageView>(Resource.Id.imageview_main);
            this.imageView.SetActivity(this);
            this.imageView.names = new[] { "Ian", "John", "Paul", "Wasi", "Mark" };

        }

        internal void ShowCategories()
        {
            Log.Info("MainActivity", "ShowCategories");
            var intent = new Intent(this, typeof(CategoriesActivity));
            StartActivity(intent);
        }

        internal void ShowSettings()
        {
            Log.Info("MainActivity", "ShowSettings");
            var intent = new Intent(this, typeof(SettingsActivity));
            StartActivity(intent);
        }
    }
}

