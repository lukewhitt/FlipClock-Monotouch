using System;
using System.Collections.Generic;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;

namespace FlipClockExample
{
	public class FlipClockView : UIView
	{
		enum kFlipAnimationState {
			Normal = 0,
			TopDown,
			BottomDown
		}
		
		private kFlipAnimationState animationState;
		private UIView topHalfFrontView;
		private UIView bottomHalfFrontView;
		private UIView topHalfBackView;
		private UIView bottomHalfBackView;
		private int viewIndex;
		private int nextViewIndex;
		private List<UIView> clockTiles;
		private float duration, zDepth, fontSize;
		private bool isAnimating = false;
		private bool allowsTouches = true;
		
		
		public bool AllowsTouches
		{
			get { return allowsTouches; }
			set { allowsTouches = value; }
		}
		
		public FlipClockView (RectangleF _frame, float _duration, float _zDepth, float _fontSize)
		{
			duration = _duration;
			zDepth = _zDepth;
			fontSize = _fontSize;
			
			Frame = _frame;
			
			clockTiles = new List<UIView>();
			for (int i = 0; i<10; i++)
			{
				UIView aNewView = NewViewWithText(i);
				clockTiles.Add(aNewView);
			}
			
			UIView aNumberView = clockTiles[0];
			AddSubviewWithTapRecognizer(aNumberView);
		}
		
		public void Increment()
		{
			if (!isAnimating)
			{
				animationState = kFlipAnimationState.Normal;
				
				int tileCount = clockTiles.Count;
				nextViewIndex = 0;
				if (viewIndex == (tileCount -1))
					nextViewIndex = 0;
				else if (viewIndex == tileCount)
				{
					nextViewIndex = 1;
					viewIndex = 0;
				}
				else
					nextViewIndex = viewIndex + 1;
				
				
				
				ChangeAnimationState();
			}
		}
		
		private UIView NewViewWithText(int text)
		{
			UIView aNewView;
			
			UILabel digitLabel = new UILabel(RectangleF.Empty);
			digitLabel.Font = UIFont.SystemFontOfSize(fontSize);
			digitLabel.Text = text.ToString();
			digitLabel.TextAlignment = UITextAlignment.Center;
			digitLabel.TextColor = UIColor.FromRGB(43,43,43);
			digitLabel.BackgroundColor = UIColor.Clear;
			digitLabel.SizeToFit();
			
			aNewView = new UIView(RectangleF.Empty);
			aNewView.Frame = new RectangleF(new PointF(0,0), Frame.Size);
			aNewView.Layer.CornerRadius = 10f;
			aNewView.Layer.MasksToBounds = true;
			aNewView.Layer.BorderColor = UIColor.FromRGB(51,51,51).CGColor;
			aNewView.Layer.BorderWidth = 1f;
			aNewView.BackgroundColor = UIColor.White;
			
			digitLabel.Center = new PointF(aNewView.Bounds.Size.Width / 2, aNewView.Bounds.Size.Height / 2);
			aNewView.AddSubview(digitLabel);
			
			UIView lineView = new UIView();
			lineView.BackgroundColor = UIColor.FromRGB(127,127,127);
			lineView.Frame = new RectangleF(0f,0f,aNewView.Frame.Size.Width, 1f);
			lineView.Center = digitLabel.Center;
			
			aNewView.AddSubview(lineView);
			
			return aNewView;
		}
		
		private void AddSubviewWithTapRecognizer (UIView aView)
		{
			UITapGestureRecognizer tap = new UITapGestureRecognizer(HandleTap);
			AddGestureRecognizer(tap);
			this.AddSubview(aView);
		}
		
		private List<UIImageView> SnapshotsForView (UIView aView)
		{
			List<UIImageView> returnList;
			
			UIGraphics.BeginImageContextWithOptions(aView.Bounds.Size, aView.Layer.Opaque, 0f);
			aView.Layer.RenderInContext(UIGraphics.GetCurrentContext());
			UIImage renderedImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			
			SizeF size = new SizeF(renderedImage.Size.Width, renderedImage.Size.Height / 2);
			
			UIImage top, bottom;
			UIGraphics.BeginImageContextWithOptions(size, aView.Layer.Opaque, 0f);
			renderedImage.Draw(PointF.Empty);
			top = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			
			UIGraphics.BeginImageContextWithOptions(size, aView.Layer.Opaque, 0f);
			renderedImage.Draw(new PointF(PointF.Empty.X, -renderedImage.Size.Height / 2));
			bottom = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			
			UIImageView topHalfView = new UIImageView(top);
			UIImageView bottomHalfView = new UIImageView(bottom);
			
			returnList = new List<UIImageView>(2){topHalfView, bottomHalfView};
			
			return returnList;
		}
		
		public void HandleTap(UITapGestureRecognizer rec)
		{
			if (allowsTouches)
				Increment();
		}
		
		private void ChangeAnimationState()
		{
			switch (animationState)
			{
			case kFlipAnimationState.Normal:
				isAnimating = true;
				UIView aView = clockTiles[viewIndex];
				UIView nextView = clockTiles[nextViewIndex];
				viewIndex++;
				AnimateViewDown(aView, nextView, duration);
				animationState = kFlipAnimationState.TopDown;
				break;
				
			case kFlipAnimationState.TopDown:
				bottomHalfBackView.Superview.BringSubviewToFront(bottomHalfBackView);
				animationState = kFlipAnimationState.BottomDown;
				break;
				
			case kFlipAnimationState.BottomDown:
				UIView newView = clockTiles[nextViewIndex];
				AddSubviewWithTapRecognizer(newView);
				
				topHalfFrontView.RemoveFromSuperview();
				bottomHalfFrontView.RemoveFromSuperview();
				topHalfBackView.RemoveFromSuperview();
				bottomHalfBackView.RemoveFromSuperview();
				
				//nextViewIndex = null;
				//viewIndex = null;
				isAnimating = false;
				animationState = kFlipAnimationState.Normal;
				break;
			}
		}
		
