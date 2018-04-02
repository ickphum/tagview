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
using System.Linq;
using Com.Bumptech.Glide.Request.Target;
using Android.Preferences;

namespace Tagview
{
    public class ImageShow  : View
    {
        private static string TAG = "ImageShow";

        Context mContext;
        Activity mActivity;

        float displayScale;
        int displayWidth;
        int displayHeight;

        // setting labels for preferences file
        /*
        public static string radiusProportionSetting = "radiusProportion";
        public static string buttonSurroundTintSetting = "buttonSurroundTint";
        public static string buttonBorderProportionSetting = "buttonBorderProportion";
        public static string categoryListProportionSetting = "categoryListProportion";
        public static string categoryListMarginProportionSetting = "categoryListMarginProportion";
        public static string categorySelectionAnimationPeriodSetting = "categorySelectionAnimationPeriod";
        public static string categoryListAnimationPeriodSetting = "categoryListAnimationPeriod";
        public static string currentDirectorySetting = "currentDirectory";
        public static string currentFileSetting = "currentFile";
        public static string currentSequenceIdSetting = "currentSequenceId";
        public static string currentIndexSetting = "currentIndex";
        */

        float radiusProportion;
        float buttonSurroundTint;
        float buttonBorderProportion;
        float categoryListProportion;
        float categoryListMarginProportion;

        // this flag tells the other routines it's safe to use the db-sourced data,
        // otherwise it's being changed by the background thread.
        bool categoriesReady = false;
        List<CategoryRec> categories;
        List<List<TagRec>> categoryTags;
        Rect categoryListRect;
        List<Rect> categoryRects;
        List<Rect> tagRects;
        Rect currentCategoryRect;

        Rect tagWheelRect;

        BitmapCache bitmapCache;
        Bitmap currentBitmap;
        Rect currentBitmapRect;

        int categoryListTop;
        ValueAnimator categoryListAnimator;

        [Flags]
        enum categoryListStates
        {
            Hidden          = 0x0,
            Showing         = 0x01,
            Shown           = 0x02,
            Hiding          = 0x04,
            ShowingCurrent  = 0x08,
            CurrentShown    = 0x10,
            HidingCurrent   = 0x20,
        }
        categoryListStates categoryListState = categoryListStates.Hidden;
        //static categoryListStates listShown = categoryListStates.Showing | categoryListStates.Shown | categoryListStates.Hiding;
        //static categoryListStates currentShown = categoryListStates.ShowingCurrent | categoryListStates.CurrentShown | categoryListStates.HidingCurrent;

        ValueAnimator currentCategoryXAnimator;
        ValueAnimator currentCategoryYAnimator;
        ValueAnimator oldCurrentCategoryXAnimator;
        ValueAnimator oldCurrentCategoryYAnimator;
        int currentCategoryX;
        int currentCategoryY;
        int currentCategoryIndex = -1;
        int oldCurrentCategoryX;
        int oldCurrentCategoryY;
        int oldCurrentCategoryIndex = -1;

        ValueAnimator tagLabelRadiusAnimator;
        ValueAnimator tagStartAngleAnimator;
        int tagLabelRadius = 0;
        float tagStartAngle = 0;

        int categorySelectionAnimationPeriod;
        int categoryListAnimationPeriod;

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

            LoadPreferences();

            displayWidth = ctx.Resources.DisplayMetrics.WidthPixels;
            displayHeight = ctx.Resources.DisplayMetrics.HeightPixels;
            categoryListRect = new Rect(0, 0, (int)(displayWidth * categoryListProportion), displayHeight);
            tagWheelRect = new Rect(categoryListRect.Width(), 0, displayWidth, displayWidth - categoryListRect.Width());
            categoryRects = new List<Rect>();
            tagRects = new List<Rect>();

