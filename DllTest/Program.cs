using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using PianoPlayingMotionGenerator;

namespace DllTest {

class Program {

    [STAThread]
    static void Main(string[] args) {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
        //new CommandPluginImpl().Run(null);
    }
}

}