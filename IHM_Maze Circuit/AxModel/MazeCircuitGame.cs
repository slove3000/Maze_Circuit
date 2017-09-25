using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxReaLabToUnity;
using AxReaLabToUnity.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace AxModel
{
    public class MazeCircuitGame
    {
        #region Field
        private const string PATHTOGAME = "Maze Circuit\\maze circuit.exe";

        private Singleton singleton = Singleton.getInstance();

        /// <summary>
        /// Difficulté de l'exercice.
        /// 0 = base line et new circuit (3 fois le circuit)
        /// 1 = training (20 fois le circuit)
        /// </summary>
        private int difficulty;
        private int zoom;
        /// <summary>
        /// Level à charger
        /// 0 = initiation et calibration
        /// 1 -> 8 = les circuit
        /// 9 = reaching
        /// </summary>
        private int level;

        /// <summary>
        /// Instance du serveur de communication
        /// </summary>
        private ReaLabServer server;

        /// <summary>
        /// Istance de l'exe du jeu
        /// </summary>
        private Process gameProcess; 
        #endregion

        public MazeCircuitGame(int diff, int zoom, int lvl)
        {
            this.difficulty = diff;
            this.zoom = zoom;
            this.level = lvl;
        }

        #region Properties
        public event NewTrajectoryHandler onNewTrajectory;
        public event NewLevelStartedHandler onLevelStarted;
        public event NewCheckpointReachedHandler onCheckpointReached;
        public event NewLevelStoppedHandler onLevelStopped;
        #endregion

        #region Methodes
        /// <summary>
        /// Démarre le serveur de communication et lance le jeu unity
        /// </summary>
        public void StartGame()
        {
            if (this.server != null)
            {
                this.server.Stop();
            }

            // Démarrage du serveur et envoie de la config
            this.server = new ReaLabServer("127.0.0.1", 8888, 8889);
            this.server.Start();
            var config = new ReaLabConfiguration(singleton.Patient.Nom, singleton.Patient.Prenom, singleton.PatientSingleton.DateNaiss, this.difficulty, this.zoom, this.level, false);
            this.server.SetReaLabConfiguration(config);

            this.server.onNewTrajectory += new NewTrajectoryHandler(server_onNewTrajectory);
            this.server.onLevelStarted += new NewLevelStartedHandler(server_onLevelStarted);
            this.server.onCheckpointReached += new NewCheckpointReachedHandler(server_onCheckpointReached);
            this.server.onLevelStopped += new NewLevelStoppedHandler(server_onLevelStopped);

            // Lancement du jeu
            if (this.gameProcess != null)
            {
                this.gameProcess.Kill();
            }
            // Trouve le handler de la fenetre mainP
            var handler = this.GetHandle("ReaPlan patient");
            // Creation du process du jeu
            this.gameProcess = new Process();
            this.gameProcess.StartInfo.FileName = PATHTOGAME;
            this.gameProcess.StartInfo.Arguments = "-parentHWND " + handler.ToInt32() + " " + Environment.CommandLine;
            this.gameProcess.StartInfo.UseShellExecute = true;
            this.gameProcess.StartInfo.CreateNoWindow = true;
            this.gameProcess.Start();
            this.gameProcess.WaitForInputIdle();

            EnumChildWindows(handler, this.WindowEnum, IntPtr.Zero);
        }

        void server_onLevelStopped(object obj, EventArgs messageArgs)
        {
            if (this.onLevelStopped != null)
            {
                this.onLevelStopped(obj, messageArgs);
            }
        }

        void server_onCheckpointReached(object obj, MessageEvent messageArgs)
        {
            if (this.onCheckpointReached != null)
            {
                this.onCheckpointReached(obj, messageArgs);
            }
        }

        void server_onLevelStarted(object obj, EventArgs messageArgs)
        {
            if (this.onLevelStarted != null)
            {
                this.onLevelStarted(obj, messageArgs);
            }
        }

        void server_onNewTrajectory(object obj, PathEvent trajectoryArgs)
        {
            if (this.onNewTrajectory != null)
            {
                this.onNewTrajectory(obj, trajectoryArgs);
            }
        }

        private IntPtr GetHandle(string window)
        {
            var windows = Application.Current.Windows;
            var handle = IntPtr.Zero;

            foreach (Window win in windows)
            {
                if (win.Title == window)
                {
                    var interop = new WindowInteropHelper(win);
                    handle = interop.Handle;
                }
            }
            return handle;
        }

        /// <summary>
        /// Arret le serveur de communication et coupe le jeu unity
        /// </summary>
        public void StopGame()
        {
            if (this.server != null)
            {
                this.server.onNewTrajectory -= new NewTrajectoryHandler(server_onNewTrajectory);
                this.server.onLevelStarted -= new NewLevelStartedHandler(server_onLevelStarted);
                this.server.onCheckpointReached -= new NewCheckpointReachedHandler(server_onCheckpointReached);
                this.server.onLevelStopped -= new NewLevelStoppedHandler(server_onLevelStopped);
                this.server.StopGame();

                if (this.gameProcess != null)
                {
                    //this.gameProcess.Kill();
                    this.gameProcess = null;
                }

                Thread.Sleep(10); 
                this.server.Stop();
                this.server = null;
            }
        }

        public void PauseGame()
        {
            if (this.server != null)
            {
                this.server.PauseGame();
            }
        }

        // Transmet les position au jeu
        public void SetPositions(double x, double y)
        {
            try
            {
                if (this.server != null)
                {
                    this.server.SetPositions(x, y);
                }
            }
            catch (Exception)
            {

            }
        }

        public double[] GetCentre()
        {
            if (this.server != null)
            {
                double[] centre = new double[2];

                var x = this.server.GetValue("centreX");
                var y = this.server.GetValue("centreY");

                if (x != null && y != null)
                {
                    centre[0] = (double)x;
                    centre[1] = (double)y;

                    return centre;
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public void SetScore(double value)
        {
            this.server.SetValue("Score", value);
        }

        public void SetHighScore(double value)
        {
            this.server.SetValue("HighScore", value);
        }

        public int GetAngle()
        {
            int angle = 0;

            if (this.server != null)
            {
                angle = int.Parse(this.server.GetValue("Angle").ToString()); 
            }

            return angle;
        }

        public double GetTimeSeg()
        {
            double temps = 0;

            if (this.server != null)
            {
                temps = double.Parse(this.server.GetValue("TempsSeg").ToString());
            }

            return temps;
        }
        #endregion

        #region Import
        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, uint lParam);

        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            SendMessage(hwnd, 0x0006, 1, 0);
            return 0;
        }
        #endregion
    }
}
