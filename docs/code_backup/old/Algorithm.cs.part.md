```csharp
/// <summary>
/// 取两个集合的交集
/// </summary>
/// <param name="list1"></param>
/// <param name="list2"></param>
/// <returns></returns>
private static List<int> getIntersectionOfTwoList(List<int> list1, List<int> list2) {
    var result = new List<int>();
    foreach (int i in list1) {
        foreach (int j in list2) {
            if (i == j && !result.Contains(i)) result.Add(i);
        }
    }
    return result;
}

/// <summary>
/// 基于右手，计算从较高单音到较低单音时，较低单音推荐使用的指法
/// 需保证：前一个单音高于后一个单音
/// </summary>
/// <param name="prev"></param>
/// <param name="note"></param>
/// <returns></returns>
public List<int> calcLowerByHigher(SingleNote prev, SingleNote note) {
    //用于获取一个手指在推荐指法集合中的优先级
    int getLevel(List<int> list, int finger) {
        for (var i = 0; i < list.Count; i++) {
            if (list[i] == finger) {
                //优先级取决于手指在集合中的位置及集合的长度
                //集合越大，能选择的指法越多，越靠前的手指越推荐
                //这个优先级等于在集合各元素前面补位，补齐四个后，当前手指的索引
                //TODO 更新优先级算法
                //return i + (4 - list.Count);
                return i;
            }
        }
        //若不在集合中
        throw new Exception("要查找索引的值不在集合中");
    }
    
    //反推法
    //可以复制这个较低音，依次尝试使用5个手指去为较低音的指法赋值
    //然后用这个较低音推导较高音的指法，若推导出的推荐指法中含有较高音的真实指法，则此指法可行
    List<int> fingers = newList();
    var fingersWithLevel = new Dictionary<int, int>();    //带有优先级的手指列表
    var low = new SingleNote(0, 0, note.note);
    for (var finger = 1; finger <= 5; finger++) {
        //若已被使用，则跳过
        if(finger == prev.finger) continue;
        //将此手指号安排给复制的较低音
        low.finger = finger;
        //用复制的较低音去推导较高音的指法
        List<int> result = calcHigherByLower(low, prev);
        //若结果中含有较高音的真实指法，则证明较低音用此指法能够到达较高音
        if(result.Contains(prev.finger))
            fingersWithLevel.Add(finger, getLevel(result, prev.finger));
    }
    //将带有优先级的手指列表按优先级顺序排序
    //先按优先级正序排序，优先级相同的再按手指号倒序
    IOrderedEnumerable<KeyValuePair<int, int>> orderedDic = 
        from pair in fingersWithLevel orderby pair.Value select pair;
    //存入集合中
    foreach (KeyValuePair<int, int> pair in orderedDic) {
        fingers.Add(pair.Key);
    }
    return fingers;
}
```
