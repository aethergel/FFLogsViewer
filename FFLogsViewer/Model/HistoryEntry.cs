using System;

namespace FFLogsViewer.Model;

public class HistoryEntry
{
    public string FirstName = null!;
    public string LastName = null!;
    public string WorldName = null!;
    public DateTime LastSeen = DateTime.Now;

    public string FullName => this.LastName != string.Empty
        ? $"{this.FirstName} {this.LastName}"
        : this.FirstName;
}
