namespace CellAutomata.Util
{
    internal static class StaticExtension
    {
        public static long Area(this Rectangle rect)
        {
            return rect.Size.Width * rect.Size.Height;
        }

        public static long Area(this Size size)
        {
            return size.Width * size.Height;
        }
    }
}
