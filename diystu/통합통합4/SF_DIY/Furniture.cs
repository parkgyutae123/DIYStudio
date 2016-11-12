using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace SF_DIY
{
    public class Furniture
    {

        [XmlIgnore]
        public double Width { get; set; }
        [XmlIgnore]
        public double Height { get; set; }
        [XmlIgnore]
        public double Depth { get; set; }
        [XmlIgnore]
        public double ScaleX { get; set; }
        [XmlIgnore]
        public double ScaleY { get; set; }
        [XmlIgnore]
        public double ScaleZ { get; set; }
        [XmlIgnore]
        public ActionType State { get; set; }
        public bool Selected { get; set; }
        [XmlIgnore]
        public string ModelPath { get; set; }
        [XmlIgnore]
        public string FurnitureName { get; set; }
        [XmlIgnore]
        public Model3DGroup Model { get; set; }
        [XmlIgnore]
        public Transform3DGroup Transform3DGroup { get; set; }
        [XmlIgnore]
        public Point3D Position { get; set; }
        [XmlIgnore]
        public double AngleZ { get; set; }
        [XmlIgnore]
        public Point3D Center
        {
            get
            {
                Rect3D bound = Model.Bounds;
                Point3D center = new Point3D(bound.X + (bound.SizeX / 2), bound.Y + (bound.SizeY / 2), bound.Z + (bound.SizeZ / 2));
                return center;
            }
        }

        [XmlIgnore]
        public Rect3D Bounds { get; set; }

        public Furniture(string name, double sizeX, double sizeY, double sizeZ, Model3DGroup model)
        {
            this.FurnitureName = name;
            this.Width = sizeX;
            this.Height = sizeY;
            this.Depth = sizeZ;
            this.ScaleX = 1; this.ScaleY = 1; this.ScaleZ = 1;
            this.Model = model;
            this.Selected = false;
            this.AngleZ = 0;
            this.Position = new Point3D(0, 0, 0);
            Transform3DGroup = new Transform3DGroup();
        }
        public Furniture(Furniture furniture)
        {
            FurnitureName = furniture.FurnitureName;
            Position = furniture.Position;
            Width = furniture.Width;
            Height = furniture.Height;
            Depth = furniture.Depth;
            ScaleX = furniture.ScaleX;
            ScaleY = furniture.ScaleY;
            ScaleZ = furniture.ScaleZ;
            AngleZ = furniture.AngleZ;
            Transform3DGroup = furniture.Transform3DGroup;
            Model = furniture.Model.CloneCurrentValue();
            Model.Transform = furniture.Model.Transform.CloneCurrentValue();
            ModelPath = furniture.ModelPath;
        }
        public Furniture()
        {
        }

        internal Point3DCollection CalculateVertex()
        {
            Point3DCollection p = new Point3DCollection();
            
            Rect3D bounds = Model.Bounds;

            p.Add(new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ + 0.2));//1
            p.Add(new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ + 0.2));//2

            p.Add(new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ + 0.2));//2
            p.Add(new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ + 0.2));//3

            p.Add(new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ + 0.2));//3
            p.Add(new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ + 0.2));//4

            p.Add(new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ + 0.2));//4
            p.Add(new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ + 0.2));//1

            return p;
        }

        #region XML
        [XmlAttribute("Position")]
        public string XmlPosition
        {
            get { return Position.ToString(); }
            set { Position = Point3D.Parse(value.Replace(';', ',')); }
        }
        [XmlAttribute("Width")]
        public string XmlWidth
        {
            get { return Width.ToString(); }
            set { Width = double.Parse(value); }
        }
        [XmlAttribute("Depth")]
        public string XmlDepth
        {
            get { return Depth.ToString(); }
            set { Depth = double.Parse(value); }
        }
        [XmlAttribute("Height")]
        public string XmlHeight
        {
            get { return Height.ToString(); }
            set { Height = double.Parse(value); }
        }
        [XmlAttribute("AngleZ")]
        public string XmlAngleZ
        {
            get { return AngleZ.ToString(); }
            set { AngleZ = double.Parse(value); }
        }
        [XmlAttribute("ScaleX")]
        public string XmlScaleX
        {
            get { return ScaleX.ToString(); }
            set { ScaleX = double.Parse(value); }
        }
        [XmlAttribute("ScaleY")]
        public string XmlScaleY
        {
            get { return ScaleY.ToString(); }
            set { ScaleY = double.Parse(value); }
        }
        [XmlAttribute("ScaleZ")]
        public string XmlScaleZ
        {
            get { return ScaleZ.ToString(); }
            set { ScaleZ = double.Parse(value); }
        }
        [XmlAttribute("FurnitureName")]
        public string XmlFurnitureName
        {
            get
            {
                if (FurnitureName == null)
                {
                    return "Nan";
                }
                return FurnitureName.ToString();
            }
            set { FurnitureName = value; }
        }
        [XmlAttribute("ModelPath")]
        public string XmlModelPath
        {
            get
            {
                if (ModelPath == null)
                {
                    return "Nan";
                }
                return ModelPath.ToString();
            }
            set { ModelPath = value; }
        }

        public bool Visible { get;  set; }
        public int FurnitureNum { get;  set; }

        #endregion
    }
}
