using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Navegar;
using GalaSoft.MvvmLight.Ioc;
using System.Collections.ObjectModel;
using AxModel;
using AxAction;
using AxReaLabToUnity.Models;
using AxModel.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using OxyPlot;
using GalaSoft.MvvmLight.Messaging;
using System.Timers;
using System.Diagnostics;

namespace AxViewModel
{
    public class MazeCircuitMenuViewModel : ViewModelBase
    {
        #region Field
        private double[] SIZE_SEG_CIRCUIT = new double[] { 0.0598, 0.0598, 0.13945, 0.13945, 0.2187, 0.13945, 0.0998, 0.1988, 0.0998, 0.1988, 0.0796 };
        private const double SIZE_SEG_REACHING = 0.195;
        private const double SIZE_SEG_FAM = 0.1988;
        /// <summary>
        /// Instance du service de navigation
        /// </summary>
        private INavigation nav;

        private Singleton singleton = Singleton.getInstance();

        /// <summary>
        /// Type de l'exercice en cours
        /// </summary>
        private TypeMazeGame currentType;

        /// <summary>
        /// Code couleur en cours
        /// </summary>
        private TypeColors currentColor;

        private int viscosite;

        private int reachingRadius;

        private bool menuEnabled;

        private bool circuitsEnabled;

        private bool newCircuitEnabled;

        /// <summary>
        /// Dernier circuit fait qui ne peut pas être séléctionnable dans le new circuit
        /// </summary>
        private int lastCircuit;

        /// <summary>
        /// Permet de savoir si le jeu à été lancé
        /// </summary>
        private bool gameStarted = false;

        /// <summary>
        /// Instance du port serie
        /// </summary>
        private ActionRobot pss;

        private MazeCircuitGame game;

        /// <summary>
        /// Coordonnées du centre du circuit actuel
        /// </summary>
        private double centreX = 0.0;
        private double centreY = 0.0;

        /// <summary>
        /// Permet de savoir si il faut envoyer ou non les positions au jeu
        /// </summary>
        private bool canSendPos = false;

        /// <summary>
        /// Permet de savoir quand est ce qu'il faut retenir les positions
        /// </summary>
        private bool canDoMath = false;

        private bool firstSegment = true;

        private string pathToFile = string.Empty;
        private string newNameFile = string.Empty;
        private double pixelX;
        private double pixelY;

        /// <summary>
        /// Frame de l'exercice en cours
        /// </summary>
        private FrameExerciceDataModel frame;

        private List<PositionDataModel> listPos = new List<PositionDataModel>();
        private List<Position2DataModel> listPos2 = new List<Position2DataModel>();

        private List<ForceDataModel> listForce = new List<ForceDataModel>();
        private List<Force2DataModel> listForce2 = new List<Force2DataModel>();

        private List<VitesseModel> listVit = new List<VitesseModel>();
        private List<Vitesse2Model> listVit2 = new List<Vitesse2Model>();

        private List<PprDataModel> listError = new List<PprDataModel>();

        private List<double> satMoy = new List<double>();
        private double highScore = 0.0;

        private List<double> ErrorMoyPlot = new List<double>();
        private List<double> VitesseMoyPlot = new List<double>();

        private int maxRepetionPlot;
        #endregion

        public MazeCircuitMenuViewModel()
        {
            this.nav = SimpleIoc.Default.GetInstance<INavigation>();
            this.pss = SimpleIoc.Default.GetInstance<ActionRobot>();
            this.currentType = TypeMazeGame.Null;

            this.PreviousViewModelCommand = new RelayCommand(this.GoBack);
            this.HomeViewModelCommand = new RelayCommand(this.GoHome);
            this.ChangeExerciceTypeCommand = new RelayCommand<int>(this.ChangeExerciceType);
            this.ChangerCouleurCommand = new RelayCommand<int>(this.ChangerCouleur);
            this.StartCommand = new RelayCommand(this.StartGame, this.Start_CanExecute);
            this.StopCommand = new RelayCommand(this.StopGame, this.Stop_CanExecute);
            this.PauseCommand = new RelayCommand(this.PauseGame, this.Stop_CanExecute);

            // De base aucun type n'est coché donc le menu n'est pas accessible
            this.MenuEnabled = false;

            this.NewCircuitEnabled = false;

            // Création de la liste de radio button pour la séléction du circuit
            this.CircuitsEnabled = false;
            this.CircuitsCheck = new ObservableCollection<CircuitRadio>();
            for (int i = 1; i < 9; i++)
            {
                var radio = new CircuitRadio(i.ToString());
                this.CircuitsCheck.Add(radio);
            }

            this.pathToFile = "Files/Patients/" + singleton.Patient.Nom + singleton.Patient.Prenom + singleton.Patient.DateDeNaissance.ToString().Replace("/", string.Empty) + "/Maze_Circuit";

            this.ErrorPlotPoint = new List<DataPoint>();
            this.VitessePlotPoint = new List<DataPoint>();
            this.MaxRepetionPlot = 10;
            this.currentColor = TypeColors.Default;
        }

        #region Properties
        public int Viscosite
        {
            get { return viscosite; }
            set 
            {
                viscosite = value;
                RaisePropertyChanged("Viscosite");
            }
        }

