using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GhostOverlay.Helpers;

public class HotkeyHelper : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private readonly Window _window;
    private readonly int _hotkeyId;
    private HwndSource? _source;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public event EventHandler? HotkeyPressed;

    public HotkeyHelper(Window window, ModifierKeys modifiers, Key key)
    {
        _window = window;
        _hotkeyId = GetHashCode();

        var helper = new WindowInteropHelper(window);

        // Wait for window to be loaded
        if (helper.Handle == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) => Initialize(modifiers, key);
        }
        else
        {
            Initialize(modifiers, key);
        }
    }

    private void Initialize(ModifierKeys modifiers, Key key)
    {
        var helper = new WindowInteropHelper(_window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(HwndHook);

        RegisterHotKey(
            helper.Handle,
            _hotkeyId,
            (uint)modifiers,
            (uint)KeyInterop.VirtualKeyFromKey(key)
        );
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        _source?.RemoveHook(HwndHook);
        var helper = new WindowInteropHelper(_window);
        if (helper.Handle != IntPtr.Zero)
        {
            UnregisterHotKey(helper.Handle, _hotkeyId);
        }
    }
}
