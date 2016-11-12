using HelixToolkit.Wpf;
using SF_DIY.Domain;
using SF_DIY.Interfaces;
using SF_DIY.TransitionsHelp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SF_DIY
{
    /// <summary>
    /// InteriorControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InteriorControl : UserControl
    {
        #region 필드

        private MouseAcitonType Viewer2DMouseMode = MouseAcitonType.SelectionMode;
        private InteriorVIewModel vm;
        private Model3D _hitModel;

        private Point3D StartPoint3d;
        private Point3D CurrentPoint3d;
        private Point StartPoint2d;
        private Point CurrentPoint2d;
        private Wall TargetWall = null;
        private Wall previousWall = null;
        private Floor TargetFloor = null;
        private Furniture TargetFurniture = null;
        private VirtualPerson TargetVirtualPersson = null;

        private Point3D Center;
        Transform3DGroup transformGroup = new Transform3DGroup();
        private Visual3D _hitResizeVisual;
        private List<UIElement> TextList;
        private int First;
        private Wall tempWall;
        private Furniture tempFurniture;
        private InteriorUndoRedo undoredo;
        private WallEditWindow Edit;
        private FloorEditWindow floorEditWindow;
        private FurnitureEditWindow furnitureEditWindow;
        private ProjectionCamera previousCamera;
        private Point3D previousPoint;
        private HelpInteriorSwitcher hp;

        #endregion

        #region Constructor
        public InteriorControl()
        {
            InitializeComponent();
            vm = new InteriorVIewModel(DesignerPane, viewportTop, viewport);
            TextList = new List<UIElement>();
            this.DataContext = vm;
            undoredo = new InteriorUndoRedo(vm);
            InteriorVIewModel.UndoRedoHandler += ActionUndoRedo;
            //도면 마우스 이벤트
            viewportTop.PreviewMouseLeftButtonDown += ViewportTop_PreviewMouseLeftButtonDown;
            viewportTop.MouseMove += ViewportTop_MouseMove;
            viewportTop.PreviewMouseLeftButtonUp += ViewportTop_PreviewMouseLeftButtonUp;
            viewportTop.PreviewMouseDoubleClick += ViewportTop_PreviewMouseDoubleClick;

            previousCamera = MainCamera;
            //3d 뷰어 마우스 이벤트

            viewport.PreviewMouseWheel += Viewport_PreviewMouseWheel;
            viewport.MouseDown += Viewport_PreviewMouseDown;
            ReleaseViewportMouseEvent();

            //키 이벤트
            this.PreviewKeyDown += InteriorControl_PreviewKeyDown;

            viewport.CameraChanged += Viewport_CameraChanged;

            //마우스 사각형 선택
            viewportTop.InputBindings.Add(new MouseBinding(vm.RectangleSelectionTopCommand, new MouseGesture(MouseAction.LeftClick)));
        }

        private void Viewport_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if(Viewer2DMouseMode == MouseAcitonType.VirtualVisit)
            //{
            //    e.Handled = true;
            //    int delta = e.Delta;
            //    if(e.Delta>0)//확대
            //    {
            //        MainCamera.Position = new Point3D(MainCamera.Position.X + delta, MainCamera.Position.Y - delta, MainCamera.Position.Z);
            //    }
            //    else //축소
            //    {
            //        MainCamera.Position = new Point3D(MainCamera.Position.X - delta, MainCamera.Position.Y + delta, MainCamera.Position.Z);
            //    }
            //}
        }

        private void Viewport_CameraChanged(object sender, RoutedEventArgs e)
        {
            if(viewport.Camera.UpDirection.Z <0.05)
            {
                viewport.Camera.UpDirection = new Vector3D(viewport.Camera.UpDirection.X, viewport.Camera.UpDirection.Y, 0.05);
            }
            if(viewport.Camera.Position.Z < 0 )
            {
                viewport.Camera.Position = new Point3D(viewport.Camera.Position.X, viewport.Camera.Position.Y, 2);
                viewport.Camera.LookDirection = new Vector3D(viewport.Camera.LookDirection.X, viewport.Camera.LookDirection.Y, -2);
            }
            if(TargetVirtualPersson != null)
            {
                if(viewport.CameraMode == CameraMode.WalkAround)
                {

                    MainCamera.Position = new Point3D(MainCamera.Position.X, MainCamera.Position.Y, 12);
                    TargetVirtualPersson.Position = viewport.Camera.Position;
                    TargetVirtualPersson.Transform = viewport.Camera.Transform;
                    TargetVirtualPersson.Angle = -(GetAngle1( MainCamera.LookDirection.X, MainCamera.LookDirection.Y, MainCamera.Position.X, MainCamera.Position.Y)-180);
                    vm.UpdateVisualPersonModel();
                    
                }
            }
        }

        private Vector3D CoercePosition(Point3D position)
        {
            if (previousPoint != null)
            {
                var delta = position - this.previousPoint;
                this.previousPoint = position;
                return delta;
            }
            this.previousPoint = position;
            return new Vector3D(0, 0, 0);
        }

        private void SetViewportMouseEvent()
        {
            viewport.MouseDown += Viewport_PreviewMouseDown;
            
        }
        private void ReleaseViewportMouseEvent()
        {
            viewport.IsPanEnabled = true;
            viewport.IsMoveEnabled = true;
            viewport.IsRotationEnabled = true;
            ViewTypeListBox.SelectedIndex = 0;
        }
        private void ActionUndoRedo()
        {
            PushUndoRedoState(0);
        }
        
        #endregion

        #region 벽지 선택
        private void Viewport_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint2d = e.GetPosition(viewport);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                viewport.CaptureMouse();
            }
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                TargetWall = null;
                var _hitModel = FindSourceToHViewport(viewport, StartPoint2d);
                if (_hitModel == null) return;
                var index = vm.GetWallMaterial(_hitModel); //벽 클릭했을때
                if (index == 9) return;
                TargetWall = vm.GetWall(_hitModel);
                if(TargetWall!=null)
                {
                    switch (index)
                    {
                        case 0: TargetWall.Side0_TextureName = vm.SelectedTextureItem.ImgName; break;
                        case 1: TargetWall.Side1_TextureName = vm.SelectedTextureItem.ImgName; break;
                        case 2: TargetWall.Side2_TextureName = vm.SelectedTextureItem.ImgName; break;
                        case 3: TargetWall.Side3_TextureName = vm.SelectedTextureItem.ImgName; break;
                        case 4: TargetWall.Side4_TextureName = vm.SelectedTextureItem.ImgName; break;
                        case 5: TargetWall.Side5_TextureName = vm.SelectedTextureItem.ImgName; break;
                        default:
                            break;
                    }

                    TargetWall = null;
                }
                vm.UpdateWallModel();

            }
        }

        #endregion

        #region 키보드 이벤트
        private void InteriorControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                CrateStopEsc();
                PushUndoRedoState(1);
                return;
            }
            if (e.Key == Key.Delete)
            {
                List<Wall> wallremovelist = new List<Wall>();
                List<Floor> floorremovelist = new List<Floor>();
                List<Furniture> furnitureremovelist = new List<Furniture>();
                foreach (Wall w in vm.Walls)
                {
                    if (w.Selected)
                        wallremovelist.Add(w);
                }
                foreach (Floor f in vm.Floors)
                {
                    if (f.Selected)
                        floorremovelist.Add(f);
                }
                foreach (Furniture fur in vm.Furnitures)
                {
                    if (fur.Selected)
                        furnitureremovelist.Add(fur);
                }
                if (wallremovelist.Count != 0 || floorremovelist.Count != 0 || furnitureremovelist.Count != 0)
                {
                    PushUndoRedoState(1);
                }
                foreach (Wall w in wallremovelist)
                {
                    vm.Walls.Remove(w);
                }
                foreach (Floor f in floorremovelist)
                {
                    vm.Floors.Remove(f);
                }
                foreach (Furniture fur in furnitureremovelist)
                {
                    vm.Furnitures.Remove(fur);
                }
                TargetRelease();
                vm.UpdateAllModel();

                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                CrateStopEsc();
                MouseModeChange(MouseAcitonType.SelectionMode);
                viewportTop.Cursor = Cursors.Arrow;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            {
                CrateStopEsc();
                MouseModeChange(MouseAcitonType.WallCreateMode);
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                CrateStopEsc();
                MouseModeChange(MouseAcitonType.RoomCreateMode);
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                vm.Copy();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                vm.Paste();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                vm.Paste();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                undoredo.Undo(1);

                if (viewport.CameraMode != CameraMode.WalkAround)
                    SetViewport3DCameraPosition();
                vm.UpdateAllModel();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                undoredo.Redo(1);

                if (viewport.CameraMode != CameraMode.WalkAround)
                    SetViewport3DCameraPosition();
                vm.UpdateAllModel();
                return;
            }
            if (viewport.CameraMode == CameraMode.WalkAround)
            {
                e.Handled = false;
                return;
            }
        }
        
        #endregion

        #region 메뉴 클릭
        private void MouseModeChanged_Event(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem btn = sender as ListBoxItem;
            switch (btn.Name)
            {
                case "btn_Selection": MouseModeChange(MouseAcitonType.SelectionMode); break;
                case "btn_Wall": MouseModeChange(MouseAcitonType.WallCreateMode); break;
                case "btn_Room": MouseModeChange(MouseAcitonType.RoomCreateMode); break;
                case "btn_VirtualVisit": MouseModeChange(MouseAcitonType.VirtualVisit); break;
                case "btn_TopView": MouseModeChange(MouseAcitonType.ViewTop); break;
                //case "btn_MaterialSelect":MouseModeChange(MouseAcitonType.MaterialSelectMode); break;
                default:
                    break;
            }
        }
        private void MouseModeChange(MouseAcitonType SelectedIndex)
        {
            IconReset();
            switch (SelectedIndex)
            {
                case MouseAcitonType.SelectionMode: MouseModeListBox.SelectedIndex = 0; 
                    CrateStopEsc(); Viewer2DMouseMode = MouseAcitonType.SelectionMode; viewportTop.Cursor = Cursors.Arrow; SelectionIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CursorDefault; TargetRelease(); 
                     break;

                case MouseAcitonType.WallCreateMode: MouseModeListBox.SelectedIndex = 1; 
                    Viewer2DMouseMode = MouseAcitonType.WallCreateMode; viewportTop.Cursor = Cursors.Cross; TargetRelease(); vm.SelectRelease();
                    viewport.Camera = previousCamera;
                    break;

                case MouseAcitonType.RoomCreateMode: MouseModeListBox.SelectedIndex = 2; 
                    Viewer2DMouseMode = MouseAcitonType.RoomCreateMode; TargetRelease(); vm.SelectRelease();
                    viewport.Camera = previousCamera;
                    break;

                case MouseAcitonType.ViewTop:
                    {
                        ViewTypeListBox.SelectedIndex = 0; TargetRelease(); vm.SelectRelease(); viewport.CameraMode = CameraMode.Inspect; viewport.Camera = previousCamera;
                        MainCamera.FieldOfView = 45; MainCamera.NearPlaneDistance = 15; Viewer2DMouseMode = MouseAcitonType.SelectionMode;
                        RemoveVirtualPerson();
                        break;
                    }
                case MouseAcitonType.VirtualVisit:
                    {
                        ViewTypeListBox.SelectedIndex = 1; MouseModeListBox.SelectedIndex = -1;
                        Viewer2DMouseMode = MouseAcitonType.VirtualVisit; viewportTop.Cursor = Cursors.Hand; TargetRelease(); vm.SelectRelease(); viewport.CameraMode = CameraMode.WalkAround;
                        break;
                    }
                
                default:
                    break;
            }
        }
        private void TargetRelease()
        {
            TargetWall = null;
            TargetFloor = null;
            TargetFurniture = null;
            TargetVirtualPersson = null;
        }
        private void IconReset()
        {
            SelectionIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CursorDefaultOutline;
        }
        private void SaveButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.FileExport();
        }
        private void NewButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("현재 만들어진 내용을 지웁니다.", "새창", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                vm.Walls.Clear();
                vm.Floors.Clear();
                vm.Furnitures.Clear();
                vm.VirtualPersonList.Clear();
                vm.UpdateAllModel();
            }
        }
        private void OpenButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.FileOpen();
        }
        private void CopyButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.Copy();
        }
        private void CutButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.Cut();
        }
        private void PasteButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.Paste();
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if(hp==null)
            {
                hp = new HelpInteriorSwitcher();
            }
            hp.Show();
            hp.Closed += Hp_Closed;
        }
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var fur in vm.Furnitures)
            {
                if(fur.Selected)
                {
                    fur.AngleZ += 45;
                    if (fur.AngleZ >= 360)
                    {
                        fur.AngleZ = 45;
                        return;
                    }
                }
            }
            vm.UpdateFurnitureModel();
        }
        private void Hp_Closed(object sender, EventArgs e)
        {
            hp = null;
        }
        #endregion

        #region 메소드

        //AllMove
        private void AllMove()
        {
            CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
            Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
            foreach (var w in vm.Walls)
            {
                if (w.Selected == true)
                {
                    Point3D startpoint = w.StartPoint;
                    Point3D endpoint = w.EndPoint;
                    startpoint.X += -Delta.X;
                    startpoint.Y += -Delta.Y;
                    endpoint.X += -Delta.X;
                    endpoint.Y += -Delta.Y;
                    w.StartPoint = startpoint;
                    w.EndPoint = endpoint;
                }
            }
            foreach (var floor in vm.Floors)
            {
                if (floor.Selected)
                {
                    for (int i = 0; i < floor.Vertexs.Count; i++)
                    {
                        Point3D p = floor.Vertexs[i];
                        p.X += -Delta.X;
                        p.Y += -Delta.Y;
                        floor.Vertexs[i] = p;
                    }
                }
            }
            foreach (var fur in vm.Furnitures)
            {
                if (fur.Selected)
                {
                    Point3D p = fur.Position;
                    p.X += -Delta.X;
                    p.Y += -Delta.Y;
                    fur.Position = p;
                }
            }
            StartPoint2d = CurrentPoint2d;
            StartPoint3d = CurrentPoint3d;

            vm.UpdateWallModel();
            vm.UpdateFloorModel();
            vm.UpdateFurnitureModel();

            if (viewport.CameraMode != CameraMode.WalkAround)
                SetViewport3DCameraPosition();
        }
        private void RemoveVirtualPerson()
        {
            vm.VirtualPersonModel.Children.Clear();
            vm.ModelToVirtualPerson.Clear();
            vm.VirtualPersonList.Clear();
            vm.UpdateVisualPersonModel();

            if (viewport.CameraMode != CameraMode.WalkAround)
                SetViewport3DCameraPosition();
            
        }
        private double GetRad(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            return rad;
        }
        private double GetAngle(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            double degree = (rad * 180) / Math.PI;
            return degree-90;
        }
        private double GetAngle1(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            double degree = (rad * 180) / Math.PI;
            return degree;
        }
        private Point3D Get2DtoRayPoint3D(Viewport3D viewport, Point p)
        {
            var ray = Viewport3DHelper.Point2DtoRay3D(viewport, p);
            if (ray != null)
            {
                var pi = ray.PlaneIntersection(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
                if (pi.HasValue)
                {
                    var pRound = new Point3D((pi.Value.X), (pi.Value.Y), 0);
                    return pRound;
                }
            }
            return new Point3D(0, 0, 0);
        }
        private void SetViewport3DCameraPosition()
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var w in vm.Walls)
            {
                if (w.StartPoint.X < w.EndPoint.X)
                {
                    if (w.StartPoint.X < minX)
                    {
                        minX = w.StartPoint.X;
                        minY = w.StartPoint.Y;
                    }
                    if (w.EndPoint.X > maxX)
                    {
                        maxX = w.EndPoint.X;
                        maxY = w.EndPoint.Y;
                    }
                }
                else if (w.StartPoint.X > w.EndPoint.X)
                {
                    if (w.EndPoint.X < minX)
                    {
                        minX = w.EndPoint.X;
                        minY = w.EndPoint.Y;
                    }
                    if (w.StartPoint.X > maxX)
                    {
                        maxX = w.StartPoint.X;
                        maxY = w.StartPoint.Y;
                    }
                }
                if (w.StartPoint.Y > w.EndPoint.Y)
                {
                    if (w.StartPoint.Y > maxY)
                    {
                        maxY = w.StartPoint.Y;
                    }
                    if (w.EndPoint.Y < minY)
                    {
                        minY = w.EndPoint.Y;
                    }
                }
                else
                {
                    if (w.EndPoint.Y > maxY)
                    {
                        maxY = w.EndPoint.Y;
                    }
                    if (w.StartPoint.Y < minY)
                    {
                        minY = w.StartPoint.Y;
                    }
                }
            }
            double abx = (maxX + minX) / 2;
            double aby = (maxY + minY) / 2;

            Point3D p = new Point3D(abx, aby, 0);
            MainCamera.LookAt(p, 0);

            Center = p;
        }
        private bool WallFocusTest()
        {
            _hitResizeVisual = FindWallResizeVisualToHViewport(viewportTop, CurrentPoint2d, TargetWall);
            _hitModel = FindSourceToHViewport(viewportTop, CurrentPoint2d);

            if (_hitResizeVisual != null)
            {
                viewportTop.Cursor = Cursors.SizeAll;
                return true;
            }
            if (_hitModel != null)
            {
                Wall twall = vm.GetWall(_hitModel);
                if (twall != null && twall.Selected)
                    viewportTop.Cursor = Cursors.ScrollAll;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool FloorFocusTest()
        {
            _hitResizeVisual = FindFloorResizeVisualToHViewport(viewportTop, CurrentPoint2d, TargetFloor);
            if (_hitResizeVisual != null)
            {
                string index = _hitResizeVisual.GetName();
                TargetFloor.SelectVertexIndex = int.Parse(index);
                viewportTop.Cursor = Cursors.SizeAll;
                return true;
            }
            RayHitTestResult ray = FindFloorSourceToHViewport(viewportTop, CurrentPoint2d);
            if (ray == null) return false;
            RayMeshGeometry3DHitTestResult rayMeshResult = ray as RayMeshGeometry3DHitTestResult;
            if (rayMeshResult == null) return false;
            GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
            if (hitgeo == null) return false;
            Floor floor = vm.GetFloor(hitgeo);
            if (floor != null && floor.Selected)
            {
                viewportTop.Cursor = Cursors.ScrollAll;
                return true;
            }
            return false;
        }
        private bool FurnitureFocusTest()
        {
            _hitResizeVisual = FindFurnitureControlVisualToHViewport(viewportTop, CurrentPoint2d, TargetFurniture);
            if (_hitResizeVisual != null)
            {
                if (TargetFurniture.State == ActionType.Rotate)
                {
                    viewportTop.Cursor = Cursors.Cross;
                }
                if (TargetFurniture.State == ActionType.TranslateZ)
                {
                    viewportTop.Cursor = Cursors.ScrollN;
                }
                if (TargetFurniture.State == ActionType.Resize)
                {
                    viewportTop.Cursor = Cursors.SizeNWSE;
                }
                if (TargetFurniture.State == ActionType.ResizeZ)
                {
                    viewportTop.Cursor = Cursors.SizeNS;
                }
                return true;
            }

            _hitModel = FindSourceToHViewport(viewportTop, CurrentPoint2d);
            if (_hitModel == null) return false;
            Furniture fur = vm.GetFurniture(_hitModel);
            if (fur != null && fur.Selected)
            {
                viewportTop.Cursor = Cursors.ScrollAll;
                return true;
            }
            return false;
        }
        private void CrateStopEsc()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            if (TargetWall != null)
            {
                vm.WallsRemove(TargetWall);
            }

            if(TargetFloor!=null)
            {
                TargetFloor.Selected = false;
            }
            TargetWall = null;
            TargetFloor = null;
            TargetFurniture = null;
            vm.UpdateAllModel();
        }
        private void PushUndoRedoState(int v)
        {
            switch (v)
            {
                case 0:
                    {
                        ChangeRepresentationInteriorObject cro = undoredo.MakeNowInteriorState(vm.Walls, vm.Floors, vm.Furnitures);
                        undoredo.InsertObjectforUndoRedo(cro);
                    }
                    break;
                case 1:
                    {
                        ChangeRepresentationInteriorObject cro = undoredo.MakeNowInteriorState(vm.Walls, vm.Floors, vm.Furnitures);
                        undoredo.PushforUndoRedo(cro);
                    }
                    break;

                default:
                    break;
            }
        }

        #region Wall Tracking 메소드
        private void ConnectedWallTracking()
        {
            if (TargetWall.ConnectedRightWall != null)
            {
                double distance1 = TargetWall.ConnectedRightWall.EndPoint.DistanceTo(TargetWall.EndPoint);
                double angle1 = GetAngle(TargetWall.EndPoint.X, TargetWall.EndPoint.Y, TargetWall.ConnectedRightWall.EndPoint.X, TargetWall.ConnectedRightWall.EndPoint.Y);
                TargetWall.ConnectedRightWall.StartPoint = TargetWall.EndPoint;
                TargetWall.ConnectedRightWall.Width = distance1;
                TargetWall.ConnectedRightWall.Angle = -angle1;
            }
            if (TargetWall.ConnectedLeftWall != null)
            {
                double distance1 = TargetWall.ConnectedLeftWall.StartPoint.DistanceTo(TargetWall.StartPoint);
                double angle1 = GetAngle(TargetWall.ConnectedLeftWall.StartPoint.X, TargetWall.ConnectedLeftWall.StartPoint.Y, TargetWall.StartPoint.X, TargetWall.StartPoint.Y);
                TargetWall.ConnectedLeftWall.EndPoint = TargetWall.StartPoint;
                TargetWall.ConnectedLeftWall.Width = distance1;
                TargetWall.ConnectedLeftWall.Angle = -angle1;
            }
        }
        private void ConnectedLeftWallTracking()
        {
            if (TargetWall.ConnectedLeftWall != null)
            {
                double distance1 = TargetWall.ConnectedLeftWall.StartPoint.DistanceTo(CurrentPoint3d);
                double angle1 = GetAngle(TargetWall.ConnectedLeftWall.StartPoint.X, TargetWall.ConnectedLeftWall.StartPoint.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                TargetWall.ConnectedLeftWall.EndPoint = CurrentPoint3d;
                TargetWall.ConnectedLeftWall.Width = distance1;
                TargetWall.ConnectedLeftWall.Angle = -angle1;
            }
        }
        private void ConnectedRightWallTracking()
        {
            if (TargetWall.ConnectedRightWall != null)
            {
                double distance1 = TargetWall.ConnectedRightWall.EndPoint.DistanceTo(CurrentPoint3d);
                double angle1 = GetAngle(CurrentPoint3d.X, CurrentPoint3d.Y, TargetWall.ConnectedRightWall.EndPoint.X, TargetWall.ConnectedRightWall.EndPoint.Y);
                TargetWall.ConnectedRightWall.StartPoint = TargetWall.EndPoint;
                TargetWall.ConnectedRightWall.Width = distance1;
                TargetWall.ConnectedRightWall.Angle = -angle1;
            }
        }
        #endregion

        #endregion

        #region HitTest
        private Model3D FindSourceToHViewport(HelixViewport3D viewport, Point p)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                
                foreach (var h in hits)
                {
                    return h.Model;
                }
                return null;
            }

            catch (Exception e)
            {
                return null;
            }
        }
        private MeshGeometry3D FindWallMeshToHViewport(HelixViewport3D viewport, Point p)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                foreach (var h in hits)
                {
                    if (h.Mesh!= null)
                    {
                        MeshGeometry3D mesh = h.Mesh;
                        return h.Mesh;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        private RayHitTestResult FindFloorSourceToHViewport(HelixViewport3D viewport, Point p)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                foreach (var h in hits)
                {
                    return h.RayHit;
                }
                return null;
            }

            catch
            {
                return null;
            }
        }
        private Visual3D FindWallResizeVisualToHViewport(HelixViewport3D viewport, Point p, Wall w)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                foreach (var h in hits)
                {
                    foreach (var v in vm.LeftControllerVisuals)
                    {
                        if (v.Equals(h.Visual) && h.Visual.GetType() == typeof(RectangleVisual3D))
                        {
                            w.State = Interfaces.ActionType.WallLeftControl;
                            return h.Visual;
                        }
                    }
                    foreach (var v in vm.RightControllerVisuals)
                    {
                        if (v.Equals(h.Visual) && h.Visual.GetType() == typeof(RectangleVisual3D))
                        {
                            w.State = Interfaces.ActionType.WallRightControl;
                            return h.Visual;
                        }
                    }
                }
                return null;
            }

            catch 
            {
                return null;
            }
        }
        private Visual3D FindFloorResizeVisualToHViewport(HelixViewport3D viewport, Point p, Floor targetFloor)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);
                foreach (var h in hits)
                {
                    foreach (var v in vm.FloorControllerVisuals)
                    {
                        if (v.Equals(h.Visual) && h.Visual.GetType() == typeof(RectangleVisual3D) && v.GetName().Equals(h.Visual.GetName()))
                        {
                            targetFloor.State = Interfaces.ActionType.FloorRsize;
                            return h.Visual;
                        }
                    }
                }
                return null;
            }

            catch
            {
                return null;
            }
        }
        private Visual3D FindFurnitureControlVisualToHViewport(HelixViewport3D viewportTop, Point startPoint2d, Furniture targetFurniture)
        {
            try
            {
                var hits = Viewport3DHelper.FindHits(viewportTop.Viewport, startPoint2d);
                foreach (var h in hits)
                {
                    if (h.Visual.GetType() != typeof(RectangleVisual3D)) continue;
                    foreach (var v in vm.FurnitureRotateControllerVisuals)
                    {
                        if (v.Equals(h.Visual))
                        {
                            targetFurniture.State = Interfaces.ActionType.Rotate;
                            return h.Visual;
                        }
                    }
                    foreach (var v in vm.FurnitureLocationZControllerVisuals)
                    {
                        if (v.Equals(h.Visual))
                        {
                            targetFurniture.State = Interfaces.ActionType.TranslateZ;
                            return h.Visual;
                        }
                    }
                    foreach (var v in vm.FurnitureWidthHightControllerVisuals)
                    {
                        if (v.Equals(h.Visual))
                        {
                            targetFurniture.State = Interfaces.ActionType.Resize;
                            return h.Visual;
                        }
                    }
                    foreach (var v in vm.FurnitureDepthControllerVisuals)
                    {
                        if (v.Equals(h.Visual))
                        {
                            targetFurniture.State = Interfaces.ActionType.ResizeZ;
                            return h.Visual;
                        }
                    }
                }
                return null;
            }

            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region 도면 마우스 이벤트
        //Down
        private void ViewportTop_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint2d = e.GetPosition(viewportTop);
            StartPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, StartPoint2d);
            if (vm.DragArrangement)
            {
                viewportTop.InputBindings.Clear();
                vm.DragArrangement = false;
                vm.Furnitures.Remove(vm.DragArrangementFurnitrue);
                viewportTop.InputBindings.Add(new MouseBinding(vm.RectangleSelectionTopCommand, new MouseGesture(MouseAction.LeftClick)));
            }

            #region 벽생성 모드일때

            if (Viewer2DMouseMode == MouseAcitonType.WallCreateMode)
            {
                viewportTop.InputBindings.Clear();
                PushUndoRedoState(0);
                previousWall = null;

                if (Keyboard.IsKeyDown(Key.Space))
                {
                    if(TargetWall!=null)
                    {
                        previousWall = TargetWall;
                        TargetWall = vm.CreateWallVoxel(previousWall.EndPoint, previousWall.EndPoint, 0);
                    }
                    else
                    {
                        TargetWall = vm.CreateWallVoxel(StartPoint3d, StartPoint3d, 0);
                    }
                }
                else
                {
                    if (TargetWall != null)
                    {
                        previousWall = TargetWall;
                        TargetWall = null;
                    }
                    TargetWall = vm.CreateWallVoxel(StartPoint3d, StartPoint3d, 0);
                }
                if (previousWall != null)
                {
                    TargetWall.ConnectedLeftWall = previousWall;
                    previousWall.ConnectedRightWall = TargetWall;

                }
                if (vm.Walls.Count > 1)
                {
                    if(viewport.CameraMode != CameraMode.WalkAround)
                    SetViewport3DCameraPosition();
                }
            }
            #endregion

            #region 선택모드일때
            if (Viewer2DMouseMode == MouseAcitonType.SelectionMode)
            {
                //선택된 개체 해제;

                #region 벽 리사이즈 박스 히트 테스트
                if (TargetWall != null)
                {
                    _hitResizeVisual = null;
                    // Wall resize visual Type init
                    _hitResizeVisual = FindWallResizeVisualToHViewport(viewportTop, StartPoint2d, TargetWall);
                    if (_hitResizeVisual != null)
                    {
                        viewportTop.CaptureMouse();
                        return;
                    }
                }
                #endregion
                #region 바닥 리사이즈 박스 히트 테스트
                if (TargetFloor != null)
                {
                    _hitResizeVisual = null;
                    // Floor resize visual Type init
                    _hitResizeVisual = FindFloorResizeVisualToHViewport(viewportTop, StartPoint2d, TargetFloor);
                    if (_hitResizeVisual != null)
                    {
                        viewportTop.CaptureMouse();
                        return;
                    }
                }
                #endregion
                #region 가구 컨트롤 박스 히트 테스트
                if (TargetFurniture != null)
                {
                    _hitResizeVisual = null;
                    _hitResizeVisual = FindFurnitureControlVisualToHViewport(viewportTop, StartPoint2d, TargetFurniture);
                    if (_hitResizeVisual != null)
                    {
                        viewportTop.CaptureMouse();
                        return;
                    }
                }
                #endregion

                //모델 히트 테스트 시작.
                _hitModel = FindSourceToHViewport(viewportTop, StartPoint2d);

                //빈곳을 클릭했을때
                if (_hitModel == null)
                {
                    vm.SelectRelease();
                    TargetRelease();
                    return;
                }

                TargetRelease();

                #region 벽 히트테스트
                TargetWall = vm.GetWall(_hitModel); //벽 클릭했을때
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    vm.SelectRelease();
                }
                if (TargetWall != null)
                {
                    TargetWall.State = Interfaces.ActionType.Move;
                    TargetWall.Selected = TargetWall.Selected == false ? true : true;
                    viewportTop.CaptureMouse();
                    vm.UpdateWallModel();
                    if(e.ClickCount < 2)
                    {
                        PushUndoRedoState(1);
                    }
                    return;
                }
                #endregion

                #region 가구 히트테스트
                TargetFurniture = vm.GetFurniture(_hitModel);
                if (TargetFurniture != null)
                {
                    TargetFurniture.State = ActionType.Move;
                    TargetFurniture.Selected = true;
                    viewportTop.Cursor = Cursors.ScrollAll;
                    viewportTop.CaptureMouse();
                    vm.UpdateFurnitureModel();
                    if (e.ClickCount < 2)
                    {
                        PushUndoRedoState(1);
                    }
                    return;
                }
                #endregion

                #region 바닥 히트테스트
                //바닥천장 클릭 했을때 hittest
                RayHitTestResult ray = FindFloorSourceToHViewport(viewportTop, StartPoint2d);
                if (ray == null) return;
                RayMeshGeometry3DHitTestResult rayMeshResult = ray as RayMeshGeometry3DHitTestResult;
                if (rayMeshResult == null) return;
                GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                if (hitgeo == null) return;
                TargetFloor = vm.GetFloor(hitgeo);
                //바닥 클릭했다면
                if (TargetFloor != null)
                {
                    TargetFloor.Selected = true;
                    TargetFloor.State = ActionType.Move;
                    viewportTop.CaptureMouse();
                    vm.UpdateFloorModel();
                    if (e.ClickCount < 2)
                    {
                        PushUndoRedoState(1);
                    }
                    return;
                }
                #endregion

                #region 가상카메라 히트테스트
                TargetVirtualPersson = vm.GetVirtualPerson(_hitModel); //벽 클릭했을때
                if (TargetVirtualPersson != null)
                {
                    TargetVirtualPersson.State = Interfaces.ActionType.Move;
                    TargetVirtualPersson.Selected = true;
                    viewportTop.CaptureMouse();
                    vm.UpdateVisualPersonModel();
                    return;
                }
                #endregion
            }
            #endregion

            #region 방생성 모드일때
            if (Viewer2DMouseMode == MouseAcitonType.RoomCreateMode)
            {
                PushUndoRedoState(0);
                viewportTop.InputBindings.Clear();
                if (TargetFloor != null && TargetFloor.Index == 4)//폴리곤 완성후 더 클릭시
                {
                    TargetFloor.Vertexs.RemoveAt(TargetFloor.Vertexs.Count - 1);
                    TargetFloor.Selected = false;
                    TargetFloor = null;
                }

                if (TargetFloor == null)//처음 클릭
                {
                    StartPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, StartPoint2d);
                    TargetFloor = vm.CreateFloorVoxel(new Point3D(StartPoint3d.X, StartPoint3d.Y, StartPoint3d.Z + 0.1));
                    TargetFloor.Selected = true;
                    TargetFloor.StartPoint = new Point3D(StartPoint3d.X, StartPoint3d.Y, StartPoint3d.Z + 0.1);

                    TargetFloor.Vertexs.Add(StartPoint3d);
                    TargetFloor.Vertexs.Add(StartPoint3d);
                    TargetFloor.Index++;
                    return;
                }
                else if (TargetFloor != null && TargetFloor.Index < 4)//점과 점 생성중일때
                {
                    TargetFloor.Vertexs.Add(new Point3D(CurrentPoint3d.X, CurrentPoint3d.Y, CurrentPoint3d.Z + TargetFloor.FHeight));
                    TargetFloor.Index++;
                    return;
                }

            }
            #endregion

            #region 가상 방문 모드일때
            if(Viewer2DMouseMode == MouseAcitonType.VirtualVisit)
            {
                PushUndoRedoState(0);
                
                StartPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, StartPoint2d);
                if (vm.VirtualPersonList.Count != 0 && viewport.CameraMode == CameraMode.WalkAround)
                {
                    vm.VirtualPersonModel.Children.Clear();
                    vm.ModelToVirtualPerson.Clear();
                    vm.VirtualPersonList.Clear();
                    vm.UpdateVisualPersonModel();
                }
                TargetVirtualPersson = vm.CreateVirtualPersonVoxel(StartPoint3d);
                viewport.Camera.Position = new Point3D(StartPoint3d.X, StartPoint3d.Y, 12);
                viewport.IsChangeFieldOfViewEnabled = true;
                MainCamera.FieldOfView = 105;
                MainCamera.NearPlaneDistance = 1;
                CameraController ca = viewport.CameraController;
                ca.MoveSensitivity = 3;
                ca.LeftRightPanSensitivity = 3;
                vm.UpdateVisualPersonModel();

            }
            #endregion
        }
        //Move
        private void ViewportTop_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentPoint2d = e.GetPosition(viewportTop);
            #region 가구 생성 드래그 일때
            if (vm.DragArrangement)
            {
                if (vm.DragArrangementFurnitrue != null && e.LeftButton == MouseButtonState.Pressed)
                {
                    viewportTop.InputBindings.Clear();
                    CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                    vm.DragArrangementFurnitrue.Position = CurrentPoint3d;

                    StartPoint2d = CurrentPoint2d;
                    StartPoint3d = CurrentPoint3d;

                    viewportTop.Cursor = Cursors.Hand;
                    Viewer2DMouseMode = MouseAcitonType.SelectionMode;
                    SelectionIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CursorDefault;
                    MouseModeListBox.SelectedIndex = 0;

                    vm.UpdateFurnitureModel();
                }
                return;
            } 
            #endregion
            #region 벽생성모드
            if (Viewer2DMouseMode == MouseAcitonType.WallCreateMode)
            {
                viewportTop.InputBindings.Clear();
                if (viewportTop.Cursor != Cursors.Cross)
                    viewportTop.Cursor = Cursors.Cross;
                if (TargetWall != null)
                {
                    CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                    double distance = StartPoint3d.DistanceTo(CurrentPoint3d);
                    double angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);

                    if (Keyboard.IsKeyDown(Key.Space))
                    {
                        if (angle < -268 || angle > 88)
                        {
                            TargetWall.Angle = -90;
                            TargetWall.EndPoint = new Point3D(((TargetWall.Width) * Math.Cos(-(-TargetWall.Angle / 57.2)) + TargetWall.StartPoint.X),
                                ((TargetWall.Width) * Math.Sin(-(-TargetWall.Angle / 57.2)) + TargetWall.StartPoint.Y),0);
                            if (previousWall != null)
                            {
                                distance = previousWall.EndPoint.DistanceTo(CurrentPoint3d);
                            }
                            TargetWall.Width = distance;//-268 +88
                            vm.UpdateWallModel();

                            if (viewport.CameraMode != CameraMode.WalkAround)
                                SetViewport3DCameraPosition();
                            CreateWallInfoText();
                            return;
                        }

                        
                        if (((-angle % 15) < 1.5 ) || (angle % 15) < 1.5)
                        {
                            double x =  -angle % 15;
                            angle += x;
                            TargetWall.Angle = -angle;
                            TargetWall.EndPoint = new Point3D(((TargetWall.Width) * Math.Cos(-(angle / 57.2)) + TargetWall.StartPoint.X),((TargetWall.Width) * Math.Sin(-(angle / 57.2)) + TargetWall.StartPoint.Y)
                            , 0);

                            if (previousWall != null)
                            {
                                distance = previousWall.EndPoint.DistanceTo(CurrentPoint3d);
                            }
                            TargetWall.Width = distance;//-268 +88
                        }


                        vm.UpdateWallModel();

                        if (viewport.CameraMode != CameraMode.WalkAround)
                            SetViewport3DCameraPosition();
                        CreateWallInfoText();
                        return;
                    }
                    else
                    {
                        TargetWall.EndPoint = CurrentPoint3d;
                        TargetWall.Width = distance;
                        TargetWall.Angle = -angle;

                        vm.UpdateWallModel();
                        CreateWallInfoText();

                        if (viewport.CameraMode != CameraMode.WalkAround)
                            SetViewport3DCameraPosition();
                        return;
                    }
                }
            }
            #endregion
            #region 선택모드
            if (Viewer2DMouseMode == MouseAcitonType.SelectionMode)
            {
                #region  Selected Wall MoveEvent Process
                if (TargetWall != null)
                {
                    if (!WallFocusTest() && e.LeftButton == MouseButtonState.Released)
                    {
                        viewportTop.Cursor = Cursors.Arrow;
                    }
                    if (TargetWall.State == Interfaces.ActionType.WallLeftControl && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        double distance = TargetWall.EndPoint.DistanceTo(CurrentPoint3d);
                        double angle = GetAngle(CurrentPoint3d.X, CurrentPoint3d.Y, TargetWall.EndPoint.X, TargetWall.EndPoint.Y);
                        TargetWall.StartPoint = CurrentPoint3d;
                        TargetWall.Width = distance;
                        TargetWall.Angle = -angle;

                        CreateWallInfoText();
                        ConnectedLeftWallTracking();



                        if (viewport.CameraMode != CameraMode.WalkAround)
                            SetViewport3DCameraPosition();
                        vm.UpdateWallModel();
                        return;

                    }
                    if (TargetWall.State == Interfaces.ActionType.WallRightControl && e.LeftButton == MouseButtonState.Pressed)
                    {

                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        double distance = TargetWall.StartPoint.DistanceTo(CurrentPoint3d);
                        double angle = GetAngle(TargetWall.StartPoint.X, TargetWall.StartPoint.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                        TargetWall.EndPoint = CurrentPoint3d;
                        TargetWall.Width = distance;
                        TargetWall.Angle = -angle;

                        CreateWallInfoText();
                        ConnectedRightWallTracking();


                        if (viewport.CameraMode != CameraMode.WalkAround)
                            SetViewport3DCameraPosition();
                        vm.UpdateWallModel();
                        return;

                    }
                    if (TargetWall.State == Interfaces.ActionType.Move && e.LeftButton == MouseButtonState.Pressed)
                    {

                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        if (CurrentPoint3d != null && TargetWall != null && TargetWall.Selected == true)
                        {
                            viewportTop.Cursor = Cursors.ScrollAll;

                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                AllMove();
                                return;
                            }
                            Point3D startp = TargetWall.StartPoint;
                            Point3D endp = TargetWall.EndPoint;

                            startp.X += -Delta.X;
                            startp.Y += -Delta.Y;
                            endp.X += -Delta.X;
                            endp.Y += -Delta.Y;
                            TargetWall.StartPoint = startp;
                            TargetWall.EndPoint = endp;

                            ConnectedWallTracking();
                            StartPoint2d = CurrentPoint2d;
                            StartPoint3d = CurrentPoint3d;
                            vm.UpdateWallModel();
                            //SetViewport3DCameraPosition();
                            return;
                        }
                    }
                }
                #endregion
                #region Selected Floor MoveEvent Process
                if (TargetFloor != null)
                {
                    if (!FloorFocusTest() && e.LeftButton == MouseButtonState.Released)
                    {
                        viewportTop.Cursor = Cursors.Arrow;
                    }
                    if (TargetFloor.State == ActionType.FloorRsize && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        Point3D p = TargetFloor.Vertexs[TargetFloor.SelectVertexIndex];
                        p.X += -Delta.X;
                        p.Y += -Delta.Y;
                        TargetFloor.Vertexs[TargetFloor.SelectVertexIndex] = p;
                        vm.UpdateFloorModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        return;

                    }
                    if (TargetFloor.State == ActionType.Move && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            AllMove();
                            return;
                        }
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        for (int i = 0; i < TargetFloor.Vertexs.Count; i++)
                        {
                            Point3D p = TargetFloor.Vertexs[i];
                            p.X += -Delta.X;
                            p.Y += -Delta.Y;
                            TargetFloor.Vertexs[i] = p;
                        }
                        vm.UpdateFloorModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        return;
                    }
                }
                #endregion
                #region Selected Furniture MoveEvent Process
                if (TargetFurniture != null)
                {
                    if (e.LeftButton == MouseButtonState.Released)
                    {
                        if (!FurnitureFocusTest())
                            viewportTop.Cursor = Cursors.Arrow;
                    }
                    if (TargetFurniture.State == ActionType.Move && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            AllMove();
                            return;
                        }
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        Point3D p = TargetFurniture.Position;
                        p.X += -Delta.X;
                        p.Y += -Delta.Y;
                        TargetFurniture.Position = p;
                        
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;

                        #region 스마트 각도
                        _hitModel = FindSourceToHViewport(viewportTop, CurrentPoint2d);
                        if (_hitModel != null)
                        {
                            tempWall = vm.GetWall(_hitModel);
                            tempFurniture = vm.GetFurniture(_hitModel);
                        }
                        if (tempWall != null)
                        {
                            if (First == 2)
                            {
                                TargetFurniture.AngleZ = tempWall.Angle + 180;
                                TargetFurniture.Position = CurrentPoint3d;
                                vm.UpdateFurnitureModel();
                                TargetWall = null;
                                First = 3;
                                return;
                            }
                            if(First==0)
                            {
                                TargetFurniture.AngleZ = tempWall.Angle;
                                TargetFurniture.Position = CurrentPoint3d;
                                TargetWall = null;
                                First = 1;
                            }
                        }
                        else
                        {
                            if(First == 1)
                            {
                                First = 2;
                                TargetWall = null;
                            }
                            if(First == 3)
                            {
                                First = 0;
                                TargetWall = null;
                            }
                        }
                        #endregion
                        #region 스마트 높이
                        if (tempFurniture != null)
                        {
                            if (tempFurniture.Depth > TargetFurniture.Depth)
                            {
                                Point3D tp = TargetFurniture.Position;
                                tp.Z = tempFurniture.Depth;
                                TargetFurniture.Position = tp;
                                tempFurniture = null;
                            }
                        }
                        #endregion
                        vm.UpdateFurnitureModel();
                        return;
                    }
                    if (TargetFurniture.State == ActionType.Rotate && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);

                        double angle = GetAngle1(TargetFurniture.Center.X, TargetFurniture.Center.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                        TargetFurniture.AngleZ = -(angle + 45);
                        CreateFurnitrueRotateInfoText();

                        vm.UpdateFurnitureModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;

                        return;
                    }
                    if (TargetFurniture.State == ActionType.Resize && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);

                        TargetFurniture.ScaleX += -Delta.X / 10;
                        TargetFurniture.ScaleY += Delta.Y / 10;

                        CreateFurnitrueScaleInfoText();
                        vm.UpdateFurnitureModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        return;
                    }
                    if (TargetFurniture.State == ActionType.ResizeZ && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);

                        TargetFurniture.ScaleZ += -Delta.Y / 10;

                        CreateFurnitrueScaleZInfoText();
                        vm.UpdateFurnitureModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        return;
                    }
                    if (TargetFurniture.State == ActionType.TranslateZ && e.LeftButton == MouseButtonState.Pressed)
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        Point3D p = TargetFurniture.Position;
                        p.Z += -Delta.Y;
                        if (p.Z < 0) p.Z = 0;
                        TargetFurniture.Position = p;

                        CreateFurnitrueheightInfoText();
                        vm.UpdateFurnitureModel();
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        return;
                    }
                }
                #endregion
                #region Selected VirtualPerson MoveEventPrecess
                if(TargetVirtualPersson !=null)
                {
                    if((TargetVirtualPersson.State == ActionType.Move && e.LeftButton == MouseButtonState.Pressed))
                    {
                        viewportTop.InputBindings.Clear();
                        CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        Point3D p = TargetVirtualPersson.Position;
                        p.X += -Delta.X;
                        p.Y += -Delta.Y;
                        TargetVirtualPersson.Position = p;
                        MainCamera.Position = p;
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateVisualPersonModel();
                        return;
                    }
                }
                #endregion
            }
            #endregion
            #region 방생성 모드일때
            if (Viewer2DMouseMode == MouseAcitonType.RoomCreateMode)
            {
                viewportTop.InputBindings.Clear();
                if (viewportTop.Cursor != Cursors.Cross)
                    viewportTop.Cursor = Cursors.Cross;

                if (TargetFloor != null && TargetFloor.Index < 4)
                {
                    CurrentPoint3d = Get2DtoRayPoint3D(viewportTop.Viewport, CurrentPoint2d);
                    List<Point3D> temp = new List<Point3D>();
                    for (int i = 0; i < TargetFloor.Vertexs.Count - 1; i++)
                    {
                        temp.Add(TargetFloor.Vertexs[i]);
                    }
                    TargetFloor.Vertexs.Clear();
                    for (int i = 0; i < temp.Count; i++)
                    {
                        TargetFloor.Vertexs.Add(temp[i]);
                    }
                    TargetFloor.Vertexs.Add(new Point3D(CurrentPoint3d.X, CurrentPoint3d.Y, CurrentPoint3d.Z + TargetFloor.FHeight));

                    vm.UpdateFloorModel();
                }
                return;
            }
            #endregion
            //move
        }
        //Up
        private void ViewportTop_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {
            _hitModel = null;
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            if(vm.DragArrangement)
            {
                //PushUndoRedoState(0);
                vm.DragArrangementFurnitrue = null;
                vm.DragArrangement = false;
            }
            vm.UpdateFunitureDataGrid();
            viewportTop.InputBindings.Add(new MouseBinding(vm.RectangleSelectionTopCommand, new MouseGesture(MouseAction.LeftClick)));
            if(Viewer2DMouseMode == MouseAcitonType.SelectionMode)
            viewportTop.Cursor = Cursors.Arrow;
            viewportTop.ReleaseMouseCapture();
        }
        //wall 수정
        private void ViewportTop_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Viewer2DMouseMode == MouseAcitonType.SelectionMode)
            {
                var p = e.GetPosition(viewportTop);
                var _hitModel = FindSourceToHViewport(viewportTop, p);
                if (_hitModel == null) return;
                var wall = vm.GetWall(_hitModel);
                if (wall != null)
                {//벽이 맞다면
                    if (Edit != null) return;
                    Edit = new WallEditWindow(wall);
                    Edit.Closed += Edit_Closed;
                    Edit.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Edit.Show();
                    wall.Selected = false;
                    return;
                }
                var floor = vm.GetFloor(_hitModel);
                if(floor != null)
                {
                    if (floorEditWindow != null) return;
                    floorEditWindow = new FloorEditWindow(floor);
                    floorEditWindow.Closed += Edit_Closed;
                    floorEditWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    floorEditWindow.Show();
                    floor.Selected = false;
                    return;
                }
                var furniture = vm.GetFurniture(_hitModel);
                if(furniture != null)
                {
                    if (furnitureEditWindow != null) return;
                    furnitureEditWindow = new FurnitureEditWindow(furniture);
                    furnitureEditWindow.Closed += Edit_Closed;
                    furnitureEditWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    furnitureEditWindow.Show();
                    furniture.Selected = false;
                    return;
                }
            }
        }
        private void Edit_Closed(object sender, EventArgs e)
        {
            vm.UpdateAllModel();
            Edit = null;
            floorEditWindow = null;
            furnitureEditWindow = null;
        }
        #endregion

        #region 텍스트 
        private void CreateFurnitrueheightInfoText()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            TextList.Clear();
            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportTop.ActualWidth / 2) + 100, CurrentPoint2d.Y - (viewportTop.ActualHeight / 2) + 30);
            Border border = new Border();
            border.Width = 100;
            border.Height = 25;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock textblock = new TextBlock();
            //길이 각도 두께

            string content = string.Format("고도: {0}cm", Math.Round((TargetFurniture.Position.Z) * 10).ToString(),
                Math.Round((TargetFurniture.Height * TargetFurniture.ScaleY) * 10).ToString());
            textblock.Text = content;
            textblock.FontSize = 13;
            textblock.Foreground = Brushes.Black;
            border.Child = textblock;

            TextList.Add(border);
            DesignerPane.Children.Add(border);
        }
        private void CreateFurnitrueScaleZInfoText()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            TextList.Clear();
            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportTop.ActualWidth / 2) + 100, CurrentPoint2d.Y - (viewportTop.ActualHeight / 2) + 30);
            Border border = new Border();
            border.Width = 100;
            border.Height = 25;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock textblock = new TextBlock();
            //길이 각도 두께

            string content = string.Format("높이: {0}cm", Math.Round((TargetFurniture.Depth * TargetFurniture.ScaleZ) * 10).ToString(),
                Math.Round((TargetFurniture.Height * TargetFurniture.ScaleY) * 10).ToString());
            textblock.Text = content;
            textblock.FontSize = 13;
            textblock.Foreground = Brushes.Black;
            border.Child = textblock;

            TextList.Add(border);
            DesignerPane.Children.Add(border);
        }
        private void CreateFurnitrueScaleInfoText()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            TextList.Clear();
            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportTop.ActualWidth / 2) + 100, CurrentPoint2d.Y - (viewportTop.ActualHeight / 2) + 30);
            Border border = new Border();
            border.Width = 100;
            border.Height = 45;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock textblock = new TextBlock();
            //길이 각도 두께

            string content = string.Format("너비: {0}cm\n깊이: {1}cm", Math.Round((TargetFurniture.Width * TargetFurniture.ScaleX) * 10).ToString(),
                Math.Round((TargetFurniture.Height * TargetFurniture.ScaleY) * 10).ToString());
            textblock.Text = content;
            textblock.FontSize = 13;
            textblock.Foreground = Brushes.Black;
            border.Child = textblock;

            TextList.Add(border);
            DesignerPane.Children.Add(border);
        }
        private void CreateFurnitrueRotateInfoText()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            TextList.Clear();
            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportTop.ActualWidth / 2) + 100, CurrentPoint2d.Y - (viewportTop.ActualHeight / 2) + 30);
            Border border = new Border();
            border.Width = 70;
            border.Height = 20;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock textblock = new TextBlock();
            //길이 각도 두께
            double calAngle;
            if (TargetFurniture.AngleZ < 0)
            {
                calAngle = TargetFurniture.AngleZ + 360;
            }
            else
            {
                calAngle = TargetFurniture.AngleZ;
            }
            string content = string.Format("각도: {0}˚", Math.Round(calAngle).ToString());
            textblock.Text = content;
            textblock.FontSize = 13;
            textblock.Foreground = Brushes.Black;
            border.Child = textblock;

            TextList.Add(border);
            DesignerPane.Children.Add(border);
        }
        private void CreateWallInfoText()
        {
            foreach (var item in TextList)
            {
                DesignerPane.Children.Remove(item);
            }
            TextList.Clear();
            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportTop.ActualWidth / 2) + 100, CurrentPoint2d.Y - (viewportTop.ActualHeight / 2) + 30);
            Border border = new Border();
            border.Width = 100;
            border.Height = 55;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock textblock = new TextBlock();

            string content = string.Format("길이: {0} cm \n각도: {1}˚\n두께: {2} cm", Math.Round(TargetWall.Width * 10).ToString(), Math.Round(TargetWall.Angle).ToString(), (TargetWall.Height * 10).ToString());
            
            textblock.Text = content;
            textblock.FontSize = 13;
            textblock.Foreground = Brushes.Black;
            textblock.Width = 100;
            textblock.Height = 55;
            border.Child = textblock;

            TextList.Add(border);
            DesignerPane.Children.Add(border);
        }

        #endregion

        
    }

    internal enum MouseAcitonType
    {
        SelectionMode = 0,
        WallCreateMode = 1,
        RoomCreateMode = 2,
        MaterialSelectMode = 3,
        ViewTop = 4,
        VirtualVisit = 5
    }
}
