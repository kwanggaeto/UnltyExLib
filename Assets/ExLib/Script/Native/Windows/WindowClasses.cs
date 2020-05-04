using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Native.WindowsAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public class Size
    {
        public double Width { set; get; }
        public double Height { set; get; }

        public Size() { }
        public Size(double Width, double Height)
        { this.Width = Width; this.Height = Height; }

        public override string ToString()
        {
            return string.Format("{{ Width = {0}, Height = {1} }}", Width, Height);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public double X { set; get; }
        public double Y { set; get; }

        public Point(double X, double Y)
         : this()
        { this.X = X; this.Y = Y; }

        public override string ToString()
        {
            return string.Format("{{ X = {0}, Y = {1} }}", X, Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left, Top, Right, Bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override string ToString()
        {
            return string.Format("{{ Left = {0}, Top = {1}, Right = {2}, Bottom = {3} }}", Left, Top, Right, Bottom);
        }
    }
}