using Android.App;
using Divisas2.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(Divisas2.Droid.CloseApplication))]

namespace Divisas2.Droid
{
	public class CloseApplication : ICloseApplication
    {
        public void Close()
        {
			var activity = (Activity)Forms.Context;
			activity.FinishAffinity();        
        }
    }
}