using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;       // ObservableCollection class
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Security.Cryptography;

namespace DragDropTutorial;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    DropCursorAdorner _dropCursorAdorner = null!;
    DragVisualAdorner _dragVisualAdorner = null!;
    private Point _startPoint;
    private Point _origLviPoint;
    private ObservableCollection<WorkItem> Items = new ObservableCollection<WorkItem>();
    private int _startIndex = -1;
    private int _newIndex = -1;

    // implement INotifyPropertyChanged 
    public event PropertyChangedEventHandler? PropertyChanged;

    private Point _mousePos;
    public Point MousePos
    {
        get { return _mousePos; }
        set
        {
            _mousePos = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MousePos)));
        }
    }

    private int _counter;
    public int Counter
    {
        get { return _counter; }
        set
        {
            _counter = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Counter)));

        }
    }
    private Point _currentMousePos;
    public Point CurrentMousePos
    {
        get
        {
            return _currentMousePos;
        }
        set
        {
            _currentMousePos = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMousePos)));
        }
    }

    private string _dragLeave = string.Empty;
    public string DragEnterLeaveInfo
    {
        get
        {
            return _dragLeave;
        }
        set
        {
            _dragLeave = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DragEnterLeaveInfo)));
        }
    }

    private string _effects = string.Empty;
    public string Effects
    {
        get
        {
            return _effects;
        }
        set
        {
            _effects = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Effects)));
        }
    }

    //
    // Ctor
    public MainWindow()
    {
        InitializeComponent();
        InitializeListView();
    }

    private void InitializeListView()
    {
        // Clear data
        lstView.Items.Clear();
        Items.Clear();

        // Add rows
        Items.Add(new WorkItem(true, "Row 1", "First row"));
        Items.Add(new WorkItem(false, "Row 2", "Second row"));
        Items.Add(new WorkItem(false, "-----", "---------- Drag Me -----"));
        Items.Add(new WorkItem(true, "Row 3", "Third row"));
        Items.Add(new WorkItem(false, "Row 4", "Fourth row"));
        Items.Add(new WorkItem(false, "Row 5", "Fifth row"));

        // Set the source binding
        lstView.ItemsSource = Items;
    }

    private void lstView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Get current mouse position
        //_startPoint = e.GetPosition(null);
        _startPoint = e.GetPosition(lstView);
    }

    // Helper to search up the VisualTree
    private static T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
    {
        do
        {
            if (current is T)
            {
                return (T)current;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        while (current != null);
        return null!;
    }

    private void lstView_MouseMove(object sender, MouseEventArgs e)
    {
        ListView listView = (sender as ListView)!;
        if (listView == null) return;

        // Get the current mouse position
        //Point mousePos = e.GetPosition(null);
        Point mousePos = e.GetPosition(listView);
        Vector diff = _startPoint - mousePos;
        MousePos = mousePos;  // Something to bind to if you want to watch

        if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            // Get the dragged ListViewItem
            ListViewItem listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null) return;

            // Get the associated WorkItem data
            WorkItem item = (WorkItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            if (item == null) return;

            // Create the DataOject
            DataObject dragData = new DataObject("WorkItem", item);

            // Get the offset for ListViewItem to move
            _origLviPoint = listViewItem.TransformToVisual(listView).Transform(new Point());

            // Get the index of the ListViewItem to move
            _startIndex = listView.SelectedIndex;

            // Create and add the Adorners
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(listView);
            _dropCursorAdorner = new DropCursorAdorner(listView);
            _dragVisualAdorner = new DragVisualAdorner(listView, listViewItem.RenderSize, new VisualBrush(listViewItem));
            _dragVisualAdorner.Opacity = 0.6;
            adornerLayer.Add(_dropCursorAdorner);
            adornerLayer.Add(_dragVisualAdorner);

            // This returns once the drag drop is completed, i.e., mouse button released or ESC key.
            DragDropEffects performed = DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);

            // Remove the adorner layer
            adornerLayer.Remove(_dropCursorAdorner);
            adornerLayer.Remove(_dragVisualAdorner);
        }
    }

    private void lstView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("WorkItem") && sender == e.Source)
        {
            // Move item in the observable collection 
            // (this will be automatically reflected to lstView.ItemsSource)
            if (_startIndex >= 0 && _newIndex >= 0)
            {
                e.Effects = DragDropEffects.Move;
                Items.Move(_startIndex, _newIndex);
            }
            _startIndex = -1;        // Done!
            _newIndex = -1;
        }
    }

    private void btnUp_Click(object sender, RoutedEventArgs e)
    {
        WorkItem item;
        int index;

        if (lstView.SelectedItems.Count != 1) return;
        item = (WorkItem)lstView.SelectedItems[0]!;
        index = Items.IndexOf(item);
        if (index > 0)
        {
            Items.Move(index, index - 1);
        }
    }

    private void btnDown_Click(object sender, RoutedEventArgs e)
    {
        WorkItem item;
        int index;

        if (lstView.SelectedItems.Count != 1) return;
        item = (WorkItem)lstView.SelectedItems[0]!;
        index = Items.IndexOf(item);
        if (index < Items.Count - 1)
        {
            Items.Move(index, index + 1);
        }
    }

    int _enterLeaveCount = 0;
    private void lstView_DragEnter(object sender, DragEventArgs e)
    {
        DragEnterLeaveInfo = "E : " + ((Control)sender).Name + (++_enterLeaveCount).ToString();
    }

    private void lstView_DragLeave(object sender, DragEventArgs e)
    {
        DragEnterLeaveInfo = "L : " + ((Control)sender).Name + (++_enterLeaveCount).ToString();
    }

    private void lstView_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("WorkItem") || sender != e.Source)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        CurrentMousePos = e.GetPosition(lstView);

        DependencyObject depObj = VisualTreeHelper.HitTest(lstView, CurrentMousePos).VisualHit;
        ListViewItem listViewItem = FindAncestor<ListViewItem>(depObj);

        if (listViewItem == null || _dropCursorAdorner == null)
        {
            return;
        }

        Point lviPoint = listViewItem.TransformToVisual(lstView).Transform(new Point());
        Rect lviBounds = VisualTreeHelper.GetDescendantBounds(listViewItem);

        bool isUpperHalf = CurrentMousePos.Y <= (lviPoint.Y + (lviBounds.Bottom / 2));
        double cursorY = isUpperHalf ? lviPoint.Y - 0.5 : lviPoint.Y + lviBounds.Bottom + 0.5;
        _newIndex = lstView.ItemContainerGenerator.IndexFromContainer(listViewItem);

        if (_newIndex != _startIndex)
        {
            if (!isUpperHalf)
            {
                _newIndex++;
            }
            if (_newIndex > _startIndex)
            {
                _newIndex--;
            }
        }
        Counter = _newIndex;
        _dropCursorAdorner.YPos = cursorY;
        _dragVisualAdorner.SetOffsets(CurrentMousePos.X - _startPoint.X, _origLviPoint.Y + CurrentMousePos.Y - _startPoint.Y);
    }

    private void lstView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        Effects = e.Effects.ToString();
    }
}
