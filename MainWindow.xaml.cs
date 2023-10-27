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
       

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop(circleUI, circleUI, DragDropEffects.Move);
            }
        }
        private void Panel_Drop(object sender, DragEventArgs e)
        {
            

        }
        private void Panel_DragOver(object sender, DragEventArgs e)
        {
            Point mousePosition = e.GetPosition(gride);
            Canvas.SetLeft(circleUI, mousePosition.X);
            Canvas.SetTop(circleUI, mousePosition.Y);
        }


    }
}
