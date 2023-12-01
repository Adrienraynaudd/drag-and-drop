using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace drag_and_drop
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private FrameworkElement? draggedElement;
        private HorizontalAlignment initialHAlign;
        private Thickness initialMargin;
        private VerticalAlignment initialVAlign;
        private Point initialMousePosition;
        private Dictionary<FrameworkElement, Point> initialMousePositions = new Dictionary<FrameworkElement, Point>();
        private Dictionary<FrameworkElement, Thickness> initialMargins = new Dictionary<FrameworkElement, Thickness>();
        private double columnCenter;
        private ColumnDefinition currentColumnDefinition;
        private FrameworkElement draggedCopy;
        private UIElementCollection PanelCopy;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DragElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fwelm)
            {
                if (fwelm.Parent is Panel p)
                {
                    isDragging = true;
                    draggedElement = fwelm;

                    if (!initialMousePositions.ContainsKey(draggedElement))
                    {
                        initialMousePositions[draggedElement] = e.GetPosition(draggedElement);
                        initialMargins[draggedElement] = draggedElement.Margin;
                    }

                     draggedCopy = CreateDraggedCopy(draggedElement);

                    initialMousePosition = initialMousePositions[draggedElement];
                    initialMargin = initialMargins[draggedElement];

                    p.Children.Add(draggedCopy);

                    draggedCopy.Opacity = 0.7;

                    initialHAlign = draggedElement.HorizontalAlignment;
                    initialVAlign = draggedElement.VerticalAlignment;

                    draggedCopy.HorizontalAlignment = initialHAlign;
                    draggedCopy.VerticalAlignment = initialVAlign;

                    p.Children.Remove(draggedElement);
                    PanelCopy = p.Children;
                    container.Children.Add(draggedElement);

                    draggedElement.Margin = new Thickness(initialMargin.Left + e.GetPosition(container).X - initialMousePosition.X, initialMargin.Top + e.GetPosition(container).Y - initialMousePosition.Y, 0, 0);

                    UpdateLayout();

                    draggedElement.HorizontalAlignment = HorizontalAlignment.Left;
                    draggedElement.VerticalAlignment = VerticalAlignment.Top;

                    DragDrop.DoDragDrop(draggedElement, draggedElement, DragDropEffects.Move);
                }
            }
        }

        private FrameworkElement CreateDraggedCopy(FrameworkElement original)
        {
            
            FrameworkElement copy = Activator.CreateInstance(original.GetType()) as FrameworkElement;

            
            copy.Width = original.ActualWidth;
            copy.Height = original.ActualHeight;
            copy.AllowDrop = original.AllowDrop;
            copy.Margin = original.Margin;
            if (original is Shape originalShape && copy is Shape copyShape)
            {
                copyShape.Fill = originalShape.Fill;
            }

            return copy;
        }

        private void Container_Drop(object sender, DragEventArgs e)
        {
            if (draggedElement != null)
            {
                Point dropPosition = e.GetPosition(containerez);
                int insertionColumn = -1;
                double minDistance = double.MaxValue;

                for (int i = 0; i < containerez.ColumnDefinitions.Count; i++)
                {
                    if (dropPosition.X < containerez.ColumnDefinitions[i].ActualWidth)
                    {
                        insertionColumn = i;
                        break;
                    }
                    dropPosition.X -= containerez.ColumnDefinitions[i].ActualWidth;
                }

                if (insertionColumn == -1)
                {
                    // Si le drop est en dehors des colonnes
                    for (int i = 0; i < containerez.ColumnDefinitions.Count; i++)
                    {
                        columnCenter = containerez.ColumnDefinitions[i].ActualWidth / 2;
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
                    StackPanel targetStackPanel = containerez.Children.OfType<StackPanel>().Where(sp => Grid.GetColumn(sp) == insertionColumn).FirstOrDefault();

                    if (targetStackPanel != null)
                    {
                        Point absolutePositionSelected = draggedElement.PointToScreen(new Point(0, 0));
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
                        container.Children.Remove(draggedElement);
                        PanelCopy.Remove(draggedCopy);
                        

                        if (insertionIndex == -1)
                        {
                            targetStackPanel.Children.Add(draggedElement);
                            int currentColumn = Grid.GetColumn(draggedElement);
                            currentColumnDefinition = containerez.ColumnDefinitions[currentColumn];
                        }
                        else
                        {

                            targetStackPanel.Children.Insert(insertionIndex, draggedElement);
                            int currentColumn = Grid.GetColumn(draggedElement);
                            currentColumnDefinition = containerez.ColumnDefinitions[currentColumn];
                        }
                        columnCenter = currentColumnDefinition.ActualWidth / 2;
                        draggedElement.Margin = new Thickness(initialMargin.Left + (columnCenter-(draggedElement.Width/2)), initialMargin.Top, initialMargin.Right, initialMargin.Bottom);
                    }
                }

                isDragging = false;
                draggedElement = null;
            }
        }

        private void DragElement_DragOver(object sender, DragEventArgs e)
        {
            if (isDragging)
            {
                Point currentMousePosition = e.GetPosition(container);

                double newLeft = initialMargin.Left + currentMousePosition.X - initialMousePosition.X;
                double newTop = initialMargin.Top + currentMousePosition.Y - initialMousePosition.Y;

                
                double maxTop = container.ActualHeight - draggedElement.ActualHeight;
                newTop = Math.Max(0, Math.Min(newTop, maxTop));

                draggedElement.Margin = new Thickness(newLeft, newTop, 0, 0);
            }
        }

    }

}
