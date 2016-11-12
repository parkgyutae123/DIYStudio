using HelixToolkit.Wpf;
using MVVMBase.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace SF_DIY
{
    class SimulationFurnitureViewModel : ViewModelBase
    {
        #region 필드
        private XmlSerializer serializer;
        private List<Visual3D> MainlinesVisuals = new List<Visual3D>();
        private HelixViewport3D main;
        #endregion
        #region 속성
        private ObservableCollection<Voxel> _Voxels;
        public ObservableCollection<Voxel> Voxels
        {
            get
            {
                return _Voxels;
            }
            set
            {
                _Voxels = value;
                OnPropertyChanged("Voxels");
            }
        }
        public Dictionary<Model3D, Voxel> ModelToVoxel { get; set; }
        public Model3DGroup Model { get; set; }
        public Dispatcher Dispatcher { get; set; }
        #endregion
        #region 생성자
        public SimulationFurnitureViewModel(HelixViewport3D main)
        {
            ModelToVoxel = new Dictionary<Model3D, Voxel>();
            Model = new Model3DGroup();
            this.main = main;
            this.Dispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
        #region Save
        public void Save()
        {
            if(nowPath!=null)
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
        #endregion
        #region SelectedItem
        internal void SelectedItem(string fileName)
        {
            try
            {
                {
                    using (var r = XmlReader.Create(fileName))
                    {
                        serializer = new XmlSerializer(typeof(ObservableCollection<Voxel>), new[] { typeof(Voxel) });
                        var v = serializer.Deserialize(r);
                        Voxels = v as ObservableCollection<Voxel>;
                        nowPath = fileName;
                    }
                    UpdateModel();
                    TotalVoxelCount = Voxels.Count;
                    main.Camera.LookAt(new Point3D((Model.Bounds.X+Model.Bounds.SizeX)/2, (Model.Bounds.Y + Model.Bounds.SizeY) / 2, (Model.Bounds.Z + Model.Bounds.SizeZ) / 2), 1000);
                    TimerClear();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("손상된 파일이거나 형식이 맞지 않는 파일 입니다. \n{0}", e.Message);
            }
        }
        #endregion
        #region SelectedVoxelItem
        private Voxel _SelectedVoxelItem;
        public Voxel SelectedVoxelItem
        {
            get
            {
                return _SelectedVoxelItem;
            }
            set
            {
                if(_SelectedVoxelItem != value)
                {
                    _SelectedVoxelItem = value;
                    SelectedRelease();
                    foreach (var v in Voxels)
                    {
                        if (v.Equals(_SelectedVoxelItem))
                        {
                            v.Selected = true;
                        }
                    }
                    //UpdateModel();
                    OnPropertyChanged("SelectedVoxelItem");
                }
            }
        }
        #endregion
        #region Time
        private double _Time = 1 ;
        public double Time
        {
            get
            {
                return _Time;
            }
            set
            {
                _Time = value;
                if(tmr!=null)
                {
                    tmr.Interval = TimeSpan.FromSeconds(_Time);
                }
                OnPropertyChanged("Time");
            }
        }

        #endregion

        #region ================UpdateModel=================
        public void UpdateModel()
        {
            Model.Children.Clear();
            ModelToVoxel.Clear();
            RemovePreviousOutLine();
            foreach (var v in Voxels)
            {
                var m = CreateModel(v);
                m.Transform = ModelTransformAngle(v);
                CreateOutLine(v);
                Model.Children.Add(m);
                ModelToVoxel.Add(m, v);
            }
            OnPropertyChanged("Model");
        }
        #endregion

        #region 메소드
        internal Voxel GetVoxel(Model3D source)
        {
            if (!ModelToVoxel.ContainsKey(source))
                return null;
            Voxel v = ModelToVoxel[source];
            return v;
        }

        public void SelectedRelease()
        {
            foreach (var v in Voxels)
            {
                v.Selected = false;
            }
        }

        internal void UpdateEditControl()
        {
        }
        #endregion

        #region CreateOutLine
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
        }



        #endregion
        #region RemoveOutLine
        private void RemovePreviousOutLine()
        {
            foreach (var line in MainlinesVisuals)
            {
                main.Children.Remove(line);
            }
            MainlinesVisuals.Clear();
        }
        #endregion
        #region CreateModel
        private GeometryModel3D CreateModel(Voxel v)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();
            if (v.ModelType != 3)
            {
                mb.AddBox(new Point3D(v.Width / 2, v.Height / 2, v.Depth / 2), v.Width, v.Height, v.Depth);
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
                if (v.TextureName.Equals("Nan"))
                {
                    m.Material = new DiffuseMaterial(new SolidColorBrush(Colors.SaddleBrown));
                }
                else
                {
                    var path = @"\..\..\Texture\" + v.TextureName + ".png";
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
                    bi.EndInit();
                    m.Material = MaterialHelper.CreateImageMaterial(bi, 1, false);
                }
            }
            m.Transform = new TranslateTransform3D(v.Position.X, v.Position.Y, v.Position.Z);
            return m;
        }
        #endregion
        #region CreateTransform3D
        private Transform3DGroup ModelTransformAngle(Voxel v)
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
        #endregion
        #region CreateLineTransform3D
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
        #endregion
        #region ViewportClear

        public void ViewportClear()
        {
            Model.Children.Clear();
            ModelToVoxel.Clear();
            RemovePreviousOutLine();
            OnPropertyChanged("Model");
        }
        #endregion
        #region IsNullCheckVoxels
        public bool IsNullCheckVoxel()
        {
            if (Voxels != null)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region 플레이어 변수

        #region AniNum
        private int _AniNum = 0;

        public int AniNum
        {
            get
            {
                return _AniNum;
            }
            set
            {
                _AniNum = value;
                List<Voxel> tmp = new List<Voxel>();
                foreach (var v in Voxels)
                {
                    tmp.Add(v);
                }
                tmp.Sort((Voxel x, Voxel y) => x.Num.CompareTo(y.Num));
                ViewportClear();
                SelectedRelease();
                
                for (int i = 0; i < AniNum; i++)
                {
                    if (i == AniNum - 1)
                    {
                        SelectedVoxelItem = tmp[i];
                    }
                    var m = CreateModel(tmp[i]);
                    m.Transform = ModelTransformAngle(tmp[i]);
                    ModelToVoxel.Add(m, tmp[i]);
                    CreateOutLine(tmp[i]);
                    Model.Children.Add(m);
                    OnPropertyChanged("Model");
                }
                OnPropertyChanged("AniNum");
            }
        }

        #endregion
        #region TotalVoxelCount
        private int _TotalVoxelCount;
        public int TotalVoxelCount
        {
            get
            {
                return _TotalVoxelCount;
            }
            set
            {
                _TotalVoxelCount = value;
                OnPropertyChanged("TotalVoxelCount");
            }
        }
        #endregion

        private DispatcherTimer tmr;
        private string nowPath;

        #endregion
        #region TimerClear
        public void TimerClear()
        {
            if (tmr != null)
            {
                AniNum = 0;
                tmr.Stop();
                tmr = null;
            }
        }
        #endregion
        #region Play
        public void Play()
        {
            if(tmr==null)
            {
                tmr = new DispatcherTimer();
                ViewportClear();
                tmr.Interval = TimeSpan.FromSeconds(Time);
                tmr.Tick += Tmr_Tick;
                tmr.Start();
                return;
            }
            if(tmr.IsEnabled==false)
            {
                tmr.Start();
            }
        }
        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (AniNum == Voxels.Count)
            {
                Reset();
            }
            else
            {
                AniNum++;
            }
        }
        
        #endregion
        #region Forward
        public void Forward()
        {
            if (AniNum >= Voxels.Count)
            {
                return;
            }
            ViewportClear();
            AniNum++;
        }
        #endregion
        #region Backward
        public void Backward()
        {
            if(AniNum == 0)
            {
                return;
            }

            ViewportClear();
            AniNum--;
        }
        #endregion
        #region Pause
        internal void Pause()
        {
            if (tmr != null)
                tmr.Stop();
        }
        #endregion
        #region Reset
        public void Reset()
        {
            TimerClear();

        }
        #endregion
    }
}
