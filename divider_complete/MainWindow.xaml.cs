using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace divider_complete
{
    public class ResultRow
    {
        public string divident { get; set; }
        public string divider { get; set; }
        public string result { get; set; }
        public string reminder { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    public class MainVM : INotifyPropertyChanged
    {
        public MainVM()
        {

        }

        private List<char> numberMap = new List<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C'
        };

        private List<int> _modulusList = new List<int>()
        {
            2, 3, 5, 7, 11, 13
        };
        public List<string> ModulusList
        {
            get
            {
                return _modulusList.Select(modulus => modulus.ToString()).ToList();
            }
        }

        private List<ResultRow> _resultTable = new List<ResultRow>();
        public List<ResultRow> ResultTable
        {
            get
            {
                return _resultTable;
            }
            set
            {
                _resultTable = value;
                OnPropertyChanged();
            }
        }

        private int _modulus = 2;
        public string Modulus
        {
            get
            {
                return _modulus.ToString();
            }
            set
            {
                _modulus = Convert.ToInt32(value);
                OnPropertyChanged();
            }
        }

        private bool _singleMode = true;
        public bool SingleMode
        {
            get
            {
                return _singleMode;
            }
            set
            {
                _singleMode = value;
                OnPropertyChanged();
            }
        }

        private string _dividentPath = "";
        public string DividentPath
        {
            get
            {
                return _dividentPath;
            }
            set
            {
                _dividentPath = value;
                OnPropertyChanged();
            }
        }

        private string _divident = "";
        public string Divident
        {
            get
            {
                return _divident;
            }
            set
            {
                _divident = Regex.Replace(value, GetRegex(), "");
                OnPropertyChanged();
            }
        }

        private string _divider = "";
        public string Divider
        {
            get
            {
                return _divider;
            }
            set
            {
                _divider = Regex.Replace(value, GetRegex(), "");
                OnPropertyChanged();
            }
        }

        private string _result = "";
        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        private string _reminder = "";
        public string Reminder
        {
            get
            {
                return _reminder;
            }
            set
            {
                _reminder = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _openDividentFile;
        public RelayCommand OpenDividentFile
        {
            get
            {
                return _openDividentFile ?? (_openDividentFile = new RelayCommand(command =>
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog =
                        new System.Windows.Forms.OpenFileDialog())
                    {
                        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFileDialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt";
                        openFileDialog.FilterIndex = 2;
                        openFileDialog.RestoreDirectory = true;

                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            DividentPath = openFileDialog.FileName;
                        }
                    }
                },
                command => true));
            }
        }

        private RelayCommand _startDivision;
        public RelayCommand StartDivision
        {
            get
            {
                return _startDivision ?? (_startDivision = new RelayCommand(command =>
                {
                    if (_singleMode) Divide();
                    else MultipleDivide();
                },
                command => true));
            }
        }

        private void Divide()
        {
            if (_divident == "" || _divider == "")
            {
                MessageBox.Show("Поле делимого или делителя пустое!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //if (_divident[0] != '1' || _divider[0] != '1')
            //{
            //    MessageBox.Show("Введен ненормированный полином!", "Ошибка",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            string divident_buffer = _divident;
            string result_buffer = "";
            while (divident_buffer.Length >= _divider.Length)
            {
                int multiplier = Convert.ToInt32(divident_buffer.Substring(0, 1), 16) / numberMap.IndexOf(_divider[0]);
                result_buffer += numberMap[multiplier];
                string divider_buffer = Multiply(_divider, multiplier);
                divident_buffer = Substract(divident_buffer, divider_buffer);
                if (divident_buffer.Length > 0)
                    if (divident_buffer[0] == '0' && divident_buffer.Length - 1 < _divider.Length)
                        divident_buffer = divident_buffer.TrimStart('0');
                if (divident_buffer.Length == _divider.Length && numberMap.IndexOf(divident_buffer[0]) < numberMap.IndexOf(_divider[0])) break;
            }

            if (divident_buffer.Count(x => x != numberMap[0]) == 0 && divident_buffer.Length > 1)
                divident_buffer = "0";

            if (_divident.Length < _divider.Length || numberMap.IndexOf(_divider[0]) > numberMap.IndexOf(_divident[0]))
            {
                divident_buffer = "-";
                result_buffer = "-";
            }
            
            Reminder = divident_buffer == "" ? "0" : divident_buffer == "0" ? divident_buffer : divident_buffer.TrimStart('0');
            Result = result_buffer;
        }

        private void MultipleDivide()
        {
            List<ResultRow> results = new List<ResultRow>();

            if (_dividentPath == "")
            {
                MessageBox.Show("Путь к файлу с делимыми пуст!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_divider == "")
            {
                MessageBox.Show("Поле делителя пустое!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //if (_divider[0] != '1')
            //{
            //    MessageBox.Show("Введен ненормированный полином в поле делителя!", "Ошибка",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            using (System.IO.StreamReader reader = new System.IO.StreamReader(_dividentPath))
            {
                string divident;
                while ((divident = reader.ReadLine()) != null)
                {
                    //if (divident[0] != '1')
                    //{
                    //    MessageBox.Show("В массиве есть ненормированный полином - " + divident + "!", "Ошибка",
                    //        MessageBoxButton.OK, MessageBoxImage.Error);
                    //    continue;
                    //}
                    if (divident.Count(x => x >= (numberMap[_modulus - 1] + 1)) > 0)
                    {
                        MessageBox.Show("В массиве есть полином неподходящий для GF(" + _modulus + ") - " + divident + "!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    string divident_buffer = divident;
                    string result_buffer = "";

                    while (divident_buffer.Length >= _divider.Length)
                    {
                        int multiplier = Convert.ToInt32(divident_buffer.Substring(0, 1), 16) / numberMap.IndexOf(_divider[0]);
                        result_buffer += numberMap[multiplier];
                        string divider_buffer = Multiply(_divider, multiplier);
                        divident_buffer = Substract(divident_buffer, divider_buffer);
                        if (divident_buffer.Length > 0)
                            if (divident_buffer[0] == '0' && divident_buffer.Length - 1 < _divider.Length)
                                divident_buffer = divident_buffer.TrimStart('0');
                        if (divident_buffer.Length == _divider.Length && numberMap.IndexOf(divident_buffer[0]) < numberMap.IndexOf(_divider[0])) break;
                    }

                    if (divident_buffer.Count(x => x != numberMap[0]) == 0 && divident_buffer.Length > 1)
                        divident_buffer = "0";

                    if (divident.Length < _divider.Length || numberMap.IndexOf(_divider[0]) > numberMap.IndexOf(_divident[0]))
                    {
                        divident_buffer = "-";
                        result_buffer = "-";
                    }

                    ResultRow resultRow = new ResultRow()
                    { 
                        divident = divident,
                        divider = _divider,
                        result = result_buffer,
                        reminder = divident_buffer == "" ? "0" : divident_buffer == "0" ? divident_buffer : divident_buffer.TrimStart('0')
                    };
                    results.Add(resultRow);
                }
            }
            ResultTable = results;
        }

        private int Multiply(int left, int right)
        {
            return Modulo((left * right), _modulus);
        }

        private string Multiply(string left, int right)
        {
            char[] result = left.ToCharArray();
            for (int i = 0; i < left.Length; i++)
            {
                result[i] = numberMap[Multiply(numberMap.IndexOf(left[i]), right)];
            }

            return new string(result);
        }

        private int Substract(int left, int right)
        {
            return Modulo((left - right), _modulus);
        }

        private string Substract(string left, string right)
        {
            char[] result = left.ToCharArray();
            for (int i = 0; i < right.Length; i++)
            {
                result[i] = numberMap[Substract(numberMap.IndexOf(left[i]), numberMap.IndexOf(right[i]))];
            }

            if (result[0] == '0')
                return new string(result).Substring(1);
            else
                return new string(result);
        }

        private int Modulo(int number, int modulus) 
        { 
            return ((number %= modulus) < 0) ? number + modulus : number; 
        }

        private string GetRegex()
        {
            switch(_modulus)
            {
                case 2:
                    return @"[^0-1]";
                case 3:
                    return @"[^0-2]";
                case 5:
                    return @"[^0-4]";
                case 7:
                    return @"[^0-6]";
                case 11:
                    return @"[^0-9, A]";
                case 13:
                    return @"[^0-9, A, B, C]";
                default:
                    return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            ResultRow resultRow = (ResultRow)resultsTable.Items[e.Row.GetIndex()];
            if (resultRow.reminder == "0") e.Row.Background = System.Windows.Media.Brushes.IndianRed;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((sender as System.Windows.Controls.Button).Content.ToString());
            MessageBox.Show("Результат скопирован в буффер обмена!", "Операция успешна", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
