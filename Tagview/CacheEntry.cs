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
using Android.Graphics;
using Android.Util;

namespace Tagview
{
    class CacheEntry
    {
        private static string TAG = "CacheEntry";

        public String filepath;
        public int listIndex;
        public Bitmap bitmap;

        public CacheEntry(String filepath, int listIndex) : this(filepath, listIndex, false) { }

        public CacheEntry(String filepath, int listIndex, bool loadInForeground)
        {
            this.filepath = filepath;
            this.listIndex = listIndex;
            this.bitmap = null;
            if (loadInForeground) {
                Log.Info(TAG, "Immediate load of bitmap for {0} - {1}", listIndex, filepath);
                LoadBitmap();
            }
            else {
                Log.Info(TAG, "Deferred load of bitmap for {0} - {1}", listIndex, filepath);
            }
        }

        public void LoadBitmap()
        {
            BitmapFactory.Options options = new BitmapFactory.Options();

            options.InJustDecodeBounds = true;
            //Log.Info(TAG, "Get bitmap size");
            BitmapFactory.DecodeFile(filepath, options);
            int imageHeight = options.OutHeight;
            int imageWidth = options.OutWidth;
            String imageType = options.OutMimeType;
            //Log.Info(TAG, "bitmap size = " + imageWidth + ", " + imageHeight);
            options.InSampleSize = CalculateInSampleSize(options, BitmapCache.displayWidth, BitmapCache.displayHeight);
            options.InJustDecodeBounds = false;

            this.bitmap = BitmapFactory.DecodeFile(filepath, options);
            Log.Info(TAG, String.Format("Loaded bitmap {0} x {1} for {2}", bitmap.Width, bitmap.Height, filepath));
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            //Log.Info(TAG, "CalculateInSampleSize for " + reqWidth + "," + reqHeight);
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            while (height >= reqHeight && width >= reqWidth) {
                height >>= 1;
                width >>= 1;
                inSampleSize <<= 1;
            }

            // we want the biggest size that keeps both bitmap dims bigger
            // than the screen and one of them just went past it, so go back one step
            if (inSampleSize > 1)
                inSampleSize >>= 1;

            //Log.Info(TAG, String.Format("first pass gives inSampleSize {2}", width, height, inSampleSize));

            return inSampleSize;
        }

    }
}