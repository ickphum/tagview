using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Util;
using Android.Content;
using System.IO;
using SQLite;
using Android.Views;
using System.Threading;
using System.Collections.Generic;
using Android.Preferences;

namespace Tagview
{
    [Activity(Label = "Tagview", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity, GestureDetector.IOnGestureListener
    {
        private static string TAG = "MainActivity";

        public ImageShow imageView;
        private GestureDetector _gestureDetector;

        // actvities started for result need an identifier
        internal static int directorySelection = 1;

        protected override void OnCreate(Bundle bundle)
        {
            Log.Info(TAG, "OnCreate");
            base.OnCreate(bundle);

            // initialise the static package variable so the static ResetPreferences method can use it.
            //var prefs = Application.Context.GetSharedPreferences(PackageName, FileCreationMode.Private);
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            if (!prefs.Contains("version")) {
                Log.Info(TAG, "no version found, reset prefs");
                ResetPreferences();
                prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            }

            /*
            var sprefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            IDictionary<string, object> allPrefs = sprefs.All;
            foreach (var key in allPrefs.Keys) {
                allPrefs.TryGetValue(key, out object value);
                Log.Info(TAG, "pref {0} = {1}", key, value);
            }
            */

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _gestureDetector = new GestureDetector(this);

            imageView = FindViewById<ImageShow>(Resource.Id.imageview_main);
        }

        protected override void OnResume()
        {
            Log.Info(TAG, "OnResume");
            base.OnResume();
            ThreadPool.QueueUserWorkItem(o => {
                this.imageView.LoadPreferences();
                this.imageView.RefreshCategories(true);
            });
        }

        public static float GetFloatPref(int resourceId, float defaultValue, ISharedPreferences prefs)
        {
            return GetFloatPref(Application.Context.Resources.GetString(resourceId), defaultValue, prefs);
        }

        public static float GetFloatPref(String label, float defaultValue, ISharedPreferences prefs)
        {
            if (prefs == null)
                prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            string strValue = prefs.GetString(label, defaultValue.ToString());

            return float.Parse(strValue);
        }

        public static int GetIntPref(int resourceId, int defaultValue, ISharedPreferences prefs)
        {
            return GetIntPref(Application.Context.Resources.GetString(resourceId), defaultValue, prefs);
        }

        public static int GetIntPref(String label, int defaultValue, ISharedPreferences prefs)
        {
            if (prefs == null)
                prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            string strValue = prefs.GetString(label, defaultValue.ToString());

            return int.Parse(strValue);
        }

        public static string GetStringPref(int resourceId, string defaultValue, ISharedPreferences prefs)
        {
            return GetStringPref(Application.Context.Resources.GetString(resourceId), defaultValue, prefs);
        }

        public static string GetStringPref(String label, string defaultValue, ISharedPreferences prefs)
        {
            if (prefs == null)
                prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            return prefs.GetString(label, defaultValue);
        }

        public static void SetSinglePref(int resourceId, float value)
        {
            SetSinglePref(Application.Context.Resources.GetString(resourceId), value);
        }

        public static void SetSinglePref(String label, float value)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            var prefEdit = prefs.Edit();
            prefEdit.PutString(label, value.ToString());

            prefEdit.Commit();
        }

        public static void SetSinglePref(int resourceId, int value)
        {
            SetSinglePref(Application.Context.Resources.GetString(resourceId), value);
        }

        public static void SetSinglePref(String label, int value)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            var prefEdit = prefs.Edit();
            prefEdit.PutString(label, value.ToString());

            prefEdit.Commit();
        }

        public static void SetSinglePref(int resourceId, String value)
        {
            SetSinglePref(Application.Context.Resources.GetString(resourceId), value);
        }

        public static void SetSinglePref(String label, String value)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            var prefEdit = prefs.Edit();
            prefEdit.PutString(label, value);

            prefEdit.Commit();
        }

        public static void ResetPreferences()
        {
            Log.Info(TAG, "ResetPreferences");

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            // install default preferences
            var prefEdit = prefs.Edit();
            prefEdit.Clear();

            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.version), "1");

            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.radiusProportion), "0.3");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.buttonSurroundTint), "0.7");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.buttonBorderProportion), "0.07");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.categoryListProportion), "0.24");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.categoryListMarginProportion), "0.08");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.categorySelectionAnimationPeriod), "500");
            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.categoryListAnimationPeriod), "700");

            prefEdit.PutString(Application.Context.Resources.GetString(Resource.String.maxCacheEntries), "5");

            prefEdit.Commit();
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

        internal void ShowPreferences()
        {
            Log.Info(TAG, "ShowPreferences");
            var intent = new Intent(this, typeof(PrefsActivity));
            StartActivity(intent);
        }

        internal void ShowSequences()
        {
            Log.Info(TAG, "ShowSequences");
            var intent = new Intent(this, typeof(SequencesActivity));
            StartActivity(intent);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok) {
                if (requestCode == directorySelection) {
                    String directory = data.GetStringExtra("directory");
                    Boolean includeChildren = data.GetBooleanExtra("includeChildren", false);
                    Log.Info(TAG, "Dir select ok: " + directory);
                    imageView.PrepareDirectory(directory);
                }
            }
            else {
                Log.Info(TAG, "Dir select cancelled");
            }
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
            Log.Info(TAG, "OnFling: " + String.Format("Fling velocity: x {0}, y {1}, e1 {2},{3}, e2 {4},{5}",
                velocityX, velocityY, e1.GetX(), e1.GetY(), e2.GetX(), e2.GetY()));
            this.imageView.HandleFling(e1, e2, velocityX, velocityY);
            return true;
        }
        public void OnLongPress(MotionEvent e) {
            Log.Info(TAG, "OnLongPress");

            // start menu
            FragmentTransaction transaction = FragmentManager.BeginTransaction();
            MenuDialog menu = new MenuDialog();
            menu.Show(transaction, "Dialog Fragment");
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

