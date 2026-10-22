public static class DamageCalculator
{
    // bonus: 0.5 nghĩa là +50%
    public static float Calculate(float baseDamage, float atk, float bonus)
    {
        return baseDamage * atk * (1f + bonus);
    }
}