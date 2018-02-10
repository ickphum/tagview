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
        public static string PACKAGE;

        public ImageShow imageView;
        private GestureDetector _gestureDetector;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // initialise the static package variable so the static ResetPreferences method can use it.
            PACKAGE = PackageName;
            var prefs = Application.Context.GetSharedPreferences(PackageName, FileCreationMode.Private);
            if (!prefs.Contains("version")) {
                Log.Info(TAG, "no version found, reset prefs");
                ResetPreferences();
                prefs = Application.Context.GetSharedPreferences(PackageName, FileCreationMode.Private);
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _gestureDetector = new GestureDetector(this);

            imageView = FindViewById<ImageShow>(Resource.Id.imageview_main);
            imageView.SetActivity(this);
        }

        protected override void OnResume()
        {
            Log.Info(TAG, "OnResume");
            base.OnResume();
            this.imageView.LoadPreferences();
            this.imageView.RefreshCategories(true);
            this.imageView.Invalidate();
        }

        public static void SetFloatPref(String label, float value)
        {
            var prefs = Application.Context.GetSharedPreferences(PACKAGE, FileCreationMode.Private);

            var prefEdit = prefs.Edit();
            prefEdit.PutFloat(label, value);

            prefEdit.Commit();
        }

        public static void ResetPreferences()
        {
            Log.Info(TAG, "ResetPreferences");

            var prefs = Application.Context.GetSharedPreferences(PACKAGE, FileCreationMode.Private);

            // install default preferences
            var prefEdit = prefs.Edit();
            prefEdit.PutInt("version", 1);
            prefEdit.PutFloat(ImageShow.radiusProportionSetting, 0.3f);
            prefEdit.PutFloat(ImageShow.buttonSurroundTintSetting, 0.7f);
            prefEdit.PutFloat(ImageShow.buttonBorderProportionSetting, 0.07f);
            prefEdit.PutFloat(ImageShow.categoryListProportionSetting, 0.24f);
            prefEdit.PutFloat(ImageShow.categoryListMarginProportionSetting, 0.08f);
            prefEdit.PutInt(ImageShow.categorySelectionAnimationPeriodSetting, 500);
            prefEdit.PutInt(ImageShow.categoryListAnimationPeriodSetting, 700);

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
                String directory = data.GetStringExtra("directory");
                Boolean includeChildren = data.GetBooleanExtra("includeChildren", false);
                Log.Info(TAG, "Dir select ok: " + directory);
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

