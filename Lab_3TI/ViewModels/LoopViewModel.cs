using LoopVerification.Models;
using LoopVerification.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace LoopVerification.ViewModels
{
    public class LoopViewModel : BaseViewModel
    {
        private ArrayModel _array;
        private LoopState _state;
        private LoopMode _currentMode;
        private int _thresholdT;
        private string _invariantText;
        private string _invariantFormula;
        private string _variantFunction;
        private bool _isInvariantValidBefore;
        private bool _isInvariantValidAfter;
        private string _wpVerification;
        private string _wpRussian;

        public ArrayModel Array
        {
            get => _array;
            set { _array = value; OnPropertyChanged(nameof(Array)); }
        }

        public LoopState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(nameof(State)); }
        }

        public LoopMode CurrentMode
        {
            get => _currentMode;
            set { _currentMode = value; UpdateInvariantDescription(); OnPropertyChanged(nameof(CurrentMode)); }
        }

        public int ThresholdT
        {
            get => _thresholdT;
            set { _thresholdT = value; OnPropertyChanged(nameof(ThresholdT)); }
        }

        public string InvariantText
        {
            get => _invariantText;
            set { _invariantText = value; OnPropertyChanged(nameof(InvariantText)); }
        }

        public string InvariantFormula
        {
            get => _invariantFormula;
            set { _invariantFormula = value; OnPropertyChanged(nameof(InvariantFormula)); }
        }

        public string VariantFunction
        {
            get => _variantFunction;
            set { _variantFunction = value; OnPropertyChanged(nameof(VariantFunction)); }
        }

        public bool IsInvariantValidBefore
        {
            get => _isInvariantValidBefore;
            set { _isInvariantValidBefore = value; OnPropertyChanged(nameof(IsInvariantValidBefore)); }
        }

        public bool IsInvariantValidAfter
        {
            get => _isInvariantValidAfter;
            set { _isInvariantValidAfter = value; OnPropertyChanged(nameof(IsInvariantValidAfter)); }
        }

        public string WPVerification
        {
            get => _wpVerification;
            set { _wpVerification = value; OnPropertyChanged(nameof(WPVerification)); }
        }

        public string WPRussian
        {
            get => _wpRussian;
            set { _wpRussian = value; OnPropertyChanged(nameof(WPRussian)); }
        }

        public ICommand GenerateArrayCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand ResetCommand { get; }

        public LoopViewModel()
        {
            _array = new ArrayModel(8);
            _state = new LoopState();
            _currentMode = LoopMode.PrefixSum;
            _thresholdT = 0;

            GenerateArrayCommand = new RelayCommand(param => GenerateArray());
            StepCommand = new RelayCommand(param => ExecuteStep(param), param => CanExecuteStep(param));
            StartCommand = new RelayCommand(param => StartExecution(param), param => CanStartExecution(param));
            ResetCommand = new RelayCommand(param => ResetExecution());

            UpdateInvariantDescription();
        }

        private void UpdateInvariantDescription()
        {
            switch (_currentMode)
            {
                case LoopMode.PrefixSum:
                    InvariantText = "res содержит сумму элементов массива от индекса 0 до j-1";
                    InvariantFormula = "res = Σ_{i=0}^{j-1} a[i] ∧ 0≤j≤n";
                    VariantFunction = "t = n-j";
                    break;
                case LoopMode.CountGreaterThanT:
                    InvariantText = "res содержит количество элементов больше T в диапазоне от 0 до j-1";
                    InvariantFormula = "res = |{i < j: a[i] > T}| ∧ 0≤j≤n";
                    VariantFunction = "t = n-j";
                    break;
                case LoopMode.PrefixMax:
                    InvariantText = "res содержит максимальный элемент в диапазоне от 0 до j-1";
                    InvariantFormula = "res = max(a[0..j)) ∧ 0≤j≤n";
                    VariantFunction = "t = n-j";
                    break;
            }

            UpdateWPVerification();
        }

        private void GenerateArray()
        {
            Array = new ArrayModel(8, -10, 10);
            ResetExecution();
        }

        private void ResetExecution()
        {
            State = new LoopState();
            State.VariantValue = Array.Length - State.Index;
            VerifyInvariant();
        }

        private void ExecuteStep(object parameter)
        {
            if (State.Index >= Array.Length)
                return;

            // Проверяем инвариант перед шагом
            bool invariantBefore = VerifyInvariant();
            State.InvariantBefore = invariantBefore;
            IsInvariantValidBefore = invariantBefore;

            // Выполняем шаг в зависимости от режима
            switch (CurrentMode)
            {
                case LoopMode.PrefixSum:
                    State.Result += Array.Values[State.Index];
                    break;
                case LoopMode.CountGreaterThanT:
                    if (Array.Values[State.Index] > ThresholdT)
                        State.Result++;
                    break;
                case LoopMode.PrefixMax:
                    if (State.Index == 0)
                        State.Result = Array.Values[0];
                    else
                        State.Result = Math.Max(State.Result, Array.Values[State.Index]);
                    break;
            }

            State.Index++;
            State.Steps++;

            // Обновляем вариант-функцию
            State.VariantValue = Array.Length - State.Index;

            // Проверяем инвариант после шага
            bool invariantAfter = VerifyInvariant();
            State.InvariantAfter = invariantAfter;
            IsInvariantValidAfter = invariantAfter;

            // Отладочная проверка инварианта
            Debug.Assert(invariantAfter, "Инвариант нарушен после шага!");

            OnPropertyChanged(nameof(State));

            if (State.Index >= Array.Length)
            {
                State.IsRunning = false;
            }
        }

        private bool CanExecuteStep(object parameter)
        {
            return !State.IsRunning && State.Index < Array.Length;
        }

        private void StartExecution(object parameter)
        {
            State.IsRunning = true;
            while (State.Index < Array.Length)
            {
                ExecuteStep(null);
                Thread.Sleep(500); // Задержка для визуализации
            }
            State.IsRunning = false;
        }

        private bool CanStartExecution(object parameter)
        {
            return !State.IsRunning && State.Index < Array.Length;
        }

        private bool VerifyInvariant()
        {
            if (Array.Values == null || Array.Values.Length == 0)
                return true;

            bool isValid = false;

            switch (CurrentMode)
            {
                case LoopMode.PrefixSum:
                    int sum = 0;
                    for (int i = 0; i < State.Index; i++)
                    {
                        sum += Array.Values[i];
                    }
                    isValid = (State.Result == sum) && (State.Index >= 0) && (State.Index <= Array.Length);
                    break;

                case LoopMode.CountGreaterThanT:
                    int count = 0;
                    for (int i = 0; i < State.Index; i++)
                    {
                        if (Array.Values[i] > ThresholdT)
                            count++;
                    }
                    isValid = (State.Result == count) && (State.Index >= 0) && (State.Index <= Array.Length);
                    break;

                case LoopMode.PrefixMax:
                    if (State.Index == 0)
                        isValid = true; // Пустой префикс
                    else
                    {
                        int max = Array.Values[0];
                        for (int i = 1; i < State.Index; i++)
                        {
                            if (Array.Values[i] > max)
                                max = Array.Values[i];
                        }
                        isValid = (State.Result == max) && (State.Index >= 0) && (State.Index <= Array.Length);
                    }
                    break;
            }

            return isValid;
        }

        private void UpdateWPVerification()
        {
            string statement = "";
            string postCondition = InvariantFormula;

            switch (CurrentMode)
            {
                case LoopMode.PrefixSum:
                    statement = "res += a[j]; j++";
                    break;
                case LoopMode.CountGreaterThanT:
                    statement = "if (a[j] > T) res++; j++";
                    break;
                case LoopMode.PrefixMax:
                    statement = "res = max(res, a[j]); j++";
                    break;
            }

            WPVerification = $"({InvariantFormula} ∧ B) ⇒ {WeakestPreconditionService.CalculateWP(statement, postCondition)}";
            WPRussian = WeakestPreconditionService.GetRussianPronunciation(WPVerification);
        }
    }
}