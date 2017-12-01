using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Animation;

using Android.Util;
using Android.Graphics;
using Android.Views.Animations;
using Android.App;
using Android.OS;
using System;

namespace Tagview
{
    public class ImageView  : View
    {

        Context mContext;
        Activity mActivity;

        // Important properties for the large bubble
        int activeIndex = -1;
        float activeX = 0;
        float activeY = 0;
        float activeRadius = 60;
        ValueAnimator animatorX;
        ValueAnimator animatorY;
        ValueAnimator animatorRadius;

        Color[] colors = new[] { Color.Red, Color.LightBlue, Color.Green, Color.Yellow, Color.Orange };

        public string[] names { get; set; }

        public ImageView(Context context) :
        base(context)
        {
            init(context);
        }
        public ImageView(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
            init(context);
        }

        public ImageView(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
        {
            init(context);
        }

        private void init(Context ctx)
        {
            mContext = ctx;
            animatorX = new ValueAnimator();
            animatorY = new ValueAnimator();
            animatorRadius = new ValueAnimator();
            animatorX.SetDuration(1000);
            animatorY.SetDuration(1000);
            animatorRadius.SetDuration(1000);
            animatorX.SetInterpolator(new DecelerateInterpolator());
            animatorY.SetInterpolator(new BounceInterpolator());

            animatorRadius.SetIntValues(new[] { radius, radius_big });
            animatorRadius.Update += (sender, e) => {
                activeRadius = (float)e.Animation.AnimatedValue;
                Invalidate();
            };

            animatorX.Update += (sender, e) => {
                activeX = (float)e.Animation.AnimatedValue;
                Invalidate();
            };
            animatorY.Update += (sender, e) => {
                activeY = (float)e.Animation.AnimatedValue;
                Invalidate();
            };

        }

        internal void ShowCategories()
        {
            Log.Debug("ImageView", "ShowCategories");
        }

        public void SetActivity(Activity activity)
        {
            mActivity = activity;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {

            float centerScreenX = Width / 2.0f;
            float centerScreenY = Height / 2.0f;
            activeIndex = isInsideCircle(e.GetX(), e.GetY());
            if (activeIndex > -1)
            {
                Toast.MakeText(mContext, "Got index" + activeIndex, ToastLength.Long).Show();
                animatorX.SetFloatValues(new[] { (float)positions[activeIndex].First, centerScreenX });
                animatorY.SetFloatValues(new[] { (float)positions[activeIndex].Second, centerScreenY });
                animatorX.Start();
                animatorY.Start();
                animatorRadius.Start();
                return true;
            }

            // start menu
            FragmentTransaction transaction = mActivity.FragmentManager.BeginTransaction();
            MenuDialog menu = new MenuDialog();
            menu.Show(transaction, "Dialog Fragment");

            return false;
        }

        int isInsideCircle(float x, float y)
        {

            for (int i = 0; i < positions.Count; i++)
            {

                int centerX = (int)positions[i].First;
                int centerY = (int)positions[i].Second;

                if (System.Math.Pow(x - centerX, 2) + System.Math.Pow(y - centerY, 2) < System.Math.Pow(radius, 2))
                {
                    return i;
                }
            }

            return -1;
        }


        const int NUM_BUBBLES = 5;
        int radius = 60;
        List<Pair> positions = new List<Pair>();
        void initPositions()
        {

            if (positions.Count == 0)
            {

                int spacing = Width / NUM_BUBBLES;
                int shift = spacing / 2;
                int bottomMargin = 10;

                for (int i = 0; i < NUM_BUBBLES; i++)
                {
                    int x = i * spacing + shift;
                    int y = Height - radius * 2 - bottomMargin;
                    positions.Add(new Pair(x, y));
                }
            }
        }

        void drawSmallCircles(Canvas canvas)
        {

            initPositions();

            var paintText = new Paint() { Color = Color.Black };
            // Get the screen's density scale
            var scale = mContext.Resources.DisplayMetrics.Density;
            // Convert the dps to pixels, based on density scale
            var textSizePx = (int)(30f * scale);
            paintText.TextSize = textSizePx;
            paintText.TextAlign = Paint.Align.Center;

            for (int i = 0; i < NUM_BUBBLES; i++)
            {
                if (i == activeIndex)
                {
                    continue;
                }

                var paintCircle = new Paint() { Color = colors[i] };
                int x = (int)positions[i].First;
                int y = (int)positions[i].Second;
                canvas.DrawCircle(x, y, radius, paintCircle);
                canvas.DrawText("" + names[i][0], x, y + radius / 2, paintText);
            }
        }

        int radius_big = 180;
        private void drawBigCircle(Canvas canvas)
        {
            if (activeIndex > -1)
            {
                var paintCircle = new Paint() { Color = colors[activeIndex] };
                canvas.DrawCircle(activeX, activeY, activeRadius, paintCircle);

                var paintText = new Paint() { Color = Color.Black };
                //  the screen's density scale
                var scale = mContext.Resources.DisplayMetrics.Density;
                // Convert the dps to pixels, based on density scale
                var textSizePx = (int)(20f * scale);
                var name = names[activeIndex];
                paintText.TextSize = textSizePx;
                paintText.TextAlign = Paint.Align.Center;
                canvas.DrawText(name, activeX, activeY + radius / 2, paintText);

            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            drawSmallCircles(canvas);
            drawBigCircle(canvas);
        }

    }
}