using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DIY
{
    class InteriorUndoRedo : IUndoRedo
    {


        private Stack<ChangeRepresentationInteriorObject> _UndoActionsCollection = new Stack<ChangeRepresentationInteriorObject>();
        private Stack<ChangeRepresentationInteriorObject> _RedoActionsCollection = new Stack<ChangeRepresentationInteriorObject>();
        private InteriorVIewModel vm;

        //생성자
        public InteriorUndoRedo(InteriorVIewModel vm)
        {
            this.vm = vm;
        }

        public event EventHandler EnableDisableUndoRedoFeature;
        //Insert
        public void InsertObjectforUndoRedo(ChangeRepresentationInteriorObject dataobject)
        {
            _UndoActionsCollection.Push(dataobject);
            ClearRedo();
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }
        public void PushforUndoRedo(ChangeRepresentationInteriorObject dataobject)
        {
            _UndoActionsCollection.Push(dataobject);
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }
        public void Redo(int level)
        {
            for (int i = 0; i < level; i++)
            {
                if (_RedoActionsCollection.Count == 0) return;

                ChangeRepresentationInteriorObject RedoObj = _RedoActionsCollection.Pop();
                if (RedoObj.Action == ActionType.Insert)
                {
                    ChangeRepresentationInteriorObject temp = MakeNowInteriorState(vm.Walls, vm.Floors, vm.Furnitures);
                    //ChangeRepresentationInteriorObject temp = MakeNowInteriorState(RedoObj.WallObject, RedoObj.FloorObject, RedoObj.FurnitureObject);
                    UndoPushInUnDoForInsert(temp.WallObject, temp.FloorObject, temp.FurnitureObject);

                    vm.Clear();
                    vm.Walls = RedoObj.WallObject;
                    vm.Floors = RedoObj.FloorObject;
                    vm.Furnitures = RedoObj.FurnitureObject;
                    vm.UpdateAllModel();
                }
            }
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }

        public void Undo(int level)
        {
            for (int i = 0; i < level; i++)
            {
                if (_UndoActionsCollection.Count == 0) return;

                ChangeRepresentationInteriorObject UndoObj = _UndoActionsCollection.Pop();
                if (UndoObj.Action == ActionType.Insert)
                {//추가 동작 상태였다면
                    ChangeRepresentationInteriorObject temp = MakeNowInteriorState(vm.Walls,vm.Floors,vm.Furnitures);
                    //ChangeRepresentationInteriorObject temp = MakeNowInteriorState(UndoObj.WallObject, UndoObj.FloorObject, UndoObj.FurnitureObject);
                    RedoPushInUnDoForInsert(temp.WallObject,temp.FloorObject,temp.FurnitureObject);

                    vm.Clear();
                    vm.Walls = UndoObj.WallObject;
                    vm.Floors = UndoObj.FloorObject;
                    vm.Furnitures = UndoObj.FurnitureObject;
                    vm.UpdateAllModel();
                }
            }
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }
        #region 상태 복사
        private void UndoPushInUnDoForInsert(List<Wall> walls, List<Floor> floors, List<Furniture> furnitures)
        {
            ChangeRepresentationInteriorObject dataObject = new ChangeRepresentationInteriorObject();
            dataObject.Action = ActionType.Insert;
            List<Wall> tempwalls = new List<Wall>();
            List<Floor> tempfloors = new List<Floor>();
            List<Furniture> tempfurnitures = new List<Furniture>();

            if (walls.Count != 0)
            {
                foreach (Wall w in walls)
                {
                    Wall wall = new Wall(w);
                    tempwalls.Add(wall);
                }
            }
            dataObject.WallObject = tempwalls;

            if (floors.Count != 0)
            {
                foreach (Floor f in floors)
                {
                    Floor floor = new Floor(f);
                    tempfloors.Add(floor);
                }
            }
            dataObject.FloorObject = tempfloors;

            if (furnitures.Count != 0)
            {
                foreach (Furniture fur in furnitures)
                {
                    Furniture furniture = new Furniture(fur);
                    tempfurnitures.Add(furniture);
                }
            }
            dataObject.FurnitureObject = tempfurnitures;

            _UndoActionsCollection.Push(dataObject);
        }
        private void RedoPushInUnDoForInsert(List<Wall> walls, List<Floor> floors, List<Furniture> furnitures)
        {
            ChangeRepresentationInteriorObject dataObject = new ChangeRepresentationInteriorObject();
            dataObject.Action = ActionType.Insert;
            List<Wall> tempwalls = new List<Wall>();
            List<Floor> tempfloors = new List<Floor>();
            List<Furniture> tempfurnitures = new List<Furniture>();
            if (walls.Count != 0)
            {
                foreach (Wall w in walls)
                {
                    Wall wall = new Wall(w);
                    tempwalls.Add(wall);
                }
            }
            dataObject.WallObject = tempwalls;

            if (floors.Count != 0)
            {
                foreach (Floor f in floors)
                {
                    Floor floor = new Floor(f);
                    tempfloors.Add(floor);
                }
            }
            dataObject.FloorObject = tempfloors;

            if (furnitures.Count != 0)
            {
                foreach (Furniture fur in furnitures)
                {
                    Furniture furniture = new Furniture(fur);
                    tempfurnitures.Add(furniture);
                }
            }
            dataObject.FurnitureObject = tempfurnitures;

            _RedoActionsCollection.Push(dataObject);
        }
        public ChangeRepresentationInteriorObject MakeNowInteriorState(List<Wall> walls,List<Floor> floors,List<Furniture> furnitures)
        {
            ChangeRepresentationInteriorObject dataObject = new ChangeRepresentationInteriorObject();
            dataObject.Action = ActionType.Insert;
            List<Wall> tempwalls = new List<Wall>();
            List<Floor> tempfloors = new List<Floor>();
            List<Furniture> tempfurnitures = new List<Furniture>();
            if (walls.Count != 0)
            {
                foreach (Wall w in walls)
                {
                    Wall wall = new Wall(w);
                    tempwalls.Add(wall);
                }
            }
            dataObject.WallObject = tempwalls;

            if (floors.Count != 0)
            {
                foreach (Floor f in floors)
                {
                    Floor floor = new Floor(f);
                    tempfloors.Add(floor);
                }
            }
            dataObject.FloorObject = tempfloors;

            if (furnitures.Count != 0)
            {
                foreach (Furniture fur in furnitures)
                {
                    Furniture furniture = new Furniture(fur);
                    tempfurnitures.Add(furniture);
                }
            }
            dataObject.FurnitureObject = tempfurnitures;

            return dataObject;
        }
        #endregion

        public bool IsUndoPossible()
        {
            if (_UndoActionsCollection.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool IsRedoPossible()
        {
            if (_RedoActionsCollection.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearUndo()
        {
            _UndoActionsCollection.Clear();
        }
        public void ClearRedo()
        {
            _RedoActionsCollection.Clear();
        }
    }
}
