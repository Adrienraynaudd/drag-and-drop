using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace drag_and_drop
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point startPoint;
        private Border isSelected;
        private int originalColumn;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var border = sender as Border;
            startPoint = e.GetPosition(border);
            border.CaptureMouse();
            isSelected = border;

            originalColumn = Grid.GetColumn(isSelected);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && isSelected != null)
            {
                var border = sender as Border;
                Point newPoint = e.GetPosition(this);
                
                int newColumn = CalculateNewColumn(newPoint);

                
                Grid.SetColumn(isSelected, newColumn);
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            isSelected.ReleaseMouseCapture();
        }

 

        
        private int CalculateNewColumn(Point newPosition)
        {
            int newColumn = originalColumn;

            int totalColumns = 4;
            double columnWidth = 100;

            for (int column = 0; column < totalColumns; column++)
            {
                double columnLeft = column * columnWidth;
                double columnRight = (column + 1) * columnWidth;
                if (newPosition.X >= columnLeft && newPosition.X < columnRight + 100)
                {
                    double columnMid = (columnLeft + columnRight) / 2;
                    if (newPosition.X > columnMid)
                    {
                        newColumn = column + 1;
                        break;
                    }

                }
                if (column >= 1)
                {
                    double columnBack = (column - 1) * columnWidth;
                    if (newPosition.X < columnBack && newPosition.X > column * 100)
                    {
                        newColumn = column - 1;
                        break;
                    }
                }
            }


            foreach (UIElement element in mainGrid.Children)
            {
                if (element is Border border  && Grid.GetColumn(border) == newColumn)
                {
                    
                    int existingColumn = newColumn;
                    int offset = 1; 

                    if (newColumn > originalColumn)
                    {
                        
                        offset = -1;
                    }
                    if (!(existingColumn + offset > totalColumns) || !(existingColumn + offset <0))
                    {
                        Grid.SetColumn(border, existingColumn + offset);
                    }
                    
                }
            }

            return newColumn;
        }

    }
}
