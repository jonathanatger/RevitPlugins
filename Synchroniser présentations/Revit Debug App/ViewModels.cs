using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace SynchroniserPresentations.ViewModels
{

    public class Vue : INotifyPropertyChanged
    {
        private string _nom;
        private bool _check;
        private string _prop;
        private string _prop2;
        private string _echelle;
        private ElementId _id;


        public string Nom
        {
            get
            {
                return _nom;
            }
            set
            {
                _nom = value;
                OnPropertyChanged("Nom");
            }
        }

        public bool Check
        {
            get { return _check; }
            set
            {
                _check = value;
                OnPropertyChanged("Check");
            }
        }

        public string Prop
        {
            get { return _prop; }
            set
            {
                _prop = value;
                OnPropertyChanged("Prop");
            }
        }

        public string Prop2
        {
            get { return _prop2; }
            set
            {
                _prop2 = value;
                OnPropertyChanged("Prop2");
            }
        }

        public string Echelle
        {
            get { return _echelle; }
            set
            {
                _echelle = value;
                OnPropertyChanged("Echelle");
            }
        }

        public ElementId Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //MVVM
    public class VueWindowModel : INotifyPropertyChanged
    {
        private ObservableCollection<Vue> _vues;
        private Vue _selectedVue;
        //private ICommand _getFiles;

        public ObservableCollection<Vue> Vues
        {
            get { return _vues; }
            set
            {
                _vues = value;
                OnPropertyChanged("Vues");
            }
        }

        public Vue SelectedVue
        {
            get { return _selectedVue; }
            set
            {
                _selectedVue = value;
                OnPropertyChanged("SelectedVue");
            }
        }

        //public ICommand GetFiles
        //{
        //    get { return _getFiles; }
        //    set
        //    {
        //        _getFiles = value;
        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public VueWindowModel()
        {
            Vues = new ObservableCollection<Vue>();
            //Vues.Add(new Vue() { Nom = "Nom1", Check = false });
            //Vues.Add(new Vue() { Nom = "Nom2", Check = false });
            //GetFiles = new RelayCommand(ChangeFileName, param => true);
        }

    }
}
