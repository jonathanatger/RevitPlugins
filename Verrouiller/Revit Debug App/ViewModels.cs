using System.Collections.ObjectModel;
using System.ComponentModel;
using Autodesk.Revit.DB;

namespace Verrouiller.ViewModels
{

    public class Objet : INotifyPropertyChanged
    {
        private string _nom;
        private bool _check;
        


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


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //MVVM
    public class ObjetWindowModel : INotifyPropertyChanged
    {
        private ObservableCollection<Objet> _vues;
        private Objet _selectedObjet;
        //private ICommand _getFiles;

        public ObservableCollection<Objet> Vues
        {
            get { return _vues; }
            set
            {
                _vues = value;
                OnPropertyChanged("Vues");
            }
        }

        public Objet SelectObjets
        {
            get { return _selectedObjet; }
            set
            {
                _selectedObjet = value;
                OnPropertyChanged("SelectedObjets");
            }
        }

       


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObjetWindowModel()
        {
            Vues = new ObservableCollection<Objet>();            
        }

    }
}
