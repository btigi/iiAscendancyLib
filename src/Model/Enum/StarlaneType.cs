namespace ii.AscendancyLib.Model.Enum
{
    // This is probably a bitfield
    //  bit 0:  blue (0), red (1)
    //  bit 1:  open (0), blocked(1)
    public enum StarlaneType : int
    {
        Blue = 0,
        Red,
        BlueBlocked,
        RedBlocked
    }
}