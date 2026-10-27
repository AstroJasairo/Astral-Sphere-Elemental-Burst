using UnityEngine;

// Cộng dồn phần lẻ khi nhân số nguyên với hệ số. Trung bình ra đúng bằng giá trị x hệ số.
public class RemainderAccumulator
{
    private float remainder;

    public int Add(float value)
    {
        float total = value + remainder;
        int whole = Mathf.FloorToInt(total);   // Phần nguyên trả ra ngay
        remainder = total - whole;             // Phần lẻ giữ lại cho lần sau
        return whole;
    }
}