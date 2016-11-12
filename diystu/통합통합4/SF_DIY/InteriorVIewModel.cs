using HelixToolkit.Wpf;
using MVVMBase.ViewModel;
using SF_DIY.Common.Behaviors;
using SF_DIY.Interfaces;
using SF_Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Serialization;

namespace SF_DIY
{
    class InteriorVIewModel : ViewModelBase
    {
        #region 필드

        public static Action UndoRedoHandler = null;

        private Grid DesignerPane;
        private HelixViewport3D viewportTop;
        private HelixViewport3D viewport;
        private List<Visual3D> OutLineWallVisuals = new List<Visual3D>();
        private List<Visual3D> OutLineFloorVisuals = new List<Visual3D>();
        private List<Visual3D> OutLineVirtualPersonVisuals = new List<Visual3D>();
        private const string OpenFileFilter = "Sketch XML Files (*.xml)|*.xml";
        private readonly IFileDialogService fileDialogService;

        private List<Visual3D> MainlinesVisuals = new List<Visual3D>();

        public List<Visual3D> SelectedVisuals = new List<Visual3D>();
        public List<Visual3D> LeftControllerVisuals = new List<Visual3D>();
        public List<Visual3D> RightControllerVisuals = new List<Visual3D>();
        public List<Visual3D> FloorControllerVisuals = new List<Visual3D>();
        public List<Visual3D> FurnitureRotateControllerVisuals = new List<Visual3D>();
        public List<Visual3D> FurnitureWidthHightControllerVisuals = new List<Visual3D>();
        public List<Visual3D> FurnitureDepthControllerVisuals = new List<Visual3D>();
        public List<Visual3D> FurnitureLocationZControllerVisuals = new List<Visual3D>();

        private IList<Model3D> selectedModels;
        private BitmapImage expandicon;
        private BitmapImage rotateicon;
        private BitmapImage bottomicon;
        private BitmapImage verticalicon;
        private BitmapImage observericon;

        #endregion

        #region 속성
        public bool DragArrangement { get; set; }
        public Furniture DragArrangementFurnitrue { get; set; }
        public List<Wall> Walls { get; set; }
        public List<Floor> Floors { get; set; }
        public List<Furniture> Furnitures { get; set; }
        public List<VirtualPerson> VirtualPersonList { get; set; }
        public List<Wall> ClipBoardWalls { get; set; }
        public List<Floor> ClipBoardFloors { get; set; }
        public List<Furniture> ClipBoardFurnitures { get; set; }
        public Model3DGroup WallModel { get; set; }
        public Model3DGroup FloorModel { get; set; }
        public Model3DGroup FurnitureModel { get; set; }
        public Model3DGroup VirtualPersonModel { get; set; }
        public Dictionary<Model3D, Wall> ModelToWall { get; set; }
        public Dictionary<Model3D, int> ModelToWallMaterial { get; set; }
        public Dictionary<Model3D, Floor> ModelToFloor { get; set; }
        public Dictionary<Model3D, VirtualPerson> ModelToVirtualPerson { get; set; }
        public Dictionary<Model3D, Furniture> ModelToFurniture { get; set; }
        public Dictionary<Model3D, Material> OriginlMaterial { get; set; }
        public Model3D PreviewModel { get; set; }
        public RectangleSelectionCommand RectangleSelectionTopCommand { get; private set; }
        #endregion