            // create animators
            {

                categoryListAnimator = new ValueAnimator();
                categoryListAnimator.SetDuration(categoryListAnimationPeriod);
                categoryListAnimator.SetInterpolator(new DecelerateInterpolator());
                categoryListAnimator.SetIntValues(0, ctx.Resources.DisplayMetrics.HeightPixels);
                categoryListAnimator.Update += (sender, e) => {
                    categoryListTop = (int)e.Animation.AnimatedValue;
                    Invalidate();
                };
                categoryListAnimator.AnimationEnd += (sender, e) => {
                    categoryListState = categoryListState == categoryListStates.Hiding
                        ? categoryListStates.Hidden
                        : categoryListStates.Shown;
                };

                currentCategoryXAnimator = new ValueAnimator();
                currentCategoryXAnimator.SetDuration(categorySelectionAnimationPeriod);
                currentCategoryXAnimator.SetInterpolator(new DecelerateInterpolator());
                currentCategoryXAnimator.Update += (sender, e) => {
                    currentCategoryX = (int)e.Animation.AnimatedValue;
                    Invalidate();
                };

                currentCategoryYAnimator = new ValueAnimator();
                currentCategoryYAnimator.SetDuration(categorySelectionAnimationPeriod);
                currentCategoryYAnimator.SetInterpolator(new DecelerateInterpolator());
                currentCategoryYAnimator.Update += (sender, e) => {
                    currentCategoryY = (int)e.Animation.AnimatedValue;
                    Invalidate();
                };

                oldCurrentCategoryXAnimator = new ValueAnimator();
                oldCurrentCategoryXAnimator.SetDuration(categorySelectionAnimationPeriod);
                oldCurrentCategoryXAnimator.SetInterpolator(new DecelerateInterpolator());
                oldCurrentCategoryXAnimator.Update += (sender, e) => {
                    oldCurrentCategoryX = (int)e.Animation.AnimatedValue;
                    Invalidate();
                };
                oldCurrentCategoryXAnimator.AnimationEnd += (sender, e) => {
                    oldCurrentCategoryIndex = -1;
                };

                oldCurrentCategoryYAnimator = new ValueAnimator();
                oldCurrentCategoryYAnimator.SetDuration(categorySelectionAnimationPeriod);
                oldCurrentCategoryYAnimator.SetInterpolator(new DecelerateInterpolator());
                oldCurrentCategoryYAnimator.Update += (sender, e) => {
                    oldCurrentCategoryY = (int)e.Animation.AnimatedValue;
                };

                tagLabelRadiusAnimator = new ValueAnimator();
                tagLabelRadiusAnimator.SetDuration(categorySelectionAnimationPeriod);
                tagLabelRadiusAnimator.SetInterpolator(new DecelerateInterpolator());
                tagLabelRadiusAnimator.Update += (sender, e) => {
                    tagLabelRadius = (int)e.Animation.AnimatedValue;
                };

                tagStartAngleAnimator = new ValueAnimator();
                tagStartAngleAnimator.SetDuration(categorySelectionAnimationPeriod);
                tagStartAngleAnimator.SetInterpolator(new DecelerateInterpolator());
                tagStartAngleAnimator.SetFloatValues((float)Math.PI / 2, 0);
                tagStartAngleAnimator.Update += (sender, e) => {
                    tagStartAngle = (float)e.Animation.AnimatedValue;
                };

            }

            displayScale = ctx.Resources.DisplayMetrics.Density;

            // prepare the current directory, if any
            PrepareDirectory();
            
            //if (!PrepareCurrentBitmap(0)) {

            /*
                var file_activity = new Intent(mContext, typeof(SelectFileActivity));
                file_activity.PutExtra("directoriesOnly", true);

                // load will trigger if dir chosen
                ((MainActivity)mContext).StartActivityForResult(file_activity, MainActivity.directorySelection);
            */

            //}
            
        }

