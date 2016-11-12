using HelixToolkit.Wpf;
using MVVMBase.Command;
using MVVMBase.ViewModel;
using SF_DIY.Common.Behaviors;
using SF_DIY.Interfaces;
using SF_DIY.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Serialization;
using static HelixToolkit.Wpf.ObjExporter;

namespace SF_DIY
{
    internal class MainViewModel : ViewModelBase
    {

        #region ratio - 배율 
        public enum ratio
        {
            width = 100,
            hegith = 100,
            depth = 100,
            diameter = 100
        }
        #endregion

        #region 필드
        int SelectedVoxel = 9999;
        AddTextureComboBox ForTexture;
        public static int ProductNum = 0;

        private readonly Color[] palette = new[]
        {
            Colors.AliceBlue,
            Colors.Aqua
        };
        HelixViewport3D main;
        HelixViewport3D top;
        HelixViewport3D front;
        HelixViewport3D right;
        public int _ShowLine = 1;
        public IList<Model3D> selectedModels;
        private List<Visual3D> MainlinesVisuals = new List<Visual3D>();
        private List<Visual3D> ToplinesVisuals = new List<Visual3D>();
        private List<Visual3D> FrontlinesVisuals = new List<Visual3D>();
        private List<Visual3D> RightlinesVisuals = new List<Visual3D>();
        public List<Visual3D> WidthControllerVisuals = new List<Visual3D>();
        //private PointsVisual3D pointsVisual;
        private bool _IsShowSizeText = true;

        private Stack<Voxel> _UndoActionsCollection = new Stack<Voxel>();
        private Stack<Voxel> _RedoActionsCollection = new Stack<Voxel>();

        private IFileDialogService fileDialogService;
        private const string SaveFileFilter = "Sketch XML Files (*.xml)|*.xml|3D model files (*.xaml)|*.xaml|Wavefront files (*.obj)|*.obj|StereoLithography files (*.stl)|*.stl";
        private const string OpenFileFilter = "Sketch XML Files (*.xml)|*.xml";
        #endregion

        #region 저장 불러오기
        private XmlSerializer serializer;

        public void FileExport()
        {
            var path = this.fileDialogService.SaveFileDialog(null, null, SaveFileFilter, ".xml");
            if (path == null)
            {
                return;
            }

            string extension = Path.GetExtension(path);
            if (extension.Equals(".xaml"))
            {
                string name = "abb";
                var p = Environment.CurrentDirectory + @"\..\..\resources\" + name + ".xaml";
                var xm = XamlWriter.Save(Model);
                using (var stream = File.Create(path))
                {
                    StreamWriter sw = new StreamWriter(stream);
                    sw.Write(xm);
                    sw.Close();
                }
                return;
            }
            else if(extension.Equals(".obj"))
            {
                var e = new ObjExporter { TextureFolder = System.IO.Path.GetDirectoryName(path) };
                string mpath = path.Replace(".obj",".mtl");
                e.MaterialsFile = mpath;
                using (var stream = File.Create(path))
                {
                    e.Export(Model, stream);
                }
                return;
            }
            else if(extension.Equals(".stl"))
            {
                var e = new StlExporter();
                using (var stream = File.Create(path))
                {
                    e.Export(Model, stream);
                }
                return;
            }
            else
            {
                this.Save(path);
            }
        }
        public void FileOpen()
        {
            var path = this.fileDialogService.OpenFileDialog(null, null, OpenFileFilter, ".xml");
            if (path == null) return;
            if (TryLoad(path))
            {
            }
            UpdateModel();
        }
        private void Save(string fileName)
        {
            using (var w = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            {
                serializer = new XmlSerializer(typeof(List<Voxel>), new[] { typeof(Voxel) });
                int cnt = 1;
                foreach (var v in Voxels)
                {
                    if(v.Num==0)
                    {
                        v.Num = cnt;
                        cnt++;
                    }
                }
                serializer.Serialize(w, Voxels);
            }
        }

        public void Save()
        {
            if (nowPath != null)
            {
                using (var w = XmlWriter.Create(nowPath, new XmlWriterSettings { Indent = true }))
                {
                    serializer = new XmlSerializer(typeof(List<Voxel>), new[] { typeof(Voxel) });
                    List<Voxel> temp = new List<Voxel>();
                    foreach (var v in Voxels)
                    {
                        temp.Add(v);
                    }
                    serializer.Serialize(w, temp);
                }
            }
        }
        public bool TryLoad(string fileName)
        {
            try
            {
                using (var r = XmlReader.Create(fileName))
                {
                    serializer = new XmlSerializer(typeof(List<Voxel>), new[] { typeof(Voxel) });
                    var v = serializer.Deserialize(r);
                    Voxels = v as List<Voxel>;
                    ProductNum = Voxels[Voxels.Count-1].CompareToList;
                    nowPath = fileName;
                }
                UpdateModel();
                main.Camera.LookAt(new Point3D((Model.Bounds.X + Model.Bounds.SizeX) / 2, (Model.Bounds.Y + Model.Bounds.SizeY) / 2, (Model.Bounds.Z + Model.Bounds.SizeZ) / 2), 1000);
                DataGridUpdate();
                PreViewHandler(Voxels);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("파일이 손상되거나 형식이 달라 정상적으로 로드 할 수 없습니다. {0}", e.Message));
                return false;
            }
        }
        #endregion

