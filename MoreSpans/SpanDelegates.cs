namespace MoreSpans;

public delegate Tto ConvertFunc<Tfrom, Tto>(Tfrom value);

public delegate Tto FromBufferFunc<Tfrom, Tto>(ReadOnlySpan<Tfrom> span);

public delegate Tfrom[] ToBufferFunc<Tfrom, Tto>(Tto value);