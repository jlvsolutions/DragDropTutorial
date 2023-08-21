using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragDropTutorial;

public class WorkItem
{
    public bool IsSelected { get; set; }
    public string Title { get; set; }
    public string Note { get; set; }

    public WorkItem(bool isSelected, string title, string note)
    {
        IsSelected = isSelected;
        Title = title;
        Note = note;
    }
}