		private void AnimateViewDown(UIView aView, UIView nextView, float duration)
		{
			List<UIImageView> frontViews = SnapshotsForView(aView);
			topHalfFrontView = frontViews[0];
			bottomHalfFrontView = frontViews[1];
			topHalfFrontView.Center = new PointF(aView.Center.X, aView.Center.Y-aView.Frame.Size.Height/4);
			
			this.AddSubview(topHalfFrontView);
			
			bottomHalfFrontView.Frame = topHalfFrontView.Frame;
			bottomHalfFrontView.Center = new PointF(bottomHalfFrontView.Center.X, aView.Center.Y+aView.Frame.Size.Height/4);
			this.AddSubview(bottomHalfFrontView);
			aView.RemoveFromSuperview();
			
			List<UIImageView> backViews = SnapshotsForView(nextView);
			topHalfBackView = backViews[0];
			bottomHalfBackView = backViews[1];
			topHalfBackView.Frame = topHalfFrontView.Frame;
			this.InsertSubviewBelow(topHalfBackView, topHalfFrontView);
			bottomHalfBackView.Frame = bottomHalfFrontView.Frame;
			this.InsertSubviewBelow(bottomHalfBackView, bottomHalfFrontView);
			
			CATransform3D skewedIDTransform = CATransform3D.Identity;
			float zDistance = zDepth;
			skewedIDTransform.m34 = 1.0f / -zDistance;
			
			PointF newTopViewAnchorPoint = new PointF(0.5f, 1.0f);
			PointF newTopViewCenter = GetCenter(topHalfFrontView.Center, topHalfFrontView.Layer.AnchorPoint,
			                                    newTopViewAnchorPoint, topHalfFrontView.Frame);
			topHalfFrontView.Layer.AnchorPoint = newTopViewAnchorPoint;
			topHalfFrontView.Center = newTopViewCenter;
			
			AnimationDelegate ad = new AnimationDelegate();
			ad.AnimationDidStop += HandleAnimationDidStop;
			
			float piOverTwo = (float)Math.PI/2;
			
			CABasicAnimation topAnim = CABasicAnimation.FromKeyPath("transform");
			topAnim.BeginTime = CAAnimation.CurrentMediaTime();
			topAnim.Duration = duration;
			topAnim.From = NSValue.FromCATransform3D(skewedIDTransform);
			topAnim.To = NSValue.FromCATransform3D(CATransform3D.MakeRotation((-piOverTwo), 1f, 0f, 0f));
			topAnim.Delegate = ad;
			topAnim.RemovedOnCompletion = false;
			topAnim.FillMode = CAFillMode.Forwards;
			topAnim.TimingFunction = CAMediaTimingFunction.FromControlPoints(0.7f, 0f, 1f, 1f);
			topHalfFrontView.Layer.AddAnimation(topAnim, "topDownFlip");
			
			PointF newBottomViewAnchorPoint = new PointF(0.5f, 0f);
			PointF newBottomViewCenter = GetCenter (bottomHalfBackView.Center, bottomHalfBackView.Layer.AnchorPoint, 
			                                        newBottomViewAnchorPoint, bottomHalfBackView.Frame);
			bottomHalfBackView.Layer.AnchorPoint = newBottomViewAnchorPoint;
			bottomHalfBackView.Center = newBottomViewCenter;
			
			CABasicAnimation bottomAnim = CABasicAnimation.FromKeyPath("transform");
			bottomAnim.BeginTime = topAnim.BeginTime + topAnim.Duration;
			bottomAnim.Duration = topAnim.Duration;
			bottomAnim.From = NSValue.FromCATransform3D(CATransform3D.MakeRotation((piOverTwo), 1f, 0f,0f));
			bottomAnim.To = NSValue.FromCATransform3D(skewedIDTransform);
			bottomAnim.Delegate = ad;
			bottomAnim.RemovedOnCompletion = false;
			bottomAnim.FillMode = CAFillMode.Both;
			bottomAnim.TimingFunction = CAMediaTimingFunction.FromControlPoints(0.3f,1f,1f,1f);
			bottomHalfBackView.Layer.AddAnimation(bottomAnim, "bottomDownFlip");
		}
		
		void HandleAnimationDidStop ()
		{
			ChangeAnimationState();
		}
		
		
		private PointF GetCenter(PointF oldCenter, PointF oldAnchor, PointF newAnchor, RectangleF frame)
		{
			PointF anchorPointDiff = new PointF(newAnchor.X - oldAnchor.X, newAnchor.Y - oldAnchor.Y);
			PointF newCenter = new PointF(oldCenter.X + (anchorPointDiff.X * frame.Size.Width),
			                              oldCenter.Y + (anchorPointDiff.Y * frame.Size.Height));
			
			return newCenter;
		}
	}
	
	public class AnimationDelegate : MonoTouch.CoreAnimation.CAAnimationDelegate
	{
		public delegate void AnimationStoppedEvent();
		public event AnimationStoppedEvent AnimationDidStop;
		
		public AnimationDelegate()
		{}
		
		public override void AnimationStopped (CAAnimation anim, bool finished)
		{
			if (AnimationDidStop != null)
				AnimationDidStop();
		}
	}
}

