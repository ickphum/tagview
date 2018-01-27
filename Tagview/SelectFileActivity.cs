using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace Tagview
{
    [Activity(Label = "SelectFileActivity")]
    public class SelectFileActivity : Activity
    {

        FileDialogAdapter fileAdapter;

        private static String TAG = "SelectFileActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.FileDialog);

            Boolean directoriesOnly = Intent.GetBooleanExtra("directoriesOnly", false);

            ListView file_lvw = FindViewById<ListView>(Resource.Id.file_lvw);
            fileAdapter = new FileDialogAdapter(this, directoriesOnly);
            file_lvw.Adapter = fileAdapter;

            String[] chunks = fileAdapter.getDirectoryChunks();
            ArrayAdapter chunkAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, chunks);
            chunkAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            Spinner parent_spn = FindViewById<Spinner>(Resource.Id.parent_spn);
            parent_spn.Adapter = chunkAdapter;

            parent_spn.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
                Spinner spinner = (Spinner)sender;
                var adapter = spinner.Adapter;

                String newDir = "";
                if (e.Position == 0) {
                    newDir = "/";
                }
                else {

                    // the root directory doesn't need a trailing slash so start at the second item
                    for (var i = 1; i <= e.Position; i++) {
                        newDir += ("/" + adapter.GetItem(i));
                    }
                }

                fileAdapter.DirectorySelected(new DirectoryItem(newDir));
            };

            FindViewById<Button>(Resource.Id.select_btn).Click += (object sender, EventArgs args) => {
                Intent myIntent = new Intent(this, typeof(SequenceActivity));
                myIntent.PutExtra("directory", fileAdapter.directory);
                myIntent.PutExtra("includeChildren", FindViewById<Switch>(Resource.Id.includeChildren_swt).Checked);

                SetResult(Result.Ok, myIntent);
                Finish();
            };

            FindViewById<Button>(Resource.Id.cancel_btn).Click += (object sender, EventArgs args) => {
                Intent myIntent = new Intent(this, typeof(SequenceActivity));
                SetResult(Result.Canceled, myIntent);
                Finish();
            };

        }

        internal void HandleItemClick(DirectoryItem directoryItem)
        {
            if (directoryItem.directoryFlag) {
                fileAdapter.DirectorySelected(directoryItem);
                String[] chunks = fileAdapter.getDirectoryChunks();
                ArrayAdapter chunkAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, chunks);
                chunkAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                Spinner parent_spn = FindViewById<Spinner>(Resource.Id.parent_spn);
                parent_spn.Adapter = chunkAdapter;
                parent_spn.SetSelection(chunks.Length - 1);
            }
        }
    }
}