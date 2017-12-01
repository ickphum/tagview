using Android.App;
using Android.OS;

namespace CustomView
{
    [Activity(Label = "CustomView", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            var awesomeview = FindViewById<AwesomeView>(Resource.Id.awesomeview_main);
            awesomeview.names = new[] { "Ian", "John", "Paul", "Wasi", "Mark" };
        }
    }
}

