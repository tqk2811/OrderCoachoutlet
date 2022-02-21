using OrderCoachoutlet.DataClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.ObservableCollection;

namespace OrderCoachoutlet.UI.ViewModels
{
    internal class MainWVM : BaseViewModel
    {
        public MainWVM()
        {
            DataManaged.OnCountChange += DataManaged_OnCountChange;
        }

        private void DataManaged_OnCountChange()
        {
            NotifyPropertyChange(nameof(DataManaged));
        }

        public DataManaged DataManaged { get; } = new DataManaged();
        public int ThreadCount
        {
            get { return Singleton.Setting.Setting.ThreadCount; }
            set { Singleton.Setting.Setting.ThreadCount = value; NotifyPropertyChange(); Singleton.Setting.Save(); }
        }
        bool _IsRunning = false;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set { _IsRunning = value; NotifyPropertyChange(); }
        }


        public LimitObservableCollection<string> Logs { get; }
            = new LimitObservableCollection<string>(() => Singleton.LogDir + $"\\{DateTime.Now:yyyy-MM-dd}.log");
    }
}
