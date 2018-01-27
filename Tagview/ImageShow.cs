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
    public class ImageShow  : View
    {
        private static string TAG = "ImageShow";

        Context mContext;
        Activity mActivity;

        // Important properties for the large bubble
        int activeIndex = -1;
        float activeX = 0;
        float activeY = 0;
        float activeRadius = 60;
        float displayScale;

        ValueAnimator animatorX;
        ValueAnimator animatorY;
        ValueAnimator animatorRadius;
        Bitmap currentBitmap;

        Color[] colors = new[] { Color.Red, Color.LightBlue, Color.Green, Color.Yellow, Color.Orange };

        public string[] names { get; set; }

        private static readonly List<Color> KellysMaxContrastSet = new List<Color>
            {
            UIntToColor(0xFFFFB300), //Vivid Yellow
            UIntToColor(0xFFFFDA84), //Vivid Yellow (2)
            UIntToColor(0xFF803E75), //Strong Purple
            UIntToColor(0xFFFF6800), //Vivid Orange
            UIntToColor(0xFFA6BDD7), //Very Light Blue
            UIntToColor(0xFFC10020), //Vivid Red
            UIntToColor(0xFFCEA262), //Grayish Yellow
            UIntToColor(0xFF817066), //Medium Gray

            //The following will not be good for people with defective color vision
            UIntToColor(0xFF007D34), //Vivid Green
            UIntToColor(0xFFF6768E), //Strong Purplish Pink
            UIntToColor(0xFF00538A), //Strong Blue
            UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
            UIntToColor(0xFF53377A), //Strong Violet
            UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
            UIntToColor(0xFFB32851), //Strong Purplish Red
            UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
            UIntToColor(0xFF7F180D), //Strong Reddish Brown
            UIntToColor(0xFF93AA00), //Vivid Yellowish Green
            UIntToColor(0xFF593315), //Deep Yellowish Brown
            UIntToColor(0xFFF13A13), //Vivid Reddish Orange
            UIntToColor(0xFF232C16), //Dark Olive Green
        };

        static public Color UIntToColor(uint color)
        {
            Byte a = (byte)(color >> 24);
            Byte r = (byte)(color >> 16);
            Byte g = (byte)(color >> 8);
            Byte b = (byte)(color >> 0);
            return new Color(r, g, b, a);
        }

        public ImageShow(Context context) :
        base(context)
        {
            init(context);
        }
        public ImageShow(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
            init(context);
        }

        public ImageShow(Context context, IAttributeSet attrs, int defStyle) :
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
            displayScale = ctx.Resources.DisplayMetrics.Density;

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

            var internalStorageRoot = Android.OS.Environment.ExternalStorageDirectory.Path;
            var cameraDir = System.IO.Path.Combine(internalStorageRoot, "DCIM", "Camera");
            // var mounts = System.IO.File.ReadAllText("/proc/mounts");
            // Log.Info(TAG, "mounts = " + mounts);
            // /storage/25AD-18F8 is the external SD root as found from /proc/mounts
            string[] files = System.IO.Directory.GetFiles(cameraDir);
            Log.Info(TAG, "init file count = " + files.Length);
            foreach (string file in files) {
                Log.Info(TAG, "file = " + file);
            }
            currentBitmap = BitmapFactory.DecodeFile(files[0]);

            // /mnt/media_rw/25AD-18F8 /storage/25AD-18F8

        }

        internal void ShowCategories()
        {
            Log.Debug("ImageView", "ShowCategories");
        }

        public void SetActivity(Activity activity)
        {
            mActivity = activity;
        }

        public void HandleSingleTap(MotionEvent e)
        {

            float centerScreenX = Width / 2.0f;
            float centerScreenY = Height / 2.0f;
            Log.Info(TAG, "touch event at " + e.GetX() + "," + e.GetY());
            activeIndex = isInsideCircle(e.GetX(), e.GetY());
            if (activeIndex > -1)
            {
                Toast.MakeText(mContext, "Got index" + activeIndex, ToastLength.Long).Show();
                animatorX.SetFloatValues(new[] { (float)positions[activeIndex].First, centerScreenX });
                animatorY.SetFloatValues(new[] { (float)positions[activeIndex].Second, centerScreenY });
                animatorX.Start();
                animatorY.Start();
                animatorRadius.Start();
                return;
            }

            // start menu
            FragmentTransaction transaction = mActivity.FragmentManager.BeginTransaction();
            MenuDialog menu = new MenuDialog();
            menu.Show(transaction, "Dialog Fragment");

            return;
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
            // Convert the dps to pixels, based on density scale
            var textSizePx = (int)(30f * displayScale);
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
                // Convert the dps to pixels, based on density scale
                var textSizePx = (int)(20f * displayScale);

                var name = names[activeIndex];
                paintText.TextSize = textSizePx;
                paintText.TextAlign = Paint.Align.Center;
                canvas.DrawText(name, activeX, activeY + radius / 2, paintText);

            }
        }

        private void DrawRoundedButton(Canvas canvas, float left, float top, float width, float height, int color)
        {
            float radiusProportion = 0.2f;

            Color mainColor = KellysMaxContrastSet[color];
            Single[] HSV = new Single[3];
            Color.ColorToHSV(mainColor, HSV);
            Log.Info(TAG, String.Format("hsv for color {0} = {1},{2},{3}", color, HSV[0], HSV[1], HSV[2]));
            Int32 alpha = Color.GetAlphaComponent(mainColor.ToArgb());
            Single saveSaturation = HSV[1];
            Single borderWidth = width / 10;
            HSV[1] /= 2;
            Color shine = Color.HSVToColor(alpha, HSV);
            HSV[1] = saveSaturation;
            HSV[2] /= 2;
            Color shadow = Color.HSVToColor(alpha, HSV);
            
            canvas.DrawRoundRect(left, top, left + width, top + height,
                width * radiusProportion,
                height * radiusProportion,
                new Paint() { Color = shine });

            Path shadowClip = new Path();
            shadowClip.MoveTo(left + width, top);
            shadowClip.LineTo(left, top + height);
            shadowClip.LineTo(left + width, top + height);
            shadowClip.Close();
            canvas.Save();
            canvas.ClipPath(shadowClip);

            canvas.DrawRoundRect(left, top, left + width, top + height,
                width * radiusProportion,
                height * radiusProportion,
                new Paint() { Color = shadow });

            canvas.Restore();

            canvas.DrawRoundRect(left + borderWidth, top + borderWidth, left + width - borderWidth, top + height - borderWidth,
                (width - borderWidth) * radiusProportion,
                (height - borderWidth) * radiusProportion,
                new Paint() { Color = mainColor });

            var paintText = new Paint() { Color = Color.White };
            var textSizePx = (int)(15f * displayScale);
            paintText.TextSize = textSizePx;
            paintText.TextAlign = Paint.Align.Center;
            canvas.DrawText(color.ToString(), left + width / 2, top + height / 2 + textSizePx / 3, paintText);

        }

        protected override void OnDraw(Canvas canvas)
        {
            float canvasW = canvas.Width;
            float canvasH = canvas.Height;
            float bitmapW = currentBitmap.Width;
            float bitmapH = currentBitmap.Height;
            Log.Info("ImageView.OnDraw", "canvas size = " + canvasW + "x" + canvasH);
            float canvasRatio = (float)canvasW / (float)canvasH;
            float bitmapRatio = (float)bitmapW / (float)bitmapH;
            Log.Info("ImageView.OnDraw", "bm size = " + bitmapW + "x" + bitmapH);
            float bitmapScale = bitmapRatio > canvasRatio
                ? canvasW / bitmapW
                : canvasH / bitmapH;
            Log.Info("ImageView.OnDraw", "bmr = " + bitmapRatio + ", bms = " + bitmapScale);
            Rect dest = new Rect(
                (int)(bitmapRatio > canvasRatio ? 0 : (canvasW - (bitmapW * bitmapScale)) / 2),
                (int)(bitmapRatio > canvasRatio ? (canvasH - (bitmapH * bitmapScale)) / 2 : 0),
                (int)(bitmapRatio > canvasRatio ? canvasW - 1 : canvasW - ((canvasW - (bitmapW * bitmapScale)) / 2)),
                (int)(bitmapRatio > canvasRatio ? canvasH - ((canvasH - (bitmapH * bitmapScale)) / 2) : 0));
            Log.Info("ImageView.OnDraw", "dest L = " + dest.Left);
            Log.Info("ImageView.OnDraw", "dest T = " + dest.Top);
            Log.Info("ImageView.OnDraw", "dest R = " + dest.Right);
            Log.Info("ImageView.OnDraw", "dest B = " + dest.Bottom);
            Log.Info("ImageView.OnDraw", "dest W = " + dest.Width());
            Log.Info("ImageView.OnDraw", "dest H = " + dest.Height());

            canvas.DrawBitmap(currentBitmap, null, dest, null);

            drawSmallCircles(canvas);
            drawBigCircle(canvas);

            for (int i = 0; i < 10; i++) {
                DrawRoundedButton(canvas, 50f, 50f + i * 200f, 150f, 150f, i);
            }
            
        }

    }
}