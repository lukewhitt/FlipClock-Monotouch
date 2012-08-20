using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace FlipClockExample
{
	public class MainViewController : UIViewController
	{
		public MainViewController ()
		{
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			FlipClockView flip = new FlipClockView(new RectangleF(10,15,50, 100), 0.3f, 150f, 60f);
			this.View.AddSubview(flip);
			
			FlipClockView flip2 = new FlipClockView(new RectangleF(140,15,50, 100), 0.3f, 150f, 60f);
			flip2.AllowsTouches = false;
			this.View.AddSubview(flip2);

		}
	}
}

