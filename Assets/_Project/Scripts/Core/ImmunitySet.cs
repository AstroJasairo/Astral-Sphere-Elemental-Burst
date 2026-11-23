using System.Collections.Generic;

// Các nguồn miễn nhiễm đang hiệu lực. Mỗi nguồn tự Set (và tự gỡ bằng Set None hoặc Remove), nên không đạp nhau.
public class ImmunitySet
{
    private struct Entry { public object source; public ImmunityFlags flags; }

    private readonly List<Entry> entries = new List<Entry>();

    public ImmunityFlags Flags { get; private set; }

    public bool Has(ImmunityFlags flag) { return (Flags & flag) != 0; }

    public void Set(object source, ImmunityFlags flags)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (!Equals(entries[i].source, source)) continue;

            if (flags == ImmunityFlags.None) entries.RemoveAt(i);
            else entries[i] = new Entry { source = source, flags = flags };
            Recompute();
            return;
        }

        if (flags != ImmunityFlags.None)
        {
            entries.Add(new Entry { source = source, flags = flags });
            Recompute();
        }
    }

    public void Remove(object source) { Set(source, ImmunityFlags.None); }

    public void Clear()
    {
        entries.Clear();
        Flags = ImmunityFlags.None;
    }

    private void Recompute()
    {
        ImmunityFlags all = ImmunityFlags.None;
        for (int i = 0; i < entries.Count; i++) all |= entries[i].flags;
        Flags = all;
    }
}