using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY.Interfaces
{
    interface IUndoRedo
    {
        void Undo(int level);
        void Redo(int level);
    }
    class ChangeRepresentationObject
    {
        public ActionType Action;
        public List<Voxel> StateObject;
    }
    class ChangeRepresentationInteriorObject
    {
        public ActionType Action;
        public List<Wall> WallObject;
        public List<Furniture> FurnitureObject;
        public List<Floor> FloorObject;
    }
    #region enums

    public enum ActionType
    {
        Delete = 0,
        Move = 1,
        Resize = 2,
        Insert = 3,
        Rotate = 4,
        ChangeTexture = 5,
        WallLeftControl = 6,
        WallRightControl = 7,
        FloorRsize = 8,
        TranslateZ = 9,
        ResizeX = 10,
        ResizeY = 11,
        ResizeZ = 12


    }
    public enum AxisType
    {
        defualt = 0,
        xAxis = 1,
        yAxis = 2,
        zAxis = 3
    }

    #endregion
}
