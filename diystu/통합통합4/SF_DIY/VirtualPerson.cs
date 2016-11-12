using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using SF_DIY.Interfaces;

namespace SF_DIY
{
    class VirtualPerson
    {
        public Point3D Position { get; set; }
        public ActionType State { get; internal set; }
        public bool Selected { get; internal set; }
        public Transform3D Transform { get; internal set; }
        public double Angle { get; internal set; }
        public MeshBuilder MeshBuilder { get; internal set; }

        public VirtualPerson(Point3D Position)
        {
            this.Position= Position;
        }
    }
}