        #region 속성
        public bool IsShowSizeText
        {
            get
            {
                return _IsShowSizeText;
            }
            set
            {
                _IsShowSizeText = value;
                OnPropertyChanged("IsShowSizeText");
            }

        }

        public List<Voxel> Voxels { get; set; }
        public List<Voxel> ClipBoardVoxels { get; set; }
        public Model3DGroup Model { get; set; }
        public Model3DGroup ControllerModel { get; set; }
        public Dictionary<Model3D, Voxel> ModelToVoxel { get; set; }
        public Dictionary<Model3D, Voxel> ResizeXModelToVoxel { get; set; }
        public Dictionary<Model3D, Voxel> ResizeYModelToVoxel { get; set; }
        public Dictionary<Model3D, Voxel> RotateModelToVoxel { get; set; }
        public Dictionary<Model3D, Material> OriginalMarterial { get; private set; }
        OrthographicCamera _Tcamera;
        OrthographicCamera _Fcamera;
        OrthographicCamera _Rcamera;
        public OrthographicCamera TopCamera
        {
            get { return _Tcamera; }
            set
            {
                _Tcamera = value;
                OnPropertyChanged("TopCamera");
            }
        }
        public OrthographicCamera FrontCamera
        {
            get { return _Fcamera; }
            set
            {
                _Fcamera = value;
                OnPropertyChanged("FrontCamera");
            }
        }
        public OrthographicCamera RightCamera
        {
            get { return _Rcamera; }
            set
            {
                _Rcamera = value;
                OnPropertyChanged("RightCamera");
            }
        }
        public IList<BillboardTextItem> TextItems { get; set; }
        public List<Model3D> Highlighted { get; set; }
        public Model3D PreviewModel { get; set; }
        public int PaletteIndex { get; set; }

        public RectangleSelectionCommand RectangleSelectionCommand { get; private set; }
        public RectangleSelectionCommand RectangleSelectionTopCommand { get; private set; }
        public RectangleSelectionCommand RectangleSelectionFrontCommand { get; private set; }
        public RectangleSelectionCommand RectangleSelectionRightCommand { get; private set; }

        #endregion

        #region Command

        #region ShowSizeTextCommand Button Command
        private ICommand _ShowSizeTextCommand;
        public ICommand ShowSizeTextCommand
        {
            get { return _ShowSizeTextCommand ?? (_ShowSizeTextCommand = new AppCommand(ShowSizeTextChanged)); }
        }

        private void ShowSizeTextChanged(object obj)
        {
            IsShowSizeText = !IsShowSizeText;
            UpdateModel();
        }
        #endregion

        #region 외곽선 표시
        private ICommand _ShowLinesVisual3D;


        public ICommand ShowLinesVisual3D
        {
            get { return _ShowLinesVisual3D ?? (_ShowLinesVisual3D = new AppCommand(IsShowLine)); }
        }
        private void IsShowLine(Object o)
        {
            if (_ShowLine != 2)
            {
                _ShowLine++;
            }
            else
            {
                _ShowLine = 0;
            }
            UpdateModel();
        }
        #endregion

        #endregion

        #region 생성자
        public static event Action<List<Voxel>> PreViewHandler = null;

        public MainViewModel(DoItYourSlefControl This, HelixViewport3D main, HelixViewport3D top, HelixViewport3D front, HelixViewport3D right)
        {
            doit = This;
            MyModel.GetInstance().Init();
            ForTexture = new AddTextureComboBox();

            this.fileDialogService = new FileDialogService();
            this.main = main; this.top = top; this.front = front; this.right = right; 
            Model = new Model3DGroup();
            ControllerModel = new Model3DGroup();
            Voxels = new List<Voxel>();
            ClipBoardVoxels = new List<Voxel>();
            Highlighted = new List<Model3D>();
            ModelToVoxel = new Dictionary<Model3D, Voxel>();
            ResizeXModelToVoxel = new Dictionary<Model3D, Voxel>();
            ResizeYModelToVoxel = new Dictionary<Model3D, Voxel>();
            RotateModelToVoxel = new Dictionary<Model3D, Voxel>();
            OriginalMarterial = new Dictionary<Model3D, Material>();
            TopCamera = (OrthographicCamera)top.Camera;
            FrontCamera = (OrthographicCamera)front.Camera;
            RightCamera = (OrthographicCamera)right.Camera;
            
            TextItems = new List<BillboardTextItem>();
            this.RectangleSelectionCommand = new RectangleSelectionCommand(main.Viewport, this.HandleSelectionEvent);
            this.RectangleSelectionTopCommand = new RectangleSelectionCommand(top.Viewport, this.HandleSelectionEvent);
            this.RectangleSelectionFrontCommand = new RectangleSelectionCommand(front.Viewport, this.HandleSelectionEvent);
            this.RectangleSelectionRightCommand = new RectangleSelectionCommand(right.Viewport, this.HandleSelectionEvent);

            MyModel.Selected += new SelectedProductEventHandler(AddVoxel);
            MaterialEditControl.AngleEventHandler += UpdateAngle;

            UpdateModel();
        }



        #endregion

        #region 회전

