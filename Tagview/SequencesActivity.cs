﻿using System;
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
    [Activity(Label = "SequencesActivity")]
    public class SequencesActivity : Activity
    {
        private static string TAG = "SequencesActivity";

        SequenceAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.DataList);

            FindViewById<TextView>(Resource.Id.title_tvw).Text = "Sequences";

            ListView sequence_lvw = FindViewById<ListView>(Resource.Id.children_lvw);
            adapter = new SequenceAdapter(this);
            adapter.Fill();
            sequence_lvw.Adapter = adapter;

            FindViewById<ImageButton>(Resource.Id.add_child_btn).Click += (object sender, EventArgs args) => {
                Log.Info(TAG, "Add sequence");
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("New Sequence");
                EditText et = new EditText(this);
                alert.SetView(et);
                alert.SetPositiveButton("Add", (senderAlert, args2) => {
                    adapter.Add(et.Text);
                });
                alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
                Dialog dialog = alert.Create();
                dialog.Show();

            };
        }

        public void HandleSequenceClick(SequenceRec sequence)
        {
            Android.Widget.Toast.MakeText(this, "Selected " + sequence.Name, Android.Widget.ToastLength.Short).Show();
            Log.Info(TAG, "ItemClick");
            var sequence_activity = new Intent(this, typeof(SequenceActivity));
            sequence_activity.PutExtra("Id", sequence.Id);
            sequence_activity.PutExtra("Name", sequence.Name);
            StartActivity(sequence_activity);
        }

        public void EditSequence(int position, SequenceRec sequence)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Edit Sequence");
            EditText et = new EditText(this);
            et.Text = sequence.Name;

            alert.SetView(et);
            alert.SetPositiveButton("Update", (senderAlert, args2) => {
                sequence.Name = et.Text;
                adapter.Update(position, sequence);
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public void DeleteSequence(int position, SequenceRec sequence)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Delete Sequence '" + sequence.Name + "'");

            alert.SetPositiveButton("Delete", (senderAlert, args2) => {
                adapter.Delete(position, sequence);
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args2) => { });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

    }
}