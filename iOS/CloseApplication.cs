using System.Threading;
using Divisas2.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(Divisas2.iOS.CloseApplication))]

namespace Divisas2.iOS
{
    public class CloseApplication : ICloseApplication
    {
        public void Close()
        {
            Thread.CurrentThread.Abort();

        }
    }
}