        public void LoadPreferences()
        {
            // initialise the view settings from preferences
            Log.Info(TAG, "LoadPreferences");
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            radiusProportion = MainActivity.GetFloatPref(Resource.String.radiusProportion, 0.3f, null);
            buttonSurroundTint = MainActivity.GetFloatPref(Resource.String.buttonSurroundTint, 0.7f, prefs);
            buttonBorderProportion = MainActivity.GetFloatPref(Resource.String.buttonBorderProportion, 0.07f, prefs);
            categoryListProportion = MainActivity.GetFloatPref(Resource.String.categoryListProportion, 0.24f, prefs);
            categoryListMarginProportion = MainActivity.GetFloatPref(Resource.String.categoryListMarginProportion, 0.08f, prefs);
            categorySelectionAnimationPeriod = MainActivity.GetIntPref(Resource.String.categorySelectionAnimationPeriod, 500, prefs);
            categoryListAnimationPeriod = MainActivity.GetIntPref(Resource.String.categoryListAnimationPeriod, 700, prefs);

            return;
        }

        public void PrepareDirectory()
        {
            // creating the cache will load the current directory/sequence, load the initial
            // bitmap and queue the background loading of the adjacent images for the cache
            Log.Info(TAG, "PrepareDirectory for currentDirectory");
            bitmapCache = new BitmapCache(displayWidth, displayHeight);
            PrepareCurrentBitmap(0);
        }

        public void PrepareDirectory(String directory)
        {
            // creating the cache with the specified directory, load the initial
            // bitmap and queue the background loading of the adjacent images for the cache
            Log.Info(TAG, "PrepareDirectory for new directory " + directory);
            bitmapCache = new BitmapCache(displayWidth, displayHeight, directory);
            PrepareCurrentBitmap(0);
        }

        private Boolean PrepareCurrentBitmap(int indexIncrement) {
            Log.Info(TAG, "PrepareCurrentBitmap " + indexIncrement);

            currentBitmap = bitmapCache.MoveToBitmap(indexIncrement);

            if (currentBitmap == null) {
                Log.Error(TAG, "LoadCachedBitmap ret'd null");
                return false;
            }
            else {
                
                float bitmapW = currentBitmap.Width;
                float bitmapH = currentBitmap.Height;
                //Log.Info("ImageView.OnDraw", "canvas size = " + canvasW + "x" + canvasH);
                float canvasRatio = (float)displayWidth / (float)displayHeight;
                float bitmapRatio = (float)bitmapW / (float)bitmapH;
                //Log.Info("ImageView.OnDraw", "bm size = " + bitmapW + "x" + bitmapH);
                float bitmapScale = bitmapRatio > canvasRatio
                    ? displayWidth / bitmapW
                    : displayHeight / bitmapH;
                //Log.Info("ImageView.OnDraw", "bmr = " + bitmapRatio + ", bms = " + bitmapScale);
                currentBitmapRect = new Rect(
                    (int)(bitmapRatio > canvasRatio ? 0 : (displayWidth - (bitmapW * bitmapScale)) / 2),
                    (int)(bitmapRatio > canvasRatio ? (displayHeight - (bitmapH * bitmapScale)) / 2 : 0),
                    (int)(bitmapRatio > canvasRatio ? displayWidth - 1 : displayWidth - ((displayWidth - (bitmapW * bitmapScale)) / 2)),
                    (int)(bitmapRatio > canvasRatio ? displayHeight - ((displayHeight - (bitmapH * bitmapScale)) / 2) : displayHeight - 1));

                Invalidate();
                Log.Info(TAG, "bitmap prepared ok");
                return true;
            }
        }

        internal void RefreshCategories(bool invalidate)
        {
            categoriesReady = false;
            Log.Info(TAG, "RefreshCategories " + invalidate);
            categories = DataStore.LoadCategories();
            categoryTags = new List<List<TagRec>>();
            foreach (var category in categories.Select((x, i) => new { rec = x, index = i })) {
                List<TagRec> tags = DataStore.LoadTags(category.rec.id);
                categoryTags.Add(tags);
            }
            categoriesReady = true;
            if (invalidate) {
                ((MainActivity)Context).RunOnUiThread(() => {
                    Log.Info(TAG, "Invalidate on UI thread");
                    Invalidate();
                });
            }
        }

