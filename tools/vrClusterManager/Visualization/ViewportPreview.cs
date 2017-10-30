using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vrClusterManager
{
    public class ViewportPreview : INotifyPropertyChanged
    {
        //Main screen widtn resolution
        private double _mainScreenWidth;
        public double mainScreenWidth
        {
            get { return _mainScreenWidth; }
            set {
                Set(ref _mainScreenWidth, value, "mainScreenWidth");
                CanvasSizesCalc();
            }
        }

        //Main screen height resolution
        private double _mainScreenHeight;
        public double mainScreenHeight
        {
            get { return _mainScreenHeight; }
            set {
                Set(ref _mainScreenHeight, value, "mainScreenHeight");
                CanvasSizesCalc();
            }
        }

        //Main screen offset x on canvas
        private double _mainScreenOffsetX;
        public double mainScreenOffsetX
        {
            get { return _mainScreenOffsetX; }
            set { Set(ref _mainScreenOffsetX, value, "mainScreenOffsetX"); }
        }

        //Main screen offset y on canvas
        private double _mainScreenOffsetY;
        public double mainScreenOffsetY
        {
            get { return _mainScreenOffsetY; }
            set { Set(ref _mainScreenOffsetY, value, "mainScreenOffsetY"); }
        }

        //Canvas width and height
        private double _canvasSize;
        public double canvasSize
        {
            get { return _canvasSize; }
            set { Set(ref _canvasSize, value, "canvasSize"); }
        }

        //Text zone width and height
        private double _textZone;
        public double textZone
        {
            get { return _textZone; }
            set { Set(ref _textZone, value, "textZone"); }
        }

        //String with params of main screen size
        public string screenMaxSize
        {
            get { return SizeParameters(mainScreenOffsetX, mainScreenOffsetY); }
        }

        //String with params of viewport size
        public string viewportOffset
        {
            get { return SizeParameters(mainScreenOffsetX, mainScreenOffsetY); }
        }

        //Implementation of INotifyPropertyChanged method for TwoWay binding
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnNotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //Set property with OnNotifyPropertyChanged call
        protected void Set<T>(ref T field, T newValue, string propertyName)
        {
            field = newValue;
            OnNotifyPropertyChanged(propertyName);
        }

        public ViewportPreview()
        {
            mainScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            mainScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        }

        private void CanvasSizesCalc()
        {
            try
            {
                canvasSize = ((mainScreenWidth >= mainScreenHeight) ? mainScreenWidth : mainScreenHeight) * 1.1;
                textZone = 0.04 * canvasSize;
                mainScreenOffsetX = (canvasSize - mainScreenWidth) / 2;
                mainScreenOffsetY = (canvasSize - mainScreenHeight) / 2;
            }
            catch (DivideByZeroException)
            {

            }
        }

        private string SizeParameters(double x, double y)
        {
            return "(" + x.ToString() + "," + y.ToString() + ")";
        }

    }
}
