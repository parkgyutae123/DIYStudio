using HelixToolkit.Wpf;
using MaterialDesignThemes.Wpf;
using SF_DIY.Domain;
using SF_DIY.Interfaces;
using SF_DIY.TransitionsHelp;
using SUT.PrintEngine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SF_DIY
{
    /// <summary>
    /// MenusAndToolBars.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DoItYourSlefControl : UserControl
    {
        public static Action DataGridEvnetHandler = null;
        MaterialAddWindow materialAddWindow;
        #region 필드
        Point StartPoint2d;
        Point CurrentPoint2d;
        Point3D StartPoint3d;
        Point3D CurrentPoint3d;
        Rect3D TargetBoundbox;
        MainViewModel vm;
        private Model3D _hitModel;
        Voxel voxel;
        UndoRedo undoredo;
        private Point3D previousPoint;
        int responsiveness = 25;
        private bool _IsGridMove = false;
        List<UIElement> AngleTextList;
        public bool IsZoomBinding = true;
        private HelpSwitcher hs;

        #endregion

        #region 생성자
        public DoItYourSlefControl()
        {
            InitializeComponent();
            vm = new MainViewModel(this,viewport, viewportTop, viewportFront, viewportRight);
            this.DataContext = vm;
            viewport.DataContext = vm;
            viewportFront.DataContext = vm;
            viewportRight.DataContext = vm;
            viewportTop.DataContext = vm;
            viewportTop.CameraChanged += Viewport_CameraChanged;
            viewportFront.CameraChanged += Viewport_CameraChanged;
            viewportRight.CameraChanged += Viewport_CameraChanged;
            this.viewport.InputBindings.Add(new MouseBinding(vm.RectangleSelectionCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift)));
            this.viewportFront.InputBindings.Add(new MouseBinding(vm.RectangleSelectionFrontCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift)));
            this.viewportRight.InputBindings.Add(new MouseBinding(vm.RectangleSelectionRightCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift)));
            this.viewportTop.InputBindings.Add(new MouseBinding(vm.RectangleSelectionTopCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift)));
            undoredo = new UndoRedo(vm);
            viewport.Focus();
            AngleTextList = new List<UIElement>();
        }

        #endregion

        #region 줌
        private void Viewport_CameraChanged(object sender, RoutedEventArgs e)
        {
            ZoomEvent(sender);
            vm.UpdateModel();
        }
        private void ZoomEvent(object sender)
        {
            HelixViewport3D view = sender as HelixViewport3D;
            if (view.Camera == TopCamera)
            {
                FrontCamera.Position = new Point3D(FrontCamera.Position.X, -TopCamera.Position.Z, FrontCamera.Position.Z);
                FrontCamera.LookDirection = new Vector3D(FrontCamera.LookDirection.X, -TopCamera.LookDirection.Z, FrontCamera.LookDirection.Z);
                FrontCamera.Width = TopCamera.Width;
                RightCamera.Position = new Point3D(TopCamera.Position.Z, RightCamera.Position.Y, RightCamera.Position.Z);
                RightCamera.LookDirection = new Vector3D(TopCamera.LookDirection.Z, RightCamera.LookDirection.Y, RightCamera.LookDirection.Z);
                RightCamera.Width = TopCamera.Width;
            }
            if (view.Camera == FrontCamera)
            {
                TopCamera.Position = new Point3D(TopCamera.Position.X, TopCamera.Position.Y, -FrontCamera.Position.Y);
                TopCamera.LookDirection = new Vector3D(TopCamera.LookDirection.X, TopCamera.LookDirection.Y, -FrontCamera.LookDirection.Y);
                TopCamera.Width = FrontCamera.Width;
                RightCamera.Position = new Point3D(-FrontCamera.Position.Y, RightCamera.Position.Y, RightCamera.Position.Z);
                RightCamera.LookDirection = new Vector3D(-FrontCamera.LookDirection.Y, RightCamera.LookDirection.Y, RightCamera.LookDirection.Z);
                RightCamera.Width = FrontCamera.Width;
            }
            if (view.Camera == RightCamera)
            {
                FrontCamera.Position = new Point3D(FrontCamera.Position.X, -RightCamera.Position.X, FrontCamera.Position.Z);
                FrontCamera.LookDirection = new Vector3D(FrontCamera.LookDirection.X, -RightCamera.LookDirection.X, FrontCamera.LookDirection.Z);
                FrontCamera.Width = RightCamera.Width;
                TopCamera.Position = new Point3D(TopCamera.Position.X, TopCamera.Position.Y, RightCamera.Position.X);
                TopCamera.LookDirection = new Vector3D(TopCamera.LookDirection.X, TopCamera.LookDirection.Y, RightCamera.LookDirection.X);
                TopCamera.Width = RightCamera.Width;
            }
        }
        #endregion

        #region 메뉴 클릭 이벤트     
        private void NewButton_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("현재 만들어진 내용을 지웁니다.", "새창", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                vm.Voxels.Clear();
                vm.UpdateModel();
            }
        }
        private void InteriorButton_Click(object sender, MouseButtonEventArgs e)
        {
            if(vm.Model.Children.Count == 0)
            {
                return;
            }
            foreach (var v in vm.Voxels)
            {
                v.Selected = false;
            }
            vm.IsShowSizeText = false;
            ShowSizeTextIcon.Kind = PackIconKind.MessageText;
            vm._ShowLine = 0;
            OutlineIcon.Kind = PackIconKind.CubeOutline;
            vm.UpdateModel();
            TransferDIY CreateItemName = new TransferDIY(vm.Model);
            CreateItemName.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CreateItemName.Topmost = true;
            CreateItemName.Show();
        }
        private void DeleteButton_Click(object sender, MouseButtonEventArgs e)
        {
            List<Voxel> remove_list = new List<Voxel>();
            foreach (var v in vm.Voxels)
            {
                if(v.Selected)
                {
                    remove_list.Add(v);
                }
            }
            foreach (var v in remove_list)
            {
                vm.Voxels.Remove(v);
            }
            vm.UpdateModel();
        }
        private void CutButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.Cut();
        }
        private void CopyButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.Copying();
        }
        private void AllSelectButton_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var v in vm.Voxels)
            {
                v.Selected = true;
            }
            vm.UpdateModel();
        }
        private void PasteButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.Paste();
        }
        private void AddModel_Click(object sender, MouseButtonEventArgs e)
        {
            materialAddWindow = new MaterialAddWindow();
            materialAddWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            materialAddWindow.ShowDialog();
        }
        private void Rotate3dButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            foreach (var v in vm.Voxels)
            {
                if (v.Selected)
                {
                    v.AxisType = AxisType.zAxis;
                    v.AngleZ += 15;
                }
            }
            vm.UpdateModel();
            ChangeRepresentationObject cro = undoredo.MakeNowVoxelsState(vm.Voxels);
            undoredo.InsertObjectforUndoRedo(cro);
        }
        private void IsGridMove_Click(object sender, MouseButtonEventArgs e)
        {
            _IsGridMove = !_IsGridMove;
            if (_IsGridMove)
            {
                GridIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Grid;
            }
            else
            {
                GridIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GridOff;
            }
        }
        private void OutLineButton_Click(object sender, MouseButtonEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if(mi!=null)
            {
                switch (mi.Header.ToString())
                {
                    case "외곽선 없애기":
                        vm._ShowLine = 0;
                        OutlineIcon.Kind = PackIconKind.Cube; break;
                    case "외곽선 표시":
                        vm._ShowLine = 1;
                        OutlineIcon.Kind = PackIconKind.CubeOutline; break;
                    case "외곽선만 보기":
                        vm._ShowLine = 2;
                        OutlineIcon.Kind = PackIconKind.Hexagon; break;

                    default:
                        break;
                }
                vm.UpdateModel();
                return;
            }
            if (vm._ShowLine == 0)
            {
                OutlineIcon.Kind = PackIconKind.Cube;
            }
            else if (vm._ShowLine == 1)
            {
                OutlineIcon.Kind = PackIconKind.CubeOutline;
            }
            else if (vm._ShowLine == 2)
            {
                OutlineIcon.Kind = PackIconKind.Hexagon;
            }
        }
        private void ShowSizeTextButton_Click(object sender, MouseButtonEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                switch (mi.Header.ToString())
                {
                    case "길이 정보 보기":
                        vm.IsShowSizeText = true;
                        ShowSizeTextIcon.Kind = PackIconKind.MessageTextOutline; break;
                    case "길이 정보 보지 않음":
                        vm.IsShowSizeText = false;
                        ShowSizeTextIcon.Kind = PackIconKind.MessageText; break;
                    
                    default:
                        break;
                }
                vm.UpdateModel();
                return;
            }
            if (vm.IsShowSizeText)
            {
                ShowSizeTextIcon.Kind = PackIconKind.MessageTextOutline;
            }
            else
            {
                ShowSizeTextIcon.Kind = PackIconKind.MessageText;
            }
        }
        private void ZoomBindingButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsZoomBinding)
            {
                viewportTop.CameraChanged -= Viewport_CameraChanged;
                viewportFront.CameraChanged -= Viewport_CameraChanged;
                viewportRight.CameraChanged -= Viewport_CameraChanged;
                ZoomBindingIcon.Kind = PackIconKind.Contrast;
                IsZoomBinding = false;
            }
            else
            {

                viewportTop.CameraChanged += Viewport_CameraChanged;
                viewportFront.CameraChanged += Viewport_CameraChanged;
                viewportRight.CameraChanged += Viewport_CameraChanged;
                ZoomBindingIcon.Kind = PackIconKind.ContrastBox;
                IsZoomBinding = true;
            }
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var visualSize = new Size(doitvisual.ActualWidth, doitvisual.ActualHeight);
            var printControl = PrintControlFactory.Create(visualSize, doitvisual);
            printControl.ShowPrintPreview();
        }
        private void ToSaveButton_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var v in vm.Voxels)
            {
                v.Selected = false;
            }
            vm.IsShowSizeText = false;
            ShowSizeTextIcon.Kind = PackIconKind.MessageText;
            vm._ShowLine = 0;
            OutlineIcon.Kind = PackIconKind.CubeOutline;
            vm.UpdateModel();
            if (vm.nowPath !=null)
            {
                vm.Save();
            }
            else
            {
                vm.FileExport();
            }
                       
        }
        private void SaveButton_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var v in vm.Voxels)
            {
                v.Selected = false;
            }
            vm.IsShowSizeText = false;
            ShowSizeTextIcon.Kind = PackIconKind.MessageText;
            vm._ShowLine = 0;
            OutlineIcon.Kind = PackIconKind.CubeOutline;
            vm.UpdateModel();

            vm.FileExport();
        }
        private void OpenButton_Click(object sender, MouseButtonEventArgs e)
        {

            vm.IsShowSizeText = false;
            ShowSizeTextIcon.Kind = PackIconKind.MessageText;
            vm._ShowLine = 0;
            OutlineIcon.Kind = PackIconKind.CubeOutline;
            vm.UpdateModel();
            vm.FileOpen();
        }
        #region 도움말
        private void HelpButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (hs == null)
            {
                hs = new HelpSwitcher();
            }
            hs.Topmost = true;
            hs.Show();
            hs.Closed += Hs_Closed;
        }
        private void Hs_Closed(object sender, EventArgs e)
        {
            hs = null;
        }
        #endregion
        #endregion

        #region 메소드

        private Model3D FindSource(Point p, out Vector3D normal)
        {
            var hits = Viewport3DHelper.FindHits(viewport.Viewport, p);

            foreach (var h in hits)
            {
                if (h.Model == vm.PreviewModel)
                    continue;
                normal = h.Normal;
                return h.Model;
            }
            normal = new Vector3D();
            return null;
        }
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
        private double GetAngle(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            double rad = Math.Atan2(dx, dy);
            double degree = (rad * 180) / Math.PI;
            return degree - 90;
        }
        private void RotateModel(Voxel v, int CameraType)
        {
            if (CameraType == 1)
            {
                CurrentPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, CurrentPoint2d);
                double angle = GetAngle(v.Center.X, v.Center.Y, CurrentPoint3d.X, CurrentPoint3d.Y);

                v.AxisType = AxisType.zAxis;
                v.AngleZ = (int)-angle;

                foreach (var item in AngleTextList)
                {
                    TopLeftView.Children.Remove(item);
                }
                ShowAngleText(v.AngleZ, CameraType);
                return;
            }
            if (CameraType == 2)
            {
                CurrentPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, CurrentPoint2d);
                double angle = GetAngle(v.Center.X, v.Center.Z, CurrentPoint3d.X, CurrentPoint3d.Y);

                v.AxisType = AxisType.yAxis;
                v.AngleY = (int)angle;

                foreach (var item in AngleTextList)
                {
                    BottomLeftView.Children.Remove(item);
                }

                ShowAngleText(v.AngleY, CameraType);
                return;
            }
            if (CameraType == 3)
            {

                CurrentPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, CurrentPoint2d);
                double angle = GetAngle(v.Center.Y, v.Center.Z, CurrentPoint3d.X, CurrentPoint3d.Y);

                v.AxisType = AxisType.xAxis;
                v.AngleX = (int)-angle;

                foreach (var item in AngleTextList)
                {
                    BottomRightView.Children.Remove(item);
                }
                ShowAngleText(v.AngleX, CameraType);
            }
        }
        private void ShowAngleText(double angle, int viewportType)
        {
            double calAngle;
            if (angle < 0)
            {
                calAngle = angle + 360;
            }
            else
            {
                calAngle = angle;
            }

            TranslateTransform tl = new TranslateTransform(CurrentPoint2d.X - (viewportFront.ActualWidth / 2) + 50, CurrentPoint2d.Y - (viewportFront.ActualHeight / 2) + 30);

            Border border = new Border();
            border.Width = 70;
            border.Height = 20;
            border.Background = Brushes.LightYellow;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.RenderTransform = tl;

            TextBlock angletext = new TextBlock();
            angletext.Text = string.Format(" 각도: {0}˚", Math.Round(calAngle).ToString());
            angletext.FontSize = 13;
            angletext.Foreground = Brushes.Black;
            border.Child = angletext;
            AngleTextList.Add(border);
            switch (viewportType)
            {
                case 1: TopLeftView.Children.Add(border); break;
                case 2: BottomLeftView.Children.Add(border); break;
                case 3: BottomRightView.Children.Add(border); break;
                default:
                    break;
            }
        }
        private void ResizeXModel(Voxel v, int CameraType, Point3D delta)
        {
            if (CameraType == 1 || CameraType == 2)
            {
                v.Width += -delta.X;

                if (v.Width < 1)//MinSize
                {
                    v.Width = 1;
                }
                if (v.Width > v.MaxLength / 100)//maxsize
                {
                    v.Width = v.MaxLength / 100;
                }
                return;
            }
            else
            {
                v.Width += -delta.X;

                if (v.Width < 1)//MinSize
                {
                    v.Width = 1;
                }
                if (v.Width > v.MaxLength / 100)//maxsize
                {
                    v.Width = v.MaxLength / 100;
                }
            }
        }
        private void ResizeYModel(Voxel v, int CameraType, Point3D delta)
        {
            if (CameraType == 1 || CameraType == 2)
            {
                v.Height += -delta.Y;

                if (v.Height < 1)//MinSize
                {
                    v.Height = 1;
                }
                if (v.Height > v.MaxWidth / 100)//maxsize
                {
                    v.Height = v.MaxWidth / 100;
                }
                return;
            }
            else
            {
                v.Height += -delta.Y;

                if (v.Height < 1)//MinSize
                {
                    v.Height = 1;
                }
                if (v.Height > v.MaxWidth / 100)//maxsize
                {
                    v.Height = v.MaxWidth / 100;
                }
            }
        }
        private Voxel SetActionType(Model3D _hitModel)
        {
            voxel = vm.GetVoxel(_hitModel);
            if (voxel != null)
            {
                voxel.State = ActionType.Move;
                return voxel;
            }
            voxel = vm.GetResizeXVoxel(_hitModel);
            if (voxel != null)
            {
                voxel.State = ActionType.ResizeX;
                return voxel;
            }
            voxel = vm.GetResizeYVoxel(_hitModel);
            if (voxel != null)
            {
                voxel.State = ActionType.ResizeY;
                return voxel;
            }
            voxel = vm.GetRoateVoxel(_hitModel);
            if (voxel != null)
            {
                voxel.State = ActionType.Rotate;
                return voxel;
            }
            else
                return null;
        }
        private Point3D Get2DtoTopPoint3D(Viewport3D viewport, Point p)
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
        private Point3D Get2DtoFrontPoint3D(Viewport3D viewport, Point p)
        {
            var ray = Viewport3DHelper.Point2DtoRay3D(viewport, p);
            if (ray != null)
            {
                var pi = ray.PlaneIntersection(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));
                if (pi.HasValue)
                {
                    var pRound = new Point3D((pi.Value.X), (pi.Value.Z), 0);
                    return pRound;
                }
            }
            return new Point3D(0, 0, 0);
        }
        private Point3D Get2DtoRightPoint3D(Viewport3D viewport, Point p)
        {
            var ray = Viewport3DHelper.Point2DtoRay3D(viewport, p);
            if (ray != null)
            {
                var pi = ray.PlaneIntersection(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
                if (pi.HasValue)
                {
                    var pRound = new Point3D((pi.Value.Y), (pi.Value.Z), 0);
                    return pRound;
                }
            }
            return new Point3D(0, 0, 0);
        }
        #endregion

        #region 키보드 이벤트
        private void viewport_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            HelixViewport3D view = sender as HelixViewport3D;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                MessageBoxResult result = MessageBox.Show("현재 만들어진 내용을 지웁니다.", "새창", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    vm.Voxels.Clear();
                    vm.UpdateModel();
                }
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                vm.FileOpen();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                foreach (var v in vm.Voxels)
                {
                    v.Selected = false;
                }
                vm.IsShowSizeText = false;
                ShowSizeTextIcon.Kind = PackIconKind.MessageText;
                vm._ShowLine = 0;
                OutlineIcon.Kind = PackIconKind.CubeOutline;
                vm.UpdateModel();

                if (vm.nowPath != null)
                {
                    vm.Save();
                }
                else
                {
                    vm.FileExport();
                }
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                vm.Copying();
            }
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                foreach (var v in vm.Voxels)
                {
                    v.Selected = false;
                }
                undoredo.Undo(1);
                vm.UpdateModel();
                return;
            }
            List<int> RemoveVoxelList = new List<int>();
            if (e.Key == Key.Delete)
            {
                vm.Model.Children.Clear();
                vm.ModelToVoxel.Clear();
                vm.OriginalMarterial.Clear();
                List<Voxel> removelist = new List<Voxel>();
                foreach (Voxel v in vm.Voxels)
                {
                    if (v.Selected)
                        removelist.Add(v);
                }
                if (removelist.Count != 0)
                {
                    ChangeRepresentationObject cro = undoredo.MakeNowVoxelsState(vm.Voxels);
                    undoredo.InsertObjectforUndoRedo(cro);
                }
                foreach (Voxel v in removelist)
                {
                    if (v.Selected)
                    {
                        RemoveVoxelList.Add(v.CompareToList);
                        vm.Voxels.Remove(v);
                    }
                }
                vm.UpdateModel();
                vm.DataGridUpdate();
                vm.InitEditControl();
                return;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                foreach (var v in vm.Voxels)
                {
                    v.Selected = true;
                }
                vm.UpdateModel();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                undoredo.Undo(1);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                undoredo.Redo(1);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                if (vm._ShowLine != 2)
                {
                    vm._ShowLine++;
                }
                else
                {
                    vm._ShowLine = 0;
                }
                if (vm._ShowLine == 0)
                {
                    OutlineIcon.Kind = PackIconKind.CubeOutline;
                }
                else if (vm._ShowLine == 1)
                {
                    OutlineIcon.Kind = PackIconKind.Cube;
                }
                else if (vm._ShowLine == 2)
                {
                    OutlineIcon.Kind = PackIconKind.CropSquare;
                }
                vm.UpdateModel();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                vm.Copying();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                vm.Paste();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                vm.Cut();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
            {
                vm.IsShowSizeText = !vm.IsShowSizeText;
                if (vm.IsShowSizeText)
                {
                    ShowSizeTextIcon.Kind = PackIconKind.MessageTextOutline;
                }
                else
                {
                    ShowSizeTextIcon.Kind = PackIconKind.MessageText;
                }
                vm.UpdateModel();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H)
            {
                _IsGridMove = !_IsGridMove;
                if (_IsGridMove)
                {
                    GridIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Grid;
                }
                else
                {
                    GridIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GridOff;
                }
            }
        }
        #endregion

        #region Main View 마우스 이벤트
        private void viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(viewport);
            _hitModel = FindSourceToHViewport(viewport, p);
            if (_hitModel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (voxel != null && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = false;
            }
            voxel = SetActionType(_hitModel);
            if (voxel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = true;
            }
            else
            {
                vm.SelectedRelease();
                voxel.Selected = voxel.Selected == false ? true : false;
            }

            vm.UpdateModel();
            vm.DataGridUpdate();
            vm.UpdateEditControl();
            TargetBoundbox = _hitModel.Bounds;
            return;
        }
        private void viewport_MouseMove(object sender, MouseEventArgs e)
        {

        }
        private void viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            voxel = null;
        }
        #endregion

        #region Top View 마우스 이벤트
        private void viewportTop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint2d = e.GetPosition(viewportTop);
            StartPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, StartPoint2d);
            _hitModel = FindSourceToHViewport(viewportTop, StartPoint2d);
            if (_hitModel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (voxel!=null && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = false;
            }
            voxel = SetActionType(_hitModel);
            if (voxel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = true;
            }
            else
            {
                vm.SelectedRelease();
                voxel.Selected = voxel.Selected == false ? true : false;
            }
            if (e.ClickCount < 2)
            {
                ChangeRepresentationObject cro = undoredo.MakeNowVoxelsState(vm.Voxels);
                undoredo.InsertObjectforUndoRedo(cro);
            }

            vm.UpdateModel();
            vm.DataGridUpdate();
            vm.UpdateEditControl();
            TargetBoundbox = _hitModel.Bounds;
            viewportTop.CaptureMouse();

        }
        private void viewportTop_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentPoint2d = e.GetPosition(this.viewportTop);
            if (voxel != null)
            {
                voxel.ZoomScaling = TopCamera.Width;
                if (viewportTop.IsMouseCaptured)
                {
                    if (voxel.State == ActionType.ResizeX)
                    {
                        #region ResizeX
                        CurrentPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeXModel(v, 1, Delta);
                                    TopLeftView.Cursor = Cursors.SizeWE;
                                }
                            }
                        }
                        else
                        {
                            ResizeXModel(voxel, 1, Delta);
                            TopLeftView.Cursor = Cursors.SizeWE;

                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        //vm.SettingEditControl();
                        //vm.DataGridUpdate();
                        return;
                        #endregion
                    }
                    if (voxel.State == ActionType.ResizeY)
                    {
                        #region ResizeY
                        CurrentPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeYModel(v, 1, Delta);
                                    TopLeftView.Cursor = Cursors.SizeNS;


                                }
                            }
                        }
                        else
                        {
                            ResizeYModel(voxel, 1, Delta);
                            TopLeftView.Cursor = Cursors.SizeNS;
                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                    if (voxel.State == ActionType.Move)
                    {
                        #region normalMove
                        if (_IsGridMove == false)
                        {
                            
                            CurrentPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, CurrentPoint2d);
                            Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);

                            if (voxel != null) // Move model
                            {
                                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                {
                                    foreach (var v in vm.Voxels)
                                    {
                                        if (v.Selected == true)
                                        {
                                            TopLeftView.Cursor = Cursors.ScrollAll;
                                            Point3D point = v.Position;
                                            point.X += -Delta.X;
                                            point.Y += -Delta.Y;
                                            if (Keyboard.IsKeyDown(Key.Space))
                                            {
                                                if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                {
                                                    return;
                                                }
                                            }
                                            v.Position = point;
                                        }
                                    }
                                }
                                else
                                {
                                    TopLeftView.Cursor = Cursors.ScrollAll;
                                    previousPoint = voxel.Position;
                                    Point3D point = voxel.Position;
                                    point.X += -Delta.X;
                                    point.Y += -Delta.Y;
                                    if (Keyboard.IsKeyDown(Key.Space))
                                    {
                                        if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                        {
                                            return;
                                        }
                                    }
                                    voxel.Position = point;
                                }
                                vm.UpdateModel();
                            }
                            StartPoint2d = CurrentPoint2d;
                            StartPoint3d = CurrentPoint3d;
                            return;
                        }
                        #endregion
                        #region GridMove
                        else
                        {
                            CurrentPoint3d = Get2DtoTopPoint3D(viewportTop.Viewport, CurrentPoint2d);
                            double distance = Math.Abs(Math.Sqrt(Math.Pow(CurrentPoint3d.X - StartPoint3d.X, 2) + Math.Pow(CurrentPoint3d.Y - StartPoint3d.Y, 2)));
                            if (distance > 0.5)
                            {
                                if (voxel != null) // Move model
                                {
                                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                    {
                                        foreach (var v in vm.Voxels)
                                        {
                                            if (v.Selected == true)
                                            {
                                                Point3D point = v.Position;

                                                TopLeftView.Cursor = Cursors.ScrollAll;
                                                double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                                int roundAngle = (int)Math.Round(Angle);
                                                if (roundAngle < 0)
                                                {
                                                    roundAngle += 360;
                                                }
                                                if (roundAngle <= 135 && roundAngle >= 45)//위
                                                {
                                                    point.Y += -0.5;
                                                }
                                                else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                                {
                                                    point.X += -0.5;
                                                }
                                                else if (roundAngle > 225 && roundAngle < 315)//아래
                                                {
                                                    point.Y += 0.5;
                                                }
                                                else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                                {
                                                    point.X += 0.5;
                                                }
                                                if (Keyboard.IsKeyDown(Key.Space))
                                                {
                                                    if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                    {
                                                        return;
                                                    }
                                                }
                                                v.Position = point;
                                            }
                                        }

                                        vm.UpdateModel();
                                        StartPoint2d = CurrentPoint2d;
                                        StartPoint3d = CurrentPoint3d;
                                    }
                                    else
                                    {
                                        Point3D point = voxel.Position;

                                        double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                        int roundAngle = (int)Math.Round(Angle);
                                        TopLeftView.Cursor = Cursors.ScrollAll;
                                        if (roundAngle < 0)
                                        {
                                            roundAngle += 360;
                                        }
                                        if (roundAngle <= 135 && roundAngle >= 45)//위
                                        {
                                            point.Y += -0.5;
                                        }
                                        else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                        {
                                            point.X += -0.5;
                                        }
                                        else if (roundAngle > 225 && roundAngle < 315)//아래
                                        {
                                            point.Y += 0.5;
                                        }
                                        else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                        {
                                            point.X += 0.5;
                                        }
                                        if (Keyboard.IsKeyDown(Key.Space))
                                        {
                                            if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                            {
                                                return;
                                            }
                                        }
                                        voxel.Position = point;
                                    }

                                    vm.UpdateModel();
                                    StartPoint2d = CurrentPoint2d;
                                    StartPoint3d = CurrentPoint3d;
                                }
                            }
                            return;
                        }
                        #endregion
                    }
                    if (voxel.State == ActionType.Rotate)
                    {
                        #region RotateMove
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    RotateModel(v, 1);
                                    TopLeftView.Cursor = Cursors.Cross;
                                }
                            }
                        }
                        else
                        {
                            RotateModel(voxel, 1);
                            TopLeftView.Cursor = Cursors.Cross;
                        }
                        StartPoint2d = CurrentPoint2d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                }

            }
            //Viewport_PreviewMouseWheel(viewportTop, new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 0));
        }
        private void viewportTop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            viewportTop.ReleaseMouseCapture();
            if (_IsGridMove)
            {
                StartPoint2d = CurrentPoint2d;
            }
            //화면 글씨 그림 삭제
            foreach (var item in AngleTextList)
            {
                if (TopLeftView.Children.Contains(item))
                    TopLeftView.Children.Remove(item);
                if (BottomLeftView.Children.Contains(item))
                    BottomLeftView.Children.Remove(item);
                if (BottomRightView.Children.Contains(item))
                    BottomRightView.Children.Remove(item);
            }

            TopLeftView.Cursor = Cursors.Arrow;
            vm.UpdateEditControl();
        }
        #endregion

        #region Front View 마우스 이벤트

        private void viewportFront_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint2d = e.GetPosition(viewportFront);
            StartPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, StartPoint2d);
            _hitModel = FindSourceToHViewport(viewportFront, StartPoint2d);
            if (_hitModel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.DataGridUpdate();
                vm.UpdateModel();
                return;
            }
            if (voxel != null && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = false;
            }
            voxel = SetActionType(_hitModel);
            if (voxel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = true;
            }
            else
            {
                vm.SelectedRelease();
                voxel.Selected = voxel.Selected == false ? true : false;
            }
            vm.UpdateEditControl();
            if (e.ClickCount < 2)
            {
                ChangeRepresentationObject cro = undoredo.MakeNowVoxelsState(vm.Voxels);
                undoredo.InsertObjectforUndoRedo(cro);
            }
            vm.UpdateModel();
            vm.DataGridUpdate();
            TargetBoundbox = _hitModel.Bounds;
            viewportFront.CaptureMouse();
        }
        private void viewportFront_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentPoint2d = e.GetPosition(this.viewportFront);
            if (voxel != null)
            {
                voxel.ZoomScaling = FrontCamera.Width;
                if (viewportFront.IsMouseCaptured)
                {
                    if (voxel.State == ActionType.ResizeX)
                    {
                        #region ResizeX
                        CurrentPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeXModel(v, 2, Delta);
                                    BottomLeftView.Cursor = Cursors.SizeWE;
                                }
                            }
                        }
                        else
                        {
                            ResizeXModel(voxel, 2, Delta);
                            BottomLeftView.Cursor = Cursors.SizeWE;
                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        //vm.SettingEditControl();
                        //vm.DataGridUpdate();
                        return;
                        #endregion
                    }
                    if (voxel.State == ActionType.ResizeY)
                    {
                        #region ResizeY
                        CurrentPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeYModel(v, 2, Delta);
                                    BottomLeftView.Cursor = Cursors.SizeNS;
                                }
                            }
                        }
                        else
                        {
                            ResizeYModel(voxel, 2, Delta);
                            BottomLeftView.Cursor = Cursors.SizeNS;
                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        //vm.SettingEditControl();
                        //vm.DataGridUpdate();
                        return;
                        #endregion
                    }

                    if (voxel.State == ActionType.Move)
                    {
                        #region normalMove
                        if (_IsGridMove == false)
                        {
                            CurrentPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, CurrentPoint2d);
                            Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                            if (voxel != null) // Move model
                            {
                                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                {
                                    foreach (var v in vm.Voxels)
                                    {
                                        if (v.Selected == true)
                                        {
                                            BottomLeftView.Cursor = Cursors.ScrollAll;
                                            Point3D point = v.Position;
                                            point.X += -Delta.X;
                                            point.Z += -Delta.Y;
                                            if (Keyboard.IsKeyDown(Key.Space))
                                            {
                                                if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                {
                                                    return;
                                                }
                                            }
                                            if (point.Z < 0)
                                            {
                                                point.Z = 0;
                                            }
                                            v.Position = point;
                                        }
                                    }
                                }
                                else
                                {
                                    BottomLeftView.Cursor = Cursors.ScrollAll;
                                    previousPoint = voxel.Position;
                                    Point3D point = voxel.Position;
                                    point.X += -Delta.X;
                                    point.Z += -Delta.Y;
                                    if (Keyboard.IsKeyDown(Key.Space))
                                    {
                                        if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                        {
                                            return;
                                        }
                                    }
                                    if (point.Z < 0)
                                    {
                                        point.Z = 0;
                                    }
                                    voxel.Position = point;
                                }


                                vm.UpdateModel();
                            }
                            StartPoint2d = CurrentPoint2d;
                            StartPoint3d = CurrentPoint3d;
                            return;
                        }
                        #endregion
                        #region GridMove
                        else
                        {
                            CurrentPoint3d = Get2DtoFrontPoint3D(viewportFront.Viewport, CurrentPoint2d);
                            double distance = Math.Abs(Math.Sqrt(Math.Pow(CurrentPoint3d.X - StartPoint3d.X, 2) + Math.Pow(CurrentPoint3d.Y - StartPoint3d.Y, 2)));
                            if (distance > 0.5)
                            {
                                if (voxel != null) // Move model
                                {
                                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                    {
                                        foreach (var v in vm.Voxels)
                                        {
                                            if (v.Selected == true)
                                            {
                                                Point3D point = v.Position;

                                                BottomLeftView.Cursor = Cursors.ScrollAll;
                                                double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                                int roundAngle = (int)Math.Round(Angle);
                                                if (roundAngle < 0)
                                                {
                                                    roundAngle += 360;
                                                }
                                                if (roundAngle <= 135 && roundAngle >= 45)//위
                                                {
                                                    point.Z += -0.5;
                                                }
                                                else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                                {
                                                    point.X += -0.5;
                                                }
                                                else if (roundAngle > 225 && roundAngle < 315)//아래
                                                {
                                                    point.Z += 0.5;
                                                }
                                                else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                                {
                                                    point.X += 0.5;
                                                }
                                                if (Keyboard.IsKeyDown(Key.Space))
                                                {
                                                    if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                    {
                                                        return;
                                                    }
                                                }
                                                if (point.Z < 0)
                                                {
                                                    point.Z = 0;
                                                }
                                                v.Position = point;
                                            }
                                        }

                                        vm.UpdateModel();
                                        StartPoint2d = CurrentPoint2d;
                                        StartPoint3d = CurrentPoint3d;
                                    }
                                    else
                                    {
                                        Point3D point = voxel.Position;

                                        double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                        int roundAngle = (int)Math.Round(Angle);
                                        BottomLeftView.Cursor = Cursors.ScrollAll;
                                        if (roundAngle < 0)
                                        {
                                            roundAngle += 360;
                                        }
                                        if (roundAngle <= 135 && roundAngle >= 45)//위
                                        {
                                            point.Z += -0.5;
                                        }
                                        else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                        {
                                            point.X += -0.5;
                                        }
                                        else if (roundAngle > 225 && roundAngle < 315)//아래
                                        {
                                            point.Z += 0.5;
                                        }
                                        else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                        {
                                            point.X += 0.5;
                                        }
                                        if (Keyboard.IsKeyDown(Key.Space))
                                        {
                                            if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                            {
                                                return;
                                            }
                                        }
                                        if(point.Z <0)
                                        {
                                            point.Z = 0;
                                        }
                                        voxel.Position = point;
                                    }

                                    vm.UpdateModel();
                                    StartPoint2d = CurrentPoint2d;
                                    StartPoint3d = CurrentPoint3d;
                                }
                            }
                            return;
                        }
                        #endregion
                    }
                    if (voxel.State == ActionType.Rotate)
                    {
                        #region RotateMove
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    RotateModel(v, 2);
                                    BottomLeftView.Cursor = Cursors.Cross;
                                }
                            }
                        }
                        else
                        {
                            RotateModel(voxel, 2);
                            BottomLeftView.Cursor = Cursors.Cross;
                        }
                        StartPoint2d = CurrentPoint2d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                }
            }

        }
        private void viewportFront_MouseUp(object sender, MouseButtonEventArgs e)
        {
            viewportFront.ReleaseMouseCapture();

            if (_IsGridMove)
            {
                StartPoint2d = CurrentPoint2d;
            }
            //화면 글씨 그림 삭제
            foreach (var item in AngleTextList)
            {
                if (TopLeftView.Children.Contains(item))
                    TopLeftView.Children.Remove(item);
                if (BottomLeftView.Children.Contains(item))
                    BottomLeftView.Children.Remove(item);
                if (BottomRightView.Children.Contains(item))
                    BottomRightView.Children.Remove(item);
            }
            BottomLeftView.Cursor = Cursors.Arrow;
            vm.UpdateEditControl();
        }

        #endregion

        #region Right View 마우스 이벤트
        private void viewportRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint2d = e.GetPosition(viewportRight);
            StartPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, StartPoint2d);
            _hitModel = FindSourceToHViewport(viewportRight, StartPoint2d);
            if (_hitModel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (voxel != null && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = false;
            }
            voxel = SetActionType(_hitModel);
            if (voxel == null)
            {
                vm.SelectedRelease();
                vm.InitEditControl();
                vm.UpdateModel();
                vm.DataGridUpdate();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                voxel.Selected = true;
            }
            else
            {
                vm.SelectedRelease();
                voxel.Selected = voxel.Selected == false ? true : false;
            }
            vm.UpdateEditControl();
            if (e.ClickCount < 2)
            {
                ChangeRepresentationObject cro = undoredo.MakeNowVoxelsState(vm.Voxels);
                undoredo.InsertObjectforUndoRedo(cro);
            }
            vm.UpdateModel();
            vm.DataGridUpdate();
            TargetBoundbox = _hitModel.Bounds;
            viewportRight.CaptureMouse();
        }
        private void viewportRight_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentPoint2d = e.GetPosition(viewportRight);
            if (voxel != null)
            {
                voxel.ZoomScaling = RightCamera.Width;
                if (viewportRight.IsMouseCaptured)
                {
                    if (voxel.State == ActionType.ResizeX)
                    {
                        #region ResizeX
                        CurrentPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeXModel(v, 3, Delta);
                                    BottomRightView.Cursor = Cursors.SizeWE;
                                }
                            }
                        }
                        else
                        {
                            ResizeXModel(voxel, 3, Delta);
                            BottomRightView.Cursor = Cursors.SizeWE;
                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                    if (voxel.State == ActionType.ResizeY)
                    {
                        #region ResizeY
                        CurrentPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, CurrentPoint2d);
                        Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    ResizeYModel(v, 3, Delta);
                                    BottomRightView.Cursor = Cursors.SizeNS;
                                }
                            }
                        }
                        else
                        {
                            ResizeYModel(voxel, 3, Delta);
                            BottomRightView.Cursor = Cursors.SizeNS;
                        }
                        StartPoint2d = CurrentPoint2d;
                        StartPoint3d = CurrentPoint3d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                    if (voxel.State == ActionType.Move)
                    {
                        #region 그냥이동
                        responsiveness = 1000 / (int)RightCamera.Width;
                        if (_IsGridMove == false)
                        {
                            CurrentPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, CurrentPoint2d);
                            Point3D Delta = new Point3D((StartPoint3d.X - CurrentPoint3d.X), (StartPoint3d.Y - CurrentPoint3d.Y), 0);
                            if (voxel != null) // Move model
                            {
                                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                {
                                    foreach (var v in vm.Voxels)
                                    {
                                        if (v.Selected == true)
                                        {
                                            BottomRightView.Cursor = Cursors.ScrollAll;
                                            Point3D point = v.Position;
                                            point.Y += -Delta.X;
                                            point.Z += -Delta.Y;
                                            if (Keyboard.IsKeyDown(Key.Space))
                                            {
                                                if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                {
                                                    return;
                                                }
                                            }
                                            if (point.Z < 0)
                                            {
                                                point.Z = 0;
                                            }
                                            v.Position = point;
                                        }
                                    }
                                }
                                else
                                {
                                    BottomRightView.Cursor = Cursors.ScrollAll;
                                    previousPoint = voxel.Position;
                                    Point3D point = voxel.Position;
                                    point.Y += -Delta.X;
                                    point.Z += -Delta.Y;
                                    if (Keyboard.IsKeyDown(Key.Space))
                                    {
                                        if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                        {
                                            return;
                                        }
                                    }
                                    if (point.Z < 0)
                                    {
                                        point.Z = 0;
                                    }
                                    voxel.Position = point;
                                }

                                vm.UpdateModel();
                            }
                            StartPoint2d = CurrentPoint2d;
                            StartPoint3d = CurrentPoint3d;
                            return;
                        }
                        #endregion
                        #region GridMove
                        else
                        {
                            CurrentPoint3d = Get2DtoRightPoint3D(viewportRight.Viewport, CurrentPoint2d);
                            double distance = Math.Abs(Math.Sqrt(Math.Pow(CurrentPoint3d.X - StartPoint3d.X, 2) + Math.Pow(CurrentPoint3d.Y - StartPoint3d.Y, 2)));
                            if (distance > 0.5)
                            {
                                if (voxel != null) // Move model
                                {
                                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                    {
                                        foreach (var v in vm.Voxels)
                                        {
                                            if (v.Selected == true)
                                            {
                                                Point3D point = v.Position;

                                                BottomRightView.Cursor = Cursors.ScrollAll;
                                                double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                                int roundAngle = (int)Math.Round(Angle);
                                                if (roundAngle < 0)
                                                {
                                                    roundAngle += 360;
                                                }
                                                if (roundAngle <= 135 && roundAngle >= 45)//위
                                                {
                                                    point.Z += -0.5;
                                                }
                                                else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                                {
                                                    point.Y += -0.5;
                                                }
                                                else if (roundAngle > 225 && roundAngle < 315)//아래
                                                {
                                                    point.Z += 0.5;
                                                }
                                                else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                                {
                                                    point.Y += 0.5;
                                                }
                                                if (Keyboard.IsKeyDown(Key.Space))
                                                {
                                                    if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                                    {
                                                        return;
                                                    }
                                                }
                                                if (point.Z < 0)
                                                {
                                                    point.Z = 0;
                                                }
                                                v.Position = point;
                                            }
                                        }

                                        vm.UpdateModel();
                                        StartPoint2d = CurrentPoint2d;
                                        StartPoint3d = CurrentPoint3d;
                                    }
                                    else
                                    {
                                        Point3D point = voxel.Position;

                                        double Angle = GetAngle(StartPoint3d.X, StartPoint3d.Y, CurrentPoint3d.X, CurrentPoint3d.Y);
                                        int roundAngle = (int)Math.Round(Angle);
                                        BottomRightView.Cursor = Cursors.ScrollAll;
                                        if (roundAngle < 0)
                                        {
                                            roundAngle += 360;
                                        }
                                        if (roundAngle <= 135 && roundAngle >= 45)//위
                                        {
                                            point.Z += -0.5;
                                        }
                                        else if (roundAngle > 135 && roundAngle < 225)//왼쪽
                                        {
                                            point.Y += -0.5;
                                        }
                                        else if (roundAngle > 225 && roundAngle < 315)//아래
                                        {
                                            point.Z += 0.5;
                                        }
                                        else if (roundAngle < 45 || roundAngle > 315)//오른쪽
                                        {
                                            point.Y += 0.5;
                                        }
                                        if (Keyboard.IsKeyDown(Key.Space))
                                        {
                                            if (IsCollisionAABB(TargetBoundbox, voxel, _hitModel))
                                            {
                                                return;
                                            }
                                        }
                                        if (point.Z < 0)
                                        {
                                            point.Z = 0;
                                        }
                                        voxel.Position = point;
                                    }

                                    vm.UpdateModel();
                                    StartPoint2d = CurrentPoint2d;
                                    StartPoint3d = CurrentPoint3d;
                                }
                            }
                            return;
                        }
                        #endregion
                    }
                    if (voxel.State == ActionType.Rotate)
                    {
                        #region RotateMove
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            foreach (var v in vm.Voxels)
                            {
                                if (v.Selected == true)
                                {
                                    RotateModel(v, 3);
                                    BottomRightView.Cursor = Cursors.Cross;
                                }
                            }
                        }
                        else
                        {
                            RotateModel(voxel, 3);
                            BottomRightView.Cursor = Cursors.Cross;
                        }
                        StartPoint2d = CurrentPoint2d;
                        vm.UpdateModel();
                        return;
                        #endregion
                    }
                }
            }
        }
        private void viewportRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            viewportRight.ReleaseMouseCapture();

            if (_IsGridMove)
            {
                StartPoint2d = CurrentPoint2d;
            }
            //화면 글씨 그림 삭제
            foreach (var item in AngleTextList)
            {
                if (TopLeftView.Children.Contains(item))
                    TopLeftView.Children.Remove(item);
                if (BottomLeftView.Children.Contains(item))
                    BottomLeftView.Children.Remove(item);
                if (BottomRightView.Children.Contains(item))
                    BottomRightView.Children.Remove(item);
            }
            BottomRightView.Cursor = Cursors.Arrow;
            vm.UpdateEditControl();
        }
        #endregion

        #region 충돌 이벤트 알고리즘
        public bool IsCollisionAABB(Rect3D mainBox, Voxel main, Model3D _hitmodel)
        {
            Point3D minP = new Point3D(main.Position.X, main.Position.Y, main.Position.Z);
            Point3D maxP = new Point3D(minP.X + mainBox.SizeX, minP.Y + mainBox.SizeY, minP.Z + mainBox.SizeZ);

            int count = 0;
            bool crush = false;
            foreach (var tar in vm.Model.Children)
            {
                if (main == vm.GetVoxel(tar)) continue;
                
                Point3D dest_minP = new Point3D(tar.Bounds.X, tar.Bounds.Y, tar.Bounds.Z);
                Point3D dest_maxP = new Point3D(tar.Bounds.X + tar.Bounds.SizeX, tar.Bounds.Y + tar.Bounds.SizeY, tar.Bounds.Z + tar.Bounds.SizeZ);

                if (count == 1) return crush;
                if ((minP.X < dest_maxP.X && maxP.X > dest_minP.X) &&
                    (minP.Y < dest_maxP.Y && maxP.Y > dest_minP.Y) &&
                    (minP.Z < dest_maxP.Z && maxP.Z > dest_minP.Z))
                {
                    crush = true;
                    count++;
                }
                else
                {
                    crush = false;
                }
            }
            return crush;
        }


        #endregion

        #region UndoRedo 

        void UnDoObject_EnableDisableUndoRedoFeature(object sender, EventArgs e)
        {
            if (undoredo.IsUndoPossible())
            {
                btnUndo.IsEnabled = true;
            }
            else
            {
                btnUndo.IsEnabled = false;

            }

            if (undoredo.IsRedoPossible())
            {
                btnRedo.IsEnabled = true;
            }
            else
            {
                btnRedo.IsEnabled = false;
            }

        }
        private void UndoButton_Click(object sender, MouseButtonEventArgs e)
        {
            undoredo.Undo(1);
        }
        private void RedoButton_Click(object sender, MouseButtonEventArgs e)
        {
            undoredo.Redo(1);
        }








        #endregion

        
    }
}
