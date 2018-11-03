using System;

namespace MdlxViewer
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static string[] args_ = new string[0];
        public static View MainView;
        [STAThread]
        static void Main(string[] args)
        {
            //args = new string[] { @"D:\Jeux\Kingdom Hearts\app_KH2Tools\export\obj\F_EH560.mdlx" };
            //args = new string[] { @"P_EX100.mdlx" };
            args_ = args;
            System.Diagnostics.Process[] already = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            foreach (System.Diagnostics.Process p in already)
            {
                if (p.Id!= System.Diagnostics.Process.GetCurrentProcess().Id)
                {
                    p.Kill();
                }
            }
            MainView = new View();
            MainView.Run();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
