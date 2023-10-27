using drag_and_drop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
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
using DataObject = System.Windows.DataObject;

namespace drag_and_drop
{
    public partial class Circle : UserControl
    {
        public Circle()
        {
            InitializeComponent();
            txt.Text = new Random((int)DateTime.Now.Ticks).NextDouble().ToString();
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(DataFormats.StringFormat, circleUI.Fill.ToString());
                data.SetData("Double", circleUI.Height);
                data.SetData("Object", this);
                data.SetData("OriginParent", this.Parent);


                if (this.Parent is StackPanel parent)
				{
                    // Position de l'objet et de la fenêtre
					Point objPos = PointToScreen(new Point(0, 0));
					Point winPos = MainWindow.Ref.PointToScreen(new Point(0, 0));

					// Position relative obj / fenêtre
					double relX = objPos.X - winPos.X;
					double relY = objPos.Y - winPos.Y;

                    // remove l'enfant du Stackpanel parent, ajout dans la grid DNDContainer
					parent.Children.Remove(this);
                    MainWindow.Ref.DNDContainer.Children.Add(this);

                    // utilisation de la marge pour positionner l'object exactement là où il était à l'origine
                    this.Margin = new Thickness(relY, relX, 0, 0);
				}
                

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            // These Effects values are set in the drop target's
            // DragOver event handler.
             if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Pen);
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            
            e.Handled = true;
        }
    }
}
