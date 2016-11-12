using SF_DIY.Interfaces;
using System;
using System.Collections.Generic;

namespace SF_DIY
{
    class UndoRedo : IUndoRedo
    {

        private Stack<ChangeRepresentationObject> _UndoActionsCollection = new Stack<ChangeRepresentationObject>();
        private Stack<ChangeRepresentationObject> _RedoActionsCollection = new Stack<ChangeRepresentationObject>();
        private MainViewModel vm;

        public UndoRedo(MainViewModel vm)
        {
            this.vm = vm;
        }
        
        public event EventHandler EnableDisableUndoRedoFeature;

        
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

        public void InsertObjectforUndoRedo(ChangeRepresentationObject dataobject)
        {
            _UndoActionsCollection.Push(dataobject);
            if(_UndoActionsCollection.Count == 100)
            {
                _UndoActionsCollection.Clear();
            }
            _RedoActionsCollection.Clear();
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }
        public void PushforUndoRedo(ChangeRepresentationObject dataobject)
        {
            _UndoActionsCollection.Push(dataobject);
            if (_UndoActionsCollection.Count == 100)
            {
                _UndoActionsCollection.Clear();
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

                ChangeRepresentationObject UndoObj = _UndoActionsCollection.Pop();
                if (UndoObj.Action == ActionType.Insert)
                {
                    ChangeRepresentationObject temp = MakeNowVoxelsState(vm.Voxels);
                    RedoPushInUnDoForInsert(temp.StateObject);

                    //추가 동작 상태였다면
                    vm.Clear();
                    vm.Voxels = UndoObj.StateObject;
                    vm.UpdateModel();
                }
            }
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

                ChangeRepresentationObject RedoObj = _RedoActionsCollection.Pop();
                if (RedoObj.Action == ActionType.Insert)
                {
                    ChangeRepresentationObject temp = MakeNowVoxelsState(vm.Voxels);
                    UndoPushInUnDoForInsert(temp.StateObject);

                    //추가 동작 상태였다면
                    vm.Clear();
                    vm.Voxels = RedoObj.StateObject;
                    vm.UpdateModel();
                }
            }
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }
        private void UndoPushInUnDoForInsert(List<Voxel> stateObject)
        {
            ChangeRepresentationObject dataobj = new ChangeRepresentationObject();
            dataobj.Action = ActionType.Insert;

            List<Voxel> temp = new List<Voxel>();
            if (stateObject.Count != 0)
            {
                foreach (Voxel v in stateObject)
                {
					Voxel t = new Voxel(v.Position, v.ModelType, v.Width, v.Height, v.Depth);
					t.Transform3D = v.Transform3D;
					t.OutLineSizeTextTransform3D = v.OutLineSizeTextTransform3D;
					t.Center = v.Center;
                    t.ProductName = v.ProductName;
                    t.CompareToList = v.CompareToList; 
                    t.AngleX = v.AngleX;
                    t.AngleY = v.AngleY;
                    t.AngleZ = v.AngleZ;
                    t.AxisType = v.AxisType;
                    t.TextureName = v.TextureName;
                    t.BitmapMaterial = v.BitmapMaterial;
                    t.ProductName = v.ProductName;
                    t.MaxLength = v.MaxLength;
                    t.MaxWidth = v.MaxWidth;
                    t.Num = v.Num;
                    temp.Add(t);
				}
            }
            dataobj.StateObject = temp;
            _UndoActionsCollection.Push(dataobj);
        }
        private void RedoPushInUnDoForInsert(List<Voxel> stateObject)
        {
            ChangeRepresentationObject dataobj = new ChangeRepresentationObject();
            dataobj.Action = ActionType.Insert;

            List<Voxel> temp = new List<Voxel>();
            if (stateObject.Count != 0)
            {
                foreach (Voxel v in stateObject)
                {
					Voxel t = new Voxel(v.Position, v.ModelType, v.Width, v.Height, v.Depth);
					t.Transform3D = v.Transform3D;
					t.OutLineSizeTextTransform3D = v.OutLineSizeTextTransform3D;
                    t.ProductName = v.ProductName;
                    t.CompareToList = v.CompareToList;
                    t.Center = v.Center;
                    t.AngleX = v.AngleX;
                    t.AngleY = v.AngleY;
                    t.AngleZ = v.AngleZ;
                    t.AxisType = v.AxisType;
                    t.TextureName = v.TextureName;
                    t.BitmapMaterial = v.BitmapMaterial;
                    t.ProductName = v.ProductName;
                    t.MaxLength = v.MaxLength;
                    t.MaxWidth = v.MaxWidth;
                    t.Num = v.Num;
                    temp.Add(t);
                }
            }
            dataobj.StateObject = temp;
            _RedoActionsCollection.Push(dataobj);
        }
        #region UndoHelperFunctions

        public ChangeRepresentationObject MakeNowVoxelsState(List<Voxel> voxels)
        {
            ChangeRepresentationObject dataObject = new ChangeRepresentationObject();
            dataObject.Action = ActionType.Insert;
            List<Voxel> temp = new List<Voxel>();
            if(voxels.Count !=0)
            {
                foreach (Voxel v in voxels)
                {
					Voxel t = new Voxel(v.Position, v.ModelType, v.Width, v.Height, v.Depth);
					t.Transform3D = v.Transform3D;
					t.OutLineSizeTextTransform3D = v.OutLineSizeTextTransform3D;
					t.Center = v.Center;
                    t.ProductName = v.ProductName;
                    t.CompareToList = v.CompareToList;
                    t.AngleX = v.AngleX;
                    t.AngleY = v.AngleY;
                    t.AngleZ = v.AngleZ;
                    t.AxisType = v.AxisType;
                    t.TextureName = v.TextureName;
                    t.BitmapMaterial = v.BitmapMaterial;
                    t.ProductName = v.ProductName;
                    t.MaxLength = v.MaxLength;
                    t.MaxWidth = v.MaxWidth;
                    t.Num = v.Num;
                    temp.Add(t);
				}
            }
            dataObject.StateObject = temp;
            return dataObject;
        }
        public void clearUnRedo()
        {
            _UndoActionsCollection.Clear();
        }

        #endregion

        
    }
}
