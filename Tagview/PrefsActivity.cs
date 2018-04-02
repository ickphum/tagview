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
    [Activity(Label = "PrefsActivity")]
    public class PrefsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.preferences);
            
            FragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.fragment_container, new PrefsFragment())
                .Commit();
                
        }
    }
}