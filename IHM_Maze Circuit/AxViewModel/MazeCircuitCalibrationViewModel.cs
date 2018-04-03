using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using AxModel;
using Navegar;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System.Xml.Linq;
using System.IO;
using AxAction;
using AxModel.Helpers;
namespace AxViewModel
{
    public class MazeCircuitCalibrationViewModel : ViewModelBase
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
        /// Coordonné x de la calibration
        /// </summary>
        private double calibrX = 0.0;

        /// <summary>
        /// Coordonné y de la calibration
        /// </summary>
        private double calibrY = 0.0;

        /// <summary>
        /// Coordonnée en pixel à donner au jeu
        /// </summary>
        private double pixelX = 0.0;

        /// <summary>
        /// Coordonnée en pixel à donner au jeu
        /// </summary>
        private double pixelY = 0.0;

        /// <summary>
        /// Permet de savoir si on peut passer à l'écran suivant
        /// </summary>
        private bool canNext = false;

        /// <summary>
        /// Instance de l'objet gérant le jeu et la communication
        /// </summary>
        private MazeCircuitGame game;

        /// <summary>
        /// Permet de savoir si le jeu est lancé
        /// </summary>
        private bool gameStarted = false;

        /// <summary>
        /// Instance du port série
        /// </summary>
        private ActionRobot pss;
        #endregion

        #region Ctor
        public MazeCircuitCalibrationViewModel()
        {
            this.pss = SimpleIoc.Default.GetInstance<ActionRobot>();

            this.NextViewModelCommand = new RelayCommand(this.GoNext, this.NextCanExecute);
            this.PreviousViewModelCommand = new RelayCommand(this.GoBack);
            this.HomeViewModelCommand = new RelayCommand(this.GoHome);
            this.StartCalibrationCommand = new RelayCommand(this.StartCalibration, StartCanExecute);
            this.StopCalibrationCommand = new RelayCommand(this.StopCalibration, StopCanExecute);

            this.nav = SimpleIoc.Default.GetInstance<INavigation>(); 
            this.pathPatient = "Files/Patients/" + singleton.Patient.Nom + singleton.Patient.Prenom + singleton.Patient.DateDeNaissance.ToString().Replace("/", string.Empty);
            this.GetPreselection();
        }
        #endregion

        #region Commands
        public RelayCommand NextViewModelCommand { get; set; }
        public RelayCommand PreviousViewModelCommand { get; set; }
        public RelayCommand HomeViewModelCommand { get; set; }
        public RelayCommand StartCalibrationCommand { get; set; }
        public RelayCommand StopCalibrationCommand { get; set; }
        #endregion

        #region Properties
       
        #endregion

