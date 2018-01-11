using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Util;
using Android.Content;
using System.IO;
using SQLite;
using Android.Views;

namespace Tagview
{
    [Activity(Label = "Tagview", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity, GestureDetector.IOnGestureListener
    {
        private static string TAG = "MainActivity";
        public ImageView imageView;
        private GestureDetector _gestureDetector;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _gestureDetector = new GestureDetector(this);

            this.imageView = FindViewById<ImageView>(Resource.Id.imageview_main);
            this.imageView.SetActivity(this);
            this.imageView.names = new[] { "Ian", "John", "Paul", "Wasi", "Mark" };

        }

        internal void ShowCategories()
        {
            Log.Info(TAG, "ShowCategories");
            var intent = new Intent(this, typeof(CategoriesActivity));
            StartActivity(intent);
        }

        internal void ShowSettings()
        {
            Log.Info(TAG, "ShowSettings");
            var intent = new Intent(this, typeof(SettingsActivity));
            StartActivity(intent);
        }

        internal void ShowSequences()
        {
            Log.Info(TAG, "ShowSequences");
            var intent = new Intent(this, typeof(SequencesActivity));
            StartActivity(intent);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return false;
        }

        public bool OnDown(MotionEvent e)
        {
            Log.Info(TAG, "OnDown");
            return true;
        }
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            Log.Info(TAG, "OnFling: " + String.Format("Fling velocity: {0} x {1}", velocityX, velocityY));
            return true;
        }
        public void OnLongPress(MotionEvent e) {
            Log.Info(TAG, "OnLongPress");
        }
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            Log.Info(TAG, "OnScroll");
            return true;
        }
        public void OnShowPress(MotionEvent e) {
            Log.Info(TAG, "OnShowPress");
        }
        public bool OnSingleTapUp(MotionEvent e)
        {
            Log.Info(TAG, "OnSingleTapUp");
            this.imageView.HandleSingleTap(e);
            return false;
        }
    }
}

