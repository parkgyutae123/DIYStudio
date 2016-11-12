using SF_DIY.ServiceReference1;
using SUT.PrintEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SF_DIY
{
    public class MaterailListItem
    {

        public MaterailListItem() { }

        public MaterailListItem(int num, String name, String length, String material, int saleNum, int price)
        {
            this.num = num;
            this.name = name;
            this.length = length;
            this.material = material;
            this.saleNum = saleNum;
            this.price = price;
        }

        int num;
        String name;
        String length;
        String material;
        int saleNum;
        int price;

        public int Num
        {
            set { num = value; }
            get { return num; }
        }
        public String Name
        {
            set { name = value; }
            get { return name; }
        }
        public String Length
        {
            set { length = value; }
            get { return length; }
        }
        public String Material
        {
            set { material = value; }
            get { return material; }
        }
        public int SaleNum
        {
            set { saleNum = value; }
            get { return saleNum; }
        }
        public int Price
        {
            set { price = value; }
            get { return price; }
        }

    }
    /// <summary>
    /// PrintPreView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PrintPreView : UserControl
    {
        #region 필드
        String KindName = null;
        int number = 1;
        Dictionary<String, List<Voxel>> plyWoods;
        Dictionary<String, List<Voxel>> rectLumbers;
        Dictionary<String, List<Voxel>> cylinders;
        Dictionary<String, List<Voxel>> materialKinds;
        ListView priceListView;
        GridView grdvue;
        GridViewColumn col;
        List<Voxel> voxels;
        int allPrice = 0;
        #endregion
        #region 생성자
        public PrintPreView()
        {
            InitializeComponent();
            MainViewModel.PreViewHandler += MainViewModel_PreViewHandler;
        }
        #endregion
        #region 사용한각목 가져오는 이벤트헨틀러
        private void MainViewModel_PreViewHandler(List<Voxel> obj)
        {
            voxels = obj;
        }
        #endregion
        #region 종류별정렬
        private void KindSort(Voxel kind)
        {
            KindName = kind.ProductName;

            if (kind.ModelType == 1)
            {
                if (!rectLumbers.ContainsKey(KindName))
                {
                    rectLumbers[KindName] = new List<Voxel>();
                }
            }
            else if (kind.ModelType == 2)
            {
                if (!plyWoods.ContainsKey(KindName))
                {
                    plyWoods[KindName] = new List<Voxel>();
                }
            }
            else
            {
                if (!cylinders.ContainsKey(KindName))
                {
                    cylinders[KindName] = new List<Voxel>();
                }
            }

            if (kind.ModelType == 1)
            {
                rectLumbers[KindName].Add(kind);
            }
            else if (kind.ModelType == 2)
            {
                plyWoods[KindName].Add(kind);
            }
            else
            {
                cylinders[KindName].Add(kind);
            }
        }
        #endregion
        #region 로딩초기화
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            materialPictureCanvas.Children.Clear();
            priceGrid.Children.Clear();
            totalPrice.Text = "0";
            allPrice = 0;
            number = 1;
            if (voxels == null)
            {
                return;
            }
            foreach (var v in voxels)
            {
                if (v.TextureName.Equals("Nan"))
                {
                    MessageBox.Show("재질 선택이 안된 재료가 있습니다.");
                    return;
                }
            }

            materialKinds = new Dictionary<string, List<Voxel>>();
            priceListView = new ListView();
            grdvue = new GridView();
            priceGrid.Children.Add(priceListView);
            priceListView.View = grdvue;

            if (voxels == null)
            {
                return;
            }

            for (int a = 0; a < voxels.Count; a++)
            {
                String name = voxels[a].TextureName;
                if (!materialKinds.ContainsKey(name))
                {
                    materialKinds[name] = new List<Voxel>();
                }
                materialKinds[name].Add(voxels[a]);
            }

            Product[] rectsalelist = MyModel.GetInstance().Get_RectangleList();
            Product[] cylinderlist = MyModel.GetInstance().Get_CylinderList();
            col = new GridViewColumn();
            col.Header = "번호";
            col.DisplayMemberBinding = new Binding("Num");
            grdvue.Columns.Add(col);

            col = new GridViewColumn();
            col.Header = "이름";
            col.DisplayMemberBinding = new Binding("Name");
            col.Width = 80;
            grdvue.Columns.Add(col);

            col = new GridViewColumn();
            col.Header = "길이(mm)";
            col.DisplayMemberBinding = new Binding("Length");
            col.Width = 80;
            grdvue.Columns.Add(col);

            col = new GridViewColumn();
            col.Header = "재질";
            col.DisplayMemberBinding = new Binding("Material");
            col.Width = 80;
            grdvue.Columns.Add(col);

            col = new GridViewColumn();
            col.Header = "개수";
            col.DisplayMemberBinding = new Binding("SaleNum");
            grdvue.Columns.Add(col);

            col = new GridViewColumn();
            col.Header = "가격(원)";
            col.DisplayMemberBinding = new Binding("Price");
            col.Width = 80;
            grdvue.Columns.Add(col);


            foreach (KeyValuePair<String, List<Voxel>> kind in materialKinds)
            {
                Double maxlength = 0.0;
                Double cmaxlenth = 0.0;
                for (int a = 0; a < rectsalelist.Length; a++)
                {
                    if (rectsalelist[a].Texture == kind.Key)
                    {
                        if (maxlength <= rectsalelist[a].MaxLength)
                        {
                            maxlength = rectsalelist[a].MaxLength;
                        }
                    }
                }
                for (int a = 0; a < cylinderlist.Length; a++)
                {
                    if (cylinderlist[a].Texture == kind.Key)
                    {
                        cmaxlenth = cylinderlist[a].MaxLength;
                    }
                }
                rectLumbers = new Dictionary<string, List<Voxel>>();
                cylinders = new Dictionary<string, List<Voxel>>();
                plyWoods = new Dictionary<string, List<Voxel>>();


                for (int a = 0; a < kind.Value.Count; a++)
                {
                    KindSort(kind.Value[a]);
                }
                DrawUseLumberMaterial(kind.Key, maxlength);
                DrawUseCylinderMaterial(kind.Key, cmaxlenth);
                MeasureMaterial(kind.Key);
            }

            totalPrice.Text = allPrice.ToString();
        }
        #endregion\
        #region 합판가격 측정
        private void MeasureMaterial(String texture)
        {
            foreach (KeyValuePair<String, List<Voxel>> lst in plyWoods)
            {
                MaterailListItem listitem;
                List<Voxel> plyList = lst.Value;
                Product[] plywoodSaleList = MyModel.GetInstance().Board_ListForPreView;
                Double unitCost = 0.0;

                for (int a = 0; a < plywoodSaleList.Length; a++)
                {
                    if (plywoodSaleList[a].Texture == texture && plywoodSaleList[a].Name == lst.Key)
                    {
                        unitCost = plywoodSaleList[a].Price;
                    }
                }

                foreach (Voxel plywood in plyList)
                {
                    Double price = ((plywood.Width * 100) * (plywood.Height * 100)) / unitCost;
                    listitem = new MaterailListItem(number++, lst.Key, ((plywood.Width) * 100).ToString() + " X " +
                        ((plywood.Height) * 100).ToString(), texture, 1, (int)price);
                    priceListView.Items.Add(listitem);
                    allPrice += (int)price;
                }
            }
        }
        #endregion
        #region 각목사용그리기와 가격측정
        private void DrawUseLumberMaterial(String texture, Double maxLenth)
        {

            foreach (KeyValuePair<String, List<Voxel>> lumble in rectLumbers)
            {
                TextBlock textureKind = new TextBlock();
                textureKind.FontSize = 20;
                textureKind.Foreground = Brushes.Black;
                textureKind.FontWeight = FontWeights.Bold;
                textureKind.Margin = new Thickness(0, 10, 0, 15);
                textureKind.Text = texture;
                materialPictureCanvas.Children.Add(textureKind);

                List<Voxel> rectLumberList = lumble.Value;
                List<Voxel> reLumberList = new List<Voxel>();
                List<Voxel> reSortLumberList = new List<Voxel>();

                Double len = maxLenth / 100;

                foreach (Voxel v in rectLumberList)
                {
                    Voxel copy = new Voxel(v);
                    reLumberList.Add(copy);
                }
                reLumberList.Sort((Voxel x, Voxel y) => y.Width.CompareTo(x.Width));
                Double space = len;


                while (reLumberList.Count != 0)
                {
                    List<Voxel> result = ReSortMaterial(reLumberList, len);
                    for (int a = 0; a < result.Count; a++)
                    {
                        reSortLumberList.Add(result[a]);
                    }
                }

                //
                Product[] rectsalelist = MyModel.GetInstance().Rectangle_ListForPreView;
                Dictionary<Double, int> measureList = new Dictionary<double, int>();
                List<Double> lengths = new List<Double>();
                List<int> productCount = new List<int>();
                for (int a = 0; a < rectsalelist.Length; a++)
                {
                    if (rectsalelist[a].Texture == texture && rectsalelist[a].Name == lumble.Key)
                    {
                        lengths.Add(rectsalelist[a].MaxLength);
                        measureList[rectsalelist[a].MaxLength] = 0;
                    }
                }

                lengths.Sort((Double x, Double y) => y.CompareTo(x));

                //

                String lumberName = lumble.Key;
                TextBlock lumberTextBlock = new TextBlock();
                lumberTextBlock.Text = lumberName;
                lumberTextBlock.Foreground = Brushes.Black;
                materialPictureCanvas.Children.Add(lumberTextBlock);

                Double length = reSortLumberList[0].Width;
                StackPanel rectBase = new StackPanel();
                int uses = reSortLumberList.Count;

                int index = 0;

                for (int a = 0; a < uses; a++)
                {
                    if (a == 0)
                    {
                        rectBase = new StackPanel();
                        rectBase.Height = 30;
                        rectBase.Width = maxLenth / 2;
                        rectBase.Background = Brushes.Beige;
                        rectBase.Orientation = Orientation.Horizontal;
                        materialPictureCanvas.Children.Add(rectBase);
                    }
                    if (length > len)
                    {
                        rectBase = new StackPanel();
                        rectBase.Margin = new Thickness(0, 10, 0, 0);
                        rectBase.Height = 30;
                        rectBase.Width = maxLenth / 2;
                        rectBase.Background = Brushes.Beige;
                        rectBase.Orientation = Orientation.Horizontal;
                        materialPictureCanvas.Children.Add(rectBase);
                        length -= reSortLumberList[a].Width;
                        //
                        for (int b = 0; b < lengths.Count; b++)
                        {
                            if (length / (lengths[b] / 100) >= 1)
                            {
                                if (length > (lengths[b] / 100))
                                {
                                    index = b - 1;
                                    break;
                                }
                                index = b;
                                break;
                            }
                        }
                        measureList[lengths[index]] = measureList[lengths[index]] + 1;
                        //
                        length = reSortLumberList[a].Width;
                    }

                    Border uselist = new Border();
                    Color clr = Color.FromArgb((Byte)(reSortLumberList[a].Width * 50), 100, 40, 20);
                    SolidColorBrush brush = new SolidColorBrush(clr);
                    uselist.Background = brush;
                    uselist.BorderBrush = Brushes.DarkGray;
                    uselist.Width = (reSortLumberList[a].Width / 2) * 100;
                    uselist.Padding = new Thickness(5);

                    TextBlock useLumberName = new TextBlock();
                    useLumberName.Text = ((reSortLumberList[a].Width) * 100).ToString()+"mm";
                    useLumberName.FontWeight = FontWeights.Bold;
                    useLumberName.Foreground = Brushes.Black;
                    uselist.Child = useLumberName;
                    rectBase.Children.Add(uselist);

                    if (a != uses - 1)
                    {
                        length += reSortLumberList[a + 1].Width;
                    }
                    //
                    if (a == uses - 1)
                    {
                        for (int b = 0; b < lengths.Count; b++)
                        {
                            if (length / (lengths[b] / 100) >= 1)
                            {
                                if (length > (lengths[b] / 100))
                                {
                                    index = b - 1;
                                    break;
                                }
                                index = b;
                                break;
                            }
                            if (b == lengths.Count - 1 && index == 0)
                            {
                                index = lengths.Count - 1;
                            }
                        }
                        measureList[lengths[index]] = measureList[lengths[index]] + 1;

                        foreach (KeyValuePair<Double, int> dic in measureList)
                        {
                            Double price = 0.0;
                            for (int x = 0; x < rectsalelist.Length; x++)
                            {
                                if (rectsalelist[x].Texture == texture && rectsalelist[x].Name == lumble.Key && rectsalelist[x].MaxLength == dic.Key)
                                {
                                    price = rectsalelist[x].Price;
                                }
                            }
                            if (dic.Value != 0)
                            {
                                MaterailListItem listItem = new MaterailListItem(number++, lumble.Key,
                                    dic.Key.ToString(), texture, dic.Value, (int)(price * dic.Value));
                                priceListView.Items.Add(listItem);
                                allPrice += (int)(price * dic.Value);
                            }
                        }
                    }
                    //
                }
            }
        }
        #endregion
        #region 재정렬
        private List<Voxel> ReSortMaterial(List<Voxel> materiallist, Double len)
        {
            List<Voxel> box = new List<Voxel>();
            Double length = len;

            for (int a = 0; a < materiallist.Count; a++)
            {
                if (length >= materiallist[a].Width)
                {
                    length -= materiallist[a].Width;
                    Voxel tmp = materiallist[a];
                    box.Add(tmp);
                    materiallist.RemoveAt(a);
                }
            }

            return box;
        }
        #endregion
        #region 원통사용그리기
        private void DrawUseCylinderMaterial(String texture, Double maxLenth)
        {


            foreach (KeyValuePair<String, List<Voxel>> cylinder in cylinders)
            {

                TextBlock textureKind = new TextBlock();
                textureKind.FontSize = 20;
                textureKind.Foreground = Brushes.Black;
                textureKind.FontWeight = FontWeights.Bold;
                textureKind.Margin = new Thickness(0, 10, 0, 15);
                textureKind.Text = texture;

                materialPictureCanvas.Children.Add(textureKind);
                List<Voxel> cylindersList = cylinder.Value;
                List<Voxel> reCylindersList = new List<Voxel>();
                List<Voxel> reSortCylindersList = new List<Voxel>();
                Double len = maxLenth / 100;

                foreach (Voxel v in cylindersList)
                {
                    Voxel copy = new Voxel(v);
                    reCylindersList.Add(copy);
                }
                reCylindersList.Sort((Voxel x, Voxel y) => y.Width.CompareTo(x.Width));

                while (reCylindersList.Count != 0)
                {
                    List<Voxel> result = ReSortMaterial(reCylindersList, len);
                    for (int a = 0; a < result.Count; a++)
                    {
                        reSortCylindersList.Add(result[a]);
                    }
                }

                //
                Product[] cylindersalelist = MyModel.GetInstance().Cylinder_ListForPreView;
                Dictionary<Double, int> measureList = new Dictionary<double, int>();
                List<Double> lengths = new List<Double>();
                List<int> productCount = new List<int>();
                for (int a = 0; a < cylindersalelist.Length; a++)
                {
                    if (cylindersalelist[a].Texture == texture && cylindersalelist[a].Name == cylinder.Key)
                    {
                        lengths.Add(cylindersalelist[a].MaxLength);
                        measureList[cylindersalelist[a].MaxLength] = 0;
                    }
                }

                lengths.Sort((Double x, Double y) => y.CompareTo(x));

                //

                String lumberName = cylinder.Key + " (원통)";
                TextBlock lumberTextBlock = new TextBlock();
                lumberTextBlock.Text = lumberName;
                lumberTextBlock.Foreground = Brushes.Black;
                materialPictureCanvas.Children.Add(lumberTextBlock);

                Double length = reSortCylindersList[0].Width;
                StackPanel rectBase = new StackPanel();
                int uses = reSortCylindersList.Count;
                int index = 0;

                for (int a = 0; a < uses; a++)
                {
                    if (a == 0)
                    {
                        rectBase = new StackPanel();
                        rectBase.Height = 30;
                        rectBase.Width = maxLenth / 2;
                        rectBase.Background = Brushes.Beige;
                        rectBase.Orientation = Orientation.Horizontal;
                        materialPictureCanvas.Children.Add(rectBase);
                    }
                    if (length > len)
                    {
                        rectBase = new StackPanel();
                        rectBase.Height = 30;
                        rectBase.Margin = new Thickness(0, 10, 0, 0);
                        rectBase.Width = maxLenth / 2;
                        rectBase.Background = Brushes.Beige;
                        rectBase.Orientation = Orientation.Horizontal;
                        materialPictureCanvas.Children.Add(rectBase);
                        length -= reSortCylindersList[a].Width;
                        //
                        for (int b = 0; b < lengths.Count; b++)
                        {
                            if (length / (lengths[b] / 100) >= 1)
                            {
                                if (length > (lengths[b] / 100))
                                {
                                    index = b - 1;
                                    break;
                                }
                                index = b;
                                break;
                            }
                        }
                        measureList[lengths[index]] = measureList[lengths[index]] + 1;
                        //
                        length = reSortCylindersList[a].Width;
                    }

                    Border uselist = new Border();
                    Color clr = Color.FromArgb((Byte)(reSortCylindersList[a].Width * 50), 100, 40, 20);
                    SolidColorBrush brush = new SolidColorBrush(clr);
                    uselist.Background = brush;
                    uselist.BorderBrush = Brushes.DarkGray;
                    uselist.Width = (reSortCylindersList[a].Width / 2) * 100;
                    uselist.Padding = new Thickness(5);

                    TextBlock useLumberName = new TextBlock();
                    useLumberName.FontWeight = FontWeights.Bold;
                    useLumberName.Foreground = Brushes.Black;
                    useLumberName.Text = ((reSortCylindersList[a].Width) * 100).ToString()+"mm";
                    uselist.Child = useLumberName;
                    rectBase.Children.Add(uselist);

                    if (a != uses - 1)
                    {
                        length += reSortCylindersList[a + 1].Width;
                    }
                    if (a == uses - 1)
                    {
                        for (int b = 0; b < lengths.Count; b++)
                        {
                            if (length / (lengths[b] / 100) >= 1)
                            {
                                if (length > (lengths[b] / 100))
                                {
                                    index = b - 1;
                                    break;
                                }
                                index = b;
                                break;
                            }
                            if (b == lengths.Count - 1 && index == 0)
                            {
                                index = lengths.Count - 1;
                            }
                        }
                        measureList[lengths[index]] = measureList[lengths[index]] + 1;

                        foreach (KeyValuePair<Double, int> dic in measureList)
                        {
                            Double price = 0.0;
                            for (int x = 0; x < cylindersalelist.Length; x++)
                            {
                                if (cylindersalelist[x].Texture == texture && cylindersalelist[x].Name == cylinder.Key && cylindersalelist[x].MaxLength == dic.Key)
                                {
                                    price = cylindersalelist[x].Price;
                                }
                            }
                            if (dic.Value != 0)
                            {
                                MaterailListItem listItem = new MaterailListItem(number++, cylinder.Key,
                                    dic.Key.ToString(), texture, dic.Value, (int)(price * dic.Value));
                                priceListView.Items.Add(listItem);
                                allPrice += (int)(price * dic.Value);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region 프린트
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            priceGrid.Children.Clear();
            TextBlock priceText = new TextBlock();
            priceText.Margin = new Thickness(20);
            priceText.Text = "가격 정보표";
            priceText.FontWeight = FontWeights.DemiBold;
            priceText.Foreground = Brushes.Black;
            priceText.FontSize = 30;
            materialPictureCanvas.Children.Add(priceText);
            materialPictureCanvas.Children.Add(priceListView);
            var visualSize = new Size(printSpace.ActualWidth, printSpace.ActualHeight);
            var printControl = PrintControlFactory.Create(visualSize, printSpace);
            printControl.ShowPrintPreview();
            materialPictureCanvas.Children.Remove(priceListView);
            materialPictureCanvas.Children.Remove(priceText);
            priceGrid.Children.Add(priceListView);
        }
        #endregion
    }
}
