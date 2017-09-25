using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using AxModel;
using System.IO;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Command;
using Navegar;
using GalaSoft.MvvmLight.Ioc;

namespace AxViewModel
{
    public class MazeCircuitOptionViewModel : ViewModelBase
    {
        #region Fileds
        /// <summary>
        /// Singleton de l'application qui contient les info du patient
        /// </summary>
        private Singleton singleton = Singleton.getInstance();

        private INavigation nav;

        /// <summary>
        /// Chemin d'acces au dossier du patient
        /// </summary>
        private string pathPatient = string.Empty;

        /// <summary>
        /// Type de la tache à effectué. True si uni et False si bi
        /// </summary>
        private bool _uniChecked;

        /// <summary>
        /// Actions que doit faire la main gauche. True si la main gauche est utilisé en X et False si c'est en Y
        /// </summary>
        private bool _gaucheXChecked;

        /// <summary>
        /// Type de la tache à effectué. True si uni et False si bi
        /// </summary>
        private bool _biChecked;

        /// <summary>
        /// Actions que doit faire la main gauche. True si la main gauche est utilisé en X et False si c'est en Y
        /// </summary>
        private bool _gaucheYChecked;
        #endregion

        #region Ctor
        public MazeCircuitOptionViewModel()
        {
            this.NextViewModelCommand = new RelayCommand(this.GoNext, this.NextCanExecute);
            this.PreviousViewModelCommand = new RelayCommand(this.GoBack);

            this.nav = SimpleIoc.Default.GetInstance<INavigation>(); 
            this.pathPatient = "Files/Patients/" + singleton.Patient.Nom + singleton.Patient.Prenom + singleton.Patient.DateDeNaissance.ToString().Replace("/", string.Empty);
            this.GetPreselection();
        }
        #endregion

        #region Commands
        public RelayCommand NextViewModelCommand { get; set; }

