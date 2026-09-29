namespace SmartishTable;

public class RepeaterContext<SmartishTItem>
{
  public SmartishTItem Item { get; set; } = default!;
  /// <summary>
  /// Index is -1 when using virtualization
  /// </summary>
  public int Index { get; set; }
}