        public void HandleSingleTap(MotionEvent e)
        {

            float centerScreenX = Width / 2.0f;
            float centerScreenY = Height / 2.0f;
            Log.Info(TAG, "touch event at " + e.GetX() + "," + e.GetY());
            int eventX = (int)e.GetX();
            int eventY = (int)e.GetY();
            
            foreach (var category in categoryRects.Select((x, i) => new { rect = x, index = i })) {
                if (category.rect.Contains(eventX, eventY) && currentCategoryIndex != category.index) {
                    //Toast.MakeText(mContext, String.Format("Category {0} selected", category.index), ToastLength.Short).Show();
                    currentCategoryXAnimator.SetIntValues(category.rect.Left, (int)(tagWheelRect.CenterX() - category.rect.Width() / 2));
                    currentCategoryYAnimator.SetIntValues(category.rect.Top, (int)(tagWheelRect.CenterY() - category.rect.Height() / 2));

                    if (currentCategoryIndex >= 0) {
                        oldCurrentCategoryXAnimator.SetIntValues(currentCategoryRect.Left, categoryRects[currentCategoryIndex].Left);
                        oldCurrentCategoryYAnimator.SetIntValues(currentCategoryRect.Top, categoryRects[currentCategoryIndex].Top);
                        oldCurrentCategoryIndex = currentCategoryIndex;
                        oldCurrentCategoryXAnimator.Start();
                        oldCurrentCategoryYAnimator.Start();
                    }

                    int maxTagLabelRadius = ((tagWheelRect.Width() - category.rect.Width()) >> 2) + (category.rect.Width() >> 1);
                    tagLabelRadiusAnimator.SetIntValues(0, maxTagLabelRadius);
                    tagLabelRadiusAnimator.Start();
                    tagStartAngleAnimator.Start();

                    currentCategoryIndex = category.index;
                    currentCategoryXAnimator.Start();
                    currentCategoryYAnimator.Start();

                    return;
                }
            }

            // tap on current category to close it
            if (currentCategoryRect != null && currentCategoryRect.Contains(eventX, eventY)) {
                oldCurrentCategoryXAnimator.SetIntValues(currentCategoryRect.Left, categoryRects[currentCategoryIndex].Left);
                oldCurrentCategoryYAnimator.SetIntValues(currentCategoryRect.Top, categoryRects[currentCategoryIndex].Top);
                oldCurrentCategoryIndex = currentCategoryIndex;
                currentCategoryIndex = -1;
                currentCategoryRect = null;
                oldCurrentCategoryXAnimator.Start();
                oldCurrentCategoryYAnimator.Start();
            }

            // tap on a tag to toggle it for this image
            foreach (var tag in tagRects.Select((x, i) => new { rect = x, index = i })) {
                if (tag.rect.Contains(eventX, eventY)) {
                    Log.Info(TAG, "toggle tag " + tag.index);
                    List<TagRec> tags = categoryTags[currentCategoryIndex];
                    bitmapCache.ToggleImageTag(tags[tag.index].id);
                    Invalidate();
                    return;
                }
            }

            // open new dir
            if (currentBitmap == null) {
                var file_activity = new Intent(mContext, typeof(SelectFileActivity));
                file_activity.PutExtra("directoriesOnly", true);
                ((MainActivity)mContext).StartActivityForResult(file_activity, 1);
            }

            return;
        }

