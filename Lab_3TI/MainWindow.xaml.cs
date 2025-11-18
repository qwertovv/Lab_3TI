using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;

namespace LoopInvariantChecker
{
    public partial class MainWindow : Window
    {
        private int[] array = new int[0];
        private int threshold = 50;
        private int j = 0;
        private int res = 0;
        private int variantFunction = 0;
        private bool isCompleted = false;
        private bool isRunning = false;
        private string currentMode = "Prefix Sum";

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            // Инициализация комбобокса
            ModeComboBox.SelectedIndex = 0;

            // Обновление интерфейса
            UpdateInterface();
            AddLog("Приложение запущено");
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int size = int.Parse(ArraySizeBox.Text);
                int min = int.Parse(MinValueBox.Text);
                int max = int.Parse(MaxValueBox.Text);
                threshold = int.Parse(ThresholdBox.Text);

                var random = new Random();
                array = new int[size];
                ArrayListBox.Items.Clear();

                for (int i = 0; i < size; i++)
                {
                    array[i] = random.Next(min, max + 1);
                    ArrayListBox.Items.Add($"a[{i}] = {array[i]}");
                }

                InitializeLoop();
                AddLog($"Сгенерирован массив размера {size}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeLoop();
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            Step();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) return;

            isRunning = true;
            try
            {
                while (!isCompleted && isRunning)
                {
                    Step();
                    await Task.Delay(500);
                }
            }
            finally
            {
                isRunning = false;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedItem is ComboBoxItem item)
            {
                currentMode = item.Content.ToString();
                InitializeLoop();
                UpdateInterface();
            }
        }

        private void InitializeLoop()
        {
            j = 0;
            isCompleted = false;

            switch (currentMode)
            {
                case "Prefix Sum":
                    res = 0;
                    break;
                case "Count > T":
                    res = 0;
                    break;
                case "Prefix Max":
                    res = int.MinValue;
                    break;
            }

            variantFunction = array.Length;
            UpdateInterface();
            AddLog("Цикл инициализирован");
        }

        private void Step()
        {
            if (isCompleted) return;

            // Проверка инварианта до шага
            bool invariantBefore = CheckInvariant();
            InvBeforeCircle.Fill = invariantBefore ? Brushes.Green : Brushes.Red;
            System.Diagnostics.Debug.Assert(invariantBefore, "Инвариант нарушен до выполнения шага");

            // Выполнение шага
            ExecuteStep();

            // Проверка инварианта после шага
            bool invariantAfter = CheckInvariant();
            InvAfterCircle.Fill = invariantAfter ? Brushes.Green : Brushes.Red;
            System.Diagnostics.Debug.Assert(invariantAfter, "Инвариант нарушен после выполнения шага");

            // Обновление вариант-функции
            int oldVariant = variantFunction;
            variantFunction = array.Length - j;
            AddLog($"Вариант-функция: {oldVariant} -> {variantFunction}");

            // Проверка завершения
            if (j >= array.Length)
            {
                isCompleted = true;
                AddLog("Цикл завершен");
            }

            UpdateInterface();
        }

        private void ExecuteStep()
        {
            if (j >= array.Length) return;

            switch (currentMode)
            {
                case "Prefix Sum":
                    res += array[j];
                    j++;
                    break;

                case "Count > T":
                    if (array[j] > threshold)
                        res++;
                    j++;
                    break;

                case "Prefix Max":
                    if (j == 0 || array[j] > res)
                        res = array[j];
                    j++;
                    break;
            }

            AddLog($"Выполнен шаг: j={j}, res={res}");
        }

        private bool CheckInvariant()
        {
            if (j < 0 || j > array.Length) return false;

            switch (currentMode)
            {
                case "Prefix Sum":
                    return CheckPrefixSumInvariant();
                case "Count > T":
                    return CheckCountInvariant();
                case "Prefix Max":
                    return CheckPrefixMaxInvariant();
                default:
                    return false;
            }
        }

        private bool CheckPrefixSumInvariant()
        {
            int expectedSum = 0;
            for (int i = 0; i < j; i++)
            {
                expectedSum += array[i];
            }
            return res == expectedSum;
        }

        private bool CheckCountInvariant()
        {
            int expectedCount = 0;
            for (int i = 0; i < j; i++)
            {
                if (array[i] > threshold)
                    expectedCount++;
            }
            return res == expectedCount;
        }

        private bool CheckPrefixMaxInvariant()
        {
            if (j == 0) return res == int.MinValue;

            int expectedMax = array[0];
            for (int i = 1; i < j; i++)
            {
                if (array[i] > expectedMax)
                    expectedMax = array[i];
            }
            return res == expectedMax;
        }

        private void UpdateInterface()
        {
            // Обновление текстов
            JText.Text = $"j = {j}";
            ResText.Text = $"res = {res}";
            VariantText.Text = $"Вариант-функция: {variantFunction}";

            // Прогресс-бар
            VariantProgress.Maximum = array.Length;
            VariantProgress.Value = variantFunction;

            // Статус
            StatusModeText.Text = $"Режим: {currentMode}";
            StatusCompletedText.Text = $"Завершено: {isCompleted}";

            // Инварианты
            UpdateInvariantTexts();
        }

        private void UpdateInvariantTexts()
        {
            switch (currentMode)
            {
                case "Prefix Sum":
                    InvVerbalText.Text = "res содержит сумму элементов a[0] до a[j-1]";
                    InvFormulaText.Text = "res = Σ_{i=0}^{j-1} a[i] ∧ 0 ≤ j ≤ n";
                    break;
                case "Count > T":
                    InvVerbalText.Text = "res содержит количество элементов > T в a[0..j-1]";
                    InvFormulaText.Text = "res = |{ i < j : a[i] > T }| ∧ 0 ≤ j ≤ n";
                    break;
                case "Prefix Max":
                    InvVerbalText.Text = "res содержит максимальный элемент в a[0..j-1]";
                    InvFormulaText.Text = "res = max(a[0..j-1]) ∧ 0 ≤ j ≤ n";
                    break;
            }

            WpFormulaText.Text = "(Inv ∧ B) ⇒ wp(S, Inv)";
            WpVerbalText.Text = "Если инвариант истинен и условие цикла выполняется, то после выполнения тела цикла инвариант сохранится";
        }

        private void AddLog(string message)
        {
            LogListBox.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
            if (LogListBox.Items.Count > 50)
                LogListBox.Items.RemoveAt(LogListBox.Items.Count - 1);
        }
        // Добавь эти методы в конец класса MainWindow:

        public int TestPrefixSum(int[] testArray)
        {
            array = testArray;
            InitializeLoop();

            while (!isCompleted)
            {
                Step();
            }

            return res;
        }

        public int TestCountGreaterThanT(int[] testArray, int testThreshold)
        {
            array = testArray;
            threshold = testThreshold;
            InitializeLoop();

            while (!isCompleted)
            {
                Step();
            }

            return res;
        }

        public int TestPrefixMax(int[] testArray)
        {
            array = testArray;
            InitializeLoop();

            while (!isCompleted)
            {
                Step();
            }

            return res;
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HelpWindow helpWindow = new HelpWindow();
                helpWindow.Owner = this; // Устанавливаем главное окно как владельца
                helpWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                helpWindow.ShowDialog(); // Показываем как модальное окно
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть справку: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}