        #region Methodes
        /// <summary>
        /// Gestion de la préslection des options si ce n'est pas la première fois que le patient fait l'exercice
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
                        if (elem.Name == "Uni_Gauche" && Singleton.UniBi == true && Singleton.MainGaucheX == true)
                        {
                            if (elem.Element("CalibrX").Value != string.Empty)
                            {
                                this.calibrX = Convert.ToDouble(elem.Element("CalibrX").Value.Replace('.', ','));
                            }
                            if (elem.Element("CalibrY").Value != string.Empty)
                            {
                                this.calibrY = Convert.ToDouble(elem.Element("CalibrY").Value.Replace('.', ','));
                            }
                        }
                        else if (elem.Name == "Uni_Droit" && Singleton.UniBi == true && Singleton.MainGaucheX == false)
                        {
                            if (elem.Element("CalibrX").Value != string.Empty)
                            {
                                this.calibrX = Convert.ToDouble(elem.Element("CalibrX").Value.Replace('.', ','));
                            }
                            if (elem.Element("CalibrY").Value != string.Empty)
                            {
                                this.calibrY = Convert.ToDouble(elem.Element("CalibrY").Value.Replace('.', ','));
                            }
                        }
                        else if (elem.Name == "Bi_Gauche" && Singleton.UniBi == false && Singleton.MainGaucheX == true)
                        {
                            if (elem.Element("CalibrX").Value != string.Empty)
                            {
                                this.calibrX = Convert.ToDouble(elem.Element("CalibrX").Value.Replace('.', ','));
                            }
                            if (elem.Element("CalibrY").Value != string.Empty)
                            {
                                this.calibrY = Convert.ToDouble(elem.Element("CalibrY").Value.Replace('.', ','));
                            }
                        }
                        else if (elem.Name == "Bi_Droit" && Singleton.UniBi == false && Singleton.MainGaucheX == false)
                        {
                            if (elem.Element("CalibrX").Value != string.Empty)
                            {
                                this.calibrX = Convert.ToDouble(elem.Element("CalibrX").Value.Replace('.', ','));
                            }
                            if (elem.Element("CalibrY").Value != string.Empty)
                            {
                                this.calibrY = Convert.ToDouble(elem.Element("CalibrY").Value.Replace('.', ','));
                            }
                        }
                    }

                    if(this.calibrX != 0.0 && this.calibrY != 0.0)
                    {
                        Singleton.CalibrX = this.calibrX;
                        Singleton.CalibrY = this.calibrY;
                        this.canNext = true;
                        NextViewModelCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Passer à l'écran suivant et enregistrer les options séléctionnées dans le fichier et dans le singleton
        /// </summary>
        private void GoNext()
        {
            this.StopCalibration();
            this.nav.NavigateTo<MazeCircuitMenuViewModel>(this, null, true);
        }

        /// <summary>
        /// La méthode next ne peut être utilisé que si la calibration à été faite
        /// </summary>
        /// <returns></returns>
        private bool NextCanExecute()
        {
            return this.canNext;
        }
        /// <summary>
        /// Retour en arriére
        /// </summary>
        private void GoBack()
        {
            this.StopCalibration();
            nav.GoBack();
        }

        /// <summary>
        /// Retour à l'accueil
        /// </summary>
        private void GoHome()
        {
            this.StopCalibration();
            nav.NavigateTo<HomeViewModel>("SetIsRetour", new object[] { true });
        }

        /// <summary>
        /// Début de la calibration de l'exercice
        /// </summary>
        private void StartCalibration()
        {

            this.pss.Pss.PositionDataReceived += new AxCommunication.onPositionDataReceived(Pss_PositionDataReceived);
            this.pss.Pss.Position2DataReceived += new AxCommunication.onPosition2DataReceived(Pss_Position2DataReceived);
            this.game = new MazeCircuitGame(0, 0, 9);
            this.game.StartGame();

            this.gameStarted = true;
            this.StartCalibrationCommand.RaiseCanExecuteChanged();

            //this.pss.Pss.PositionDataReceived +=new AxCommunication.onPositionDataReceived(Pss_PositionDataReceived);
            //this.pss.Pss.Position2DataReceived +=new AxCommunication.onPosition2DataReceived(Pss_Position2DataReceived);

            // Positionner le robot + init du mod calibration
            // Détermine quelle bornes envoyer au robot pour la calibration
            FrameExerciceDataModel frame = new FrameExerciceDataModel();
            if (Singleton.UniBi)
            {
                // Unimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    frame = new MazeCalibModel(547, 374, UniBiCodes.UnimanuelGauche).MakeFrame();//537 547
                }
                else
                {
                    // droite
                    frame = new MazeCalibModel(556, 964, UniBiCodes.UnimanuelDroite).MakeFrame();
                }
            }
            else
            {
                // Bimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    frame = new MazeCalibModel(547, 964, UniBiCodes.BimanuelGaucheXDroiteY).MakeFrame();
                }
                else
                {
                    // droite
                    frame = new MazeCalibModel(556, 374, UniBiCodes.BimanuelGaucheYDroiteX).MakeFrame();
                }
            }

            // Envoie du mod de calibration
            pss.Pss.SendExerciceFrame(frame);
            pss.Pss.SendCommandFrame(CommandCodes.mode_libre);
        }

        private bool StartCanExecute()
        {
            return !gameStarted;
        }

        private void Pss_PositionDataReceived(object sender, PositionDataModel e)
        {
            // Sauvegarde des derniére positions connue
            if (Singleton.UniBi)
            {
                // Unimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    this.calibrX = e.PositionX;
                    this.calibrY = e.PositionY;

                    this.pixelX = EchelleUtils.MiseEchelleXPosition(e.PositionX);
                    this.pixelY = EchelleUtils.MiseEchelleYPosition(e.PositionY);
                }
            }
            else
            {
                // Bimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    this.calibrX = e.PositionX;
                    this.pixelX = EchelleUtils.MiseEchelleXPosition(e.PositionX);
                }
                else
                {
                    // droite
                    this.calibrY = e.PositionY;
                    this.pixelY = EchelleUtils.MiseEchelleYPosition(e.PositionY);
                }
            }

            if (this.game != null)
            {
                this.game.SetPositions(pixelX, pixelY);
            }
        }

        private void Pss_Position2DataReceived(object sender, Position2DataModel e)
        {
            // Sauvegarde des derniére positions connue
            if (Singleton.UniBi)
            {
                // Unimanuel
                if (!Singleton.MainGaucheX)
                {
                    // droite
                    this.calibrX = e.PositionX;
                    this.calibrY = e.PositionY;

                    this.pixelX = EchelleUtils.MiseEchelleXPosition2(e.PositionX);
                    this.pixelY = EchelleUtils.MiseEchelleYPosition2(e.PositionY); //1080 -
                }
            }
            else
            {
                // Bimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    this.calibrY = e.PositionY;
                    this.pixelY = EchelleUtils.MiseEchelleYPosition2(e.PositionY);
                }
                else
                {
                    // droite
                    this.calibrX = e.PositionX;
                    this.pixelX = EchelleUtils.MiseEchelleXPosition2(e.PositionX);
                }
            }
        }

        /// <summary>
        /// Fin de la calibration, quand la position est idéale
        /// </summary>
        private void StopCalibration()
        {
            if (this.game != null)
            {
                this.game.StopGame();
                this.game = null;
                this.gameStarted = false;
                this.StopCalibrationCommand.RaiseCanExecuteChanged();

                XDocument doc;

                if (File.Exists(this.pathPatient + "/InfoPatient.xml"))
                {
                    doc = XDocument.Load(this.pathPatient + "/InfoPatient.xml");

                    if (Singleton.UniBi == true && Singleton.MainGaucheX == true)
                    {
                        doc.Root.Element("Uni_Gauche").Element("CalibrX").ReplaceNodes(this.calibrX / 100.0);
                        doc.Root.Element("Uni_Gauche").Element("CalibrY").ReplaceNodes(this.calibrY / 100.0);
                    }
                    else if (Singleton.UniBi == true && Singleton.MainGaucheX == false)
                    {
                        doc.Root.Element("Uni_Droit").Element("CalibrX").ReplaceNodes(this.calibrX / 100.0);
                        doc.Root.Element("Uni_Droit").Element("CalibrY").ReplaceNodes(this.calibrY / 100.0);
                    }
                    else if (Singleton.UniBi == false && Singleton.MainGaucheX == true)
                    {
                        doc.Root.Element("Bi_Gauche").Element("CalibrX").ReplaceNodes(this.calibrX / 100.0);
                        doc.Root.Element("Bi_Gauche").Element("CalibrY").ReplaceNodes(this.calibrY / 100.0);
                    }
                    else if (Singleton.UniBi == false && Singleton.MainGaucheX == false)
                    {
                        doc.Root.Element("Bi_Droit").Element("CalibrX").ReplaceNodes(this.calibrX / 100.0);
                        doc.Root.Element("Bi_Droit").Element("CalibrY").ReplaceNodes(this.calibrY / 100.0);
                    }

                    
                    doc.Save(this.pathPatient + "/InfoPatient.xml");

                    Singleton.CalibrX = this.calibrX / 100.0;
                    Singleton.CalibrY = this.calibrY / 100.0;
                }
            }

            this.pss.Pss.SendCommandFrame(CommandCodes.STOPnv);
            this.pss.Pss.PositionDataReceived -= new AxCommunication.onPositionDataReceived(Pss_PositionDataReceived);
            this.pss.Pss.Position2DataReceived -= new AxCommunication.onPosition2DataReceived(Pss_Position2DataReceived);

            this.canNext = true;
            NextViewModelCommand.RaiseCanExecuteChanged();
        }

        private bool StopCanExecute()
        {
            return gameStarted;
        }
        #endregion
    }
}
