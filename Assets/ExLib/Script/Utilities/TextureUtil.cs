using UnityEngine;
using System.Collections;
using System.Threading;

namespace ExLib.Utils
{
    public sealed class TextureScale
    {
        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static System.Action<Texture2D> OnComplete;

        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();
            if (OnComplete != null)
                OnComplete.Invoke(tex);
            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }
    }

    public static class TextureUtil
    {
        public enum TextureRotationAxis
        {
            ROTATE_AXIS_Y_180,
            ROTATE_AXIS_X_180,
            ROTATE_AXIS_Z_180,
            ROTATE_AXIS_Z_CW_90,
            ROTATE_AXIS_Z_CCW_90,
        }

        public static Texture2D ResizeTexture(Texture2D source, Vector2 size)
        {
            //*** Get All the source pixels
            Color[] aSourceColor = source.GetPixels(0);
            Vector2 vSourceSize = new Vector2(source.width, source.height);

            //*** Calculate New Size
            float xWidth = size.x;
            float xHeight = size.y;

            //*** Make New
            Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

            //*** Make destination array
            int xLength = (int)xWidth * (int)xHeight;
            Color[] aColor = new Color[xLength];

            Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

            //*** Loop through destination pixels and process
            Vector2 vCenter = new Vector2();
            for (int ii = 0; ii < xLength; ii++)
            {
                //*** Figure out x&y
                float xX = (float)ii % xWidth;
                float xY = Mathf.Floor((float)ii / xWidth);

                //*** Calculate Center
                vCenter.x = (xX / xWidth) * vSourceSize.x;
                vCenter.y = (xY / xHeight) * vSourceSize.y;

                //*** Average
                //*** Calculate grid around point
                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                //*** Loop and accumulate
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for (int iy = xYFrom; iy < xYTo; iy++)
                {
                    for (int ix = xXFrom; ix < xXTo; ix++)
                    {

                        //*** Get Color
                        oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[ii] = oColorTemp / (float)xGridCount;
            }

            //*** Set Pixels
            oNewTex.SetPixels(aColor);
            oNewTex.Apply();

            //*** Return
            return oNewTex;
        }

        public static void FlipTexture(Texture2D origin, ref Texture2D target, TextureRotationAxis flipWay)
        {
            int hlen = origin.width;
            int vlen = origin.height;

            if (origin.width != target.width || origin.height != target.height)
            {
                Texture2D.DestroyImmediate(target);
                target = new Texture2D(origin.width, origin.height);
            }

            for (int i = 0; i < hlen; i++)
            {
                for (int j = 0; j < vlen; j++)
                {
                    if (flipWay == TextureRotationAxis.ROTATE_AXIS_Y_180)
                    {
                        target.SetPixel(hlen - 1 - i, j, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_X_180)
                    {
                        target.SetPixel(i, vlen - 1 - j, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_180)
                    {
                        Color[] colors = origin.GetPixels();
                        System.Array.Reverse(colors);
                        target.SetPixels(colors);
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CW_90)
                    {
                        target.SetPixel(vlen - 1 - j, i, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CCW_90)
                    {
                        target.SetPixel(j, hlen - 1 - i, origin.GetPixel(i, j));
                    }
                }
            }

            target.Apply();
        }



        public static Color[] FlipTexture(int width, int height, Color[] origin, TextureRotationAxis flipWay)
        {
            Color[] colors = new Color[origin.Length];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (flipWay == TextureRotationAxis.ROTATE_AXIS_Y_180)
                    {
                        int xa = width * j + i;
                        int xb = width * j + (width - 1 - i);
                        colors[xb] = origin[xa];
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_X_180)
                    {
                        int xa = width * j + i;
                        int xb = width * (height - 1 - j) + i;
                        colors[xb] = origin[xa];
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_180)
                    {
                        System.Array.Reverse(origin);
                        System.Array.Copy(origin, colors, origin.Length);
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CW_90)
                    {
                        int xa = width * j + i;
                        int xb = (height - 1 - j) + i;
                        colors[xb] = origin[xa];
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CCW_90)
                    {
                        int xa = width * j + i;
                        int xb = (height - 1 - j) + (width - 1 - i);
                        colors[xb] = origin[xa];
                    }
                }
            }

            return colors;
        }

        public static void FlipTexture(WebCamTexture origin, ref Texture2D target, TextureRotationAxis flipWay)
        {
            int hlen = origin.width;
            int vlen = origin.height;

            if (origin.width != target.width || origin.height != target.height)
            {
                Texture2D.DestroyImmediate(target);
                target = new Texture2D(origin.width, origin.height);
            }

            for (int i = 0; i < hlen; i++)
            {
                for (int j = 0; j < vlen; j++)
                {
                    if (flipWay == TextureRotationAxis.ROTATE_AXIS_Y_180)
                    {
                        target.SetPixel(hlen - 1 - i, j, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_X_180)
                    {
                        target.SetPixel(i, vlen - 1 - j, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_180)
                    {
                        Color[] colors = origin.GetPixels();
                        System.Array.Reverse(colors);
                        target.SetPixels(colors);
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CW_90)
                    {
                        target.SetPixel(vlen - 1 - j, i, origin.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CCW_90)
                    {
                        target.SetPixel(j, hlen - 1 - i, origin.GetPixel(i, j));
                    }
                }
            }

            target.Apply();
        }

        public static void FlipTexture(ref Texture2D target, TextureRotationAxis flipWay)
        {
            Texture2D clone = new Texture2D(target.width, target.height);
            clone.SetPixels(target.GetPixels());
            int hlen = target.width;
            int vlen = target.height;

            for (int i = 0; i < hlen; i++)
            {
                for (int j = 0; j < vlen; j++)
                {
                    if (flipWay == TextureRotationAxis.ROTATE_AXIS_Y_180)
                    {
                        target.SetPixel(hlen - 1 - i, j, clone.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_X_180)
                    {
                        target.SetPixel(i, vlen - 1 - j, clone.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_180)
                    {
                        Color[] colors = clone.GetPixels();
                        System.Array.Reverse(colors);
                        target.SetPixels(colors);
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CW_90)
                    {
                        target.SetPixel(vlen - 1 - j, i, clone.GetPixel(i, j));
                    }
                    else if (flipWay == TextureRotationAxis.ROTATE_AXIS_Z_CCW_90)
                    {
                        target.SetPixel(j, hlen - 1 - i, clone.GetPixel(i, j));
                    }
                }
            }
            target.Apply();
            Texture2D.DestroyImmediate(clone);
        }
    }
}
