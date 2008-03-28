/*----------------------------------------------------------------
// Copyright (C) 2008 jillzhang 版权所有。 
//  
// 文件名：GifHelper.cs
// 文件功能描述：
// 
// 创建标识：jillzhang 
// 修改标识：
// 修改描述：
//
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/

/*-------------------------New BSD License ------------------
 Copyright (c) 2008, jillzhang
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of jillzhang nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace Jillzhang.GifUtility
{
    public class GifHelper
    {
        #region 对gif动画添加水印
        /// <summary>
        /// 对gif动画添加水印
        /// </summary>
        /// <param name="gifFilePath">原gif动画的路径</param>
        /// <param name="text">水印文字</param>
        /// <param name="textForceColor">水印文字的颜色，因为gif不是真彩色图片，所以在显示的时候，该颜色可能有所误差，但基本上可以确定颜色范围</param>
        /// <param name="font">字体</param>
        /// <param name="x">水印位置横坐标</param>
        /// <param name="y">水印位置纵坐标</param>
        /// <param name="outputPath">输出路径</param>
        public void WaterMark(string gifFilePath, string text, Color textForceColor, Font font, float x, float y, string outputPath)
        {
            if (!File.Exists(gifFilePath))
            {
                throw new IOException(string.Format("文件{0}不存在!", gifFilePath));
            }
            using (Bitmap ora_Img = new Bitmap(gifFilePath))
            {
                if (ora_Img.RawFormat.Guid != ImageFormat.Gif.Guid)
                {
                    throw new IOException(string.Format("文件{0}!", gifFilePath));
                }
            }
            int lastDisposal = 0;
            Bitmap lastImage = null;
            using (GifDecoder decoder = new GifDecoder())
            {
                decoder.Decode(gifFilePath);
                short width = decoder.Width;
                short height = decoder.Height;
                int index = 0;
                Color textColor = textForceColor;// Color.FromArgb(closestC);
                foreach (GifFrame f in decoder.Frames)
                {
                    int xOffSet = f.ImageDescriptor.XOffSet;
                    int yOffSet = f.ImageDescriptor.YOffSet;
                    int iw = f.ImageDescriptor.Width;
                    int ih = f.ImageDescriptor.Height;
                    if ((f.Image.Width != width || f.Image.Height != height))
                    {
                        f.ImageDescriptor.XOffSet = 0;
                        f.ImageDescriptor.YOffSet = 0;
                        f.ImageDescriptor.Width = width;
                        f.ImageDescriptor.Height = height;
                    }
                    int transIndex = -1;
                    if (f.GraphicExtension.TransparencyFlag)
                    {
                        transIndex = f.GraphicExtension.TranIndex;
                    }
                    if (iw == width && ih == height && index == 0)
                    {
                        Graphics g = Graphics.FromImage(f.Image);
                        g.DrawString(text, font, new SolidBrush(textColor), new PointF(x, y));
                        g.Dispose();
                    }
                    else
                    {
                        int bgColor = Convert.ToInt32(decoder.GlobalColorIndexedTable[f.GraphicExtension.TranIndex]);
                        Color c = Color.FromArgb(bgColor);
                        Bitmap newImg = null;
                        Graphics g;

                        newImg = new Bitmap(width, height);
                        g = Graphics.FromImage(newImg);
                        if (lastImage != null)
                        {
                            g.DrawImageUnscaled(lastImage, new Point(0, 0));
                        }
                        if (f.GraphicExtension.DisposalMethod == 1)
                        {
                            g.DrawRectangle(new Pen(new SolidBrush(c)), new Rectangle(xOffSet, yOffSet, iw, ih));
                        }
                        if (f.GraphicExtension.DisposalMethod == 2 && lastDisposal != 1)
                        {
                            g.Clear(c);
                        }
                        g.DrawImageUnscaled(f.Image, new Point(xOffSet, yOffSet));
                        g.DrawString(text, font, new SolidBrush(textColor), new PointF(x, y));
                        g.Dispose();
                        f.Image.Dispose();
                        f.Image = newImg;
                    }
                    lastImage = f.Image;
                    Quantizer(f.Image, decoder.Palette);
                    lastDisposal = f.GraphicExtension.DisposalMethod;
                    index++;
                }

                using (GifEncoder gifEncoder = new GifEncoder(outputPath))
                {
                    gifEncoder.Encode(decoder);
                }
            }
        }
        #endregion

        public void GetRealImages(GifDecoder decoder)
        {
            int lastDisposal = 0;
            Bitmap lastImage = null;
            int index = 0;
            short width = decoder.Width;
            short height = decoder.Height;
            foreach (GifFrame f in decoder.Frames)
            {
                int xOffSet = f.ImageDescriptor.XOffSet;
                int yOffSet = f.ImageDescriptor.YOffSet;
                int iw = f.ImageDescriptor.Width;
                int ih = f.ImageDescriptor.Height;
                if ((f.Image.Width != width || f.Image.Height != height))
                {
                    f.ImageDescriptor.XOffSet = 0;
                    f.ImageDescriptor.YOffSet = 0;
                    f.ImageDescriptor.Width = (short)width;
                    f.ImageDescriptor.Height = (short)height;
                }
                int transIndex = -1;
                if (f.GraphicExtension.TransparencyFlag)
                {
                    transIndex = f.GraphicExtension.TranIndex;
                }
                if (iw == width && ih == height && index == 0)
                {
                  
                }
                else
                {
                    int bgColor = Convert.ToInt32(decoder.GlobalColorIndexedTable[f.GraphicExtension.TranIndex]);
                    Color c = Color.FromArgb(bgColor);
                    Bitmap newImg = null;
                    Graphics g;

                    newImg = new Bitmap(width, height);
                    g = Graphics.FromImage(newImg);
                    if (lastImage != null)
                    {
                        g.DrawImageUnscaled(lastImage, new Point(0, 0));
                    }
                    if (f.GraphicExtension.DisposalMethod == 1)
                    {
                        g.DrawRectangle(new Pen(new SolidBrush(c)), new Rectangle(xOffSet, yOffSet, iw, ih));
                    }
                    if (f.GraphicExtension.DisposalMethod == 2 && lastDisposal != 1)
                    {
                        g.Clear(c);
                    }
                    g.DrawImageUnscaled(f.Image, new Point(xOffSet, yOffSet));                  
                    g.Dispose();
                    f.Image.Dispose();
                    f.Image = newImg;
                }
                lastImage = f.Image;
                Quantizer(f.Image, decoder.Palette);                
                lastDisposal = f.GraphicExtension.DisposalMethod;
                index++;
            }
        }

        #region gif动画缩略
        /// <summary>
        /// 获取gif动画的缩略图
        /// </summary>
        /// <param name="gifFilePath">原gif图片路径</param>
        /// <param name="rate">缩放大小</param>
        /// <param name="outputPath">缩略图大小</param>
        public void GetThumbnail(string gifFilePath, double rate, string outputPath)
        {
            if (!File.Exists(gifFilePath))
            {
                throw new IOException(string.Format("文件{0}不存在!", gifFilePath));
            }
            using (Bitmap ora_Img = new Bitmap(gifFilePath))
            {
                if (ora_Img.RawFormat.Guid != ImageFormat.Gif.Guid)
                {
                    throw new IOException(string.Format("文件{0}!", gifFilePath));
                }
            }
            using (GifDecoder d = new GifDecoder())
            {
                d.Decode(gifFilePath);
                if (rate != 1.0)
                {
                    d.LogicalScreenDescriptor.Width = (short)(d.LogicalScreenDescriptor.Width * rate);
                    d.LogicalScreenDescriptor.Height = (short)(d.LogicalScreenDescriptor.Height * rate);
                    int index = 0;
                    foreach (GifFrame f in d.Frames)
                    {
                        f.ImageDescriptor.XOffSet = (short)(f.ImageDescriptor.XOffSet * rate);
                        f.ImageDescriptor.YOffSet = (short)(f.ImageDescriptor.YOffSet * rate);
                        f.ImageDescriptor.Width = (short)(f.ImageDescriptor.Width * rate);
                        f.ImageDescriptor.Height = (short)(f.ImageDescriptor.Height * rate);
                        if (f.ImageDescriptor.Width == 0)
                        {
                            f.ImageDescriptor.Width = 1;
                        }
                        if (f.ImageDescriptor.Height == 0)
                        {
                            f.ImageDescriptor.Height = 1;
                        }
                        Bitmap bmp = new Bitmap(f.ImageDescriptor.Width, f.ImageDescriptor.Height);
                        Graphics g = Graphics.FromImage(bmp);
                        g.DrawImage(f.Image, new Rectangle(0, 0, f.ImageDescriptor.Width, f.ImageDescriptor.Height));
                        g.Dispose();
                        Quantizer(bmp, d.Palette);
                        f.Image.Dispose();
                        f.Image = bmp;
                        index++;
                    }
                }
                GifEncoder e = new GifEncoder(outputPath);
                e.Encode(d);
            }
        }

        #region 对图像进行量化，使其适应调色板
        /// <summary>
        /// 对图像进行量化，使其适应调色板
        /// </summary>
        /// <param name="bmp">图像</param>
        /// <param name="colorTab">调色板</param>
        void Quantizer(Bitmap bmp, Color32[] colorTab)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            Hashtable table = new Hashtable();
            unsafe
            {
                int* bmpScan = (int*)bmpData.Scan0.ToPointer();
                for (int i = 0; i < bmp.Height * bmp.Width; i++)
                {
                    Color c = Color.FromArgb(bmpScan[i]);
                    int rc = FindCloser(c, colorTab, table);
                    Color newc = Color.FromArgb(rc);
                    bmpScan[i] = rc;
                }
            }
            bmp.UnlockBits(bmpData);
        }
        /// <summary>
        /// 对图像进行量化，使其适应调色板
        /// </summary>
        /// <param name="bmp">图像</param>
        /// <param name="colorTab">调色板</param>
        void Quantizer(Bitmap bmp, int[] colorTab)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            Hashtable table = new Hashtable();
            unsafe
            {
                int* bmpScan = (int*)bmpData.Scan0.ToPointer();
                for (int i = 0; i < bmp.Height * bmp.Width; i++)
                {
                    Color c = Color.FromArgb(bmpScan[i]);
                    int rc = FindCloser(c, colorTab, table);
                    Color newc = Color.FromArgb(rc);
                    bmpScan[i] = rc;
                }
            }
            bmp.UnlockBits(bmpData);
        }
        int FindCloser(Color c, Color32[] act, Hashtable table)
        {
            if (table.Contains(c))
            {
                return ((Color32)table[c]).ARGB;
            }
            int index = 0;
            int min = 0;
            int minIndex = 0;
            while (index < act.Length)
            {
                Color ac = act[index].Color;
                int tempIndex = index;
                int cr = Math.Abs(c.R - ac.R);
                int cg = Math.Abs(c.G - ac.G);
                int cb = Math.Abs(c.B - ac.B);
                int ca = Math.Abs(c.A - ac.A);
                int result = cr + cg + cb + ca;
                if (result == 0)
                {
                    minIndex = tempIndex;
                    break;
                }
                if (tempIndex == 0)
                {
                    min = result;
                }
                else
                {
                    if (result < min)
                    {
                        min = result;
                        minIndex = tempIndex;
                    }
                }
                index++;
            }
            if (!table.Contains(c))
            {
                table.Add(c, act[minIndex]);
            }
            return act[minIndex].ARGB;
        }
        int FindCloser(Color c, int[] act, Hashtable table)
        {
            if (table.Contains(c))
            {
                return Convert.ToInt32(table[c]);
            }
            int index = 0;
            int min = 0;
            int minIndex = 0;
            while (index < act.Length)
            {
                Color ac = Color.FromArgb(act[index]);
                int tempIndex = index;
                int cr = Math.Abs(c.R - ac.R);
                int cg = Math.Abs(c.G - ac.G);
                int cb = Math.Abs(c.B - ac.B);
                int ca = Math.Abs(c.A - ac.A);
                int result = cr + cg + cb + ca;
                if (result == 0)
                {
                    minIndex = tempIndex;
                    break;
                }
                if (tempIndex == 0)
                {
                    min = result;
                }
                else
                {
                    if (result < min)
                    {
                        min = result;
                        minIndex = tempIndex;
                    }
                }
                index++;
            }
            if (!table.Contains(c))
            {
                table.Add(c, act[minIndex]);
            }
            return act[minIndex];
        }
        #endregion

        #endregion

        #region Gif动画单色化
        /// <summary>
        /// Gif动画单色化
        /// </summary>
        /// <param name="gifFilePath">原动画路径</param>
        /// <param name="outputPath">单色后动画路径</param>
        public void Monochrome(string gifFilePath, string outputPath)
        {
            if (!File.Exists(gifFilePath))
            {
                throw new IOException(string.Format("文件{0}不存在!", gifFilePath));
            }
            using (Bitmap ora_Img = new Bitmap(gifFilePath))
            {
                if (ora_Img.RawFormat.Guid != ImageFormat.Gif.Guid)
                {
                    throw new IOException(string.Format("文件{0}!", gifFilePath));
                }
            }
            using (GifDecoder decoder = new GifDecoder())
            {
                decoder.Decode(gifFilePath);
                int transIndex = decoder.LogicalScreenDescriptor.BgColorIndex;
                int c1 = (255 << 24) | (158 << 16) | (128 << 8) | 128;
                int c2 = (255 << 24) | (0 << 16) | (0 << 8) | 0;
                int c3 = (255 << 24) | (255 << 16) | (255 << 8) | 255;
                int c4 = (255 << 24) | (0 << 16) | (0 << 8) | 0;
                int[] palette = new int[] { c1, c2, c3, c4 };
                byte[] buffer = new byte[12] { 128, 128, 128, 0, 0, 0, 255, 255, 255, 0, 0, 0 };
                decoder.GlobalColorTable = buffer;
                decoder.LogicalScreenDescriptor.BgColorIndex = 0;
                decoder.LogicalScreenDescriptor.GlobalColorTableSize = 4;
                decoder.LogicalScreenDescriptor.GlobalColorTableFlag = true;
                int index = 0;
                foreach (GifFrame f in decoder.Frames)
                {
                    int[] act = decoder.GetColotTable(f.LocalColorTable);
                    f.LocalColorTable = buffer;
                    int bgColor = act[(transIndex / 3)];
                    Color bgC = Color.FromArgb(bgColor);
                    byte bgGray = (byte)(bgC.R * 0.3 + bgC.G * 0.59 + bgC.B * 0.11);
                    BitmapData bmpData = f.Image.LockBits(new Rectangle(0, 0, f.Image.Width, f.Image.Height), ImageLockMode.ReadWrite, f.Image.PixelFormat);
                    unsafe
                    {
                        int* p = (int*)bmpData.Scan0.ToPointer();
                        for (int i = 0; i < f.Image.Width * f.Image.Height; i++)
                        {
                            if (p[i] == 0)
                            {
                                p[i] = c1;

                            }
                            else
                            {
                                Color c = Color.FromArgb(p[i]);
                                int gray = (byte)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                                if (gray > bgGray)
                                {
                                    if (bgGray > 128)
                                    {
                                        p[i] = c2;
                                    }
                                    else
                                    {
                                        p[i] = c3;
                                    }
                                }
                                else if (gray < bgGray)
                                {
                                    if (bgGray > 128)
                                    {
                                        p[i] = c3;
                                    }
                                    else
                                    {
                                        p[i] = c2;
                                    }
                                }
                                else
                                {
                                    p[i] = c1;
                                }
                            }
                        }
                    }
                    f.Image.UnlockBits(bmpData);
                    f.GraphicExtension.TranIndex = 0;
                    f.ColorDepth = 2;
                    f.ImageDescriptor.LctFlag = false;
                    index++;
                }

                using (GifEncoder gifEncoder = new GifEncoder(outputPath))
                {
                    gifEncoder.Encode(decoder);
                }
            }
        }
        #endregion

        #region 合并多个gif动画,在时间坐标上
        Size FindMaxSize(List<string> sources)
        {
            List<int> widths = new List<int>();
            List<int> heights = new List<int>();
            foreach (string s in sources)
            {
                Bitmap bmp = new Bitmap(s);
                widths.Add(bmp.Width);
                heights.Add(bmp.Height);
                bmp.Dispose();
            }
            widths.Sort();
            heights.Sort();
            return new Size(widths[widths.Count - 1], heights[heights.Count - 1]);
        }



        /// <summary>
        /// 合并多个gif文件
        /// </summary>
        /// <param name="sourceGifs">原图像路径集合</param>
        /// <param name="outGif">合并后图像路径</param>
        /// <param name="delay">间隔时间</param>
        /// <param name="repeat">是否重复播放</param> 
        public void Merge(List<string> sourceGifs, string outGif, short delay, bool repeat)
        {
            GifDecoder decoder = new GifDecoder();
            List<GifDecoder> decoders = new List<GifDecoder>();
            int index = 0;
            short lastDelay = delay;
            foreach (string source in sourceGifs)
            {
                if (!File.Exists(source))
                {
                    throw new IOException(string.Format("文件{0}不存在!", source));
                }
                using (Bitmap ora_Img = new Bitmap(source))
                {
                    if (ora_Img.RawFormat.Guid != ImageFormat.Gif.Guid)
                    {
                        throw new IOException(string.Format("文件{0}!", source));
                    }
                }
                GifDecoder d = new GifDecoder();
                d.Decode(source);
                if (index == 0)
                {
                    decoder = d;
                }
                int frameCount = 0;
                foreach (GifFrame f in d.Frames)
                {
                    if (frameCount == 0 && f.GraphicExtension.DisposalMethod == 0)
                    {
                        f.GraphicExtension.DisposalMethod = 2;
                    }
                    if (!f.ImageDescriptor.LctFlag)
                    {
                        f.ImageDescriptor.LctSize = f.LocalColorTable.Length / 3;
                        f.ImageDescriptor.LctFlag = true;
                        f.GraphicExtension.TranIndex = d.LogicalScreenDescriptor.BgColorIndex;
                        f.LocalColorTable = d.GlobalColorTable;
                    }
                    if (frameCount == 0)
                    {
                        f.Delay = f.GraphicExtension.Delay = lastDelay;
                    }
                    if (f.Delay == 0)
                    {
                        f.Delay = f.GraphicExtension.Delay = lastDelay;
                    }
                    f.ColorDepth = (byte)(Math.Log(f.ImageDescriptor.LctSize, 2));
                    lastDelay = f.GraphicExtension.Delay;
                    frameCount++;
                }
                decoder.Frames.AddRange(d.Frames);
                decoders.Add(d);
                index++;
            }
            using (GifEncoder gifEncoder = new GifEncoder(outGif))
            {
                if (repeat && decoder.ApplictionExtensions.Count == 0)
                {
                    ApplicationEx ae = new ApplicationEx();
                    decoder.ApplictionExtensions.Add(ae);
                }
                decoder.LogicalScreenDescriptor.PixcelAspect = 0;
                Size maxSize = FindMaxSize(sourceGifs);
                decoder.LogicalScreenDescriptor.Width = decoder.Width = (short)maxSize.Width;
                decoder.LogicalScreenDescriptor.Height = decoder.Height = (short)maxSize.Height;
                gifEncoder.Encode(decoder);
                foreach (GifDecoder d in decoders)
                {
                    if (d != null)
                    {
                        d.Dispose();
                    }
                }
            }
        }
        #endregion

        
        #region 合并多个gif动画,在空间坐标上
        /// <summary>
        /// 合并多个gif动画,在空间坐标上
        /// </summary>
        /// <param name="sourceGifs">原图像</param>
        /// <param name="outPath">合并后图像</param>
        public void Merge(List<string> sourceGifs, string outPath)
        {
            List<List<GifFrame>> frames = new List<List<GifFrame>>();
            foreach (string source in sourceGifs)
            {
                if (!File.Exists(source))
                {
                    throw new IOException(string.Format("文件{0}不存在!", source));
                }
                using (Bitmap ora_Img = new Bitmap(source))
                {
                    if (ora_Img.RawFormat.Guid != ImageFormat.Gif.Guid)
                    {
                        throw new IOException(string.Format("文件{0}!", source));
                    }
                }
                GifDecoder d = new GifDecoder();
                d.Decode(source);
                GetRealImages(d);             
                int index = 0;
                foreach (GifFrame f in d.Frames)
                {                   
                    if (frames.Count <= index)
                    {
                        List<GifFrame> list = new List<GifFrame>();
                        frames.Add(list);
                    }
                    List<GifFrame> frameList = frames[index];
                    frameList.Add(f);
                    index++;
                }             
            }
            List<GifFrame> frameCol = new List<GifFrame>();
            int frameIndex = 0;
            foreach (List<GifFrame> fs in frames)
            {
                GifFrame frame = Merge(fs);           
                frameCol.Add(frame);                
                if(frame.Image.Width != frameCol[0].Image.Width
                    || frame.Image.Height != frameCol[0].Image.Height)
                {
                    frame.ImageDescriptor.XOffSet = frames[frameIndex][0].ImageDescriptor.XOffSet;
                    frame.ImageDescriptor.YOffSet = frames[frameIndex][0].ImageDescriptor.YOffSet;
                    frame.GraphicExtension.DisposalMethod = frames[frameIndex][0].GraphicExtension.DisposalMethod;                    
                }
                frame.GraphicExtension.Delay = frame.Delay = frames[frameIndex][0].Delay;                
                frameIndex++;
            }
            GifEncoder gif = new GifEncoder(outPath);
            gif.WriteHeader("GIF89a");
            LogicalScreenDescriptor lcd = new LogicalScreenDescriptor();
            lcd.Width =(short) frameCol[0].Image.Width;
            lcd.Height = (short)frameCol[0].Image.Height;
            gif.WriteLSD(lcd);
            ApplicationEx ape = new ApplicationEx();
            List<ApplicationEx> apps = new List<ApplicationEx>();
            apps.Add(ape);
            gif.SetApplicationExtensions(apps);
            gif.SetFrames(frameCol);
            gif.Dispose();
        }
        #endregion

        GifFrame Merge(List<GifFrame> frames)
        {
            Bitmap bmp = null;
            Graphics g = null;      
            foreach (GifFrame f in frames)
            {
                if (bmp == null)
                {
                    bmp = f.Image;
                    g = Graphics.FromImage(bmp);
                }
                else
                {
                    g.DrawImageUnscaled(f.Image, new Point(f.ImageDescriptor.XOffSet, f.ImageDescriptor.YOffSet));
                }
            }
            if (g != null)
            {
                g.Dispose();
            }
            GifFrame frame = new GifFrame();
            Color32[] pellatte = new OcTreeQuantizer(8).Quantizer(bmp);
            Quantizer(bmp, pellatte);
            frame.Image = bmp;
            frame.LocalColorTable = GetColorTable(pellatte);
            frame.ImageDescriptor = new ImageDescriptor();
            frame.ImageDescriptor.LctFlag = true;
            frame.ImageDescriptor.LctSize = pellatte.Length;
            frame.ImageDescriptor.Width = (short)bmp.Width;
            frame.ImageDescriptor.Height = (short)bmp.Height;
            frame.ColorDepth = 8;          
            frame.GraphicExtension = new GraphicEx();            
            frame.GraphicExtension.DisposalMethod = 0;
            frame.GraphicExtension.TransparencyFlag = true;
            frame.GraphicExtension.TranIndex = 255;           
            return frame;
        }

        byte[] GetColorTable(Color32[] pellatte)
        {
            byte[] buffer = new byte[pellatte.Length*3];
            int index = 0;
            for (int i = 0; i < pellatte.Length; i++)
            {
                buffer[index++] = pellatte[i].Red;
                buffer[index++] = pellatte[i].Green;
                buffer[index++] = pellatte[i].Blue;
            }
            return buffer;
        }
    }
}
