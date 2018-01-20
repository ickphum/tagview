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

namespace Tagview
{
    [Activity(Label = "SequenceActivity")]
    public class SequenceActivity : Activity
    {
        private const String TAG = "SequenceActivity";

        SequenceDirAdapter dirAdapter;
        private int sequenceId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.sequence);
            sequenceId = Intent.GetIntExtra("Id", -1);
            string Name = Intent.GetStringExtra("Name");

            Spinner sortSpinner = FindViewById<Spinner>(Resource.Id.sort_options_spn);

            //spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var sortAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.sort_options_array, Resource.Layout.large_spinner);
            sortAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sortSpinner.Adapter = sortAdapter;

            ListView directory_lvw = FindViewById<ListView>(Resource.Id.directory_lvw);
            dirAdapter = new SequenceDirAdapter(this);
            dirAdapter.Fill(sequenceId);
            directory_lvw.Adapter = dirAdapter;

            FindViewById<ImageButton>(Resource.Id.addDirectory_btn).Click += (object sender, EventArgs args) => {
                var file_activity = new Intent(this, typeof(SelectFileActivity));
                file_activity.PutExtra("directoriesOnly", true);
                StartActivityForResult(file_activity, 1);

                // We implement OnActivityResult below to know when to tell the adapter to refresh the list.
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok) {
                String directory = data.GetStringExtra("directory");
                Boolean includeChildren = data.GetBooleanExtra("includeChildren", false);
                Log.Info(TAG, "Dir select ok: " + directory);
                dirAdapter.Add(new SequenceDirRec(sequenceId, directory, includeChildren));
            }
            else {
                Log.Info(TAG, "Dir select cancelled");
            }
        }


        public void DeleteSequenceDir(int position, SequenceDirRec sequenceDir)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete Directory '" + sequenceDir.directory + "' from Sequence?");

            alert.SetPositiveButton("Delete", (senderAlert, args2) => {
                dirAdapter.Delete(position, sequenceDir);
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

    }
}