        internal void HandleFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {

            if (categoryListRect.Contains((int)e1.GetX(), (int)e1.GetY())) {
                if (categoriesReady) {

                    if (Math.Abs(velocityY) > Math.Abs(velocityX) && Math.Abs(velocityY) > 2000) {
                        Log.Info(TAG, "vertical fling inside category list, vy=" + velocityY + ", state=" + categoryListState);

                        // vertical fling, so valid
                        if (velocityY < 0 && categoryListState == categoryListStates.Hidden) {
                            Log.Info(TAG, "show category list");
                            RefreshCategories(false);
                            categoryListState = categoryListStates.Showing;
                            categoryListAnimator.SetIntValues(mContext.Resources.DisplayMetrics.HeightPixels, 0);
                            categoryListAnimator.Start();
                        }
                        if (velocityY > 0 && categoryListState == categoryListStates.Shown) {
                            Log.Info(TAG, "hide category list");
                            currentCategoryIndex = -1;
                            oldCurrentCategoryIndex = -1;
                            currentCategoryRect = null;
                            categoryListState = categoryListStates.Hiding;
                            categoryListAnimator.SetIntValues(0, mContext.Resources.DisplayMetrics.HeightPixels);
                            categoryListAnimator.Start();
                        }
                    }

                }
                else {
                    Log.Info(TAG, "ignore category action, categories not ready");
                }
            }
            else {
                Log.Info(TAG, "fling outside category list " + categoryListRect);
                PrepareCurrentBitmap(velocityX > 0 ? -1 : 1);
                //AdjustButton(e1, e2, velocityX, velocityY);
            }
        }

        public void AdjustButton(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (Math.Abs(velocityX) > Math.Abs(velocityY)) {
                if (velocityX > 0) {
                    categoryListProportion += 0.02f;
                    MainActivity.SetSinglePref(Resource.String.categoryListProportion, categoryListProportion);
                }
                else {
                    if (categoryListProportion > 0.02) {
                        categoryListProportion -= 0.02f;
                        Log.Info(TAG, "categoryListProportion now " + categoryListProportion);
                        MainActivity.SetSinglePref(Resource.String.categoryListProportion, categoryListProportion);
                    }
                }
            }
            else {
                if (velocityY > 0) {
                    categoryListMarginProportion += 0.01f;
                    MainActivity.SetSinglePref(Resource.String.categoryListMarginProportion, categoryListMarginProportion);
                }
                else {
                    if (categoryListMarginProportion > 0.01) {
                        categoryListMarginProportion -= 0.01f;
                        MainActivity.SetSinglePref(Resource.String.categoryListMarginProportion, categoryListMarginProportion);
                    }
                }
            }

            int displayWidth = mContext.Resources.DisplayMetrics.WidthPixels;
            int displayHeight = mContext.Resources.DisplayMetrics.HeightPixels;
            categoryListRect = new Rect(0, 0, (int)(displayWidth * categoryListProportion), displayHeight);
            tagWheelRect = new Rect(categoryListRect.Width(), 0, displayWidth, displayWidth - categoryListRect.Width());

            Invalidate();
            Toast.MakeText(mContext, String.Format("Width {0}, Border {1}", categoryListProportion, categoryListMarginProportion), ToastLength.Short).Show();
        }

