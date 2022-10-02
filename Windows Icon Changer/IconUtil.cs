using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace Windows.IconChanger;

public static class IconUtil
{
    /// <summary>
    /// Set folder icon for a given folder. and instantly refreshes it.
    /// </summary>
    /// <param name="icoFile"> full path to the icon file [MUST BE .Ico]</param>
    /// <param name="folderPath">path to the folder</param>
    /// <param name="setHiddenAndSystem">if this ico file is to be set hidden and system, default is true, recommended</param>
    public static void SetFolderIcon(string icoFile, string folderPath,bool setHiddenAndSystem=true)
    {
        if (!File.Exists(icoFile))
        {
            throw new ArgumentException($"Icon File does not exist at {icoFile}");
        }

        if (!Directory.Exists(folderPath))
        {
            throw new ArgumentException($"Folder Path is not valid: {folderPath}");
        }

        var newIcoPath = Path.Combine(folderPath, Path.GetFileName(icoFile));
        if (!File.Exists(newIcoPath))
        {
            File.Copy(icoFile,newIcoPath);
        }
        icoFile = Path.GetFileName(icoFile);
        var folderSettings = new Shell32.SHFOLDERCUSTOMSETTINGS
        {
            dwMask = Shell32.FOLDERCUSTOMSETTINGSMASK.FCSM_ICONFILE,
            pszIconFile = icoFile,
            dwSize = (uint)Marshal.SizeOf(typeof(Shell32.SHFOLDERCUSTOMSETTINGS)),
            cchIconFile = 0
        };
        //FolderSettings.iIconIndex = 0;
        var unused = Shell32.SHGetSetFolderCustomSettings(ref folderSettings, folderPath, 
            Shell32.FCS.FCS_FORCEWRITE);
        Shell32.SHChangeNotify(Shell32.SHCNE.SHCNE_UPDATEDIR, Shell32.SHCNF.SHCNF_PATHW, folderPath);
        if (setHiddenAndSystem)
        {
            SetHiddenAndSystem(newIcoPath);
        }
    }

    public static void RefreshIconCache()
    {
        _ = Kernel32.Wow64DisableWow64FsRedirection(out _);
        var objProcess = new Process
        {
            StartInfo =
            {
                FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) +
                           "\\ie4uinit.exe",
                Arguments = "-ClearIconCache",
                WindowStyle = ProcessWindowStyle.Normal
            }
        };
        objProcess.Start();
        objProcess.WaitForExit();
        objProcess.Close();
        Kernel32.Wow64EnableWow64FsRedirection(true);
    }
    /// <summary>
    /// Deletes set folder icon
    /// </summary>
    /// <param name="iconFullPath">Full path to icon file. eg- K:\movies\War\1917\1917.ico</param>
    public static void DeleteIconsFromFolder(string iconFullPath)
    {
        if (!File.Exists(iconFullPath))
        {
            throw new InvalidOperationException("Icon file must exist");
        }
        var folderName = Path.GetDirectoryName(iconFullPath);
        var iniFile = Path.Combine(folderName ?? throw new InvalidOperationException("Path must exist"), "desktop.ini");
        File.Delete(iconFullPath);
        File.Delete(iniFile);
        Shell32.SHChangeNotify(Shell32.SHCNE.SHCNE_UPDATEDIR, Shell32.SHCNF.SHCNF_PATHW, folderName);
    }

    private static void SetHiddenAndSystem(string filePath)
    {
        // Set file attribute to "Hidden"
        if ((File.GetAttributes(filePath) & FileAttributes.Hidden) != FileAttributes.Hidden)
        {
            File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
        }
    
        // Set file attribute to "System"
        if ((File.GetAttributes(filePath) & FileAttributes.System) != FileAttributes.System)
        {
            File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.System);
        }
            
    }
}