        #region 생성자
        public InteriorVIewModel(Grid _DesignerPane, HelixViewport3D viewportTop, HelixViewport3D viewport)
        {
            this.DesignerPane = _DesignerPane;
            this.viewportTop = viewportTop;
            this.viewport = viewport;
            this.fileDialogService = new FileDialogService();

            Floors = new List<Floor>();
            Walls = new List<Wall>();
            Furnitures = new List<Furniture>();
            VirtualPersonList = new List<VirtualPerson>();

            ClipBoardWalls = new List<Wall>();
            ClipBoardFloors = new List<Floor>();
            ClipBoardFurnitures = new List<Furniture>();

            WallModel = new Model3DGroup();
            FloorModel = new Model3DGroup();
            FurnitureModel = new Model3DGroup();
            VirtualPersonModel = new Model3DGroup();

            ModelToWall = new Dictionary<Model3D, Wall>();
            ModelToFloor = new Dictionary<Model3D, Floor>();
            ModelToFurniture = new Dictionary<Model3D, Furniture>();
            ModelToVirtualPerson = new Dictionary<Model3D, VirtualPerson>();
            
            ModelToWallMaterial = new Dictionary<Model3D, int>();

            OriginlMaterial = new Dictionary<Model3D, Material>();

            this.RectangleSelectionTopCommand = new RectangleSelectionCommand(viewportTop.Viewport, this.HandleSelectionEvent);

            MaterialTree.stringHandler += ModelName;
            CreateFurnitureControllerIcon();
            InitTextureComboBox();
        }
        public void InitTextureComboBox()
        {
            ImgeDic = new Dictionary<string, BitmapImage>();

            //이미지 초기화
            string dirPath = @"..\..\Texture\sources\";
            if (System.IO.Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                foreach (var item in di.GetFiles())
                {
                    if (item.Extension.Equals(".png"))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(item.FullName);
                        bi.EndInit();

                        ImgeDic[item.Name] = bi;
                    }
                    if (item.Extension.Equals(".jpg"))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(item.FullName);
                        bi.EndInit();

                        ImgeDic[item.Name] = bi;
                    }
                }
            }
            TextureImages = new InteriorVIewModel[ImgeDic.Count];
            int cnt = 0;
            foreach (KeyValuePair<string, BitmapImage> iter in ImgeDic)
            {
                ImageSource temp_Source = iter.Value as ImageSource;
                TextureImages[cnt] = new InteriorVIewModel(iter.Key, temp_Source);
                cnt++;
            }

        }
        public InteriorVIewModel(string imgName, ImageSource imgSource)
        {
            ImgName = imgName;
            ImgSource = imgSource;
            TextureIndex = index;
            index++;
        }
        private void CreateFurnitureControllerIcon()
        {
            string path = @"\..\..\resources\arrow-expand-all.png";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
            bi.EndInit();
            expandicon = bi;

            path = @"\..\..\resources\rotate-3d.png";
            bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
            bi.EndInit();
            rotateicon = bi;

            path = @"\..\..\resources\format-vertical-align-bottom.png";
            bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
            bi.EndInit();
            bottomicon = bi;

            path = @"\..\..\resources\swap-vertical.png";
            bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
            bi.EndInit();
            verticalicon = bi;

            path = @"\..\..\resources\observer.png";
            bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + path, UriKind.Relative);
            bi.EndInit();
            observericon = bi;

        }
        #endregion

        #region 가구
        private void ModelName(string str)
        {
            SelectModel(str);
        }
        public void SelectModel(string modelType)
        {
            switch (modelType)
            {
                case "Whirlpool": AddFurniture(@"..\..\resources\Whirlpool.3ds"); break;
                case "Bed1": AddFurniture(@"..\..\resources\Bed1.3ds"); break;
                case "Bed2": AddFurniture(@"..\..\resources\Bed2.3ds"); break;
                case "bookcase": AddFurniture(@"..\..\resources\bookcase.3ds"); break;
                case "Chair1": AddFurniture(@"..\..\resources\Chair1.3ds"); break;
                case "Chair2": AddFurniture(@"..\..\resources\Chair2.3ds"); break;
                case "clothcase": AddFurniture(@"..\..\resources\clothcase.3ds"); break;
                case "Desk1": AddFurniture(@"..\..\resources\Desk1.3ds"); break;
                case "Desk2": AddFurniture(@"..\..\resources\Desk2.3ds"); break;
                case "dishwasher": AddFurniture(@"..\..\resources\dishwasher.3ds"); break;
                case "door1": AddFurniture(@"..\..\resources\door.3ds"); break;
                case "PC1": AddFurniture(@"..\..\resources\PC1.3ds"); break;
                case "PC2": AddFurniture(@"..\..\resources\PC2.3ds"); break;
                case "Showercurtain": AddFurniture(@"..\..\resources\Showercurtain.3ds"); break;
                case "Table1": AddFurniture(@"..\..\resources\Table1.3ds"); break;
                case "Table2": AddFurniture(@"..\..\resources\Table2.3ds"); break;
                case "Table3": AddFurniture(@"..\..\resources\Table3.3ds"); break;
                case "TV1": AddFurniture(@"..\..\resources\TV1.3ds"); break;
                case "TV2": AddFurniture(@"..\..\resources\TV2.3ds"); break;
                case "Whirlpool2": AddFurniture(@"..\..\resources\Whirlpool2.3ds"); break;
                case "wineRack": AddFurniture(@"..\..\resources\wineRack.3ds"); break;
                case "냉장고": AddFurniture(@"..\..\resources\냉장고.3ds"); break;
                case "냉장고2": AddFurniture(@"..\..\resources\냉장고2.3ds"); break;
                case "냉장고3": AddFurniture(@"..\..\resources\냉장고3.3ds"); break;
                case "서랍장": AddFurniture(@"..\..\resources\서랍장.3ds"); break;
                case "세면대": AddFurniture(@"..\..\resources\세면대.3ds"); break;
                case "세면대2": AddFurniture(@"..\..\resources\세면대2.3ds"); break;
                case "세탁기": AddFurniture(@"..\..\resources\세탁기.3ds"); break;
                case "송민상": AddFurniture(@"..\..\resources\송민상.3ds"); break;
                case "에어컨": AddFurniture(@"..\..\resources\에어컨.3ds"); break;
                case "옷장": AddFurniture(@"..\..\resources\옷장.3ds"); break;
                case "전자렌지": AddFurniture(@"..\..\resources\전자렌지.3ds"); break;
                case "주방": AddFurniture(@"..\..\resources\주방.3ds"); break;
                case "변기1": AddFurniture(@"..\..\resources\변기1.3ds"); break;
                case "변기2": AddFurniture(@"..\..\resources\변기2.3ds"); break;
                case "피아노": AddFurniture(@"..\..\resources\piano.3ds"); break;
                case "라디에이터": AddFurniture(@"..\..\resources\radiator.3ds"); break;
                case "접시세트": AddFurniture(@"..\..\resources\DishSet.3ds"); break;
                case "스피커": AddFurniture(@"..\..\resources\speaker.3ds"); break;
                case "소파": AddFurniture(@"..\..\resources\sofa.3ds"); break;
                case "소파1": AddFurniture(@"..\..\resources\sofa.3ds"); break;
                case "소파2": AddFurniture(@"..\..\resources\sofa2.3ds"); break;
                case "샤워기": AddFurniture(@"..\..\resources\shower.3ds"); break;
                case "door2": AddFurniture(@"..\..\resources\door2.3ds"); break;
                case "door3": AddFurniture(@"..\..\resources\door3.3ds"); break;
                case "officeChair": AddFurniture(@"..\..\resources\officeChair.obj"); break;
                case "액자1": AddFurniture(@"..\..\resources\액자1.3ds"); break;
                case "액자2": AddFurniture(@"..\..\resources\액자2.3ds"); break;
                case "액자3": AddFurniture(@"..\..\resources\액자3.3ds"); break;
                case "선반1": AddFurniture(@"..\..\resources\shelf1.3ds"); break;
                case "선반2": AddFurniture(@"..\..\resources\shelf2.3ds"); break;
                default: AddXamlFurniture(modelType); break;
            }
            //UpdateFurnitureModel();
        }
        private void AddXamlFurniture(string path)
        {
            string ex = Path.GetExtension(path);
            if(ex.Equals(".obj"))
            {
                try
                {
                    if (DragArrangementFurnitrue == null)
                    {
                        UndoRedoHandler();
                    }
                    if (DragArrangementFurnitrue != null)
                    {
                        Furnitures.Remove(DragArrangementFurnitrue);
                        DragArrangementFurnitrue = null;
                    }
                    var mi = new ModelImporter();

                    Model3DGroup m = mi.Load(path);
                    Rect3D boundbox = m.Bounds;
                    SelectRelease();
                    string name = Path.GetFileNameWithoutExtension(path);
                    DragArrangementFurnitrue = new Furniture(name, boundbox.SizeX, boundbox.SizeY, boundbox.SizeZ, m);
                    DragArrangementFurnitrue.ModelPath = path;
                    DragArrangementFurnitrue.State = ActionType.Move;
                    DragArrangementFurnitrue.Selected = true;
                    DragArrangementFurnitrue.FurnitureNum = ++FunitureNum;
                    Furnitures.Add(DragArrangementFurnitrue);
                    DragArrangement = true;
                }
                catch 
                {
                    MessageBox.Show("파일이 손상되거나 정상적인 파일이 아닙니다.");
                }
                return;
            }
            if(ex.Equals(".xaml"))
            {
                try
                {
                    Model3DGroup mb = XamlReader.Load(XmlReader.Create(path)) as Model3DGroup;
                    if (mb != null)
                    {
                        Rect3D boundbox = mb.Bounds;
                        if (DragArrangementFurnitrue == null)
                        {
                            UndoRedoHandler();
                        }
                        if (DragArrangementFurnitrue != null)
                        {
                            Furnitures.Remove(DragArrangementFurnitrue);
                        }
                        string name = Path.GetFileNameWithoutExtension(path);
                        DragArrangementFurnitrue = new Furniture(name, boundbox.SizeX, boundbox.SizeY, boundbox.SizeZ, mb);
                        DragArrangementFurnitrue.ModelPath = path;
                        DragArrangementFurnitrue.State = ActionType.Move;
                        DragArrangementFurnitrue.FurnitureNum = ++FunitureNum;
                        Furnitures.Add(DragArrangementFurnitrue);
                        DragArrangement = true;


                    }
                }
                catch
                {
                    MessageBox.Show("파일이 손상되거나 정상적인 파일이 아닙니다.");
                }
            }
           
        }
        public void AddFurniture(string path)
        {
            if (DragArrangementFurnitrue == null)
            {
                UndoRedoHandler();
            }
            if (DragArrangementFurnitrue != null)
            {
                Furnitures.Remove(DragArrangementFurnitrue);
                DragArrangementFurnitrue = null;
            }
            var mi = new ModelImporter();

            Model3DGroup m = mi.Load(path);
            Rect3D boundbox = m.Bounds;
            SelectRelease();
            string name = Path.GetFileNameWithoutExtension(path);
            DragArrangementFurnitrue = new Furniture(name, boundbox.SizeX, boundbox.SizeY, boundbox.SizeZ, m);
            DragArrangementFurnitrue.ModelPath = path;
            DragArrangementFurnitrue.State = ActionType.Move;
            DragArrangementFurnitrue.Selected = true;
            DragArrangementFurnitrue.FurnitureNum = ++FunitureNum;
            Furnitures.Add(DragArrangementFurnitrue);
            DragArrangement = true;


        }
        public void UpdateFurnitureModel()
        {
            ModelToFurniture.Clear();
            FurnitureModel.Children.Clear();
            RemovePreviousFurnitureOutLineController();
            foreach (var f in Furnitures)
            {
                if (!f.Visible)
                {
                    Model3DGroup m = f.Model;

                    m.Transform = ModelTransform(f);

                    foreach (var el in m.Children)
                    {
                        var t = el as GeometryModel3D;
                        if (t != null)
                        {
                            f.Bounds = t.Bounds;
                            ModelToFurniture.Add(t, f);
                        }
                    }
                    if (f.Selected)
                    {
                        CreateFurnitureOutLineAndController(f);
                    }
                    FurnitureModel.Children.Add(m);
                }
            }
            OnPropertyChanged("FurnitureModel");
        }
        private Transform3D ModelTransform(Furniture f)
        {
            Transform3DGroup trans = new Transform3DGroup();
            AxisAngleRotation3D rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), (double)f.AngleZ);

            RotateTransform3D rtz = new RotateTransform3D(rotationZ);

            TranslateTransform3D tl = new TranslateTransform3D(f.Position.X, f.Position.Y, f.Position.Z);
            //ScaleTransform3D st = new ScaleTransform3D(f.ScaleX, f.ScaleY, f.ScaleZ);
            //ScaleTransform3D st = new ScaleTransform3D(f.ScaleX,f.ScaleY,f.ScaleZ,f.Model.Bounds.X,f.Model.Bounds.Y+f.Model.Bounds.SizeY,f.Model.Bounds.Z);
            ScaleTransform3D st = new ScaleTransform3D(new Vector3D(f.ScaleX, f.ScaleY, f.ScaleZ));
            trans.Children.Add(rtz);
            trans.Children.Add(st);
            trans.Children.Add(tl);
            f.Transform3DGroup = trans;
            return trans;
        }
        internal Furniture GetFurniture(Model3D source)
        {
            if (!ModelToFurniture.ContainsKey(source))
                return null;
            Furniture v = ModelToFurniture[source];
            return v;
        }
        #endregion

        #region 저장 불러오기
        private XmlSerializer serializer;

        public void FileExport()
        {
            var path = this.fileDialogService.SaveFileDialog(null, null, OpenFileFilter, ".xml");
            if (path == null)
            {
                return;
            }
            this.Save(path);
        }
        public void FileOpen()
        {
            var path = this.fileDialogService.OpenFileDialog(null, null, OpenFileFilter, ".xml");
            if (path == null) return;
            if (TryLoad(path))
            {
            }
            else
            {
                MessageBox.Show("불러오기 실패\n손상된 파일이거나 파일 내부에 사용된 리소스가 없습니다.");
                return;
            }
            UpdateAllModel();
        }
        public void Save(string fileName)
        {
            List<ForSaveClass> WallCopy = new List<ForSaveClass>();
            List<ForSaveClass> FloorCopy = new List<ForSaveClass>();
            List<ForSaveClass> FurnitureCopy = new List<ForSaveClass>();

            ///벽 리스트 저장
            for (short a = 0; a < Walls.Count; a++)
            {
                WallCopy.Add(new ForSaveClass());
            }
            int cnt = 0;
            foreach (var w in Walls)
            {
                //wall
                WallCopy[cnt].XmlStartPoint = w.XmlStartPoint;
                WallCopy[cnt].XmlEndPoint = w.XmlEndPoint;
                WallCopy[cnt].XmlWidth = w.XmlWidth;
                WallCopy[cnt].XmlDepth = w.XmlDepth;
                WallCopy[cnt].XmlHeight = w.XmlHeight;
                WallCopy[cnt].XmlAngle = w.XmlAngle;
                WallCopy[cnt].XmlSide0_TextureName = w.XmlSide0_TextureName;
                WallCopy[cnt].XmlSide1_TextureName = w.XmlSide1_TextureName;
                WallCopy[cnt].XmlSide2_TextureName = w.XmlSide2_TextureName;
                WallCopy[cnt].XmlSide3_TextureName = w.XmlSide3_TextureName;
                WallCopy[cnt].XmlSide4_TextureName = w.XmlSide4_TextureName;
                WallCopy[cnt].XmlSide5_TextureName = w.XmlSide5_TextureName;
                //floor
                WallCopy[cnt].XmlIndex = "0";
                WallCopy[cnt].XmlRoomName = "Nan";
                WallCopy[cnt].XmlFloorTextruePath = "Nan";
                WallCopy[cnt].XmlCeilingTextruePath = "Nan";
                WallCopy[cnt].XmlIsShowFloor = "False";
                WallCopy[cnt].XmlIsShowCeiling = "False";
                WallCopy[cnt].XmlIsCeilingColorUsing = "False";
                WallCopy[cnt].XmlIsCeilingTextureUsing = "False";
                WallCopy[cnt].XmlIsFloorColorUsing = "False";
                WallCopy[cnt].XmlIsFloorTextureUsing = "False";
                WallCopy[cnt].XmlFirstPoint = "Nan";
                WallCopy[cnt].XmlSecondPoint = "Nan";
                WallCopy[cnt].XmlThirdPoint = "Nan";
                WallCopy[cnt].XmlFourthPoint = "Nan";
                WallCopy[cnt].XmlFloorColor = "#FFFFFF";
                WallCopy[cnt].XmlCeilingColor = "#FFFFFF";
                WallCopy[cnt].XmlFHeight = "0";
                //furniture
                WallCopy[cnt].XmlPosition = "0,0,0";
                WallCopy[cnt].XmlAngleZ = "0";
                WallCopy[cnt].XmlScaleX = "0";
                WallCopy[cnt].XmlScaleY = "0";
                WallCopy[cnt].XmlScaleZ = "0";
                WallCopy[cnt].XmlFurnitureName = "Nan";
                WallCopy[cnt].XmlModelPath = "Nan";
                cnt++;
            }

            ///바닥 리스트 저장
            for (int i = 0; i < Floors.Count; i++)
            {
                FloorCopy.Add(new ForSaveClass());
            }

            cnt = 0;
            foreach (var f in Floors)
            {
                //wall
                FloorCopy[cnt].XmlEndPoint = "0,0,0";
                FloorCopy[cnt].XmlAngle = "0";
                FloorCopy[cnt].XmlSide0_TextureName = "Nan";
                FloorCopy[cnt].XmlSide1_TextureName = "Nan";
                FloorCopy[cnt].XmlSide2_TextureName = "Nan";
                FloorCopy[cnt].XmlSide3_TextureName = "Nan";
                FloorCopy[cnt].XmlSide4_TextureName = "Nan";
                FloorCopy[cnt].XmlSide5_TextureName = "Nan";
                //floor
                FloorCopy[cnt].XmlIndex = f.Index.ToString();
                List<Point3D> temp1 = new List<Point3D>();
                if (f.Vertexs.Count>2)
                {
                    foreach (var v in f.Vertexs)
                    {
                        temp1.Add(new Point3D(v.X, v.Y, v.Z));
                    }
                    FloorCopy[cnt].Vertexs = temp1;
                }
                FloorCopy[cnt].XmlRoomName = f.RoomName.ToString();
                FloorCopy[cnt].XmlFloorTextruePath = f.FloorTextruePath.ToString();
                FloorCopy[cnt].XmlCeilingTextruePath = f.CeilingTextruePath.ToString();
                FloorCopy[cnt].XmlIsShowFloor = f.XmlIsShowFloor;
                FloorCopy[cnt].XmlIsShowCeiling = f.XmlIsShowCeiling;
                FloorCopy[cnt].XmlIsCeilingColorUsing = f.XmlIsCeilingColorUsing;
                FloorCopy[cnt].XmlIsCeilingTextureUsing = f.XmlIsCeilingTextureUsing;
                FloorCopy[cnt].XmlIsFloorColorUsing = f.XmlIsFloorColorUsing;
                FloorCopy[cnt].XmlIsFloorTextureUsing = f.XmlIsFloorTextureUsing;
                FloorCopy[cnt].XmlFirstPoint = f.XmlFirstPoint;
                FloorCopy[cnt].XmlSecondPoint = f.XmlSecondPoint;
                FloorCopy[cnt].XmlThirdPoint = f.XmlThirdPoint;
                FloorCopy[cnt].XmlFourthPoint = f.XmlFourthPoint;
                FloorCopy[cnt].XmlFloorColor = f.XmlFloorColor;
                FloorCopy[cnt].XmlCeilingColor = f.XmlCeilingColor;
                FloorCopy[cnt].XmlFHeight = f.XmlFHeight;
                //furniture
                FloorCopy[cnt].XmlPosition = "0,0,0";
                FloorCopy[cnt].XmlAngleZ = "0";
                FloorCopy[cnt].XmlScaleX = "0";
                FloorCopy[cnt].XmlScaleY = "0";
                FloorCopy[cnt].XmlScaleZ = "0";
                FloorCopy[cnt].XmlFurnitureName = "Nan";
                FloorCopy[cnt].XmlModelPath = "Nan";

                cnt++;
            }

            ///가구 리스트 저장
            for (short a = 0; a < Furnitures.Count; a++)
            {
                FurnitureCopy.Add(new ForSaveClass());
            }

            cnt = 0;
            foreach (var f in Furnitures)
            {
                //furniture
                FurnitureCopy[cnt].XmlPosition = f.XmlPosition;
                FurnitureCopy[cnt].XmlWidth = f.XmlWidth;
                FurnitureCopy[cnt].XmlDepth = f.XmlDepth;
                FurnitureCopy[cnt].XmlHeight = f.XmlHeight;
                FurnitureCopy[cnt].XmlAngleZ = f.XmlAngleZ;
                FurnitureCopy[cnt].XmlScaleX = f.XmlScaleX;
                FurnitureCopy[cnt].XmlScaleY = f.XmlScaleY;
                FurnitureCopy[cnt].XmlScaleZ = f.XmlScaleZ;
                FurnitureCopy[cnt].XmlFurnitureName = f.XmlFurnitureName;
                FurnitureCopy[cnt].XmlModelPath = f.XmlModelPath;

                //wall
                FurnitureCopy[cnt].XmlStartPoint = "0,0,0";
                FurnitureCopy[cnt].XmlEndPoint = "0,0,0";
                FurnitureCopy[cnt].XmlAngle = "0";
                FurnitureCopy[cnt].XmlSide0_TextureName = "Nan";
                FurnitureCopy[cnt].XmlSide1_TextureName = "Nan";
                FurnitureCopy[cnt].XmlSide2_TextureName = "Nan";
                FurnitureCopy[cnt].XmlSide3_TextureName = "Nan";
                FurnitureCopy[cnt].XmlSide4_TextureName = "Nan";
                FurnitureCopy[cnt].XmlSide5_TextureName = "Nan";
                //floor
                FurnitureCopy[cnt].XmlIndex = "0";
                FurnitureCopy[cnt].XmlRoomName = "Nan";
                FurnitureCopy[cnt].XmlFloorTextruePath = "Nan";
                FurnitureCopy[cnt].XmlCeilingTextruePath = "Nan";
                FurnitureCopy[cnt].XmlIsShowFloor = "False";
                FurnitureCopy[cnt].XmlIsShowCeiling = "False";
                FurnitureCopy[cnt].XmlIsCeilingColorUsing = "False";
                FurnitureCopy[cnt].XmlIsCeilingTextureUsing = "False";
                FurnitureCopy[cnt].XmlIsFloorColorUsing = "False";
                FurnitureCopy[cnt].XmlIsFloorTextureUsing = "False";
                FurnitureCopy[cnt].XmlFirstPoint = "Nan";
                FurnitureCopy[cnt].XmlSecondPoint = "Nan";
                FurnitureCopy[cnt].XmlThirdPoint = "Nan";
                FurnitureCopy[cnt].XmlFourthPoint = "Nan";
                FurnitureCopy[cnt].XmlFloorColor = "#FFFFFF";
                FurnitureCopy[cnt].XmlCeilingColor = "#FFFFFF";
                FurnitureCopy[cnt].XmlFHeight = "0";

                cnt++;
            }

            List<List<ForSaveClass>> temp = new List<List<ForSaveClass>>();
            temp.Add(WallCopy);
            temp.Add(FloorCopy);
            temp.Add(FurnitureCopy);

            using (var w = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            {
                serializer = new XmlSerializer(typeof(List<List<ForSaveClass>>), new[] { typeof(List<ForSaveClass>) });
                serializer.Serialize(w, temp);
            }
        }
        public bool TryLoad(string fileName)
        {
            try
            {
                using (var r = XmlReader.Create(fileName))
                {
                    serializer = new XmlSerializer(typeof(List<List<ForSaveClass>>), new[] { typeof(List<ForSaveClass>) });
                    var v = serializer.Deserialize(r);
                    List<List<ForSaveClass>> tempCopy = v as List<List<ForSaveClass>>;
                    List<ForSaveClass> WallCopy = tempCopy[0];
                    List<ForSaveClass> FloorCopy = tempCopy[1];
                    List<ForSaveClass> FurnitureCopy = tempCopy[2];

                    Walls = new List<Wall>();
                    for (short a = 0; a < WallCopy.Count; a++)
                    {
                        Walls.Add(new Wall());
                    }
                    int cnt = 0;
                    foreach (var w in Walls)
                    {
                        w.XmlStartPoint = WallCopy[cnt].XmlStartPoint;
                        w.XmlEndPoint = WallCopy[cnt].XmlEndPoint;
                        w.XmlWidth = WallCopy[cnt].XmlWidth;
                        w.XmlDepth = WallCopy[cnt].XmlDepth;
                        w.XmlHeight = WallCopy[cnt].XmlHeight;
                        w.XmlAngle = WallCopy[cnt].XmlAngle;
                        w.XmlSide0_TextureName = WallCopy[cnt].XmlSide0_TextureName;
                        w.XmlSide1_TextureName = WallCopy[cnt].XmlSide1_TextureName;
                        w.XmlSide2_TextureName = WallCopy[cnt].XmlSide2_TextureName;
                        w.XmlSide3_TextureName = WallCopy[cnt].XmlSide3_TextureName;
                        w.XmlSide4_TextureName = WallCopy[cnt].XmlSide4_TextureName;
                        w.XmlSide5_TextureName = WallCopy[cnt].XmlSide5_TextureName;
                        cnt++;
                    }

                    Floors = new List<Floor>();
                    for (int i = 0; i < FloorCopy.Count; i++)
                    {
                        Floors.Add(new Floor());
                    }
                    cnt = 0;
                    foreach (var f in Floors)
                    {
                        f.XmlIndex = FloorCopy[cnt].XmlIndex;
                        f.XmlRoomName = FloorCopy[cnt].XmlRoomName;
                        f.XmlFloorTextruePath = FloorCopy[cnt].XmlFloorTextruePath;
                        f.XmlCeilingTextruePath = FloorCopy[cnt].XmlCeilingTextruePath;
                        f.XmlIsShowFloor = FloorCopy[cnt].XmlIsShowFloor;
                        f.XmlIsShowCeiling = FloorCopy[cnt].XmlIsShowCeiling;
                        f.XmlIsCeilingColorUsing = FloorCopy[cnt].XmlIsCeilingColorUsing;
                        f.XmlIsCeilingTextureUsing = FloorCopy[cnt].XmlIsCeilingTextureUsing;
                        f.XmlIsFloorColorUsing = FloorCopy[cnt].XmlIsFloorColorUsing;
                        f.XmlIsFloorTextureUsing = FloorCopy[cnt].XmlIsFloorTextureUsing;
                        f.XmlFirstPoint = FloorCopy[cnt].XmlFirstPoint;
                        f.XmlSecondPoint = FloorCopy[cnt].XmlSecondPoint;
                        f.XmlThirdPoint = FloorCopy[cnt].XmlThirdPoint;
                        f.XmlFourthPoint = FloorCopy[cnt].XmlFourthPoint;
                        f.XmlFHeight = FloorCopy[cnt].XmlFHeight;

                        cnt++;
                    }

                    Furnitures = new List<Furniture>();
                    for (short a = 0; a < FurnitureCopy.Count; a++)
                    {
                        Furnitures.Add(new Furniture());
                    }
                    cnt = 0;
                    foreach (var f in Furnitures)
                    {
                        f.XmlPosition = FurnitureCopy[cnt].XmlPosition;
                        f.XmlWidth = FurnitureCopy[cnt].XmlWidth;
                        f.XmlDepth = FurnitureCopy[cnt].XmlDepth;
                        f.XmlHeight = FurnitureCopy[cnt].XmlHeight;
                        f.XmlAngleZ = FurnitureCopy[cnt].XmlAngleZ;
                        f.XmlScaleX = FurnitureCopy[cnt].XmlScaleX;
                        f.XmlScaleY = FurnitureCopy[cnt].XmlScaleY;
                        f.XmlScaleZ = FurnitureCopy[cnt].XmlScaleZ;
                        f.XmlFurnitureName = FurnitureCopy[cnt].XmlFurnitureName;
                        f.XmlModelPath = FurnitureCopy[cnt].XmlModelPath;
                        f.FurnitureNum = cnt;
                        if (Path.GetExtension(f.ModelPath).Equals(".xaml"))
                        {
                            Model3DGroup mb = XamlReader.Load(XmlReader.Create(f.ModelPath)) as Model3DGroup;
                            if (mb != null)
                            {
                                Rect3D boundbox = mb.Bounds;
                                f.Model = mb;
                                f.Bounds = boundbox;
                            }
                        }
                        else if (Path.GetExtension(f.ModelPath).Equals(".obj"))
                        {
                            var mi = new ModelImporter();

                            Model3DGroup mb = mi.Load(f.ModelPath);
                            if (mb != null)
                            {
                                Rect3D boundbox = mb.Bounds;
                                f.Model = mb;
                                f.Bounds = boundbox;
                            }
                        }
                        else if (Path.GetExtension(f.ModelPath).Equals(".3ds"))
                        {
                            var mi = new ModelImporter();

                            Model3DGroup mb = mi.Load(f.ModelPath);
                            if (mb != null)
                            {
                                Rect3D boundbox = mb.Bounds;
                                f.Model = mb;
                                f.Bounds = boundbox;
                            }
                        }

                        cnt++;
                    }

                }
                UpdateAllModel();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Create OutLines
        private void CreateFloorOutLine(Floor f)
        {
            LinesVisual3D line = new LinesVisual3D();
            GridLinesVisual3D lines = new GridLinesVisual3D();
            line.Points = f.GetVertexs();
            line.Color = Colors.Black;
            line.Thickness = 1.5;
            OutLineFloorVisuals.Add(line);
            viewportTop.Children.Add(line);
        }
        private void CreateWallOutLine(Wall w)
        {
            LinesVisual3D line = new LinesVisual3D();
            line.Transform = w.Transform3D;
            line.Points = w.CalculateVertex();
            line.Color = Colors.Black;
            line.Thickness = 2;
            OutLineWallVisuals.Add(line);
            viewportTop.Children.Add(line);
        }

        #endregion

        #region Create SelectedOutLine and Controller
        //remove
        private void RemovePreviousWallOutLineAndController()
        {

            foreach (var line in OutLineWallVisuals)
            {
                viewportTop.Children.Remove(line);
            }
            foreach (var line in SelectedVisuals)
            {
                viewportTop.Children.Remove(line);
            }
            foreach (var cont in LeftControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }
            foreach (var cont in RightControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }
            OutLineWallVisuals.Clear();
            SelectedVisuals.Clear();
            LeftControllerVisuals.Clear();
            RightControllerVisuals.Clear();
        }
        public void RemovePreviousFloorOutLineAndController()
        {
            foreach (var text in MainlinesVisuals)
            {
                viewportTop.Children.Remove(text);
            }
            foreach (var line in OutLineFloorVisuals)
            {
                viewportTop.Children.Remove(line);
            }
            foreach (var con in FloorControllerVisuals)
            {
                viewportTop.Children.Remove(con);
            }
            MainlinesVisuals.Clear();
            OutLineFloorVisuals.Clear();
            FloorControllerVisuals.Clear();

        }
        public void RemovePreviousFurnitureOutLineController()
        {
            foreach (var line in SelectedVisuals)
            {
                viewportTop.Children.Remove(line);
            }
            foreach (var cont in FurnitureRotateControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }
            foreach (var cont in FurnitureWidthHightControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }
            foreach (var cont in FurnitureDepthControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }
            foreach (var cont in FurnitureLocationZControllerVisuals)
            {
                viewportTop.Children.Remove(cont);
            }

            SelectedVisuals.Clear();
            FurnitureRotateControllerVisuals.Clear();
            FurnitureWidthHightControllerVisuals.Clear();
            FurnitureDepthControllerVisuals.Clear();
            FurnitureLocationZControllerVisuals.Clear();
        }
        //create
        private void CreateWallOutLineAndWallControllor(Wall w)
        {
            LinesVisual3D line = new LinesVisual3D();
            line.Transform = w.Transform3D;
            line.Points = w.CalculateVertex();
            line.Color = Colors.CornflowerBlue;
            line.Thickness = 4;
            OutLineWallVisuals.Add(line);
            viewportTop.Children.Add(line);

            RectangleVisual3D leftrect = new RectangleVisual3D();
            leftrect.Length = w.Height;
            leftrect.Width = w.Height;
            leftrect.Material = MaterialHelper.CreateMaterial(Colors.Red);
            
            Transform3DGroup trans = new Transform3DGroup();
            TranslateTransform3D tl = new TranslateTransform3D(w.StartPoint.X, w.StartPoint.Y, w.Depth + 0.1);
            AxisAngleRotation3D ro = new AxisAngleRotation3D(new Vector3D(0, 0, 1), w.Angle);
            RotateTransform3D r = new RotateTransform3D(ro);
            trans.Children.Add(r);
            trans.Children.Add(tl);
            leftrect.Transform = trans;
            LeftControllerVisuals.Add(leftrect);
            viewportTop.Children.Add(leftrect);

            RectangleVisual3D rightrect = new RectangleVisual3D();
            rightrect.Length = w.Height;
            rightrect.Width = w.Height;
            rightrect.Material = MaterialHelper.CreateMaterial(Colors.Blue);

            trans = new Transform3DGroup();
            tl = new TranslateTransform3D(w.EndPoint.X, w.EndPoint.Y, w.Depth + 0.1);
            ro = new AxisAngleRotation3D(new Vector3D(0, 0, 1), w.Angle);
            r = new RotateTransform3D(ro);
            trans.Children.Add(r);
            trans.Children.Add(tl);

            rightrect.Transform = trans;
            RightControllerVisuals.Add(rightrect);
            viewportTop.Children.Add(rightrect);
        }
        private void CreateFurnitureOutLineAndController(Furniture f)
        {

            LinesVisual3D line = new LinesVisual3D();
            //line.Transform = f.Transform3DGroup;
            line.Points = f.CalculateVertex();
            line.Color = Colors.CornflowerBlue;
            line.Thickness = 3;
            SelectedVisuals.Add(line);
            viewportTop.Children.Add(line);

            Transform3DGroup trans;

            //Rotate
            RectangleVisual3D rotaterect = new RectangleVisual3D();
            rotaterect.Length = 2;
            rotaterect.Width = 2;
            rotaterect.Material = MaterialHelper.CreateImageMaterial(rotateicon, 1);
            trans = new Transform3DGroup();
            trans.Children.Add(new TranslateTransform3D(f.Model.Bounds.X, f.Model.Bounds.Y + f.Model.Bounds.SizeY, f.Model.Bounds.Z + f.Model.Bounds.SizeZ + 0.3));
            //trans.Children.Add(f.Transform3DGroup);
            rotaterect.Transform = trans;
            FurnitureRotateControllerVisuals.Add(rotaterect);
            viewportTop.Children.Add(rotaterect);

            //Z
            RectangleVisual3D LocationZ = new RectangleVisual3D();
            LocationZ.Length = 2;
            LocationZ.Width = 2;
            LocationZ.Material = MaterialHelper.CreateImageMaterial(bottomicon, 1);
            //LocationZ.Transform = new TranslateTransform3D(f.Bounds.X + f.Bounds.SizeX, f.Bounds.Y + f.Bounds.SizeY, f.Bounds.Z + f.Bounds.SizeZ + 0.3);
            trans = new Transform3DGroup();
            trans.Children.Add(new TranslateTransform3D(f.Model.Bounds.X + f.Model.Bounds.SizeX, f.Model.Bounds.Y + f.Model.Bounds.SizeY, f.Model.Bounds.Z + f.Model.Bounds.SizeZ + 0.3));
            //trans.Children.Add(f.Transform3DGroup);
            LocationZ.Transform = trans;
            FurnitureLocationZControllerVisuals.Add(LocationZ);
            viewportTop.Children.Add(LocationZ);

            //Scale
            RectangleVisual3D Scalerect = new RectangleVisual3D();
            Scalerect.Length = 2;
            Scalerect.Width = 2;
            Scalerect.Material = MaterialHelper.CreateImageMaterial(expandicon, 1);
            trans = new Transform3DGroup();
            trans.Children.Add(new TranslateTransform3D(f.Model.Bounds.X + f.Model.Bounds.SizeX, f.Model.Bounds.Y, f.Model.Bounds.Z + f.Model.Bounds.SizeZ + 0.3));
            //trans.Children.Add(f.Transform3DGroup);
            Scalerect.Transform = trans;
            FurnitureWidthHightControllerVisuals.Add(Scalerect);
            viewportTop.Children.Add(Scalerect);

            //ScaleZ
            RectangleVisual3D Depthrect = new RectangleVisual3D();
            //Depthrect.Material = MaterialHelper.CreateImageMaterial()
            Depthrect.Length = 2;
            Depthrect.Width = 2;
            Depthrect.Material = MaterialHelper.CreateImageMaterial(verticalicon, 1);
            trans = new Transform3DGroup();
            trans.Children.Add(new TranslateTransform3D(f.Model.Bounds.X, f.Model.Bounds.Y, f.Model.Bounds.Z + f.Model.Bounds.SizeZ + 0.3));
            //trans.Children.Add(f.Transform3DGroup);
            Depthrect.Transform = trans;
            FurnitureDepthControllerVisuals.Add(Depthrect);
            viewportTop.Children.Add(Depthrect);


        }
        private void CreateFloorResizeVisual(Floor f)
        {
            int index = 0;
            foreach (var v in f.Vertexs)
            {
                RectangleVisual3D rect = new RectangleVisual3D();
                rect.Length = 2;
                rect.Width = 2;
                rect.Material = MaterialHelper.CreateMaterial(Colors.Blue);
                rect.Transform = new TranslateTransform3D(v.X, v.Y, f.defualtHeight + 1);
                rect.SetName(index.ToString());
                index++;
                FloorControllerVisuals.Add(rect);
                viewportTop.Children.Add(rect);
            }
        }
        private void CreateVirtualPersonSelectedOutline(VirtualPerson vp)
        {
            LinesVisual3D line = new LinesVisual3D();
            line.Transform = vp.Transform;
            line.Points = vp.MeshBuilder.ToMesh().Positions;
            line.Color = Colors.CornflowerBlue;
            line.Thickness = 3;
            OutLineVirtualPersonVisuals.Add(line);
            viewportTop.Children.Add(line);
        }
        #endregion

        #region ModelUpdate
        public void UpdateWallModel()
        {
            WallModel.Children.Clear();
            ModelToWall.Clear();
            OriginlMaterial.Clear();
            ModelToWallMaterial.Clear();
            RemovePreviousWallOutLineAndController();

            foreach (var w in Walls)
            {
                var m = CreateWall(w);
                if (w.Selected)
                {
                    CreateWallOutLineAndWallControllor(w);
                }
                else
                    CreateWallOutLine(w);
                ModelToWall.Add(m, w);
                WallModel.Children.Add(m);
            }
            OnPropertyChanged("WallModel");
        }
        public void UpdateFloorModel()
        {
            FloorModel.Children.Clear();
            ModelToFloor.Clear();
            RemovePreviousFloorOutLineAndController();

            foreach (var f in Floors)
            {
                if (f.Selected)
                {
                    CreateFloorResizeVisual(f);
                }
                if (f.Index >= 2)
                {
                    if(f.IsShowFloor)
                    {
                        var m = CreateFloor(f);
                        CreateAreaText(f);
                        ModelToFloor.Add(m, f);
                        FloorModel.Children.Add(m);
                    }

                    if (f.IsShowCeiling)
                    {
                        var cm = CreateCeiling(f);
                        ModelToFloor.Add(cm, f);
                        FloorModel.Children.Add(cm);
                    }
                }
            }
            OnPropertyChanged("FloorModel");
        }

        private void CreateAreaText(Floor f)
        {
           
            if (f.Vertexs.Count==4)
            {
                double a1 = Math.Sqrt(((f.Vertexs[0].X - f.Vertexs[1].X) * (f.Vertexs[0].X - f.Vertexs[1].X)) + ((f.Vertexs[0].Y - f.Vertexs[1].Y) * (f.Vertexs[0].Y - f.Vertexs[1].Y)));
                double b1 = Math.Sqrt(((f.Vertexs[1].X - f.Vertexs[2].X) * (f.Vertexs[1].X - f.Vertexs[2].X)) + ((f.Vertexs[1].Y - f.Vertexs[2].Y) * (f.Vertexs[1].Y - f.Vertexs[2].Y)));
                double c1 = Math.Sqrt(((f.Vertexs[2].X - f.Vertexs[0].X) * (f.Vertexs[2].X - f.Vertexs[0].X)) + ((f.Vertexs[2].Y - f.Vertexs[0].Y) * (f.Vertexs[2].Y - f.Vertexs[0].Y)));
                double s1 = (a1 + b1 + c1) / 2;
                double result1 = Math.Sqrt(s1 * ((s1 - a1) * (s1 - b1) * (s1 - c1)));

                double a2 = Math.Sqrt(((f.Vertexs[0].X - f.Vertexs[2].X) * (f.Vertexs[0].X - f.Vertexs[2].X)) + ((f.Vertexs[0].Y - f.Vertexs[2].Y) * (f.Vertexs[0].Y - f.Vertexs[2].Y)));
                double b2 = Math.Sqrt(((f.Vertexs[2].X - f.Vertexs[3].X) * (f.Vertexs[2].X - f.Vertexs[3].X)) + ((f.Vertexs[2].Y - f.Vertexs[3].Y) * (f.Vertexs[2].Y - f.Vertexs[3].Y)));
                double c2 = Math.Sqrt(((f.Vertexs[3].X - f.Vertexs[0].X) * (f.Vertexs[3].X - f.Vertexs[0].X)) + ((f.Vertexs[3].Y - f.Vertexs[0].Y) * (f.Vertexs[3].Y - f.Vertexs[0].Y)));
                double s2 = (a2 + b2 + c2) / 2;
                double result2 = Math.Sqrt(s2 * ((s2 - a2) * (s2 - b2) * (s2 - c2)));

                double Result = result1 + result2;
                
                BillboardTextVisual3D btv3d = new BillboardTextVisual3D();
                if (f.Vertexs[0].X < f.Vertexs[1].X && f.Vertexs[1].Y > f.Vertexs[2].Y)//우하단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y - (f.Vertexs[1].Y - f.Vertexs[3].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X > f.Vertexs[1].X && f.Vertexs[1].Y < f.Vertexs[2].Y)//좌상단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X - (f.Vertexs[0].X - f.Vertexs[2].X) / 2, f.Vertexs[0].Y + (f.Vertexs[3].Y - f.Vertexs[1].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X > f.Vertexs[1].X && f.Vertexs[1].Y > f.Vertexs[2].Y)//좌하단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y - (f.Vertexs[1].Y - f.Vertexs[3].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X < f.Vertexs[1].X && f.Vertexs[1].Y < f.Vertexs[2].Y)//우상단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y + (f.Vertexs[3].Y - f.Vertexs[1].Y) / 2, 100);
                }
                btv3d.Foreground = Brushes.Black;
                btv3d.FontSize = 15;
                if(f.RoomName.Equals("Nan"))
                {
                    btv3d.Text = string.Format("{0} ㎡", Math.Round(Result / 100, 2));
                }
                else
                {
                    btv3d.Text = string.Format("{0}\n{1} ㎡", f.RoomName.ToString() ,Math.Round(Result / 100, 2));
                }

                MainlinesVisuals.Add(btv3d);
                viewportTop.Children.Add(btv3d);
                return;
            }
            if (f.Vertexs.Count == 3)
            {
                double a1 = Math.Sqrt(((f.Vertexs[0].X - f.Vertexs[1].X) * (f.Vertexs[0].X - f.Vertexs[1].X)) + ((f.Vertexs[0].Y - f.Vertexs[1].Y) * (f.Vertexs[0].Y - f.Vertexs[1].Y)));
                double b1 = Math.Sqrt(((f.Vertexs[1].X - f.Vertexs[2].X) * (f.Vertexs[1].X - f.Vertexs[2].X)) + ((f.Vertexs[1].Y - f.Vertexs[2].Y) * (f.Vertexs[1].Y - f.Vertexs[2].Y)));
                double c1 = Math.Sqrt(((f.Vertexs[2].X - f.Vertexs[0].X) * (f.Vertexs[2].X - f.Vertexs[0].X)) + ((f.Vertexs[2].Y - f.Vertexs[0].Y) * (f.Vertexs[2].Y - f.Vertexs[0].Y)));
                double s1 = (a1 + b1 + c1) / 2;
                double result1 = Math.Sqrt(s1 * ((s1 - a1) * (s1 - b1) * (s1 - c1)));

                BillboardTextVisual3D btv3d = new BillboardTextVisual3D();
                if (f.Vertexs[0].X < f.Vertexs[1].X && f.Vertexs[1].Y > f.Vertexs[2].Y)//우하단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y - (f.Vertexs[1].Y - f.Vertexs[2].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X > f.Vertexs[1].X && f.Vertexs[1].Y < f.Vertexs[2].Y)//좌상단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X - (f.Vertexs[0].X - f.Vertexs[2].X) / 2, f.Vertexs[0].Y + (f.Vertexs[2].Y - f.Vertexs[1].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X > f.Vertexs[1].X && f.Vertexs[1].Y > f.Vertexs[2].Y)//좌하단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y - (f.Vertexs[1].Y - f.Vertexs[2].Y) / 2, 100);
                }
                else if (f.Vertexs[0].X < f.Vertexs[1].X && f.Vertexs[1].Y < f.Vertexs[2].Y)//우상단
                {
                    btv3d.Position = new Point3D(f.Vertexs[0].X + (f.Vertexs[2].X - f.Vertexs[0].X) / 2, f.Vertexs[0].Y + (f.Vertexs[2].Y - f.Vertexs[1].Y) / 2, 100);
                }
                btv3d.Foreground = Brushes.Black;
                btv3d.FontSize = 15;
                btv3d.Text = string.Format("{0} ㎡", Math.Round(result1 / 100, 2));

                MainlinesVisuals.Add(btv3d);
                viewportTop.Children.Add(btv3d);
            }
        }

        public void UpdateVisualPersonModel()
        {
            VirtualPersonModel.Children.Clear();
            //ModelToVirtualPerson.Clear();
            foreach (var item in OutLineVirtualPersonVisuals)
            {
                viewportTop.Children.Remove(item);
            }
            OutLineVirtualPersonVisuals.Clear();

            foreach (var vp in VirtualPersonList)
            {
                var m = CreateVirtualPerson(vp);
                if(vp.Selected)
                {
                    CreateVirtualPersonSelectedOutline(vp);
                }
                VirtualPersonModel.Children.Add(m);
            }
            OnPropertyChanged("VirtualPersonModel");
        }
        #endregion

        #region CreateGeometryModel3D
        #region Wall
        private Model3DGroup CreateWall(Wall w)
        {
            var cubelet = new Model3DGroup();
            for (int face = 0; face < 6; face++)
            {
                cubelet.Children.Add(CreateFace(face, new Point3D(w.Width / 2, 0, w.Depth / 2), w.Height, w.Width, w.Depth, w));
            }

            TranslateTransform3D tl = new TranslateTransform3D(w.StartPoint.X, w.StartPoint.Y, 0);
            AxisAngleRotation3D ro = new AxisAngleRotation3D(new Vector3D(0, 0, 1), w.Angle);
            RotateTransform3D r = new RotateTransform3D(ro);
            Transform3DGroup trans = new Transform3DGroup();
            trans.Children.Add(r);
            trans.Children.Add(tl);
            cubelet.Transform = trans;
            w.Transform3D = trans;
            return cubelet;
        }
        private GeometryModel3D CreateFace(int face, Point3D center, double width, double length, double height, Wall w)
        {
            var m = new GeometryModel3D();
            var b = new MeshBuilder(false, true);
            switch (face)
            {
                case 0://시작점 면
                    b.AddCubeFace(center, new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1), length, width, height);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 0);
                    break;
                case 1://끝점 면
                    b.AddCubeFace(center, new Vector3D(1, 0, 0), new Vector3D(0, 0, -1), length, width, height);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 1);
                    break;
                case 2://안쪽 옆
                    b.AddCubeFace(center, new Vector3D(0, -1, 0), new Vector3D(0, 0, 1), width, length, height);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 2);
                    break;
                case 3://바깥쪽
                    b.AddCubeFace(center, new Vector3D(0, 1, 0), new Vector3D(0, 0, -1), width, length, height);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 3);
                    break;
                case 4://위
                    b.AddCubeFace(center, new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), height, length, width);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 4);
                    w.Bounds = b.ToMesh().Bounds;
                    break;
                case 5://바닥
                    b.AddCubeFace(center, new Vector3D(0, 0, 1), new Vector3D(0, -1, 0), height, length, width);
                    m.Geometry = b.ToMesh();
                    ModelToWallMaterial.Add(m, 5);
                    break;
            }

            UpdateMaterial(face, w, m);
            ModelToWall.Add(m, w);
            return m;
        }
        private void UpdateMaterial(int face, Wall w, GeometryModel3D m)
        {
            switch (face)
            {
                case 0:
                    if (w.Side0_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side0_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                case 1:
                    if (w.Side1_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side1_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                case 2:
                    if (w.Side2_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side2_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                case 3:
                    if (w.Side3_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side3_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                case 4:
                    if (w.Side4_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side4_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                case 5:
                    if (w.Side5_TextureName.Equals(""))
                    {
                        m.Material = MaterialHelper.CreateMaterial(Colors.White);
                    }
                    else
                    {
                        var path = @"\..\..\Texture\sources\" + w.Side5_TextureName;
                        ImageBrush ib = new ImageBrush();

                        ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                        ib.TileMode = TileMode.Tile;
                        ib.ViewportUnits = BrushMappingMode.Absolute;
                        ib.ViewboxUnits = BrushMappingMode.Absolute;
                        ib.Viewbox = new Rect(0, 0, ib.ImageSource.Width, ib.ImageSource.Height);
                        ib.Viewport = new Rect(0, 0, 0.05, 0.1);
                        m.Material = new DiffuseMaterial(ib);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region Floor
        private GeometryModel3D CreateFloor(Floor f)
        {
            GeometryModel3D m = new GeometryModel3D();
            MeshBuilder mb = new MeshBuilder();
            if (f.Vertexs.Count == 5)
            {
                f.Vertexs.RemoveAt(4);
            }
            List<Point3D> temp = new List<Point3D>();
            for (int i = 0; i < f.Vertexs.Count; i++)
            {
                temp.Add(new Point3D(f.Vertexs[i].X, f.Vertexs[i].Y, f.Vertexs[i].Z + f.FHeight));
            }
            
            mb.AddPolygon(temp);

            m.Geometry = mb.ToMesh();

            if (f.IsFloorTextureUsing)
            {
                var path = @"\..\..\Texture\sources\" + f.FloorTextruePath;

                ImageBrush ib = new ImageBrush();

                ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                ib.TileMode = TileMode.Tile;
                ib.Stretch = Stretch.Uniform;
                ib.Viewport = new Rect(0, 0, 0.05, 0.05);
                ib.ViewportUnits = BrushMappingMode.Absolute;
                m.Material = new DiffuseMaterial(ib);
                m.BackMaterial = new DiffuseMaterial(ib);
                return m;


            }
            m.Material = new DiffuseMaterial(new SolidColorBrush(f.FloorColor));
            m.BackMaterial = new DiffuseMaterial(new SolidColorBrush(f.FloorColor));
            return m;
        }

        private GeometryModel3D CreateCeiling(Floor f)
        {
            GeometryModel3D m = new GeometryModel3D();
            MeshBuilder mb = new MeshBuilder();
            if (f.Vertexs.Count == 5)
            {
                f.Vertexs.RemoveAt(4);
            }
            List<Point3D> temp = new List<Point3D>();
            for (int i = 0; i < f.Vertexs.Count; i++)
            {
                temp.Add(new Point3D(f.Vertexs[i].X, f.Vertexs[i].Y, f.Vertexs[i].Z + (f.defualtHeight-0.1)));
            }
            mb.AddPolygon(temp);
            mb.CreateNormals = false;
            mb.CreateTextureCoordinates = true;
            m.Geometry = mb.ToMesh();

            if (f.IsCeilingTextureUsing)
            {
                var path = @"\..\..\Texture\sources\" + f.CeilingTextruePath;

                ImageBrush ib = new ImageBrush();

                ib.ImageSource = new BitmapImage(new Uri(Environment.CurrentDirectory + path, UriKind.Relative));
                ib.TileMode = TileMode.Tile;
                ib.Stretch = Stretch.Uniform;
                ib.Viewport = new Rect(0, 0, 0.05, 0.05);
                ib.ViewportUnits = BrushMappingMode.Absolute;
                m.Material = new DiffuseMaterial(ib);
                return m;
            }
            m.Material = new DiffuseMaterial(new SolidColorBrush(f.CeilingColor));
            return m;
        }
        #endregion
        #region VitualPerson
        private GeometryModel3D CreateVirtualPerson(VirtualPerson vp)
        {
            var m = new GeometryModel3D();
            var mb = new MeshBuilder();
            mb.AddBox(vp.Position, 7, 5, 1);
            m.Geometry = mb.ToMesh();
            m.Material = MaterialHelper.CreateImageMaterial(observericon, 1, false);

            Transform3DGroup trans = new Transform3DGroup();
            AxisAngleRotation3D ro = new AxisAngleRotation3D(new Vector3D(0, 0, 1), vp.Angle);
            RotateTransform3D r = new RotateTransform3D(ro, vp.Position);
            trans.Children.Add(r);
            vp.Transform = trans;
            m.Transform = trans;
            vp.MeshBuilder = mb;
            ModelToVirtualPerson.Add(m, vp);
            return m;
        }
        #endregion

        //private Model3DGroup CreateFloor(Floor f)
        //{
        //    Model3DGroup mg = new Model3DGroup();

        //    MeshGeometry3D mesh = new MeshGeometry3D();
        //    int index = 0;
        //    for (int i = 1; i < f.Vertexs.Count-1; i++)
        //    {
        //        CreateTriangleModel(f.Vertexs[0], f.Vertexs[i], f.Vertexs[i + 1], ref index,mesh);

        //    }

        //    string Path = @"\..\..\Texture\tree.PNG";
        //    BitmapImage bi = new BitmapImage();
        //    bi.BeginInit();
        //    bi.UriSource = new Uri(Environment.CurrentDirectory + Path, UriKind.Relative);
        //    bi.EndInit();

        //    Material material = MaterialHelper.CreateMaterial(new SolidColorBrush(Colors.White), 0, 255, false);

        //    GeometryModel3D model = new GeometryModel3D();
        //    model.Geometry = mesh;
        //    //model.Material = material;
        //    //model.BackMaterial = material;
        //    model.Material = MaterialHelper.CreateImageMaterial(bi, 1, false);
        //    model.BackMaterial = MaterialHelper.CreateImageMaterial(bi, 1, false);
        //    mg.Children.Add(model);
        //    ModelToFloor.Add(model, f);
        //    return mg;
        //}

        #endregion

        #region Add메소드 
        internal Wall CreateWallVoxel(Point3D StartPoint, Point3D EndPoint, double angle)
        {
            Wall wall = new Wall(StartPoint, EndPoint, angle);
            Walls.Add(wall);
            UpdateWallModel();
            return wall;
        }
        internal Floor CreateFloorVoxel(Point3D StartPoint)
        {
            Floor floor = new Floor(StartPoint);
            Floors.Add(floor);
            return floor;
        }
        internal VirtualPerson CreateVirtualPersonVoxel(Point3D Position)
        {
            VirtualPerson vp = new VirtualPerson(Position);
            VirtualPersonList.Add(vp);
            UpdateVisualPersonModel();
            return vp;
        }
        #endregion

        #region 메소드
        public void Copy()
        {
            ClipBoardWalls.Clear();
            foreach (var w in Walls)
            {
                if (w.Selected)
                {
                    Wall tempwall = new Wall(w);
                    ClipBoardWalls.Add(tempwall);
                }
            }

            ClipBoardFloors.Clear();
            foreach (var f in Floors)
            {
                if (f.Selected)
                {
                    Floor tempfloor = new Floor(f);
                    ClipBoardFloors.Add(tempfloor);
                }
            }

            ClipBoardFurnitures.Clear();
            foreach (var fur in Furnitures)
            {
                if (fur.Selected)
                {
                    Furniture tempfur = new Furniture(fur);
                    ClipBoardFurnitures.Add(tempfur);
                }
            }
        }
        public void Cut()
        {
            List<Wall> wallremove = new List<Wall>();
            List<Floor> floorremove = new List<Floor>();
            List<Furniture> furnitureremove = new List<Furniture>();

            ClipBoardWalls.Clear();
            foreach (var w in Walls)
            {
                if (w.Selected)
                {
                    Wall tempwall = new Wall(w);
                    ClipBoardWalls.Add(tempwall);
                    wallremove.Add(w);
                }
            }

            ClipBoardFloors.Clear();
            foreach (var f in Floors)
            {
                if (f.Selected)
                {
                    Floor tempfloor = new Floor(f);
                    ClipBoardFloors.Add(tempfloor);
                    floorremove.Add(f);
                }
            }

            ClipBoardFurnitures.Clear();
            foreach (var fur in Furnitures)
            {
                if (fur.Selected)
                {
                    Furniture tempfur = new Furniture(fur);
                    ClipBoardFurnitures.Add(tempfur);
                    furnitureremove.Add(fur);
                }
            }
            foreach (var w in wallremove)
            {
                if (Walls.Contains(w))
                {
                    Walls.Remove(w);
                }
            }
            foreach (var f in floorremove)
            {
                if (Floors.Contains(f))
                {
                    Floors.Remove(f);
                }
            }
            foreach (var ff in furnitureremove)
            {
                if (Furnitures.Contains(ff))
                {
                    Furnitures.Remove(ff);
                }
            }
            UpdateAllModel();
        }
        public void Paste()
        {
            foreach (var w in ClipBoardWalls)
            {
                Walls.Add(w);
            }
            foreach (var f in ClipBoardFloors)
            {
                Floors.Add(f);
            }
            foreach (var fur in ClipBoardFurnitures)
            {
                Furnitures.Add(fur);
            }
            ClipBoardWalls.Clear();
            ClipBoardFloors.Clear();
            ClipBoardFurnitures.Clear();
        }
        internal Wall GetWall(Model3D source)
        {
            if (!ModelToWall.ContainsKey(source))
                return null;
            Wall w = ModelToWall[source];
            return w;
        }
        internal VirtualPerson GetVirtualPerson(Model3D source)
        {
            if (!ModelToVirtualPerson.ContainsKey(source))
                return null;
            VirtualPerson vp = ModelToVirtualPerson[source];
            return vp;
        }
        internal int GetWallMaterial(Model3D source)
        {
            if (!ModelToWallMaterial.ContainsKey(source))
                return 9;
            var m = ModelToWallMaterial[source];
            return m;
        }
        internal Floor GetFloor(Model3D source)
        {
            if (!ModelToFloor.ContainsKey(source))
                return null;
            Floor f = ModelToFloor[source];
            return f;
        }
        internal void FloorsRemove(Floor f)
        {
            if (Floors.Contains(f))
            {
                Floors.Remove(f);
            }
            UpdateFloorModel();
        }
        internal void WallsRemove(Wall w)
        {
            if (Walls.Contains(w))
            {
                Walls.Remove(w);
            }
            UpdateWallModel();
        }
        internal void VisualPersonRemove(VirtualPerson vp)
        {
            if (VirtualPersonList.Contains(vp))
            {
                VirtualPersonList.Remove(vp);
            }
            UpdateVisualPersonModel();
        }
        private void CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, ref int index, MeshGeometry3D mesh)
        {
            //MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(index++);
            mesh.TriangleIndices.Add(index++);
            mesh.TriangleIndices.Add(index++);

        }
        internal void SelectRelease()
        {
            foreach (var w in Walls)
            {
                w.Selected = false;
            }
            foreach (var f in Floors)
            {
                f.Selected = false;
            }
            foreach (var f in Furnitures)
            {
                f.Selected = false;
            }
            foreach (var v in VirtualPersonList)
            {
                v.Selected = false;
            }
            UpdateWallModel();
            UpdateFloorModel();
            UpdateFurnitureModel();
            UpdateVisualPersonModel();
        }
        public void Clear()
        {
            Walls.Clear();
            Furnitures.Clear();
            Floors.Clear();
            UpdateAllModel();
        }
        public void UpdateAllModel()
        {
            UpdateWallModel();
            UpdateFloorModel();
            UpdateFurnitureModel();
            UpdateVisualPersonModel();
        }
        #endregion

        #region RectSelelctor

        public SelectionHitMode SelectionMode
        {
            get
            {
                return this.RectangleSelectionTopCommand.SelectionHitMode;
            }

            set
            {
                this.RectangleSelectionTopCommand.SelectionHitMode = value;
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
                if (selectedModels == null)
                {
                    return;
                }
                foreach (var model in selectedModels)
                {
                    Wall select = GetWall(model);
                    foreach (var v in Walls)
                    {
                        if (v.Equals(select))
                        {
                            v.Selected = true;
                            break;
                        }
                    }
                    Floor Floor = GetFloor(model);
                    foreach (var v in Floors)
                    {
                        if (v.Equals(Floor))
                        {
                            v.Selected = true;
                            break;
                        }
                    }
                    Furniture Fur = GetFurniture(model);
                    foreach (var v in Furnitures)
                    {
                        if (v.Equals(Fur))
                        {
                            v.Selected = true;
                            break;
                        }
                    }
                }
                UpdateWallModel();
                UpdateFloorModel();
                UpdateFurnitureModel();
            }

        }

        #endregion

        #region 콤보박스 바인딩
        private InteriorVIewModel[] TextureImages;
        Dictionary<string, BitmapImage> ImgeDic;
        static int index = 0;
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
        #region TextureIndex
        private int _TextureIndex;
        public int TextureIndex
        {
            get
            {
                return _TextureIndex;
            }
            set
            {
                _TextureIndex = value;
            }
        }
        #endregion

        #region SelectedTextureItem
        private InteriorVIewModel _SelectedTextureItem;
        public InteriorVIewModel SelectedTextureItem
        {
            get
            {
                return _SelectedTextureItem;
            }
            set
            {
                _SelectedTextureItem = value;
                OnPropertyChanged("SelectedTextureItem");
            }
        }
        #endregion

        #region SelectedIndex
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
        public InteriorVIewModel[] All
        {
            get
            {
                return TextureImages;
            }
        }
        #endregion

        #region 가구 정보창 Property

        #region FunitureList
        private ObservableCollection<FunitureInfoForDataGrid> _FunitureList = new ObservableCollection<FunitureInfoForDataGrid>();
        public ObservableCollection<FunitureInfoForDataGrid> FunitureList
        {
            get
            {
                return _FunitureList;
            }
        }
        #endregion

        #region SelectedFuniture
        private FunitureInfoForDataGrid _SelectedFuniture;
        public FunitureInfoForDataGrid SelectedFuniture
        {
            get
            {
                return _SelectedFuniture;
            }

            set
            {
                _SelectedFuniture = value;
                OnPropertyChanged("SelectedFuniture");
                foreach (var f in Furnitures)
                {
                    if (f.FurnitureNum == SelectedFuniture.FunitureNum)
                    {
                        f.Visible = !SelectedFuniture.IsSelected;
                    }
                }
                UpdateFunitureDataGrid();
                UpdateFurnitureModel();
            }
        }

        #endregion

        #region IsAllItemsSelected
        private bool _IsAllItemSelected;
        public static int FunitureNum = 0;

        public bool IsAllItemSelected
        {
            get
            {
                return _IsAllItemSelected;
            }
            set
            {
                _IsAllItemSelected = value;
                OnPropertyChanged("IsAllItemSelected");
                if (value == true)
                {
                    foreach (var f in Furnitures)
                    {
                        f.Visible = true;
                    }

                }
                else
                {
                    foreach (var f in Furnitures)
                    {
                        f.Visible = false;
                    }
                }
                UpdateFurnitureModel();
            }
        }

        #endregion

        #endregion

        #region UpdateFunitureDataGrid
        public void UpdateFunitureDataGrid()
        {
            _FunitureList.Clear();

            foreach (var f in Furnitures)
            {
                FunitureInfoForDataGrid temp = new FunitureInfoForDataGrid();
                temp.Name = f.FurnitureName;
                temp.Width = Math.Round((f.Width * f.ScaleX) * 10);
                temp.Height = Math.Round((f.Height * f.ScaleY) * 10);
                temp.IsSelected = f.Visible;
                temp.FunitureNum = f.FurnitureNum;

                _FunitureList.Add(temp);

            }
        }
        #endregion

    }
}