        private static Transform3DGroup ModelTransformAngle(Voxel v)
        {
            Transform3DGroup trans = new Transform3DGroup();
            AxisAngleRotation3D rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (double)v.AngleZ);
            AxisAngleRotation3D rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), (double)v.AngleY);
            AxisAngleRotation3D rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), (double)v.AngleX);

            RotateTransform3D rtz = new RotateTransform3D(rotationZ, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));
            RotateTransform3D rty = new RotateTransform3D(rotationY, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));
            RotateTransform3D rtx = new RotateTransform3D(rotationX, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));

            TranslateTransform3D tl = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);
            trans.Children.Add(rtz);
            trans.Children.Add(rty);
            trans.Children.Add(rtx);
            trans.Children.Add(tl);
            v.Transform3D = trans;
            return trans;
        }
        private Transform3DGroup LineTransformAngle(Voxel v)
        {

            Transform3DGroup trans = new Transform3DGroup();
            AxisAngleRotation3D rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (double)v.AngleZ);
            AxisAngleRotation3D rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), (double)v.AngleY);
            AxisAngleRotation3D rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), (double)v.AngleX);

            if (v.ModelType == 3)
            {
                RotateTransform3D rtz = new RotateTransform3D(rotationZ, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));
                RotateTransform3D rty = new RotateTransform3D(rotationY, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));
                RotateTransform3D rtx = new RotateTransform3D(rotationX, new Point3D(v.Center.X - v.Position.X, v.Center.Y - v.Position.Y, v.Center.Z - v.Position.Z));
                trans.Children.Add(rtz);
                trans.Children.Add(rty);
                trans.Children.Add(rtx);
            }
            else
            {
                RotateTransform3D rtz = new RotateTransform3D(rotationZ, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
                RotateTransform3D rty = new RotateTransform3D(rotationY, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
                RotateTransform3D rtx = new RotateTransform3D(rotationX, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
                trans.Children.Add(rtz);
                trans.Children.Add(rty);
                trans.Children.Add(rtx);
            }

            v.OutLineSizeTextTransform3D = trans;
            return trans;

        }
        private Transform3DGroup TextTransformAngle(Voxel v, Point3D center)
        {

            Transform3DGroup trans = new Transform3DGroup();
            AxisAngleRotation3D rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (double)v.AngleZ);
            AxisAngleRotation3D rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), (double)v.AngleY);
            AxisAngleRotation3D rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), (double)v.AngleX);

            RotateTransform3D rtz = new RotateTransform3D(rotationZ, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
            RotateTransform3D rty = new RotateTransform3D(rotationY, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
            RotateTransform3D rtx = new RotateTransform3D(rotationX, new Point3D(v.Center.X, v.Center.Y, v.Center.Z));
            trans.Children.Add(rtz);
            trans.Children.Add(rty);
            trans.Children.Add(rtx);
            v.OutLineSizeTextTransform3D = trans;
            return trans;

        }
        #endregion

        #region ======UPDATEMODEL=====
        public void UpdateModel()
        {
            Model.Children.Clear();
            ControllerModel.Children.Clear();
            ModelToVoxel.Clear();
            ResizeXModelToVoxel.Clear();
            ResizeYModelToVoxel.Clear();
            RotateModelToVoxel.Clear();
            RemovePreviousOutLine();

            foreach (var v in Voxels)
            {
                if (!v.Visible)
                {
                    var m = CreateModel(v);
                    m.Transform = ModelTransformAngle(v);

                    if (v.Selected)
                    {
                        var resizeballx = CreateResizeXBallModel(v);
                        resizeballx.Transform = v.Transform3D;
                        ControllerModel.Children.Add(resizeballx);
                        ResizeXModelToVoxel.Add(resizeballx, v);

                        var rotateball = CreateRotateBallModel(v);
                        rotateball.Transform = v.Transform3D;
                        ControllerModel.Children.Add(rotateball);
                        RotateModelToVoxel.Add(rotateball, v);

                        if (v.ModelType == 2)
                        {
                            var rby = CreateResizeYBallModel(v);
                            rby.Transform = v.Transform3D;
                            ControllerModel.Children.Add(rby);
                            ResizeYModelToVoxel.Add(rby, v);
                        }

                        if (IsShowSizeText)
                        {
                            CreateSizeText(v);
                        }
                    }
                    switch (_ShowLine)
                    {
                        case 0: break;
                        case 1: CreateOutLine(v); break;
                        case 2: TransparentMaterial(m, v); break;
                    }
                    Model.Children.Add(m);
                    ModelToVoxel.Add(m, v);
                }
                OnPropertyChanged("Model");
                OnPropertyChanged("ControllerModel");
            }
        }
        
        #endregion

        #region Create모델

        private GeometryModel3D CreateModel(Voxel v)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();
            if (v.ModelType != 3)
            {
                mb.AddBox(new Point3D((v.Width / 2), (v.Height / 2), (v.Depth / 2)), v.Width, v.Height, v.Depth);
            }
            else
            {
                mb.AddCone(new Point3D(0, v.Height / 2, v.Depth / 2), new Vector3D(1, 0, 0), v.Height / 2, v.Height / 2, v.Width, true, true, 10);
            }

            m.Geometry = mb.ToMesh();
            
            v.Points = mb.ToMesh().Positions;

            if (v.Selected)
            {
                m.Material = MaterialHelper.CreateMaterial(Colors.OrangeRed, 0.5);
            }
            else
            {
                if(v.TextureName.Equals("Nan"))
                {
                    m.Material = new DiffuseMaterial(new SolidColorBrush(Colors.SaddleBrown));
                }
                else
                {
                    var path = @"..\..\Texture\" + v.TextureName + ".png";
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(path, UriKind.Relative);
                    bi.EndInit();
                    m.Material = MaterialHelper.CreateImageMaterial(bi, 1, false);
                }
            }
            m.Transform = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);
            return m;
        }
        private GeometryModel3D CreateResizeXBallModel(Voxel v)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();
            double size;
            if (doit.IsZoomBinding)
            {
                size = TopCamera.Width / 100;
            }
            else
            {
                size = v.ZoomScaling / 100;
            }
            mb.AddSphere(new Point3D(v.Width, v.Height / 2, v.Depth / 2), size,10,3);
            m.Geometry = mb.ToMesh();
            m.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            m.Transform = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);

            return m;
        }
        private GeometryModel3D CreateResizeYBallModel(Voxel v)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();

            double size;
            if (doit.IsZoomBinding)
            {
                size = TopCamera.Width / 100;
            }
            else
            {
                size = v.ZoomScaling / 100;
            }
            mb.AddSphere(new Point3D(v.Width / 2, v.Height, v.Depth / 2), size, 10, 3);
            m.Geometry = mb.ToMesh();
            m.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            m.Transform = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);

            return m;
        }
        private GeometryModel3D CreateRotateBallModel(Voxel v)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();
            
            double size;
            if (doit.IsZoomBinding)
            {
                size = TopCamera.Width / 100;
            }
            else
            {
                size = v.ZoomScaling / 100;
            }
            mb.AddSphere(new Point3D(v.Width + 2, v.Height / 2, v.Depth / 2), size, 10, 3);
            m.Geometry = mb.ToMesh();
            m.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            m.Transform = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);

            return m;
        }
        private void CreateSizeText(Voxel v)
        {
            //main
            //width 
            BillboardTextVisual3D btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X + v.Width / 2, v.Position.Y - 2, v.Position.Z);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Width * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);


            MainlinesVisuals.Add(btv3d);
            main.Children.Add(btv3d);

            //Height
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X - 2, v.Position.Y + v.Height / 2, v.Position.Z);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Height * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            MainlinesVisuals.Add(btv3d);
            main.Children.Add(btv3d);

            //Depth
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X + v.Width / 2, v.Position.Y + v.Height + 2, v.Position.Z);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Depth * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            MainlinesVisuals.Add(btv3d);
            main.Children.Add(btv3d);
            //top
            //width 
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X + v.Width / 2, v.Position.Y - 2, v.Position.Z+1);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Width * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            ToplinesVisuals.Add(btv3d);
            top.Children.Add(btv3d);

            //Height
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X - 2, v.Position.Y + v.Height / 2, v.Position.Z+1);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Height * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            ToplinesVisuals.Add(btv3d);
            top.Children.Add(btv3d);

            //front
            //width 
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X + v.Width / 2, v.Position.Y+1, v.Position.Z - 2);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Width * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            FrontlinesVisuals.Add(btv3d);
            front.Children.Add(btv3d);

            //Depth
            btv3d = new BillboardTextVisual3D();
            btv3d.Position = new Point3D(v.Position.X - 2, v.Position.Y + v.Height+1 / 2, v.Position.Z);
            //btv3d.Background = Brushes.Transparent;
            btv3d.Foreground = Brushes.Black;
            btv3d.FontSize = 15;
            btv3d.Text = string.Format("{0} mm", Math.Round(v.Depth * 100, 0));
            btv3d.Transform = TextTransformAngle(v, btv3d.Position);

            FrontlinesVisuals.Add(btv3d);
            front.Children.Add(btv3d);

        }
        private void CreateOutLine(Voxel v)
        {
                LinesVisual3D line = new LinesVisual3D();

                line.Thickness = 1;
                line.Transform = LineTransformAngle(v);
                if (v.ModelType == 3)
                {
                    line.Points = v.Points;
                    Transform3DGroup trans = LineTransformAngle(v);
                    trans.Children.Add(new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z));
                    line.Transform = trans;
                    line.Thickness = 0.3;
                }
                else
                    line.Points = v.CalculateVertex();
                line.Color = Colors.Black;
                MainlinesVisuals.Add(line);
                main.Children.Add(line);
                //TopView OutLine Created
                line = new LinesVisual3D();
                line.Thickness = 1;
                line.Transform = LineTransformAngle(v);
                if (v.ModelType == 3)
                {
                    line.Points = v.Points;
                    Transform3DGroup trans = LineTransformAngle(v);
                    trans.Children.Add(new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z));
                    line.Transform = trans;
                    line.Thickness = 0.3;
                }
                else
                    line.Points = v.CalculateVertex();
                line.Color = Colors.Red;
                ToplinesVisuals.Add(line);
                top.Children.Add(line);

                //FrontView OutLine Created
                line = new LinesVisual3D();
                line.Thickness = 1;
                line.Transform = LineTransformAngle(v);
                if (v.ModelType == 3)
                {
                    line.Points = v.Points;
                    Transform3DGroup trans = LineTransformAngle(v);
                    trans.Children.Add(new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z));
                    line.Transform = trans;
                    line.Thickness = 0.3;
                }
                else
                    line.Points = v.CalculateVertex();
                line.Color = Colors.Red;
                FrontlinesVisuals.Add(line);
                front.Children.Add(line);

                //RightView OutLine Created
                line = new LinesVisual3D();
                line.Thickness = 1;
                line.Transform = LineTransformAngle(v);
                if (v.ModelType == 3)
                {
                    line.Points = v.Points;
                    Transform3DGroup trans = LineTransformAngle(v);
                    trans.Children.Add(new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z));
                    line.Transform = trans;
                    line.Thickness = 0.3;
                }
                else
                    line.Points = v.CalculateVertex();
                line.Color = Colors.Red;
                RightlinesVisuals.Add(line);
                right.Children.Add(line);
            
        }
        #endregion

        #region 재료 정보창 관련

        #region EditControl 초기화 InitEditControl()
        public void InitEditControl()
        {
            Name = null;
            Width = 0;
            Height = 0;
            Length = 0;
            Diameter = 0;
            SelectedVoxel = 9999;
        }
        #endregion

        public void ChangeComboBox(string Pname, int ModelType)
        {
            List<string> testList = new List<string>();
            Product[] temp = null;
            if (ModelType == 1)
            {
                temp = MyModel.GetInstance().Get_RectangleList();
            }
            else if (ModelType == 2)
            {
                temp = MyModel.GetInstance().Get_BoardList();
            }

            for (short a = 0; a < temp.Length; a++)
            {
                if (Pname == temp[a].Name)
                {
                    if (!testList.Contains(temp[a].Texture))
                    {
                        testList.Add(temp[a].Texture);
                    }
                }
            }
            AddTextureComboBox[] testAry = new AddTextureComboBox[testList.Count];
            for (short a = 0; a < testAry.Length; a++)
            {
                testAry[a] = new AddTextureComboBox(testList[a], MyModel.GetInstance().GetImage(testList[a]));
            }
            All = testAry;
        }
        public void SelectedItemSetting(string Texture)
        {
            for (short a = 0; a < All.Length; a++)
            {
                if (All[a].ImgName == Texture)
                {
                    SelectedIndex = a;
                    break;
                }
            }
        }
        private void UpdateAngle(int type, int angle)
        {
            foreach (var v in Voxels)
            {
                if (v.Selected == true)
                {
                    if (type == 1)
                    {
                        v.AngleX = angle;
                        break;
                    }
                    else if (type == 2)
                    {
                        v.AngleY = angle;
                        break;
                    }
                    else
                    {
                        v.AngleZ = angle;
                        break;
                    }
                }
            }
            UpdateModel();
            SettingEditControl();
            DataGridUpdate();
        }
        public void DataGridUpdate()
        {
            Product_List.Clear();
            int count = 1;
            foreach (Voxel v in Voxels)
            {
                ProductForDataGrid p = new ProductForDataGrid();
                p.Name = v.ProductName;
                p.Length = Math.Round(v.Width * 100, 0);
                p.Num = count;
                p.CompareNum = v.CompareToList;
                p.IsSelected = v.Visible;
                count++;
                Product_List.Add(p);
                if(v.Selected)
                {
                    p.SelectedVoxel = true;
                }
            }
        }
        public void SettingEditControl()
        {
            foreach (Voxel v in Voxels)
            {

                if (v.Selected == true)
                {
                    Name = v.ProductName;
                    Width = Math.Round(v.Height * 100);
                    Height = v.Depth * 100;
                    Length = Math.Round(v.Width * 100, 0);
                    AngleX = v.AngleX;
                    AngleY = v.AngleY;
                    AngleZ = v.AngleZ;
                }

            }
        }
        public void UpdateEditControl()
        {
            foreach (Voxel v in Voxels)
            {

                if (v.Selected == true)
                {
                    Name = v.ProductName;
                    Width = Math.Round(v.Height * 100);
                    Height = v.Depth * 100;
                    Length = Math.Round(v.Width * 100, 0);
                    AngleX = v.AngleX;
                    AngleY = v.AngleY;
                    AngleZ = v.AngleZ;
                    LenghthEnabled = true;
                    WidthEnabled = false;

                    if (v.ModelType == 1)
                    {
                        //All = ForTexture.RectangleCombo;
                        ChangeComboBox(v.ProductName, v.ModelType);
                    }
                    if (v.ModelType == 2)
                    {
                        WidthEnabled = true;
                        // All = ForTexture.BoardCombo;
                        ChangeComboBox(v.ProductName, v.ModelType);

                    }
                    if (v.ModelType == 3)
                    {
                        Diameter = v.Height * 100;

                        List<string> testList = new List<string>();
                        Product[] temp = MyModel.GetInstance().Get_CylinderList();

                        for (short a = 0; a < temp.Length; a++)
                        {
                            if (v.ProductName == temp[a].Name)
                            {
                                if (!testList.Contains(temp[a].Texture))
                                {
                                    testList.Add(temp[a].Texture);
                                }
                            }
                        }
                        if (Length > 800)
                        {
                            testList.Remove("레드파인");
                        }
                        AddTextureComboBox[] testAry = new AddTextureComboBox[testList.Count];
                        for (short a = 0; a < testAry.Length; a++)
                        {
                            testAry[a] = new AddTextureComboBox(testList[a], MyModel.GetInstance().GetImage(testList[a]));
                        }
                        All = testAry;

                    }
                    SelectedVoxel = v.CompareToList;
                    SelectedItemSetting(v.TextureName);

                    break;
                }
            }
            // DataGridUpdate();
        }
        #endregion

        #region 메소드

        private void AddVoxel(Product p)
        {
            Point3D pRound = new Point3D(1, 1, 0);
            double width = p.SelectedLength / (double)ratio.depth;
            double height = p.Width / (double)ratio.width;
            double depth = p.Height / (double)ratio.hegith;
            int type = p.ModelType;

            foreach (var v in Voxels)
            {
                v.ShowDetail = false;
                v.Selected = false;
            }

            string TextureName = "Nan";

            Voxel temp = new Voxel(pRound, p.ModelType, width, height, depth);
            temp.ModelType = p.ModelType;
            temp.TextureName = TextureName;
            temp.ShowDetail = true;
            temp.Selected = true;
            temp.MaxLength = p.MaxLength;
            temp.MaxWidth = p.MaxWidth;

            if (p.ModelType == 1)
            {
                string WidthForName = p.Width.ToString();
                string HeightForName = p.Height.ToString();
                
                //string name = string.Format(WidthForName + "x" + HeightForName);
                temp.ProductName = p.Name;
            }
            else if (p.ModelType == 2)
            {
                string name = string.Format(p.Height.ToString() + "T");
                temp.ProductName = p.Name;
                temp.Height = p.SelectedWidth / (double)ratio.hegith;
            }
            else
            {
                string name = string.Format("Ø" + p.Diameter.ToString());
                temp.ProductName = p.Name;
                temp.Height = (p.Diameter / (double)ratio.diameter);
                temp.Depth = (p.Diameter / (double)ratio.diameter);

            }
            temp.CompareToList = ++ProductNum;
            Voxels.Add(temp);

            UpdateModel();
            DataGridUpdate();
            UpdateEditControl();
            PreViewHandler(Voxels);
        }
        private void RemovePreviousOutLine()
        {
            foreach (var line in MainlinesVisuals)
            {
                main.Children.Remove(line);
            }
            foreach (var line in ToplinesVisuals)
            {
                top.Children.Remove(line);
            }
            foreach (var line in FrontlinesVisuals)
            {
                front.Children.Remove(line);
            }
            foreach (var line in RightlinesVisuals)
            {
                right.Children.Remove(line);
            }
            MainlinesVisuals.Clear();
            ToplinesVisuals.Clear();
            FrontlinesVisuals.Clear();
            RightlinesVisuals.Clear();
        }
        internal Voxel GetVoxel(Model3D source)
        {
            if (!ModelToVoxel.ContainsKey(source))
                return null;
            Voxel v = ModelToVoxel[source];
            return v;
        }
        internal Voxel GetResizeXVoxel(Model3D source)
        {
            if (!ResizeXModelToVoxel.ContainsKey(source))
                return null;
            Voxel v = ResizeXModelToVoxel[source];
            return v;
        }
        internal Voxel GetResizeYVoxel(Model3D source)
        {
            if (!ResizeYModelToVoxel.ContainsKey(source))
                return null;
            Voxel v = ResizeYModelToVoxel[source];
            return v;
        }
        internal Voxel GetRoateVoxel(Model3D source)
        {
            if (!RotateModelToVoxel.ContainsKey(source))
                return null;
            Voxel v = RotateModelToVoxel[source];
            return v;
        }
        public void Move(Voxel moving, Point Gap)
        {
            UpdateModel();
        }
        public void Remove(Model3D model)
        {
            if (!ModelToVoxel.ContainsKey(model))
                return;
            var v = ModelToVoxel[model];
            Voxels.Remove(v);
            UpdateModel();
            DataGridUpdate();
        }
        public void Clear()
        {
            Voxels.Clear();
            UpdateModel();
            DataGridUpdate();
        }
        public void Copying()
        {
            ClipBoardVoxels.Clear();
            foreach (var v in Voxels)
            {
                if (v.Selected)
                {
                    Voxel t = new Voxel(new Point3D(v.Position.X, v.Position.Y, v.Position.Z), v.ModelType, v.Width, v.Height, v.Depth);
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
                    ClipBoardVoxels.Add(t);
                }
            }
        }
        public void Cut()
        {
            ClipBoardVoxels.Clear();
            List<Voxel> temp = new List<Voxel>();
            foreach (var v in Voxels)
            {
                if (v.Selected)
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
                    ClipBoardVoxels.Add(t);
                    temp.Add(v);
                }
            }
            foreach (var v in temp)
            {
                if (Voxels.Contains(v))
                {
                    Voxels.Remove(v);
                }
            }
            UpdateModel();
            DataGridUpdate();
        }
        public void Paste()
        {

            List<Voxel> temp = new List<Voxel>();
            foreach (var tv in ClipBoardVoxels)
            {
                temp.Add(tv);
            }
            ClipBoardVoxels.Clear();
            foreach (var v in temp)
            {
                Voxel t = new Voxel(new Point3D(v.Position.X, v.Position.Y, v.Position.Z), v.ModelType, v.Width, v.Height, v.Depth);
                t.Transform3D = v.Transform3D;
                t.OutLineSizeTextTransform3D = v.OutLineSizeTextTransform3D;
                t.Center = v.Center;
                t.ProductName = v.ProductName;
                t.CompareToList = ++ProductNum;
                t.AngleX = v.AngleX;
                t.AngleY = v.AngleY;
                t.AngleZ = v.AngleZ;
                t.AxisType = v.AxisType;
                t.TextureName = v.TextureName;
                t.BitmapMaterial = v.BitmapMaterial;
                t.ProductName = v.ProductName;
                t.MaxLength = v.MaxLength;
                t.MaxWidth = v.MaxWidth;
                Voxels.Add(t);
                ClipBoardVoxels.Add(t);
            }
            UpdateModel();
            DataGridUpdate();
        }
        private void TransparentMaterial(GeometryModel3D m, Voxel v)
        {
            if (!v.Selected)
            {
                m.Material = new DiffuseMaterial(Brushes.Transparent);
            }
            CreateOutLine(v);
        }
        public void SelectedRelease()
        {
            foreach (var v in Voxels)
            {
                v.Selected = false;
            }
        }
        #endregion

        #region Bindding MaterialEditcontrol

        #region Name
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion

        #region Width
        private double _Width;
        public double Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (_Width != value)
                {

                    _Width = value;
                    OnPropertyChanged("Width");
                    LengthSizeUp();
                }
            }
        }
        #endregion

        #region Height
        private double _Height;
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    OnPropertyChanged("Height");
                }
            }
        }
        #endregion


        #region Length
        private double _Lenght;
        public double Length
        {
            get
            {
                return _Lenght;
            }
            set
            {

                if (_Lenght != value)
                {
                    _Lenght = value;
                    OnPropertyChanged("Length");
                    WidthSizeUp();
                }
            }
        }
        private void WidthSizeUp()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                return;
            }
            foreach (Voxel v in Voxels)
            {
                if (v.Selected == true)
                {
                    v.Width = _Lenght / (double)ratio.depth;
                    if (v.ModelType == 3 && v.Width > 8)
                    {
                        for (short a = 0; a < All.Length; a++)
                        {
                            if (All[a].ImgName == "레드파인")
                            {

                            }
                        }
                    }
                }
            }
            UpdateModel();
        }
        private void LengthSizeUp()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                return;
            }
            foreach (Voxel v in Voxels)
            {
                if (v.Selected == true)
                {
                    v.Height = _Width / (double)ratio.hegith;
                }
            }
            UpdateModel();
        }

        private void VoxelSizeUp()
        {
            foreach (Voxel v in Voxels)
            {
                if (v.ShowDetail == true)
                {
                    v.Width = _Lenght / (double)ratio.depth;
                    if (v.ModelType == 2)
                    {
                        v.Height = _Width / (double)ratio.hegith;
                    }
                }
            }
            UpdateModel();
        }


        #endregion

        #region Diameter
        private double _Diameter;
        public double Diameter
        {
            get
            {
                return _Diameter;
            }
            set
            {
                _Diameter = value;
                OnPropertyChanged("Diameter");
            }
        }
        #endregion

        #region ImageSource
        private ImageSource _ImgSource;
        public ImageSource ImgSource
        {
            get
            {
                return _ImgSource;
            }
            set
            {
                _ImgSource = value;
            }
        }
        #endregion
        #region     ImageName

        private string _ImgName;
        public string ImgName
        {
            get
            {
                return _ImgName;
            }
            set
            {
                _ImgName = value;
            }
        }
        #endregion

        #region All
        public AddTextureComboBox[] All
        {
            get
            {

                return ForTexture.All;
            }
            set
            {
                ForTexture.All = value;
                OnPropertyChanged("All");
            }

        }
        #endregion

        #region AngleX
        private double _AngleX;
        public double AngleX
        {
            get
            {
                return _AngleX;
            }
            set
            {
                if (_AngleX != value)
                {
                    _AngleX = value;
                    OnPropertyChanged("AngleX");
                }
            }
        }
        #endregion

        #region AngleY
        private double _AngleY;
        public double AngleY
        {
            get
            {
                return _AngleY;
            }
            set
            {
                if (_AngleY != value)
                {
                    _AngleY = value;
                    OnPropertyChanged("AngleY");
                }
            }
        }
        #endregion

        #region AngleZ
        private double _AngleZ;
        public double AngleZ
        {
            get
            {
                return _AngleZ;
            }
            set
            {
                if (_AngleZ != value)
                {
                    _AngleZ = value;
                    OnPropertyChanged("AngleZ");
                }
            }
        }
        #endregion

        #region NameEnabled
        private bool _NameEnabled;
        public bool NameEnabled
        {
            get
            {
                return _NameEnabled;
            }
            set
            {
                if (value != _NameEnabled)
                {
                    _NameEnabled = value;
                    OnPropertyChanged("NameEnabled");
                }
            }
        }
        #endregion

        #region WidthEnabled
        private bool _WidthEnabled;
        public bool WidthEnabled
        {
            get
            {
                return _WidthEnabled;
            }
            set
            {
                if (value != _WidthEnabled)
                {
                    _WidthEnabled = value;
                    OnPropertyChanged("WidthEnabled");
                }
            }
        }
        #endregion

        #region HeightEnabled
        private bool _HeightEnabled;
        public bool HeightEnabled
        {
            get
            {
                return _WidthEnabled;
            }
            set
            {
                if (value != _HeightEnabled)
                {
                    _HeightEnabled = value;
                    OnPropertyChanged("HeightEnabled");
                }
            }
        }
        #endregion

        #region LenghthEnabled
        private bool _LenghthEnabled;
        public bool LenghthEnabled
        {
            get
            {
                return _LenghthEnabled;
            }
            set
            {
                if (value != _LenghthEnabled)
                {
                    _LenghthEnabled = value;
                    OnPropertyChanged("LenghthEnabled");
                }
            }
        }
        #endregion

        #region DiameterEnabled
        private bool _DiameterEnabled;
        public bool DiameterEnabled
        {
            get
            {
                return _DiameterEnabled;
            }
            set
            {
                if (value != DiameterEnabled)
                {
                    _DiameterEnabled = value;
                    OnPropertyChanged("DiameterEnabled");
                }
            }
        }
        #endregion

        #endregion

        #region SelectedTextureItem
        private AddTextureComboBox _SelectedTextureItem;
        public AddTextureComboBox SelectedTextureItem
        {
            get
            {
                return _SelectedTextureItem;
            }
            set
            {
                _SelectedTextureItem = value;
                OnPropertyChanged("SelectedTextureItem");
                foreach (Voxel v in Voxels)
                {
                    if (v.Selected&&SelectedTextureItem !=null)
                    {
                        v.TextureName = SelectedTextureItem.ImgName;
                        v.BitmapMaterial = MyModel.GetInstance().GetImage(v.TextureName);
                    }
                }
                UpdateModel();
            }
        }

        private void ChangeTexture()
        {
            if (SelectedTextureItem == null) return;
            foreach (Voxel v in Voxels)
            {
                if (v.CompareToList == SelectedVoxel)
                {
                    v.TextureName = SelectedTextureItem.ImgName;
                    v.BitmapMaterial = MyModel.GetInstance().GetImage(v.TextureName);
                    UpdateModel();
                }
            }
        }
        #endregion

        #region SelectedIndex;
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }
        #endregion

        #region DataGridBinding
        #region Product_List
        private ObservableCollection<ProductForDataGrid> _Product_List = new ObservableCollection<ProductForDataGrid>();
        public ObservableCollection<ProductForDataGrid> Product_List
        {
            get
            {
                return _Product_List;
            }
        }
        #endregion

        #region SelectedItem
        private ProductForDataGrid _SelectedItem;
        public ProductForDataGrid SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
                OnPropertyChanged("SelectedItem");
                foreach (var v in Voxels)
                {
                    if (v.CompareToList == SelectedItem.CompareNum)
                    {
                        v.Visible = !SelectedItem.IsSelected;
                    }
                }
                UpdateModel();
                DataGridUpdate();
            }
        }

        #endregion

        #region IsAllItemsSelected
        private bool _IsAllItemsSelected;
        private DoItYourSlefControl doit;
        internal string nowPath;
        private GridLines gridlines;

        public bool IsAllItemsSelected
        {
            get
            {
                return _IsAllItemsSelected;
            }
            set
            {
                _IsAllItemsSelected = value;
                OnPropertyChanged("IsAllItemsSelected");
                if (value == true)
                {
                    foreach (var iter in Voxels)
                    {
                        iter.Visible = true;
                    }
                }
                else
                {
                    foreach (var iter in Voxels)
                    {
                        iter.Visible = false;
                    }
                }
                UpdateModel();
                DataGridUpdate();
            }
        }
        #endregion

        #endregion

        #region RectSelelctor

        public SelectionHitMode SelectionMode
        {
            get
            {
                return this.RectangleSelectionCommand.SelectionHitMode;
            }

            set
            {
                this.RectangleSelectionCommand.SelectionHitMode = value;
            }
        }
        public IEnumerable<SelectionHitMode> SelectionModes
        {
            get
            {
                return Enum.GetValues(typeof(SelectionHitMode)).Cast<SelectionHitMode>();
            }
        }
        private void HandleSelectionEvent(object sender, ModelsSelectedEventArgs args)
        {
            this.selectedModels = args.SelectedModels;

            var rectangleSelectionArgs = args as ModelsSelectedByRectangleEventArgs;
            if (rectangleSelectionArgs != null)
            {
                this.ChangeMaterial(this.selectedModels, rectangleSelectionArgs.Rectangle.Size != default(Size) ? Materials.Red : Materials.Green);
            }

        }
        private void ChangeMaterial(IEnumerable<Model3D> models, Material material)
        {
            if (models == null)
            {
                return;
            }
            foreach (var model in selectedModels)
            {
                Voxel selectv = GetVoxel(model);
                if (selectv == null) continue;
                foreach (var v in Voxels)
                {
                    if (v.Equals(selectv))
                    {
                        v.Selected = true;
                        break;
                    }
                }
            }
            UpdateModel();
        }

        #endregion
    }
}
