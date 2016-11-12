using HelixToolkit.Wpf;
using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace SF_DIY
{
    public class Wall
    {
        [XmlIgnore]
        public ActionType State { get; set; }
        [XmlIgnore]
        public Point3D StartPoint { get; set; }
        [XmlIgnore]
        public Point3D EndPoint { get; set; }
        [XmlIgnore]
        public Transform3DGroup Transform3D { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }
        [XmlIgnore]
        public double Width { get; set; }
        [XmlIgnore]
        public double Depth { get; set; }
        [XmlIgnore]
        public double Height { get; set; }
        [XmlIgnore]
        public double Angle { get; set; }
        [XmlIgnore]
        public string Path { get; set; }
        [XmlIgnore]
        public string TextureName { get; set; }
        [XmlIgnore]
        public Rect3D Bounds { get; internal set; }
        [XmlIgnore]
        public MeshBuilder Meshbuild { get; set; }
        [XmlIgnore]
        public Wall ConnectedLeftWall { get; set; }
        [XmlIgnore]
        public Wall ConnectedRightWall { get; set; }
        [XmlIgnore]
        public string Side0_TextureName { get; internal set; }
        [XmlIgnore]
        public string Side1_TextureName { get; internal set; }
        [XmlIgnore]
        public string Side2_TextureName { get; internal set; }
        [XmlIgnore]
        public string Side3_TextureName { get; internal set; }
        [XmlIgnore]
        public string Side4_TextureName { get; internal set; }
        [XmlIgnore]
        public string Side5_TextureName { get; internal set; }
        public Wall(Point3D startp, Point3D endp, double angle)
        {
            StartPoint = startp;
            EndPoint = endp;
            Angle = (int)angle;
            Depth = 25;
            Height = 1.5;
            Path = null;
            Side0_TextureName = "";
            Side1_TextureName = "";
            Side2_TextureName = "";
            Side3_TextureName = "";
            Side4_TextureName = "";
            Side5_TextureName = "";
            
        }
        public Wall(Wall _wall)
        {
            StartPoint = _wall.StartPoint;
            EndPoint = _wall.EndPoint;
            Transform3D = _wall.Transform3D;
            Width = _wall.Width;
            Depth = _wall.Depth;
            Height = _wall.Height;
            Angle = _wall.Angle;
            Meshbuild = _wall.Meshbuild;
            Path = _wall.Path;
            Side0_TextureName = _wall.Side0_TextureName;
            Side1_TextureName = _wall.Side1_TextureName;
            Side2_TextureName = _wall.Side2_TextureName;
            Side3_TextureName = _wall.Side3_TextureName;
            Side4_TextureName = _wall.Side4_TextureName;
            Side5_TextureName = _wall.Side5_TextureName;
        }
        public Wall()
        { }
        internal Point3DCollection CalculateVertex()
        {
            Point3DCollection p = new Point3DCollection();

            //var m = Meshbuild.ToMesh();

            //var bounds = m.Bounds;
            var bounds = this.Bounds;

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
        [XmlAttribute("StartPoint")]
        public string XmlStartPoint
        {
            get { return StartPoint.ToString(); }
            set { StartPoint = Point3D.Parse(value.Replace(';', ',')); }
        }
        [XmlAttribute("EndPoint")]
        public string XmlEndPoint
        {
            get { return EndPoint.ToString(); }
            set { EndPoint = Point3D.Parse(value.Replace(';', ',')); }
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
        [XmlAttribute("Angle")]
        public string XmlAngle
        {
            get { return Angle.ToString(); }
            set { Angle = double.Parse(value); }
        }
        [XmlAttribute("Path")]
        public string XmlPath
        {
            get { return Path.ToString(); }
            set { Path = value; }
        }
        [XmlAttribute("Side0_TextureName")]
        public string XmlSide0_TextureName
        {
            get { return Side0_TextureName.ToString(); }
            set { Side0_TextureName = value; }
        }
        [XmlAttribute("Side1_TextureName")]
        public string XmlSide1_TextureName
        {
            get { return Side1_TextureName.ToString(); }
            set { Side1_TextureName = value; }
        }
        [XmlAttribute("Side2_TextureName")]
        public string XmlSide2_TextureName
        {
            get { return Side2_TextureName.ToString(); }
            set { Side2_TextureName = value; }
        }
        [XmlAttribute("Side3_TextureName")]
        public string XmlSide3_TextureName
        {
            get { return Side3_TextureName.ToString(); }
            set { Side3_TextureName = value; }
        }
        [XmlAttribute("Side4_TextureName")]
        public string XmlSide4_TextureName
        {
            get { return Side4_TextureName.ToString(); }
            set { Side4_TextureName = value; }
        }
        [XmlAttribute("Side5_TextureName")]
        public string XmlSide5_TextureName
        {
            get { return Side5_TextureName.ToString(); }
            set { Side5_TextureName = value; }
        }

        #endregion
    }
}
