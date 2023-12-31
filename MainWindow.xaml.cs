﻿using System;
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
        private double columnCenterX;
        private double columnCenterY;
        private ColumnDefinition currentColumnDefinition;
        private FrameworkElement draggedCopy;
        private UIElementCollection PanelCopy;
        public MainWindow()
        {
            InitializeComponent();
            DragLeave += MainWindow_DragLeave;
        }

        private void MainWindow_DragLeave(object sender, DragEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X >= ActualWidth || mousePosition.Y >= ActualHeight)
            {

                Debug.WriteLine("DragLeave");
                Container_Drop(sender, e);
            }
        }
        private void DragElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fwelm)
            {
                if (fwelm.Parent is Panel p)
                {
                    isDragging = true;
                    draggedElement = fwelm;
                    draggedCopy = CreateDraggedCopy(draggedElement);
                    initialMousePosition.X = fwelm.ActualWidth / 2;
                    initialMousePosition.Y = fwelm.ActualHeight / 2;
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

        private Border CreateDraggedCopy(FrameworkElement original)
        {
            Border ghost = new Border
            {
                Background = new BitmapCacheBrush(original),
                Width = original.ActualWidth,
                Height = original.ActualHeight,
                AllowDrop = original.AllowDrop,
                Margin = original.Margin
            };

            return ghost;
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
                        columnCenterX = containerez.ColumnDefinitions[i].ActualWidth / 2;


                        double distance = Math.Abs(dropPosition.X - columnCenterX);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            insertionColumn = i;
                        }
                    }
                }

                if (insertionColumn != -1)
                {
                    Panel targetPanel = containerez.Children.OfType<Panel>().Where(sp => Grid.GetColumn(sp) == insertionColumn).FirstOrDefault();
                    

                    if (targetPanel != null)
                    {
                        Point absolutePositionSelected = draggedElement.PointToScreen(new Point(0, 0));
                        Point relativePositionSelected = targetPanel.PointFromScreen(absolutePositionSelected);
                        int insertionIndex = -1;
                        columnCenterY = containerez.ActualHeight / 2;
                        if (targetPanel is StackPanel stackpanelV && stackpanelV.Orientation == Orientation.Vertical)
                        {
                            for (int i = 0; i < targetPanel.Children.Count; i++)
                            {
                                UIElement child = targetPanel.Children[i];
                                Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));

                                if (relativePositionSelected.Y <= relativePositionChild.Y)
                                {
                                    insertionIndex = i;
                                    break;
                                }
                            }
                        }
                        else if (targetPanel is StackPanel stackpanelH && stackpanelH.Orientation == Orientation.Horizontal)
                        {
                            for (int i = 0; i < targetPanel.Children.Count; i++)
                            {
                                UIElement child = targetPanel.Children[i];
                                Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));

                                if (relativePositionSelected.X <= relativePositionChild.X)
                                {
                                    insertionIndex = i;
                                    break;
                                }
                            }
                        }else if(targetPanel is WrapPanel wrapPanelV)
                        {
                            for (int i = 0; i < targetPanel.Children.Count; i++)
                            {
                                UIElement child = targetPanel.Children[i];
                                Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));
                                if (wrapPanelV.Orientation == Orientation.Vertical)
                                {
                                    if (relativePositionSelected.Y <= relativePositionChild.Y)
                                    {
                                        if (relativePositionSelected.X < relativePositionChild.X)
                                        {
                                            insertionIndex = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (relativePositionSelected.X <= relativePositionChild.X)
                                    {
                                        if (relativePositionSelected.Y <= relativePositionChild.Y)
                                        {
                                            insertionIndex = i;
                                            break;
                                        }
                                    }
                                }
                                
                            }
                        }

                        container.Children.Remove(draggedElement);
                        PanelCopy.Remove(draggedCopy);

                        if (insertionIndex == -1)
                        {
                            targetPanel.Children.Add(draggedElement);
                            int currentColumn = Grid.GetColumn(draggedElement);
                            currentColumnDefinition = containerez.ColumnDefinitions[currentColumn];
                        }
                        else
                        {
                            targetPanel.Children.Insert(insertionIndex, draggedElement);
                            int currentColumn = Grid.GetColumn(draggedElement);
                            currentColumnDefinition = containerez.ColumnDefinitions[currentColumn];
                        }
                        columnCenterX = currentColumnDefinition.ActualWidth / 2;
                        if (targetPanel is StackPanel stackpanelColV && stackpanelColV.Orientation == Orientation.Vertical)
                        {
                            draggedElement.Margin = new Thickness(initialMargin.Left + (columnCenterX - (draggedElement.Width / 2)), initialMargin.Top, initialMargin.Right, initialMargin.Bottom);
                        }
                        else if (targetPanel is StackPanel stackpanelColH && stackpanelColH.Orientation == Orientation.Horizontal)
                        {
                            double newY = initialMargin.Top + (columnCenterY - (draggedElement.Height / 2));

                            double maxTop = containerez.ActualHeight - draggedElement.ActualHeight;
                            newY = Math.Max(0, Math.Min(newY, maxTop));

                            draggedElement.Margin = new Thickness(initialMargin.Left, newY, initialMargin.Right, initialMargin.Bottom);
                        }
                        else if (targetPanel is WrapPanel wrapPanelColV && wrapPanelColV.Orientation == Orientation.Vertical)
                        {
                            draggedElement.Margin = new Thickness(0);
                        }
                        else if (targetPanel is WrapPanel wrapPanelColH && wrapPanelColH.Orientation == Orientation.Horizontal)
                        {
                            draggedElement.Margin = new Thickness(0);
                        }

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
                CopyDragOver(e);
            }
        }
        private void CopyDragOver(DragEventArgs e)
        {
            Point copyDropPosition = e.GetPosition(containerez);
            int insertionColumn = -1;
            double minDistance = double.MaxValue;

            for (int i = 0; i < containerez.ColumnDefinitions.Count; i++)
            {
                if (copyDropPosition.X < containerez.ColumnDefinitions[i].ActualWidth)
                {
                    insertionColumn = i;
                    break;
                }
                copyDropPosition.X -= containerez.ColumnDefinitions[i].ActualWidth;
            }

            if (insertionColumn != -1)
            {
                Panel targetPanel = containerez.Children.OfType<Panel>().Where(sp => Grid.GetColumn(sp) == insertionColumn).FirstOrDefault();
                if (targetPanel != null)
                {
                    Point absolutePositionSelected = draggedElement.PointToScreen(new Point(0, 0));
                    Point relativePositionSelected = targetPanel.PointFromScreen(absolutePositionSelected);
                    int insertionIndex = -1;
                    if (targetPanel is StackPanel stackpanelColV && stackpanelColV.Orientation == Orientation.Vertical)
                    {
                        for (int i = 0; i < targetPanel.Children.Count; i++)
                        {
                            UIElement child = targetPanel.Children[i];
                            Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));

                            if (relativePositionSelected.Y <= relativePositionChild.Y)
                            {
                                insertionIndex = i;
                                break;
                            }
                        }
                        if (!draggedCopy.Margin.Equals(new Thickness(0)))
                        {
                            int currentColumn = Grid.GetColumn(draggedCopy);
                            currentColumnDefinition = containerez.ColumnDefinitions[currentColumn];
                            columnCenterX = currentColumnDefinition.ActualWidth / 2;
                            draggedCopy.Margin = new Thickness(initialMargin.Left + (columnCenterX - (draggedCopy.Width / 2)), initialMargin.Top, initialMargin.Right, initialMargin.Bottom);
                        }
                    }
                    else if (targetPanel is StackPanel stackpanelH && stackpanelH.Orientation == Orientation.Horizontal)
                    {
                        for (int i = 0; i < targetPanel.Children.Count; i++)
                        {
                            UIElement child = targetPanel.Children[i];
                            Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));

                            if (relativePositionSelected.X <= relativePositionChild.X)
                            {
                                insertionIndex = i;
                                break;
                            }
                        }
                        if (!draggedCopy.Margin.Equals(new Thickness(0)))
                        {
                            columnCenterY = containerez.ActualHeight / 2;
                            double newY = initialMargin.Top + (columnCenterY - (draggedCopy.Height / 2));
                            double maxTop = containerez.ActualHeight - draggedCopy.ActualHeight;
                            newY = Math.Max(0, Math.Min(newY, maxTop));
                            draggedCopy.Margin = new Thickness(initialMargin.Left, newY, initialMargin.Right, initialMargin.Bottom);
                        }
                    }
                    else if (targetPanel is WrapPanel wrapPanelV)
                    {
                        for (int i = 0; i < targetPanel.Children.Count; i++)
                        {
                            UIElement child = targetPanel.Children[i];
                            Point relativePositionChild = child.TransformToAncestor(targetPanel).Transform(new Point(0, 0));
                            if (wrapPanelV.Orientation == Orientation.Vertical)
                            {
                                if (relativePositionSelected.Y <= relativePositionChild.Y)
                                {
                                    if (relativePositionSelected.X < relativePositionChild.X)
                                    {
                                        insertionIndex = i;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (relativePositionSelected.X <= relativePositionChild.X)
                                {
                                    if (relativePositionSelected.Y <= relativePositionChild.Y)
                                    {
                                        insertionIndex = i;
                                        break;
                                    }
                                }
                            }

                        }
                    }


                    PanelCopy.Remove(draggedCopy);

                    if (insertionIndex == -1)
                    {
                        targetPanel.Children.Add(draggedCopy);
                        PanelCopy = targetPanel.Children;
                    }
                    else
                    {
                        targetPanel.Children.Insert(insertionIndex, draggedCopy);
                        PanelCopy = targetPanel.Children;
                    }
                }
            }
        }
        private int compare(Panel _panel, FrameworkElement _elm1, FrameworkElement _elm2)
        {
            /*if (_panel.LogicalOrientationPublic == Orientation.Horizontal)
            {
                if (_elm1.)
            }*/
            return 0;
        }
    }

}
