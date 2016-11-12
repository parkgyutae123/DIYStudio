using SF_DIY.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using System.Windows.Media;

namespace SF_DIY
{
    public class Floor
    {
        public readonly int defualtHeight = 25;

        [XmlIgnore]
        public double FHeight { get; set; }
        [XmlIgnore]
        public Color FloorColor { get;  set; }
        [XmlIgnore]
        public Color CeilingColor { get;  set; }
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
        public string RoomName {get;set; }
        [XmlIgnore]
        public Point3D StartPoint { get; set; }
        [XmlIgnore]
        public string FloorTextruePath { get; set; }
        [XmlIgnore]
        public string CeilingTextruePath { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }
        [XmlIgnore]
        public List<Point3D> Vertexs { get; set; }
        [XmlIgnore]
        public int SelectVertexIndex { get; set; }
        [XmlIgnore]
        public ActionType State { get; set; }
        public Point3DCollection GetCeilingVertexs()
        {
            Point3DCollection temp = new Point3DCollection();
            for (int i = 0; i < Vertexs.Count - 1; i++)
            {
                temp.Add((Vertexs[i]));
                temp.Add((Vertexs[i + 1]));
            }
            temp.Add(Vertexs[0]);
            temp.Add(Vertexs[Vertexs.Count - 1]);
            return temp;
        }
        public Point3DCollection GetVertexs()
        {
            Point3DCollection temp = new Point3DCollection();
            for (int i = 0; i < Vertexs.Count - 1; i++)
            {
                temp.Add((Vertexs[i]));
                temp.Add((Vertexs[i + 1]));
            }
            temp.Add(Vertexs[0]);
            temp.Add(Vertexs[Vertexs.Count - 1]);
            return temp;
        }

        public Floor(Point3D startPoint)
        {
            Vertexs = new List<Point3D>(100);
            this.StartPoint = startPoint;
            Index = 0;
            IsShowFloor = true;
            IsShowCeiling = false;
            IsFloorColorUsing = true;
            IsFloorTextureUsing= false;
            IsCeilingColorUsing = false;
            IsCeilingTextureUsing = false;
            FHeight = 0.1;
            RoomName = "Nan";
            FloorTextruePath = "Nan";
            CeilingTextruePath = "Nan";
            FloorColor = Colors.LightGray;
        }
        public Floor()
        {
            Vertexs = new List<Point3D>(100);
        }
        public Floor(Floor floor)
        {
            Vertexs = new List<Point3D>(100);
            foreach (var v in floor.Vertexs)
            {
                Vertexs.Add(new Point3D(v.X,v.Y,v.Z));
            }
            FloorTextruePath = floor.FloorTextruePath;
            CeilingTextruePath = floor.CeilingTextruePath;
            StartPoint = floor.StartPoint;
            Index = floor.Index;
            RoomName = floor.RoomName;
            FHeight = floor.FHeight;
            FloorColor = floor.FloorColor;
            CeilingColor = floor.CeilingColor;

            IsShowCeiling = floor.IsShowCeiling;
            IsShowFloor= floor.IsShowFloor;
            IsCeilingColorUsing = floor.IsCeilingColorUsing;
            IsCeilingTextureUsing = floor.IsCeilingTextureUsing;
            IsFloorColorUsing = floor.IsFloorColorUsing;
            IsFloorTextureUsing = floor.IsFloorTextureUsing;
        }

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
            get { return RoomName.ToString(); }
            set { RoomName = value; }
        }
        [XmlAttribute("FHeight")]
        public string XmlFHeight
        {
            get { return FHeight.ToString(); }
            set { FHeight = double.Parse(value); }
        }
        [XmlAttribute("FloorTextruePath")]
        public string XmlFloorTextruePath
        {
            get { return FloorTextruePath.ToString(); }
            set { FloorTextruePath = value; }
        }
        [XmlAttribute("CeilingTextruePath")]
        public string XmlCeilingTextruePath
        {
            get { return CeilingTextruePath.ToString(); }
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
       
        [XmlAttribute("StartPoint")]
        public string XmlStartPoint
        {
            get { return StartPoint.ToString(); }
            set { StartPoint = Point3D.Parse(value.Replace(';', ',')); }
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
                    Vertexs.Add(Point3D.Parse(value.Replace(';', ',')));
                }
            }
        }
        [XmlAttribute("FloorColor")]
        public string XmlFloorColor
        {
            get
            {
                return FloorColor.ToString();
            }
            set
            {
                var obj = (Color)ColorConverter.ConvertFromString(value);
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
        #endregion
    }
}
