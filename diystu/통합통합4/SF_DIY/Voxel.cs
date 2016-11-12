using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace SF_DIY
{
    public class Voxel
    {
        #region 필드
        private Point3D minPoint3d;
        private Point3D maxPoint3d;
        private Point3DCollection points;
        private Point3D center;
        private double anglex = 0;
        private double anglez = 0;
        private double angley = 0;
        #endregion
        #region 프로퍼티
        [XmlIgnore]
        public Point3D Center
        {
            get
            {
                return center = new Point3D(Position.X + (Width / 2), Position.Y + (Height / 2), Position.Z + (Depth / 2));
            }

            set
            {
                center = new Point3D(Position.X + (Width / 2), Position.Y + (Height / 2), Position.Z + (Depth / 2));
            }

        }
        [XmlIgnore]
        public Point3DCollection Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }
        [XmlIgnore]
        public bool Visible { get; set; }
        [XmlIgnore]
        public double MaxWidth { get; set; }
        [XmlIgnore]
        public double MaxLength { get; set; }
        [XmlIgnore]
        public bool ShowDetail { set; get; }
        [XmlIgnore]
        public string ProductName { get; set; }
        [XmlIgnore]
        public int Num { get; set; }
        [XmlIgnore]
        public string TextureName { get; set; }
        [XmlIgnore]
        public int CompareToList { get; set; }
        [XmlIgnore]
        public BitmapImage BitmapMaterial { get; set; }
        [XmlIgnore]
        public Transform3DGroup Transform3D { get; set; }
        [XmlIgnore]
        public Transform3D OutLineSizeTextTransform3D { get; set; }
        [XmlIgnore]
        public Point3D Position { get; set; }
        [XmlIgnore]
        public int ModelType { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }
        [XmlIgnore]
        public double Width { get; set; }
        [XmlIgnore]
        public double Height { get; set; }
        [XmlIgnore]
        public double Depth { get; set; }
        [XmlIgnore]
        public AxisType AxisType { get; set; }
        [XmlIgnore]
        public ActionType State { get; set; }

        [XmlIgnore]
        public double ZoomScaling { get; set; }

        [XmlIgnore]
        public double AngleX
        {
            get
            {
                return anglex;
            }
            set
            {
                anglex = value;
            }
        }
        [XmlIgnore]
        public double AngleY
        {
            get
            {
                return angley;
            }
            set
            {
                angley = value;
            }
        }
        [XmlIgnore]
        public double AngleZ
        {
            get
            {
                return anglez;
            }
            set
            {
                anglez = value;
            }
        }
        #endregion
        #region 생성자
        public Voxel()
        {
        }
        public Voxel(Point3D p, int modelType, double width, double height, double depth)
        {
            Position = p;
            ModelType = modelType;
            Width = width;
            Height = height;
            Depth = depth;
            //Points = CalculateVertex();//OutLinePoints
        }
        public Voxel(Voxel copy)
        {
            Position = copy.Position;
            ModelType = copy.ModelType;
            Width = copy.Width;
            Height = copy.Height;
            Depth = copy.Depth;
            MaxWidth = copy.MaxWidth;
            MaxLength = copy.MaxLength;
            TextureName = copy.TextureName;
            ProductName = copy.ProductName;
            AngleX = copy.AngleX;
            AngleY = copy.AngleY;
            AngleZ = copy.AngleZ;
            CompareToList = copy.CompareToList;
        }

        #endregion
        #region 메소드
        public Point3DCollection CalculateVertex()
        {
            Point3DCollection p = new Point3DCollection();
            p.Add(new Point3D(this.Position.X, this.Position.Y, this.Position.Z));//1
            p.Add(new Point3D(this.Position.X + Width, this.Position.Y, this.Position.Z));//2

            p.Add(new Point3D(this.Position.X + this.Width, this.Position.Y, this.Position.Z));//2
            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z));//3

            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z));//3
            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z));//4

            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z));//4
            p.Add(new Point3D(Position.X, Position.Y, Position.Z));//1

            p.Add(new Point3D(Position.X, Position.Y, Position.Z + Depth));//5
            p.Add(new Point3D(Position.X + Width, Position.Y, Position.Z + Depth));//6

            p.Add(new Point3D(Position.X + Width, Position.Y, Position.Z + Depth));//6
            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z + Depth));//7

            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z + Depth));//7
            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z + Depth));//8

            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z + Depth));//8
            p.Add(new Point3D(Position.X, Position.Y, Position.Z + Depth));//5

            p.Add(new Point3D(Position.X, Position.Y, Position.Z));//1
            p.Add(new Point3D(Position.X, Position.Y, Position.Z + Depth));//5

            p.Add(new Point3D(Position.X + Width, Position.Y, Position.Z));//2
            p.Add(new Point3D(Position.X + Width, Position.Y, Position.Z + Depth));//6

            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z));//3
            p.Add(new Point3D(Position.X + Width, Position.Y + Height, Position.Z + Depth));//7

            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z));//4
            p.Add(new Point3D(Position.X, Position.Y + Height, Position.Z + Depth));//8

            return p;
        }
        #endregion

        #region XML

        [XmlAttribute("Position")]
        public string XmlPosition
        {
            get { return Position.ToString(); }
            set { Position = Point3D.Parse(value.Replace(';', ',')); }
        }

        [XmlAttribute("MaxWidth")]
        public string XmlMaxWidth
        {
            get { return MaxWidth.ToString(); }
            set { MaxWidth = double.Parse(value); }
        }
        [XmlAttribute("MaxLength")]
        public string XmlMaxLength
        {
            get { return MaxLength.ToString(); }
            set { MaxLength = double.Parse(value); }
        }
        [XmlAttribute("Width")]
        public string XmlWidth
        {
            get { return Width.ToString(); }
            set { Width = double.Parse(value); }
        }
        [XmlAttribute("Height")]
        public string XmlHeight
        {
            get { return Height.ToString(); }
            set { Height = double.Parse(value); }
        }
        [XmlAttribute("Depth")]
        public string XmlDepth
        {
            get { return Depth.ToString(); }
            set { Depth = double.Parse(value); }
        }
        [XmlAttribute("ModelType")]
        public string XmlModelType
        {
            get { return ModelType.ToString(); }
            set { ModelType = int.Parse(value); }
        }
        [XmlAttribute("AngleX")]
        public string XmlAngleX
        {
            get { return AngleX.ToString(); }
            set { AngleX = double.Parse(value); }
        }
        [XmlAttribute("AngleY")]
        public string XmlAngleY
        {
            get { return AngleY.ToString(); }
            set { AngleY = double.Parse(value); }
        }
        [XmlAttribute("AngleZ")]
        public string XmlAngleZ
        {
            get { return AngleZ.ToString(); }
            set { AngleZ = double.Parse(value); }
        }
        [XmlAttribute("TextureName")]
        public string XmlTextureName
        {
            get { return TextureName.ToString(); }
            set { TextureName = value; }
        }
        [XmlAttribute("ProductName")]
        public string XmlProductName
        {
            get { return ProductName.ToString(); }
            set { ProductName = value; }
        }
        [XmlAttribute("CompareToList")]
        public string XmlCompareToList
        {
            get { return CompareToList.ToString(); }
            set { CompareToList = int.Parse(value); }
        }
        [XmlAttribute("Num")]
        public string XmlNum
        {
            get { return Num.ToString(); }
            set { Num = int.Parse(value); }
        }


        #endregion

    }

}
