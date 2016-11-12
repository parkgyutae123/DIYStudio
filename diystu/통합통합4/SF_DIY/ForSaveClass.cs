using HelixToolkit.Wpf;
using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace SF_DIY
{
    public class ForSaveClass
    {
        [XmlIgnore]
        public ActionType State { get; set; }
        [XmlIgnore]
        public Point3D StartPoint { get; set; }
        [XmlIgnore]
        public Point3D EndPoint { get; set; }
        [XmlIgnore]
        public double Width { get; set; }
        [XmlIgnore]
        public double Depth { get; set; }
        [XmlIgnore]
        public double Height { get; set; }
        [XmlIgnore]
        public double Angle { get; set; }

        [XmlIgnore]
        public string TextureName { get; set; }
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

        [XmlAttribute("Angle")]
        public string XmlAngle
        {
            get { return Angle.ToString(); }
            set { Angle = double.Parse(value); }
        }
        [XmlAttribute("Side0_TextureName")]
        public string XmlSide0_TextureName
        {
            get
            {
                if(Side0_TextureName==null)
                {
                    return "Nan";
                }
                return Side0_TextureName.ToString();
            }
            set { Side0_TextureName = value; }
        }
        [XmlAttribute("Side1_TextureName")]
        public string XmlSide1_TextureName
        {
            get
            {
                if (Side1_TextureName == null)
                {
                    return "Nan";
                }
                return Side1_TextureName.ToString();
            }
            set { Side1_TextureName = value; }
        }
        [XmlAttribute("Side2_TextureName")]
        public string XmlSide2_TextureName
        {
            get
            {
                if (Side2_TextureName == null)
                {
                    return "Nan";
                }
                return Side2_TextureName.ToString();
            }
            set { Side2_TextureName = value; }
        }
        [XmlAttribute("Side3_TextureName")]
        public string XmlSide3_TextureName
        {
            get
            {
                if (Side3_TextureName == null)
                {
                    return "Nan";
                }
                return Side3_TextureName.ToString();
            }
            set { Side3_TextureName = value; }
        }
        [XmlAttribute("Side4_TextureName")]
        public string XmlSide4_TextureName
        {
            get
            {
                if (Side4_TextureName == null)
                {
                    return "Nan";
                }
                return Side4_TextureName.ToString();
            }
            set { Side4_TextureName = value; }
        }
        [XmlAttribute("Side5_TextureName")]
        public string XmlSide5_TextureName
        {
            get
            {
                if (Side5_TextureName == null)
                {
                    return "Nan";
                }
                return Side5_TextureName.ToString();
            }
            set { Side5_TextureName = value; }
        }

        #endregion

        [XmlIgnore]
        public double ScaleX { get; set; }
        [XmlIgnore]
        public double ScaleY { get; set; }
        [XmlIgnore]
        public double ScaleZ { get; set; }
        [XmlIgnore]
        public string ModelPath { get; set; }
        [XmlIgnore]
        public string FurnitureName { get; set; }
        [XmlIgnore]
        public Model3DGroup Model { get; set; }
        [XmlIgnore]
        public Point3D Position { get; set; }
        [XmlIgnore]
        public double AngleZ { get; set; }




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
                if(FurnitureName ==null)
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

        #endregion




        [XmlIgnore]
        public int Index { get; set; }
        [XmlIgnore]
        public bool IsShowCeiling { get; set; }
        [XmlIgnore]
        public bool IsShowFloor { get; set; }
        [XmlIgnore]
        public bool IsCeilingColorUsing { get; set; }
        [XmlIgnore]
        public bool IsCeilingTextureUsing { get; set; }
        [XmlIgnore]
        public bool IsFloorColorUsing { get; set; }
        [XmlIgnore]
        public bool IsFloorTextureUsing { get; set; }
        [XmlIgnore]
        public string RoomName { get; set; }
        [XmlIgnore]
        public string FloorTextruePath { get; set; }
        [XmlIgnore]
        public string CeilingTextruePath { get; set; }
        [XmlIgnore]
        public List<Point3D> Vertexs { get; set; }

        #region XML

        [XmlAttribute("Index")]
        public string XmlIndex
        {
            get { return Index.ToString(); }
            set { Index = int.Parse(value); }
        }
        [XmlAttribute("RoomName")]
        public string XmlRoomName
        {
            get
            {
                if (RoomName == null)
                {
                    return "Nan";
                }
                return RoomName.ToString();
            }
            set { RoomName = value; }
        }
        [XmlAttribute("FloorTextruePath")]
        public string XmlFloorTextruePath
        {
            get
            {
                if (FloorTextruePath == null)
                {
                    return "Nan";
                }
                return FloorTextruePath.ToString();
            }
            set { FloorTextruePath = value; }
        }
        [XmlAttribute("CeilingTextruePath")]
        public string XmlCeilingTextruePath
        {
            get
            {
                if (CeilingTextruePath == null)
                {
                    return "Nan";
                }
                return CeilingTextruePath.ToString();
            }
            set { CeilingTextruePath = value; }
        }
        [XmlAttribute("IsShowFloor")]
        public string XmlIsShowFloor
        {
            get { return IsShowFloor.ToString(); }
            set { IsShowFloor = bool.Parse(value); }
        }
        [XmlAttribute("IsShowCeiling")]
        public string XmlIsShowCeiling
        {
            get { return IsShowCeiling.ToString(); }
            set { IsShowCeiling = bool.Parse(value); }
        }

        [XmlAttribute("IsCeilingColorUsing")]
        public string XmlIsCeilingColorUsing
        {
            get { return IsCeilingColorUsing.ToString(); }
            set { IsCeilingColorUsing = bool.Parse(value); }
        }
        [XmlAttribute("IsCeilingTextureUsing")]
        public string XmlIsCeilingTextureUsing
        {
            get { return IsCeilingTextureUsing.ToString(); }
            set { IsCeilingTextureUsing = bool.Parse(value); }
        }
        [XmlAttribute("IsFloorColorUsing")]
        public string XmlIsFloorColorUsing
        {
            get { return IsFloorColorUsing.ToString(); }
            set { IsFloorColorUsing = bool.Parse(value); }
        }
        [XmlAttribute("IsFloorTextureUsing")]
        public string XmlIsFloorTextureUsing
        {
            get { return IsFloorTextureUsing.ToString(); }
            set { IsFloorTextureUsing = bool.Parse(value); }
        }
        [XmlAttribute("FirstPoint")]
        public string XmlFirstPoint
        {
            get
            {
                try
                {
                    if (Vertexs[0] != null)
                    {
                        return Vertexs[0].ToString();
                    }
                }
                catch
                {
                    return "Nan";
                }
                return "Nan";
            }
            set
            {
                if (value != "Nan")
                {
                    if(Vertexs == null)
                    {
                        Vertexs = new List<Point3D>();
                    }
                    Vertexs.Add(Point3D.Parse(value.Replace(';', ',')));
                }
            }
        }
        [XmlAttribute("SecondPoint")]
        public string XmlSecondPoint
        {
            get
            {
                try
                {
                    if (Vertexs[1] != null)
                    {
                        return Vertexs[1].ToString();
                    }
                }
                catch
                {
                    return "Nan";
                }
                return "Nan";
            }
            set
            {
                if (value != "Nan")
                {
                    if (Vertexs == null)
                    {
                        Vertexs = new List<Point3D>();
                    }
                    Vertexs.Add(Point3D.Parse(value.Replace(';', ',')));
                }
            }
        }
        [XmlAttribute("ThirdPoint")]
        public string XmlThirdPoint
        {
            get
            {
                try
                {
                    if (Vertexs[2] != null)
                    {
                        return Vertexs[2].ToString();
                    }
                }
                catch
                {
                    return "Nan";
                }
                return "Nan";
            }
            set
            {
                if (value != "Nan")
                {
                    if (Vertexs == null)
                    {
                        Vertexs = new List<Point3D>();
                    }
                    Vertexs.Add(Point3D.Parse(value.Replace(';', ',')));
                }
            }
        }
        [XmlAttribute("FourthPoint")]
        public string XmlFourthPoint
        {
            get
            {
                try
                {
                    if (Vertexs[3] != null)
                    {
                        return Vertexs[3].ToString();
                    }
                }
                catch
                {
                    return "Nan";
                }
                return "Nan";
            }
            set
            {
                if (value != "Nan")
                {
                    if (Vertexs == null)
                    {
                        Vertexs = new List<Point3D>();
                    }
                    Vertexs.Add(Point3D.Parse(value.Replace(';', ',')));
                }
            }
        }
        #endregion

        [XmlIgnore]
        public Color FloorColor { get; set; }
        [XmlIgnore]
        public Color CeilingColor { get; set; }

        [XmlAttribute("FloorColor")]
        public string XmlFloorColor
        {
            get
            {
                return FloorColor.ToString();
            }
            set
            {
                var obj = (Color) ColorConverter.ConvertFromString(value);
                if (obj != null) FloorColor = (Color)obj;

            }
        }
        [XmlAttribute("CeilingColor")]
        public string XmlCeilingColor
        {
            get
            {
                return CeilingColor.ToString();
            }
            set
            {
                var obj = (Color)ColorConverter.ConvertFromString(value);
                if (obj != null) CeilingColor = (Color)obj;

            }
        }


        [XmlIgnore]
        public double FHeight { get; set; }

        [XmlAttribute("FHeight")]
        public string XmlFHeight
        {
            get { return FHeight.ToString(); }
            set { FHeight = double.Parse(value); }
        }
    }
}
