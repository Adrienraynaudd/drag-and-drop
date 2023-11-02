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
        private Point initialMousePosition;
        private Point initialElementPosition;
        private UIElement selected;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
               
                selected = (UIElement)sender;
                if (!Canvass.Children.Contains(selected))
                {
                    if (blue.Children.Contains(selected))
                    {
                        blue.Children.Remove(selected);

                    }
                    else if (pink.Children.Contains(selected))
                    {
                        pink.Children.Remove(selected);
                    }
                    gride.Children.Remove(selected);
                    Canvass.Children.Add(selected);
                }
                DragDrop.DoDragDrop(selected, selected, DragDropEffects.Move);
            }
        }
        private void Panel_Drop(object sender, DragEventArgs e)
        {
            if (selected != null)
            {
                Point dropPosition = e.GetPosition(gride);
                int insertionIndex = -1;
                for (int i = 0; i < gride.Children.Count; i++)
                {
                    var child = gride.Children[i] as UIElement;
                    if (child != null)
                    {
                        Point position = child.TranslatePoint(new Point(0, 0), gride);
                        if (position.Y + (child.RenderSize.Height / 2) > dropPosition.Y)
                        {
                            insertionIndex = i;
                            break;
                        }

                    }
                }
                if (insertionIndex == -1)
                {
                    Canvass.Children.Remove(selected);
                    gride.Children.Add(selected);
                }
                else
                {
                    Canvass.Children.Remove(selected);
                    gride.Children.Insert(insertionIndex, selected);
                }
                initialMousePosition = default(Point);
                initialElementPosition = default(Point);
                selected = null;
            }

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
