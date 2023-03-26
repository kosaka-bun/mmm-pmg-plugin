using System;
using System.Data;
using System.IO;
using System.Text;

namespace PianoPlayingMotionGenerator.Util {

public static class CsvFileHelper {

    /// <summary>
    /// 将CSV文件的数据读取到DataTable中
    /// </summary>
    /// <param name="filePath">CSV文件路径</param>
    /// <returns>返回读取了CSV数据的DataTable</returns>
    public static DataTable openCsv(string filePath) {
        Encoding encoding = getType(filePath); //Encoding.ASCII;
        var dt = new DataTable();
        var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
        var sr = new StreamReader(fs, encoding);
        //string fileContent = sr.ReadToEnd();
        //encoding = sr.CurrentEncoding;
        //记录每次读取的一行记录
        var strLine = "";
        //记录每行记录中的各字段内容
        string[] aryLine = null;
        string[] tableHead = null;
        //标示列数
        var columnCount = 0;
        //标示是否是读取的第一行
        var isFirst = true;
        //逐行读取CSV中的数据
        while((strLine = sr.ReadLine()) != null) {
            //strLine = Common.ConvertStringUTF8(strLine, encoding);
            //strLine = Common.ConvertStringUTF8(strLine);

            if(isFirst) {
                tableHead = strLine.Split(',');
                isFirst = false;
                columnCount = tableHead.Length;
                //创建列
                for(var i = 0; i < columnCount; i++) {
                    var dc = new DataColumn(tableHead[i]);
                    dt.Columns.Add(dc);
                }
            } else {
                aryLine = strLine.Split(',');
                DataRow dr = dt.NewRow();
                for(var j = 0; j < columnCount; j++) dr[j] = aryLine[j];
                dt.Rows.Add(dr);
            }
        }
        if(aryLine != null && aryLine.Length > 0) 
            dt.DefaultView.Sort = tableHead[0] + " " + "asc";
        sr.Close();
        fs.Close();
        return dt;
    }

    /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
    /// <param name="fileName">文件路径</param>
    /// <returns>文件的编码类型</returns>
    private static Encoding getType(string fileName) {
        var fs = new FileStream(fileName, FileMode.Open,
            FileAccess.Read);
        Encoding r = getType(fs);
        fs.Close();
        return r;
    }

    /// 通过给定的文件流，判断文件的编码类型
    /// <param name="fs">文件流</param>
    /// <returns>文件的编码类型</returns>
    private static Encoding getType(FileStream fs) {
        if(fs == null) throw new ArgumentNullException(nameof(fs));
        // byte[] Unicode = { 0xFF, 0xFE, 0x41 };
        // byte[] UnicodeBIG = { 0xFE, 0xFF, 0x00 };
        // byte[] UTF8 = { 0xEF, 0xBB, 0xBF }; //带BOM
        var reVal = Encoding.Default;

        var r = new BinaryReader(fs, Encoding.Default);
        int i;
        int.TryParse(fs.Length.ToString(), out i);
        byte[] ss = r.ReadBytes(i);
        if(isUtf8Bytes(ss) || ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
            reVal = Encoding.UTF8;
        else if(ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            reVal = Encoding.BigEndianUnicode;
        else if(ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            reVal = Encoding.Unicode;
        r.Close();
        return reVal;
    }

    /// 判断是否是不带 BOM 的 UTF8 格式
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool isUtf8Bytes(byte[] data) {
        var charByteCounter = 1; //计算当前正分析的字符应还有的字节数
        byte curByte; //当前分析的字节.
        for(var i = 0; i < data.Length; i++) {
            curByte = data[i];
            if(charByteCounter == 1) {
                if(curByte >= 0x80) {
                    //判断当前
                    while(((curByte <<= 1) & 0x80) != 0) charByteCounter++;
                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                    if(charByteCounter == 1 || charByteCounter > 6) return false;
                }
            } else {
                //若是UTF-8 此时第一位必须为1
                if((curByte & 0xC0) != 0x80) return false;
                charByteCounter--;
            }
        }
        if(charByteCounter > 1) throw new Exception("非预期的byte格式");
        return true;
    }
}

}