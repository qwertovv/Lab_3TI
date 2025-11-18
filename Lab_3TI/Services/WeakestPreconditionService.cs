namespace LoopVerification.Services
{
    public static class WeakestPreconditionService
    {
        public static string CalculateWP(string statement, string postCondition)
        {
            if (string.IsNullOrEmpty(statement) || string.IsNullOrEmpty(postCondition))
                return "true";

            if (statement.Contains("res += a[j]") && postCondition.Contains("res = Σ_{i=0}^{k-1} a[i]"))
            {
                return "(res + a[j] = Σ_{i=0}^{k-1} a[i]) ∧ (0≤k≤j+1)";
            }

            if (statement.Contains("j++") && postCondition.Contains("0≤k≤j"))
            {
                return postCondition.Replace("j", "j+1");
            }

            if (statement.Contains("if (a[j] > T) res++") && postCondition.Contains("|{i < k: a[i] > T}|"))
            {
                return "((a[j] > T) → (res+1 = |{i < k: a[i] > T}|)) ∧ ((a[j] ≤ T) → (res = |{i < k: a[i] > T}|))";
            }

            if (statement.Contains("res = max(res, a[j])") && postCondition.Contains("res = max(a[0..k))"))
            {
                return "(res ≥ a[j] → res = max(a[0..k))) ∧ (res < a[j] → a[j] = max(a[0..k)))";
            }

            return $"wp({statement}, {postCondition})";
        }

        public static string GetRussianPronunciation(string formula)
        {
            if (formula.Contains("Σ_{i=0}^{k-1} a[i]"))
                return "res равно сумме элементов массива a от 0 до k-1";

            if (formula.Contains("|{i < k: a[i] > T}|"))
                return "res равно количеству элементов массива a с индексом меньше k, которые больше T";

            if (formula.Contains("max(a[0..k))"))
                return "res равно максимальному элементу в массиве a от индекса 0 до k-1";

            return formula;
        }
    }
}