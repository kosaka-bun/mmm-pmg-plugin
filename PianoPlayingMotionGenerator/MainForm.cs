using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable UnassignedField.Global
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace PianoPlayingMotionGenerator {

public partial class MainForm : Form, Printable {

    //窗口被加载时要执行的内容
    private void Form1_Load(object sender, EventArgs e) {
        try {
            printer = new MainFormPrinter(this);
            printer.println("Loading...");
            Program.loadData();
            Program.printData();
            printer.println("Loaded");
            executeBtn.Enabled = true;
        } catch(Exception ex) {
            println("未成功加载，请重新打开窗口，错误信息如下：");
            println(ex);
        }
    }

    //点击按钮，程序开始运行时要执行的内容
    private void execute() {
        try {
            Program.execute();
        } catch(Exception e) {
            println("生成过程中出现错误，错误信息如下：");
            println(e);
        }
    }

    /* 如果第一次点击事件还没处理完成，出现了第二次点击事件，则第二次点击事件将在第一次
       点击事件处理完成后才会被处理，不会多线程同时处理两个点击事件。 */
    private void executeBtn_Click(object sender, EventArgs e) {
        //按钮被点击时，关闭部分可能影响执行过程的控件
        leftHandFile.Enabled = false;
        rightHandFile.Enabled = false;
        newThreadInvoke(sender, () => {
            execute();
            leftHandFile.Enabled = true;
            rightHandFile.Enabled = true;
        });
    }

    /// <summary>
    /// 关闭被触发的控件，并在新线程中执行控件触发动作，执行完成后开启控件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ac"></param>
    private void newThreadInvoke(object sender, Action ac) {
        var control = (Control)sender;
        control.Enabled = false;
        new Thread(() => {
            ac();
            control.Enabled = true;
        }).Start();
    }

    private void rightHandFile_Click(object sender, EventArgs e) {
        DialogResult result = handFileDialog.ShowDialog();
        if(result != DialogResult.OK) return;
        ((TextBox)sender).Text = handFileDialog.SafeFileName;
        Program.rightPath = handFileDialog.FileName;
        println("右手：" + Program.rightPath);
    }

    private void leftHandFile_Click(object sender, EventArgs e) {
        DialogResult result = handFileDialog.ShowDialog();
        if(result != DialogResult.OK) return;
        ((TextBox)sender).Text = handFileDialog.SafeFileName;
        Program.leftPath = handFileDialog.FileName;
        println("左手：" + Program.leftPath);
    }

    private void handFileDialog_FileOk(object sender, CancelEventArgs e) {
        var dialog = (OpenFileDialog) sender;
        dialog.InitialDirectory = dialog.FileName;
    }

    public void clear() {
        //content.Clear();
        console.Clear();
    }

    public void println(object str) {
        print(str);
        println();
    }

    public void println() {
        print("\r\n");
    }

    public void print(object str) {
        console.AppendText(str.ToString());
    }

    private void printRowCheckBox_CheckedChanged(object sender, EventArgs e) {
        var checkBox = (CheckBox)sender;
        if(checkBox.Checked)
            println("不建议开启，会大幅降低计算速度");
    }

    public MainForm() {
        InitializeComponent();
        //将默认宽度高度设置为最小宽度高度
        MinimumSize = Size;
        //设置文件拾取对话框默认目录为桌面
        handFileDialog.InitialDirectory = Environment.GetFolderPath(
            Environment.SpecialFolder.Desktop);
    }
    
    public static MainFormPrinter printer;

    //private readonly StringBuilder content = new StringBuilder();

    //是否运行测试内容
    public bool willRunTest => runTestCheckBox.Checked;

    //是否允许在执行时输出每行信息
    public bool willPrintRow => printRowCheckBox.Checked;
}

public interface Printable {

    void print(object str);

    void clear();

    void println();

    void println(object str);
}

public class MainFormPrinter : Printable {

    private readonly MainForm mainForm;

    private readonly Queue<object> textQueue = new Queue<object>();

    public bool willStop;

    public MainFormPrinter(MainForm mainForm) {
        this.mainForm = mainForm;
        var printThread = new Thread(doPrint);
        printThread.Start();
    }

    private void doPrint() {
        void printAction(object obj) {
            mainForm.print(obj);
        }
        for(; ; ) {
            if(willStop) break;
            if(!mainForm.Visible) break;
            try {
                object obj = textQueue.Dequeue();
                mainForm.Invoke((Action<object>) printAction, obj);
            } catch(InvalidOperationException ioe) {
                Thread.Sleep(100);
            }
        }
    }

    public void print(object str) {
        lock(this) {
            textQueue.Enqueue(str);
        }
    }

    public void clear() {
        lock(this) {
            textQueue.Clear();
            mainForm.Invoke(new Action(() => {
                mainForm.clear();
            }));
        }
    }

    public void println() {
        lock(this) {
            print("\r\n");
        }
    }

    public void println(object str) {
        lock(this) {
            print(str);
            print("\r\n");
        }
    }
}

}