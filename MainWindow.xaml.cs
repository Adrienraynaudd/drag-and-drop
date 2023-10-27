using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace drag_and_drop
{

    public partial class MainWindow : Window
    {
        public static MainWindow Ref;

        private Point initialMousePosition;
        private Point initialElementPosition;
        private UIElement selected;

        public MainWindow()
        {
            InitializeComponent();

            Ref = this;
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Initiate the drag-and-drop operation.
                selected = (UIElement)sender;

                DragDrop.DoDragDrop(selected, selected, DragDropEffects.Move);
            }
        }
        private void Panel_Drop(object sender, DragEventArgs e)
        {
            initialMousePosition = default(Point);
            initialElementPosition = default(Point);

        }
        private void Panel_DragOver(object sender, DragEventArgs e)
        {
            if (initialMousePosition == default(Point))
            {

                initialMousePosition = e.GetPosition(gride);
                initialElementPosition = new Point(Canvas.GetLeft(selected), Canvas.GetTop(selected));
            }
            else
            {
                Point currentMousePosition = e.GetPosition(gride);
                double deltaX = currentMousePosition.X - initialMousePosition.X;
                double deltaY = currentMousePosition.Y - initialMousePosition.Y;

                Canvas.SetLeft(selected, initialElementPosition.X + deltaX);
                Canvas.SetTop(selected, initialElementPosition.Y + deltaY);
            }
        }


    }
}