        private void DrawRoundedButton(Canvas canvas, float left, float top, float totalSize, string label, Color mainColor)
        {
            float buttonMargin = totalSize * categoryListMarginProportion;
            float buttonSize = totalSize - buttonMargin * 2;

            Single[] HSV = new Single[3];
            Color.ColorToHSV(mainColor, HSV);
            //Log.Info(TAG, String.Format("hsv for color {0} = {1},{2},{3}", color, HSV[0], HSV[1], HSV[2]));
            Int32 alpha = Color.GetAlphaComponent(mainColor.ToArgb());
            Single saveSaturation = HSV[1];
            Single saveValue = HSV[2];
            HSV[1] *= buttonSurroundTint;
            Color shine = Color.HSVToColor(alpha, HSV);
            HSV[1] = saveSaturation;
            HSV[2] *= buttonSurroundTint;
            Color shadow = Color.HSVToColor(alpha, HSV);
            
            canvas.DrawRoundRect(
                left + buttonMargin, top + buttonMargin, 
                left + buttonMargin + buttonSize, top + buttonMargin + buttonSize,
                buttonSize * radiusProportion,
                buttonSize * radiusProportion,
                new Paint() { Color = shine });

            Path shadowClip = new Path();
            shadowClip.MoveTo(left + totalSize, top);
            shadowClip.LineTo(left, top + totalSize);
            shadowClip.LineTo(left + totalSize, top + totalSize);
            shadowClip.Close();

            // use save and restore (below) to remove the clipping region after we've
            // drawn the shadow
            canvas.Save();
            canvas.ClipPath(shadowClip);

            canvas.DrawRoundRect(
                left + buttonMargin, top + buttonMargin,
                left + buttonMargin + buttonSize, top + buttonMargin + buttonSize,
                buttonSize * radiusProportion,
                buttonSize * radiusProportion,
                new Paint() { Color = shadow });

            canvas.Restore();

            Single borderWidth = buttonSize * buttonBorderProportion;
            //Log.Info(TAG, "borderWidth = " + borderWidth);

            canvas.DrawRoundRect(
                left + buttonMargin + borderWidth, top + buttonMargin + borderWidth,
                left + buttonMargin + buttonSize - borderWidth, 
                top + buttonMargin + buttonSize - borderWidth,
                (buttonSize - 2 * borderWidth) * radiusProportion,
                (buttonSize - 2 * borderWidth) * radiusProportion,
                new Paint() { Color = mainColor });

            var paintText = new Paint() { Color = mainColor.GetBrightness() > 0.6 ? Color.Black : Color.White };

            var textSizePx = (int)(15f * displayScale);
            paintText.TextSize = textSizePx;
            paintText.TextAlign = Paint.Align.Center;
            canvas.DrawText(
                //String.Format("{0}, S {1}, V {2}, B {3}", color.ToString(), saveSaturation, saveValue, mainColor.GetBrightness()),
                label,
                left + totalSize / 2, top + totalSize / 2 + textSizePx / 3, 
                paintText);

        }