        public RelayCommand PreviousViewModelCommand { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets le type de la tache
        /// </summary>
        public bool UniChecked
        {
            get { return _uniChecked; }
            set 
            {
                _uniChecked = value;
                RaisePropertyChanged("UniChecked");
            }
        }

        /// <summary>
        /// Gets or sets le type de la tache
        /// </summary>
        public bool BiChecked
        {
            get { return _biChecked; }
            set
            {
                _biChecked = value;
                RaisePropertyChanged("BiChecked");
            }
        }

        /// <summary>
        /// Gets or sets l'action de la main gauche
        /// </summary>
        public bool GaucheXChecked
        {
            get { return _gaucheXChecked; }
            set 
            {
                _gaucheXChecked = value;
                RaisePropertyChanged("GaucheXChecked;");
            }
        }

        /// <summary>
        /// Gets or sets l'action de la main gauche
        /// </summary>
        public bool GaucheYChecked
        {
            get { return _gaucheYChecked; }
            set
            {
                _gaucheYChecked = value;
                RaisePropertyChanged("GaucheYChecked;");
            }
        }
        #endregion

        #region Methodes
        /// <summary>
        /// Gestion de la préslection des options si ce n'est pas la première fois que le patien fait l'exercice
        /// </summary>
        private void GetPreselection()
        {
            if (File.Exists(this.pathPatient + "/InfoPatient.xml"))
            {
                // Si le fichier existe c'est que l'exercice à déjà été réalisé et qu'une préselection est possible
                XDocument doc = XDocument.Load(this.pathPatient + "/InfoPatient.xml");
                if (doc != null)
                {
                    foreach (var elem in doc.Root.Elements())
                    {
                        if (elem.Name == "Uni_Gauche" && Convert.ToBoolean(elem.FirstAttribute.Value) == true)
                        {
                            this.UniChecked = true;
                            this.BiChecked = false;
                            this.GaucheXChecked = true;
                            this.GaucheYChecked = false;
                        }
                        else if (elem.Name == "Uni_Droit" && Convert.ToBoolean(elem.FirstAttribute.Value) == true)
                        {
                            this.UniChecked = true;
                            this.BiChecked = false;
                            this.GaucheXChecked = false;
                            this.GaucheYChecked = true;
                        }
                        else if (elem.Name == "Bi_Gauche" && Convert.ToBoolean(elem.FirstAttribute.Value) == true)
                        {
                            this.UniChecked = false;
                            this.BiChecked = true;
                            this.GaucheXChecked = true;
                            this.GaucheYChecked = false;
                        }
                        else if (elem.Name == "Bi_Droit" && Convert.ToBoolean(elem.FirstAttribute.Value) == true)
                        {
                            this.UniChecked = false;
                            this.BiChecked = true;
                            this.GaucheXChecked = false;
                            this.GaucheYChecked = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Passer à l'écran suivant et enregistrer les options séléctionnées dans le fichier et dans le singleton
        /// </summary>
        private void GoNext()
        {
            XDocument doc;

            if (File.Exists(this.pathPatient + "/InfoPatient.xml"))
            {
                doc = XDocument.Load(this.pathPatient + "/InfoPatient.xml");
            }
            else
            {
                if (!Directory.Exists(this.pathPatient))
                {
                    Directory.CreateDirectory(this.pathPatient);
                }

                doc = new XDocument(
                new XDeclaration("1.0", "UTF-16", null),
                new XElement("Calibration_Settings",
                    new XElement("Uni_Gauche", new XAttribute("Last", false),
                        new XElement("CalibrX"),
                        new XElement("CalibrY")),
                        new XElement("Uni_Droit", new XAttribute("Last", false),
                        new XElement("CalibrX"),
                        new XElement("CalibrY")),
                        new XElement("Bi_Gauche", new XAttribute("Last", false),
                        new XElement("CalibrX"),
                        new XElement("CalibrY")),
                        new XElement("Bi_Droit", new XAttribute("Last", false),
                        new XElement("CalibrX"),
                        new XElement("CalibrY"))));
            }

            if (this.UniChecked == true)
            {
                doc.Root.Element("Bi_Gauche").SetAttributeValue("Last", false);
                doc.Root.Element("Bi_Droit").SetAttributeValue("Last", false);

                if (this.GaucheXChecked == true)
                {
                    doc.Root.Element("Uni_Gauche").SetAttributeValue("Last", true);
                    doc.Root.Element("Uni_Droit").SetAttributeValue("Last", false);
                }
                else
                {
                    doc.Root.Element("Uni_Gauche").SetAttributeValue("Last", false);
                    doc.Root.Element("Uni_Droit").SetAttributeValue("Last", true);
                }
            }
            else
            {
                doc.Root.Element("Uni_Gauche").SetAttributeValue("Last", false);
                doc.Root.Element("Uni_Droit").SetAttributeValue("Last", false);

                if (this.GaucheXChecked == true)
                {
                    doc.Root.Element("Bi_Gauche").SetAttributeValue("Last", true);
                    doc.Root.Element("Bi_Droit").SetAttributeValue("Last", false);
                }
                else
                {
                    doc.Root.Element("Bi_Gauche").SetAttributeValue("Last", false);
                    doc.Root.Element("Bi_Droit").SetAttributeValue("Last", true);
                }
            }
            
            doc.Save(this.pathPatient + "/InfoPatient.xml");

            Singleton.MainGaucheX = this.GaucheXChecked;
            Singleton.UniBi = this.UniChecked;

            this.nav.NavigateTo<MazeCircuitCalibrationViewModel>(this, null, true);
        }

        /// <summary>
        /// La méthode next ne peut être utilisé que si la séléction des options à été faite
        /// </summary>
        /// <returns></returns>
        private bool NextCanExecute()
        {
            if ((!this.GaucheXChecked && !this.GaucheYChecked) || (!this.UniChecked && !this.BiChecked))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Retour en arriére ou sur l'écran d'accueil
        /// </summary>
        private void GoBack()
        {
            if (nav.GetTypeViewModelToBack() == typeof(HomeViewModel))
            {
                nav.GoBack("SetIsRetour", new object[] { true });
            }
            else
            {
                nav.GoBack();
            }
        }
        #endregion
    }
}
