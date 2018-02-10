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
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.settings);

            FindViewById<Button>(Resource.Id.clear_database_btn).Click += (object sender, EventArgs args) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("OK To Clear Database");
                alert.SetMessage("This will remove all Categories, Tags, Sequences, etc. Is this OK?");
                alert.SetPositiveButton("Delete", (senderAlert, args2) => {
                    DataStore.ClearDatabase();
                    Toast.MakeText(this, "Database cleared.", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            };

            FindViewById<Button>(Resource.Id.reset_settings_btn).Click += (object sender, EventArgs args) => {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("OK To Reset Preferences");
                alert.SetMessage("This will restore all preferences to their initial values. Is this OK?");
                alert.SetPositiveButton("Reset", (senderAlert, args2) => {
                    MainActivity.ResetPreferences();
                    Toast.MakeText(this, "Preferences reset.", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            };

        }
    }
}