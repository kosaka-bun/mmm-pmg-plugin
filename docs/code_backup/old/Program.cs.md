```csharp
private static void execute(string path, Hand hand) {
    DataTable data = CSVFileHelper.OpenCSV(path);
    foreach (DataRow row in data.Rows) {
        if(form.willPrintRow) printDataRow(row);
        //获取行数据
        int parse(string head) {
            return int.Parse(row[head].ToString());
        }
        int on = parse("#ON_FRAME");
        int off = parse(" OFF_FRAME");
        int note = parse(" NOTE");
        int finger = parse(" FINGER");
        int wrist = parse(" WRIST_POS");
        //移动手腕
        hand.wristMoveTo(wrist, on);
        //移动手指并弹下
        hand.fingerMoveToAndPress(finger, note, on - 1, on);
        //抬起手指
        hand.liftFinger(finger, off - 2 <= on ? on + 1 : off - 2, 
            off);
    }
}
```