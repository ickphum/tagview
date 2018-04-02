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
    public class MenuDialog : DialogFragment
    {
        private MainActivity mainActivity;

        public static MenuDialog NewInstance(Bundle bundle)
        {
            MenuDialog fragment = new MenuDialog();
            fragment.Arguments = bundle;
            fragment.mainActivity = (MainActivity)fragment.Activity;
            return fragment;
        }

        public interface OnAddFriendListener
        {
            void onAddFriendSubmit(String friendEmail);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.menu, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            Button mResume_btn = view.FindViewById<Button>(Resource.Id.resume);
            mResume_btn.Click += (object sender, EventArgs args) =>
            {
                Log.Debug("MenuDialog", "resume");
                Dismiss();
            };

            view.FindViewById<Button>(Resource.Id.categories).Click += (object sender, EventArgs args) =>
            {
                ((MainActivity)this.Activity).ShowCategories();
            };

            view.FindViewById<Button>(Resource.Id.settings).Click += (object sender, EventArgs args) =>
            {
                ((MainActivity)this.Activity).ShowSettings();
            };

            view.FindViewById<Button>(Resource.Id.sequences).Click += (object sender, EventArgs args) => {
                ((MainActivity)this.Activity).ShowSequences();
            };

            view.FindViewById<Button>(Resource.Id.preferences).Click += (object sender, EventArgs args) => {
                ((MainActivity)this.Activity).ShowPreferences();
            };

            return view;

        }
    }
}