        public int ReachingRadius
        {
            get { return reachingRadius; }
            set
            {
                reachingRadius = value;
                RaisePropertyChanged("ReachingRadius");
            }
        }

        public bool MenuEnabled
        {
            get { return menuEnabled; }
            set 
            {
                menuEnabled = value;
                RaisePropertyChanged("MenuEnabled");
            }
        }

        public ObservableCollection<CircuitRadio> CircuitsCheck { get; set; }

        public bool OptionsEnabled
        {
            get { return !gameStarted; }
        }

        public bool CircuitsEnabled
        {
            get { return circuitsEnabled; }
            set 
            {
                circuitsEnabled = value;
                RaisePropertyChanged("CircuitsEnabled");
            }
        }

        public bool NewCircuitEnabled
        {
            get { return newCircuitEnabled; }
            set 
            {
                newCircuitEnabled = value;
                RaisePropertyChanged("NewCircuitEnabled");
            }
        }

        public List<DataPoint> ErrorPlotPoint { get; set; }
        public List<DataPoint> VitessePlotPoint { get; set; }

        public int MaxRepetionPlot
        {
            get { return maxRepetionPlot; }
            set 
            {
                maxRepetionPlot = value;
                RaisePropertyChanged("MaxRepetionPlot");
            }
        }

        #endregion

        #region Commands
        public RelayCommand PreviousViewModelCommand { get; set; }
        public RelayCommand HomeViewModelCommand { get; set; }
        public RelayCommand<int> ChangeExerciceTypeCommand { get; set; }
        public RelayCommand<int> ChangerCouleurCommand { get; set; }
        public RelayCommand StartCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        #endregion

        #region Methodes
        /// <summary>
        /// Retour en arriére
        /// </summary>
        private void GoBack()
        {
            this.StopGame();
            nav.GoBack();
        }

        /// <summary>
        /// Retour à l'accueil
        /// </summary>
        private void GoHome()
        {
            this.StopGame();
            nav.NavigateTo<HomeViewModel>("SetIsRetour", new object[] { true });
        }

        /// <summary>
        /// Change l'interface en fonction du type d'exercice séléctionné
        /// </summary>
        /// <param name="type"></param>
        private void ChangeExerciceType(int type)
        {
            this.currentType = (TypeMazeGame)type;
            this.MenuEnabled = true;

            switch (this.currentType)
            {
                case TypeMazeGame.Familiarisation:
                    this.CircuitsEnabled = false;
                    break;
                case TypeMazeGame.BaseLine:
                    this.CircuitsEnabled = true;
                    this.ActiveAllCircuits();
                    this.MaxRepetionPlot = 3;
                    break;
                case TypeMazeGame.Training:
                    this.CircuitsEnabled = true;
                    this.ActiveAllCircuits();
                    this.MaxRepetionPlot = 20;
                    break;
                case TypeMazeGame.Reaching:
                    this.CircuitsEnabled = false;
                    this.MaxRepetionPlot = 64;
                    break;
                case TypeMazeGame.NewCircuit:
                    this.CircuitsEnabled = true;
                    // Désactive le circuit qui à déjà été fait
                    this.CircuitsCheck[this.lastCircuit].Activated = false;
                    this.CircuitsCheck[this.lastCircuit].Checked = false;
                    this.MaxRepetionPlot = 3;
                    break;
                case TypeMazeGame.After:
                    this.CircuitsEnabled = true;
                    this.ActiveAllCircuits();
                    this.MaxRepetionPlot = 3;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Change l'interface en fonction du type d'exercice séléctionné
        /// </summary>
        /// <param name="type"></param>
        private void ChangerCouleur(int type)
        {
            this.currentColor = (TypeColors)type;
        }

        private void StartGame()
        {
            this.SendViscosite();

            if (this.currentType == TypeMazeGame.BaseLine || this.currentType == TypeMazeGame.After || this.currentType == TypeMazeGame.Training)
            {
                // Si le jeu est fait en training ou base line on peut retenir le circuit fait et ne plus le proposer pour le new circuit
                this.lastCircuit = this.IndexOfCircuitChecked();
                this.NewCircuitEnabled = true;
            }

            // Lancer le jeux en fonction du mode
            switch (currentType)
            {
                case TypeMazeGame.Null:
                    break;
                case TypeMazeGame.Familiarisation:
                    game = new MazeCircuitGame(2, 0, this.IndexOfCircuitChecked());
                    break;
                case TypeMazeGame.BaseLine:
                    game = new MazeCircuitGame(0, 0, this.IndexOfCircuitChecked() + 1);
                    break;
                case TypeMazeGame.Training:
                    game = new MazeCircuitGame(1, 0, this.IndexOfCircuitChecked() + 1);
                    break;
                case TypeMazeGame.Reaching:
                    game = new MazeCircuitGame(0, ReachingRadius, 10);
                    break;
                case TypeMazeGame.NewCircuit:
                    game = new MazeCircuitGame(0, 0, this.IndexOfCircuitChecked() + 1);
                    break;
                case TypeMazeGame.After:
                    game = new MazeCircuitGame(0, 0, this.IndexOfCircuitChecked() + 1);
                    break;
                default:
                    break;
            }

            this.game.StartGame();
            this.highScore = 0;

            // Gestion des états de l'interface
            this.gameStarted = true;
            RaisePropertyChanged("OptionsEnabled");
            this.CircuitsEnabled = false;
            this.firstSegment = true;

            // Abonnements à tous les events
            this.game.onNewTrajectory += new AxReaLabToUnity.NewTrajectoryHandler(game_onNewTrajectory);
            this.game.onLevelStarted += new AxReaLabToUnity.NewLevelStartedHandler(game_onLevelStarted);
            this.game.onCheckpointReached += new AxReaLabToUnity.NewCheckpointReachedHandler(game_onCheckpointReached);
            this.game.onLevelStopped += new AxReaLabToUnity.NewLevelStoppedHandler(game_onLevelStopped);

            this.pss.Pss.PositionDataReceived += new AxCommunication.onPositionDataReceived(Pss_PositionDataReceived);
            this.pss.Pss.Position2DataReceived += new AxCommunication.onPosition2DataReceived(Pss_Position2DataReceived);
            this.pss.Pss.PprDataReceived += new AxCommunication.onPprDataReceived(Pss_PprDataReceived);
            this.pss.Pss.VitesseDataReceived += new AxCommunication.onVitesseDataReceived(Pss_VitesseDataReceived);
            this.pss.Pss.Vitesse2DataReceived += new AxCommunication.onVitesse2DataReceived(Pss_Vitesse2DataReceived);
            this.pss.Pss.ForceDataReceived += new AxCommunication.onForceDataReceived(Pss_ForceDataReceived);
            this.pss.Pss.Force2DataReceived += new AxCommunication.onForce2DataReceived(Pss_Force2DataReceived);
            this.pss.Pss.StreamingDone += new EventHandler(Pss_StreamingDone);

            this.newNameFile = this.FindFileName(this.pathToFile, this.currentType.ToString(), ".txt");

            this.game.SetColor((int)this.currentColor);
        }

        void Pss_StreamingDone(object sender, EventArgs e)
        {
            if (this.frame != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    pss.Pss.SendExerciceFrame(frame);

                }), DispatcherPriority.Normal);
            }
        }

