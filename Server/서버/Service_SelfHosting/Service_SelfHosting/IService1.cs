using For_Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Media;
using System.Windows.Media;

namespace Service_SelfHosting
{
    // 참고: "리팩터링" 메뉴에서 "이름 바꾸기" 명령을 사용하여 코드 및 config 파일에서 인터페이스 이름 "IService1"을 변경할 수 있습니다.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        byte[] LoadImage(string imageNmae);

        [OperationContract]
        List<string> LoadList();

        [OperationContract]
        bool SaveImage(string name, byte[] imahe);

        [OperationContract]
        void RemoveImage(string name);

        [OperationContract]
        List<Product> GetRectangular_lumber();

        [OperationContract]
        List<Product> GetBoard_lumber();

        [OperationContract]
        List<Product> GetCylinder_lumber();

        [OperationContract]
        Dictionary<string, byte[]> GetImageFile();

        [OperationContract]
        bool SaveProduct(List<Product> ProductList);

        [OperationContract]
        bool RemoveProduct(List<Product> ProductList);
    }
}
