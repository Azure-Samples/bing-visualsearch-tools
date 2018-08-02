using System;
using System.Windows;
using VSPing.Models;
using VSPing.Utils;

namespace VSPing.ViewModels
{
    public class BoundingBox : BindableBase
    {

        protected double x;
        public double X { get { return this.x; } set { SetProperty(ref this.x, value); } }

        protected double y;
        public double Y { get { return this.y; } set { SetProperty(ref this.y, value); } }

        protected double w;
        public double Width { get { return this.w; } set { SetProperty(ref this.w, value); } }

        protected double h;
        public double Height { get { return this.h; } set { SetProperty(ref this.h, value); } }

        public virtual void ScaleToImageSize(Size imageSize) { }
    }

    public class QueryBoundingBox : BoundingBox
    {
        public ScaledBox ScaledBox { get; set; }

        public QueryBoundingBox(ScaledBox bb)
        {
            this.ScaledBox = bb;
        }

        public override void ScaleToImageSize(Size imageSize)
        {

            double x0 = this.ScaledBox.cl * imageSize.Width;
            double y0 = this.ScaledBox.ct * imageSize.Height;
            double w0 = (this.ScaledBox.cr - this.ScaledBox.cl) * imageSize.Width;
            double h0 = (this.ScaledBox.cb - this.ScaledBox.ct) * imageSize.Height;

            this.X = x0;
            this.Y = y0;
            this.Width = w0;
            this.Height = h0;
        }

    }

    public class TagsBoundingBox : BoundingBox
    {
        public ImageBoundingBox ImageBoundingBox { get; private set; }
        public string Name { get; private set; }

        public TagsBoundingBox(string Name, ImageBoundingBox imageBoundingBox)
        {
            this.ImageBoundingBox = imageBoundingBox;
            this.Name = Name;
        }

        public override void ScaleToImageSize(Size imageSize)
        {

            double x0 = this.ImageBoundingBox.DisplayRectangle.TopLeft.X * imageSize.Width;
            double y0 = this.ImageBoundingBox.DisplayRectangle.TopLeft.Y * imageSize.Height;
            double w0 = (this.ImageBoundingBox.DisplayRectangle.TopRight.X - this.ImageBoundingBox.DisplayRectangle.TopLeft.X) * imageSize.Width;
            double h0 = (this.ImageBoundingBox.DisplayRectangle.BottomLeft.Y - this.ImageBoundingBox.DisplayRectangle.TopLeft.Y) * imageSize.Height;

            //point sized bounding boxes are sized to be 5px by 5px
            if (w0 == 0) w0 = 5;
            if (h0 == 0) h0 = 5;

            this.X = x0;
            this.Y = y0;
            this.Width = w0;
            this.Height = h0;
        }

    }
}