        protected override void OnDraw(Canvas canvas)
        {
            float canvasW = canvas.Width;
            float canvasH = canvas.Height;

            if (currentBitmap != null) {
                Log.Info(TAG, "Draw bitmap");

                canvas.DrawBitmap(currentBitmap, null, currentBitmapRect, null);
            }
            else {
                Log.Info(TAG, "currentBitmap is null");
            }
            int listWidth = categoryListRect.Width();
            if (categoryListState != categoryListStates.Hidden) {
                categoryRects.Clear();
                foreach (var category in categories.Select((x, i) => new { rec = x, index = i })) {
                    int left = 0;
                    int top = categoryListTop + category.index * listWidth;
                    categoryRects.Add(new Rect(left, top, left + listWidth, top + listWidth));

                    if (category.index == currentCategoryIndex) {

                        // draw current category at alternate location; on the way or out

                        currentCategoryRect = new Rect(currentCategoryX, currentCategoryY, currentCategoryX + listWidth, currentCategoryY + listWidth);

                        // draw tags for category
                        List<TagRec> tags = categoryTags[currentCategoryIndex];
                        tagRects.Clear();
                        if (tags.Count > 0) {
                            var textPaint = new Paint() { Color = Color.Black };
                            var textSizePx = (int)(12f * displayScale);
                            textPaint.TextSize = textSizePx;

                            int maxLength = 0;
                            Rect textRect = new Rect();
                            foreach (var tag in tags) {
                                textPaint.GetTextBounds(tag.name, 0, tag.name.Length, textRect);
                                if (textRect.Width() > maxLength) {
                                    maxLength = textRect.Width();
                                }
                            }
                            maxLength = (int)(maxLength * 0.65);

                            int centerX = currentCategoryRect.CenterX();
                            int centerY = currentCategoryRect.CenterY();

                            Int32[] grayColors = new Int32[] { Color.White.ToArgb(), Color.LightGray.ToArgb(), Color.DarkGray.ToArgb() };
                            Int32[] greenColors = new Int32[] { UIntToColor(0xff9fff9a).ToArgb(), UIntToColor(0xff51e849).ToArgb(), UIntToColor(0xff269c1f).ToArgb() };
                            Single[] stops = new Single[] { 0.0f, 0.5f, 0.9f };

                            double angle = tagStartAngle;
                            double angleStep = (Math.PI * 2) / tags.Count;
                            foreach (var tag in tags.Select((x, i) => new { rec = x, index = i })) {
                                int tagX = (int)(tagLabelRadius * Math.Sin(angle));
                                int tagY = (int)(tagLabelRadius * Math.Cos(angle));

                                textPaint.GetTextBounds(tag.rec.name, 0, tag.rec.name.Length, textRect);
                                int height = Math.Abs(textRect.Top + textRect.Bottom);
                                int textMargin = 5;
                                textRect.Set(textRect.Left - textMargin, textRect.Top - textMargin,
                                    textRect.Right + textMargin, textRect.Bottom + textMargin);
                                Paint circlePaint = new Paint();
                                RadialGradient grayGradient = new RadialGradient(centerX + tagX, centerY + tagY, maxLength * 1.1f, grayColors, stops, Shader.TileMode.Clamp);
                                RadialGradient greenGradient = new RadialGradient(centerX + tagX, centerY + tagY, maxLength * 1.1f, greenColors, stops, Shader.TileMode.Clamp);

                                circlePaint.SetShader(bitmapCache.CurrentImageHasTag(tag.rec.id) ? greenGradient : grayGradient);
                                canvas.DrawCircle(
                                    centerX + tagX,
                                    centerY + tagY,
                                    maxLength,
                                    circlePaint);

                                // this is the rect we'll check for touches; easier to check and
                                // more forgiving than checking the circle
                                tagRects.Add(new Rect(centerX + tagX - maxLength, centerY + tagY - maxLength,
                                    centerX + tagX + maxLength, centerY + tagY + maxLength));
                                Log.Info(TAG, "new Rect({0},{1},{2},{3})", centerX + tagX - maxLength, centerY + tagY - maxLength,
                                    centerX + tagX + maxLength, centerY + tagY + maxLength);

                                //canvas.DrawLine(centerX, centerY, centerX + tagX, centerY + tagY, new Paint() { Color = Color.White });
                                canvas.DrawText(
                                    tag.rec.name,
                                    centerX + tagX - textRect.Width() / 2, centerY + tagY + height / 2, // + textRect.Height() / 2,
                                    textPaint);
                                // Log.Info(TAG, "tag " + tag.rec.name);

                                angle += angleStep;
                            }
                        }

                        DrawRoundedButton(canvas, currentCategoryX, currentCategoryY, listWidth, category.rec.name, KellysMaxContrastSet[category.index]);
                    }
                    else if (category.index == oldCurrentCategoryIndex) {

                        // draw previous current category on the way back
                        DrawRoundedButton(canvas, oldCurrentCategoryX, oldCurrentCategoryY, listWidth, category.rec.name, KellysMaxContrastSet[category.index]);
                    }
                    else {
                        DrawRoundedButton(canvas, left, top, listWidth, category.rec.name, KellysMaxContrastSet[category.index]);
                    }
                    
                }
            }

            if (false) {
                Paint stroke = new Paint() { Color = Color.White };
                stroke.SetStyle(Paint.Style.Stroke);
                canvas.DrawRect(categoryListRect, stroke);
                canvas.DrawRect(tagWheelRect, stroke);
            }
        }

    }
}