        void Pss_Force2DataReceived(object sender, Force2DataModel e)
        {
            if (canDoMath)
            {
                this.listForce2.Add(e);
            }
        }

        void Pss_ForceDataReceived(object sender, ForceDataModel e)
        {
            if (canDoMath)
            {
                this.listForce.Add(e);
            }
        }

        void Pss_Vitesse2DataReceived(object sender, Vitesse2Model e)
        {
            if (canDoMath)
            {
                this.listVit2.Add(e);
            }
        }

        void Pss_VitesseDataReceived(object sender, VitesseModel e)
        {
            if (canDoMath)
            {
                this.listVit.Add(e);
            }
        }

        void Pss_Position2DataReceived(object sender, Position2DataModel e)
        {
            if (canDoMath)
            {
                this.listPos2.Add(e);
            }

            if (this.canSendPos)
            {
                if (Singleton.UniBi)
                {
                    // Unimanuel
                    if (!Singleton.MainGaucheX)
                    {
                        // droite
                        var posXPix = EchelleUtils.MiseEchelleXPosition2(e.PositionXd);
                        var calibXPix = EchelleUtils.MiseEchelleXPosition2(Singleton.CalibrX * 100);
                        this.pixelX = posXPix - calibXPix + this.centreX;
                        var posYPix = EchelleUtils.MiseEchelleYPosition2(e.PositionYd);
                        var calibYPix = EchelleUtils.MiseEchelleYPosition2(Singleton.CalibrY * 100);
                        this.pixelY = posYPix - calibYPix + this.centreY;
                    }
                }
                else
                {
                    // Bimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        var posYPix = EchelleUtils.MiseEchelleYPosition2(e.PositionYd);
                        var calibYPix = EchelleUtils.MiseEchelleYPosition2(Singleton.CalibrY * 100);
                        this.pixelY = posYPix - calibYPix + this.centreY;
                    }
                    else
                    {
                        // droite
                        var posXPix = EchelleUtils.MiseEchelleXPosition2(e.PositionXd);
                        var calibXPix = EchelleUtils.MiseEchelleXPosition2(Singleton.CalibrX * 100);
                        this.pixelX = posXPix - calibXPix + this.centreX;
                    }
                }
            }
        }

        void Pss_PositionDataReceived(object sender, PositionDataModel e)
        {
            if (canDoMath)
            {
                this.listPos.Add(e);
            }

            if (this.canSendPos)
            {
                if (Singleton.UniBi)
                {
                    // Unimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        var posXPix = EchelleUtils.MiseEchelleXPosition(e.PositionXd);
                        var calibXPix = EchelleUtils.MiseEchelleXPosition(Singleton.CalibrX * 100);
                        this.pixelX = posXPix - calibXPix + this.centreX;
                        var posYPix = EchelleUtils.MiseEchelleYPosition(e.PositionYd);
                        var calibYPix = EchelleUtils.MiseEchelleYPosition(Singleton.CalibrY * 100);
                        this.pixelY = posYPix - calibYPix + this.centreY;
                    }
                }
                else
                {
                    // Bimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        var posXPix = EchelleUtils.MiseEchelleXPosition(e.PositionXd);
                        var calibXPix = EchelleUtils.MiseEchelleXPosition(Singleton.CalibrX * 100);
                        this.pixelX = posXPix - calibXPix + this.centreX;
                    }
                    else
                    {
                        // droite
                        var posYPix = EchelleUtils.MiseEchelleYPosition(e.PositionYd);
                        var calibYPix = EchelleUtils.MiseEchelleYPosition(Singleton.CalibrY * 100);
                        this.pixelY = posYPix - calibYPix + this.centreY;
                    }
                }

                if (this.game != null)
                {
                    this.game.SetPositions(pixelX, pixelY);
                }
            }
        }

        void Pss_PprDataReceived(object sender, PprDataModel e)
        {
            if (this.canDoMath)
            {
                this.listError.Add(e);
            }
        }

        void game_onLevelStopped(object obj, EventArgs messageArgs)
        {
            this.pss.Pss.SendCommandFrame(CommandCodes.STOPnv);
            this.canDoMath = false;
            double score = 0;

            if (this.currentType == TypeMazeGame.Reaching)
            {
                // Sauvegarde le dernier segement du reaching avant de tout reset
                this.SaveSegment(0);

                // Score pour le Reaching qui ne se calcul pas de la meme maniere

                if (satMoy.Count > 0)
                {
                    foreach (var sat in satMoy)
                    {
                        score += sat;
                    }
                    score /= satMoy.Count;
                    score *= 100;
                }
                else
                {
                    score = 1;
                }
            }
            else
            {
                if (this.ErrorMoyPlot.Count > 0)
                {
                    // ErrorMoyPlot à une erreurMoy par segement et il y a 11 segments par tour
                    double nbrTurn = Math.Floor(this.ErrorMoyPlot.Count / 11.0);
                    
                    for (int i = 0; i < nbrTurn; i++)
                    {
                        int segMin = i * 11;
                        int segMax = segMin + 10;
                        double errorTour = 0.0;
                        // Calcul de l'erreur moyenne du tour i
                        for (int j = segMin; j <= segMax; j++)
                        {
                            errorTour += ErrorMoyPlot[j];
                        }

                        errorTour /= 11.0;

                        score += (10 + (10 * (1 -errorTour)));
                    }
                }
                else
                {
                    score = 1;
                }
            }

            if (score > highScore)
            {
                highScore = score;
            }

            this.game.SetScore((int)score); 
            this.game.SetHighScore((int)highScore);

            // Actualisation des Plot
            // En reaching on actualise a chaque segement plutot que a chaque fin de circuit
            if (this.currentType != TypeMazeGame.Reaching)
            {
                double axeX = 1.0;
                if (this.ErrorPlotPoint.Count > 0)
                {
                    axeX = this.ErrorPlotPoint.Last().X + 1;
                }

                double errorMoy = 0.0;
                foreach (var error in this.ErrorMoyPlot)
                {
                    errorMoy += error;
                }
                errorMoy /= this.ErrorMoyPlot.Count;
                var newError = new DataPoint(axeX, errorMoy);
                this.ErrorPlotPoint.Add(newError);

                double vitesseMoy = 0.0;
                foreach (var vitesse in this.VitesseMoyPlot)
                {
                    vitesseMoy += vitesse;
                }
                vitesseMoy /= this.VitesseMoyPlot.Count;
                var newVitesse = new DataPoint(axeX, vitesseMoy);
                this.VitessePlotPoint.Add(newVitesse);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Messenger.Default.Send<bool>(true, "RefreshPlot");
                }), DispatcherPriority.Normal);
            }
            
            // Reset des liste de segment
            listPos = new List<PositionDataModel>();
            listPos2 = new List<Position2DataModel>();

            listForce = new List<ForceDataModel>();
            listForce2 = new List<Force2DataModel>();

            listVit = new List<VitesseModel>();
            listVit2 = new List<Vitesse2Model>();

            listError = new List<PprDataModel>();

            satMoy = new List<double>();

            VitesseMoyPlot = new List<double>();
            ErrorMoyPlot = new List<double>();

            // Repositionnement du robot sur la ligne de départ
            pss.Pss.SendExerciceFrame(frame);
        }

        void game_onCheckpointReached(object obj, MessageEvent messageArgs)
        {
            var frameSeg = new StreamNextSegModel((byte)messageArgs.Data).MakeFrame();
            this.pss.Pss.SendExerciceFrame(frameSeg);
            this.SaveSegment(messageArgs.Data);
        }

        void game_onLevelStarted(object obj, EventArgs messageArgs)
        {
            pss.Pss.SendCommandFrame(CommandCodes.StreamingMod);

            if (this.currentType != TypeMazeGame.Null)
            {
                this.canDoMath = true;
            }
        }

        void game_onNewTrajectory(object obj, PathEvent trajectoryArgs)
        {
            if (this.game != null)
            {
                var centre = this.game.GetCentre();

                if (centre != null)
                {
                    this.centreX = centre[0];
                    this.centreY = centre[1];
                }

                // Shift le circuit
                var newTraj = this.ShiftCircuit(trajectoryArgs.Path);
                // Streaming du nouveau circuit shifté
                var newTrajStream = this.PrepareToStreaming(newTraj);
                this.pss.Pss.StreamTrajectory(newTrajStream);

                if (Singleton.UniBi)
                {
                    // Unimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        frame = new MazeGameModel((ushort)(Singleton.CalibrX * 10), (ushort)(Singleton.CalibrY * 10), UniBiCodes.UnimanuelGauche).MakeFrame();
                    }
                    else
                    {
                        // droite
                        frame = new MazeGameModel((ushort)(Singleton.CalibrX * 10), (ushort)(Singleton.CalibrY * 10), UniBiCodes.UnimanuelDroite).MakeFrame();
                    }
                }
                else
                {
                    // Bimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        frame = new MazeGameModel((ushort)(Singleton.CalibrX * 10), (ushort)(Singleton.CalibrY * 10), UniBiCodes.BimanuelGaucheXDroiteY).MakeFrame();
                    }
                    else
                    {
                        // droite
                        frame = new MazeGameModel((ushort)(Singleton.CalibrX * 10), (ushort)(Singleton.CalibrY * 10), UniBiCodes.BimanuelGaucheYDroiteX).MakeFrame();
                    }
                }
                this.canSendPos = true;// Lancement du mod et positionement du robot
            }
        }

        private bool Start_CanExecute()
        {
            if (this.gameStarted || (this.IndexOfCircuitChecked() == -1 && (this.currentType == TypeMazeGame.BaseLine || this.currentType == TypeMazeGame.After || this.currentType == TypeMazeGame.NewCircuit || this.currentType == TypeMazeGame.Training)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void StopGame()
        {
            this.canDoMath = false;
            this.canSendPos = false;
            // Reset des liste de segment
            listPos = new List<PositionDataModel>();
            listPos2 = new List<Position2DataModel>();

            listForce = new List<ForceDataModel>();
            listForce2 = new List<Force2DataModel>();

            listVit = new List<VitesseModel>();
            listVit2 = new List<Vitesse2Model>();

            listError = new List<PprDataModel>();

            satMoy = new List<double>();

            this.ErrorPlotPoint.Clear();
            this.VitessePlotPoint.Clear();
            this.ErrorMoyPlot.Clear();
            this.VitesseMoyPlot.Clear();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Messenger.Default.Send<bool>(false, "RefreshPlot");
            }), DispatcherPriority.Normal);

            this.pss.Pss.SendCommandFrame(CommandCodes.STOPnv);
            if (this.game != null)
            {
                this.game.onNewTrajectory -= new AxReaLabToUnity.NewTrajectoryHandler(game_onNewTrajectory);
                this.game.onLevelStarted -= new AxReaLabToUnity.NewLevelStartedHandler(game_onLevelStarted);
                this.game.onCheckpointReached -= new AxReaLabToUnity.NewCheckpointReachedHandler(game_onCheckpointReached);
                this.game.onLevelStopped -= new AxReaLabToUnity.NewLevelStoppedHandler(game_onLevelStopped);
                this.pss.Pss.PositionDataReceived -= new AxCommunication.onPositionDataReceived(Pss_PositionDataReceived);
                this.pss.Pss.Position2DataReceived -= new AxCommunication.onPosition2DataReceived(Pss_Position2DataReceived);
                this.pss.Pss.PprDataReceived += new AxCommunication.onPprDataReceived(Pss_PprDataReceived);
                this.pss.Pss.VitesseDataReceived -= new AxCommunication.onVitesseDataReceived(Pss_VitesseDataReceived);
                this.pss.Pss.Vitesse2DataReceived -= new AxCommunication.onVitesse2DataReceived(Pss_Vitesse2DataReceived);
                this.pss.Pss.ForceDataReceived -= new AxCommunication.onForceDataReceived(Pss_ForceDataReceived);
                this.pss.Pss.Force2DataReceived -= new AxCommunication.onForce2DataReceived(Pss_Force2DataReceived);
                this.pss.Pss.StreamingDone -= new EventHandler(Pss_StreamingDone);
                this.game.StopGame();

                this.game = null;
            }

            // Gestion des états de l'interface
            this.gameStarted = false;
            RaisePropertyChanged("OptionsEnabled");
            if (this.currentType == TypeMazeGame.BaseLine || this.currentType == TypeMazeGame.After || this.currentType == TypeMazeGame.Training || this.currentType == TypeMazeGame.NewCircuit)
            {
                this.CircuitsEnabled = true;
            }
        }

        private bool Stop_CanExecute()
        {
            return this.gameStarted;
        }

        private void PauseGame()
        {
            if (this.game != null)
            {
                this.canSendPos = !this.canSendPos;
                this.canDoMath = !this.canDoMath;
                this.game.PauseGame();
            }
        }

        /// <summary>
        /// Retourne l'index du circuit qui est séléctionné
        /// </summary>
        /// <returns>-1 si aucun circuit n'est séléctionné</returns>
        private int IndexOfCircuitChecked()
        {
            int index = -1;
            foreach (var item in this.CircuitsCheck)
            {
                if (item.Checked)
                {
                    index = this.CircuitsCheck.IndexOf(item);
                }
            }

            if (currentType == TypeMazeGame.Familiarisation)
                return 0;

            return index;
        }

        /// <summary>
        /// Réactive tous les circuits
        /// </summary>
        private void ActiveAllCircuits()
        {
            foreach (var item in this.CircuitsCheck)
            {
                if (!item.Activated)
                {
                    item.Activated = true;
                }
            }
        }

        private PointData[] ShiftCircuit(PointData[] path)
        {
            PointData[] newPath = new PointData[path.Length];

            double centreCM = 0.0;
            double deltaX = 0.0;
            double centreYCM = 0.0;
            double deltaY = 0.0;
            if (Singleton.UniBi)
            {
                // Unimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    centreCM = EchelleUtils.MiseEchelleEnvoyerX(this.centreX);
                    deltaX = Singleton.CalibrX - EchelleUtils.MiseEchelleEnvoyerX(this.centreX);
                    centreYCM = EchelleUtils.MiseEchelleEnvoyerY(this.centreY);
                    deltaY = Singleton.CalibrY - EchelleUtils.MiseEchelleEnvoyerY(this.centreY);
                }
                else
                {
                    // droite
                    centreCM = EchelleUtils.MiseEchelleEnvoyerX2(this.centreX);
                    deltaX = Singleton.CalibrX - EchelleUtils.MiseEchelleEnvoyerX2(this.centreX);
                    centreYCM = EchelleUtils.MiseEchelleEnvoyerY2(this.centreY);
                    deltaY = Singleton.CalibrY - EchelleUtils.MiseEchelleEnvoyerY2(this.centreY);
                }
            }
            else
            {
                // Bimanuel
                if (Singleton.MainGaucheX)
                {
                    // gauche
                    centreCM = EchelleUtils.MiseEchelleEnvoyerX(this.centreX);
                    deltaX = Singleton.CalibrX - EchelleUtils.MiseEchelleEnvoyerX(this.centreX);
                    centreYCM = EchelleUtils.MiseEchelleEnvoyerY2(this.centreY);
                    deltaY = Singleton.CalibrY - EchelleUtils.MiseEchelleEnvoyerY2(this.centreY);
                }
                else
                {
                    // droite
                    centreCM = EchelleUtils.MiseEchelleEnvoyerX2(this.centreX);
                    deltaX = Singleton.CalibrX - EchelleUtils.MiseEchelleEnvoyerX2(this.centreX);
                    centreYCM = EchelleUtils.MiseEchelleEnvoyerY(this.centreY);
                    deltaY = Singleton.CalibrY - EchelleUtils.MiseEchelleEnvoyerY(this.centreY);
                }
            }

            if (deltaX < 0.09 && deltaX > 0)
            {
                deltaX = 0.0;
            }
            if (deltaY < 0.09 && deltaY > 0)
            {
                deltaY = 0.0;
            }

            for (int i = 0; i < path.Length; i++)
            {
                double coordX = 0.0;
                double coordY = 0.0;

                if (Singleton.UniBi)
                {
                    // Unimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        coordX = EchelleUtils.MiseEchelleEnvoyerX(path[i].Xd);
                        coordY = EchelleUtils.MiseEchelleEnvoyerY(path[i].Yd);
                    }
                    else
                    {
                        // droite
                        coordX = EchelleUtils.MiseEchelleEnvoyerX2(path[i].Xd);
                        coordY = EchelleUtils.MiseEchelleEnvoyerY2(path[i].Yd);
                    }
                }
                else
                {
                    // Bimanuel
                    if (Singleton.MainGaucheX)
                    {
                        // gauche
                        coordX = EchelleUtils.MiseEchelleEnvoyerX(path[i].Xd);
                        coordY = EchelleUtils.MiseEchelleEnvoyerY2(path[i].Yd);
                    }
                    else
                    {
                        // droite
                        coordX = EchelleUtils.MiseEchelleEnvoyerX2(path[i].Xd);
                        coordY = EchelleUtils.MiseEchelleEnvoyerY(path[i].Yd);
                    }
                }

                var newPoint = new PointData(coordX + deltaX, coordY + deltaY);
                newPath[i] = newPoint;
            }

            return newPath;
        }

        private List<List<System.Windows.Point>> PrepareToStreaming(PointData[] path)
        {
            List<List<System.Windows.Point>> newPath = new List<List<System.Windows.Point>>();

            for (int i = 0; i < path.Length; i++)
            {
                List<System.Windows.Point> segment = new List<System.Windows.Point>();
                System.Windows.Point p1 = new System.Windows.Point(path[i].Xd, path[i].Yd);
                System.Windows.Point p2;
                if (i < path.Length - 1)
                {
                    p2 = new System.Windows.Point(path[i + 1].Xd, path[i + 1].Yd);
                }
                else
                {
                    p2 = new System.Windows.Point(path[0].Xd, path[0].Yd);
                }
                segment.Add(p1);
                segment.Add(p2);
                newPath.Add(segment);
            }

            return newPath;
        }

        /// <summary>
        /// Enregistre les données d'un segment dans les fichiers
        /// </summary>
        private void SaveSegment(int checkpoint)
        {
            // L'angle en circuit est tjs a 45. Il varie seulement si le jeu est en reaching
            int angle = 45;
            if (this.currentType == TypeMazeGame.Reaching)
            {
                angle = this.game.GetAngle();
            }

            if (!Directory.Exists(this.pathToFile))
            {
                Directory.CreateDirectory(this.pathToFile);
            }

            using (StreamWriter file = new StreamWriter(this.pathToFile + "/" + this.newNameFile + ".txt", true))
            {
                if (this.firstSegment)
                {
                    // Sauvegarde des info du circuit
                    int uniBi = 0;
                    if (Singleton.UniBi == false)
                    {
                        uniBi = 1;
                    }

                    int xGauche = 0;
                    if (Singleton.MainGaucheX == false)
                    {
                        xGauche = 1;
                    }

                    var indiceCircuit = this.IndexOfCircuitChecked() + 1;

                    if (currentType == TypeMazeGame.Familiarisation)
                        indiceCircuit = 0;

                    file.WriteLine(uniBi + " " + xGauche + " " + indiceCircuit + " " + this.Viscosite + System.Environment.NewLine);

                    this.firstSegment = false;
                }
                // Prend la plus petite taille de liste pour ne pas avoir de dépassement d'indice lors des calcul
                var minLength = FindMinimumCount(listPos.Count, listPos2.Count, listForce.Count, listForce2.Count, listVit.Count, listVit2.Count, listError.Count);

                // Sauvegarde des données brutes a 80 hz
                double errorMoy = 0.0;
                double vitMoy = 0.0;
                List<double> vitJerk = new List<double>();
                for (int i = 0; i < minLength; i++)
                {
                    // Norme de l'erreur et erreur moyenne
                    double coordX = 0.0;
                    double coordY = 0.0;

                    if (Singleton.UniBi)
                    {
                        // Unimanuel
                        if (Singleton.MainGaucheX)
                        {
                            // gauche
                            coordX = listPos[i].PositionXd;
                            coordY = listPos[i].PositionYd;
                        }
                        else
                        {
                            // droite
                            coordX = listPos2[i].PositionXd;
                            coordY = listPos2[i].PositionYd;
                        }
                    }
                    else
                    {
                        // Bimanuel
                        if (Singleton.MainGaucheX)
                        {
                            // gauche
                            coordX = listPos[i].PositionXd;
                            coordY = listPos2[i].PositionYd;
                        }
                        else
                        {
                            // droite
                            coordX = listPos2[i].PositionXd;
                            coordY = listPos[i].PositionYd;
                        }
                    }
                    var erreur = AxAnalyse.Ax_Generique.Pythagorean(this.listError[i].PprX - coordX, this.listError[i].PprY - coordY) / 10000.0;
                    errorMoy += erreur;

                    // Norme de la vitesse
                    var normeVitGauche = AxAnalyse.Ax_Generique.Pythagorean(Math.Abs(this.listVit[i].VitesseX / 10000.0), Math.Abs(this.listVit[i].VitesseY / 10000.0));
                    var normeVitDroite = AxAnalyse.Ax_Generique.Pythagorean(Math.Abs(this.listVit2[i].VitesseX / 10000.0), Math.Abs(this.listVit2[i].VitesseY / 10000.0));
                    var vit = AxAnalyse.Ax_Generique.Pythagorean(normeVitGauche, normeVitDroite);
                    vitMoy += vit;

                    // Jerk
                    vitJerk.Add(vit);

                    // Bimanual coordination factor (0 en uni)
                    double numerateur = 0.0;
                    double denominateur = 1.0;
                    if (Singleton.MainGaucheX == true)
                    {
                        numerateur = Math.Min(Math.Abs((normeVitGauche / Math.Cos(angle))), Math.Abs((normeVitDroite / Math.Sin(angle))));
                        denominateur = AxAnalyse.Ax_Generique.Pythagorean((normeVitGauche / Math.Cos(angle)), (normeVitDroite / Math.Sin(angle)));
                    }
                    else
                    {
                        numerateur = Math.Min(Math.Abs((normeVitDroite / Math.Cos(angle))), Math.Abs((normeVitGauche / Math.Sin(angle))));
                        denominateur = AxAnalyse.Ax_Generique.Pythagorean((normeVitDroite / Math.Cos(angle)), (normeVitGauche / Math.Sin(angle)));
                    }
                    double bcf = 0.0;
                    if (denominateur != 0)
                    {
                        bcf = numerateur / denominateur;
                    }

                    file.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", Math.Round(this.listPos[i].PositionXd / 10000.0, 4), Math.Round(this.listPos[i].PositionYd / 10000.0, 4), this.listForce[i].ForceX, this.listForce[i].ForceY, Math.Round(this.listPos2[i].PositionXd / 10000.0, 4), Math.Round(this.listPos2[i].PositionYd / 10000.0, 4), this.listForce2[i].ForceX, this.listForce2[i].ForceY, Math.Round(erreur, 4), Math.Round(bcf, 2));
                }

                if (minLength > 0)
                {
                    errorMoy /= minLength;
                    this.ErrorMoyPlot.Add(errorMoy);
                    vitMoy /= minLength;
                    this.VitesseMoyPlot.Add(vitMoy);

                    // Acutalisation du plot
                    // En reaching on actualise a chaque segement
                    if (this.currentType == TypeMazeGame.Reaching)
                    {
                        double axeX = 1.0;
                        if (this.ErrorPlotPoint.Count > 0)
                        {
                            axeX = this.ErrorPlotPoint.Last().X + 1;
                        }
                        var newError = new DataPoint(axeX, errorMoy);
                        this.ErrorPlotPoint.Add(newError);

                        var newVitesse = new DataPoint(axeX, vitMoy);
                        this.VitessePlotPoint.Add(newVitesse);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Messenger.Default.Send<bool>(true, "RefreshPlot");
                        }), DispatcherPriority.Normal); 
                    }

                    // Sat
                    double sat = 1.0 * (vitMoy / errorMoy);
                    satMoy.Add(sat);

                    // NJerk calcul des accélération
                    List<double> acceJerk = new List<double>();
                    for (int i = 1; i < vitJerk.Count; i++)
                    {
                        double a = (vitJerk[i] - vitJerk[i - 1]) / 0.0125;
                        acceJerk.Add(a);
                    }
                    // NJerk calcul des jerk
                    double sommeJerk = 0.0;
                    for (int i = 1; i < acceJerk.Count; i++)
                    {
                        double j = (acceJerk[i] - acceJerk[i - 1]) / 0.0125;
                        sommeJerk += Math.Pow(j, 2) * 0.0125;
                    }
                    // NJerk
                    sommeJerk /= 2;
                    double tempsSeg = this.game.GetTimeSeg(); // sec
                    double longeurSeg = this.SegmentSize(checkpoint); // metre
                    double nJerk = Math.Sqrt(sommeJerk * (Math.Pow(tempsSeg, 5) / Math.Pow(longeurSeg, 2)));

                    file.WriteLine("{0} {1} {2} {3}" + System.Environment.NewLine, Math.Round(nJerk, 4), Math.Round(sat, 4), Math.Round(errorMoy, 4) ,Math.Round(vitMoy, 4));
                }
            }

            // Reset des liste de segment
            listPos.Clear(); ;
            listPos2.Clear();

            listForce.Clear();
            listForce2.Clear();

            listVit.Clear();
            listVit2.Clear();

            listError.Clear();
        }

        /// <summary>
        /// Trouve le nouveau du fichier si celui demandé existe déjà
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseName"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string FindFileName(string path, string baseName, string ext)
        {
            int indiceFichier = 1;
            while (File.Exists(path + "/" + baseName + ext))
            {
                if (indiceFichier == 1)
                    baseName += "_" + indiceFichier.ToString();
                else
                {
                    var newNom = baseName.Replace((indiceFichier - 1).ToString(), indiceFichier.ToString());
                    baseName = newNom;
                }
                indiceFichier++;
            }

            return baseName;
        }

        /// <summary>
        /// Trouve le plus int entre 7 nombre
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        private int FindMinimumCount(int a, int b, int c, int d, int e, int f,int g)
        {
            var min1 = Math.Min(a, b);
            var min2 = Math.Min(c, d);
            var min3 = Math.Min(e, f);

            var min4 = Math.Min(min1, min2);
            var min5 = Math.Min(min3, g);

            var min6 = Math.Min(min4, min5);
            return min6;
        }

        private double SegmentSize(int checkpoint)
        {
            var size = 0.0;

            if (this.currentType == TypeMazeGame.Reaching)
            {
                size = SIZE_SEG_REACHING;
            }
            else if (this.currentType == TypeMazeGame.Familiarisation)
            {
                size = SIZE_SEG_FAM;
            }
            else
            {
                size = SIZE_SEG_CIRCUIT[checkpoint];
            }

            return size;
        }

        private void SendViscosite()
        {
            var reguConfig = new RegulateurConfig();

            // Adaptation de la config en fonction du niveau de visc.
            switch (this.Viscosite)
            {
                case 0: reguConfig.CGlob = 15;
                    break;
                case 1: reguConfig.CGlob = 30;
                    break;
                case 2: reguConfig.CGlob = 45;
                    break;
                case 3: reguConfig.CGlob = 60;
                    break;
                default: reguConfig.CGlob = 15;
                    break;
            }

            FrameExerciceDataModel newConfig = new FrameExerciceDataModel();
            newConfig.Address = ConfigAddresses.Regulateur;
            newConfig.Data1 = reguConfig.Kp;
            newConfig.Data2 = reguConfig.Ki;
            newConfig.Data3 = reguConfig.CGlob;
            newConfig.Data4 = 0;

            this.pss.Pss.SendExerciceFrame(newConfig);
        }
        #endregion
    }

    enum TypeMazeGame
    {
        Null = 0,
        Familiarisation = 1,
        BaseLine = 2,
        Training = 3,
        Reaching = 4,
        NewCircuit = 5,
        After = 6
    }

    enum TypeColors
    {
        Null = 0,
        Default = 1,
        WhiteRed = 2,
        BlueYellow = 3
    }
}
