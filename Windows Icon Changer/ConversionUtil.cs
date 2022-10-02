using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;
using WinCopies.GUI.Drawing;

namespace Windows.IconChanger;

public static class ConversionUtil
{
    public static string PngToIco(string pngFilePath,string icoPath="",int width=256,int height=256)
    {
        if (!File.Exists(pngFilePath))
        {
            throw new InvalidOperationException(
                "The png file must exist at provided path, which could not be found");
        }
        if (icoPath == "")
        {
            icoPath = pngFilePath.Replace("png", "ico", StringComparison.InvariantCultureIgnoreCase);
        }
        using var img = new Bitmap(pngFilePath);
        using var icon = new Bitmap(img, width, height);
        var bitmapSource = LoadBitmap(icon);
        var stream = new MemoryStream();
        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        encoder.Save(stream);
        return Convert(new Bitmap(stream),icoPath);
    }
    private static BitmapSource LoadBitmap(Bitmap source)
    {
        var ip = source.GetHbitmap();
        BitmapSource bs;

        try
        {
            bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            Gdi32.DeleteObject(ip);
        }

        return bs;
    }

    private static string Convert(Bitmap bitmap, string icoPath)
    {
        var mIcon = new MultiIcon();
        mIcon.Add("Untitled").CreateFrom(bitmap, IconOutputFormat.Vista);
        mIcon.SelectedIndex = 0;
        mIcon.Save(icoPath, MultiIconFormat.ICO);
        return icoPath;
    }
}