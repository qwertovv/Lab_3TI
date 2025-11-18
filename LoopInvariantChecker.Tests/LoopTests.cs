using Xunit;
using System;
using System.Windows; 

namespace LoopInvariantChecker.Tests
{
    public class LoopTests
    {
        

        [Fact]
        public void CountGreaterThanT_PostCondition_CorrectCount()
        {
            // Arrange
            int[] testArray = { 1, 5, 3, 6, 2 };
            int threshold = 3;
            int expectedCount = 2;

            // Act
            int actualCount = SimulateCountGreaterThanT(testArray, threshold);

            // Assert
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void PrefixMax_PostCondition_CorrectMaxValue()
        {
            // Arrange
            int[] testArray = { 1, 5, 3, 6, 2 };
            int expectedMax = 6;

            // Act
            int actualMax = SimulatePrefixMax(testArray);

            // Assert
            Assert.Equal(expectedMax, actualMax);
        }

        [Fact]
        public void VariantFunction_DecreasesMonotonically()
        {
            // Arrange
            int[] testArray = { 1, 2, 3 };
            int initialVariant = testArray.Length;

            // Act
            var variantHistory = SimulateVariantFunctionDecrease(testArray);

            // Assert
            for (int i = 1; i < variantHistory.Count; i++)
            {
                Assert.True(variantHistory[i] < variantHistory[i - 1],
                    $"Вариант-функция должна монотонно убывать. Шаг {i}: {variantHistory[i - 1]} -> {variantHistory[i]}");
            }
            Assert.Equal(0, variantHistory[variantHistory.Count - 1]);
        }

        [Theory]
        [InlineData(new int[] { 1, 2, 3 }, 6)]
        [InlineData(new int[] { 0, 0, 0 }, 0)]
        [InlineData(new int[] { -1, 1, -1 }, -1)]
        [InlineData(new int[] { 10 }, 10)]
        public void PrefixSum_VariousArrays_CorrectResult(int[] array, int expectedSum)
        {
            // Act
            int result = SimulatePrefixSum(array);

            // Assert
            Assert.Equal(expectedSum, result);
        }

        [Fact]
        public void EmptyArray_PrefixSum_ReturnsZero()
        {
            // Arrange
            int[] emptyArray = new int[0];

            // Act
            int result = SimulatePrefixSum(emptyArray);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void SingleElementArray_PrefixMax_ReturnsElement()
        {
            // Arrange
            int[] singleElementArray = { 42 };

            // Act
            int result = SimulatePrefixMax(singleElementArray);

            // Assert
            Assert.Equal(42, result);
        }

        // Вспомогательные методы для симуляции логики циклов
        private int SimulatePrefixSum(int[] array)
        {
            int j = 0;
            int res = 0;
            int variantFunction = array.Length;

            while (j < array.Length)
            {
                // Проверяем, что вариант-функция >= 0
                Assert.True(variantFunction >= 0, "Вариант-функция должна быть >= 0");

                // Сохраняем инвариант: res = сумма array[0..j-1]
                int expectedSum = 0;
                for (int i = 0; i < j; i++)
                {
                    expectedSum += array[i];
                }
                Assert.Equal(expectedSum, res);

                // Выполняем шаг
                res += array[j];
                j++;
                variantFunction = array.Length - j;

                // Проверяем инвариант после шага
                expectedSum = 0;
                for (int i = 0; i < j; i++)
                {
                    expectedSum += array[i];
                }
                Assert.Equal(expectedSum, res);
            }

            return res;
        }

        private int SimulateCountGreaterThanT(int[] array, int threshold)
        {
            int j = 0;
            int res = 0;
            int variantFunction = array.Length;

            while (j < array.Length)
            {
                // Проверяем вариант-функцию
                Assert.True(variantFunction >= 0, "Вариант-функция должна быть >= 0");

                // Проверяем инвариант: res = количество элементов > threshold в array[0..j-1]
                int expectedCount = 0;
                for (int i = 0; i < j; i++)
                {
                    if (array[i] > threshold)
                        expectedCount++;
                }
                Assert.Equal(expectedCount, res);

                // Выполняем шаг
                if (array[j] > threshold)
                    res++;
                j++;
                variantFunction = array.Length - j;

                // Проверяем инвариант после шага
                expectedCount = 0;
                for (int i = 0; i < j; i++)
                {
                    if (array[i] > threshold)
                        expectedCount++;
                }
                Assert.Equal(expectedCount, res);
            }

            return res;
        }

        private int SimulatePrefixMax(int[] array)
        {
            int j = 0;
            int res = int.MinValue;
            int variantFunction = array.Length;

            while (j < array.Length)
            {
                // Проверяем вариант-функцию
                Assert.True(variantFunction >= 0, "Вариант-функция должна быть >= 0");

                // Проверяем инвариант: res = максимум array[0..j-1]
                if (j == 0)
                {
                    Assert.Equal(int.MinValue, res);
                }
                else
                {
                    int expectedMax = array[0];
                    for (int i = 1; i < j; i++)
                    {
                        if (array[i] > expectedMax)
                            expectedMax = array[i];
                    }
                    Assert.Equal(expectedMax, res);
                }

                // Выполняем шаг
                if (j == 0 || array[j] > res)
                    res = array[j];
                j++;
                variantFunction = array.Length - j;

                // Проверяем инвариант после шага
                if (j > 0)
                {
                    int expectedMax = array[0];
                    for (int i = 1; i < j; i++)
                    {
                        if (array[i] > expectedMax)
                            expectedMax = array[i];
                    }
                    Assert.Equal(expectedMax, res);
                }
            }

            return res;
        }

        private System.Collections.Generic.List<int> SimulateVariantFunctionDecrease(int[] array)
        {
            var variants = new System.Collections.Generic.List<int>();
            int j = 0;
            int variantFunction = array.Length;

            variants.Add(variantFunction);

            while (j < array.Length)
            {
                j++;
                variantFunction = array.Length - j;
                variants.Add(variantFunction);
            }

            return variants;
        }
    }

    public class InvariantTests
    {
        [Fact]
        public void PrefixSum_Invariant_HoldsDuringExecution()
        {
            // Arrange
            int[] array = { 1, 2, 3, 4, 5 };

            // Act & Assert
            TestInvariantHoldsDuringExecution(array, "PrefixSum");
        }

        [Fact]
        public void CountGreaterThanT_Invariant_HoldsDuringExecution()
        {
            // Arrange
            int[] array = { 1, 5, 3, 6, 2 };
            int threshold = 3;

            // Act & Assert
            TestInvariantHoldsDuringExecution(array, "CountGreaterThanT", threshold);
        }

        [Fact]
        public void PrefixMax_Invariant_HoldsDuringExecution()
        {
            // Arrange
            int[] array = { 1, 5, 3, 6, 2 };

            // Act & Assert
            TestInvariantHoldsDuringExecution(array, "PrefixMax");
        }

        private void TestInvariantHoldsDuringExecution(int[] array, string mode, int threshold = 0)
        {
            int j = 0;
            int res = GetInitialValue(mode);

            while (j <= array.Length)
            {
                // Проверяем инвариант в каждой точке
                bool invariantHolds = CheckInvariant(array, j, res, mode, threshold);
                Assert.True(invariantHolds, $"Инвариант нарушен на шаге j={j}, res={res}");

                if (j < array.Length)
                {
                    // Выполняем один шаг
                    switch (mode)
                    {
                        case "PrefixSum":
                            res += array[j];
                            break;
                        case "CountGreaterThanT":
                            if (array[j] > threshold)
                                res++;
                            break;
                        case "PrefixMax":
                            if (j == 0 || array[j] > res)
                                res = array[j];
                            break;
                    }
                    j++;
                }
                else
                {
                    break;
                }
            }
        }

        private int GetInitialValue(string mode)
        {
            return mode switch
            {
                "PrefixSum" => 0,
                "CountGreaterThanT" => 0,
                "PrefixMax" => int.MinValue,
                _ => 0
            };
        }

        private bool CheckInvariant(int[] array, int j, int res, string mode, int threshold)
        {
            if (j < 0 || j > array.Length)
                return false;

            return mode switch
            {
                "PrefixSum" => CheckPrefixSumInvariant(array, j, res),
                "CountGreaterThanT" => CheckCountInvariant(array, j, res, threshold),
                "PrefixMax" => CheckPrefixMaxInvariant(array, j, res),
                _ => false
            };
        }

        private bool CheckPrefixSumInvariant(int[] array, int j, int res)
        {
            int expectedSum = 0;
            for (int i = 0; i < j; i++)
            {
                expectedSum += array[i];
            }
            return res == expectedSum;
        }

        private bool CheckCountInvariant(int[] array, int j, int res, int threshold)
        {
            int expectedCount = 0;
            for (int i = 0; i < j; i++)
            {
                if (array[i] > threshold)
                    expectedCount++;
            }
            return res == expectedCount;
        }

        private bool CheckPrefixMaxInvariant(int[] array, int j, int res)
        {
            if (j == 0)
                return res == int.MinValue;

            int expectedMax = array[0];
            for (int i = 1; i < j; i++)
            {
                if (array[i] > expectedMax)
                    expectedMax = array[i];
            }
            return res == expectedMax;
        }
    }
}