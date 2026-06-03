using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Timer = System.Threading.Timer;

namespace Wpf;

[ExcludeFromCodeCoverage]
public partial class ElementPickerWindow : Window
{
    private readonly Timer _updateTimer;
    private readonly int _magnifierSize = 140;
    private bool _isSelecting = true;
    
    public event EventHandler<Point>? ElementSelected;
    public event EventHandler? SelectionCancelled;
    
    public ElementPickerWindow()
    {
        InitializeComponent();
        
        // Таймер для обновления лупы
        _updateTimer = new Timer(UpdateMagnifier, null, 0, 50);
        
        // Скрываем окно при загрузке
        this.Loaded += (s, e) => this.Focus();
    }
    
    private void UpdateMagnifier(object? state)
    {
        if (!_isSelecting) return;
    
        Dispatcher.Invoke(() =>
        {
            try
            {
                var mousePos = System.Windows.Forms.Cursor.Position;
                // Захватываем экран (лупа уже скрыта)
                var screenshot = CaptureArea(
                    mousePos.X - _magnifierSize/2, 
                    mousePos.Y - _magnifierSize/2, 
                    _magnifierSize, 
                    _magnifierSize);
            
                // Показываем содержимое обратно
                MagnifierContent.Visibility = Visibility.Visible;
            
                // Увеличиваем
                var scale = 3;
                var zoomed = new TransformedBitmap(screenshot, new ScaleTransform(scale, scale));
                MagnifierContent.Background = new ImageBrush(zoomed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Magnifier error: {ex.Message}");
            }
        });
    }
    
    private BitmapSource CaptureArea(int x, int y, int width, int height)
    {
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
        
        return ConvertToBitmapSource(bitmap);
    }
    
    private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }
    
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
    
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;
        
        _isSelecting = false;
        var mousePos = System.Windows.Forms.Cursor.Position;
        var point = new Point(mousePos.X, mousePos.Y);
        
        ElementSelected?.Invoke(this, point);
        
        _updateTimer.Dispose();
        this.Close();
    }
    
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _isSelecting = false;
            _updateTimer.Dispose();
            SelectionCancelled?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _updateTimer?.Dispose();
        base.OnClosed(e);
    }
}