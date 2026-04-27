using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Maximus.Views
{
    public partial class MapWindow : Window
    {
        private Point lastMousePosition;
        private bool isDragging = false;
        private double currentZoom = 1.0;
        private const double MinZoom = 0.05;
        private const double MaxZoom = 4.0;
        private const double ZoomStep = 1.1; 

        public MapWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => CenterAndZoomOut();
        }

        private void CenterAndZoomOut()
        {
            currentZoom = 0.5; 
            UpdateScale();
            
            MapScrollViewer.UpdateLayout();
            MapScrollViewer.ScrollToHorizontalOffset((MapImage.ActualWidth * currentZoom - MapScrollViewer.ActualWidth) / 2);
            MapScrollViewer.ScrollToVerticalOffset((MapImage.ActualHeight * currentZoom - MapScrollViewer.ActualHeight) / 2);
        }

        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(MapScrollViewer);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            ApplyZoomAtPoint(zoomFactor, mousePos);
            e.Handled = true;
        }

        private void ApplyZoomAtPoint(double factor, Point relativePoint)
        {
            double oldZoom = currentZoom;
            double newZoom = Math.Clamp(currentZoom * factor, MinZoom, MaxZoom);

            if (Math.Abs(newZoom - oldZoom) < 0.001) return;
            double absoluteX = (MapScrollViewer.HorizontalOffset + relativePoint.X) / oldZoom;
            double absoluteY = (MapScrollViewer.VerticalOffset + relativePoint.Y) / oldZoom;

            currentZoom = newZoom;
            UpdateScale();

            MapScrollViewer.UpdateLayout();

            MapScrollViewer.ScrollToHorizontalOffset(absoluteX * currentZoom - relativePoint.X);
            MapScrollViewer.ScrollToVerticalOffset(absoluteY * currentZoom - relativePoint.Y);
        }

        private void UpdateScale()
        {
            ScaleTransform.ScaleX = currentZoom;
            ScaleTransform.ScaleY = currentZoom;
            ZoomLabel.Text = $"Zoom: {(int)(currentZoom * 100)}%";
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            lastMousePosition = e.GetPosition(MapScrollViewer);
            ContentGrid.CaptureMouse();
            ContentGrid.Cursor = Cursors.SizeAll;
        }

        private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ContentGrid.ReleaseMouseCapture();
            ContentGrid.Cursor = Cursors.Arrow;
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(MapScrollViewer);
                double deltaX = currentPosition.X - lastMousePosition.X;
                double deltaY = currentPosition.Y - lastMousePosition.Y;

                MapScrollViewer.ScrollToHorizontalOffset(MapScrollViewer.HorizontalOffset - deltaX);
                MapScrollViewer.ScrollToVerticalOffset(MapScrollViewer.VerticalOffset - deltaY);

                lastMousePosition = currentPosition;
            }
        }


        private void ZoomIn_Click(object sender, RoutedEventArgs e) => 
            ApplyZoomAtPoint(ZoomStep, new Point(MapScrollViewer.ActualWidth / 2, MapScrollViewer.ActualHeight / 2));

        private void ZoomOut_Click(object sender, RoutedEventArgs e) => 
            ApplyZoomAtPoint(1 / ZoomStep, new Point(MapScrollViewer.ActualWidth / 2, MapScrollViewer.ActualHeight / 2));

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = 1.0;
            UpdateScale();
            MapScrollViewer.ScrollToHorizontalOffset(0);
            MapScrollViewer.ScrollToVerticalOffset(0);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    }
}