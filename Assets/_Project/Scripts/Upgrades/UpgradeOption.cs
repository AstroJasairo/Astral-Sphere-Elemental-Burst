using System;
using UnityEngine;

// Một lựa chọn trong màn chọn nâng cấp.
// "Apply" chứa sẵn hành động cần chạy khi người chơi chọn (mẫu thiết kế Command),
// nên UI không cần biết đây là vũ khí hay chỉ số.
public class UpgradeOption
{
    public string Title;
    public string Description;
    public Sprite Icon;
    public Action Apply;
}