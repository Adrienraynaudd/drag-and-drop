using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace drag_and_drop
{

    public partial class MainWindow : Window
    {
        private Point initialMousePosition;
        private Point initialElementPosition;
        private UIElement selected;
        private int insertionIndex = -1;
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
                    Point mousePos = e.GetPosition(Canvass);
                    double offsetX = 50; 
                    double offsetY = 50;
                    gride.Children.Remove(selected);
                    Canvas.SetLeft(selected, mousePos.X - offsetX);
                    Canvas.SetTop(selected, mousePos.Y - offsetY);
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
                int insertionColumn = -1;
                double minDistance = double.MaxValue;

                for (int i = 0; i < gride.ColumnDefinitions.Count; i++)
                {
                    if (dropPosition.X < gride.ColumnDefinitions[i].ActualWidth)
                    {
                        insertionColumn = i;
                        break;
                    }
                    dropPosition.X -= gride.ColumnDefinitions[i].ActualWidth;
                }

                if (insertionColumn == -1)
                {
                    // Si le drop est en dehors des colonnes
                    for (int i = 0; i < gride.ColumnDefinitions.Count; i++)
                    {
                        double columnCenter = gride.ColumnDefinitions[i].ActualWidth / 2;
                        double distance = Math.Abs(dropPosition.X - columnCenter);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            insertionColumn = i;
                        }
                    }
                }

                if (insertionColumn != -1)
                {
                    StackPanel targetStackPanel = gride.Children
                        .OfType<StackPanel>()
                        .Where(sp => Grid.GetColumn(sp) == insertionColumn)
                        .FirstOrDefault();

                    if (targetStackPanel != null)
                    {
                        Point absolutePositionSelected = selected.PointToScreen(new Point(0, 0));
                        Point relativePositionSelected = targetStackPanel.PointFromScreen(absolutePositionSelected);

                        int insertionIndex = -1;

                        for (int i = 0; i < targetStackPanel.Children.Count; i++)
                        {
                            UIElement child = targetStackPanel.Children[i];
                            Point relativePositionChild = child.TransformToAncestor(targetStackPanel).Transform(new Point(0, 0));

                            if (relativePositionSelected.Y < relativePositionChild.Y)
                            {
                                insertionIndex = i;
                                break;
                            }
                        }

                        Canvass.Children.Remove(selected);

                        if (insertionIndex == -1)
                        {
                            targetStackPanel.Children.Add(selected);
                        }
                        else
                        {
                            targetStackPanel.Children.Insert(insertionIndex, selected);
                        }
                    }

                }
            }
        }



        private void Panel_DragOver(object sender, DragEventArgs e)
        {
            
            if (initialMousePosition == default(Point))
            {

                initialMousePosition = e.GetPosition(Canvass);
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
