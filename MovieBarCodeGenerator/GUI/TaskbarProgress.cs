using System.Runtime.InteropServices;

namespace MovieBarCodeGenerator.GUI;

public static class TaskbarProgress
{
    public enum TaskbarStates
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8,
    }

    [ComImport]
    [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        [PreserveSig]
        void HrInit();

        [PreserveSig]
        void AddTab(IntPtr hwnd);

        [PreserveSig]
        void DeleteTab(IntPtr hwnd);

        [PreserveSig]
        void ActivateTab(IntPtr hwnd);

        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

        [PreserveSig]
        void SetProgressState(IntPtr hwnd, TaskbarStates state);
    }

    [ComImport]
    [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
    [ClassInterface(ClassInterfaceType.None)]
    private class TaskbarInstance
    {
    }

    private static readonly ITaskbarList3 taskbarInstance = (ITaskbarList3)new TaskbarInstance();
    private static readonly bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

    public static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
    {
        if (taskbarSupported) taskbarInstance.SetProgressState(windowHandle, taskbarState);
    }

    public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
    {
        if (taskbarSupported) taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
    }
}
