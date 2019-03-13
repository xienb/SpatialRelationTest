using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

namespace SpatialRelationTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Point _lineStartPoint;//画线起点

        private List<System.Windows.Point> _pointList = new List<System.Windows.Point>();//画线轨迹
        public MainWindow()
        {
            InitializeComponent();
        }

     
        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            for (int i = 0; i < myCanvas.Children.Count;)
            {
                if (myCanvas.Children[i] is Shape)
                {
                    myCanvas.Children.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            _lineStartPoint = e.MouseDevice.GetPosition(myCanvas);
            _pointList.Clear();
        }

        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            myImage.Source = BitmapToBitmapSource(PointerFun());
            _pointList.Clear();
        }

        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.MouseDevice.GetPosition(myCanvas);
                if (_pointList.Count == 0)
                {
                    // 加入起始点
                    _pointList.Add(new System.Windows.Point(_lineStartPoint.X, _lineStartPoint.Y));
                }
                else
                {
                    // 加入移动过程中的point
                    _pointList.Add(currentPoint);
                }

                // 去重复点
                var disList = _pointList.Distinct().ToList();
                var count = disList.Count(); // 总点数
                if (currentPoint != this._lineStartPoint && this._lineStartPoint != null)
                {
                    var l = new Line();
                    l.Stroke = System.Windows.Media.Brushes.Red;
                    //l.StrokeDashArray = new DoubleCollection(new List<double>() {1,1,1,1});
                    l.StrokeThickness = 1;
                    if (count < 2)
                        return;
                    l.X1 = disList[count - 2].X;  // count-2  保证 line的起始点为点集合中的倒数第二个点。
                    l.Y1 = disList[count - 2].Y;
                    // 终点X,Y 为当前point的X,Y
                    l.X2 = currentPoint.X;
                    l.Y2 = currentPoint.Y;

                    //_currentLineList.Add(l);
                    myCanvas.Children.Add(l);
                }
            }
        }



        public bool contains(System.Windows.Point test)
        {
            int i;
            int j;
            bool result = false;
            for (i = 0, j = _pointList.Count - 1; i < _pointList.Count; j = i++)
            {
                if ((_pointList[i].Y > test.Y) != (_pointList[j].Y > test.Y) &&
                    (test.X < (_pointList[j].X - _pointList[i].X) * (test.Y - _pointList[i].Y) / (_pointList[j].Y - _pointList[i].Y) + _pointList[i].X))
                {
                    result = !result;
                }
            }
            return result;
        }


        public unsafe Bitmap PointerFun()
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            Bitmap curBitmap = new Bitmap(400, 400);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 400, 400);
            BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);//curBitmap.PixelFormat
            byte* ptr = (byte*)(bmpData.Scan0);
            //int cj = Vmax - Vmin;
            for (int y = 0; y < 400; y++)
            {
                for (int x = 0; x < 400; x++)
                {
                    int index = 400 * y + x;

                    System.Windows.Point p = new System.Windows.Point(x, y);
                    if (contains(p))
                    {
                        ptr[0] = 0;
                        ptr[1] = 128;
                        ptr[2] = 0;
                    }
                    else
                    {
                        ptr[0] = 169;
                        ptr[1] = 169;
                        ptr[2] = 169;
                    }
                    ptr += 3;
                }
                ptr += bmpData.Stride - bmpData.Width * 3;//每行读取到最后“有用”数据时，跳过未使用空间XX
            }
            curBitmap.UnlockBits(bmpData);
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            return curBitmap;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return result;
        }
    }
    
}
