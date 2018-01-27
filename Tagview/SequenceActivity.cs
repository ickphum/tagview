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
using Newtonsoft.Json;

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
            sequenceId = Intent.GetIntExtra("sequenceId", -1);
            SequenceRec sequence = DataStore.FindSequence(sequenceId);

            Spinner sortSpinner = FindViewById<Spinner>(Resource.Id.sort_options_spn);
            var sortAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.sort_options_array, Resource.Layout.large_spinner);
            sortAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sortSpinner.Adapter = sortAdapter;
            sortSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
                sequence.sortCode = e.Position;
                DataStore.UpdateSequence(sequence);
            };

            // handling of the sort codes relies on the spinner's string list corresponding
            // with the enums through which we interpret the database codes, when we actually
            // use them. We don't need to think about the significance of the code when maintaining it.
            sortSpinner.SetSelection(sequence.sortCode);

            CheckBox reverseSortCheckbox = FindViewById<CheckBox>(Resource.Id.reverse_chb);
            reverseSortCheckbox.Checked = sequence.reverseSort == 1;
            reverseSortCheckbox.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) => {
                sequence.reverseSort = e.IsChecked ? 1 : 0;
                DataStore.UpdateSequence(sequence);
            };

            FindViewById<TextView>(Resource.Id.slideShowPeriodSecs_txt).Text = "" + sequence.slideShowPeriodSecs;

            FindViewById<ImageButton>(Resource.Id.timingDown_btn).Click += (object sender, EventArgs args) => {
                AdjustSlideshowPeriod(-1, sequence);
            };
            FindViewById<ImageButton>(Resource.Id.timingUp_btn).Click += (object sender, EventArgs args) => {
                AdjustSlideshowPeriod(1, sequence);
            };

            ListView directory_lvw = FindViewById<ListView>(Resource.Id.directory_lvw);
            dirAdapter = new SequenceDirAdapter(this);
            dirAdapter.Fill(sequence.id);
            directory_lvw.Adapter = dirAdapter;

            FindViewById<ImageButton>(Resource.Id.addDirectory_btn).Click += (object sender, EventArgs args) => {
                var file_activity = new Intent(this, typeof(SelectFileActivity));
                file_activity.PutExtra("directoriesOnly", true);
                StartActivityForResult(file_activity, 1);

                // We implement OnActivityResult below to know when to tell the adapter to refresh the list.
            };
        }

        private void AdjustSlideshowPeriod(float delta, SequenceRec sequence)
        {
            TextView slideshowPeriod_txt = FindViewById<TextView>(Resource.Id.slideShowPeriodSecs_txt);
            float currentPeriod = float.Parse(slideshowPeriod_txt.Text);
            if ((delta == -1 && currentPeriod < 2) || (delta == 1 && currentPeriod < 1)) {
                delta /= 10;
            }
            if ((delta == -1 && currentPeriod > 300) || (delta == 1 && currentPeriod > 290)) {
                delta *= 100;
            }
            else {
                if ((delta == -1 && currentPeriod > 30) || (delta == 1 && currentPeriod > 29)) {
                    delta *= 10;
                }
            }
            currentPeriod += delta;
            sequence.slideShowPeriodSecs = currentPeriod;
            DataStore.UpdateSequence(sequence);

            if (currentPeriod == 1000 && delta > 0) {
                Android.Widget.Toast.MakeText(this, "Seriously?", Android.Widget.ToastLength.Short).Show();
            }

            slideshowPeriod_txt.Text = "" + currentPeriod;

            // disable down button at 0.1
            FindViewById<ImageButton>(Resource.Id.timingDown_btn).Clickable = (currentPeriod > 0.15);
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